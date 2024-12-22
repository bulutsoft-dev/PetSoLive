using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Helpers;
using Microsoft.AspNetCore.Identity;

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
        var pets = await _petService.GetAllPetsAsync();
        return View(pets);  // Pass the list of pets to the view
    }

    // GET: /Adoption/Adopt/{id}
// AdoptionController.cs

public async Task<IActionResult> Adopt(int petId)
{
    var username = HttpContext.Session.GetString("Username");
    if (username == null)
    {
        return RedirectToAction("Login", "Account");
    }

    var user = await _userService.GetUserByUsernameAsync(username);
    if (user == null)
    {
        return BadRequest("User not found.");
    }

    // Check if the user has already sent an adoption request for this pet
    var existingRequest = await _adoptionService.GetAdoptionRequestByUserAndPetAsync(user.Id, petId);
    if (existingRequest != null)
    {
        // If a request already exists, show an error message and prevent re-submission
        ViewBag.ErrorMessage = "You have already submitted an adoption request for this pet.";
        ViewBag.PetId = petId;
        return View("AdoptionRequestExists"); // Return a view informing the user
    }

    var pet = await _petService.GetPetByIdAsync(petId);
    if (pet == null)
    {
        return NotFound();
    }

    ViewData["PetName"] = pet.Name;
    ViewData["PetId"] = pet.Id;

    return View(user);
}

[HttpPost]
public async Task<IActionResult> Adopt(int petId, string name, string email, string phone, string address, DateTime dateOfBirth, string message)
{
    var username = HttpContext.Session.GetString("Username");
    if (username == null)
    {
        return RedirectToAction("Login", "Account");
    }

    var pet = await _petService.GetPetByIdAsync(petId);
    if (pet == null)
    {
        return NotFound();
    }

    var user = await _userService.GetUserByUsernameAsync(username);
    if (user == null)
    {
        return BadRequest("User not found.");
    }

    // Check if the user has already sent an adoption request for this pet
    var existingRequest = await _adoptionService.GetAdoptionRequestByUserAndPetAsync(user.Id, petId);
    if (existingRequest != null)
    {
        ViewBag.ErrorMessage = "You have already submitted an adoption request for this pet.";
        ViewBag.PetId = petId;
        ViewBag.PetName = pet.Name;
        return View("AdoptionRequestExists");
    }

    // Update the user directly using the UserService
    user.PhoneNumber = phone;
    user.Address = address;
    user.DateOfBirth = dateOfBirth;

    // Call the update method from UserService
    await _userService.UpdateUserAsync(user);

    var adoptionRequest = new AdoptionRequest
    {
        PetId = petId,
        Message = message,
        Status = AdoptionStatus.Pending,
        RequestDate = DateTime.Now,
        UserId = user.Id,
        User = user
    };

    await _adoptionService.CreateAdoptionRequestAsync(adoptionRequest);
    await SendAdoptionRequestNotificationAsync(adoptionRequest);
    await SendAdoptionConfirmationEmailAsync(user, pet);

    return RedirectToAction("Details", "Pet", new { id = petId });
}


    public async Task SendAdoptionRequestNotificationAsync(AdoptionRequest adoptionRequest)
    {
        var petOwner = await _petOwnerService.GetPetOwnerByPetIdAsync(adoptionRequest.PetId);
        var petOwnerUser = await _userService.GetUserByIdAsync(petOwner.UserId);

        var user = adoptionRequest.User;
        var pet = adoptionRequest.Pet;

        var subject = "New Adoption Request for Your Pet";
        var emailHelper = new EmailHelper();
        var body = emailHelper.GenerateAdoptionRequestEmailBody(user, pet, adoptionRequest);

        await _emailService.SendEmailAsync(petOwnerUser.Email, subject, body);
    }
    public async Task SendAdoptionConfirmationEmailAsync(User user, Pet pet)
    {
        var subject = "Adoption Request Submitted Successfully";
        var emailHelper = new EmailHelper();
    
        // Email body generation logic (you can customize this as needed)
        var body = emailHelper.GenerateAdoptionConfirmationEmailBody(user, pet);

        await _emailService.SendEmailAsync(user.Email, subject, body);
    }

}
