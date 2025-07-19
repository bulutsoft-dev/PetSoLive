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
using PetSoLive.Core.Interfaces;
using PetSoLive.Web.Controllers;
using Xunit;
using PetSoLive.Web.Helpers;

namespace PetSoLive.Tests.Controllers
{
    public class PetControllerTests
    {
        private readonly Mock<IServiceManager> _serviceManagerMock;
        private readonly Mock<IStringLocalizer<PetController>> _localizerMock;
        private readonly Mock<IPetService> _petServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IAdoptionService> _adoptionServiceMock;
        private readonly Mock<IAdoptionRequestService> _adoptionRequestServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ISession> _sessionMock;
        private readonly Mock<ImgBBHelper> _imgBBHelperMock;
        private readonly PetController _controller;
        private readonly DefaultHttpContext _httpContext;

        public PetControllerTests()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _localizerMock = new Mock<IStringLocalizer<PetController>>();
            _petServiceMock = new Mock<IPetService>();
            _userServiceMock = new Mock<IUserService>();
            _adoptionServiceMock = new Mock<IAdoptionService>();
            _adoptionRequestServiceMock = new Mock<IAdoptionRequestService>();
            _emailServiceMock = new Mock<IEmailService>();
            _sessionMock = new Mock<ISession>();
            _imgBBHelperMock = new Mock<ImgBBHelper>("dummy_api_key");

            // Setup IServiceManager
            _serviceManagerMock.SetupGet(m => m.PetService).Returns(_petServiceMock.Object);
            _serviceManagerMock.SetupGet(m => m.UserService).Returns(_userServiceMock.Object);
            _serviceManagerMock.SetupGet(m => m.AdoptionService).Returns(_adoptionServiceMock.Object);
            _serviceManagerMock.SetupGet(m => m.AdoptionRequestService).Returns(_adoptionRequestServiceMock.Object);
            _serviceManagerMock.SetupGet(m => m.EmailService).Returns(_emailServiceMock.Object);

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
            _controller = new PetController(_serviceManagerMock.Object, _localizerMock.Object, _imgBBHelperMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContext
                }
            };
            // TempData initialization for tests
            _controller.TempData = new TempDataDictionary(_httpContext, Mock.Of<ITempDataProvider>());
        }

        #region Create (GET)
        [Fact]
        public void Create_Get_WhenUserNotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

            // Act
            var result = _controller.Create();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public void Create_Get_WhenUserLoggedIn_ReturnsView()
        {
            // Arrange
            var username = "TestUser";
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });

            // Act
            var result = _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }
        #endregion

        #region Create (POST)
        [Fact]
        public async Task Create_Post_WhenUserNotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            var pet = new Pet { Id = 1, Name = "Doggo" };
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

            // Act
            var result = await _controller.Create(pet, null);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Create_Post_WhenModelIsValid_CreatesPetAndRedirectsToIndex()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet { Id = 1, Name = "Doggo", IsNeutered = true };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.CreatePetAsync(pet)).Returns(Task.CompletedTask);
            _petServiceMock.Setup(s => s.AssignPetOwnerAsync(It.IsAny<PetOwner>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(pet, null);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Adoption", redirectResult.ControllerName);
            _petServiceMock.Verify(s => s.CreatePetAsync(pet), Times.Once());
            _petServiceMock.Verify(s => s.AssignPetOwnerAsync(It.Is<PetOwner>(
                po => po.UserId == user.Id && po.PetId == pet.Id
            )), Times.Once());
            Assert.Equal("PetCreatedSuccess", _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task Create_Post_WhenModelIsInvalid_ReturnsView()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet();
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _controller.Create(pet, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(pet, viewResult.Model);
            Assert.Equal("InvalidPetData", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Create_Post_WhenIsNeuteredNull_SetsToFalse()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet { Id = 1, Name = "Doggo", IsNeutered = null };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.CreatePetAsync(It.IsAny<Pet>())).Returns(Task.CompletedTask);
            _petServiceMock.Setup(s => s.AssignPetOwnerAsync(It.IsAny<PetOwner>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(pet, null);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Adoption", redirectResult.ControllerName);
            _petServiceMock.Verify(s => s.CreatePetAsync(It.Is<Pet>(p => p.IsNeutered == false)), Times.Once());
        }
        #endregion

        #region Details
        [Fact]
        public async Task Details_WhenPetNotFound_ReturnsErrorViewWithMessage()
        {
            // Arrange
            var petId = 999;
            _petServiceMock.Setup(s => s.GetPetByIdAsync(petId)).ThrowsAsync(new KeyNotFoundException("Pet not found."));

            // Act
            var result = await _controller.Details(petId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Error", viewResult.ViewName);
            Assert.Equal("PetNotFound", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Details_WhenPetFound_ReturnsViewWithPetAndViewData()
        {
            // Arrange
            var petId = 1;
            var pet = new Pet { Id = petId, Name = "Cat" };
            _petServiceMock.Setup(s => s.GetPetByIdAsync(petId)).ReturnsAsync(pet);
            _adoptionRequestServiceMock.Setup(r => r.GetAdoptionRequestsByPetIdAsync(petId))
                .ReturnsAsync(new List<AdoptionRequest>());
            _adoptionServiceMock.Setup(a => a.GetAdoptionByPetIdAsync(petId)).ReturnsAsync((Adoption)null);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

            // Act
            var result = await _controller.Details(petId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(pet, viewResult.Model);
            Assert.Equal("PetAvailable", viewResult.ViewData["AdoptionStatus"]);
            Assert.False((bool)viewResult.ViewData["IsUserLoggedIn"]);
            Assert.Null(viewResult.ViewData["Adoption"]);
            Assert.Empty((List<AdoptionRequest>)viewResult.ViewData["AdoptionRequests"]);
            Assert.False((bool)viewResult.ViewData["HasAdoptionRequest"]);
        }

        [Fact]
        public async Task Details_WhenPetAdopted_SetsAdoptionStatus()
        {
            // Arrange
            var petId = 1;
            var pet = new Pet { Id = petId, Name = "Cat" };
            var adoption = new Adoption { Id = 1, PetId = petId };
            _petServiceMock.Setup(s => s.GetPetByIdAsync(petId)).ReturnsAsync(pet);
            _adoptionRequestServiceMock.Setup(r => r.GetAdoptionRequestsByPetIdAsync(petId))
                .ReturnsAsync(new List<AdoptionRequest>());
            _adoptionServiceMock.Setup(a => a.GetAdoptionByPetIdAsync(petId)).ReturnsAsync(adoption);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

            // Act
            var result = await _controller.Details(petId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(pet, viewResult.Model);
            Assert.Equal("PetAdopted", viewResult.ViewData["AdoptionStatus"]);
            Assert.Equal(adoption, viewResult.ViewData["Adoption"]);
        }

        [Fact]
        public async Task Details_WhenUserLoggedIn_SetsUserSpecificViewData()
        {
            // Arrange
            var petId = 1;
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet { Id = petId, Name = "Cat" };
            var adoptionRequest = new AdoptionRequest { Id = 101, UserId = user.Id, PetId = petId };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(petId)).ReturnsAsync(pet);
            _petServiceMock.Setup(s => s.IsUserOwnerOfPetAsync(petId, user.Id)).ReturnsAsync(true);
            _adoptionRequestServiceMock.Setup(r => r.GetAdoptionRequestsByPetIdAsync(petId))
                .ReturnsAsync(new List<AdoptionRequest> { adoptionRequest });
            _adoptionServiceMock.Setup(a => a.GetAdoptionByPetIdAsync(petId)).ReturnsAsync((Adoption)null);
            _adoptionServiceMock.Setup(a => a.GetAdoptionRequestByUserAndPetAsync(user.Id, petId))
                .ReturnsAsync(adoptionRequest);

            // Act
            var result = await _controller.Details(petId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True((bool)viewResult.ViewData["IsUserLoggedIn"]);
            Assert.True((bool)viewResult.ViewData["IsOwner"]);
            Assert.True((bool)viewResult.ViewData["HasAdoptionRequest"]);
            Assert.Contains(adoptionRequest, (List<AdoptionRequest>)viewResult.ViewData["AdoptionRequests"]);
        }
        #endregion

        #region Edit (GET)
        [Fact]
        public async Task Edit_Get_WhenUserNotLoggedIn_RedirectsToLogin()
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
        public async Task Edit_Get_WhenPetNotFound_ReturnsErrorView()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(999)).ThrowsAsync(new KeyNotFoundException("Pet not found."));

            // Act
            var result = await _controller.Edit(999);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Edit_Get_WhenPetAlreadyAdopted_ReturnsErrorView()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet { Id = 1, Name = "Doggo" };
            var adoption = new Adoption { Id = 1, PetId = 1 };
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _petServiceMock.Setup(s => s.IsUserOwnerOfPetAsync(1, user.Id)).ReturnsAsync(true);
            _adoptionServiceMock.Setup(a => a.GetAdoptionByPetIdAsync(1)).ReturnsAsync(adoption);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Edit_Get_WhenNotOwner_ReturnsErrorView()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet { Id = 1, Name = "Doggo" };
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _petServiceMock.Setup(s => s.IsUserOwnerOfPetAsync(1, user.Id)).ReturnsAsync(false);
            _adoptionServiceMock.Setup(a => a.GetAdoptionByPetIdAsync(1)).ReturnsAsync((Adoption)null);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Edit_Get_WhenValid_ReturnsViewWithPet()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet { Id = 1, Name = "Doggo" };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _petServiceMock.Setup(s => s.IsUserOwnerOfPetAsync(1, user.Id)).ReturnsAsync(true);
            _adoptionServiceMock.Setup(a => a.GetAdoptionByPetIdAsync(1)).ReturnsAsync((Adoption)null);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(pet, viewResult.Model);
            Assert.Null(viewResult.ViewName);
        }
        #endregion

        #region Edit (POST)
        [Fact]
        public async Task Edit_Post_WhenUserNotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            var pet = new Pet { Id = 1, Name = "Doggo" };
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

            // Act
            var result = await _controller.Edit(1, pet);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Edit_Post_WhenPetNotFound_ReturnsErrorView()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet { Id = 1, Name = "Doggo" };
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ThrowsAsync(new KeyNotFoundException("Pet not found."));

            // Act
            var result = await _controller.Edit(1, pet);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Edit_Post_WhenNotOwner_ReturnsErrorView()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet { Id = 1, Name = "Doggo" };
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _petServiceMock.Setup(s => s.IsUserOwnerOfPetAsync(1, user.Id)).ReturnsAsync(false);

            // Act
            var result = await _controller.Edit(1, pet);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Edit_Post_WhenModelIsInvalid_ReturnsView()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet { Id = 1 };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _petServiceMock.Setup(s => s.IsUserOwnerOfPetAsync(1, user.Id)).ReturnsAsync(true);
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _controller.Edit(1, pet);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(pet, viewResult.Model);
            Assert.Equal("InvalidPetData", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Edit_Post_WhenValid_SendsEmailsAndRedirects()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet { Id = 1, Name = "Doggo" };
            var updatedPet = new Pet { Id = 1, Name = "Updated Doggo", IsNeutered = true };
            var request1 = new AdoptionRequest { Id = 101, User = new User { Id = 201, Email = "request1@test.com" } };
            var request2 = new AdoptionRequest { Id = 102, User = new User { Id = 202, Email = "request2@test.com" } };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _petServiceMock.Setup(s => s.IsUserOwnerOfPetAsync(1, user.Id)).ReturnsAsync(true);
            _petServiceMock.Setup(s => s.UpdatePetAsync(1, updatedPet, user.Id)).Returns(Task.CompletedTask);
            _adoptionRequestServiceMock.Setup(r => r.GetAdoptionRequestsByPetIdAsync(1))
                .ReturnsAsync(new List<AdoptionRequest> { request1, request2 });
            _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Edit(1, updatedPet);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal(1, redirectResult.RouteValues["id"]);
            _petServiceMock.Verify(s => s.UpdatePetAsync(1, updatedPet, user.Id), Times.Once());
            _emailServiceMock.Verify(e => e.SendEmailAsync("request1@test.com", "PetUpdateEmailSubject", It.IsAny<string>()), Times.Once());
            _emailServiceMock.Verify(e => e.SendEmailAsync("request2@test.com", "PetUpdateEmailSubject", It.IsAny<string>()), Times.Once());
            Assert.Equal("PetUpdatedSuccess", _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task Edit_Post_WhenIsNeuteredNull_SetsToFalse()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet { Id = 1, Name = "Doggo" };
            var updatedPet = new Pet { Id = 1, Name = "Updated Doggo", IsNeutered = null };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _petServiceMock.Setup(s => s.IsUserOwnerOfPetAsync(1, user.Id)).ReturnsAsync(true);
            _petServiceMock.Setup(s => s.UpdatePetAsync(1, It.IsAny<Pet>(), user.Id)).Returns(Task.CompletedTask);
            _adoptionRequestServiceMock.Setup(r => r.GetAdoptionRequestsByPetIdAsync(1))
                .ReturnsAsync(new List<AdoptionRequest>());
            _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Edit(1, updatedPet);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _petServiceMock.Verify(s => s.UpdatePetAsync(1, It.Is<Pet>(p => p.IsNeutered == false), user.Id), Times.Once());
        }

        [Fact]
        public async Task Edit_Post_WhenUnauthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet { Id = 1, Name = "Doggo" };
            var updatedPet = new Pet { Id = 1, Name = "Updated Doggo" };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _petServiceMock.Setup(s => s.IsUserOwnerOfPetAsync(1, user.Id)).ReturnsAsync(true);
            _petServiceMock.Setup(s => s.UpdatePetAsync(1, updatedPet, user.Id))
                .ThrowsAsync(new UnauthorizedAccessException("You are not authorized to update this pet."));
            _adoptionRequestServiceMock.Setup(r => r.GetAdoptionRequestsByPetIdAsync(1))
                .ReturnsAsync(new List<AdoptionRequest>());

            // Act
            var result = await _controller.Edit(1, updatedPet);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Error", viewResult.ViewName);
            Assert.Equal("NotAuthorizedToEdit", _controller.TempData["ErrorMessage"]);
        }
        #endregion

        #region Delete (GET)
        [Fact]
        public async Task Delete_Get_WhenUserNotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Delete_Get_WhenPetNotFound_ReturnsErrorView()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ThrowsAsync(new KeyNotFoundException("Pet not found."));

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Error", viewResult.ViewName);
            Assert.Equal("PetNotFound", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Delete_Get_WhenPetAlreadyAdopted_ReturnsErrorView()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet { Id = 1, Name = "Doggo" };
            var adoption = new Adoption { Id = 1, PetId = 1 };
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _petServiceMock.Setup(s => s.IsUserOwnerOfPetAsync(1, user.Id)).ReturnsAsync(true);
            _adoptionServiceMock.Setup(a => a.GetAdoptionByPetIdAsync(1)).ReturnsAsync(adoption);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Delete_Get_WhenNotOwner_ReturnsErrorView()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet { Id = 1, Name = "Doggo" };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _petServiceMock.Setup(s => s.IsUserOwnerOfPetAsync(1, user.Id)).ReturnsAsync(false);
            _adoptionServiceMock.Setup(a => a.GetAdoptionByPetIdAsync(1)).ReturnsAsync((Adoption)null);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Error", viewResult.ViewName);
            Assert.Equal("NotAuthorizedToDelete", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Delete_Get_WhenValid_ReturnsViewWithPet()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet { Id = 1, Name = "Doggo" };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _petServiceMock.Setup(s => s.IsUserOwnerOfPetAsync(1, user.Id)).ReturnsAsync(true);
            _adoptionServiceMock.Setup(a => a.GetAdoptionByPetIdAsync(1)).ReturnsAsync((Adoption)null);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(pet, viewResult.Model);
            Assert.Null(viewResult.ViewName);
        }
        #endregion

        #region Delete (POST)
        [Fact]
        public async Task Delete_Post_WhenUserNotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Delete_Post_WhenPetNotFound_ReturnsErrorView()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ThrowsAsync(new KeyNotFoundException("Pet not found."));

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Error", viewResult.ViewName);
            Assert.Equal("PetNotFound", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Delete_Post_WhenPetAlreadyAdopted_ReturnsErrorView()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet { Id = 1, Name = "Doggo" };
            var adoption = new Adoption { Id = 1, PetId = 1 };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _adoptionServiceMock.Setup(a => a.GetAdoptionByPetIdAsync(1)).ReturnsAsync(adoption);
            _adoptionRequestServiceMock.Setup(r => r.GetAdoptionRequestsByPetIdAsync(1))
                .ReturnsAsync(new List<AdoptionRequest>());

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Error", viewResult.ViewName);
            Assert.Equal("PetAdoptedCannotDelete", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Delete_Post_WhenNotAuthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet { Id = 1, Name = "Doggo" };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _petServiceMock.Setup(s => s.DeletePetAsync(1, user.Id))
                .ThrowsAsync(new UnauthorizedAccessException("You are not authorized to delete this pet."));
            _adoptionServiceMock.Setup(a => a.GetAdoptionByPetIdAsync(1)).ReturnsAsync((Adoption)null);
            _adoptionRequestServiceMock.Setup(r => r.GetAdoptionRequestsByPetIdAsync(1))
                .ReturnsAsync(new List<AdoptionRequest>());

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Error", viewResult.ViewName);
            Assert.Equal("NotAuthorizedToDelete", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Delete_Post_WhenPetNotFoundInService_ThrowsKeyNotFoundException()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet { Id = 1, Name = "Doggo" };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _petServiceMock.Setup(s => s.DeletePetAsync(1, user.Id))
                .ThrowsAsync(new KeyNotFoundException("Pet not found."));
            _adoptionServiceMock.Setup(a => a.GetAdoptionByPetIdAsync(1)).ReturnsAsync((Adoption)null);
            _adoptionRequestServiceMock.Setup(r => r.GetAdoptionRequestsByPetIdAsync(1))
                .ReturnsAsync(new List<AdoptionRequest>());

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Error", viewResult.ViewName);
            Assert.Equal("PetNotFound", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Delete_Post_WhenValid_DeletesPetAndSendsEmails()
        {
            // Arrange
            var username = "TestUser";
            var user = new User { Id = 10, Username = username };
            var pet = new Pet { Id = 1, Name = "Doggo" };
            var request1 = new AdoptionRequest { Id = 101, User = new User { Id = 201, Email = "request1@test.com" } };
            var request2 = new AdoptionRequest { Id = 102, User = new User { Id = 202, Email = "request2@test.com" } };
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => { value = usernameBytes; return true; });
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _petServiceMock.Setup(s => s.DeletePetAsync(1, user.Id)).Returns(Task.CompletedTask);
            _adoptionServiceMock.Setup(a => a.GetAdoptionByPetIdAsync(1)).ReturnsAsync((Adoption)null);
            _adoptionRequestServiceMock.Setup(r => r.GetAdoptionRequestsByPetIdAsync(1))
                .ReturnsAsync(new List<AdoptionRequest> { request1, request2 });
            _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Adoption", redirectResult.ControllerName);
            _petServiceMock.Verify(s => s.DeletePetAsync(1, user.Id), Times.Once());
            _emailServiceMock.Verify(e => e.SendEmailAsync("request1@test.com", "PetDeletionEmailSubject", It.IsAny<string>()), Times.Once());
            _emailServiceMock.Verify(e => e.SendEmailAsync("request2@test.com", "PetDeletionEmailSubject", It.IsAny<string>()), Times.Once());
            Assert.Equal("PetDeletedSuccess", _controller.TempData["SuccessMessage"]);
        }
        #endregion
    }
}