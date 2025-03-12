using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace PetSoLive.Web.Controllers
{
    public class HomeController : Controller
    {
        
        private readonly IStringLocalizer<HomeController> _localizer;

        public HomeController(IStringLocalizer<HomeController> localizer)
        {
            _localizer = localizer;
        }
        public IActionResult Index()
        {
            ViewData["Title"] = _localizer["Home Page"];
            return View();
        }
        public IActionResult About() => View();
        public IActionResult Error()
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }
    }
}