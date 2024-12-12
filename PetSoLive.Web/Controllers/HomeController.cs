using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PetSoLive.Web.Models;

namespace PetSoLive.Web.Controllers;

public class HomeController : Controller
{
    private readonly IAdoptionService _adoptionService;
    public HomeController(IAdoptionService adoptionService)
    {
        _adoptionService = adoptionService;
    }
    public IActionResult Index()
    {
        var adoptions = _adoptionService.GetAllAdoptions();
        return View(adoptions);
    }
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}