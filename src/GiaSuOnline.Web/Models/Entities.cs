using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace GiaSuOnline.Web.Models;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)] public string FullName { get; set; } = "";
}
public enum ProfileStatus { Pending, Approved, Rejected }
public class TutorProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    [MaxLength(3000)] public string Introduction { get; set; } = "";
    public decimal HourlyRate { get; set; }
    public int ExperienceYears { get; set; }
    public ProfileStatus Status { get; set; } = ProfileStatus.Pending;
    [MaxLength(1000)] public string? RejectionReason { get; set; }
    public string? PhotoKey { get; set; }
    public string? CertificateKey { get; set; }
    public string? PhotoContentType { get; set; }
    public string? CertificateContentType { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string Revision { get; set; } = Guid.NewGuid().ToString("N");
    public List<TutorSubject> Subjects { get; set; } = [];
    public List<TutorReview> Reviews { get; set; } = [];
    public List<AvailabilitySlot> Availability { get; set; } = [];
}
public class Subject
{
    public int Id { get; set; }
    [MaxLength(100)] public string Name { get; set; } = "";
}
public class TutorSubject
{
    public int TutorProfileId { get; set; }
    public TutorProfile TutorProfile { get; set; } = null!;
    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
}
// Chỉ đọc dữ liệu tóm tắt cho US06. Tạo lịch/đánh giá thuộc sprint sau.
public class TutorReview
{
    public int Id { get; set; }
    public int TutorProfileId { get; set; }
    public int Stars { get; set; }
    [MaxLength(500)] public string Comment { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
}
public class AvailabilitySlot
{
    public int Id { get; set; }
    public int TutorProfileId { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public bool IsBooked { get; set; }
}
