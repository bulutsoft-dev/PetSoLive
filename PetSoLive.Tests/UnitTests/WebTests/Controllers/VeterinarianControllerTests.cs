
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

// Adjust these namespaces to match your actual project
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Interfaces;
using PetSoLive.Web.Controllers;
namespace PetSoLive.Tests.Controllers;

public class VeterinarianControllerTests
{
private readonly Mock<IVeterinarianService> _veterinarianServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IAdminService> _adminServiceMock;

        private readonly VeterinarianController _controller;
        private readonly DefaultHttpContext _httpContext;

        public VeterinarianControllerTests()
        {
            _veterinarianServiceMock = new Mock<IVeterinarianService>();
            _userServiceMock = new Mock<IUserService>();
            _adminServiceMock = new Mock<IAdminService>();

            _controller = new VeterinarianController(
                _veterinarianServiceMock.Object,
                _userServiceMock.Object,
                _adminServiceMock.Object,
                null
            );

            _httpContext = new DefaultHttpContext
            {
                Session = new VeterinarianTestSession() // custom test session
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };
        }

        #region Register (GET)
        [Fact]
        public async Task Register_Get_WhenNotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            // user is not logged in => no Username in session

            // Act
            var result = await _controller.Register() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Account", result.ControllerName);
        }

        [Fact]
        public async Task Register_Get_WhenUserLoggedInAndNoExistingApplication_ReturnsView()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "TestUser");
            var user = new User { Id = 10, Username = "TestUser" };

            // Mock user retrieval
            _userServiceMock
                .Setup(s => s.GetUserByUsernameAsync("TestUser"))
                .ReturnsAsync(user);

            // No existing vet application
            _veterinarianServiceMock
                .Setup(v => v.GetByUserIdAsync(user.Id))
                .ReturnsAsync((Veterinarian)null);

            // Act
            var result = await _controller.Register() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ViewName); // default view
            Assert.False(_controller.ViewBag.ApplicationSubmitted == true);
        }

        [Fact]
        public async Task Register_Get_WhenUserHasExistingApplication_SetsViewBagFlag()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "TestUser");
            var user = new User { Id = 10, Username = "TestUser" };

            _userServiceMock
                .Setup(s => s.GetUserByUsernameAsync("TestUser"))
                .ReturnsAsync(user);

            // Vet application already exists
            var existingVet = new Veterinarian { Id = 5, UserId = user.Id };
            _veterinarianServiceMock
                .Setup(v => v.GetByUserIdAsync(user.Id))
                .ReturnsAsync(existingVet);

            // Act
            var result = await _controller.Register() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.True((bool)_controller.ViewBag.ApplicationSubmitted);
        }
        #endregion

        #region Register (POST)
        [Fact]
        public async Task Register_Post_WhenNotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            // not logged in => no Username
            var userId = 10;

            // Act
            var result = await _controller.Register(userId, "Quals", "Address", "555-1234") as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Account", result.ControllerName);
        }

        [Fact]
        public async Task Register_Post_WhenUserHasExistingApp_ReturnsErrorInModelState()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "TestUser");
            var user = new User { Id = 10, Username = "TestUser" };

            _userServiceMock.Setup(u => u.GetUserByUsernameAsync("TestUser"))
                .ReturnsAsync(user);

            _veterinarianServiceMock.Setup(v => v.GetByUserIdAsync(10))
                .ReturnsAsync(new Veterinarian { Id = 5, UserId = 10 });

            // Act
            var result = await _controller.Register(10, "Quals", "Address", "Phone") as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(_controller.ModelState.IsValid); 
            Assert.True(_controller.ModelState.ContainsKey(""));
        }

        [Fact]
        public async Task Register_Post_WhenModelIsValid_RegistersVetAndRedirects()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "TestUser");
            var user = new User { Id = 10, Username = "TestUser" };
            _userServiceMock
                .Setup(u => u.GetUserByUsernameAsync("TestUser"))
                .ReturnsAsync(user);

            _veterinarianServiceMock
                .Setup(v => v.GetByUserIdAsync(10))
                .ReturnsAsync((Veterinarian)null); // no existing application

            // We can consider model to be valid because we pass in plausible parameters
            _controller.ModelState.Clear();

            // Act
            var result = await _controller.Register(10, "My Quals", "My Clinic", "555-1234") as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(VeterinarianController.Register), result.ActionName);

            _veterinarianServiceMock.Verify(v => v.RegisterVeterinarianAsync(
                user.Id, "My Quals", "My Clinic", "555-1234"), Times.Once);
        }
        #endregion

        #region Index
        [Fact]
        public async Task Index_WhenNotLoggedIn_RedirectsToLogin()
        {
            // Arrange: no username in session

            // Act
            var result = await _controller.Index() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Account", result.ControllerName);
        }

        [Fact]
        public async Task Index_WhenNotAdmin_RedirectsToErrorWithMessage()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "TestUser");
            var user = new User { Id = 1, Username = "TestUser" };

            _userServiceMock.Setup(u => u.GetUserByUsernameAsync("TestUser"))
                .ReturnsAsync(user);

            // Not admin
            _adminServiceMock.Setup(a => a.IsUserAdminAsync(user.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Index() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Error", result.ActionName);
            Assert.Equal("Home", result.ControllerName);
            Assert.Equal("You are not authorized to view this page.", _controller.ViewBag.ErrorMessage);
        }

        [Fact]
        public async Task Index_WhenAdmin_ReturnsViewWithVets()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "AdminUser");
            var user = new User { Id = 2, Username = "AdminUser" };
            _userServiceMock.Setup(u => u.GetUserByUsernameAsync("AdminUser"))
                .ReturnsAsync(user);

            _adminServiceMock.Setup(a => a.IsUserAdminAsync(user.Id))
                .ReturnsAsync(true);

            var vetList = new List<Veterinarian>
            {
                new Veterinarian { Id = 10, UserId = 99 },
                new Veterinarian { Id = 11, UserId = 100 }
            };
            _veterinarianServiceMock.Setup(v => v.GetAllVeterinariansAsync())
                .ReturnsAsync(vetList);

            // Act
            var result = await _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(vetList, result.Model);
        }
        #endregion

        #region Approve
        [Fact]
        public async Task Approve_WhenNotLoggedIn_RedirectsToLogin()
        {
            // Arrange: no username in session

            // Act
            var result = await _controller.Approve(10) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Account", result.ControllerName);
        }

        [Fact]
        public async Task Approve_WhenNotAdmin_RedirectsToError()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "TestUser");
            var user = new User { Id = 1 };
            _userServiceMock.Setup(u => u.GetUserByUsernameAsync("TestUser"))
                .ReturnsAsync(user);

            _adminServiceMock.Setup(a => a.IsUserAdminAsync(1)).ReturnsAsync(false);

            // Act
            var result = await _controller.Approve(10) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Error", result.ActionName);
            Assert.Equal("Home", result.ControllerName);
            Assert.Equal("You are not authorized to approve veterinarians.", _controller.ViewBag.ErrorMessage);
        }

        [Fact]
        public async Task Approve_WhenVetNotFound_RedirectsToError()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "AdminUser");
            var user = new User { Id = 2 };
            _userServiceMock.Setup(u => u.GetUserByUsernameAsync("AdminUser"))
                .ReturnsAsync(user);

            _adminServiceMock.Setup(a => a.IsUserAdminAsync(2)).ReturnsAsync(true);

            _veterinarianServiceMock.Setup(v => v.GetByIdAsync(999))
                .ReturnsAsync((Veterinarian)null);

            // Act
            var result = await _controller.Approve(999) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Error", result.ActionName);
            Assert.Equal("Home", result.ControllerName);
            Assert.Equal("Veterinarian not found.", _controller.ViewBag.ErrorMessage);
        }

        [Fact]
        public async Task Approve_WhenVetIsPending_CallsServiceApproveAndRedirects()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "AdminUser");
            var adminUser = new User { Id = 2 };
            _userServiceMock.Setup(u => u.GetUserByUsernameAsync("AdminUser"))
                .ReturnsAsync(adminUser);

            _adminServiceMock.Setup(a => a.IsUserAdminAsync(2))
                .ReturnsAsync(true);

            var pendingVet = new Veterinarian
            {
                Id = 10,
                Status = VeterinarianStatus.Pending
            };
            _veterinarianServiceMock.Setup(v => v.GetByIdAsync(10))
                .ReturnsAsync(pendingVet);

            // Act
            var result = await _controller.Approve(10) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(VeterinarianController.Index), result.ActionName);
            _veterinarianServiceMock.Verify(v => v.ApproveVeterinarianAsync(10), Times.Once);
        }

        [Fact]
        public async Task Approve_WhenVetIsNotPending_NoActionTaken()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "AdminUser");
            var adminUser = new User { Id = 2 };
            _userServiceMock.Setup(u => u.GetUserByUsernameAsync("AdminUser"))
                .ReturnsAsync(adminUser);

            _adminServiceMock.Setup(a => a.IsUserAdminAsync(2)).ReturnsAsync(true);

            // Suppose the vet is already Approved
            var approvedVet = new Veterinarian
            {
                Id = 10,
                Status = VeterinarianStatus.Approved
            };
            _veterinarianServiceMock.Setup(v => v.GetByIdAsync(10)).ReturnsAsync(approvedVet);

            // Act
            var result = await _controller.Approve(10) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(VeterinarianController.Index), result.ActionName);

            // The service should not be called for an already approved vet
            _veterinarianServiceMock.Verify(v => v.ApproveVeterinarianAsync(It.IsAny<int>()), Times.Never);
        }
        #endregion

        #region Reject
        [Fact]
        public async Task Reject_WhenVetNotFound_RedirectsToError()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "AdminUser");
            var user = new User { Id = 10 };
            _userServiceMock.Setup(u => u.GetUserByUsernameAsync("AdminUser"))
                .ReturnsAsync(user);

            _adminServiceMock.Setup(a => a.IsUserAdminAsync(10)).ReturnsAsync(true);

            _veterinarianServiceMock.Setup(v => v.GetByIdAsync(999)).ReturnsAsync((Veterinarian)null);

            // Act
            var result = await _controller.Reject(999) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Error", result.ActionName);
            Assert.Equal("Home", result.ControllerName);
            Assert.Equal("Veterinarian not found.", _controller.ViewBag.ErrorMessage);
        }

        [Fact]
        public async Task Reject_WhenPendingVet_CallsServiceRejectAndRedirects()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "AdminUser");
            var user = new User { Id = 10 };
            _userServiceMock.Setup(u => u.GetUserByUsernameAsync("AdminUser"))
                .ReturnsAsync(user);

            _adminServiceMock.Setup(a => a.IsUserAdminAsync(10)).ReturnsAsync(true);

            var pendingVet = new Veterinarian { Id = 20, Status = VeterinarianStatus.Pending };
            _veterinarianServiceMock.Setup(v => v.GetByIdAsync(20))
                .ReturnsAsync(pendingVet);

            // Act
            var result = await _controller.Reject(20) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(VeterinarianController.Index), result.ActionName);
            _veterinarianServiceMock.Verify(v => v.RejectVeterinarianAsync(20), Times.Once);
        }

        [Fact]
        public async Task Reject_WhenVetIsNotPending_NoActionTaken()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "AdminUser");
            var user = new User { Id = 10 };
            _userServiceMock.Setup(u => u.GetUserByUsernameAsync("AdminUser"))
                .ReturnsAsync(user);

            _adminServiceMock.Setup(a => a.IsUserAdminAsync(10))
                .ReturnsAsync(true);

            // Vet already Rejected
            var rejectedVet = new Veterinarian
            {
                Id = 21,
                Status = VeterinarianStatus.Rejected
            };
            _veterinarianServiceMock.Setup(v => v.GetByIdAsync(21)).ReturnsAsync(rejectedVet);

            // Act
            var result = await _controller.Reject(21) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(VeterinarianController.Index), result.ActionName);

            // Should not call reject again
            _veterinarianServiceMock.Verify(v => v.RejectVeterinarianAsync(It.IsAny<int>()), Times.Never);
        }
        #endregion
    }

#region TestSession

/// <summary>
/// A simple test session class implementing ISession in-memory for controller tests.
/// </summary>
public class VeterinarianTestSession : ISession
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