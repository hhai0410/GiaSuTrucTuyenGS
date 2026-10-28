using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace GiaSuOnline.Web.Data;
public class DesignTimeFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var config=new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json",optional:true).AddEnvironmentVariables().Build();
        var cs=config.GetConnectionString("DefaultConnection") ?? "Server=LOQ\\SQLEXPRESS;Database=GiaSuOnlineGS;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=10";
        return new(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs).Options);
    }
}
