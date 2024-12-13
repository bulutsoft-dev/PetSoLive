using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;
using System.Threading.Tasks;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Entities;  // This should be included to access Adoption class

namespace PetSoLive.Web.Controllers
{
    public class AdoptionController : Controller
    {
        private readonly IAdoptionService _adoptionService;

        public AdoptionController(IAdoptionService adoptionService)
        {
            _adoptionService = adoptionService;
        }

        // Display the list of all adoptions (available pets)
        public async Task<IActionResult> Index()
        {
            var adoptions = await _adoptionService.GetAllAdoptionsAsync();
            return View(adoptions);  // Ensure you're passing the correct model (IEnumerable<Adoption>)
        }

        // Display the details of a specific adoption (single pet)
        public async Task<IActionResult> Details(int id)
        {
            var adoption = await _adoptionService.GetAdoptionByIdAsync(id);
            if (adoption == null)
            {
                return NotFound();  // If no adoption is found, return 404
            }
            return View(adoption);  // Pass the adoption model to the view
        }

        // Update the status of an adoption (e.g., Pending, Adopted)
        public async Task<IActionResult> UpdateAdoptionStatus(int id, AdoptionStatus status)
        {
            var adoption = await _adoptionService.GetAdoptionByIdAsync(id);  // Corrected to async method
            if (adoption == null)
            {
                return NotFound();  // If no adoption is found, return 404
            }

            // Update the status of the adoption
            adoption.Status = status;

            // Call the service to update the adoption in the database
            await _adoptionService.UpdateAdoptionAsync(adoption);  // Assuming UpdateAdoptionAsync is implemented in IAdoptionService

            // Redirect to the details page to see the updated status
            return RedirectToAction("Details", new { id = adoption.Id });
        }
    }
}
