// /PetSoLive.Web/Controllers/AdoptionController.cs
using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;
using System.Threading.Tasks;
using System;

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
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var adoptions = await _adoptionService.GetAllAdoptionsAsync();
            return View(adoptions);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePet(Pet pet)
        {
            if (ModelState.IsValid)
            {
                // You can handle image uploading here, if needed
                await _petService.CreatePetAsync(pet); // This will depend on your service layer
                return RedirectToAction(nameof(Index)); // Redirect to the list of pets or adoptions
            }

            return View(pet);
        }


    }
}