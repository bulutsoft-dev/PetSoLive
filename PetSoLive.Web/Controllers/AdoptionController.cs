using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Interfaces;

namespace PetSoLive.Web.Controllers;

public class AdoptionController : Controller
{
    private readonly IServiceManager _serviceManager;
    private readonly IStringLocalizer<AdoptionController> _localizer;

    public AdoptionController(
        IServiceManager serviceManager,
        IStringLocalizer<AdoptionController> localizer)
    {
        _serviceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var pets = await _serviceManager.PetService.GetAllPetsAsync();
        ViewData["Title"] = _localizer["AvailablePetsTitle"];
        return View(pets);
    }

    public async Task<IActionResult> Adopt(int petId)
    {
        var username = HttpContext.Session.GetString("Username");
        if (username == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _serviceManager.UserService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return BadRequest("User not found.");
        }

        var existingRequest = await _serviceManager.AdoptionService.GetAdoptionRequestByUserAndPetAsync(user.Id, petId);
        if (existingRequest != null)
        {
            ViewBag.ErrorMessage = "You have already submitted an adoption request for this pet.";
            ViewBag.PetId = petId;
            return View("AdoptionRequestExists");
        }

        var pet = await _serviceManager.PetService.GetPetByIdAsync(petId);
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

        var pet = await _serviceManager.PetService.GetPetByIdAsync(petId);
        if (pet == null)
        {
            return NotFound();
        }

        var user = await _serviceManager.UserService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return BadRequest("User not found.");
        }

        var existingRequest = await _serviceManager.AdoptionService.GetAdoptionRequestByUserAndPetAsync(user.Id, petId);
        if (existingRequest != null)
        {
            ViewBag.ErrorMessage = "You have already submitted an adoption request for this pet.";
            ViewBag.PetId = petId;
            ViewBag.PetName = pet.Name;
            return View("AdoptionRequestExists");
        }

        user.PhoneNumber = phone;
        user.Address = address;
        user.DateOfBirth = dateOfBirth;

        await _serviceManager.UserService.UpdateUserAsync(user);

        var adoptionRequest = new AdoptionRequest
        {
            PetId = petId,
            Message = message,
            Status = AdoptionStatus.Pending,
            RequestDate = DateTime.Now,
            UserId = user.Id,
            User = user
        };

        await _serviceManager.AdoptionService.CreateAdoptionRequestAsync(adoptionRequest);
        await SendAdoptionRequestNotificationAsync(adoptionRequest);
        await SendAdoptionConfirmationEmailAsync(user, pet);

        return RedirectToAction("Details", "Pet", new { id = petId });
    }

    public async Task SendAdoptionRequestNotificationAsync(AdoptionRequest adoptionRequest)
    {
        var petOwner = await _serviceManager.PetOwnerService.GetPetOwnerByPetIdAsync(adoptionRequest.PetId);
        if (petOwner == null)
        {
            return;
        }
        
        var petOwnerUser = await _serviceManager.UserService.GetUserByIdAsync(petOwner.UserId);

        var user = adoptionRequest.User;
        var pet = adoptionRequest.Pet;

        var subject = "New Adoption Request for Your Pet";
        var emailHelper = new EmailHelper();
        var body = emailHelper.GenerateAdoptionRequestEmailBody(user, pet, adoptionRequest);

        await _serviceManager.EmailService.SendEmailAsync(petOwnerUser.Email, subject, body);
    }

    public async Task SendAdoptionConfirmationEmailAsync(User user, Pet pet)
    {
        var subject = "Adoption Request Submitted Successfully";
        var emailHelper = new EmailHelper();
        var body = emailHelper.GenerateAdoptionRequestConfirmationEmailBody(user, pet);

        await _serviceManager.EmailService.SendEmailAsync(user.Email, subject, body);
    }

    public async Task<IActionResult> ApproveRequest(int adoptionRequestId, int petId)
    {
        var adoptionRequest = await _serviceManager.AdoptionRequestService.GetAdoptionRequestByIdAsync(adoptionRequestId);
        if (adoptionRequest == null || adoptionRequest.PetId != petId)
        {
            return NotFound();
        }

        var pet = await _serviceManager.PetService.GetPetByIdAsync(petId);
        var petOwner = pet.PetOwners.FirstOrDefault();

        if (petOwner?.UserId.ToString() != User?.Identity?.Name)
        {
            return Unauthorized();
        }

        adoptionRequest.Status = AdoptionStatus.Approved;
        await _serviceManager.AdoptionRequestService.UpdateAdoptionRequestAsync(adoptionRequest);

        var approvedUser = adoptionRequest.User;
        if (approvedUser != null)
        {
            await SendApprovalEmailAsync(approvedUser, pet);
        }

        var pendingRequests = await _serviceManager.AdoptionRequestService.GetPendingRequestsByPetIdAsync(petId);
        if (pendingRequests != null)
        {
            foreach (var request in pendingRequests)
            {
                if (request.Id != adoptionRequestId)
                {
                    request.Status = AdoptionStatus.Rejected;
                    await _serviceManager.AdoptionRequestService.UpdateAdoptionRequestAsync(request);

                    var rejectedUser = request.User;
                    if (rejectedUser != null)
                    {
                        await SendRejectionEmailAsync(rejectedUser, pet);
                    }
                }
            }
        }

        var adoption = new Adoption
        {
            PetId = petId,
            UserId = adoptionRequest.UserId,
            AdoptionDate = DateTime.Now,
            Status = AdoptionStatus.Approved,
            Pet = pet,
            User = adoptionRequest.User
        };

        await _serviceManager.AdoptionService.CreateAdoptionAsync(adoption);

        return RedirectToAction("Index");
    }

    private async Task SendApprovalEmailAsync(User user, Pet pet)
    {
        var subject = "Your Adoption Request Has Been Approved";
        var body = new EmailHelper().GenerateAdoptionConfirmationEmailBody(user, pet);
        await _serviceManager.EmailService.SendEmailAsync(user.Email, subject, body);
    }

    private async Task SendRejectionEmailAsync(User user, Pet pet)
    {
        var subject = "Adoption Request Rejected";
        var body = new EmailHelper().GenerateRejectionEmailBody(user, pet);
        await _serviceManager.EmailService.SendEmailAsync(user.Email, subject, body);
    }
}