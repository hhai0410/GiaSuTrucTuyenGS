using GiaSuOnline.Web.Data;
using GiaSuOnline.Web.Models;
using GiaSuOnline.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace GiaSuOnline.Web.Services;
public class BookingRuleException(string message) : Exception(message);
public class SchedulingService(ApplicationDbContext db, TimeProvider clock)
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;
    // Mọi thay đổi lịch/booking lấy cùng một khóa ghi trên hồ sơ trong transaction.
    // SQL Server giữ khóa này đến commit; không dựa vào khóa trong bộ nhớ của một web server.
    private async Task LockAsync(int profileId)
    {
        if (await db.TutorProfiles.Where(x => x.Id == profileId).ExecuteUpdateAsync(s => s.SetProperty(x => x.ScheduleRevision, x => x.ScheduleRevision + 1)) != 1)
            throw new BookingRuleException("Không tìm thấy gia sư.");
    }
    private async Task<TutorProfile> OwnProfile(string userId)
        => await db.TutorProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId)
            ?? throw new BookingRuleException("Bạn cần tạo hồ sơ gia sư trước.");
    public async Task<int> SaveSlotAsync(string owner, SlotForm form)
    {
        var start = VietnamTime.Utc(form.StartsAt); var end = VietnamTime.Utc(form.EndsAt);
        if (start <= Now || end <= start) throw new BookingRuleException("Khung giờ phải ở tương lai và kết thúc sau lúc bắt đầu.");
        var profile = await OwnProfile(owner);
        await using var tx = await db.Database.BeginTransactionAsync();
        await LockAsync(profile.Id);
        if (start <= Now) throw new BookingRuleException("Khung giờ đã qua trong lúc xử lý. Vui lòng chọn lại.");
        if (!await db.TutorProfiles.AnyAsync(x => x.Id == profile.Id && x.Status == ProfileStatus.Approved))
            throw new BookingRuleException("Chỉ gia sư đã duyệt mới được thiết lập lịch rảnh.");
        await ExpireLockedAsync(profile.Id);
        AvailabilitySlot slot;
        if (form.Id.HasValue)
        {
            slot = await db.AvailabilitySlots.SingleOrDefaultAsync(x => x.Id == form.Id && x.TutorProfileId == profile.Id && !x.IsDeleted)
                ?? throw new BookingRuleException("Không tìm thấy khung giờ của bạn.");
            if (slot.IsBooked) throw new BookingRuleException("Không được sửa khung giờ đã có người đặt.");
            if (slot.Revision != form.Revision) throw new BookingRuleException("Khung giờ vừa thay đổi. Hãy tải lại trang.");
        }
        else { slot = new() { TutorProfileId = profile.Id }; db.AvailabilitySlots.Add(slot); }
        if (await db.AvailabilitySlots.AnyAsync(x => x.TutorProfileId == profile.Id && !x.IsDeleted && x.Id != slot.Id && x.StartsAtUtc < end && x.EndsAtUtc > start))
            throw new BookingRuleException("Khung giờ chồng lấn với lịch đã có.");
        slot.StartsAtUtc = start; slot.EndsAtUtc = end; slot.Revision = Guid.NewGuid().ToString("N");
        await db.SaveChangesAsync(); await tx.CommitAsync(); return slot.Id;
    }
    public async Task DeleteSlotAsync(string owner, int id, string revision)
    {
        var p = await OwnProfile(owner);
        await using var tx = await db.Database.BeginTransactionAsync(); await LockAsync(p.Id);
        await ExpireLockedAsync(p.Id);
        var slot = await db.AvailabilitySlots.SingleOrDefaultAsync(x => x.Id == id && x.TutorProfileId == p.Id && !x.IsDeleted)
            ?? throw new BookingRuleException("Không tìm thấy khung giờ của bạn.");
        if (slot.IsBooked) throw new BookingRuleException("Không được xóa khung giờ đã có người đặt.");
        if (slot.Revision != revision) throw new BookingRuleException("Khung giờ vừa thay đổi. Hãy tải lại trang.");
        // Xóa mềm để giữ lịch sử các yêu cầu từng bị từ chối/hết hạn.
        slot.IsDeleted = true; slot.Revision = Guid.NewGuid().ToString("N");
        await db.SaveChangesAsync(); await tx.CommitAsync();
    }
    public static decimal Price(AvailabilitySlot slot) => Math.Round(slot.TutorProfile.HourlyRate * (decimal)(slot.EndsAtUtc - slot.StartsAtUtc).TotalMinutes / 60, 2, MidpointRounding.AwayFromZero);
    public async Task<int> BookAsync(string studentId, BookForm form)
    {
        var profileId = await db.AvailabilitySlots.Where(x => x.Id == form.SlotId).Select(x => (int?)x.TutorProfileId).SingleOrDefaultAsync()
            ?? throw new BookingRuleException("Khung giờ không tồn tại.");
        await using var tx = await db.Database.BeginTransactionAsync(); await LockAsync(profileId);
        await ExpireLockedAsync(profileId);
        var slot = await db.AvailabilitySlots.Include(x => x.TutorProfile).ThenInclude(x => x.User).SingleAsync(x => x.Id == form.SlotId);
        if (slot.IsDeleted || slot.StartsAtUtc <= Now || slot.TutorProfile.Status != ProfileStatus.Approved)
            throw new BookingRuleException("Khung giờ không còn khả dụng.");
        if (slot.IsBooked || slot.Revision != form.SlotRevision)
            throw new BookingRuleException("Khung giờ vừa có người đặt hoặc đã thay đổi. Vui lòng chọn lại.");
        if (slot.TutorProfile.Revision != form.ProfileRevision)
            throw new BookingRuleException("Hồ sơ hoặc học phí vừa thay đổi. Vui lòng xem và xác nhận lại.");
        if (slot.TutorProfile.UserId == studentId) throw new BookingRuleException("Bạn không thể đặt lịch với chính mình.");
        var student = await db.Users.SingleOrDefaultAsync(x => x.Id == studentId)
            ?? throw new BookingRuleException("Tài khoản không tồn tại.");
        var booking = new Booking { SlotId = slot.Id, TutorProfileId = profileId, StudentId = studentId,
            StartsAtUtc = slot.StartsAtUtc, EndsAtUtc = slot.EndsAtUtc, HourlyRate = slot.TutorProfile.HourlyRate,
            Amount = Price(slot), CreatedAtUtc = Now, ExpiresAtUtc = Now.AddHours(24), Status = BookingStatus.Pending };
        slot.IsBooked = true; slot.Revision = Guid.NewGuid().ToString("N");
        db.Bookings.Add(booking); await db.SaveChangesAsync();
        Queue(slot.TutorProfile.User.Email!, "GS – Yêu cầu đặt lịch mới", $"Bạn có yêu cầu #{booking.Id} từ {student.FullName}.\nThời gian: {VietnamTime.Format(booking.StartsAtUtc)} – {VietnamTime.Format(booking.EndsAtUtc)}.\nVui lòng đăng nhập GS để chấp nhận hoặc từ chối trong 24 giờ và trước khi buổi học bắt đầu.");
        await db.SaveChangesAsync(); await tx.CommitAsync(); return booking.Id;
    }
    public async Task<BookingStatus> DecideAsync(string owner, DecisionForm form)
    {
        var p = await OwnProfile(owner);
        await using var tx = await db.Database.BeginTransactionAsync(); await LockAsync(p.Id);
        await ExpireLockedAsync(p.Id);
        var booking = await db.Bookings.Include(x => x.Slot).Include(x => x.Student).SingleOrDefaultAsync(x => x.Id == form.Id && x.TutorProfileId == p.Id)
            ?? throw new BookingRuleException("Không tìm thấy yêu cầu của bạn.");
        if (booking.Status == BookingStatus.Expired)
        { await db.SaveChangesAsync(); await tx.CommitAsync(); return booking.Status; }
        if (booking.Status != BookingStatus.Pending || booking.Revision != form.Revision)
            throw new BookingRuleException("Yêu cầu đã được xử lý hoặc vừa thay đổi. Hãy tải lại danh sách.");
        booking.Status = form.Accept ? BookingStatus.Confirmed : BookingStatus.Rejected;
        booking.Revision = Guid.NewGuid().ToString("N");
        if (!form.Accept) { booking.Slot.IsBooked = false; booking.Slot.Revision = Guid.NewGuid().ToString("N"); }
        Queue(booking.Student.Email!, "GS – Kết quả yêu cầu đặt lịch", $"Yêu cầu #{booking.Id}: {VietnamTime.Status(booking.Status)}.\nThời gian: {VietnamTime.Format(booking.StartsAtUtc)}.\nVui lòng đăng nhập GS để xem chi tiết.");
        await db.SaveChangesAsync(); await tx.CommitAsync(); return booking.Status;
    }
    private void Queue(string to, string subject, string body)
        => db.BookingEmails.Add(new() { Recipient = to, Subject = subject, Body = body, CreatedAtUtc = Now, NextAttemptAtUtc = Now });
    private async Task ExpireLockedAsync(int profileId)
    {
        var now = Now;
        var expired = await db.Bookings.Include(x => x.Slot).Include(x => x.Student)
            .Where(x => x.TutorProfileId == profileId && x.Status == BookingStatus.Pending && (x.ExpiresAtUtc <= now || x.StartsAtUtc <= now)).ToListAsync();
        foreach (var booking in expired)
        {
            booking.Status = BookingStatus.Expired; booking.Revision = Guid.NewGuid().ToString("N");
            booking.Slot.IsBooked = false; booking.Slot.Revision = Guid.NewGuid().ToString("N");
            Queue(booking.Student.Email!, "GS – Yêu cầu đặt lịch hết hạn", $"Yêu cầu #{booking.Id} đã hết hạn do không được xác nhận trước thời hạn hoặc giờ học. Khung giờ được giải phóng; bạn có thể chọn khung giờ tương lai khác.");
        }
        await db.SaveChangesAsync();
    }
    public async Task ExpireAsync()
    {
        var now = Now;
        var ids = await db.Bookings.Where(x => x.Status == BookingStatus.Pending && (x.ExpiresAtUtc <= now || x.StartsAtUtc <= now))
            .Select(x => x.TutorProfileId).Distinct().ToListAsync();
        foreach (var id in ids)
        {
            await using var tx = await db.Database.BeginTransactionAsync(); await LockAsync(id);
            await ExpireLockedAsync(id); await tx.CommitAsync();
        }
    }
}
