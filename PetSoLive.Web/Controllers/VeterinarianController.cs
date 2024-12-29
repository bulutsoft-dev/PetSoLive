using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Enums;

namespace PetSoLive.Web.Controllers
{
    public class VeterinarianController : Controller
    {
        private readonly IVeterinarianService _veterinarianService;
        private readonly IUserService _userService;
        private readonly IAdminService _adminService;

        public VeterinarianController(IVeterinarianService veterinarianService, IUserService userService, IAdminService adminService)
        {
            _veterinarianService = veterinarianService;
            _userService = userService;
            _adminService = adminService;
        }

        private async Task<IActionResult> CheckLoginAsync()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account"); // Kullanıcı giriş yapmamışsa Login sayfasına yönlendir
            }

            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // Kullanıcı bulunamadıysa Login sayfasına yönlendir
            }

            return null; // Kullanıcı giriş yapmışsa null döndür
        }

        public async Task<IActionResult> Register()
        {
            var loginCheckResult = await CheckLoginAsync();
            if (loginCheckResult != null) return loginCheckResult;

            var username = HttpContext.Session.GetString("Username");
            var user = await _userService.GetUserByUsernameAsync(username);
            
            var existingApplication = await _veterinarianService.GetByUserIdAsync(user.Id);
            if (existingApplication != null)
            {
                ViewBag.ApplicationSubmitted = true; // Başvuru yapılmış, formu gösterme
                return View();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(int userId, string qualifications, string clinicAddress, string clinicPhoneNumber)
        {
            var loginCheckResult = await CheckLoginAsync();
            if (loginCheckResult != null) return loginCheckResult;

            var username = HttpContext.Session.GetString("Username");
            var user = await _userService.GetUserByUsernameAsync(username);

            var existingApplication = await _veterinarianService.GetByUserIdAsync(user.Id);
            if (existingApplication != null)
            {
                ModelState.AddModelError("", "You have already submitted an application for veterinarian registration.");
                return View();
            }

            if (ModelState.IsValid)
            {
                await _veterinarianService.RegisterVeterinarianAsync(user.Id, qualifications, clinicAddress, clinicPhoneNumber);
                return RedirectToAction(nameof(Register)); 
            }

            return View();
        }

        public async Task<IActionResult> Index()
        {
            var loginCheckResult = await CheckLoginAsync();
            if (loginCheckResult != null) return loginCheckResult;

            var username = HttpContext.Session.GetString("Username");
            var user = await _userService.GetUserByUsernameAsync(username);

            var isAdmin = await _adminService.IsUserAdminAsync(user.Id);
            if (!isAdmin)
            {
                ViewBag.ErrorMessage = "You are not authorized to view this page.";
                return RedirectToAction("Error", "Home");
            }

            var veterinarians = await _veterinarianService.GetAllVeterinariansAsync();
            return View(veterinarians);
        }

        public async Task<IActionResult> Approve(int veterinarianId)
        {
            var loginCheckResult = await CheckLoginAsync();
            if (loginCheckResult != null) return loginCheckResult;

            var username = HttpContext.Session.GetString("Username");
            var user = await _userService.GetUserByUsernameAsync(username);

            var isAdmin = await _adminService.IsUserAdminAsync(user.Id);
            if (!isAdmin)
            {
                ViewBag.ErrorMessage = "You are not authorized to approve veterinarians.";
                return RedirectToAction("Error", "Home");
            }

            var veterinarian = await _veterinarianService.GetByIdAsync(veterinarianId);
            if (veterinarian == null)
            {
                ViewBag.ErrorMessage = "Veterinarian not found.";
                return RedirectToAction("Error", "Home");
            }

            if (veterinarian.Status == VeterinarianStatus.Pending)
            {
                await _veterinarianService.ApproveVeterinarianAsync(veterinarianId);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Reject(int veterinarianId)
        {
            var loginCheckResult = await CheckLoginAsync();
            if (loginCheckResult != null) return loginCheckResult;

            var username = HttpContext.Session.GetString("Username");
            var user = await _userService.GetUserByUsernameAsync(username);

            var isAdmin = await _adminService.IsUserAdminAsync(user.Id);
            if (!isAdmin)
            {
                ViewBag.ErrorMessage = "You are not authorized to reject veterinarians.";
                return RedirectToAction("Error", "Home");
            }

            var veterinarian = await _veterinarianService.GetByIdAsync(veterinarianId);
            if (veterinarian == null)
            {
                ViewBag.ErrorMessage = "Veterinarian not found.";
                return RedirectToAction("Error", "Home");
            }

            if (veterinarian.Status == VeterinarianStatus.Pending)
            {
                await _veterinarianService.RejectVeterinarianAsync(veterinarianId);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
