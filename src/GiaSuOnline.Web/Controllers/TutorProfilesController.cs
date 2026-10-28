using System.Security.Claims;
using GiaSuOnline.Web.Data;
using GiaSuOnline.Web.Models;
using GiaSuOnline.Web.Services;
using GiaSuOnline.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace GiaSuOnline.Web.Controllers;
public class TutorProfilesController(ProfileService service, ApplicationDbContext db, UploadStore uploads) : Controller
{
    [Authorize(Roles="Tutor")] public async Task<IActionResult> Mine()
        => View(await service.Profiles.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)));
    [HttpGet, Authorize(Roles="Tutor")] public async Task<IActionResult> Edit()
    {
        var p = await service.Profiles.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));
        var form = p == null ? new ProfileForm() : new ProfileForm { Introduction=p.Introduction, HourlyRate=p.HourlyRate, ExperienceYears=p.ExperienceYears, SubjectIds=p.Subjects.Select(x => x.SubjectId).ToArray(), Revision=p.Revision, HasPhoto=p.PhotoKey!=null, HasCertificate=p.CertificateKey!=null };
        await Options(form); return View(form);
    }
    [HttpPost, Authorize(Roles="Tutor"), RequestSizeLimit(12 * 1024 * 1024)] public async Task<IActionResult> Edit(ProfileForm form)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var current = await db.TutorProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId);
        form.HasPhoto=current?.PhotoKey!=null; form.HasCertificate=current?.CertificateKey!=null;
        if (form.Photo == null && !form.HasPhoto) ModelState.AddModelError("Photo", "Vui lòng thêm ảnh đại diện.");
        if (form.Certificate == null && !form.HasCertificate) ModelState.AddModelError("Certificate", "Vui lòng thêm chứng chỉ.");
        foreach (var (file, photo, key) in new[] {(form.Photo, true, "Photo"), (form.Certificate, false, "Certificate")})
            if (file != null && await UploadStore.ValidateAsync(file, photo) is string error) ModelState.AddModelError(key, error);
        if (ModelState.IsValid)
        {
            try { await service.SaveAsync(userId, form); TempData["Success"]="Đã lưu hồ sơ và gửi chờ duyệt."; return RedirectToAction(nameof(Mine)); }
            catch (ArgumentException e) { ModelState.AddModelError("", e.Message); }
            catch (DbUpdateConcurrencyException) { ModelState.AddModelError("", "Hồ sơ vừa thay đổi. Hãy tải lại trang trước khi lưu."); }
        }
        await Options(form); return View(form);
    }
    private async Task Options(ProfileForm form) => form.SubjectOptions = await db.Subjects.OrderBy(x => x.Name).Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToListAsync();
    [AllowAnonymous, HttpGet] public async Task<IActionResult> Details(int id)
    { var detail=await service.PublicDetailAsync(id); return detail == null ? NotFound() : View(detail); }
    [AllowAnonymous, HttpGet] public async Task<IActionResult> Photo(int id)
    {
        var p=await db.TutorProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.Id==id);
        if (p == null || p.PhotoKey == null || (p.Status != ProfileStatus.Approved && !User.IsInRole("Admin") && p.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))) return NotFound();
        var path=uploads.PathFor(p.PhotoKey); return System.IO.File.Exists(path) ? PhysicalFile(path,p.PhotoContentType!, enableRangeProcessing:false) : NotFound();
    }
    [Authorize, HttpGet] public async Task<IActionResult> Certificate(int id)
    {
        var p=await db.TutorProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.Id==id);
        if (p?.CertificateKey == null) return NotFound();
        if (!User.IsInRole("Admin") && p.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();
        var path=uploads.PathFor(p.CertificateKey);
        return System.IO.File.Exists(path) ? PhysicalFile(path,p.CertificateContentType!, "chung-chi"+Path.GetExtension(path)) : NotFound();
    }
    [HttpGet, AllowAnonymous] public async Task<IActionResult> Booking(int id)
    { var d=await service.PublicDetailAsync(id); return d == null ? NotFound() : View(d); }
}
