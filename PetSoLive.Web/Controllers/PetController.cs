using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;

namespace PetSoLive.Web.Controllers
{
    public class PetController : Controller
    {
        private readonly IPetService _petService;
        private readonly IUserService _userService;
        private readonly IAdoptionService _adoptionService;
        private readonly IAdoptionRequestRepository _adoptionRequestRepository;
        private readonly IEmailService _emailService;
        private readonly IStringLocalizer<LostPetAdController> _localizer;

        public PetController(
            IPetService petService, 
            IUserService userService, 
            IAdoptionService adoptionService, 
            IAdoptionRequestRepository adoptionRequestRepository, 
            IEmailService emailService,
            IStringLocalizer<LostPetAdController> localizer)
        {
            _petService = petService;
            _userService = userService;
            _adoptionService = adoptionService;
            _adoptionRequestRepository = adoptionRequestRepository;
            _emailService = emailService;
            _localizer = localizer;
        }

        private async Task<User> GetLoggedInUserAsync()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return null;
            return await _userService.GetUserByUsernameAsync(username);
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
                await _petService.CreatePetAsync(pet);

                var petOwner = new PetOwner
                {
                    PetId = pet.Id,
                    UserId = user.Id,
                    OwnershipDate = DateTime.Now
                };

                await _petService.AssignPetOwnerAsync(petOwner);

                return RedirectToAction("Index", "Adoption");
            }

            return View(pet);
        }

        public async Task<IActionResult> Details(int id)
        {
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null)
            {
                ViewBag.ErrorMessage = "Pet not found.";
                return View("Error");
            }

            var adoptionRequests = await _adoptionRequestRepository.GetAdoptionRequestsByPetIdAsync(id);
            var adoption = await _adoptionService.GetAdoptionByPetIdAsync(id);

            var user = await GetLoggedInUserAsync();
            var isUserLoggedIn = user != null;
            var isOwner = user != null && await _petService.IsUserOwnerOfPetAsync(id, user.Id);
            var hasAdoptionRequest = user != null && await _adoptionService.GetAdoptionRequestByUserAndPetAsync(user.Id, id) != null;

            ViewBag.AdoptionStatus = adoption != null 
                ? "This pet has already been adopted." 
                : "This pet is available for adoption.";

            ViewBag.IsUserLoggedIn = isUserLoggedIn;
            ViewBag.Adoption = adoption;
            ViewBag.IsOwner = isOwner;
            ViewBag.AdoptionRequests = adoptionRequests;
            ViewBag.HasAdoptionRequest = hasAdoptionRequest;

            return View(pet);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var loginRedirect = RedirectToLoginIfNotLoggedIn();
            if (loginRedirect != null) return loginRedirect;

            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null)
            {
                ViewBag.ErrorMessage = "Pet not found.";
                return View("Error");
            }

            var adoption = await _adoptionService.GetAdoptionByPetIdAsync(id);
            if (adoption != null)
            {
                ViewBag.ErrorMessage = "This pet has already been adopted and cannot be edited.";
                return View("Error");
            }

            var user = await GetLoggedInUserAsync();
            if (!await _petService.IsUserOwnerOfPetAsync(id, user.Id))
            {
                ViewBag.ErrorMessage = "You are not authorized to edit this pet.";
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
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null)
            {
                ViewBag.ErrorMessage = "Pet not found.";
                return View("Error");
            }

            if (!await _petService.IsUserOwnerOfPetAsync(id, user.Id))
            {
                ViewBag.ErrorMessage = "You are not authorized to edit this pet.";
                return View("Error");
            }

            if (updatedPet.IsNeutered == null)
            {
                updatedPet.IsNeutered = false;
            }

            await _petService.UpdatePetAsync(id, updatedPet, user.Id);

            var adoptionRequests = await _adoptionRequestRepository.GetAdoptionRequestsByPetIdAsync(id);
            foreach (var request in adoptionRequests)
            {
                await SendPetUpdateEmailAsync(request.User, pet);
            }

            return RedirectToAction("Details", new { id = pet.Id });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var loginRedirect = RedirectToLoginIfNotLoggedIn();
            if (loginRedirect != null) return loginRedirect;

            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null)
            {
                ViewBag.ErrorMessage = "Pet not found.";
                return View("Error");
            }

            var adoption = await _adoptionService.GetAdoptionByPetIdAsync(id);
            if (adoption != null)
            {
                ViewBag.ErrorMessage = "This pet has already been adopted and cannot be deleted.";
                return View("Error");
            }

            var user = await GetLoggedInUserAsync();
            if (!await _petService.IsUserOwnerOfPetAsync(id, user.Id))
            {
                ViewBag.ErrorMessage = "You are not authorized to delete this pet.";
                return View("Error");
            }

            return View(pet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loginRedirect = RedirectToLoginIfNotLoggedIn();
            if (loginRedirect != null) return loginRedirect;

            var user = await GetLoggedInUserAsync();
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null)
            {
                ViewBag.ErrorMessage = "Pet not found.";
                return View("Error");
            }

            var adoptionRequests = await _adoptionRequestRepository.GetAdoptionRequestsByPetIdAsync(id);
            var adoption = await _adoptionService.GetAdoptionByPetIdAsync(id);
            if (adoption != null)
            {
                ViewBag.ErrorMessage = "This pet has already been adopted and cannot be deleted.";
                return View("Error");
            }

            try
            {
                await _petService.DeletePetAsync(id, user.Id);

                foreach (var request in adoptionRequests)
                {
                    await SendPetDeletionEmailAsync(request.User, pet);
                }

                return RedirectToAction("Index", "Adoption");
            }
            catch (UnauthorizedAccessException)
            {
                ViewBag.ErrorMessage = "You are not authorized to delete this pet.";
                return View("Error");
            }
            catch (KeyNotFoundException)
            {
                ViewBag.ErrorMessage = "The pet you're trying to delete does not exist.";
                return View("Error");
            }
        }

        private async Task SendPetUpdateEmailAsync(User user, Pet pet)
        {
            var subject = "The pet you requested adoption for has been updated";
            var body = new EmailHelper().GeneratePetUpdateEmailBody(user, pet);
            await _emailService.SendEmailAsync(user.Email, subject, body);
        }

        private async Task SendPetDeletionEmailAsync(User user, Pet pet)
        {
            var subject = "The pet you requested adoption for has been deleted";
            var body = new EmailHelper().GeneratePetDeletionEmailBody(user, pet);
            await _emailService.SendEmailAsync(user.Email, subject, body);
        }
    }
}
