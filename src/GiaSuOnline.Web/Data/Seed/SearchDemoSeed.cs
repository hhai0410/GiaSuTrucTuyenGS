using GiaSuOnline.Web.Models;
using Microsoft.EntityFrameworkCore;
namespace GiaSuOnline.Web.Data.Seed;
public static class SearchDemoSeed
{
    // Chỉ gọi từ công cụ benchmark với database *_Demo, không seed vào dữ liệu thật.
    public static async Task RunAsync(ApplicationDbContext db)
    {
        if (await db.TutorProfiles.CountAsync() >= 500) return;
        var subjects = await db.Subjects.OrderBy(x => x.Id).Select(x => x.Id).ToListAsync();
        for (var i = 0; i < 500; i++)
        {
            var id = $"gs-search-demo-{i:D3}";
            if (await db.Users.AnyAsync(x => x.Id == id)) continue;
            var user = new ApplicationUser { Id = id, FullName = $"Gia sư demo {i:D3}", UserName = $"tutor{i:D3}@gs-demo.invalid", NormalizedUserName = $"TUTOR{i:D3}@GS-DEMO.INVALID", Email = $"tutor{i:D3}@gs-demo.invalid", NormalizedEmail = $"TUTOR{i:D3}@GS-DEMO.INVALID", SecurityStamp = Guid.NewGuid().ToString() };
            var p = new TutorProfile { User = user, Status = ProfileStatus.Approved, Introduction = "Dữ liệu giả chỉ dùng kiểm thử tìm kiếm, không phải hồ sơ gia sư thật.", HourlyRate = 100000 + i % 10 * 50000, ExperienceYears = i % 8, TeachingLanguage = i % 2 == 0 ? "vi" : "en" };
            p.Subjects.Add(new TutorSubject { SubjectId = subjects[i % subjects.Count] });
            p.Reviews.Add(new TutorReview { Stars = 1 + i % 5, Comment = "Đánh giá giả cho benchmark, không phải đánh giá thực.", CreatedAtUtc = DateTime.UtcNow });
            db.TutorProfiles.Add(p);
        }
        await db.SaveChangesAsync();
    }
}
