using GiaSuOnline.Web.Models;
using GiaSuOnline.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace GiaSuOnline.Web.Controllers;
public class HomeController(ProfileService profiles) : Controller
{
    public async Task<IActionResult> Index() => View(await profiles.Profiles.AsNoTracking().Where(x => x.Status == ProfileStatus.Approved).OrderByDescending(x => x.Id).Take(12).ToListAsync());
    [Route("Home/Status/{code:int}")] public IActionResult StatusPage(int code) { Response.StatusCode=code; return View("Status",code); }
    public IActionResult Error() { Response.StatusCode=500; return View("Status",500); }
}
