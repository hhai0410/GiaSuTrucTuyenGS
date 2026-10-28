using GiaSuOnline.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace GiaSuOnline.Web.Data.Seed;
public static class DatabaseSeed
{
    public static async Task RunAsync(IServiceProvider services, IConfiguration config)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Student", "Tutor", "Admin" })
            if (!await roles.RoleExistsAsync(role)) Check(await roles.CreateAsync(new(role)));
        var db = services.GetRequiredService<ApplicationDbContext>();
        if (!await db.Subjects.AnyAsync())
        {
            db.Subjects.AddRange(new[] {"Toán", "Ngữ văn", "Tiếng Anh", "Vật lý", "Hóa học", "Tin học"}.Select(n => new Subject { Name = n }));
            await db.SaveChangesAsync();
        }
        var email = config["SeedAdmin:Email"]; var password = config["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);
        // Không tự nâng quyền tài khoản tự đăng ký có cùng email.
        if (user != null) return;
        user = new() { UserName = email, Email = email, EmailConfirmed = true, FullName = "Quản trị GS" };
        Check(await users.CreateAsync(user, password));
        Check(await users.AddToRoleAsync(user, "Admin"));
    }
    private static void Check(IdentityResult r) { if (!r.Succeeded) throw new InvalidOperationException(string.Join("; ",r.Errors.Select(x => x.Description))); }
}
