using System.Security.Claims;
using GiaSuOnline.Web.Data;
using GiaSuOnline.Web.Models;
using GiaSuOnline.Web.Services;
using GiaSuOnline.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace GiaSuOnline.Web.Controllers;
[Authorize]
public class BookingsController(ApplicationDbContext db, SchedulingService schedule, TimeProvider clock) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    [HttpGet, Authorize(Roles="Student")] public async Task<IActionResult> Confirm(int slotId)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var slot = await db.AvailabilitySlots.AsNoTracking().Include(x => x.TutorProfile).ThenInclude(x => x.User)
            .SingleOrDefaultAsync(x => x.Id == slotId && !x.IsDeleted && !x.IsBooked && x.StartsAtUtc > now && x.TutorProfile.Status == ProfileStatus.Approved);
        return slot == null ? NotFound() : View(new BookingConfirmation(slot, SchedulingService.Price(slot)));
    }
    [HttpPost, Authorize(Roles="Student")] public async Task<IActionResult> Create(BookForm form)
    {
        if (!ModelState.IsValid) return BadRequest();
        try { var id = await schedule.BookAsync(UserId, form); TempData["Success"]="Đã gửi yêu cầu đặt lịch. Gia sư sẽ xác nhận trước thời hạn."; return RedirectToAction(nameof(Details),new {id}); }
        catch (BookingRuleException e) { TempData["Warning"]=e.Message; return RedirectToAction(nameof(Mine)); }
        catch (DbUpdateConcurrencyException) { TempData["Warning"]="Khung giờ vừa thay đổi. Vui lòng chọn lại."; return RedirectToAction(nameof(Mine)); }
    }
    [HttpGet, Authorize(Roles="Student")] public async Task<IActionResult> Mine()
        => View(await db.Bookings.AsNoTracking().Include(x => x.TutorProfile).ThenInclude(x => x.User).Where(x => x.StudentId == UserId).OrderByDescending(x => x.CreatedAtUtc).ToListAsync());
    [HttpGet, Authorize(Roles="Tutor")] public async Task<IActionResult> Requests()
        => View(await db.Bookings.AsNoTracking().Include(x => x.Student).Where(x => x.TutorProfile.UserId == UserId).OrderByDescending(x => x.CreatedAtUtc).ToListAsync());
    [HttpGet] public async Task<IActionResult> Details(int id)
    {
        var booking = await db.Bookings.AsNoTracking().Include(x => x.TutorProfile).ThenInclude(x => x.User).Include(x => x.Student)
            .SingleOrDefaultAsync(x => x.Id == id && (x.StudentId == UserId || x.TutorProfile.UserId == UserId));
        return booking == null ? NotFound() : View(booking);
    }
    [HttpPost, Authorize(Roles="Tutor")] public async Task<IActionResult> Decide(DecisionForm form)
    {
        if (!ModelState.IsValid) return BadRequest();
        try { var status=await schedule.DecideAsync(UserId,form); TempData["Success"]=$"Yêu cầu: {VietnamTime.Status(status)}."; }
        catch (BookingRuleException e) { TempData["Warning"]=e.Message; }
        return RedirectToAction(nameof(Requests));
    }
}
