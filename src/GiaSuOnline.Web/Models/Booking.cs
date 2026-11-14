using System.ComponentModel.DataAnnotations;
namespace GiaSuOnline.Web.Models;
public enum BookingStatus { Pending, Confirmed, Rejected, Expired }
public class Booking
{
    public int Id { get; set; }
    public int SlotId { get; set; }
    public AvailabilitySlot Slot { get; set; } = null!;
    public int TutorProfileId { get; set; }
    public TutorProfile TutorProfile { get; set; } = null!;
    public string StudentId { get; set; } = "";
    public ApplicationUser Student { get; set; } = null!;
    public BookingStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal Amount { get; set; }
    [MaxLength(32)] public string Revision { get; set; } = Guid.NewGuid().ToString("N");
}
public class BookingEmail
{
    public long Id { get; set; }
    [MaxLength(256)] public string Recipient { get; set; } = "";
    [MaxLength(200)] public string Subject { get; set; } = "";
    [MaxLength(4000)] public string Body { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
    public DateTime NextAttemptAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public DateTime? LeaseUntilUtc { get; set; }
    [MaxLength(32)] public string? LeaseToken { get; set; }
    public int Attempts { get; set; }
    [MaxLength(100)] public string? LastError { get; set; }
}
