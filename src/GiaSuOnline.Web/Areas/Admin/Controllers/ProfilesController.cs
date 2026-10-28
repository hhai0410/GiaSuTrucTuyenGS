using GiaSuOnline.Web.Data;
using GiaSuOnline.Web.Models;
using GiaSuOnline.Web.Services;
using GiaSuOnline.Web.ViewModels;
using GiaSuOnline.Web.Integrations.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace GiaSuOnline.Web.Areas.Admin.Controllers;
[Area("Admin"), Authorize(Roles="Admin")]
public class ProfilesController(ProfileService service, ApplicationDbContext db, IEmailSender email, ILogger<ProfilesController> logger) : Controller
{
    [HttpGet] public async Task<IActionResult> Index(ProfileStatus? status)
    {
        ViewBag.Status = status ?? ProfileStatus.Pending;
        return View(await service.Profiles.AsNoTracking().Where(x => x.Status == (status ?? ProfileStatus.Pending)).OrderByDescending(x => x.Id).ToListAsync());
    }
    [HttpGet] public async Task<IActionResult> Details(int id)
    { var p=await service.Profiles.AsNoTracking().SingleOrDefaultAsync(x => x.Id==id); return p == null ? NotFound() : View(p); }
    [HttpPost] public async Task<IActionResult> Moderate(ModerationForm form)
    {
        if (!form.Approve && string.IsNullOrWhiteSpace(form.Reason)) ModelState.AddModelError("Reason", "Từ chối bắt buộc có lý do.");
        var p=await service.Profiles.SingleOrDefaultAsync(x => x.Id==form.Id);
        if (p == null) return NotFound();
        if (!ModelState.IsValid) { TempData["Warning"]="Lý do từ chối cần có nội dung và không quá 1000 ký tự."; return RedirectToAction(nameof(Details),new { id=form.Id }); }
        if (p.Status != ProfileStatus.Pending || p.Revision != form.Revision) return Conflict("Hồ sơ đã thay đổi. Hãy tải lại trước khi duyệt.");
        p.Status=form.Approve ? ProfileStatus.Approved : ProfileStatus.Rejected; p.RejectionReason=form.Approve ? null : form.Reason!.Trim();
        p.Revision=Guid.NewGuid().ToString("N");
        try { await db.SaveChangesAsync(); } catch (DbUpdateConcurrencyException) { return Conflict("Hồ sơ đã thay đổi. Hãy tải lại."); }
        try { await email.SendAsync(p.User.Email!, "GS – Kết quả kiểm duyệt hồ sơ", form.Approve ? "Hồ sơ của bạn đã được duyệt và hiển thị công khai." : "Hồ sơ bị từ chối. Lý do: " + p.RejectionReason); }
        catch (Exception ex) { logger.LogError("Email kiểm duyệt chưa gửi được ({Type}).",ex.GetType().Name); TempData["Warning"]="Đã cập nhật trạng thái nhưng gửi email thất bại. Hãy kiểm tra SMTP và dùng nút gửi lại kết quả."; }
        TempData["Success"]="Đã cập nhật kết quả kiểm duyệt.";
        return RedirectToAction(nameof(Details),new { id=p.Id });
    }
    [HttpPost] public async Task<IActionResult> Resend(int id)
    {
        var p=await service.Profiles.AsNoTracking().SingleOrDefaultAsync(x => x.Id==id);
        if (p == null) return NotFound();
        if (p.Status == ProfileStatus.Pending) return BadRequest();
        try { await email.SendAsync(p.User.Email!,"GS – Kết quả kiểm duyệt hồ sơ",p.Status == ProfileStatus.Approved ? "Hồ sơ đã được duyệt." : "Hồ sơ bị từ chối. Lý do: " + p.RejectionReason); TempData["Success"]="Đã gửi email kết quả."; }
        catch { TempData["Warning"]="Chưa gửi được email. Vui lòng kiểm tra SMTP."; }
        return RedirectToAction(nameof(Details),new { id });
    }
}
