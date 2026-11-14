using System.Security.Claims;
using GiaSuOnline.Web.Data;
using GiaSuOnline.Web.Services;
using GiaSuOnline.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace GiaSuOnline.Web.Controllers;
[Authorize(Roles="Tutor")]
public class AvailabilityController(ApplicationDbContext db, SchedulingService schedule, TimeProvider clock) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    [HttpGet] public async Task<IActionResult> Index(DateOnly? week)
    {
        if (!ModelState.IsValid) return BadRequest();
        var date = week ?? VietnamTime.Today(clock.GetUtcNow().UtcDateTime);
        if (date.Year is < 2000 or > 2100) return BadRequest();
        var monday = date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
        var start = VietnamTime.Utc(monday.ToDateTime(TimeOnly.MinValue)); var end = start.AddDays(7);
        var p = await db.TutorProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == UserId);
        var slots = p == null ? [] : await db.AvailabilitySlots.AsNoTracking().Where(x => x.TutorProfileId == p.Id && !x.IsDeleted && x.StartsAtUtc < end && x.EndsAtUtc > start).OrderBy(x => x.StartsAtUtc).ToListAsync();
        return View(new WeekSchedule(monday, p, slots));
    }
    [HttpGet] public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            var tomorrow = VietnamTime.Today(clock.GetUtcNow().UtcDateTime).AddDays(1).ToDateTime(new TimeOnly(9, 0));
            return View(new SlotForm { StartsAt = tomorrow, EndsAt = tomorrow.AddHours(1) });
        }
        var slot = await db.AvailabilitySlots.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.TutorProfile.UserId == UserId);
        if (slot == null) return NotFound();
        return View(new SlotForm { Id = slot.Id, StartsAt = VietnamTime.Local(slot.StartsAtUtc), EndsAt = VietnamTime.Local(slot.EndsAtUtc), Revision = slot.Revision });
    }
    [HttpPost] public async Task<IActionResult> Edit(SlotForm form)
    {
        if (!ModelState.IsValid) return View(form);
        try { await schedule.SaveSlotAsync(UserId, form); TempData["Success"]="Đã lưu khung giờ rảnh."; return RedirectToAction(nameof(Index), new { week = DateOnly.FromDateTime(form.StartsAt).ToString("yyyy-MM-dd") }); }
        catch (BookingRuleException e) { ModelState.AddModelError("", e.Message); return View(form); }
    }
    [HttpPost] public async Task<IActionResult> Delete(int id, string revision)
    {
        try { await schedule.DeleteSlotAsync(UserId, id, revision); TempData["Success"]="Đã xóa khung giờ."; }
        catch (BookingRuleException e) { TempData["Warning"]=e.Message; }
        return RedirectToAction(nameof(Index));
    }
    [HttpGet] public async Task<IActionResult> Events(DateTime start, DateTime end)
    {
        if (!ModelState.IsValid || end <= start || end - start > TimeSpan.FromDays(42)) return BadRequest();
        start = start.ToUniversalTime(); end = end.ToUniversalTime();
        var slots = await db.AvailabilitySlots.AsNoTracking().Where(x => !x.IsDeleted && x.TutorProfile.UserId == UserId && x.StartsAtUtc < end && x.EndsAtUtc > start).OrderBy(x => x.StartsAtUtc).ToListAsync();
        return Json(slots.Select(x => new { id=x.Id, title=x.IsBooked?"Đã đặt":"Còn trống", start=DateTime.SpecifyKind(x.StartsAtUtc,DateTimeKind.Utc).ToString("O"), end=DateTime.SpecifyKind(x.EndsAtUtc,DateTimeKind.Utc).ToString("O"), color=x.IsBooked?"#a34835":"#176b59", url=x.IsBooked ? null : Url.Action("Edit",new {id=x.Id}) }));
    }
}
