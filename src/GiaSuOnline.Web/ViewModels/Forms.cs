using System.ComponentModel.DataAnnotations;
using GiaSuOnline.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace GiaSuOnline.Web.ViewModels;
public class RegisterForm
{
    [Required(ErrorMessage="Nhập họ tên."), StringLength(100), Display(Name="Họ tên")] public string FullName { get; set; } = "";
    [Required, EmailAddress, Display(Name="Email")] public string Email { get; set; } = "";
    [Required, StringLength(100, MinimumLength=8), DataType(DataType.Password), Display(Name="Mật khẩu (ít nhất 8 ký tự)")] public string Password { get; set; } = "";
    [Compare(nameof(Password)), DataType(DataType.Password), Display(Name="Nhập lại mật khẩu")] public string ConfirmPassword { get; set; } = "";
    [Required, RegularExpression("^(Student|Tutor)$"), Display(Name="Vai trò")] public string Role { get; set; } = "Student";
}
public class LoginForm
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password), Display(Name="Mật khẩu")] public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}
public class ForgotForm { [Required, EmailAddress] public string Email { get; set; } = ""; }
public class ResetForm
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Token { get; set; } = "";
    [Required, StringLength(100, MinimumLength=8), DataType(DataType.Password), Display(Name="Mật khẩu mới")] public string Password { get; set; } = "";
    [Compare(nameof(Password)), DataType(DataType.Password), Display(Name="Nhập lại mật khẩu")] public string ConfirmPassword { get; set; } = "";
}
public class ProfileForm
{
    [Required, StringLength(3000, MinimumLength=20), Display(Name="Giới thiệu (20–3000 ký tự)")] public string Introduction { get; set; } = "";
    [Range(typeof(decimal), "1", "100000000"), Display(Name="Học phí / giờ (VNĐ)")] public decimal HourlyRate { get; set; }
    [Range(0, 70), Display(Name="Số năm kinh nghiệm")] public int ExperienceYears { get; set; }
    [MinLength(1, ErrorMessage="Chọn ít nhất một môn dạy."), Display(Name="Môn dạy")] public int[] SubjectIds { get; set; } = [];
    [Display(Name="Ảnh đại diện (JPG/PNG, tối đa 5 MB)")] public IFormFile? Photo { get; set; }
    [Display(Name="Chứng chỉ (JPG/PNG/PDF, tối đa 5 MB)")] public IFormFile? Certificate { get; set; }
    public string? Revision { get; set; }
    public List<SelectListItem> SubjectOptions { get; set; } = [];
    public bool HasPhoto { get; set; }
    public bool HasCertificate { get; set; }
}
public class ModerationForm
{
    public int Id { get; set; }
    [Required] public string Revision { get; set; } = "";
    public bool Approve { get; set; }
    [StringLength(1000), Display(Name="Lý do từ chối")] public string? Reason { get; set; }
}
public record TutorDetail(TutorProfile Profile, List<TutorReview> Reviews, List<AvailabilitySlot> Slots);
