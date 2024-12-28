using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Enums;

namespace PetSoLive.Web.Controllers
{
    public class VeterinarianController : Controller
    {
        private readonly IVeterinarianService _veterinarianService;
        private readonly IUserService _userService;  // Assuming you have a UserService for retrieving user data

        public VeterinarianController(IVeterinarianService veterinarianService, IUserService userService)
        {
            _veterinarianService = veterinarianService;
            _userService = userService;
        }

        // Register a new veterinarian - GET
        public async Task<IActionResult> Register()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account"); // Redirect to login page if user is not logged in
            }

            // Get user details
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }

            // Check if the user has already submitted an application
            var existingApplication = await _veterinarianService.GetByUserIdAsync(user.Id);
            if (existingApplication != null)
            {
                // If the user already applied, display message and disable form
                ViewBag.ApplicationSubmitted = true;
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
                return RedirectToAction("Login", "Account"); // Redirect to login page if user is not logged in
            }

            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }

            // Check if the user has already applied
            var existingApplication = await _veterinarianService.GetByUserIdAsync(user.Id);
            if (existingApplication != null)
            {
                // If application exists, do not allow another submission
                ModelState.AddModelError("", "You have already submitted an application for veterinarian registration.");
                return View();
            }

            if (ModelState.IsValid)
            {
                // Register veterinarian with Pending status
                await _veterinarianService.RegisterVeterinarianAsync(user.Id, qualifications, clinicAddress, clinicPhoneNumber);
                return RedirectToAction(nameof(Index)); // Redirect to list of veterinarians after registering
            }
            return View(); // Return the form in case of validation failure
        }

        // List all veterinarians (if needed)
        public async Task<IActionResult> Index()
        {
            var veterinarians = await _veterinarianService.GetAllVeterinariansAsync();
            return View(veterinarians);
        }

        // Approve veterinarian
        public async Task<IActionResult> Approve(int veterinarianId)
        {
            var veterinarian = await _veterinarianService.GetByIdAsync(veterinarianId);
            if (veterinarian == null)
            {
                return NotFound(); // If the veterinarian doesn't exist
            }

            if (veterinarian.Status == VeterinarianStatus.Pending)
            {
                await _veterinarianService.ApproveVeterinarianAsync(veterinarianId);
            }

            return RedirectToAction(nameof(Index));
        }

        // Reject veterinarian
        public async Task<IActionResult> Reject(int veterinarianId)
        {
            var veterinarian = await _veterinarianService.GetByIdAsync(veterinarianId);
            if (veterinarian == null)
            {
                return NotFound(); // If the veterinarian doesn't exist
            }

            if (veterinarian.Status == VeterinarianStatus.Pending)
            {
                await _veterinarianService.RejectVeterinarianAsync(veterinarianId);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
