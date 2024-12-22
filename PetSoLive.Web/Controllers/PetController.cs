using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace PetSoLive.Web.Controllers
{
    public class PetController : Controller
    {
        private readonly IPetService _petService;
        private readonly IUserService _userService; // Add IUserService to access user data
        private readonly IAdoptionService _adoptionService;
        private readonly IAdoptionRequestRepository _adoptionRequestRepository;
        private readonly IEmailService _emailService;

        public PetController(IPetService petService, IUserService userService, IAdoptionService adoptionService, IAdoptionRequestRepository adoptionRequestRepository,IEmailService emailService)
        {
            _petService = petService;
            _userService = userService;
            _adoptionService = adoptionService;
            _adoptionRequestRepository = adoptionRequestRepository;
            _emailService = emailService;
        }

        // GET: /Pet/Create
        public IActionResult Create()
        {
            // Check if user session exists
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pet pet)
        {
            // Check if user session exists
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the user from session
            var user = await _userService.GetUserByUsernameAsync(username);

            // Check if 'IsNeutered' value is null when not checked
            if (pet.IsNeutered == null)
            {
                pet.IsNeutered = false; // or leave it as null, depending on your requirements
            }

            if (ModelState.IsValid)
            {
                // Create the pet first
                await _petService.CreatePetAsync(pet);

                // Now create the PetOwner relationship
                var petOwner = new PetOwner
                {
                    PetId = pet.Id,
                    UserId = user.Id,
                    OwnershipDate = DateTime.Now  // Track when the user became the owner
                };

                // Associate the pet with the user as an owner
                await _petService.AssignPetOwnerAsync(petOwner);

                // Redirect to AdoptionController.Index or another page
                return RedirectToAction("Index", "Adoption");
            }

            // Return to the form with validation errors if any
            return View(pet);
        }




        public async Task<IActionResult> Details(int id)
        {
            // Fetch the pet details using the provided id
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null)
            {
                // If pet is not found, return a 404 error
                return NotFound();
            }

            // Fetch related data
            var adoptionRequests = await _adoptionRequestRepository.GetAdoptionRequestsByPetIdAsync(id);
            var adoption = await _adoptionService.GetAdoptionByPetIdAsync(id);

            // Get the currently logged-in user's information
            var username = HttpContext.Session.GetString("Username");
            var isUserLoggedIn = username != null;
            var isOwner = false;
            bool hasAdoptionRequest = false;

            if (isUserLoggedIn)
            {
                var user = await _userService.GetUserByUsernameAsync(username);

                // Check if the logged-in user is the pet owner
                isOwner = await _petService.IsUserOwnerOfPetAsync(id, user.Id);

                // Check if the user has already submitted an adoption request for this pet
                hasAdoptionRequest = await _adoptionService.GetAdoptionRequestByUserAndPetAsync(user.Id, id) != null;
            }

            // If the pet has been adopted, prevent further adoption requests
            if (adoption != null)
            {
                ViewBag.AdoptionStatus = "This pet has already been adopted.";
            }
            else
            {
                ViewBag.AdoptionStatus = "This pet is available for adoption.";
            }

            // Pass all necessary data to the view
            ViewBag.IsUserLoggedIn = isUserLoggedIn;
            ViewBag.Adoption = adoption;
            ViewBag.IsOwner = isOwner;
            ViewBag.AdoptionRequests = adoptionRequests;
            ViewBag.HasAdoptionRequest = hasAdoptionRequest; // Pass the flag to indicate if the user already requested adoption

            // Return the pet details view
            return View(pet);
        }

        
        
        // GET: /Pet/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                // Redirect to error page with an authentication message
                ViewBag.ErrorMessage = "You must be logged in to edit a pet.";
                return View("Error");
            }

            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null)
            {
                // Pet not found, return error page
                ViewBag.ErrorMessage = "The pet you're trying to edit does not exist.";
                return View("Error");
            }

            var adoption = await _adoptionService.GetAdoptionByPetIdAsync(id);
            if (adoption != null)
            {
                // If adopted, prevent editing and show an error message
                ViewBag.ErrorMessage = "This pet has already been adopted and cannot be edited.";
                return View("Error");
            }

            var user = await _userService.GetUserByUsernameAsync(username);
            if (!await _petService.IsUserOwnerOfPetAsync(id, user.Id))
            {
                // If the user is not the pet's owner, show an error
                ViewBag.ErrorMessage = "You are not authorized to edit this pet.";
                return View("Error");
            }

            return View(pet); // If all checks pass, show the edit form
        }



   // POST: /Pet/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pet updatedPet)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                ViewBag.ErrorMessage = "You must be logged in to edit a pet.";
                return View("Error");
            }

            var user = await _userService.GetUserByUsernameAsync(username);
            var pet = await _petService.GetPetByIdAsync(id);

            if (pet == null)
            {
                ViewBag.ErrorMessage = "The pet you're trying to edit does not exist.";
                return View("Error");
            }

            if (!await _petService.IsUserOwnerOfPetAsync(id, user.Id))
            {
                ViewBag.ErrorMessage = "You are not authorized to edit this pet.";
                return View("Error");
            }

            // If IsNeutered is null, set it to false
            if (updatedPet.IsNeutered == null)
            {
                updatedPet.IsNeutered = false;
            }

            // Update the pet
            await _petService.UpdatePetAsync(id, updatedPet, user.Id);

// Fetch adoption requests for the updated pet
            var adoptionRequests = await _adoptionRequestRepository.GetAdoptionRequestsByPetIdAsync(id);
            foreach (var request in adoptionRequests)
            {
                var recipientEmail = request.User.Email;
                await SendPetUpdateEmailAsync(request.User, pet); // Call the method to send the email
            }

            // Redirect to the pet details page
            return RedirectToAction("Details", new { id = pet.Id });
        }

        // GET: /Pet/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                ViewBag.ErrorMessage = "You must be logged in to delete a pet.";
                return View("Error");
            }

            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null)
            {
                ViewBag.ErrorMessage = "The pet you're trying to delete does not exist.";
                return View("Error");
            }

            var adoption = await _adoptionService.GetAdoptionByPetIdAsync(id);
            if (adoption != null)
            {
                ViewBag.ErrorMessage = "This pet has already been adopted and cannot be deleted.";
                return View("Error");
            }

            var user = await _userService.GetUserByUsernameAsync(username);
            if (!await _petService.IsUserOwnerOfPetAsync(id, user.Id))
            {
                ViewBag.ErrorMessage = "You are not authorized to delete this pet.";
                return View("Error");
            }

            return View(pet); // Show confirmation page
        }
        // POST: /Pet/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                ViewBag.ErrorMessage = "You must be logged in to delete a pet.";
                return View("Error");
            }

            var user = await _userService.GetUserByUsernameAsync(username);
            var pet = await _petService.GetPetByIdAsync(id);
            var adoptionRequests = await _adoptionRequestRepository.GetAdoptionRequestsByPetIdAsync(id);

            if (pet == null)
            {
                ViewBag.ErrorMessage = "The pet you're trying to delete does not exist.";
                return View("Error");
            }

            // Fetch adoption information to prevent deletion of adopted pets
            var adoption = await _adoptionService.GetAdoptionByPetIdAsync(id);
            if (adoption != null)
            {
                ViewBag.ErrorMessage = "This pet has already been adopted and cannot be deleted.";
                return View("Error");
            }

            try
            {
                // Delete the pet
                await _petService.DeletePetAsync(id, user.Id);

                // Send email notifications to users who requested adoption for this pet
// Send email notifications to users who requested adoption for this pet
                foreach (var request in adoptionRequests)
                {
                    var recipientEmail = request.User.Email;
                    await SendPetDeletionEmailAsync(request.User, pet); // Call the method to send the email
                }


                // Redirect to the adoption index page after deletion
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
        
        
        
        
// Method to send email notifications about pet updates
        private async Task SendPetUpdateEmailAsync(User user, Pet pet)
        {
            var subject = "The pet you requested adoption for has been updated";
            var body = new EmailHelper().GeneratePetUpdateEmailBody(user, pet);  // Use EmailHelper to generate body
            await _emailService.SendEmailAsync(user.Email, subject, body);  // Send email with generated body
        }

// Method to send email notifications about pet deletions
        private async Task SendPetDeletionEmailAsync(User user, Pet pet)
        {
            var subject = "The pet you requested adoption for has been deleted";
            var body = new EmailHelper().GeneratePetDeletionEmailBody(user, pet);  // Use EmailHelper to generate body
            await _emailService.SendEmailAsync(user.Email, subject, body);  // Send email with generated body
        }


    }
}
