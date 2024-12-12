using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
// PetSoLive Solution Structure (Simplified Implementation)

// 1. Presentation Layer (/PetSoLive.Web)

// Controllers (MVC pattern)
namespace PetSoLive.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Check session for username
            var username = HttpContext.Session.GetString("Username");
            ViewData["Username"] = username;
            return View();
        }
        public IActionResult About() => View();
    }

    
}

// Views (HTML with Razor Syntax in /Views folders)
// - Home: Index.cshtml, About.cshtml
// - Account: Login.cshtml, Register.cshtml
// - Adoption: Index.cshtml, Details.cshtml
// - Assistance: Create.cshtml, List.cshtml

// Static files served from /wwwroot (CSS, JS, Images)
// Program.cs: Entry point to configure and run the ASP.NET application
// Startup.cs: Middleware configuration and service dependency injection
