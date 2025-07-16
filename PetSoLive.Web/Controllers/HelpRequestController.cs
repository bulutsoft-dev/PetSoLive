using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Interfaces;

namespace PetSoLive.Web.Controllers;

public class HelpRequestController : Controller
{
    private readonly IServiceManager _serviceManager;
    private readonly IStringLocalizer<HelpRequestController> _localizer;
    private readonly PetSoLive.Web.Helpers.ImgBBHelper _imgBBHelper;

    public HelpRequestController(IServiceManager serviceManager, IStringLocalizer<HelpRequestController> localizer, PetSoLive.Web.Helpers.ImgBBHelper imgBBHelper)
    {
        _serviceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        _imgBBHelper = imgBBHelper;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _serviceManager.UserService.GetUserByUsernameAsync(username);
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
    public async Task<IActionResult> Create(HelpRequest helpRequest, IFormFile image)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _serviceManager.UserService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        helpRequest.UserId = user.Id;
        helpRequest.CreatedAt = DateTime.UtcNow;
        helpRequest.Status = HelpRequestStatus.Active;

        if (ModelState.IsValid)
        {
            // Resim varsa ImgBB'ye yükle ve url'yi ata
            if (image != null)
            {
                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                var imageBytes = ms.ToArray();
                var imageUrl = await _imgBBHelper.UploadImageAsync(imageBytes);
                helpRequest.ImageUrl = imageUrl;
            }
            await _serviceManager.HelpRequestService.CreateHelpRequestAsync(helpRequest);

            var veterinarians = await _serviceManager.VeterinarianService.GetAllVeterinariansAsync();
            foreach (var veterinarian in veterinarians)
            {
                await SendNewHelpRequestEmailAsync(veterinarian.User, helpRequest, user);
            }

            return RedirectToAction("Index");
        }

        ViewBag.User = user;
        return View(helpRequest);
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var helpRequests = await _serviceManager.HelpRequestService.GetHelpRequestsAsync();
        return View(helpRequests);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var helpRequest = await _serviceManager.HelpRequestService.GetHelpRequestByIdAsync(id);

        if (helpRequest == null)
        {
            return NotFound();
        }

        var comments = await _serviceManager.CommentService.GetCommentsByHelpRequestIdAsync(id);
        helpRequest.Comments = comments;

        var username = HttpContext.Session.GetString("Username");
        var user = username != null ? await _serviceManager.UserService.GetUserByUsernameAsync(username) : null;

        ViewBag.CanEditOrDelete = user != null && helpRequest.UserId == user.Id;
        ViewBag.isVeterinarian = user != null && await _serviceManager.VeterinarianService.GetApprovedByUserIdAsync(user.Id) != null;
        if (user != null)
        {
            ViewBag.CanEditOrDeleteComment = comments.Where(c => c.UserId == user.Id).Select(c => c.Id).ToList();
        }
        var approvedVets = await _serviceManager.VeterinarianService.GetAllVeterinariansAsync();
        ViewBag.ApprovedVeterinarianUserIds = approvedVets.Select(v => v.UserId).ToList();
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

        var user = await _serviceManager.UserService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var helpRequest = await _serviceManager.HelpRequestService.GetHelpRequestByIdAsync(id);
        if (helpRequest == null || helpRequest.UserId != user.Id)
        {
            return Unauthorized();
        }

        return View(helpRequest);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(HelpRequest helpRequest, IFormFile image)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _serviceManager.UserService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var existingRequest = await _serviceManager.HelpRequestService.GetHelpRequestByIdAsync(helpRequest.Id);
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
            // Yeni resim yüklendiyse upload et
            if (image != null)
            {
                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                var imageBytes = ms.ToArray();
                var imageUrl = await _imgBBHelper.UploadImageAsync(imageBytes);
                existingRequest.ImageUrl = imageUrl;
            }
            await _serviceManager.HelpRequestService.UpdateHelpRequestAsync(existingRequest);

            var veterinarians = await _serviceManager.VeterinarianService.GetAllVeterinariansAsync();
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

        var user = await _serviceManager.UserService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var helpRequest = await _serviceManager.HelpRequestService.GetHelpRequestByIdAsync(id);
        if (helpRequest == null || helpRequest.UserId != user.Id)
        {
            return Unauthorized();
        }

        await _serviceManager.HelpRequestService.DeleteHelpRequestAsync(id);

        var veterinarians = await _serviceManager.VeterinarianService.GetAllVeterinariansAsync();
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

        var user = await _serviceManager.UserService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var helpRequest = await _serviceManager.HelpRequestService.GetHelpRequestByIdAsync(id);
        if (helpRequest == null)
        {
            return NotFound();
        }

        var comment = new Comment
        {
            Content = content,
            CreatedAt = DateTime.UtcNow,
            HelpRequestId = helpRequest.Id,
            UserId = user.Id,
            VeterinarianId = null
        };

        await _serviceManager.CommentService.AddCommentAsync(comment);

        return RedirectToAction("Details", "HelpRequest", new { id = helpRequest.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EditComment(int id)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Account");
        }
        var user = await _serviceManager.UserService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var comment = await _serviceManager.CommentService.GetCommentByIdAsync(id);
        if (comment == null)
        {
            return NotFound();
        }
        ViewBag.User = user;
        return View(comment);
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

        var user = await _serviceManager.UserService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var existingComment = await _serviceManager.CommentService.GetCommentByIdAsync(id);
        if (existingComment == null || existingComment.UserId != user.Id)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            ModelState.AddModelError(nameof(content), "Content cannot be empty.");
            return View(existingComment);
        }

        existingComment.Content = content;
        existingComment.CreatedAt = DateTime.UtcNow;
        existingComment.VeterinarianId = null;

        await _serviceManager.CommentService.UpdateCommentAsync(existingComment);

        return RedirectToAction("Details", new { id = helpRequestId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _serviceManager.UserService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var comment = await _serviceManager.CommentService.GetCommentByIdAsync(commentId);
        if (comment == null || comment.UserId != user.Id)
        {
            return Unauthorized();
        }

        await _serviceManager.CommentService.DeleteCommentAsync(commentId);

        return RedirectToAction("Details", new { id = comment.HelpRequestId });
    }

    private async Task SendNewHelpRequestEmailAsync(User veterinarian, HelpRequest helpRequest, User requester)
    {
        string subject = "New Help Request: Animal in Need!";
        var emailHelper = new EmailHelper();
        string body = emailHelper.GenerateVeterinarianNotificationEmailBody(helpRequest, requester);

        await _serviceManager.EmailService.SendEmailAsync(veterinarian.Email, subject, body);
    }

    private async Task SendUpdatedHelpRequestEmailAsync(User veterinarian, HelpRequest helpRequest, User requester)
    {
        string subject = "Help Request Updated: Animal in Need!";
        var emailHelper = new EmailHelper();
        string body = emailHelper.GenerateEditHelpRequestEmailBody(helpRequest, requester);

        await _serviceManager.EmailService.SendEmailAsync(veterinarian.Email, subject, body);
    }

    private async Task SendDeletedHelpRequestEmailAsync(User veterinarian, HelpRequest helpRequest, User requester)
    {
        string subject = "Help Request Deleted: Animal in Need!";
        var emailHelper = new EmailHelper();
        string body = emailHelper.GenerateDeleteHelpRequestEmailBody(helpRequest, requester);

        await _serviceManager.EmailService.SendEmailAsync(veterinarian.Email, subject, body);
    }
}