using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System.Threading.Tasks;

namespace PetSoLive.Web.Controllers
{
    public class PetController : Controller
    {
        private readonly IPetService _petService;

        public PetController(IPetService petService)
        {
            _petService = petService;
        }

        // GET: /Pet/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Pet/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pet? pet)
        {
            if (ModelState.IsValid)
            {
                await _petService.CreatePetAsync(pet);
                return RedirectToAction("Create"); // Redirect to pet list or adoption page
            }
            return View(pet); // Return to the form with validation errors if any
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Fetch the pet by ID
            var pet = await _petService.GetPetByIdAsync(id);

            if (pet == null)
            {
                return NotFound();  // Return 404 if the pet is not found
            }

            return View(pet);  // Pass the pet to the view
        }
    }
}