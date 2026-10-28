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
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<ApplicationUser>().HasIndex(x => x.NormalizedEmail).IsUnique();
        b.Entity<TutorProfile>().HasIndex(x => x.UserId).IsUnique();
        b.Entity<TutorProfile>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        b.Entity<TutorProfile>().Property(x => x.HourlyRate).HasPrecision(18, 2);
        b.Entity<TutorProfile>().Property(x => x.Revision).IsConcurrencyToken();
        b.Entity<TutorSubject>().HasKey(x => new { x.TutorProfileId, x.SubjectId });
        b.Entity<TutorSubject>().HasOne(x => x.TutorProfile).WithMany(x => x.Subjects).HasForeignKey(x => x.TutorProfileId);
        b.Entity<TutorSubject>().HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<TutorProfile>().HasMany(x => x.Reviews).WithOne().HasForeignKey(x => x.TutorProfileId);
        b.Entity<TutorProfile>().HasMany(x => x.Availability).WithOne().HasForeignKey(x => x.TutorProfileId);
        b.Entity<Subject>().HasIndex(x => x.Name).IsUnique();
    }
}
