using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

// Adjust namespaces to your own project structure
using PetSoLive.Business.Services;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Interfaces;
using PetSoLive.Web.Controllers;

namespace PetSoLive.Tests.Controllers;

public class HelpRequestsControlerTests
{
 private readonly Mock<IHelpRequestService> _helpRequestServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IVeterinarianService> _veterinarianServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ICommentService> _commentServiceMock;

    private readonly HelpRequestController _controller;
    private readonly DefaultHttpContext _httpContext;

    public HelpRequestsControlerTests()
    {
        _helpRequestServiceMock = new Mock<IHelpRequestService>();
        _userServiceMock = new Mock<IUserService>();
        _veterinarianServiceMock = new Mock<IVeterinarianService>();
        _emailServiceMock = new Mock<IEmailService>();
        _commentServiceMock = new Mock<ICommentService>();

        _controller = new HelpRequestController(
            _helpRequestServiceMock.Object,
            _userServiceMock.Object,
            _emailServiceMock.Object,
            _veterinarianServiceMock.Object,
            _commentServiceMock.Object,
            null // IMapper is not used in the controller
        );

        // Setup a fake HTTP context with a test session
        _httpContext = new DefaultHttpContext
        {
            Session = new TestSession()
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContext
        };
    }

    [Fact]
    public async Task Create_Get_WhenNoUserLoggedIn_RedirectsToLogin()
    {
        // Arrange
        // Session has no "Username"

        // Act
        var result = await _controller.Create() as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Login", result.ActionName);
        Assert.Equal("Account", result.ControllerName);
    }

    [Fact]
    public async Task Create_Get_WhenUserLoggedIn_ReturnsView()
    {
        // Arrange
        _httpContext.Session.SetString("Username", "TestUser");

        var user = new User { Id = 1, Username = "TestUser" };
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync("TestUser"))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Create() as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.ViewName); // default view
        Assert.IsType<HelpRequest>(result.Model);
        Assert.Equal(user, _controller.ViewBag.User);
    }

    [Fact]
    public async Task Create_Post_WhenNoUserLoggedIn_RedirectsToLogin()
    {
        // Arrange
        var newHelpRequest = new HelpRequest { Title = "Test Request" };
        // No "Username" in session => not logged in

        // Act
        var result = await _controller.Create(newHelpRequest) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Login", result.ActionName);
        Assert.Equal("Account", result.ControllerName);
    }

    [Fact]
    public async Task Create_Post_WhenValidModel_SendsEmailsToAllVetsAndRedirectsToIndex()
    {
        // Arrange
        _httpContext.Session.SetString("Username", "TestUser");
        var user = new User { Id = 1, Username = "TestUser" };
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync("TestUser")).ReturnsAsync(user);

        var newHelpRequest = new HelpRequest { Id = 100, Title = "Test Request" };

        // Suppose we have 2 approved vets
        var vetUser1 = new User { Id = 11, Email = "vet1@test.com" };
        var vetUser2 = new User { Id = 12, Email = "vet2@test.com" };
        var vets = new List<Veterinarian>
        {
            new Veterinarian { Id = 101, User = vetUser1 },
            new Veterinarian { Id = 102, User = vetUser2 }
        };
        _veterinarianServiceMock.Setup(v => v.GetAllVeterinariansAsync())
            .ReturnsAsync(vets);

        // Act
        var result = await _controller.Create(newHelpRequest) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);

        // Verify help request creation
        _helpRequestServiceMock.Verify(s => s.CreateHelpRequestAsync(It.Is<HelpRequest>(
            hr => hr.UserId == user.Id && hr.Status == HelpRequestStatus.Active)), Times.Once);

        // Verify emails sent
        _emailServiceMock.Verify(e => e.SendEmailAsync("vet1@test.com",
            It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendEmailAsync("vet2@test.com",
            It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Index_Get_ReturnsViewWithHelpRequests()
    {
        // Arrange
        var helpRequests = new List<HelpRequest>
        {
            new HelpRequest { Id = 10, Title = "Req1" },
            new HelpRequest { Id = 20, Title = "Req2" }
        };
        _helpRequestServiceMock.Setup(s => s.GetHelpRequestsAsync())
            .ReturnsAsync(helpRequests);

        // Act
        var result = await _controller.Index() as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(helpRequests, result.Model);
    }

    [Fact]
    public async Task Details_Get_WhenHelpRequestNotFound_ReturnsNotFound()
    {
        // Arrange
        _helpRequestServiceMock.Setup(s => s.GetHelpRequestByIdAsync(999))
            .ReturnsAsync((HelpRequest)null);

        // Act
        var result = await _controller.Details(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_Get_WhenHelpRequestFound_ReturnsViewWithCommentsAndViewBags()
    {
        // Arrange
        var hr = new HelpRequest { Id = 1, UserId = 2 };
        _helpRequestServiceMock.Setup(s => s.GetHelpRequestByIdAsync(1))
            .ReturnsAsync(hr);

        var comments = new List<Comment>
        {
            new Comment { Id = 10, Content = "Comment1" },
            new Comment { Id = 11, Content = "Comment2" }
        };
        _commentServiceMock.Setup(c => c.GetCommentsByHelpRequestIdAsync(1))
            .ReturnsAsync(comments);

        // Not logged in by default => user == null
        // Act
        var result = await _controller.Details(1) as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(hr, result.Model);
        Assert.Equal(comments, hr.Comments);
        // Check we are setting some ViewBag properties
        Assert.False((bool)_controller.ViewBag.CanEditOrDelete);
        Assert.False((bool)_controller.ViewBag.isVeterinarian);
    }

    [Fact]
    public async Task Edit_Get_WhenNotLoggedIn_RedirectsToLogin()
    {
        // Arrange
        // No session user => not logged in
        // Act
        var result = await _controller.Edit(1) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Login", result.ActionName);
        Assert.Equal("Account", result.ControllerName);
    }

    [Fact]
    public async Task Edit_Get_WhenHelpRequestDoesNotBelongToUser_ReturnsUnauthorized()
    {
        // Arrange
        _httpContext.Session.SetString("Username", "TestUser");
        var user = new User { Id = 1, Username = "TestUser" };
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync("TestUser"))
            .ReturnsAsync(user);

        var hr = new HelpRequest { Id = 10, UserId = 999 }; // belongs to someone else
        _helpRequestServiceMock.Setup(s => s.GetHelpRequestByIdAsync(10))
            .ReturnsAsync(hr);

        // Act
        var result = await _controller.Edit(10) as StatusCodeResult;

        // Assert
        Assert.NotNull(result);
        // The controller returns Unauthorized() => 401
        Assert.Equal(401, result.StatusCode);
    }

    [Fact]
    public async Task Delete_Post_WhenUserNotOwner_ReturnsUnauthorized()
    {
        // Arrange
        _httpContext.Session.SetString("Username", "TestUser");
        var user = new User { Id = 1, Username = "TestUser" };
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync("TestUser"))
            .ReturnsAsync(user);

        var hr = new HelpRequest { Id = 30, UserId = 999 };
        _helpRequestServiceMock.Setup(s => s.GetHelpRequestByIdAsync(30))
            .ReturnsAsync(hr);

        // Act
        var result = await _controller.Delete(30) as StatusCodeResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(401, result.StatusCode); // Unauthorized
    }

    [Fact]
    public async Task AddComment_Post_WhenUserNotLoggedIn_RedirectsToLogin()
    {
        // Arrange
        // not logged in => no "Username"
        // Act
        var result = await _controller.AddComment(1, "comment text") as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Login", result.ActionName);
        Assert.Equal("Account", result.ControllerName);
    }

    [Fact]
    public async Task AddComment_Post_WhenHelpRequestNotFound_ReturnsNotFound()
    {
        // Arrange
        _httpContext.Session.SetString("Username", "TestUser");
        var user = new User { Id = 1, Username = "TestUser" };
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync("TestUser"))
            .ReturnsAsync(user);

        // helpRequest is null
        _helpRequestServiceMock.Setup(s => s.GetHelpRequestByIdAsync(999))
            .ReturnsAsync((HelpRequest)null);

        // Act
        var result = await _controller.AddComment(999, "comment text");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddComment_Post_WhenUserLoggedIn_AddsCommentAndRedirects()
    {
        // Arrange
        _httpContext.Session.SetString("Username", "TestUser");
        var user = new User { Id = 1, Username = "TestUser" };
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync("TestUser"))
            .ReturnsAsync(user);

        var hr = new HelpRequest { Id = 100, UserId = 1 };
        _helpRequestServiceMock.Setup(s => s.GetHelpRequestByIdAsync(100)).ReturnsAsync(hr);

        // Suppose user is not a veterinarian => veterinarian is null
        _veterinarianServiceMock.Setup(v => v.GetByUserIdAsync(1)).ReturnsAsync((Veterinarian)null);

        // Act
        var result = await _controller.AddComment(100, "My new comment") as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Details", result.ActionName);
        Assert.Equal("HelpRequest", result.ControllerName);

        // Verify the comment was added
        _commentServiceMock.Verify(c => c.AddCommentAsync(It.Is<Comment>(cm =>
            cm.HelpRequestId == 100 &&
            cm.UserId == 1 &&
            cm.VeterinarianId == null &&
            cm.Content == "My new comment"
        )), Times.Once);
    }
}

#region TestSession

/// <summary>
/// A simple in-memory ISession for test usage.
/// </summary>
public class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> _storage = new Dictionary<string, byte[]>();

    public bool IsAvailable => true;
    public string Id => Guid.NewGuid().ToString();
    public IEnumerable<string> Keys => _storage.Keys;

    public void Clear() => _storage.Clear();

    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Remove(string key)
    {
        if (_storage.ContainsKey(key))
            _storage.Remove(key);
    }

    public void Set(string key, byte[] value) => _storage[key] = value;

    public bool TryGetValue(string key, out byte[] value) => _storage.TryGetValue(key, out value);
}
#endregion