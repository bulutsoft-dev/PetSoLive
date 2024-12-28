using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Entities;
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

        // Register a new veterinarian - GET (Normal kullanıcılar sadece bu sayfayı görebilir)
        public async Task<IActionResult> Register()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account"); // Kullanıcı giriş yapmamışsa, giriş sayfasına yönlendir
            }

            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }

            // Kullanıcı daha önce başvuru yapmışsa formu gösterme
            var existingApplication = await _veterinarianService.GetByUserIdAsync(user.Id);
            if (existingApplication != null)
            {
                ViewBag.ApplicationSubmitted = true; // Başvuru yapılmış, formu gösterme
                return View();
            }

            return View();
        }

        // Register a new veterinarian - POST
        [HttpPost]
        public async Task<IActionResult> Register(int userId, string qualifications, string clinicAddress, string clinicPhoneNumber)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }

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

        // List all veterinarians (Only admins can see this)
        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }

            // Check if the user is an admin
            var isAdmin = await _adminService.IsUserAdminAsync(user.Id);
            if (!isAdmin)
            {
                return Forbid(); // Admin olmayan kullanıcıları engelle
            }

            var veterinarians = await _veterinarianService.GetAllVeterinariansAsync();
            return View(veterinarians); // Admin olanlar için veteriner listesini göster
        }

        // Approve veterinarian (Only admins can approve)
        public async Task<IActionResult> Approve(int veterinarianId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }

            var isAdmin = await _adminService.IsUserAdminAsync(user.Id);
            if (!isAdmin)
            {
                return Forbid(); 
            }

            var veterinarian = await _veterinarianService.GetByIdAsync(veterinarianId);
            if (veterinarian == null)
            {
                return NotFound();
            }

            if (veterinarian.Status == VeterinarianStatus.Pending)
            {
                await _veterinarianService.ApproveVeterinarianAsync(veterinarianId);
            }

            return RedirectToAction(nameof(Index));
        }

        // Reject veterinarian (Only admins can reject)
        public async Task<IActionResult> Reject(int veterinarianId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }

            var isAdmin = await _adminService.IsUserAdminAsync(user.Id);
            if (!isAdmin)
            {
                return Forbid(); 
            }

            var veterinarian = await _veterinarianService.GetByIdAsync(veterinarianId);
            if (veterinarian == null)
            {
                return NotFound();
            }

            if (veterinarian.Status == VeterinarianStatus.Pending)
            {
                await _veterinarianService.RejectVeterinarianAsync(veterinarianId);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
