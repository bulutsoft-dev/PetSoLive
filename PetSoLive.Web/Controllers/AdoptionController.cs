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
        private readonly IPetService _petService; // Inject IPetService

        public AdoptionController(IAdoptionService adoptionService, IPetService petService)
        {
            _adoptionService = adoptionService;
            _petService = petService; // Assign the injected pet service
        }   
        /// <summary>
        /// Displays the form for creating a new adoption.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            var model = new Adoption(); // Pass an empty model for the form.
            return View(model);
        }

        /// <summary>
        /// Handles the submission of the adoption creation form.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Adoption adoption)
        {
            if (!ModelState.IsValid)
            {
                return View(adoption);
            }

            // Set default values for the new adoption.
            adoption.AdoptionDate = DateTime.Now;
            adoption.Status = Core.Enums.AdoptionStatus.Pending;

            // Save the new adoption to the database.
            await _adoptionService.CreateAdoptionAsync(adoption);

            return RedirectToAction(nameof(Index));
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
       
        [HttpPost]
        public async Task<IActionResult> Adopt(int petId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized();
            }

            var pet = await _petService.GetPetByIdAsync(petId);
            if (pet == null)
            {
                return NotFound();
            }

            try
            {
                var adoption = new Adoption
                {
                    PetId = petId,
                    UserId = userId.Value,
                    AdoptionDate = DateTime.Now,
                    Status = AdoptionStatus.Pending
                };

                await _adoptionService.CreateAdoptionAsync(adoption);
                return RedirectToAction("Index", "Adoption");
            }
            catch (InvalidOperationException ex)
            {
                // Kullanıcıya hata mesajı göster
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }



    }
}