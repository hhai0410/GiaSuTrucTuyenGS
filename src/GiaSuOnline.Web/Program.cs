using System.Globalization;
using GiaSuOnline.Web.Data;
using GiaSuOnline.Web.Data.Seed;
using GiaSuOnline.Web.Models;
using GiaSuOnline.Web.Services;
using GiaSuOnline.Web.Integrations.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiaSuOnline.Web.Jobs;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);
Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "App_Data"));
builder.Services.AddDbContext<ApplicationDbContext>(o =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection");
    if (builder.Configuration["DatabaseProvider"] == "Sqlite") o.UseSqlite(cs);
    else o.UseSqlServer(cs);
});
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(o =>
{
    o.User.RequireUniqueEmail = true;
    o.Password.RequiredLength = 8; o.Password.RequireDigit = false; o.Password.RequireUppercase = false;
    o.Password.RequireLowercase = false; o.Password.RequireNonAlphanumeric = false;
    o.Lockout.MaxFailedAccessAttempts = 5; o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    o.SignIn.RequireConfirmedEmail = true;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromMinutes(30));
builder.Services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.Zero);
builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Home/Status/403";
    o.Cookie.HttpOnly = true; o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.Events.OnRedirectToAccessDenied = c => { c.Response.StatusCode = 403; return Task.CompletedTask; };
});
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<UploadStore>(); builder.Services.AddScoped<ProfileService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<TutorSearchService>();
builder.Services.AddScoped<SchedulingService>();
builder.Services.AddScoped<BookingEmailDispatcher>();
if (!builder.Environment.IsEnvironment("Testing") && builder.Configuration.GetValue("Jobs:Enabled", true))
    builder.Services.AddHostedService<BookingMaintenance>();
builder.Services.AddHsts(o => { o.MaxAge = TimeSpan.FromDays(365); o.IncludeSubDomains = true; });
builder.Services.AddHttpsRedirection(o => { o.HttpsPort = 7043; o.RedirectStatusCode = 308; });
var app = builder.Build();
if (builder.Configuration.GetValue<bool>("SearchBenchmark"))
{
    if (!app.Environment.IsDevelopment()) throw new InvalidOperationException("Benchmark chỉ chạy trong Development.");
    var connection = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
    if (builder.Configuration["DatabaseProvider"] != "SqlServer" || !connection.InitialCatalog.EndsWith("_Demo", StringComparison.Ordinal))
        throw new InvalidOperationException("Benchmark chỉ được seed trong database SQL Server có tên kết thúc _Demo.");
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await DatabaseSeed.RunAsync(scope.ServiceProvider, new ConfigurationBuilder().Build());
    await SearchDemoSeed.RunAsync(db);
    var search = scope.ServiceProvider.GetRequiredService<TutorSearchService>();
    var subjectId = await db.Subjects.OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();
    var timings = new List<double>();
    for (var i = 0; i < 6; i++)
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        var result = await search.SearchAsync(new() { SubjectId = subjectId, MinPrice = 100000, MaxPrice = 500000, Language = "vi", MinRating = 3, Sort = "price" });
        timings.Add(timer.Elapsed.TotalMilliseconds);
        if (result.Items.Count > 10 || result.Total == 0) throw new InvalidOperationException("Benchmark không có kết quả hợp lệ.");
    }
    Console.WriteLine("SEARCH_BENCHMARK " + System.Text.Json.JsonSerializer.Serialize(new { Profiles = await db.TutorProfiles.CountAsync(), ColdMs = timings[0], WarmMaxMs = timings.Skip(1).Max(), WarmMeanMs = timings.Skip(1).Average(), TargetMs = 2000 }));
    return;
}
// Kiểm tra SMTP theo yêu cầu rõ ràng từ dòng lệnh, không mở endpoint gửi thư công khai.
if (builder.Configuration.GetValue<bool>("SmtpCheck"))
{
    if (builder.Configuration["Email:Mode"] != "Smtp")
        throw new InvalidOperationException("Kiểm tra này cần Email:Mode=Smtp, không dùng thư thử Pickup.");
    var recipient = builder.Configuration["SmtpCheckRecipient"];
    if (string.IsNullOrWhiteSpace(recipient)) throw new InvalidOperationException("Chưa có địa chỉ nhận thư thử.");
    try
    {
        using var scope = app.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IEmailSender>().SendAsync(recipient,
            "GS – Kiểm tra cấu hình SMTP Sprint 1",
            "Đây là thư kiểm tra được gửi theo yêu cầu của nhóm dự án GS.\nNếu đọc được thư này, hãy xác nhận với nhóm rằng hộp thư đã nhận được.\nThời điểm gửi (UTC): " + DateTimeOffset.UtcNow.ToString("O"));
        Console.WriteLine("Máy chủ SMTP đã chấp nhận thư thử. Hãy kiểm tra Hộp thư đến/Spam để xác nhận đã nhận.");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Gửi thư thử thất bại ({ex.GetType().Name}). Kiểm tra tài khoản, mật khẩu ứng dụng, SMTP và kết nối mạng.");
        var detail = ex.Message;
        if (builder.Configuration["Email:Password"] is { Length: > 0 } secret)
            detail = detail.Replace(secret, "[đã ẩn]");
        Console.Error.WriteLine(detail);
        Environment.ExitCode = 1;
    }
    return;
}
app.UseExceptionHandler("/Home/Error");
if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseHttpsRedirection();
app.Use(async (ctx, next) =>
{
    var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
    ctx.Items["CspNonce"] = nonce;
    ctx.Response.Headers.XContentTypeOptions = "nosniff";
    ctx.Response.Headers["Referrer-Policy"] = "same-origin";
    ctx.Response.Headers["Content-Security-Policy"] = $"default-src 'self'; img-src 'self' blob: data:; font-src 'self' data:; style-src 'self' 'nonce-{nonce}'; script-src 'self' 'nonce-{nonce}'; form-action 'self'; frame-ancestors 'none'; base-uri 'self'";
    ctx.Response.Headers.CacheControl = "no-store";
    await next();
});
app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");
app.UseStaticFiles(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization();
app.MapControllerRoute("areas", "{area:exists}/{controller=Profiles}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
// Chỉ tạo schema SQLite cho môi trường học/demo. SQL Server dùng migration có kiểm soát.
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (app.Environment.IsDevelopment() && builder.Configuration["DatabaseProvider"] == "Sqlite") await db.Database.EnsureCreatedAsync();
    await DatabaseSeed.RunAsync(scope.ServiceProvider, app.Configuration);
}
app.Run();
public partial class Program { }
