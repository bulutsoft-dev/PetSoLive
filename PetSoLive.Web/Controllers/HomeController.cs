using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.EntityFrameworkCore;
using PetSoLive.Data;

namespace PetSoLive.Web.Controllers
{
    public class HomeController : Controller
    {
        
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(IStringLocalizer<HomeController> localizer, ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _localizer = localizer;
            _logger = logger;
            _context = context;
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
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { 
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true
                }
            );

            return LocalRedirect(returnUrl);
        }

        /// <summary>
        /// Adoptions tablosunun Id sequence'ini sıfırlar. Sadece development/test için kullanın!
        /// </summary>
        [HttpPost]
        [Route("/fix-adoption-sequence")] // POST /fix-adoption-sequence
        public async Task<IActionResult> FixAdoptionIdSequence()
        {
            var maxId = await _context.Adoptions.MaxAsync(a => (int?)a.Id) ?? 0;
            var sql = $"SELECT setval('\"Adoptions_Id_seq\"', {maxId})";
            await _context.Database.ExecuteSqlRawAsync(sql);
            return Content($"Adoptions Id sequence sıfırlandı. maxId: {maxId}");
        }
    }
}