using GiaSuOnline.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GiaSuOnline.Web.Data;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<TutorProfile> TutorProfiles => Set<TutorProfile>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<TutorSubject> TutorSubjects => Set<TutorSubject>();
    public DbSet<TutorReview> TutorReviews => Set<TutorReview>();
    public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingEmail> BookingEmails => Set<BookingEmail>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<ApplicationUser>().HasIndex(x => x.NormalizedEmail).IsUnique();
        b.Entity<TutorProfile>().HasIndex(x => x.UserId).IsUnique();
        b.Entity<TutorProfile>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        b.Entity<TutorProfile>().Property(x => x.HourlyRate).HasPrecision(18, 2);
        b.Entity<TutorProfile>().Property(x => x.TeachingLanguage).HasDefaultValue("vi");
        b.Entity<TutorProfile>().Property(x => x.Revision).IsConcurrencyToken();
        b.Entity<TutorSubject>().HasKey(x => new { x.TutorProfileId, x.SubjectId });
        b.Entity<TutorSubject>().HasOne(x => x.TutorProfile).WithMany(x => x.Subjects).HasForeignKey(x => x.TutorProfileId);
        b.Entity<TutorSubject>().HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<TutorProfile>().HasMany(x => x.Reviews).WithOne().HasForeignKey(x => x.TutorProfileId);
        b.Entity<TutorProfile>().HasMany(x => x.Availability).WithOne(x => x.TutorProfile).HasForeignKey(x => x.TutorProfileId);
        b.Entity<TutorProfile>().HasIndex(x => new { x.Status, x.TeachingLanguage, x.HourlyRate });
        b.Entity<TutorSubject>().HasIndex(x => new { x.SubjectId, x.TutorProfileId });
        b.Entity<TutorReview>().HasIndex(x => new { x.TutorProfileId, x.CreatedAtUtc });
        b.Entity<AvailabilitySlot>().HasIndex(x => new { x.TutorProfileId, x.StartsAtUtc, x.EndsAtUtc });
        b.Entity<AvailabilitySlot>().Property(x => x.Revision).IsConcurrencyToken();
        b.Entity<Booking>().Property(x => x.Revision).IsConcurrencyToken();
        b.Entity<Booking>().Property(x => x.HourlyRate).HasPrecision(18, 2);
        b.Entity<Booking>().Property(x => x.Amount).HasPrecision(18, 2);
        b.Entity<Booking>().HasOne(x => x.Slot).WithMany().HasForeignKey(x => x.SlotId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Booking>().HasOne(x => x.TutorProfile).WithMany().HasForeignKey(x => x.TutorProfileId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Booking>().HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Booking>().HasIndex(x => x.SlotId).IsUnique().HasFilter("[Status] IN (0, 1)");
        b.Entity<Booking>().HasIndex(x => new { x.Status, x.ExpiresAtUtc });
        b.Entity<Booking>().HasIndex(x => new { x.StudentId, x.CreatedAtUtc });
        b.Entity<BookingEmail>().HasIndex(x => new { x.SentAtUtc, x.NextAttemptAtUtc });
        b.Entity<Subject>().HasIndex(x => x.Name).IsUnique();
    }
}
