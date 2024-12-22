using Microsoft.AspNetCore.Mvc;

namespace PetSoLive.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult About() => View();
    }
}