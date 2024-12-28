using Microsoft.AspNetCore.Mvc;
using PetSoLive.Business.Services;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;

public class HelpRequestController : Controller
{
    private readonly IHelpRequestService _helpRequestService;
    private readonly IUserService _userService;
    private readonly INotificationService _notificationService;

    public HelpRequestController(IHelpRequestService helpRequestService, 
        IUserService userService, 
        INotificationService notificationService)
    {
        _helpRequestService = helpRequestService;
        _userService = userService;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var username = HttpContext.Session.GetString("Username");  // Use "Username" session key
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Account");
        }

        // Fetch user details to show on form
        var user = await _userService.GetUserByUsernameAsync(username);  // Fetch user by username
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Create a new instance of HelpRequest to pass to the view
        var helpRequest = new HelpRequest();

        ViewBag.User = user;  // Pass the user details to the view

        return View(helpRequest);  // Pass the new instance of HelpRequest to the view
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HelpRequest helpRequest)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _userService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Set UserId and CreatedAt before saving
        helpRequest.UserId = user.Id;
        helpRequest.CreatedAt = DateTime.Now; // Ensure CreatedAt is assigned here

        if (ModelState.IsValid)
        {
            // Call service to save the help request
            await _helpRequestService.CreateHelpRequestAsync(helpRequest);

            // Send notifications if the emergency level is high
            if (helpRequest.EmergencyLevel == EmergencyLevel.High)
            {
                await _notificationService.SendEmergencyNotificationAsync("High urgency help request", helpRequest.Description);
            }

            // Redirect after successful submission
            return RedirectToAction("Index");
        }

        // If the model is not valid, return the form with validation errors
        return View(helpRequest);
    }



    // Show all help requests (Blog-like list view)
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var helpRequests = await _helpRequestService.GetHelpRequestsAsync();
        return View(helpRequests);
    }

    // Show a single help request in detail
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var helpRequest = await _helpRequestService.GetHelpRequestByIdAsync(id);
        if (helpRequest == null)
        {
            return NotFound();
        }
        return View(helpRequest);
    }
}
