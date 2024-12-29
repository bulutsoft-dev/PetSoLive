using Microsoft.AspNetCore.Mvc;
using PetSoLive.Business.Services;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;
using System.Linq;
using System.Threading.Tasks;

public class HelpRequestController : Controller
{
    private readonly IHelpRequestService _helpRequestService;
    private readonly IUserService _userService;
    private readonly IVeterinarianService _veterinarianService;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public HelpRequestController(IHelpRequestService helpRequestService, 
        IUserService userService, 
        INotificationService notificationService,
        IEmailService emailService,
        IVeterinarianService veterinarianService)
    {
        _helpRequestService = helpRequestService;
        _userService = userService;
        _notificationService = notificationService;
        _emailService = emailService;
        _veterinarianService = veterinarianService;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _userService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var helpRequest = new HelpRequest();
        ViewBag.User = user;

        return View(helpRequest);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HelpRequest helpRequest)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _userService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        helpRequest.UserId = user.Id;
        helpRequest.CreatedAt = DateTime.Now;
        helpRequest.Status = HelpRequestStatus.Active;

        if (ModelState.IsValid)
        {
            await _helpRequestService.CreateHelpRequestAsync(helpRequest);

            if (helpRequest.EmergencyLevel == EmergencyLevel.High)
            {
                await _notificationService.SendEmergencyNotificationAsync("High urgency help request", helpRequest.Description);
            }

            var veterinarians = await _veterinarianService.GetAllVeterinariansAsync();
            foreach (var veterinarian in veterinarians)
            {
                await SendNewHelpRequestEmailAsync(veterinarian.User, helpRequest, user);
            }

            return RedirectToAction("Index");
        }

        return View(helpRequest);
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var helpRequests = await _helpRequestService.GetHelpRequestsAsync();
        return View(helpRequests);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var helpRequest = await _helpRequestService.GetHelpRequestByIdAsync(id);
        if (helpRequest == null)
        {
            return NotFound();
        }

        var username = HttpContext.Session.GetString("Username");
        var user = username != null ? await _userService.GetUserByUsernameAsync(username) : null;

        ViewBag.CanEditOrDelete = user != null && helpRequest.UserId == user.Id;

        return View(helpRequest);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _userService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var helpRequest = await _helpRequestService.GetHelpRequestByIdAsync(id);
        if (helpRequest == null || helpRequest.UserId != user.Id)
        {
            return Unauthorized();
        }

        return View(helpRequest);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(HelpRequest helpRequest)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _userService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var existingRequest = await _helpRequestService.GetHelpRequestByIdAsync(helpRequest.Id);
        if (existingRequest == null || existingRequest.UserId != user.Id)
        {
            return Unauthorized();
        }

        if (ModelState.IsValid)
        {
            existingRequest.Title = helpRequest.Title;
            existingRequest.Description = helpRequest.Description;
            existingRequest.EmergencyLevel = helpRequest.EmergencyLevel;
            existingRequest.Status = helpRequest.Status;
            existingRequest.Location = helpRequest.Location;
            existingRequest.ContactName = helpRequest.ContactName;
            existingRequest.ContactPhone = helpRequest.ContactPhone;
            existingRequest.ContactEmail = helpRequest.ContactEmail;
            existingRequest.ImageUrl = helpRequest.ImageUrl;

            await _helpRequestService.UpdateHelpRequestAsync(existingRequest);

            var veterinarians = await _veterinarianService.GetAllVeterinariansAsync();
            foreach (var vet in veterinarians)
            {
                await SendUpdatedHelpRequestEmailAsync(vet.User, existingRequest, user);
            }

            return RedirectToAction("Index");
        }

        return View(helpRequest);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _userService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var helpRequest = await _helpRequestService.GetHelpRequestByIdAsync(id);
        if (helpRequest == null || helpRequest.UserId != user.Id)
        {
            return Unauthorized();
        }

        await _helpRequestService.DeleteHelpRequestAsync(id);

        var veterinarians = await _veterinarianService.GetAllVeterinariansAsync();
        foreach (var veterinarian in veterinarians)
        {
            await SendDeletedHelpRequestEmailAsync(veterinarian.User, helpRequest, user);
        }

        return RedirectToAction("Index");
    }

    // Method to send email for new help request
    private async Task SendNewHelpRequestEmailAsync(User veterinarian, HelpRequest helpRequest, User requester)
    {
        string subject = "New Help Request: Animal in Need!";
        var emailHelper = new EmailHelper();
        string body = emailHelper.GenerateVeterinarianNotificationEmailBody(helpRequest, requester);

        await _emailService.SendEmailAsync(veterinarian.Email, subject, body);
    }

    // Method to send email for updated help request
    private async Task SendUpdatedHelpRequestEmailAsync(User veterinarian, HelpRequest helpRequest, User requester)
    {
        string subject = "Help Request Updated: Animal in Need!";
        var emailHelper = new EmailHelper();
        string body = emailHelper.GenerateEditHelpRequestEmailBody(helpRequest, requester);

        await _emailService.SendEmailAsync(veterinarian.Email, subject, body);
    }

    // Method to send email for deleted help request
    private async Task SendDeletedHelpRequestEmailAsync(User veterinarian, HelpRequest helpRequest, User requester)
    {
        string subject = "Help Request Deleted: Animal in Need!";
        var emailHelper = new EmailHelper();
        string body = emailHelper.GenerateDeleteHelpRequestEmailBody(helpRequest, requester);

        await _emailService.SendEmailAsync(veterinarian.Email, subject, body);
    }
}
