using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System.Threading.Tasks;

public class LostPetAdController : Controller
{
    private readonly ILostPetAdService _lostPetAdService;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    // Constructor Dependency Injection
    public LostPetAdController(ILostPetAdService lostPetAdService, 
        IUserRepository userRepository, 
        IEmailService emailService)
    {
        _lostPetAdService = lostPetAdService;
        _userRepository = userRepository;
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

    // GET: /LostPetAd/Create
    [HttpGet]
    public IActionResult Create()
    {
        // Oturum kontrolü
        var loginRedirect = RedirectToLoginIfNotLoggedIn();
        if (loginRedirect != null) return loginRedirect;

        // Pass the list of cities to the view
        ViewBag.Cities = CityList.Cities;
        return View();
    }

    // GET: /LostPetAd/GetDistricts
    [HttpGet]
    public JsonResult GetDistricts(string city)
    {
        var districts = CityList.GetDistrictsByCity(city);
        return Json(districts);
    }

    // POST: /LostPetAd/Create
    [HttpPost]
    public async Task<IActionResult> Create(LostPetAd lostPetAd, string city, string district)
    {
        // Oturum kontrolü
        var loginRedirect = RedirectToLoginIfNotLoggedIn();
        if (loginRedirect != null) return loginRedirect;

        if (ModelState.IsValid)
        {
            // Set the location details
            lostPetAd.LastSeenCity = city;
            lostPetAd.LastSeenDistrict = district;

            // Create the lost pet ad using the service
            await _lostPetAdService.CreateLostPetAdAsync(lostPetAd, city, district);

            // Set success message
            TempData["SuccessMessage"] = "Lost Pet Ad created successfully!";

            // Redirect to the Index page after successful ad creation
            return RedirectToAction("Index");
        }

        // If the model is invalid, return the view with the model
        return View(lostPetAd);
    }
}
