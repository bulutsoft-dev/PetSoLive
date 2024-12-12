using Microsoft.AspNetCore.Mvc;

namespace PetSoLive.Web.Controllers;

public class AdoptionController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Details(int id) => View();
}
