using GiaSuOnline.Web.Data;
using GiaSuOnline.Web.Models;
using GiaSuOnline.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace GiaSuOnline.Web.Services;
public class ProfileService(ApplicationDbContext db, UploadStore uploads)
{
    public IQueryable<TutorProfile> Profiles => db.TutorProfiles.Include(x => x.User).Include(x => x.Subjects).ThenInclude(x => x.Subject);
    public async Task<TutorDetail?> PublicDetailAsync(int id)
    {
        var p = await Profiles.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.Status == ProfileStatus.Approved);
        if (p == null) return null;
        var start = DateTime.UtcNow;
        var end = start.AddDays(7);
        return new(p, await db.TutorReviews.AsNoTracking().Where(x => x.TutorProfileId == id).OrderByDescending(x => x.CreatedAtUtc).Take(5).ToListAsync(),
            await db.AvailabilitySlots.AsNoTracking().Where(x => x.TutorProfileId == id && !x.IsBooked && x.StartsAtUtc >= start && x.StartsAtUtc < end).OrderBy(x => x.StartsAtUtc).ToListAsync());
    }
    public async Task SaveAsync(string userId, ProfileForm form)
    {
        var ids = form.SubjectIds.Distinct().ToArray();
        if (ids.Length == 0 || await db.Subjects.CountAsync(x => ids.Contains(x.Id)) != ids.Length)
            throw new ArgumentException("Môn học không hợp lệ.");
        var p = await db.TutorProfiles.Include(x => x.Subjects).SingleOrDefaultAsync(x => x.UserId == userId);
        if (p == null) { p = new() { UserId = userId }; db.Add(p); }
        else
        {
            if (p.Revision != form.Revision) throw new DbUpdateConcurrencyException();
            db.TutorSubjects.RemoveRange(p.Subjects);
        }
        var oldPhoto = p.PhotoKey; var oldCertificate = p.CertificateKey;
        var newKeys = new List<string>();
        try
        {
            if (form.Photo != null) { var f = await uploads.SaveAsync(form.Photo); p.PhotoKey = f.Key; p.PhotoContentType = f.ContentType; newKeys.Add(f.Key); }
            if (form.Certificate != null) { var f = await uploads.SaveAsync(form.Certificate); p.CertificateKey = f.Key; p.CertificateContentType = f.ContentType; newKeys.Add(f.Key); }
            p.Introduction = form.Introduction.Trim(); p.HourlyRate = form.HourlyRate; p.ExperienceYears = form.ExperienceYears;
            p.Status = ProfileStatus.Pending; p.RejectionReason = null; p.UpdatedAt = DateTimeOffset.UtcNow; p.Revision = Guid.NewGuid().ToString("N");
            p.Subjects = ids.Select(id => new TutorSubject { SubjectId = id, TutorProfile = p }).ToList();
            await db.SaveChangesAsync();
        }
        catch { foreach (var key in newKeys) uploads.Delete(key); throw; }
        if (form.Photo != null) uploads.Delete(oldPhoto);
        if (form.Certificate != null) uploads.Delete(oldCertificate);
    }
}
