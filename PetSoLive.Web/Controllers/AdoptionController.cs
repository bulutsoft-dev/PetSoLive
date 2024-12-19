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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePet(Pet? pet)
        {
            if (ModelState.IsValid)
            {
                // You can handle image uploading here, if needed
                await _petService.CreatePetAsync(pet); // This will depend on your service layer
                return RedirectToAction(nameof(Index)); // Redirect to the list of pets or adoptions
            }

            return View(pet);
        }
        
        
        
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            await _adoptionService.UpdateAdoptionStatusAsync(id, AdoptionStatus.Approved);
            return RedirectToAction(nameof(Index));
        }
        
        public IActionResult Adopt(int id)
        {
            // Kullanıcının giriş yapıp yapmadığını kontrol et
            if (HttpContext.Session.GetString("Username") == null)
            {
                // Giriş yapılmamışsa, giriş sayfasına yönlendir
                return RedirectToAction("Login", "Account");
            }
            var username = HttpContext.Session.GetString("Username");

            // ViewBag veya ViewData kullanarak bu değeri view'a iletebilirsiniz
            ViewBag.IsUserLoggedIn = !string.IsNullOrEmpty(username);
            

            // Evcil hayvanı evlat edinme işlemini gerçekleştirin.
            // Örneğin: Evlat edinme durumunu güncelleyin ve kullanıcıya atayın.
            // (Burada evlat edinme işlemini gerçekleştirecek kodu yazmalısınız)

            return RedirectToAction("Index"); // İşlem sonrası listeye yönlendirme
        }



    }
}