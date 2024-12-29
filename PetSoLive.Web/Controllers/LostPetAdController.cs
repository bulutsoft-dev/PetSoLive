using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LostPetAdController : Controller
{
    private readonly ILostPetAdService _lostPetAdService;
    private readonly IEmailService _emailService;
    private readonly IUserService _userService;

    // Constructor Dependency Injection
    public LostPetAdController(ILostPetAdService lostPetAdService, 
        IUserService userService,
        IEmailService emailService)
    {
        _lostPetAdService = lostPetAdService;
        _userService = userService;
        _emailService = emailService;
    }

    // Oturum kontrolü metodu
    private IActionResult RedirectToLoginIfNotLoggedIn()
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Login", "Account");
        }
        return null;
    }

    // GET: /LostPetAd/Create
    public IActionResult Create()
    {
        var redirectResult = RedirectToLoginIfNotLoggedIn();
        if (redirectResult != null) return redirectResult;

        ViewData["Cities"] = CityList.Cities;
        ViewData["Districts"] = new List<string>(); 
        return View();
    }
    
    // GET: /LostPetAd/Index
    public async Task<IActionResult> Index()
    {
        var lostPetAds = await _lostPetAdService.GetAllLostPetAdsAsync();
        if (lostPetAds == null)
        {
            TempData["ErrorMessage"] = "Could not retrieve lost pet ads. Please try again later.";
            lostPetAds = new List<LostPetAd>();
        }

        return View(lostPetAds); 
    }
    
    // GET: /LostPetAd/Details/5
    public async Task<IActionResult> Details(int id)
    {
        // Kayıp ilanı ID'sine göre veritabanından alınır
        var lostPetAd = await _lostPetAdService.GetLostPetAdByIdAsync(id);
        
        if (lostPetAd == null)
        {
            // Eğer ilan bulunmazsa, hata mesajı gösterilir ve Index sayfasına dönülür
            TempData["ErrorMessage"] = "Lost Pet Ad not found.";
            return RedirectToAction("Index");
        }

        // Detay sayfasına ilan verisi gönderilir
        return View(lostPetAd);
    }
    

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LostPetAd lostPetAd, string city, string district)
    {
        var redirectResult = RedirectToLoginIfNotLoggedIn();
        if (redirectResult != null) return redirectResult;

        if (string.IsNullOrEmpty(city) || string.IsNullOrEmpty(district))
        {
            TempData["ErrorMessage"] = "City and District are required.";
            ViewData["Cities"] = CityList.Cities;
            ViewData["Districts"] = new List<string>();  
            return View(lostPetAd);  
        }

        lostPetAd.LastSeenCity = city;
        lostPetAd.LastSeenDistrict = district;

        var username = HttpContext.Session.GetString("Username");
        var user = await _userService.GetUserByUsernameAsync(username);
        lostPetAd.UserId = user.Id;

        await _lostPetAdService.CreateLostPetAdAsync(lostPetAd, city, district);

        var usersInLocation = await _userService.GetUsersByLocationAsync(city, district);
        foreach (var targetUser in usersInLocation)
        {
            var subject = "New Lost Pet Ad Created";
            var body = $"A new lost pet ad has been posted. Pet name: {lostPetAd.PetName}, Location: {lostPetAd.LastSeenLocation}. Description: {lostPetAd.Description}.";
            await _emailService.SendEmailAsync(targetUser.Email, subject, body);
        }

        TempData["SuccessMessage"] = "The lost pet ad has been created successfully, and notifications have been sent.";
        return RedirectToAction("Index");
    }
}

