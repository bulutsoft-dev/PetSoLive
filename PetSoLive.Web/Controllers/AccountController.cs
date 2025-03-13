using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;

namespace PetSoLive.Web.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _userService;
            
    private readonly IStringLocalizer<AccountController> _localizer;


    public AccountController(IUserService userService,IStringLocalizer<AccountController> localizer)
    {
        _userService = userService;
        _localizer = localizer;
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "Username and password are required.");
            return View();
        }

        try
        {
            var user = await _userService.AuthenticateAsync(username, password);
            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetInt32("UserId", user.Id);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid username or password.");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
        }

        return View();
    }
    public IActionResult Register()
    {
        ViewData["Cities"] = CityList.Cities;
        ViewData["Districts"] = new List<string>();  // Initial empty list for districts
    
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string username, string email, string password, string phoneNumber, string address, DateTime dateOfBirth, string city, string district)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Cities"] = CityList.Cities;
            ViewData["Districts"] = CityList.GetDistrictsByCity(city);  // Populate districts based on selected city
            return View();
        }

        string profileImageUrl = "https://www.petsolive.com.tr/";

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = password,
            PhoneNumber = phoneNumber,
            Address = address,
            DateOfBirth = dateOfBirth,
            ProfileImageUrl = profileImageUrl,
            City = city,
            District = district
        };

        await _userService.RegisterAsync(user);
        return RedirectToAction("Login");
    }

    [HttpGet]
    public JsonResult GetDistricts(string city)
    {
        var districts = CityList.GetDistrictsByCity(city);
        return Json(districts);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}