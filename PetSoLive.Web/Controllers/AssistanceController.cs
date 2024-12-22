using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;

namespace PetSoLive.Web.Controllers
{
    public class AssistanceController : Controller
    {
        private readonly IAssistanceService _assistanceService;

        public AssistanceController(IAssistanceService assistanceService)
        {
            _assistanceService = assistanceService;
        }

        public async Task<IActionResult> List()
        {
            var assistances = await _assistanceService.GetAllAssistancesAsync();
            return View(assistances);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Assistance assistance)
        {
            if (ModelState.IsValid)
            {
                await _assistanceService.CreateAssistanceAsync(assistance);
                return RedirectToAction("List");
            }
            return View(assistance);
        }
    }
}