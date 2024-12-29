using Microsoft.AspNetCore.Mvc;
using PetSoLive.Business.Services;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

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

    // Kayıp ilanı oluşturulması için gerekli olan POST metodu
    [HttpPost]
    public async Task<IActionResult> Create(LostPetAd lostPetAd, string city, string district)
    {
        // Kullanıcı kimliği oturumda kontrol edilir
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            // Kullanıcı oturum açmamışsa login sayfasına yönlendirilir
            return RedirectToAction("Login", "Account");
        }

        // Kullanıcı ID'si, kayıp ilanı ile ilişkilendirilir
        lostPetAd.UserId = int.Parse(userId);

        // Şehir ve ilçeyi birleştirerek LastSeenLocation'a ekleyelim
        lostPetAd.LastSeenLocation = $"{city}, {district}";

        // Kayıp ilanı kaydedilir
        await _lostPetAdService.CreateLostPetAdAsync(lostPetAd);

        // Yeni ilan oluşturulması bildirilir
        await _notificationService.SendNotificationAsync("New Lost Pet Ad Created", 
            lostPetAd.Description, lostPetAd.LastSeenLocation);

        // Ana sayfaya yönlendirilir
        return RedirectToAction("Index");
    }

    // Kayıp ilanları listeleme için GET metodu
    [HttpGet]
    public async Task<IActionResult> Index(string location)
    {
        // Şehir/bölgeye göre ilanlar getirilir
        var lostPetAds = await _lostPetAdService.GetLostPetAdsByLocationAsync(location);

        // Elde edilen ilanlar View'a gönderilir
        return View(lostPetAds);
    }

    // Kayıp ilanı detaylarını görüntülemek için gerekli GET metodu
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        // İlan ID'sine göre ilan detayları getirilir
        var lostPetAd = await _lostPetAdService.GetLostPetAdByIdAsync(id);

        // Eğer ilan bulunamazsa hata sayfasına yönlendirilir
        if (lostPetAd == null)
        {
            return NotFound();
        }

        // Detaylar sayfasına yönlendirilir
        return View(lostPetAd);
    }
}
