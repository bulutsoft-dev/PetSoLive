using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;
using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Interfaces;
using PetSoLive.Web.Controllers;
using Xunit;

namespace PetSoLive.Tests.Controllers
{
    public class VeterinarianControllerTests
    {
        private readonly Mock<IServiceManager> _serviceManagerMock;
        private readonly Mock<IStringLocalizer<VeterinarianController>> _localizerMock;
        private readonly Mock<IVeterinarianService> _veterinarianServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IAdminService> _adminServiceMock;
        private readonly Mock<ISession> _sessionMock;
        private readonly VeterinarianController _controller;
        private readonly DefaultHttpContext _httpContext;

        public VeterinarianControllerTests()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _localizerMock = new Mock<IStringLocalizer<VeterinarianController>>();
            _veterinarianServiceMock = new Mock<IVeterinarianService>();
            _userServiceMock = new Mock<IUserService>();
            _adminServiceMock = new Mock<IAdminService>();
            _sessionMock = new Mock<ISession>();

            // Setup IServiceManager
            _serviceManagerMock.SetupGet(m => m.VeterinarianService).Returns(_veterinarianServiceMock.Object);
            _serviceManagerMock.SetupGet(m => m.UserService).Returns(_userServiceMock.Object);
            _serviceManagerMock.SetupGet(m => m.AdminService).Returns(_adminServiceMock.Object);

            // Setup IStringLocalizer
            _localizerMock.Setup(l => l[It.IsAny<string>()])
                .Returns<string>(name => new LocalizedString(name, name));
            _localizerMock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
                .Returns<string, object[]>((name, args) => new LocalizedString(name, string.Format(name, args)));

            // Setup HttpContext and Session
            _httpContext = new DefaultHttpContext
            {
                Session = _sessionMock.Object
            };
            _controller = new VeterinarianController(_serviceManagerMock.Object, _localizerMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContext
                }
            };
            // TempData initialization for tests
            _controller.TempData = new TempDataDictionary(_httpContext, Mock.Of<ITempDataProvider>());
        }

        #region Register (GET)
        [Fact]
        public async Task Register_Get_WhenNotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

            // Act
            var result = await _controller.Register();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Register_Get_WhenUserNotFound_RedirectsToLogin()
        {
            // Arrange
            var username = "TestUser";
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.Register();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
            Assert.Equal("UserNotFound", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Register_Get_WhenNoExistingApplication_ReturnsView()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _veterinarianServiceMock.Setup(v => v.GetByUserIdAsync(user.Id)).ReturnsAsync((Veterinarian)null);

            // Act
            var result = await _controller.Register();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.False(viewResult.ViewData.ContainsKey("ApplicationSubmitted"));
        }

        [Fact]
        public async Task Register_Get_WhenExistingApplication_SetsViewDataFlag()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var existingVet = new Veterinarian { Id = 5, UserId = user.Id };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _veterinarianServiceMock.Setup(v => v.GetByUserIdAsync(user.Id)).ReturnsAsync(existingVet);

            // Act
            var result = await _controller.Register();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.True((bool)viewResult.ViewData["ApplicationSubmitted"]);
        }
        #endregion

        #region Register (POST)
        [Fact]
        public async Task Register_Post_WhenNotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

            // Act
            var result = await _controller.Register(10, "Quals", "Address", "555-1234");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Register_Post_WhenUserNotFound_RedirectsToLogin()
        {
            // Arrange
            var username = "TestUser";
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.Register(10, "Quals", "Address", "555-1234");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
            Assert.Equal("UserNotFound", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Register_Post_WhenUserIdMismatch_RedirectsToError()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);

            // Act
            var result = await _controller.Register(999, "Quals", "Address", "555-1234");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Error", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
            Assert.Equal("UnauthorizedAction", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Register_Post_WhenExistingApplication_ReturnsViewWithError()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var existingVet = new Veterinarian { Id = 5, UserId = user.Id };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _veterinarianServiceMock.Setup(v => v.GetByUserIdAsync(user.Id)).ReturnsAsync(existingVet);

            // Act
            var result = await _controller.Register(user.Id, "Quals", "Address", "555-1234");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.False(_controller.ModelState.IsValid);
            var error = _controller.ModelState[""].Errors[0];
            Assert.Equal("ExistingApplication", error.ErrorMessage);
        }

        [Fact]
        public async Task Register_Post_WhenInvalidInputs_ReturnsViewWithErrors()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _veterinarianServiceMock.Setup(v => v.GetByUserIdAsync(user.Id)).ReturnsAsync((Veterinarian)null);

            // Invalid inputs: empty qualifications, too long address, invalid phone
            var qualifications = "";
            var clinicAddress = new string('A', 201); // Exceeds 200 chars
            var clinicPhoneNumber = "invalid"; // Invalid format

            // Act
            var result = await _controller.Register(user.Id, qualifications, clinicAddress, clinicPhoneNumber);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains(_controller.ModelState["qualifications"].Errors, e => e.ErrorMessage == "QualificationsRequired");
            Assert.Contains(_controller.ModelState["clinicAddress"].Errors, e => e.ErrorMessage == "ClinicAddressTooLong");
            Assert.Contains(_controller.ModelState["clinicPhoneNumber"].Errors, e => e.ErrorMessage == "InvalidPhoneFormat");
        }

        [Fact]
        public async Task Register_Post_WhenValid_RegistersAndRedirects()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _veterinarianServiceMock.Setup(v => v.GetByUserIdAsync(user.Id)).ReturnsAsync((Veterinarian)null);
            _veterinarianServiceMock.Setup(v => v.RegisterVeterinarianAsync(user.Id, "Quals", "Address", "555-1234"))
                .ReturnsAsync(new Veterinarian { Id = 5, UserId = user.Id, Qualifications = "Quals", ClinicAddress = "Address", ClinicPhoneNumber = "555-1234" });

            // Act
            var result = await _controller.Register(user.Id, "Quals", "Address", "555-1234");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.Register), redirectResult.ActionName);
            _veterinarianServiceMock.Verify(v => v.RegisterVeterinarianAsync(user.Id, "Quals", "Address", "555-1234"), Times.Once());
            Assert.Equal("ApplicationSubmitted", _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task Register_Post_WhenServiceThrows_ReturnsViewWithError()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _veterinarianServiceMock.Setup(v => v.GetByUserIdAsync(user.Id)).ReturnsAsync((Veterinarian)null);
            _veterinarianServiceMock.Setup(v => v.RegisterVeterinarianAsync(user.Id, "Quals", "Address", "555-1234"))
                .ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.Register(user.Id, "Quals", "Address", "555-1234");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.False(_controller.ModelState.IsValid);
            var error = _controller.ModelState[""].Errors[0];
            Assert.StartsWith("RegistrationFailed", error.ErrorMessage);
        }
        #endregion

        #region Index
        [Fact]
        public async Task Index_WhenNotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

            // Act
            var result = await _controller.Index();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Index_WhenUserNotFound_RedirectsToLogin()
        {
            // Arrange
            var username = "TestUser";
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.Index();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
            Assert.Equal("UserNotFound", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Index_WhenNotAdmin_RedirectsToError()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 1, Username = username };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _adminServiceMock.Setup(a => a.IsUserAdminAsync(user.Id)).ReturnsAsync(false);

            // Act
            var result = await _controller.Index();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Error", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
            Assert.Equal("NotAuthorized", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Index_WhenAdmin_ReturnsViewWithVeterinarians()
        {
            // Arrange
            var username = "AdminUser";
            var user = new User { Id = 2, Username = username };
            var vetList = new List<Veterinarian>
            {
                new Veterinarian { Id = 10, UserId = 99 },
                new Veterinarian { Id = 11, UserId = 100 }
            };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _adminServiceMock.Setup(a => a.IsUserAdminAsync(user.Id)).ReturnsAsync(true);
            _veterinarianServiceMock.Setup(v => v.GetAllVeterinariansAsync()).ReturnsAsync(vetList);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.Equal(vetList, viewResult.Model);
        }
        #endregion

        #region Approve
        [Fact]
        public async Task Approve_WhenNotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

            // Act
            var result = await _controller.Approve(10);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Approve_WhenUserNotFound_RedirectsToLogin()
        {
            // Arrange
            var username = "TestUser";
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.Approve(10);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
            Assert.Equal("UserNotFound", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Approve_WhenNotAdmin_RedirectsToError()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 1, Username = username };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _adminServiceMock.Setup(a => a.IsUserAdminAsync(user.Id)).ReturnsAsync(false);

            // Act
            var result = await _controller.Approve(10);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Error", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
            Assert.Equal("NotAuthorizedApprove", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Approve_WhenVeterinarianNotFound_RedirectsToError()
        {
            // Arrange
            var username = "AdminUser";
            var user = new User { Id = 2, Username = username };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _adminServiceMock.Setup(a => a.IsUserAdminAsync(user.Id)).ReturnsAsync(true);
            _veterinarianServiceMock.Setup(v => v.GetByIdAsync(999)).ReturnsAsync((Veterinarian)null);

            // Act
            var result = await _controller.Approve(999);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Error", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
            Assert.Equal("VeterinarianNotFound", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Approve_WhenPending_CallsServiceAndRedirects()
        {
            // Arrange
            var username = "AdminUser";
            var user = new User { Id = 2, Username = username };
            var pendingVet = new Veterinarian { Id = 10, Status = VeterinarianStatus.Pending };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _adminServiceMock.Setup(a => a.IsUserAdminAsync(user.Id)).ReturnsAsync(true);
            _veterinarianServiceMock.Setup(v => v.GetByIdAsync(10)).ReturnsAsync(pendingVet);
            _veterinarianServiceMock.Setup(v => v.ApproveVeterinarianAsync(10)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Approve(10);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
            _veterinarianServiceMock.Verify(v => v.ApproveVeterinarianAsync(10), Times.Once());
            Assert.Equal("VeterinarianApproved", _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task Approve_WhenNotPending_ReturnsError()
        {
            // Arrange
            var username = "AdminUser";
            var user = new User { Id = 2, Username = username };
            var approvedVet = new Veterinarian { Id = 10, Status = VeterinarianStatus.Approved };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _adminServiceMock.Setup(a => a.IsUserAdminAsync(user.Id)).ReturnsAsync(true);
            _veterinarianServiceMock.Setup(v => v.GetByIdAsync(10)).ReturnsAsync(approvedVet);

            // Act
            var result = await _controller.Approve(10);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
            _veterinarianServiceMock.Verify(v => v.ApproveVeterinarianAsync(It.IsAny<int>()), Times.Never());
            Assert.Equal("InvalidVeterinarianStatus", _controller.TempData["ErrorMessage"]);
        }
        #endregion

        #region Reject
        [Fact]
        public async Task Reject_WhenNotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

            // Act
            var result = await _controller.Reject(10);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Reject_WhenUserNotFound_RedirectsToLogin()
        {
            // Arrange
            var username = "TestUser";
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.Reject(10);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
            Assert.Equal("UserNotFound", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Reject_WhenNotAdmin_RedirectsToError()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 1, Username = username };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _adminServiceMock.Setup(a => a.IsUserAdminAsync(user.Id)).ReturnsAsync(false);

            // Act
            var result = await _controller.Reject(10);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Error", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
            Assert.Equal("NotAuthorizedReject", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Reject_WhenVeterinarianNotFound_RedirectsToError()
        {
            // Arrange
            var username = "AdminUser";
            var user = new User { Id = 2, Username = username };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _adminServiceMock.Setup(a => a.IsUserAdminAsync(user.Id)).ReturnsAsync(true);
            _veterinarianServiceMock.Setup(v => v.GetByIdAsync(999)).ReturnsAsync((Veterinarian)null);

            // Act
            var result = await _controller.Reject(999);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Error", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
            Assert.Equal("VeterinarianNotFound", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Reject_WhenPending_CallsServiceAndRedirects()
        {
            // Arrange
            var username = "AdminUser";
            var user = new User { Id = 2, Username = username };
            var pendingVet = new Veterinarian { Id = 20, Status = VeterinarianStatus.Pending };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _adminServiceMock.Setup(a => a.IsUserAdminAsync(user.Id)).ReturnsAsync(true);
            _veterinarianServiceMock.Setup(v => v.GetByIdAsync(20)).ReturnsAsync(pendingVet);
            _veterinarianServiceMock.Setup(v => v.RejectVeterinarianAsync(20)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Reject(20);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
            _veterinarianServiceMock.Verify(v => v.RejectVeterinarianAsync(20), Times.Once());
            Assert.Equal("VeterinarianRejected", _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task Reject_WhenNotPending_ReturnsError()
        {
            // Arrange
            var username = "AdminUser";
            var user = new User { Id = 2, Username = username };
            var rejectedVet = new Veterinarian { Id = 21, Status = VeterinarianStatus.Rejected };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _adminServiceMock.Setup(a => a.IsUserAdminAsync(user.Id)).ReturnsAsync(true);
            _veterinarianServiceMock.Setup(v => v.GetByIdAsync(21)).ReturnsAsync(rejectedVet);

            // Act
            var result = await _controller.Reject(21);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
            _veterinarianServiceMock.Verify(v => v.RejectVeterinarianAsync(It.IsAny<int>()), Times.Never());
            Assert.Equal("InvalidVeterinarianStatus", _controller.TempData["ErrorMessage"]);
        }
        #endregion
    }
}