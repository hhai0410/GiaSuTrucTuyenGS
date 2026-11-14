using GiaSuOnline.Web.Models;
namespace GiaSuOnline.Web.Services;
public static class VietnamTime
{
    public static DateTime Local(DateTime utc) => DateTime.SpecifyKind(utc, DateTimeKind.Utc).AddHours(7);
    public static DateTime Utc(DateTime local) => DateTime.SpecifyKind(local.AddHours(-7), DateTimeKind.Utc);
    public static DateOnly Today(DateTime utc) => DateOnly.FromDateTime(Local(utc));
    public static string Format(DateTime utc) => Local(utc).ToString("dd/MM/yyyy HH:mm") + " (UTC+7)";
    public static string Status(BookingStatus s) => s switch { BookingStatus.Pending => "Chờ xác nhận", BookingStatus.Confirmed => "Đã xác nhận", BookingStatus.Rejected => "Đã từ chối", _ => "Đã hết hạn" };
    public static string Language(string code) => code switch { "en"=>"Tiếng Anh", "fr"=>"Tiếng Pháp", "ja"=>"Tiếng Nhật", "ko"=>"Tiếng Hàn", "zh"=>"Tiếng Trung", _=>"Tiếng Việt" };
}
