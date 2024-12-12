using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;

// Controllers (MVC pattern)
public class AccountController : Controller
{
    private readonly IUserService _userService;

    public AccountController(IUserService userService)
    {
        _userService = userService;
    }

    // Login page (GET)
    public IActionResult Login() => View();

    // Login (POST)
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        var user = await _userService.AuthenticateAsync(username, password);
        if (user != null)
        {
            // Store user information in session
            HttpContext.Session.SetString("Username", user.Username);
            // Set session info to ViewData for access in layout
            ViewData["Username"] = user.Username;

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Invalid username or password");
        return View();
    }

    // Register page (GET)
    public IActionResult Register() => View();

    // Register (POST)
    [HttpPost]
    public async Task<IActionResult> Register(string username, string email, string password)
    {
        if (ModelState.IsValid)
        {
            var user = new User { Username = username, Email = email, PasswordHash = password };
            await _userService.RegisterAsync(user);
            return RedirectToAction("Login");
        }
        return View();
    }

    // Logout
    public IActionResult Logout()
    {
        HttpContext.Session.Clear(); // Clears all session data
        return RedirectToAction("Login", "Account"); // Redirect to Login page
    }

}


