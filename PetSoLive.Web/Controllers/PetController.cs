using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;

namespace PetSoLive.Web.Controllers
{
    public class PetController : Controller
    {
        private readonly IServiceManager _serviceManager;
        private readonly IStringLocalizer<PetController> _localizer;

        public PetController(IServiceManager serviceManager, IStringLocalizer<PetController> localizer)
        {
            _serviceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        private async Task<User> GetLoggedInUserAsync()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return null;
            return await _serviceManager.UserService.GetUserByUsernameAsync(username);
        }

        private IActionResult RedirectToLoginIfNotLoggedIn()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return null;
        }

        public IActionResult Create()
        {
            var loginRedirect = RedirectToLoginIfNotLoggedIn();
            if (loginRedirect != null) return loginRedirect;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pet pet)
        {
            var loginRedirect = RedirectToLoginIfNotLoggedIn();
            if (loginRedirect != null) return loginRedirect;

            var user = await GetLoggedInUserAsync();

            if (pet.IsNeutered == null)
            {
                pet.IsNeutered = false;
            }

            if (ModelState.IsValid)
            {
                await _serviceManager.PetService.CreatePetAsync(pet);

                var petOwner = new PetOwner
                {
                    PetId = pet.Id,
                    UserId = user.Id,
                    OwnershipDate = DateTime.UtcNow
                };

                await _serviceManager.PetService.AssignPetOwnerAsync(petOwner);

                TempData["SuccessMessage"] = _localizer["PetCreatedSuccess"].Value;
                return RedirectToAction("Index", "Adoption");
            }

            TempData["ErrorMessage"] = _localizer["InvalidPetData"].Value;
            return View(pet);
        }

        public async Task<IActionResult> Details(int id)
        {
            Pet pet;
            try
            {
                pet = await _serviceManager.PetService.GetPetByIdAsync(id);
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = _localizer["PetNotFound"].Value;
                return View("Error");
            }

            var adoptionRequests = await _serviceManager.AdoptionRequestService.GetAdoptionRequestsByPetIdAsync(id);
            var adoption = await _serviceManager.AdoptionService.GetAdoptionByPetIdAsync(id);

            var user = await GetLoggedInUserAsync();
            var isUserLoggedIn = user != null;
            var isOwner = user != null && await _serviceManager.PetService.IsUserOwnerOfPetAsync(id, user.Id);
            var hasAdoptionRequest = user != null && await _serviceManager.AdoptionService.GetAdoptionRequestByUserAndPetAsync(user.Id, id) != null;

            ViewData["AdoptionStatus"] = adoption != null
                ? _localizer["PetAdopted"].Value
                : _localizer["PetAvailable"].Value;

            ViewData["IsUserLoggedIn"] = isUserLoggedIn;
            ViewData["Adoption"] = adoption;
            ViewData["IsOwner"] = isOwner;
            ViewData["AdoptionRequests"] = adoptionRequests;
            ViewData["HasAdoptionRequest"] = hasAdoptionRequest;

            return View(pet);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var loginRedirect = RedirectToLoginIfNotLoggedIn();
            if (loginRedirect != null) return loginRedirect;

            Pet pet;
            try
            {
                pet = await _serviceManager.PetService.GetPetByIdAsync(id);
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = _localizer["PetNotFound"].Value;
                return View("Error");
            }

            var adoption = await _serviceManager.AdoptionService.GetAdoptionByPetIdAsync(id);
            if (adoption != null)
            {
                TempData["ErrorMessage"] = _localizer["PetAdoptedCannotEdit"].Value;
                return View("Error");
            }

            var user = await GetLoggedInUserAsync();
            if (!await _serviceManager.PetService.IsUserOwnerOfPetAsync(id, user.Id))
            {
                TempData["ErrorMessage"] = _localizer["NotAuthorizedToEdit"].Value;
                return View("Error");
            }

            return View(pet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pet updatedPet)
        {
            var loginRedirect = RedirectToLoginIfNotLoggedIn();
            if (loginRedirect != null) return loginRedirect;

            var user = await GetLoggedInUserAsync();
            Pet pet;
            try
            {
                pet = await _serviceManager.PetService.GetPetByIdAsync(id);
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = _localizer["PetNotFound"].Value;
                return View("Error");
            }

            if (!await _serviceManager.PetService.IsUserOwnerOfPetAsync(id, user.Id))
            {
                TempData["ErrorMessage"] = _localizer["NotAuthorizedToEdit"].Value;
                return View("Error");
            }

            if (updatedPet.IsNeutered == null)
            {
                updatedPet.IsNeutered = false;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _serviceManager.PetService.UpdatePetAsync(id, updatedPet, user.Id);

                    var adoptionRequests = await _serviceManager.AdoptionRequestService.GetAdoptionRequestsByPetIdAsync(id);
                    foreach (var request in adoptionRequests)
                    {
                        await SendPetUpdateEmailAsync(request.User, pet);
                    }

                    TempData["SuccessMessage"] = _localizer["PetUpdatedSuccess"].Value;
                    return RedirectToAction("Details", new { id });
                }
                catch (UnauthorizedAccessException)
                {
                    TempData["ErrorMessage"] = _localizer["NotAuthorizedToEdit"].Value;
                    return View("Error");
                }
                catch (KeyNotFoundException)
                {
                    TempData["ErrorMessage"] = _localizer["PetNotFound"].Value;
                    return View("Error");
                }
            }

            TempData["ErrorMessage"] = _localizer["InvalidPetData"].Value;
            return View(updatedPet);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var loginRedirect = RedirectToLoginIfNotLoggedIn();
            if (loginRedirect != null) return loginRedirect;

            Pet pet;
            try
            {
                pet = await _serviceManager.PetService.GetPetByIdAsync(id);
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = _localizer["PetNotFound"].Value;
                return View("Error");
            }

            var adoption = await _serviceManager.AdoptionService.GetAdoptionByPetIdAsync(id);
            if (adoption != null)
            {
                TempData["ErrorMessage"] = _localizer["PetAdoptedCannotDelete"].Value;
                return View("Error");
            }

            var user = await GetLoggedInUserAsync();
            if (!await _serviceManager.PetService.IsUserOwnerOfPetAsync(id, user.Id))
            {
                TempData["ErrorMessage"] = _localizer["NotAuthorizedToDelete"].Value;
                return View("Error");
            }

            return View(pet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loginRedirect = RedirectToLoginIfNotLoggedIn();
            if (loginRedirect != null) return loginRedirect;

            var user = await GetLoggedInUserAsync();
            Pet pet;
            try
            {
                pet = await _serviceManager.PetService.GetPetByIdAsync(id);
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = _localizer["PetNotFound"].Value;
                return View("Error");
            }

            var adoption = await _serviceManager.AdoptionService.GetAdoptionByPetIdAsync(id);
            if (adoption != null)
            {
                TempData["ErrorMessage"] = _localizer["PetAdoptedCannotDelete"].Value;
                return View("Error");
            }

            try
            {
                var adoptionRequests = await _serviceManager.AdoptionRequestService.GetAdoptionRequestsByPetIdAsync(id);
                await _serviceManager.PetService.DeletePetAsync(id, user.Id);

                foreach (var request in adoptionRequests)
                {
                    await SendPetDeletionEmailAsync(request.User, pet);
                }

                TempData["SuccessMessage"] = _localizer["PetDeletedSuccess"].Value;
                return RedirectToAction("Index", "Adoption");
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = _localizer["NotAuthorizedToDelete"].Value;
                return View("Error");
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = _localizer["PetNotFound"].Value;
                return View("Error");
            }
        }

        private async Task SendPetUpdateEmailAsync(User user, Pet pet)
        {
            var subject = _localizer["PetUpdateEmailSubject"].Value;
            var body = new EmailHelper().GeneratePetUpdateEmailBody(user, pet);
            await _serviceManager.EmailService.SendEmailAsync(user.Email, subject, body);
        }

        private async Task SendPetDeletionEmailAsync(User user, Pet pet)
        {
            var subject = _localizer["PetDeletionEmailSubject"].Value;
            var body = new EmailHelper().GeneratePetDeletionEmailBody(user, pet);
            await _serviceManager.EmailService.SendEmailAsync(user.Email, subject, body);
        }
    }
}