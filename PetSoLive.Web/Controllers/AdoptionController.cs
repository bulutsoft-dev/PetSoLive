// /PetSoLive.Web/Controllers/AdoptionController.cs
using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;
using System.Threading.Tasks;
using System;
using PetSoLive.Core.Enums;

namespace PetSoLive.Web.Controllers
{
    public class AdoptionController : Controller
    
    {
        private readonly IAdoptionService _adoptionService;
        private readonly IPetService _petService;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IPetOwnerService _petOwnerService;

        public AdoptionController(IAdoptionService adoptionService, IPetService petService, IUserService userService, IEmailService emailService, IPetOwnerService petOwnerService)
        {
            _adoptionService = adoptionService;
            _petService = petService;
            _userService = userService;
            _emailService = emailService;
            _petOwnerService = petOwnerService;
        }

        /// <summary>
        /// Displays a list of all adoptions.
        /// </summary>
        /// <summary>
        /// Displays a list of available pets for adoption.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Fetch all pets available for adoption
            var pets = await _petService.GetAllPetsAsync();

            return View(pets);  // Pass the list of pets to the view
        }
       
        // Action to show the adoption request form
        [HttpGet]
        public async Task<IActionResult> Adopt(int petId)
        {
            var pet = await _petService.GetPetByIdAsync(petId);
            if (pet == null)
            {
                return NotFound();
            }

            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userService.GetUserByUsernameAsync(username);

            var adoptionRequest = new AdoptionRequest
            {
                PetId = petId,
                Pet = pet,
                UserId = user.Id,
                User = user
            };

            return View(adoptionRequest);
        }

        // Action to submit the adoption request form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAdoptRequest(AdoptionRequest adoptionRequest)
        {
            if (!ModelState.IsValid)
            {
                return View("Adopt", adoptionRequest);
            }

            // Get the pet owner
            var petOwner = await _petOwnerService.GetPetOwnerAsync(adoptionRequest.PetId);  // Call the PetOwnerService
            var petOwnerEmail = petOwner.User.Email;

            // Send an email to the pet owner
            await _emailService.SendEmailAsync(petOwnerEmail, "Adoption Request for " + adoptionRequest.Pet.Name, adoptionRequest.Message);

            // Redirect to a confirmation page
            return RedirectToAction("Index", "Adoption");
        }



    }
}