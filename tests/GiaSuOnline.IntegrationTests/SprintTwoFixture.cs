using System.Collections.Concurrent;
using GiaSuOnline.Web.Data;
using GiaSuOnline.Web.Data.Seed;
using GiaSuOnline.Web.Integrations.Email;
using GiaSuOnline.Web.Models;
using GiaSuOnline.Web.Services;
using GiaSuOnline.Web.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace GiaSuOnline.IntegrationTests;
public class TestClock : TimeProvider
{
    public DateTimeOffset Now { get; set; } = new(2030, 1, 7, 0, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => Now;
}
public class RecordingEmail : IEmailSender
{
    public ConcurrentQueue<string> Subjects { get; } = new();
    public int Failures;
    public Task SendAsync(string to, string subject, string body)
    {
        if (Interlocked.Decrement(ref Failures) >= 0) throw new IOException("Simulated SMTP outage");
        Subjects.Enqueue(subject); return Task.CompletedTask;
    }
}
public class SprintTwoFixture(bool sqlServer = false) : WebApplicationFactory<Program>
{
    private readonly string databaseName = "GiaSuGS_Test_" + Guid.NewGuid().ToString("N");
    private readonly string file = Path.Combine(Path.GetTempPath(), "gs-sprint2-" + Guid.NewGuid().ToString("N") + ".db");
    public TestClock Clock { get; } = new();
    public RecordingEmail Mail { get; } = new();
    public int TutorId { get; private set; }
    public int OtherTutorId { get; private set; }
    public int PendingTutorId { get; private set; }
    public string Owner { get; private set; } = "";
    public string OtherOwner { get; private set; } = "";
    public string Student { get; private set; } = "";
    public string Student2 { get; private set; } = "";
    public int SubjectId { get; private set; }
    protected override void ConfigureWebHost(IWebHostBuilder b)
    {
        b.UseEnvironment("Testing");
        b.ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(new Dictionary<string,string?> { ["Logging:LogLevel:Default"]="Error" }));
        b.ConfigureTestServices(s =>
        {
            s.RemoveAll<ApplicationDbContext>(); s.RemoveAll<DbContextOptions<ApplicationDbContext>>(); s.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            s.AddDbContext<ApplicationDbContext>(o =>
            {
                if (sqlServer) o.UseSqlServer(new Microsoft.Data.SqlClient.SqlConnectionStringBuilder
                { DataSource=Environment.GetEnvironmentVariable("GS_SQL_TEST_SERVER") ?? "LOQ\\SQLEXPRESS", InitialCatalog=databaseName, IntegratedSecurity=true, TrustServerCertificate=true, ConnectTimeout=15 }.ConnectionString);
                else o.UseSqlite($"Data Source={file};Default Timeout=30;Pooling=False");
            });
            s.RemoveAll<TimeProvider>(); s.AddSingleton<TimeProvider>(Clock);
            s.RemoveAll<IEmailSender>(); s.AddSingleton<IEmailSender>(Mail);
        });
    }
    public async Task InitAsync()
    {
        await Run(async sp =>
        {
            var db=sp.GetRequiredService<ApplicationDbContext>(); await db.Database.EnsureCreatedAsync();
            await DatabaseSeed.RunAsync(sp,new ConfigurationBuilder().Build());
            SubjectId=await db.Subjects.OrderBy(x=>x.Id).Select(x=>x.Id).FirstAsync();
            var users=sp.GetRequiredService<UserManager<ApplicationUser>>();
            async Task<ApplicationUser> User(string name, string role)
            {
                var u=new ApplicationUser{Email=name+"@example.test",UserName=name+"@example.test",EmailConfirmed=true,FullName=name};
                Assert.True((await users.CreateAsync(u,"Password123!")).Succeeded); Assert.True((await users.AddToRoleAsync(u,role)).Succeeded); return u;
            }
            var tutor=await User("tutor","Tutor"); Owner=tutor.Id;
            var other=await User("other","Tutor"); OtherOwner=other.Id;
            var pending=await User("pending","Tutor");
            Student=(await User("student","Student")).Id; Student2=(await User("student2","Student")).Id;
            TutorProfile Profile(ApplicationUser u, ProfileStatus status) => new() { User=u,Status=status,Introduction="Experienced tutor teaching mathematics",HourlyRate=120000,TeachingLanguage="vi",Subjects=[new(){SubjectId=SubjectId}] };
            var p=Profile(tutor,ProfileStatus.Approved);var o=Profile(other,ProfileStatus.Approved);var pendingP=Profile(pending,ProfileStatus.Pending);
            db.AddRange(p,o,pendingP);await db.SaveChangesAsync();TutorId=p.Id;OtherTutorId=o.Id;PendingTutorId=pendingP.Id;
        });
    }
    public async Task<T> Run<T>(Func<IServiceProvider,Task<T>> action) { using var scope=Services.CreateScope();return await action(scope.ServiceProvider); }
    public async Task Run(Func<IServiceProvider,Task> action) { using var scope=Services.CreateScope();await action(scope.ServiceProvider); }
    public HttpClient Client() => CreateClient(new WebApplicationFactoryClientOptions { BaseAddress=new("https://localhost"),AllowAutoRedirect=false });
    public Task<int> Slot(int day=2,int hour=10,int minutes=60) => Run(sp=>sp.GetRequiredService<SchedulingService>().SaveSlotAsync(Owner,new SlotForm
    { StartsAt=VietnamTime.Local(Clock.Now.UtcDateTime.Date.AddDays(day).AddHours(hour)),EndsAt=VietnamTime.Local(Clock.Now.UtcDateTime.Date.AddDays(day).AddHours(hour).AddMinutes(minutes)) }));
    public Task<BookForm> Form(int slotId) => Run(async sp=>
    {
        var slot=await sp.GetRequiredService<ApplicationDbContext>().AvailabilitySlots.AsNoTracking().Include(x=>x.TutorProfile).SingleAsync(x=>x.Id==slotId);
        return new BookForm{SlotId=slot.Id,SlotRevision=slot.Revision,ProfileRevision=slot.TutorProfile.Revision};
    });
    public override async ValueTask DisposeAsync()
    {
        // Chỉ database ngẫu nhiên do fixture tạo, tuyệt đối không dùng database của ứng dụng.
        if (databaseName.StartsWith("GiaSuGS_Test_",StringComparison.Ordinal))
            await Run(async sp=>await sp.GetRequiredService<ApplicationDbContext>().Database.EnsureDeletedAsync());
        await base.DisposeAsync();
    }
}
public class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute() { if(Environment.GetEnvironmentVariable("GS_SQL_TEST")!="1") Skip="Chạy GS_SQL_TEST=1 để kiểm thử đồng thời trên SQL Server riêng."; }
}
