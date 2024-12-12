using Microsoft.AspNetCore.Mvc;

namespace PetSoLive.Web.Controllers;

public class AssistanceController : Controller
{
    public IActionResult Create() => View();
    public IActionResult List() => View();
}
