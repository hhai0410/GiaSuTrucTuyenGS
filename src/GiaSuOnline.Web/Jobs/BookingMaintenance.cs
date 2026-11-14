using GiaSuOnline.Web.Data;
using GiaSuOnline.Web.Integrations.Email;
using GiaSuOnline.Web.Services;
using Microsoft.EntityFrameworkCore;
namespace GiaSuOnline.Web.Jobs;
public class BookingEmailDispatcher(ApplicationDbContext db, IEmailSender sender, TimeProvider clock)
{
    public async Task SendPendingAsync()
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var ids = await db.BookingEmails.AsNoTracking().Where(x => x.SentAtUtc == null && x.NextAttemptAtUtc <= now && (x.LeaseUntilUtc == null || x.LeaseUntilUtc <= now))
            .OrderBy(x => x.Id).Select(x => x.Id).Take(20).ToListAsync();
        foreach (var id in ids)
        {
            var token = Guid.NewGuid().ToString("N");
            var claim = db.BookingEmails.Where(x => x.Id == id && x.SentAtUtc == null && x.NextAttemptAtUtc <= now && (x.LeaseUntilUtc == null || x.LeaseUntilUtc <= now));
            if (await claim.ExecuteUpdateAsync(s => s.SetProperty(x => x.LeaseToken, token).SetProperty(x => x.LeaseUntilUtc, now.AddMinutes(5)).SetProperty(x => x.Attempts, x => x.Attempts + 1)) != 1) continue;
            var mail = await db.BookingEmails.AsNoTracking().SingleAsync(x => x.Id == id);
            var owned = db.BookingEmails.Where(x => x.Id == id && x.LeaseToken == token);
            try
            {
                await sender.SendAsync(mail.Recipient, mail.Subject, mail.Body);
                await owned.ExecuteUpdateAsync(s => s.SetProperty(x => x.SentAtUtc, clock.GetUtcNow().UtcDateTime).SetProperty(x => x.LeaseUntilUtc, (DateTime?)null).SetProperty(x => x.LeaseToken, (string?)null).SetProperty(x => x.LastError, (string?)null));
            }
            catch (Exception ex)
            {
                // Không lưu thông tin đăng nhập hoặc toàn bộ exception vào CSDL/log.
                await owned.ExecuteUpdateAsync(s => s.SetProperty(x => x.NextAttemptAtUtc, now.AddMinutes(Math.Min(60, Math.Max(1, mail.Attempts * 2))))
                    .SetProperty(x => x.LeaseUntilUtc, (DateTime?)null).SetProperty(x => x.LeaseToken, (string?)null).SetProperty(x => x.LastError, ex.GetType().Name));
            }
        }
    }
}
public class BookingMaintenance(IServiceScopeFactory scopes, ILogger<BookingMaintenance> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<SchedulingService>().ExpireAsync();
                await scope.ServiceProvider.GetRequiredService<BookingEmailDispatcher>().SendPendingAsync();
            }
            catch (Exception ex) { logger.LogWarning("Tác vụ đặt lịch chưa hoàn tất ({Type}); sẽ thử lại.", ex.GetType().Name); }
            try { if (!await timer.WaitForNextTickAsync(stoppingToken)) break; }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }
}
