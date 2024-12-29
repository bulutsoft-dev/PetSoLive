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
        // Oturum kontrolü, giriş yapmamış kullanıcıyı yönlendir
        var redirectResult = RedirectToLoginIfNotLoggedIn();
        if (redirectResult != null) return redirectResult;

        // Şehir listesini ViewData ile view'a gönderiyoruz
        ViewData["Cities"] = CityList.Cities;

        // Başlangıçta district listesi boş olacak
        ViewData["Districts"] = new List<string>(); 

        return View();
    }
    
    // GET: /LostPetAd/Index
    public async Task<IActionResult> Index()
    {
        // Get all lost pet ads using service
        var lostPetAds = await _lostPetAdService.GetAllLostPetAdsAsync();

        if (lostPetAds == null)
        {
            // Log or handle the error as needed (optional)
            TempData["ErrorMessage"] = "Could not retrieve lost pet ads. Please try again later.";
        
            // Return an empty list if null
            lostPetAds = new List<LostPetAd>();
        }

        return View(lostPetAds); // Pass the list of ads (empty or retrieved) to the view
    
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LostPetAd lostPetAd, string city, string district)
    {
        // Oturum kontrolü, giriş yapmamış kullanıcıyı yönlendir
        var redirectResult = RedirectToLoginIfNotLoggedIn();
        if (redirectResult != null) return redirectResult;

        // City ve District bilgilerini LostPetAd nesnesine ekleyin
        if (string.IsNullOrEmpty(city) || string.IsNullOrEmpty(district))
        {
            TempData["ErrorMessage"] = "City and District are required.";
            ViewData["Cities"] = CityList.Cities;
            ViewData["Districts"] = new List<string>();  // Boş district listesi
            return View(lostPetAd);  // Hata mesajı ve boş district listesiyle view'a dön
        }
    
        lostPetAd.LastSeenCity = city;
        lostPetAd.LastSeenDistrict = district;

        // Kullanıcının ID'sini kaydedin (oturumda mevcut olan kullanıcıyı varsayarak)
        var username = HttpContext.Session.GetString("Username");
        var user = await _userService.GetUserByUsernameAsync(username);
        lostPetAd.UserId = user.Id;

        // LostPetAd nesnesini veritabanına kaydedin
        await _lostPetAdService.CreateLostPetAdAsync(lostPetAd, city, district);

        // Başarılı bir işlem mesajı gösterin
        TempData["SuccessMessage"] = "The lost pet ad has been created successfully.";

        // Yönlendirme işlemi
        return RedirectToAction("Index");
    }


    

    
}
