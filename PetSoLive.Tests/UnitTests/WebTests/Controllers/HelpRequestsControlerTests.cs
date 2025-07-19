using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Interfaces;
using PetSoLive.Web.Controllers;
using PetSoLive.Web.Helpers;

namespace PetSoLive.Tests.Controllers;

public class HelpRequestControllerTests
{
    private readonly Mock<IServiceManager> _serviceManagerMock;
    private readonly Mock<IStringLocalizer<HelpRequestController>> _localizerMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IHelpRequestService> _helpRequestServiceMock;
    private readonly Mock<IVeterinarianService> _veterinarianServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ICommentService> _commentServiceMock;
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<ImgBBHelper> _imgBBHelperMock;
    private readonly HelpRequestController _controller;
    private readonly DefaultHttpContext _httpContext;

    public HelpRequestControllerTests()
    {
        _serviceManagerMock = new Mock<IServiceManager>();
        _localizerMock = new Mock<IStringLocalizer<HelpRequestController>>();
        _userServiceMock = new Mock<IUserService>();
        _helpRequestServiceMock = new Mock<IHelpRequestService>();
        _veterinarianServiceMock = new Mock<IVeterinarianService>();
        _emailServiceMock = new Mock<IEmailService>();
        _commentServiceMock = new Mock<ICommentService>();
        _sessionMock = new Mock<ISession>();
        _imgBBHelperMock = new Mock<ImgBBHelper>("dummy_api_key");

        // Setup IServiceManager to return mocked services
        _serviceManagerMock.SetupGet(m => m.UserService).Returns(_userServiceMock.Object);
        _serviceManagerMock.SetupGet(m => m.HelpRequestService).Returns(_helpRequestServiceMock.Object);
        _serviceManagerMock.SetupGet(m => m.VeterinarianService).Returns(_veterinarianServiceMock.Object);
        _serviceManagerMock.SetupGet(m => m.EmailService).Returns(_emailServiceMock.Object);
        _serviceManagerMock.SetupGet(m => m.CommentService).Returns(_commentServiceMock.Object);

        _httpContext = new DefaultHttpContext
        {
            Session = _sessionMock.Object
        };

        _controller = new HelpRequestController(_serviceManagerMock.Object, _localizerMock.Object, _imgBBHelperMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            }
        };
    }

    [Fact]
    public async Task Create_Get_WhenNoUserLoggedIn_RedirectsToLogin()
    {
        // Arrange
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

        // Act
        var result = await _controller.Create();

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("Account", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Create_Get_WhenUserLoggedIn_ReturnsView()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes(username); return true; });
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);

        // Act
        var result = await _controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
        Assert.IsType<HelpRequest>(viewResult.Model);
        Assert.Equal(user, viewResult.ViewData["User"]);
    }

    [Fact]
    public async Task Create_Post_WhenNoUserLoggedIn_RedirectsToLogin()
    {
        // Arrange
        var newHelpRequest = new HelpRequest
        {
            Title = "Test Request",
            Description = "Test Description",
            EmergencyLevel = EmergencyLevel.Medium,
            Location = "Test Location",
            Status = HelpRequestStatus.Active
        };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

        // Act
        var result = await _controller.Create(newHelpRequest, null);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("Account", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Create_Post_WhenValidModel_SendsEmailsToAllVetsAndRedirectsToIndex()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes(username); return true; });
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);

        var newHelpRequest = new HelpRequest
        {
            Id = 100,
            Title = "Test Request",
            Description = "Test Description",
            EmergencyLevel = EmergencyLevel.Medium,
            Location = "Test Location",
            ContactName = "John Doe",
            ContactPhone = "1234567890",
            ContactEmail = "john@example.com",
            Status = HelpRequestStatus.Active
        };
        _helpRequestServiceMock.Setup(s => s.CreateHelpRequestAsync(It.IsAny<HelpRequest>())).Returns(Task.CompletedTask);

        var vetUser1 = new User { Id = 11, Email = "vet1@test.com" };
        var vetUser2 = new User { Id = 12, Email = "vet2@test.com" };
        var vets = new List<Veterinarian>
        {
            new Veterinarian { Id = 101, User = vetUser1 },
            new Veterinarian { Id = 102, User = vetUser2 }
        };
        _veterinarianServiceMock.Setup(v => v.GetAllVeterinariansAsync()).ReturnsAsync(vets);
        _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(newHelpRequest, null);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        _helpRequestServiceMock.Verify(s => s.CreateHelpRequestAsync(It.Is<HelpRequest>(hr =>
            hr.UserId == user.Id &&
            hr.Status == HelpRequestStatus.Active &&
            hr.Title == "Test Request"
        )), Times.Once());
        _emailServiceMock.Verify(e => e.SendEmailAsync("vet1@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        _emailServiceMock.Verify(e => e.SendEmailAsync("vet2@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task Create_Post_WhenInvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes(username); return true; });
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);

        var invalidHelpRequest = new HelpRequest
        {
            // Missing required fields: Title, Description, EmergencyLevel, Location
            ContactName = "John Doe"
        };
        _controller.ModelState.AddModelError("Title", "Title is required.");

        // Act
        var result = await _controller.Create(invalidHelpRequest, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(invalidHelpRequest, viewResult.Model);
        Assert.False(_controller.ModelState.IsValid);
        Assert.Equal(user, viewResult.ViewData["User"]);
    }

    [Fact]
    public async Task Index_Get_ReturnsViewWithHelpRequests()
    {
        // Arrange
        var helpRequests = new List<HelpRequest>
        {
            new HelpRequest { Id = 10, Title = "Req1", Description = "Desc1", EmergencyLevel = EmergencyLevel.Low, Location = "Loc1", Status = HelpRequestStatus.Active },
            new HelpRequest { Id = 20, Title = "Req2", Description = "Desc2", EmergencyLevel = EmergencyLevel.High, Location = "Loc2", Status = HelpRequestStatus.Active }
        };
        _helpRequestServiceMock.Setup(s => s.GetHelpRequestsAsync()).ReturnsAsync(helpRequests);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(helpRequests, viewResult.Model);
    }

    [Fact]
    public async Task Details_Get_WhenHelpRequestNotFound_ReturnsNotFound()
    {
        // Arrange
        _helpRequestServiceMock.Setup(s => s.GetHelpRequestByIdAsync(999)).ReturnsAsync((HelpRequest)null);

        // Act
        var result = await _controller.Details(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_Get_WhenHelpRequestFound_ReturnsViewWithCommentsAndViewBags()
    {
        // Arrange
        var hr = new HelpRequest
        {
            Id = 1,
            UserId = 2,
            Title = "Test Request",
            Description = "Test Description",
            EmergencyLevel = EmergencyLevel.Medium,
            Location = "Test Location",
            Status = HelpRequestStatus.Active
        };
        _helpRequestServiceMock.Setup(s => s.GetHelpRequestByIdAsync(1)).ReturnsAsync(hr);

        var comments = new List<Comment>
        {
            new Comment { Id = 10, Content = "Comment1" },
            new Comment { Id = 11, Content = "Comment2" }
        };
        _commentServiceMock.Setup(c => c.GetCommentsByHelpRequestIdAsync(1)).ReturnsAsync(comments);
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User)null);
        _veterinarianServiceMock.Setup(v => v.GetApprovedByUserIdAsync(It.IsAny<int>())).ReturnsAsync((Veterinarian)null);

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(hr, viewResult.Model);
        Assert.Equal(comments, hr.Comments);
        Assert.False((bool)viewResult.ViewData["CanEditOrDelete"]);
        Assert.False((bool)viewResult.ViewData["isVeterinarian"]);
    }

    [Fact]
    public async Task Details_Get_WhenUserLoggedIn_SetsCanEditOrDeleteComment()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        var hr = new HelpRequest
        {
            Id = 1,
            UserId = 2,
            Title = "Test Request",
            Description = "Test Description",
            EmergencyLevel = EmergencyLevel.Medium,
            Location = "Test Location",
            Status = HelpRequestStatus.Active
        };
        var comments = new List<Comment>
        {
            new Comment { Id = 10, Content = "Comment1", UserId = 1 },
            new Comment { Id = 11, Content = "Comment2", UserId = 2 }
        };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes(username); return true; });
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
        _helpRequestServiceMock.Setup(s => s.GetHelpRequestByIdAsync(1)).ReturnsAsync(hr);
        _commentServiceMock.Setup(c => c.GetCommentsByHelpRequestIdAsync(1)).ReturnsAsync(comments);
        _veterinarianServiceMock.Setup(v => v.GetApprovedByUserIdAsync(It.IsAny<int>())).ReturnsAsync((Veterinarian)null);

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(hr, viewResult.Model);
        Assert.Equal(comments, hr.Comments);
        Assert.False((bool)viewResult.ViewData["CanEditOrDelete"]);
        Assert.False((bool)viewResult.ViewData["isVeterinarian"]);
        Assert.Equal(new List<int> { 10 }, viewResult.ViewData["CanEditOrDeleteComment"]);
    }

    [Fact]
    public async Task Edit_Get_WhenNotLoggedIn_RedirectsToLogin()
    {
        // Arrange
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("Account", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Edit_Get_WhenHelpRequestDoesNotBelongToUser_ReturnsUnauthorized()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes(username); return true; });
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);

        var hr = new HelpRequest
        {
            Id = 10,
            UserId = 999,
            Title = "Test",
            Description = "Desc",
            EmergencyLevel = EmergencyLevel.Medium,
            Location = "Loc",
            Status = HelpRequestStatus.Active
        };
        _helpRequestServiceMock.Setup(s => s.GetHelpRequestByIdAsync(10)).ReturnsAsync(hr);

        // Act
        var result = await _controller.Edit(10);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task Edit_Get_WhenValid_ReturnsViewWithHelpRequest()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes(username); return true; });
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);

        var hr = new HelpRequest
        {
            Id = 10,
            UserId = 1,
            Title = "Test Request",
            Description = "Test Description",
            EmergencyLevel = EmergencyLevel.Medium,
            Location = "Test Location",
            Status = HelpRequestStatus.Active
        };
        _helpRequestServiceMock.Setup(s => s.GetHelpRequestByIdAsync(10)).ReturnsAsync(hr);

        // Act
        var result = await _controller.Edit(10);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(hr, viewResult.Model);
    }

    [Fact]
    public async Task Edit_Post_WhenValidModel_UpdatesAndSendsEmails()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes(username); return true; });
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);

        var existingRequest = new HelpRequest
        {
            Id = 10,
            UserId = 1,
            Title = "Old Title",
            Description = "Old Desc",
            EmergencyLevel = EmergencyLevel.Low,
            Location = "Old Loc",
            Status = HelpRequestStatus.Active
        };
        var updatedRequest = new HelpRequest
        {
            Id = 10,
            Title = "New Title",
            Description = "New Description",
            EmergencyLevel = EmergencyLevel.High,
            Location = "New Location",
            Status = HelpRequestStatus.Active
        };
        _helpRequestServiceMock.Setup(s => s.GetHelpRequestByIdAsync(10)).ReturnsAsync(existingRequest);
        _helpRequestServiceMock.Setup(s => s.UpdateHelpRequestAsync(It.IsAny<HelpRequest>())).Returns(Task.CompletedTask);

        var vetUser = new User { Id = 11, Email = "vet@test.com" };
        var vets = new List<Veterinarian> { new Veterinarian { Id = 101, User = vetUser } };
        _veterinarianServiceMock.Setup(v => v.GetAllVeterinariansAsync()).ReturnsAsync(vets);
        _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Edit(updatedRequest, null);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        _helpRequestServiceMock.Verify(s => s.UpdateHelpRequestAsync(It.Is<HelpRequest>(hr =>
            hr.Title == "New Title" &&
            hr.Description == "New Description" &&
            hr.EmergencyLevel == EmergencyLevel.High &&
            hr.Location == "New Location"
        )), Times.Once());
        _emailServiceMock.Verify(e => e.SendEmailAsync("vet@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task Edit_Post_WhenInvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes(username); return true; });
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);

        var existingRequest = new HelpRequest
        {
            Id = 10,
            UserId = 1,
            Title = "Old Title",
            Description = "Old Desc",
            EmergencyLevel = EmergencyLevel.Low,
            Location = "Old Loc",
            Status = HelpRequestStatus.Active
        };
        var invalidRequest = new HelpRequest
        {
            Id = 10,
            // Missing required fields
            ContactName = "John Doe"
        };
        _helpRequestServiceMock.Setup(s => s.GetHelpRequestByIdAsync(10)).ReturnsAsync(existingRequest);
        _controller.ModelState.AddModelError("Title", "Title is required.");

        // Act
        var result = await _controller.Edit(invalidRequest, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(invalidRequest, viewResult.Model);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Delete_Post_WhenUserNotOwner_ReturnsUnauthorized()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes(username); return true; });
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);

        var hr = new HelpRequest
        {
            Id = 30,
            UserId = 999,
            Title = "Test",
            Description = "Desc",
            EmergencyLevel = EmergencyLevel.Medium,
            Location = "Loc",
            Status = HelpRequestStatus.Active
        };
        _helpRequestServiceMock.Setup(s => s.GetHelpRequestByIdAsync(30)).ReturnsAsync(hr);

        // Act
        var result = await _controller.Delete(30);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task Delete_Post_WhenValid_DeletesAndSendsEmails()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes(username); return true; });
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);

        var hr = new HelpRequest
        {
            Id = 30,
            UserId = 1,
            Title = "Test Request",
            Description = "Test Description",
            EmergencyLevel = EmergencyLevel.Medium,
            Location = "Test Location",
            Status = HelpRequestStatus.Active
        };
        _helpRequestServiceMock.Setup(s => s.GetHelpRequestByIdAsync(30)).ReturnsAsync(hr);
        _helpRequestServiceMock.Setup(s => s.DeleteHelpRequestAsync(30)).Returns(Task.CompletedTask);

        var vetUser = new User { Id = 11, Email = "vet@test.com" };
        var vets = new List<Veterinarian> { new Veterinarian { Id = 101, User = vetUser } };
        _veterinarianServiceMock.Setup(v => v.GetAllVeterinariansAsync()).ReturnsAsync(vets);
        _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(30);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        _helpRequestServiceMock.Verify(s => s.DeleteHelpRequestAsync(30), Times.Once());
        _emailServiceMock.Verify(e => e.SendEmailAsync("vet@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task AddComment_Post_WhenUserNotLoggedIn_RedirectsToLogin()
    {
        // Arrange
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

        // Act
        var result = await _controller.AddComment(1, "comment text");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("Account", redirectResult.ControllerName);
    }

    [Fact]
    public async Task AddComment_Post_WhenHelpRequestNotFound_ReturnsNotFound()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes(username); return true; });
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
        _helpRequestServiceMock.Setup(s => s.GetHelpRequestByIdAsync(999)).ReturnsAsync((HelpRequest)null);

        // Act
        var result = await _controller.AddComment(999, "comment text");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddComment_Post_WhenUserLoggedIn_AddsCommentAndRedirects()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes(username); return true; });
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);

        var hr = new HelpRequest
        {
            Id = 100,
            UserId = 1,
            Title = "Test Request",
            Description = "Test Description",
            EmergencyLevel = EmergencyLevel.Medium,
            Location = "Test Location",
            Status = HelpRequestStatus.Active
        };
        _helpRequestServiceMock.Setup(s => s.GetHelpRequestByIdAsync(100)).ReturnsAsync(hr);
        _veterinarianServiceMock.Setup(v => v.GetByUserIdAsync(1)).ReturnsAsync((Veterinarian)null);
        _commentServiceMock.Setup(c => c.AddCommentAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddComment(100, "My new comment");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
        Assert.Equal("HelpRequest", redirectResult.ControllerName);
        _commentServiceMock.Verify(c => c.AddCommentAsync(It.Is<Comment>(cm =>
            cm.HelpRequestId == 100 &&
            cm.UserId == 1 &&
            cm.VeterinarianId == null &&
            cm.Content == "My new comment"
        )), Times.Once());
    }

    [Fact]
    public async Task EditComment_Get_WhenNotLoggedIn_RedirectsToLogin()
    {
        // Arrange
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

        // Act
        var result = await _controller.EditComment(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("Account", redirectResult.ControllerName);
    }

    [Fact]
    public async Task EditComment_Get_WhenCommentNotFound_ReturnsNotFound()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes(username); return true; });
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
        _commentServiceMock.Setup(c => c.GetCommentByIdAsync(999)).ReturnsAsync((Comment)null);

        // Act
        var result = await _controller.EditComment(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditComment_Get_WhenValid_ReturnsViewWithComment()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes(username); return true; });
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);

        var comment = new Comment { Id = 1, UserId = 1, HelpRequestId = 100, Content = "Test Comment" };
        _commentServiceMock.Setup(c => c.GetCommentByIdAsync(1)).ReturnsAsync(comment);

        // Act
        var result = await _controller.EditComment(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(comment, viewResult.Model);
    }

    [Fact]
    public async Task EditComment_Post_WhenValid_UpdatesCommentAndRedirects()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes(username); return true; });
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);

        var comment = new Comment { Id = 1, UserId = 1, HelpRequestId = 100, Content = "Old Comment" };
        _commentServiceMock.Setup(c => c.GetCommentByIdAsync(1)).ReturnsAsync(comment);
        _veterinarianServiceMock.Setup(v => v.GetByUserIdAsync(1)).ReturnsAsync((Veterinarian)null);
        _commentServiceMock.Setup(c => c.UpdateCommentAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.EditComment(1, 100, "New Comment");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
        Assert.Equal(100, redirectResult.RouteValues["id"]);
        _commentServiceMock.Verify(c => c.UpdateCommentAsync(It.Is<Comment>(cm => cm.Content == "New Comment")), Times.Once());
    }

    [Fact]
    public async Task EditComment_Post_WhenInvalidContent_ReturnsViewWithComment()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes(username); return true; });
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);

        var comment = new Comment { Id = 1, UserId = 1, HelpRequestId = 100, Content = "Old Comment" };
        _commentServiceMock.Setup(c => c.GetCommentByIdAsync(1)).ReturnsAsync(comment);
        _controller.ModelState.AddModelError("content", "Content cannot be empty.");

        // Act
        var result = await _controller.EditComment(1, 100, "");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(comment, viewResult.Model);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task DeleteComment_Post_WhenUserNotOwner_ReturnsUnauthorized()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes(username); return true; });
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);

        var comment = new Comment { Id = 1, UserId = 999, HelpRequestId = 100 };
        _commentServiceMock.Setup(c => c.GetCommentByIdAsync(1)).ReturnsAsync(comment);

        // Act
        var result = await _controller.DeleteComment(1);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task DeleteComment_Post_WhenValid_DeletesCommentAndRedirects()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) => { value = Encoding.UTF8.GetBytes(username); return true; });
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);

        var comment = new Comment { Id = 1, UserId = 1, HelpRequestId = 100 };
        _commentServiceMock.Setup(c => c.GetCommentByIdAsync(1)).ReturnsAsync(comment);
        _commentServiceMock.Setup(c => c.DeleteCommentAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteComment(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
        Assert.Equal(100, redirectResult.RouteValues["id"]);
        _commentServiceMock.Verify(c => c.DeleteCommentAsync(1), Times.Once());
    }
}