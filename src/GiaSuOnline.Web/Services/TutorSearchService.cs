using GiaSuOnline.Web.Data;
using GiaSuOnline.Web.Models;
using GiaSuOnline.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace GiaSuOnline.Web.Services;
public class TutorSearchService(ApplicationDbContext db)
{
    public async Task<SearchResults> SearchAsync(SearchForm f)
    {
        var q = db.TutorProfiles.AsNoTracking().Where(x => x.Status == ProfileStatus.Approved);
        if (!string.IsNullOrWhiteSpace(f.Q))
        {
            var term = f.Q.Trim().ToLowerInvariant();
            q = q.Where(x => x.User.FullName.ToLower().Contains(term) || x.Introduction.ToLower().Contains(term) || x.Subjects.Any(s => s.Subject.Name.ToLower().Contains(term)));
        }
        if (f.SubjectId.HasValue) q = q.Where(x => x.Subjects.Any(s => s.SubjectId == f.SubjectId));
        // cast chỉ phục vụ lọc/sắp xếp trên cả SQLite; giá và booking vẫn lưu decimal.
        if (f.MinPrice.HasValue) q = q.Where(x => (double)x.HourlyRate >= f.MinPrice);
        if (f.MaxPrice.HasValue) q = q.Where(x => (double)x.HourlyRate <= f.MaxPrice);
        if (!string.IsNullOrEmpty(f.Language)) q = q.Where(x => x.TeachingLanguage == f.Language);
        if (f.MinRating.HasValue && f.MinRating > 0) q = q.Where(x => (x.Reviews.Select(r => (double?)r.Stars).Average() ?? 0) >= f.MinRating);
        var total = await q.CountAsync();
        f.Page = Math.Min(f.Page, Math.Max(1, (int)Math.Ceiling(total / 10.0)));
        var sorted = f.Sort switch
        {
            "price" => q.OrderBy(x => (double)x.HourlyRate),
            "price_desc" => q.OrderByDescending(x => (double)x.HourlyRate),
            _ => q.OrderByDescending(x => x.Reviews.Select(r => (double?)r.Stars).Average() ?? 0)
        };
        var items = await sorted.ThenBy(x => x.Id).Skip((f.Page - 1) * 10).Take(10)
            .Select(x => new TutorCard(x.Id, x.User.FullName, x.HourlyRate, x.ExperienceYears, x.TeachingLanguage,
                x.Reviews.Select(r => (double?)r.Stars).Average(), x.Reviews.Count,
                x.Subjects.Select(s => s.Subject.Name).ToList(), x.PhotoKey != null)).ToListAsync();
        return new(f, items, await SubjectsAsync(), total);
    }
    public Task<List<SelectListItem>> SubjectsAsync() => db.Subjects.AsNoTracking().OrderBy(x => x.Name).Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToListAsync();
    public async Task<List<string>> SuggestAsync(string? term)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2 || term.Length > 100) return [];
        term = term.Trim().ToLowerInvariant();
        var names = await db.TutorProfiles.AsNoTracking().Where(x => x.Status == ProfileStatus.Approved && x.User.FullName.ToLower().Contains(term))
            .Select(x => x.User.FullName).Distinct().OrderBy(x => x).Take(5).ToListAsync();
        var subjects = await db.Subjects.AsNoTracking().Where(x => x.Name.ToLower().Contains(term)
            && db.TutorSubjects.Any(s => s.SubjectId == x.Id && s.TutorProfile.Status == ProfileStatus.Approved))
            .OrderBy(x => x.Name).Select(x => x.Name).Take(3).ToListAsync();
        return names.Concat(subjects).Distinct().Take(8).ToList();
    }
}
