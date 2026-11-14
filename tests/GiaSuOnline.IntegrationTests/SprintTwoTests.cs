using System.Net;
using System.Text.RegularExpressions;
using GiaSuOnline.Web.Data;
using GiaSuOnline.Web.Data.Seed;
using GiaSuOnline.Web.Jobs;
using GiaSuOnline.Web.Models;
using GiaSuOnline.Web.Services;
using GiaSuOnline.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GiaSuOnline.IntegrationTests;
public class SprintTwoTests
{
    private static async Task<SprintTwoFixture> App()
    { var f=new SprintTwoFixture();await f.InitAsync();return f; }
    private static async Task<string> Token(HttpClient c,string path)
    { var body=await c.GetStringAsync(path);return WebUtility.HtmlDecode(Regex.Match(body,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value); }
    private static async Task<HttpResponseMessage> Post(HttpClient c,string path,Dictionary<string,string> form,string? get=null)
    {form["__RequestVerificationToken"]=await Token(c,get??path);return await c.PostAsync(path,new FormUrlEncodedContent(form));}
    private static Task<HttpResponseMessage> Login(HttpClient c,string name) => Post(c,"/Account/Login",new(){{"Email",name+"@example.test"},{"Password","Password123!"}});
    [Fact] public async Task CombinedSearchLanguagePriceRatingAndPagination()
    {
        await using var f=await App();
        await f.Run(async sp=>
        {
            var db=sp.GetRequiredService<ApplicationDbContext>();
            db.TutorReviews.Add(new(){TutorProfileId=f.TutorId,Stars=5,Comment="Test rating",CreatedAtUtc=f.Clock.Now.UtcDateTime});
            var other=await db.TutorProfiles.FindAsync(f.OtherTutorId);other!.TeachingLanguage="en";other.HourlyRate=300000;
            await db.SaveChangesAsync();
            var s=sp.GetRequiredService<TutorSearchService>();
            var result=await s.SearchAsync(new(){SubjectId=f.SubjectId,MinPrice=100000,MaxPrice=150000,MinRating=4,Language="vi"});
            Assert.Single(result.Items);Assert.Equal(f.TutorId,result.Items[0].Id);Assert.Equal(5,result.Items[0].Rating);
            Assert.Empty((await s.SearchAsync(new(){Language="en",MaxPrice=150000})).Items);
            Assert.DoesNotContain((await s.SearchAsync(new())).Items,x=>x.Id==f.PendingTutorId);
            Assert.Empty((await s.SuggestAsync("pending")));
            Assert.Contains("tutor",await s.SuggestAsync("tutor"));
        });
    }
    [Fact] public async Task Search500ProfilesHasStableTenItemPagesAndSorts()
    {
        await using var f=await App();
        await f.Run(async sp=>
        {
            var db=sp.GetRequiredService<ApplicationDbContext>(); await SearchDemoSeed.RunAsync(db);
            var service=sp.GetRequiredService<TutorSearchService>();
            var first=await service.SearchAsync(new(){Sort="price"});var second=await service.SearchAsync(new(){Sort="price",Page=2});
            Assert.Equal(10,first.Items.Count);Assert.Equal(10,second.Items.Count);Assert.Empty(first.Items.Select(x=>x.Id).Intersect(second.Items.Select(x=>x.Id)));
            Assert.True(first.Items.Select(x=>x.Rate).SequenceEqual(first.Items.Select(x=>x.Rate).OrderBy(x=>x)));
            Assert.True((await service.SearchAsync(new(){Sort="price_desc"})).Items[0].Rate >= first.Items.Last().Rate);
        });
    }
    [Fact] public async Task SlotCrudRejectsOverlapPastAndOtherOwnerAndConvertsUtc()
    {
        await using var f=await App();var id=await f.Slot();
        var form=await f.Form(id);
        await Assert.ThrowsAsync<BookingRuleException>(()=>f.Slot());
        await Assert.ThrowsAsync<BookingRuleException>(()=>f.Slot(day:-1));
        await f.Run(async sp=>
        {
            var s=sp.GetRequiredService<SchedulingService>();var db=sp.GetRequiredService<ApplicationDbContext>();
            var slot=await db.AvailabilitySlots.AsNoTracking().SingleAsync(x=>x.Id==id);
            Assert.Equal(10,slot.StartsAtUtc.Hour);Assert.Equal(17,VietnamTime.Local(slot.StartsAtUtc).Hour);
            await Assert.ThrowsAsync<BookingRuleException>(()=>s.DeleteSlotAsync(f.OtherOwner,id,form.SlotRevision));
        });
        await f.Run(async sp=>
        {
            var s=sp.GetRequiredService<SchedulingService>();
            await s.SaveSlotAsync(f.Owner,new(){Id=id,Revision=form.SlotRevision,StartsAt=VietnamTime.Local(f.Clock.Now.UtcDateTime.AddDays(3)),EndsAt=VietnamTime.Local(f.Clock.Now.UtcDateTime.AddDays(3).AddHours(1))});
        });
        var edited=await f.Form(id);await f.Run(sp=>sp.GetRequiredService<SchedulingService>().DeleteSlotAsync(f.Owner,id,edited.SlotRevision));
        Assert.True(await f.Run(sp=>sp.GetRequiredService<ApplicationDbContext>().AvailabilitySlots.Where(x=>x.Id==id).Select(x=>x.IsDeleted).SingleAsync()));
    }
    [Fact] public async Task PendingTutorCannotCreateSlots()
    {
        await using var f=await App();
        var pendingOwner=await f.Run(sp=>sp.GetRequiredService<ApplicationDbContext>().TutorProfiles.Where(x=>x.Id==f.PendingTutorId).Select(x=>x.UserId).SingleAsync());
        await Assert.ThrowsAsync<BookingRuleException>(()=>f.Run(sp=>sp.GetRequiredService<SchedulingService>().SaveSlotAsync(pendingOwner,new(){StartsAt=new(2030,1,10,10,0,0),EndsAt=new(2030,1,10,11,0,0)})));
    }
    [Fact] public async Task BookingSnapshotsPriceBlocksChangesAndCreatesOneEmail()
    {
        await using var f=await App();var id=await f.Slot(minutes:90);var form=await f.Form(id);
        var bookingId=await f.Run(sp=>sp.GetRequiredService<SchedulingService>().BookAsync(f.Student,form));
        await f.Run(async sp=>
        {
            var db=sp.GetRequiredService<ApplicationDbContext>();var b=await db.Bookings.AsNoTracking().SingleAsync();
            Assert.Equal(bookingId,b.Id);Assert.Equal(180000,b.Amount);Assert.Equal(BookingStatus.Pending,b.Status);Assert.Equal(f.Clock.Now.UtcDateTime.AddHours(24),b.ExpiresAtUtc);
            Assert.Equal(1,await db.BookingEmails.CountAsync());
            var p=await db.TutorProfiles.FindAsync(f.TutorId);p!.HourlyRate=500000;await db.SaveChangesAsync();
            Assert.Equal(120000,(await db.Bookings.AsNoTracking().SingleAsync()).HourlyRate);
        });
        await Assert.ThrowsAsync<BookingRuleException>(()=>f.Run(sp=>sp.GetRequiredService<SchedulingService>().BookAsync(f.Student2,form)));
        await Assert.ThrowsAsync<BookingRuleException>(()=>f.Run(sp=>sp.GetRequiredService<SchedulingService>().DeleteSlotAsync(f.Owner,id,form.SlotRevision)));
        await Assert.ThrowsAsync<BookingRuleException>(()=>f.Run(sp=>sp.GetRequiredService<SchedulingService>().SaveSlotAsync(f.Owner,new(){Id=id,Revision=form.SlotRevision,StartsAt=new(2030,1,10,10,0,0),EndsAt=new(2030,1,10,11,0,0)})));
    }
    [Fact] public async Task ChangedProfileOrSlotAndPastSlotCannotBeBooked()
    {
        await using var f=await App();var id=await f.Slot();var form=await f.Form(id);
        await f.Run(async sp=>{var db=sp.GetRequiredService<ApplicationDbContext>();var p=await db.TutorProfiles.FindAsync(f.TutorId);p!.Revision=Guid.NewGuid().ToString("N");await db.SaveChangesAsync();});
        await Assert.ThrowsAsync<BookingRuleException>(()=>f.Run(sp=>sp.GetRequiredService<SchedulingService>().BookAsync(f.Student,form)));
        var updated=await f.Form(id);f.Clock.Now=f.Clock.Now.AddDays(5);
        await Assert.ThrowsAsync<BookingRuleException>(()=>f.Run(sp=>sp.GetRequiredService<SchedulingService>().BookAsync(f.Student,updated)));
    }
    [Fact] public async Task RejectReopensSlotAndRebookingPreservesHistory()
    {
        await using var f=await App();var id=await f.Slot();var booking=await f.Run(async sp=>await sp.GetRequiredService<SchedulingService>().BookAsync(f.Student,await f.Form(id)));
        var revision=await f.Run(sp=>sp.GetRequiredService<ApplicationDbContext>().Bookings.Where(x=>x.Id==booking).Select(x=>x.Revision).SingleAsync());
        await Assert.ThrowsAsync<BookingRuleException>(()=>f.Run(sp=>sp.GetRequiredService<SchedulingService>().DecideAsync(f.OtherOwner,new(){Id=booking,Revision=revision,Accept=true})));
        Assert.Equal(BookingStatus.Rejected,await f.Run(sp=>sp.GetRequiredService<SchedulingService>().DecideAsync(f.Owner,new(){Id=booking,Revision=revision,Accept=false})));
        var next=await f.Run(async sp=>await sp.GetRequiredService<SchedulingService>().BookAsync(f.Student2,await f.Form(id)));
        Assert.NotEqual(booking,next);
        Assert.Equal(2,await f.Run(sp=>sp.GetRequiredService<ApplicationDbContext>().Bookings.CountAsync()));
    }
    [Fact] public async Task ConfirmCannotBeProcessedTwiceAndSlotStaysReserved()
    {
        await using var f=await App();var id=await f.Slot();var booking=await f.Run(async sp=>await sp.GetRequiredService<SchedulingService>().BookAsync(f.Student,await f.Form(id)));
        var rev=await f.Run(sp=>sp.GetRequiredService<ApplicationDbContext>().Bookings.Where(x=>x.Id==booking).Select(x=>x.Revision).SingleAsync());
        Assert.Equal(BookingStatus.Confirmed,await f.Run(sp=>sp.GetRequiredService<SchedulingService>().DecideAsync(f.Owner,new(){Id=booking,Revision=rev,Accept=true})));
        await Assert.ThrowsAsync<BookingRuleException>(()=>f.Run(sp=>sp.GetRequiredService<SchedulingService>().DecideAsync(f.Owner,new(){Id=booking,Revision=rev,Accept=false})));
        Assert.True(await f.Run(sp=>sp.GetRequiredService<ApplicationDbContext>().AvailabilitySlots.Where(x=>x.Id==id).Select(x=>x.IsBooked).SingleAsync()));
    }
    [Fact] public async Task ExpirationAt24HoursReleasesSlotOnceAndRejectsLateConfirmation()
    {
        await using var f=await App();var id=await f.Slot(day:3);var b=await f.Run(async sp=>await sp.GetRequiredService<SchedulingService>().BookAsync(f.Student,await f.Form(id)));
        var revision=await f.Run(sp=>sp.GetRequiredService<ApplicationDbContext>().Bookings.Where(x=>x.Id==b).Select(x=>x.Revision).SingleAsync());
        f.Clock.Now=f.Clock.Now.AddHours(24);
        await f.Run(sp=>sp.GetRequiredService<SchedulingService>().ExpireAsync());await f.Run(sp=>sp.GetRequiredService<SchedulingService>().ExpireAsync());
        Assert.Equal(BookingStatus.Expired,await f.Run(sp=>sp.GetRequiredService<SchedulingService>().DecideAsync(f.Owner,new(){Id=b,Revision=revision,Accept=true})));
        Assert.False(await f.Run(sp=>sp.GetRequiredService<ApplicationDbContext>().AvailabilitySlots.Where(x=>x.Id==id).Select(x=>x.IsBooked).SingleAsync()));
        Assert.Equal(2,await f.Run(sp=>sp.GetRequiredService<ApplicationDbContext>().BookingEmails.CountAsync()));
    }
    [Fact] public async Task StartTimeBefore24HoursAlsoExpiresPendingRequest()
    {
        await using var f=await App();var id=await f.Slot(day:0,hour:2);await f.Run(async sp=>await sp.GetRequiredService<SchedulingService>().BookAsync(f.Student,await f.Form(id)));
        f.Clock.Now=f.Clock.Now.AddHours(2);await f.Run(sp=>sp.GetRequiredService<SchedulingService>().ExpireAsync());
        Assert.Equal(BookingStatus.Expired,await f.Run(sp=>sp.GetRequiredService<ApplicationDbContext>().Bookings.Select(x=>x.Status).SingleAsync()));
    }
    [Fact] public async Task EmailFailureRetriesWithoutDuplicateBookingAndDispatcherDoesNotResendSent()
    {
        await using var f=await App();var id=await f.Slot();await f.Run(async sp=>await sp.GetRequiredService<SchedulingService>().BookAsync(f.Student,await f.Form(id)));
        f.Mail.Failures=1;await f.Run(sp=>sp.GetRequiredService<BookingEmailDispatcher>().SendPendingAsync());
        Assert.Empty(f.Mail.Subjects);Assert.Equal(1,await f.Run(sp=>sp.GetRequiredService<ApplicationDbContext>().Bookings.CountAsync()));
        f.Clock.Now=f.Clock.Now.AddMinutes(3);await f.Run(sp=>sp.GetRequiredService<BookingEmailDispatcher>().SendPendingAsync());await f.Run(sp=>sp.GetRequiredService<BookingEmailDispatcher>().SendPendingAsync());
        Assert.Single(f.Mail.Subjects);
    }
    [Fact] public async Task HttpBookingFlowOwnershipRoleCsrfAndFriendlyDoubleSubmit()
    {
        await using var f=await App();var id=await f.Slot();using var guest=f.Client();using var student=f.Client();using var other=f.Client();using var tutor=f.Client();
        Assert.Equal(HttpStatusCode.Redirect,(await guest.GetAsync($"/Bookings/Confirm?slotId={id}")).StatusCode);
        await Login(student,"student");await Login(other,"student2");await Login(tutor,"tutor");
        Assert.Equal(HttpStatusCode.Forbidden,(await student.GetAsync("/Availability")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await student.PostAsync("/Bookings/Create",new FormUrlEncodedContent([]))).StatusCode);
        var form=await f.Form(id);var values=new Dictionary<string,string>{{"SlotId",id.ToString()},{"SlotRevision",form.SlotRevision},{"ProfileRevision",form.ProfileRevision},{"StudentId",f.Student2}};
        var result=await Post(student,"/Bookings/Create",values,$"/Bookings/Confirm?slotId={id}");Assert.Equal(HttpStatusCode.Redirect,result.StatusCode);
        var location=result.Headers.Location!.ToString();Assert.Equal(HttpStatusCode.OK,(await student.GetAsync(location)).StatusCode);Assert.Equal(HttpStatusCode.NotFound,(await other.GetAsync(location)).StatusCode);
        Assert.Equal(f.Student,await f.Run(sp=>sp.GetRequiredService<ApplicationDbContext>().Bookings.Select(x=>x.StudentId).SingleAsync()));
        Assert.Equal(HttpStatusCode.Redirect,(await Post(student,"/Bookings/Create",values,"/Bookings/Mine")).StatusCode);Assert.Equal(1,await f.Run(sp=>sp.GetRequiredService<ApplicationDbContext>().Bookings.CountAsync()));
        var b=await f.Run(sp=>sp.GetRequiredService<ApplicationDbContext>().Bookings.AsNoTracking().SingleAsync());
        Assert.Equal(HttpStatusCode.Redirect,(await Post(tutor,"/Bookings/Decide",new(){{"Id",b.Id.ToString()},{"Revision",b.Revision},{"Accept","true"}},"/Bookings/Requests")).StatusCode);
        Assert.Contains("Đã xác nhận",WebUtility.HtmlDecode(await student.GetStringAsync(location)));
        Assert.Equal(HttpStatusCode.BadRequest,(await guest.GetAsync("/Tutors?MinPrice=500&MaxPrice=100")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await guest.GetAsync("/Tutors?Q=%27%20OR%201%3D1--")).StatusCode);
    }
    [Fact] public async Task TwoStudentsCompeteForOneSlotOnSeparateSqliteConnections()
    {await using var f=await App();await Race(f);}
    [SqlServerFact, Trait("Database","SqlServer")] public async Task SqlServerConcurrentBookingAndOverlappingSlots()
    {
        await using var f=new SprintTwoFixture(true);await f.InitAsync();await Race(f);
        var gate=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<bool> Add(){await gate.Task;try{await f.Slot(day:4);return true;}catch(BookingRuleException){return false;}}
        var one=Task.Run(Add);var two=Task.Run(Add);gate.SetResult();Assert.Single(await Task.WhenAll(one,two), x=>x);
        var booking=await f.Run(sp=>sp.GetRequiredService<ApplicationDbContext>().Bookings.AsNoTracking().SingleAsync());
        f.Clock.Now=f.Clock.Now.AddHours(24);
        await Task.WhenAll(f.Run(sp=>sp.GetRequiredService<SchedulingService>().ExpireAsync()),
            f.Run(sp=>sp.GetRequiredService<SchedulingService>().DecideAsync(f.Owner,new(){Id=booking.Id,Revision=booking.Revision,Accept=true})));
        Assert.Equal(BookingStatus.Expired,await f.Run(sp=>sp.GetRequiredService<ApplicationDbContext>().Bookings.Select(x=>x.Status).SingleAsync()));
        Assert.Equal(2,await f.Run(sp=>sp.GetRequiredService<ApplicationDbContext>().BookingEmails.CountAsync()));
    }
    [SqlServerFact, Trait("Database","SqlServer")] public async Task SqlServerMigrationPreservesSprintOneProfilesAndExistingSlots()
    {
        await using var f=new SprintTwoFixture(true);
        await f.Run(async sp=>
        {
            var db=sp.GetRequiredService<ApplicationDbContext>();
            var migrator=Microsoft.EntityFrameworkCore.Infrastructure.AccessorExtensions.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>(db);
            await migrator.MigrateAsync("20261028183755_InitialSprintOne");
            var u=new ApplicationUser{Id="migration-test",FullName="Existing tutor",Email="migration@example.test",UserName="migration@example.test"};db.Users.Add(u);await db.SaveChangesAsync();
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO [TutorProfiles] ([UserId],[Introduction],[HourlyRate],[ExperienceYears],[Status],[UpdatedAt],[Revision]) VALUES ({u.Id},{"Existing profile retained"},{120000m},{2},{1},{DateTimeOffset.UtcNow},{"old-revision"})");
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO [AvailabilitySlots] ([TutorProfileId],[StartsAtUtc],[EndsAtUtc],[IsBooked]) SELECT [Id],{new DateTime(2030,1,9,10,0,0)},{new DateTime(2030,1,9,11,0,0)},{false} FROM [TutorProfiles] WHERE [UserId]={u.Id}");
            await migrator.MigrateAsync();
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Equal("vi",(await db.TutorProfiles.AsNoTracking().SingleAsync()).TeachingLanguage);
            Assert.Equal(32,(await db.AvailabilitySlots.AsNoTracking().SingleAsync()).Revision.Length);
        });
    }
    [Fact] public async Task ParallelEmailDispatchersClaimOnlyOneDelivery()
    {
        await using var f=await App();var id=await f.Slot();await f.Run(async sp=>await sp.GetRequiredService<SchedulingService>().BookAsync(f.Student,await f.Form(id)));
        await Task.WhenAll(Task.Run(()=>f.Run(sp=>sp.GetRequiredService<BookingEmailDispatcher>().SendPendingAsync())),Task.Run(()=>f.Run(sp=>sp.GetRequiredService<BookingEmailDispatcher>().SendPendingAsync())));
        Assert.Single(f.Mail.Subjects);
    }
    [Fact] public async Task WeeklyCalendarAndEventsAreOwnerScopedAndUtc()
    {
        await using var f=await App();await f.Slot();using var tutor=f.Client();using var other=f.Client();await Login(tutor,"tutor");await Login(other,"other");
        var html=await tutor.GetStringAsync("/Availability?week=2030-01-07");Assert.Contains("fullcalendar",html);Assert.Contains("csp-nonce",html);
        const string path="/Availability/Events?start=2030-01-06T17%3A00%3A00Z&end=2030-01-13T17%3A00%3A00Z";
        var events=await tutor.GetStringAsync(path);Assert.Contains("2030-01-09T10:00:00",events);Assert.Equal("[]",await other.GetStringAsync(path));
        Assert.Equal(HttpStatusCode.BadRequest,(await tutor.GetAsync("/Availability/Events?start=2030-01-01&end=2031-01-01")).StatusCode);
    }
    private static async Task Race(SprintTwoFixture f)
    {
        var slot=await f.Slot();var form=await f.Form(slot);var gate=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<bool> Book(string user){await gate.Task;try{await f.Run(sp=>sp.GetRequiredService<SchedulingService>().BookAsync(user,form));return true;}catch(BookingRuleException){return false;}}
        var a=Task.Run(()=>Book(f.Student));var b=Task.Run(()=>Book(f.Student2));gate.SetResult();Assert.Single(await Task.WhenAll(a,b), x=>x);
        Assert.Equal(1,await f.Run(sp=>sp.GetRequiredService<ApplicationDbContext>().Bookings.CountAsync()));Assert.Equal(1,await f.Run(sp=>sp.GetRequiredService<ApplicationDbContext>().BookingEmails.CountAsync()));
    }
}
