using Microsoft.AspNetCore.Mvc;
using PetSoLive.Business.Services;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;

public class LostPetAdController : Controller
{
    private readonly ILostPetAdService _lostPetAdService;
    private readonly INotificationService _notificationService;

    public LostPetAdController(ILostPetAdService lostPetAdService, 
        INotificationService notificationService)
    {
        _lostPetAdService = lostPetAdService;
        _notificationService = notificationService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(LostPetAd lostPetAd)
    {
        // Oturumda kullanıcı ID'sini kontrol et
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        lostPetAd.UserId = int.Parse(userId);  // Kullanıcı ID'sini kaydet
        await _lostPetAdService.CreateLostPetAdAsync(lostPetAd);

        await _notificationService.SendNotificationAsync("New Lost Pet Ad Created", 
            lostPetAd.Description, lostPetAd.LastSeenLocation);

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Index(string location)
    {
        var lostPetAds = await _lostPetAdService.GetLostPetAdsByLocationAsync(location);
        return View(lostPetAds);
    }
}