// /PetSoLive.Web/Controllers/PetController.cs
using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;
using System.Threading.Tasks;

namespace PetSoLive.Web.Controllers
{
    public class PetController : Controller
    {
        private readonly IRepository<Pet> _petRepository;

        public PetController(IRepository<Pet> petRepository)
        {
            _petRepository = petRepository;
        }

        /// <summary>
        /// Displays the form for adding a new pet.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Pet()); // Pass an empty Pet model for the form.
        }

        /// <summary>
        /// Handles the submission of the pet creation form.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pet pet)
        {
            if (!ModelState.IsValid)
            {
                return View(pet);
            }

            // Save the new pet to the database.
            await _petRepository.AddAsync(pet);

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays a list of all pets.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var pets = await _petRepository.GetAllAsync();
            return View(pets);
        }
    }
}