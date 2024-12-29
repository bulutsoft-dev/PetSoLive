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
    private readonly IEmailService _emailService;
    private readonly ICommentService _commentService; // Add comment service

    public HelpRequestController(IHelpRequestService helpRequestService, 
        IUserService userService, 
        IEmailService emailService,
        IVeterinarianService veterinarianService,
        ICommentService commentService) // Inject comment service
    {
        _helpRequestService = helpRequestService;
        _userService = userService;
        _emailService = emailService;
        _veterinarianService = veterinarianService;
        _commentService = commentService; // Assign comment service
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

        // Yorumları dahil et (Include Comments)
        var comments = await _commentService.GetCommentsByHelpRequestIdAsync(id);
        helpRequest.Comments = comments;

        var username = HttpContext.Session.GetString("Username");
        var user = username != null ? await _userService.GetUserByUsernameAsync(username) : null;

        ViewBag.CanEditOrDelete = user != null && helpRequest.UserId == user.Id;
        ViewBag.isVeterinarian = user != null && await _veterinarianService.GetByUserIdAsync(user.Id) != null;
        // Pass whether the current user can edit or delete specific comments
        if (user != null)
        {
            ViewBag.CanEditOrDeleteComment = comments.Where(c => c.UserId == user.Id).Select(c => c.Id).ToList();
        }
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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int id, string content)
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
        if (helpRequest == null)
        {
            return NotFound();
        }

        // Check if the user is a veterinarian
        var veterinarian = await _veterinarianService.GetByUserIdAsync(user.Id);
        int? veterinarianId = veterinarian?.Id; // If user is a veterinarian, get their ID, otherwise set to null

        var comment = new Comment
        {
            Content = content,
            CreatedAt = DateTime.Now,
            HelpRequestId = helpRequest.Id,
            UserId = user.Id,
            VeterinarianId = veterinarianId // Store VeterinarianId if the user is a veterinarian
        };

        // Add the comment to the database
        await _commentService.AddCommentAsync(comment);

        return RedirectToAction("Details", "HelpRequest", new { id = helpRequest.Id });
    }
    
    [HttpGet]
    public async Task<IActionResult> EditComment(int id)
    {
        var comment = await _commentService.GetCommentByIdAsync(id);
        if (comment == null)
        {
            return NotFound();
        }

        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _userService.GetUserByUsernameAsync(username);
        if (user == null || comment.UserId != user.Id)
        {
            return Unauthorized();
        }

        return View(comment); // Yorumu doğrudan View'a gönderiyoruz.
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditComment(int id, int helpRequestId, string content)
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

        var existingComment = await _commentService.GetCommentByIdAsync(id);
        if (existingComment == null || existingComment.UserId != user.Id)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            ModelState.AddModelError(nameof(content), "Content cannot be empty.");
            return View(existingComment); // Hatalı durumlarda eski yorumu yeniden yükler.
        }

        existingComment.Content = content;
        existingComment.CreatedAt = DateTime.Now;

        var veterinarian = await _veterinarianService.GetByUserIdAsync(user.Id);
        existingComment.VeterinarianId = veterinarian?.Id;

        await _commentService.UpdateCommentAsync(existingComment);

        return RedirectToAction("Details", new { id = helpRequestId });
    }




// Delete Comment Action
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment(int commentId)
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

        var comment = await _commentService.GetCommentByIdAsync(commentId);
        if (comment == null || comment.UserId != user.Id)
        {
            return Unauthorized();
        }

        await _commentService.DeleteCommentAsync(commentId);

        return RedirectToAction("Details", new { id = comment.HelpRequestId });
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