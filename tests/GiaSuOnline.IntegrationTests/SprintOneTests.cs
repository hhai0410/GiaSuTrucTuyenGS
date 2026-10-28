using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using GiaSuOnline.Web.Data;
using GiaSuOnline.Web.Data.Seed;
using GiaSuOnline.Web.Integrations.Email;
using GiaSuOnline.Web.Models;
using GiaSuOnline.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace GiaSuOnline.IntegrationTests;
public class TestMail : IEmailSender
{
    public List<(string To,string Body)> Messages { get; } = [];
    public Task SendAsync(string to,string subject,string body) { Messages.Add((to,body)); return Task.CompletedTask; }
}
public class AppFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    public TestMail Mail { get; } = new();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        connection.Open();
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(new Dictionary<string,string?>
        { ["Logging:LogLevel:Default"]="Warning", ["Logging:LogLevel:Microsoft.AspNetCore.Antiforgery"]="Error" }));
        builder.ConfigureTestServices(s =>
        {
            s.RemoveAll<ApplicationDbContext>(); s.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            s.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            s.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(connection));
            s.RemoveAll<IEmailSender>(); s.AddSingleton<IEmailSender>(Mail);
        });
    }
    public async Task InitializeAsync()
    {
        using var scope=Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreatedAsync();
        await DatabaseSeed.RunAsync(scope.ServiceProvider,new ConfigurationBuilder().Build());
    }
    public HttpClient Client() => CreateClient(new WebApplicationFactoryClientOptions { BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false });
    public async Task UserAsync(string email,string role)
    {
        using var scope=Services.CreateScope();var u=scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user=new ApplicationUser{UserName=email,Email=email,EmailConfirmed=true,FullName="Gia sư thử nghiệm"};
        Assert.True((await u.CreateAsync(user,"Password123!")).Succeeded);Assert.True((await u.AddToRoleAsync(user,role)).Succeeded);
    }
}
public class SprintOneTests
{
    private static async Task<string> Token(HttpClient c,string path)
    {
        var html=await c.GetStringAsync(path);
        return WebUtility.HtmlDecode(Regex.Match(html,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
    }
    private static async Task<HttpResponseMessage> Post(HttpClient c,string path,Dictionary<string,string> values,string? get=null)
    { values["__RequestVerificationToken"]=await Token(c,get??path);return await c.PostAsync(path,new FormUrlEncodedContent(values)); }
    private static Task<HttpResponseMessage> Login(HttpClient c,string email,string password="Password123!") => Post(c,"/Account/Login",new(){{"Email",email},{"Password",password}});
    private static async Task<(AppFactory App,HttpClient Client)> Setup()
    {var a=new AppFactory();await a.InitializeAsync();return(a,a.Client());}
    [Fact] public async Task RegistrationHashesPasswordSeedsRolesAndSendsConfirmation()
    {
        var(a,c)=await Setup();using(a)using(c){
        var r=await Post(c,"/Account/Register",new(){{"FullName","Học viên GS"},{"Email","new@example.test"},{"Password","abcdefgh"},{"ConfirmPassword","abcdefgh"},{"Role","Student"}});
        Assert.Equal(HttpStatusCode.Redirect,r.StatusCode);Assert.Single(a.Mail.Messages);
        using var scope=a.Services.CreateScope();var users=scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();var user=await users.FindByEmailAsync("new@example.test");
        Assert.NotNull(user);Assert.NotEqual("abcdefgh",user.PasswordHash);Assert.False(user.EmailConfirmed);Assert.True(await users.IsInRoleAsync(user,"Student"));
        var link=a.Mail.Messages[0].Body.Split('\n').Last();Assert.Equal(HttpStatusCode.Redirect,(await c.GetAsync(link)).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect,(await Login(c,"new@example.test","abcdefgh")).StatusCode);
        var duplicate=await Post(c,"/Account/Register",new(){{"FullName","Test"},{"Email","NEW@example.test"},{"Password","abcdefgh"},{"ConfirmPassword","abcdefgh"},{"Role","Tutor"}});
        Assert.Contains("Email đã được sử dụng",WebUtility.HtmlDecode(await duplicate.Content.ReadAsStringAsync()));}
    }
    [Theory]
    [InlineData("Admin","abcdefgh","valid@example.test")]
    [InlineData("Tutor","short","valid@example.test")]
    [InlineData("Student","abcdefgh","invalid-email")]
    public async Task RegistrationRejectsInvalidInputs(string role,string password,string email)
    {
        var(a,c)=await Setup();using(a)using(c){await Post(c,"/Account/Register",new(){{"FullName","Test"},{"Email",email},{"Password",password},{"ConfirmPassword",password},{"Role",role}});
        using var s=a.Services.CreateScope();Assert.Equal(0,await s.ServiceProvider.GetRequiredService<ApplicationDbContext>().Users.CountAsync());}
    }
    [Fact] public async Task LockoutAfterFiveFailuresForFifteenMinutes()
    {
        var(a,c)=await Setup();using(a)using(c){await a.UserAsync("lock@example.test","Tutor");
        for(int i=0;i<5;i++)await Login(c,"lock@example.test","wrongpass");
        Assert.Equal(HttpStatusCode.OK,(await Login(c,"lock@example.test")).StatusCode);
        using var s=a.Services.CreateScope();var u=await s.ServiceProvider.GetRequiredService<ApplicationDbContext>().Users.SingleAsync();
        Assert.True(u.LockoutEnd>DateTimeOffset.UtcNow.AddMinutes(14));}
    }
    [Fact] public async Task ResetTokenSingleUseAndInvalidatesOldPassword()
    {
        var(a,c)=await Setup();using(a)using(c){await a.UserAsync("reset@example.test","Student");
        await Post(c,"/Account/ForgotPassword",new(){{"Email","reset@example.test"}});
        var link=new Uri(a.Mail.Messages.Single().Body.Split('\n').Last());var query=QueryHelpers.ParseQuery(link.Query);
        var data=new Dictionary<string,string>{{"Email","reset@example.test"},{"Token",query["token"].ToString()},{"Password","newpassword"},{"ConfirmPassword","newpassword"}};
        Assert.Equal(HttpStatusCode.Redirect,(await Post(c,"/Account/ResetPassword",data,link.PathAndQuery)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await Post(c,"/Account/ResetPassword",data,link.PathAndQuery)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await Login(c,"reset@example.test")).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect,(await Login(c,"reset@example.test","newpassword")).StatusCode);}
    }
    [Fact] public async Task ExpiredResetTokenRejected()
    {
        var(a,c)=await Setup();using(a)using(c){await a.UserAsync("expiry@example.test","Student");
        using var scope=a.Services.CreateScope();
        var options=scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<DataProtectionTokenProviderOptions>>();
        Assert.Equal(TimeSpan.FromMinutes(30),options.Value.TokenLifespan);
        var users=scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();var user=(await users.FindByEmailAsync("expiry@example.test"))!;
        options.Value.TokenLifespan=TimeSpan.FromSeconds(-1);
        var token=await users.GeneratePasswordResetTokenAsync(user);
        Assert.False((await users.ResetPasswordAsync(user,token,"newpassword")).Succeeded);}
    }
    [Fact] public async Task SecurityBlocksCsrfRoleEscalationAndRedirectsHttp()
    {
        var(a,c)=await Setup();using(a)using(c){await a.UserAsync("student@example.test","Student");await Login(c,"student@example.test");
        Assert.Equal(HttpStatusCode.Forbidden,(await c.GetAsync("/Admin/Profiles")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await c.GetAsync("/TutorProfiles/Edit")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await c.PostAsync("/Account/Logout",new FormUrlEncodedContent([]))).StatusCode);
        Assert.Equal(HttpStatusCode.PermanentRedirect,(await c.GetAsync("http://localhost/")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,(await c.GetAsync("/TutorProfiles/Details/999999")).StatusCode);
        var response=await c.GetAsync("/");Assert.Contains("nosniff",response.Headers.GetValues("X-Content-Type-Options"));}
    }
    private static async Task<HttpResponseMessage> SaveProfile(HttpClient c,string? revision=null,decimal price=150000,string introduction="Tôi có kinh nghiệm giảng dạy và đồng hành cùng học viên.",string filename="photo.png",byte[]? bytes=null)
    {
        var token=await Token(c,"/TutorProfiles/Edit");
        using var content=new MultipartFormDataContent();
        foreach(var kv in new Dictionary<string,string>{{"__RequestVerificationToken",token},{"Introduction",introduction},{"HourlyRate",price.ToString(System.Globalization.CultureInfo.InvariantCulture)},{"ExperienceYears","3"},{"SubjectIds","1"},{"Revision",revision??""}}) content.Add(new StringContent(kv.Value),kv.Key);
        var png=bytes??Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+a8S8AAAAASUVORK5CYII=");
        content.Add(new ByteArrayContent(png),"Photo",filename);content.Add(new ByteArrayContent(Encoding.ASCII.GetBytes("%PDF-1.4\n%%EOF")),"Certificate","certificate.pdf");
        return await c.PostAsync("/TutorProfiles/Edit",content);
    }
    [Fact] public async Task ProfileApprovalRejectionEditingPrivacyAndXssEndToEnd()
    {
        var(a,tutor)=await Setup();using(a)using(tutor)using(var admin=a.Client())using(var guest=a.Client()){
        await a.UserAsync("tutor@example.test","Tutor");await a.UserAsync("admin@example.test","Admin");await Login(tutor,"tutor@example.test");await Login(admin,"admin@example.test");
        Assert.Equal(HttpStatusCode.Redirect,(await SaveProfile(tutor,introduction:"<script>alert('xss')</script> Tôi dạy Toán rất tận tâm.")).StatusCode);
        int id;string revision;
        using(var s=a.Services.CreateScope()){var p=await s.ServiceProvider.GetRequiredService<ApplicationDbContext>().TutorProfiles.SingleAsync();id=p.Id;revision=p.Revision;Assert.Equal(ProfileStatus.Pending,p.Status);}
        Assert.Equal(HttpStatusCode.NotFound,(await guest.GetAsync($"/TutorProfiles/Details/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,(await guest.GetAsync($"/TutorProfiles/Photo/{id}")).StatusCode);
        await Post(admin,"/Admin/Profiles/Moderate",new(){{"Id",id.ToString()},{"Revision",revision},{"Approve","false"}},$"/Admin/Profiles/Details/{id}");
        using(var s=a.Services.CreateScope()){Assert.Equal(ProfileStatus.Pending,(await s.ServiceProvider.GetRequiredService<ApplicationDbContext>().TutorProfiles.SingleAsync()).Status);}
        await Post(admin,"/Admin/Profiles/Moderate",new(){{"Id",id.ToString()},{"Revision",revision},{"Approve","false"},{"Reason","Bổ sung thông tin"}},$"/Admin/Profiles/Details/{id}");
        using(var s=a.Services.CreateScope()){var p=await s.ServiceProvider.GetRequiredService<ApplicationDbContext>().TutorProfiles.SingleAsync();Assert.Equal(ProfileStatus.Rejected,p.Status);revision=p.Revision;}
        await SaveProfile(tutor,revision,introduction:"<script>alert('xss')</script> Tôi dạy Toán rất tận tâm.");
        using(var s=a.Services.CreateScope()){revision=(await s.ServiceProvider.GetRequiredService<ApplicationDbContext>().TutorProfiles.SingleAsync()).Revision;}
        Assert.Equal(HttpStatusCode.Redirect,(await Post(admin,"/Admin/Profiles/Moderate",new(){{"Id",id.ToString()},{"Revision",revision},{"Approve","true"}},$"/Admin/Profiles/Details/{id}")).StatusCode);
        var html=await guest.GetStringAsync($"/TutorProfiles/Details/{id}");Assert.Contains("&lt;script&gt;",html);Assert.DoesNotContain("<script>alert",html);Assert.Contains("Booking",html);
        Assert.Equal(HttpStatusCode.OK,(await guest.GetAsync($"/TutorProfiles/Booking/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await guest.GetAsync($"/TutorProfiles/Photo/{id}")).StatusCode);
        using var student=a.Client();await a.UserAsync("other@example.test","Student");await Login(student,"other@example.test");
        Assert.Equal(HttpStatusCode.Forbidden,(await student.GetAsync($"/TutorProfiles/Certificate/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await admin.GetAsync($"/TutorProfiles/Certificate/{id}")).StatusCode);
        using(var s=a.Services.CreateScope()){revision=(await s.ServiceProvider.GetRequiredService<ApplicationDbContext>().TutorProfiles.SingleAsync()).Revision;}
        await SaveProfile(tutor,revision);
        Assert.Equal(HttpStatusCode.NotFound,(await guest.GetAsync($"/TutorProfiles/Details/{id}")).StatusCode);
        Assert.Equal(2,a.Mail.Messages.Count);}
    }
    [Theory]
    [InlineData("photo.exe",false)]
    [InlineData("photo.jpg",false)]
    [InlineData("photo.png",true)]
    public async Task InvalidUploadsRejected(string name,bool oversized)
    {
        var(a,c)=await Setup();using(a)using(c){await a.UserAsync("upload@example.test","Tutor");await Login(c,"upload@example.test");
        await SaveProfile(c,filename:name,bytes:oversized?new byte[5*1024*1024+1]:Encoding.UTF8.GetBytes("fake image"));
        using var s=a.Services.CreateScope();Assert.False(await s.ServiceProvider.GetRequiredService<ApplicationDbContext>().TutorProfiles.AnyAsync());}
    }
    [Fact] public async Task InvalidPriceRejectedAndSqlInputDoesNotChangeSchema()
    {
        var(a,c)=await Setup();using(a)using(c){await a.UserAsync("price@example.test","Tutor");await Login(c,"price@example.test");await SaveProfile(c,price:-1);
        await Post(c,"/Account/Login",new(){{"Email","' OR 1=1; DROP TABLE AspNetUsers;--"},{"Password","password"}});
        using var s=a.Services.CreateScope();var db=s.ServiceProvider.GetRequiredService<ApplicationDbContext>();Assert.False(await db.TutorProfiles.AnyAsync());Assert.Equal(1,await db.Users.CountAsync());}
    }
    [Fact] public async Task LogoutRevokesCopiedCookieAndPostRequired()
    {
        var(a,c)=await Setup();using(a)using(c){await a.UserAsync("logout@example.test","Tutor");var login=await Login(c,"logout@example.test");
        var copiedCookie=login.Headers.GetValues("Set-Cookie").Single(x=>x.StartsWith(".AspNetCore.Identity.Application=")).Split(';')[0];
        Assert.Equal(HttpStatusCode.OK,(await c.GetAsync("/TutorProfiles/Mine")).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect,(await Post(c,"/Account/Logout",new(),"/")).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect,(await c.GetAsync("/TutorProfiles/Mine")).StatusCode);
        using var replay=a.Client();replay.DefaultRequestHeaders.Add("Cookie",copiedCookie);
        Assert.Equal(HttpStatusCode.Redirect,(await replay.GetAsync("/TutorProfiles/Mine")).StatusCode);}
    }
}
