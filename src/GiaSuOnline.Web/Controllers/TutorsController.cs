using GiaSuOnline.Web.Services;
using GiaSuOnline.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
namespace GiaSuOnline.Web.Controllers;
public class TutorsController(TutorSearchService search) : Controller
{
    [HttpGet] public async Task<IActionResult> Index(SearchForm form)
    {
        if (!ModelState.IsValid) { Response.StatusCode = 400; return View(new SearchResults(form, [], await search.SubjectsAsync(), 0)); }
        return View(await search.SearchAsync(form));
    }
    [HttpGet] public async Task<IActionResult> Suggestions(string? q) => Json(await search.SuggestAsync(q));
}
