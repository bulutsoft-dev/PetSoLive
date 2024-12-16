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

        public PetController(IPetService petService)
        {
            _petService = petService;
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

        // POST: /Pet/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pet? pet)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                await _petService.CreatePetAsync(pet);

                // Redirect to AdoptionController.Index
                return RedirectToAction("Index", "Adoption");
            }

            // Return to the form with validation errors if any
            return View(pet);
        }

        // GET: /Pet/Details/{id}
        [AllowAnonymous] // Allow unauthenticated users to view pet details
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Fetch the pet by ID
            var pet = await _petService.GetPetByIdAsync(id);

            if (pet == null)
            {
                return NotFound(); // Return 404 if the pet is not found
            }

            return View(pet); // Pass the pet to the view
        }
    }
}

