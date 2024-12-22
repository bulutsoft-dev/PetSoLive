using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;

public class AccountController : Controller
{
    private readonly IUserService _userService;

    public AccountController(IUserService userService)
    {
        _userService = userService;
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
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string username, string email, string password, string phoneNumber, string address, DateTime dateOfBirth, string profileImageUrl)
    {
        if (ModelState.IsValid)
        {
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = password,
                PhoneNumber = phoneNumber,
                Address = address,
                DateOfBirth = dateOfBirth,
                ProfileImageUrl = profileImageUrl
            };

            try
            {
                await _userService.RegisterAsync(user);
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }

        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
