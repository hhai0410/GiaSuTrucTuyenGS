using System.ComponentModel.DataAnnotations;
using GiaSuOnline.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace GiaSuOnline.Web.ViewModels;
public class SearchForm : IValidatableObject
{
    [StringLength(100)] public string? Q { get; set; }
    public int? SubjectId { get; set; }
    [Range(0, 100000000)] public double? MinPrice { get; set; }
    [Range(0, 100000000)] public double? MaxPrice { get; set; }
    [Range(0, 5)] public double? MinRating { get; set; }
    [RegularExpression("^(vi|en|fr|ja|ko|zh)$")] public string? Language { get; set; }
    [RegularExpression("^(price|price_desc|rating)$")] public string Sort { get; set; } = "rating";
    [Range(1, 1000000)] public int Page { get; set; } = 1;
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice > MaxPrice)
            yield return new("Giá tối thiểu không được lớn hơn giá tối đa.", [nameof(MaxPrice)]);
    }
}
public record TutorCard(int Id, string Name, decimal Rate, int Experience, string Language, double? Rating, int ReviewCount, List<string> Subjects, bool HasPhoto);
public record SearchResults(SearchForm Filter, List<TutorCard> Items, List<SelectListItem> Subjects, int Total)
{
    public int Pages => (int)Math.Ceiling(Total / 10.0);
}
public class SlotForm
{
    public int? Id { get; set; }
    [Required, Display(Name="Bắt đầu (giờ Việt Nam)")] public DateTime StartsAt { get; set; }
    [Required, Display(Name="Kết thúc (giờ Việt Nam)")] public DateTime EndsAt { get; set; }
    public string? Revision { get; set; }
}
public record WeekSchedule(DateOnly Monday, TutorProfile? Profile, List<AvailabilitySlot> Slots);
public class BookForm
{
    public int SlotId { get; set; }
    [Required] public string SlotRevision { get; set; } = "";
    [Required] public string ProfileRevision { get; set; } = "";
}
public record BookingConfirmation(AvailabilitySlot Slot, decimal Amount);
public class DecisionForm
{
    public int Id { get; set; }
    [Required] public string Revision { get; set; } = "";
    public bool Accept { get; set; }
}
