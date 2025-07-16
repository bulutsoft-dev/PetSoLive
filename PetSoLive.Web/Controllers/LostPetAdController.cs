using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.DTOs;

namespace PetSoLive.Web.Controllers;

public class LostPetAdController : Controller
{
    private readonly IServiceManager _serviceManager;
    private readonly IStringLocalizer<LostPetAdController> _localizer;
    private readonly PetSoLive.Web.Helpers.ImgBBHelper _imgBBHelper;

    // Constructor Dependency Injection
    public LostPetAdController(IServiceManager serviceManager, IStringLocalizer<LostPetAdController> localizer, PetSoLive.Web.Helpers.ImgBBHelper imgBBHelper)
    {
        _serviceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        _imgBBHelper = imgBBHelper;
    }

    // Oturum kontrolü metodu
    private IActionResult? RedirectToLoginIfNotLoggedIn()
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Login", "Account");
        }
        return null;
    }

    // Yeni bir GET metodu, AJAX isteğini karşılayacak
    public IActionResult GetDistrictsByCity(string city)
    {
        // GetDistrictsByCity metodunu çağırarak ilgili ilçeleri alıyoruz
        var districts = CityList.GetDistrictsByCity(city);

        // Dönen ilçeleri JSON formatında döndürüyoruz
        return Json(districts);
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

    // Kayıp ilanı oluşturma işlemi
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LostPetAd lostPetAd, string city, string district, IFormFile image)
    {
        var redirectResult = RedirectToLoginIfNotLoggedIn();
        if (redirectResult != null) return redirectResult;

        if (string.IsNullOrEmpty(city) || string.IsNullOrEmpty(district))
        {
            TempData["ErrorMessage"] = _localizer["CityAndDistrictRequired"]?.Value ?? "City and District are required.";
            ViewData["Cities"] = CityList.Cities;
            ViewData["Districts"] = new List<string>();
            return View(lostPetAd);
        }

        lostPetAd.LastSeenCity = city;
        lostPetAd.LastSeenDistrict = district;

        var username = HttpContext.Session.GetString("Username");
        var user = await _serviceManager.UserService.GetUserByUsernameAsync(username);
        lostPetAd.UserId = user.Id;

        // Resim varsa ImgBB'ye yükle ve url'yi ata
        if (image != null)
        {
            using var ms = new MemoryStream();
            await image.CopyToAsync(ms);
            var imageBytes = ms.ToArray();
            var imageUrl = await _imgBBHelper.UploadImageAsync(imageBytes);
            lostPetAd.ImageUrl = imageUrl;
        }
        await _serviceManager.LostPetAdService.CreateLostPetAdAsync(lostPetAd, city, district);

        TempData["SuccessMessage"] = _localizer["AdCreatedSuccess"]?.Value ?? "The lost pet ad has been created successfully, and notifications have been sent.";
        return RedirectToAction("Index");
    }

    // GET: /LostPetAd/Index
    public async Task<IActionResult> Index(string city, string district, string petType, DateTime? datePostedAfter)
    {
        var filterDto = new LostPetAdFilterDto
        {
            City = city,
            District = district,
            PetType = petType,
            DatePostedAfter = datePostedAfter
        };
        var lostPetAds = await _serviceManager.LostPetAdService.GetFilteredLostPetAdsAsync(filterDto);
        if (lostPetAds == null)
        {
            TempData["ErrorMessage"] = _localizer["RetrieveAdsError"]?.Value ?? "Could not retrieve lost pet ads. Please try again later.";
            lostPetAds = new List<LostPetAd>();
        }
        ViewData["Cities"] = CityList.Cities;
        ViewData["Districts"] = !string.IsNullOrEmpty(city) ? CityList.GetDistrictsByCity(city) : new List<string>();
        ViewData["SelectedCity"] = city;
        ViewData["SelectedDistrict"] = district;
        ViewData["SelectedPetType"] = petType;
        ViewData["SelectedDatePostedAfter"] = datePostedAfter;
        return View(lostPetAds);
    }

    // GET: /LostPetAd/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var lostPetAd = await _serviceManager.LostPetAdService.GetLostPetAdByIdAsync(id);
        if (lostPetAd == null)
        {
            TempData["ErrorMessage"] = _localizer["AdNotFound"]?.Value ?? "Lost Pet Ad not found.";
            return RedirectToAction("Index");
        }

        var currentUser = HttpContext.Session.GetString("Username");
        ViewData["CurrentUser"] = currentUser;
        return View(lostPetAd);
    }

    // GET: /LostPetAd/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var redirectResult = RedirectToLoginIfNotLoggedIn();
        if (redirectResult != null) return redirectResult;

        var lostPetAd = await _serviceManager.LostPetAdService.GetLostPetAdByIdAsync(id);
        if (lostPetAd == null)
        {
            TempData["ErrorMessage"] = _localizer["AdNotFound"]?.Value ?? "Lost Pet Ad not found.";
            return RedirectToAction("Index");
        }

        // Check if the current user is the owner of the ad
        var currentUser = HttpContext.Session.GetString("Username");
        if (lostPetAd.User == null || lostPetAd.User.Username != currentUser)
        {
            TempData["ErrorMessage"] = _localizer["EditPermissionDenied"]?.Value ?? "You do not have permission to edit this ad.";
            return RedirectToAction("Index");
        }

        // Populate city and district lists
        ViewData["Cities"] = CityList.Cities;
        ViewData["Districts"] = lostPetAd.LastSeenCity != null ? CityList.GetDistrictsByCity(lostPetAd.LastSeenCity) : new List<string>();

        return View(lostPetAd);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LostPetAd updatedLostPetAd, string city, string district, IFormFile image)
    {
        var redirectResult = RedirectToLoginIfNotLoggedIn();
        if (redirectResult != null) return redirectResult;

        if (id != updatedLostPetAd.Id)
        {
            TempData["ErrorMessage"] = _localizer["InvalidAdId"]?.Value ?? "Invalid ad ID.";
            return RedirectToAction("Index");
        }

        var lostPetAd = await _serviceManager.LostPetAdService.GetLostPetAdByIdAsync(id);
        if (lostPetAd == null)
        {
            TempData["ErrorMessage"] = _localizer["AdNotFound"]?.Value ?? "Lost Pet Ad not found.";
            return RedirectToAction("Index");
        }

        // Check if the current user is the owner of the ad
        var currentUser = HttpContext.Session.GetString("Username");
        if (lostPetAd.User == null || lostPetAd.User.Username != currentUser)
        {
            TempData["ErrorMessage"] = _localizer["EditPermissionDenied"]?.Value ?? "You do not have permission to edit this ad.";
            return RedirectToAction("Index");
        }

        // Update the properties of the lost pet ad
        lostPetAd.PetName = updatedLostPetAd.PetName;
        lostPetAd.Description = updatedLostPetAd.Description;
        lostPetAd.LastSeenCity = city;
        lostPetAd.LastSeenDistrict = district;
        lostPetAd.LastSeenDate = updatedLostPetAd.LastSeenDate;

        // Eğer yeni bir dosya yüklendiyse, ImgBB'ye upload et ve ImageUrl'yi güncelle
        if (image != null)
        {
            using var ms = new MemoryStream();
            await image.CopyToAsync(ms);
            var imageBytes = ms.ToArray();
            var imageUrl = await _imgBBHelper.UploadImageAsync(imageBytes);
            lostPetAd.ImageUrl = imageUrl;
        }

        try
        {
            // Update the lost pet ad
            await _serviceManager.LostPetAdService.UpdateLostPetAdAsync(lostPetAd);
            TempData["SuccessMessage"] = _localizer["AdUpdatedSuccess"]?.Value ?? "The lost pet ad has been updated successfully.";
            return RedirectToAction("Details", new { id = lostPetAd.Id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = _localizer["UpdateAdError"]?.Value ?? $"An error occurred while updating the lost pet ad: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    // GET: /LostPetAd/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var lostPetAd = await _serviceManager.LostPetAdService.GetLostPetAdByIdAsync(id);

        if (lostPetAd == null)
        {
            TempData["ErrorMessage"] = _localizer["AdNotFound"]?.Value ?? "Lost Pet Ad not found.";
            return RedirectToAction("Index");
        }

        // Check if the current user is the owner of the ad
        var currentUser = HttpContext.Session.GetString("Username");

        if (lostPetAd.User == null)
        {
            TempData["ErrorMessage"] = _localizer["UserNotFound"]?.Value ?? "The user associated with this ad is not found.";
            return RedirectToAction("Index");
        }

        if (lostPetAd.User.Username != currentUser)
        {
            TempData["ErrorMessage"] = _localizer["DeletePermissionDenied"]?.Value ?? "You do not have permission to delete this ad.";
            return RedirectToAction("Index");
        }

        TempData["DeleteMessage"] = _localizer["DeleteConfirmation"]?.Value ?? "Are you sure you want to delete this ad?";
        return View(lostPetAd);
    }

    // POST: /LostPetAd/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var lostPetAd = await _serviceManager.LostPetAdService.GetLostPetAdByIdAsync(id);

        if (lostPetAd == null)
        {
            TempData["ErrorMessage"] = _localizer["AdNotFound"]?.Value ?? "Lost Pet Ad not found.";
            return RedirectToAction("Index");
        }

        // Check if the current user is the owner of the ad
        var currentUser = HttpContext.Session.GetString("Username");

        if (lostPetAd.User == null)
        {
            TempData["ErrorMessage"] = _localizer["UserNotFound"]?.Value ?? "The user associated with this ad is not found.";
            return RedirectToAction("Index");
        }

        if (lostPetAd.User.Username != currentUser)
        {
            TempData["ErrorMessage"] = _localizer["DeletePermissionDenied"]?.Value ?? "You do not have permission to delete this ad.";
            return RedirectToAction("Index");
        }

        await _serviceManager.LostPetAdService.DeleteLostPetAdAsync(lostPetAd);
        TempData["SuccessMessage"] = _localizer["AdDeletedSuccess"]?.Value ?? "The lost pet ad has been deleted successfully.";
        return RedirectToAction("Index");
    }
}