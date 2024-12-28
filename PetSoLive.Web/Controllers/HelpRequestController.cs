using Microsoft.AspNetCore.Mvc;
using PetSoLive.Business.Services;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;

public class HelpRequestController : Controller
{
    private readonly IHelpRequestService _helpRequestService;
    private readonly INotificationService _notificationService;

    public HelpRequestController(IHelpRequestService helpRequestService, 
        INotificationService notificationService)
    {
        _helpRequestService = helpRequestService;
        _notificationService = notificationService;
    }

    // Create a new Help Request
    [HttpPost]
    public async Task<IActionResult> Create(HelpRequest helpRequest)
    {
        // Oturumda kullanıcı ID'sini kontrol et
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        helpRequest.UserId = int.Parse(userId);  // Kullanıcı ID'sini kaydet
        helpRequest.CreatedAt = DateTime.Now; // Set the creation time
        await _helpRequestService.CreateHelpRequestAsync(helpRequest);

        // Send notification if emergency level is high
        if (helpRequest.EmergencyLevel == EmergencyLevel.High)
        {
            await _notificationService.SendEmergencyNotificationAsync("High urgency help request", helpRequest.Description);
        }

        return RedirectToAction("Index");
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
