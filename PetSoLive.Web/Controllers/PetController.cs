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

        public PetController(IPetService petService, IUserService userService, IAdoptionService adoptionService, IAdoptionRequestRepository adoptionRequestRepository)
        {
            _petService = petService;
            _userService = userService;
            _adoptionService = adoptionService;
            _adoptionRequestRepository = adoptionRequestRepository;
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
            // Redirect to error page with an authentication message
            ViewBag.ErrorMessage = "You must be logged in to edit a pet.";
            return View("Error");
        }

        var user = await _userService.GetUserByUsernameAsync(username);
        var pet = await _petService.GetPetByIdAsync(id);

        if (pet == null)
        {
            // Pet not found, return error page
            ViewBag.ErrorMessage = "The pet you're trying to edit does not exist.";
            return View("Error");
        }

        if (!await _petService.IsUserOwnerOfPetAsync(id, user.Id))
        {
            // If the user is not the pet's owner, show an error
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
        var adoption = await _adoptionService.GetAdoptionByPetIdAsync(id);

        if (adoption != null)
        {
            ViewBag.ErrorMessage = "This pet has already been adopted and cannot be deleted.";
            return View("Error");
        }

        try
        {
            await _petService.DeletePetAsync(id, user.Id);
            return RedirectToAction("Index", "Adoption"); // Redirect to main page
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





    }
}

