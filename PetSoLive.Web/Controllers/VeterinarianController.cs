using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Interfaces;

namespace PetSoLive.Web.Controllers
{
    public class VeterinarianController : Controller
    {
        private readonly IServiceManager _serviceManager;
        private readonly IStringLocalizer<VeterinarianController> _localizer;

        public VeterinarianController(IServiceManager serviceManager, IStringLocalizer<VeterinarianController> localizer)
        {
            _serviceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        private async Task<User> GetLoggedInUserAsync()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username)) return null;
            return await _serviceManager.UserService.GetUserByUsernameAsync(username);
        }

        private IActionResult RedirectToLoginIfNotLoggedIn()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
            {
                return RedirectToAction("Login", "Account");
            }
            return null;
        }

        public async Task<IActionResult> Register()
        {
            var loginRedirect = RedirectToLoginIfNotLoggedIn();
            if (loginRedirect != null) return loginRedirect;

            var user = await GetLoggedInUserAsync();
            if (user == null)
            {
                TempData["ErrorMessage"] = _localizer["UserNotFound"].Value;
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserId = user.Id;

            var existingApplication = await _serviceManager.VeterinarianService.GetByUserIdAsync(user.Id);
            if (existingApplication != null)
            {
                ViewData["ApplicationSubmitted"] = true;
                return View();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(int userId, string qualifications, string clinicAddress, string clinicPhoneNumber)
        {
            var loginRedirect = RedirectToLoginIfNotLoggedIn();
            if (loginRedirect != null) return loginRedirect;

            var user = await GetLoggedInUserAsync();
            if (user == null)
            {
                TempData["ErrorMessage"] = _localizer["UserNotFound"].Value;
                return RedirectToAction("Login", "Account");
            }

            if (userId != user.Id)
            {
                TempData["ErrorMessage"] = _localizer["UnauthorizedAction"].Value;
                return RedirectToAction("Error", "Home");
            }

            var existingApplication = await _serviceManager.VeterinarianService.GetByUserIdAsync(user.Id);
            if (existingApplication != null)
            {
                ModelState.AddModelError("", _localizer["ExistingApplication"].Value);
                return View();
            }

            // Manual validation
            if (string.IsNullOrWhiteSpace(qualifications))
            {
                ModelState.AddModelError("qualifications", _localizer["QualificationsRequired"].Value);
            }
            else if (qualifications.Length > 500)
            {
                ModelState.AddModelError("qualifications", _localizer["QualificationsTooLong"].Value);
            }

            if (string.IsNullOrWhiteSpace(clinicAddress))
            {
                ModelState.AddModelError("clinicAddress", _localizer["ClinicAddressRequired"].Value);
            }
            else if (clinicAddress.Length > 200)
            {
                ModelState.AddModelError("clinicAddress", _localizer["ClinicAddressTooLong"].Value);
            }

            if (string.IsNullOrWhiteSpace(clinicPhoneNumber))
            {
                ModelState.AddModelError("clinicPhoneNumber", _localizer["ClinicPhoneRequired"].Value);
            }
            else if (clinicPhoneNumber.Length > 20)
            {
                ModelState.AddModelError("clinicPhoneNumber", _localizer["ClinicPhoneTooLong"].Value);
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(clinicPhoneNumber, @"^[\d\s\-+]+$"))
            {
                ModelState.AddModelError("clinicPhoneNumber", _localizer["InvalidPhoneFormat"].Value);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Store the returned Veterinarian if needed
                    var veterinarian = await _serviceManager.VeterinarianService.RegisterVeterinarianAsync(
                        userId,
                        qualifications,
                        clinicAddress,
                        clinicPhoneNumber
                    );
                    TempData["SuccessMessage"] = _localizer["ApplicationSubmitted"].Value;
                    return RedirectToAction(nameof(Register));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", _localizer["RegistrationFailed"].Value + " " + ex.Message);
                }
            }

            return View();
        }

        public async Task<IActionResult> Index()
        {
            var loginRedirect = RedirectToLoginIfNotLoggedIn();
            if (loginRedirect != null) return loginRedirect;

            var user = await GetLoggedInUserAsync();
            if (user == null)
            {
                TempData["ErrorMessage"] = _localizer["UserNotFound"].Value;
                return RedirectToAction("Login", "Account");
            }

            var isAdmin = await _serviceManager.AdminService.IsUserAdminAsync(user.Id);
            if (!isAdmin)
            {
                TempData["ErrorMessage"] = _localizer["NotAuthorized"].Value;
                return RedirectToAction("Error", "Home");
            }

            var veterinarians = await _serviceManager.VeterinarianService.GetAllVeterinariansAsync();
            return View(veterinarians);
        }

        public async Task<IActionResult> Approve(int veterinarianId)
        {
            var loginRedirect = RedirectToLoginIfNotLoggedIn();
            if (loginRedirect != null) return loginRedirect;

            var user = await GetLoggedInUserAsync();
            if (user == null)
            {
                TempData["ErrorMessage"] = _localizer["UserNotFound"].Value;
                return RedirectToAction("Login", "Account");
            }

            var isAdmin = await _serviceManager.AdminService.IsUserAdminAsync(user.Id);
            if (!isAdmin)
            {
                TempData["ErrorMessage"] = _localizer["NotAuthorizedApprove"].Value;
                return RedirectToAction("Error", "Home");
            }

            var veterinarian = await _serviceManager.VeterinarianService.GetByIdAsync(veterinarianId);
            if (veterinarian == null)
            {
                TempData["ErrorMessage"] = _localizer["VeterinarianNotFound"].Value;
                return RedirectToAction("Error", "Home");
            }

            if (veterinarian.Status == VeterinarianStatus.Pending)
            {
                await _serviceManager.VeterinarianService.ApproveVeterinarianAsync(veterinarianId);
                TempData["SuccessMessage"] = _localizer["VeterinarianApproved"].Value;
            }
            else
            {
                TempData["ErrorMessage"] = _localizer["InvalidVeterinarianStatus"].Value;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Reject(int veterinarianId)
        {
            var loginRedirect = RedirectToLoginIfNotLoggedIn();
            if (loginRedirect != null) return loginRedirect;

            var user = await GetLoggedInUserAsync();
            if (user == null)
            {
                TempData["ErrorMessage"] = _localizer["UserNotFound"].Value;
                return RedirectToAction("Login", "Account");
            }

            var isAdmin = await _serviceManager.AdminService.IsUserAdminAsync(user.Id);
            if (!isAdmin)
            {
                TempData["ErrorMessage"] = _localizer["NotAuthorizedReject"].Value;
                return RedirectToAction("Error", "Home");
            }

            var veterinarian = await _serviceManager.VeterinarianService.GetByIdAsync(veterinarianId);
            if (veterinarian == null)
            {
                TempData["ErrorMessage"] = _localizer["VeterinarianNotFound"].Value;
                return RedirectToAction("Error", "Home");
            }

            if (veterinarian.Status == VeterinarianStatus.Pending)
            {
                await _serviceManager.VeterinarianService.RejectVeterinarianAsync(veterinarianId);
                TempData["SuccessMessage"] = _localizer["VeterinarianRejected"].Value;
            }
            else
            {
                TempData["ErrorMessage"] = _localizer["InvalidVeterinarianStatus"].Value;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}