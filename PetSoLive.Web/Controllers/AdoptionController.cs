using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using Microsoft.AspNetCore.Identity;
using PetSoLive.Core.Helpers;

public class AdoptionController : Controller
{
    private readonly IAdoptionService _adoptionService;
    private readonly IPetService _petService;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly IPetOwnerService _petOwnerService;

    public AdoptionController(IAdoptionService adoptionService, IPetService petService, IUserService userService, IEmailService emailService, IPetOwnerService petOwnerService)
    {
        _adoptionService = adoptionService;
        _petService = petService;
        _userService = userService;
        _emailService = emailService;
        _petOwnerService = petOwnerService;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Fetch all pets available for adoption
        var pets = await _petService.GetAllPetsAsync();

        return View(pets);  // Pass the list of pets to the view
    }
  // GET: /Adoption/Adopt/{id}
  public async Task<IActionResult> Adopt(int petId)
  {
      var username = HttpContext.Session.GetString("Username");
      if (username == null)
      {
          return RedirectToAction("Login", "Account");
      }

      // Kullanıcıyı çek
      var user = await _userService.GetUserByUsernameAsync(username);
      if (user == null)
      {
          return BadRequest("User not found.");
      }

      // Pet bilgisi
      var pet = await _petService.GetPetByIdAsync(petId);
      if (pet == null)
      {
          return NotFound();
      }

      // ViewData ile pet bilgisi gönder
      ViewData["PetName"] = pet.Name;
      ViewData["PetId"] = pet.Id;

      return View(user); // Model olarak kullanıcıyı gönder
  }


[HttpPost]
public async Task<IActionResult> Adopt(int petId, string name, string email, string phone, string address, DateTime dateOfBirth, string message)
{
    // Kullanıcı oturumunun olup olmadığını kontrol et
    var username = HttpContext.Session.GetString("Username");
    if (username == null)
    {
        return RedirectToAction("Login", "Account"); // Kullanıcı oturum açmamışsa giriş sayfasına yönlendir
    }

    // Pet bilgisi al
    var pet = await _petService.GetPetByIdAsync(petId);
    if (pet == null)
    {
        return NotFound();
    }

    // Oturum açan kullanıcıyı al
    var user = await _userService.GetUserByUsernameAsync(username);
    if (user == null)
    {
        return BadRequest("User not found.");
    }

    // Kullanıcı bilgilerini güncelle
    bool isUpdated = false;

    // Telefon numarası değişmişse güncelle
    if (user.PhoneNumber != phone)
    {
        user.PhoneNumber = phone;
        isUpdated = true;
    }
    
    // Adres değişmişse güncelle
    if (user.Address != address)
    {
        user.Address = address;
        isUpdated = true;
    }

    // Doğum tarihi değişmişse güncelle
    if (user.DateOfBirth != dateOfBirth)
    {
        user.DateOfBirth = dateOfBirth;
        isUpdated = true;
    }

    // Kullanıcı bilgileri güncellenmişse kaydet
    if (isUpdated)
    {
        await _userService.UpdateUserAsync(user); // Kullanıcıyı güncelle
    }

    // Evcil hayvan sahiplenme isteği oluştur
    var adoptionRequest = new AdoptionRequest
    {
        PetId = petId,
        Message = message,
        Status = AdoptionStatus.Pending,
        RequestDate = DateTime.Now,
        UserId = user.Id,  // Oturumdaki kullanıcıyı otomatik olarak atıyoruz
        User = user         // Kullanıcı ilişkisini ayarlıyoruz
    };

    // Evcil hayvan sahiplenme isteğini kaydet
    await _adoptionService.CreateAdoptionRequestAsync(adoptionRequest);

    // Pet sahibiyle bildirim göndermek
    await SendAdoptionRequestNotificationAsync(adoptionRequest);

    // Evcil hayvanın detaylarına yönlendir
    return RedirectToAction("Details", "Pet", new { id = petId });
}



// This method sends an email to the pet owner regarding the new adoption request
    public async Task SendAdoptionRequestNotificationAsync(AdoptionRequest adoptionRequest)
    {
        var petOwner = await _petOwnerService.GetPetOwnerByPetIdAsync(adoptionRequest.PetId);
        var petOwnerUser = await _userService.GetUserByIdAsync(petOwner.UserId);

        // Fetching detailed user and pet information
        var user = adoptionRequest.User;
        var pet = adoptionRequest.Pet;

        var subject = "New Adoption Request for Your Pet";

        // Use the helper to generate the email body
        var emailHelper = new EmailHelper();
        var body = emailHelper.GenerateAdoptionRequestEmailBody(user, pet, adoptionRequest);

        // Sending the email
        await _emailService.SendEmailAsync(petOwnerUser.Email, subject, body);
    }



}
