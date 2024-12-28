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
        await _helpRequestService.CreateHelpRequestAsync(helpRequest);

        if (helpRequest.EmergencyLevel == EmergencyLevel.High)
        {
            await _notificationService.SendEmergencyNotificationAsync("High urgency help request", helpRequest.Description);
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Oturumda kullanıcı ID'sini kontrol et
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var helpRequests = await _helpRequestService.GetHelpRequestsByUserAsync(int.Parse(userId));
        return View(helpRequests);
    }
}