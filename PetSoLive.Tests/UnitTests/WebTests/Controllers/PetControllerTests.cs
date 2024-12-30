using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

// Adjust namespaces to match your actual project structure
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Web.Controllers;

public class PetControllerTests
{
    private readonly Mock<IPetService> _petServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IAdoptionService> _adoptionServiceMock;
        private readonly Mock<IAdoptionRequestRepository> _adoptionRequestRepoMock;
        private readonly Mock<IEmailService> _emailServiceMock;

        private PetController _controller;
        private DefaultHttpContext _httpContext;

        public PetControllerTests()
        {
            _petServiceMock = new Mock<IPetService>();
            _userServiceMock = new Mock<IUserService>();
            _adoptionServiceMock = new Mock<IAdoptionService>();
            _adoptionRequestRepoMock = new Mock<IAdoptionRequestRepository>();
            _emailServiceMock = new Mock<IEmailService>();

            _controller = new PetController(
                _petServiceMock.Object,
                _userServiceMock.Object,
                _adoptionServiceMock.Object,
                _adoptionRequestRepoMock.Object,
                _emailServiceMock.Object
            );

            // Set up a fake HTTP context with a session
            _httpContext = new DefaultHttpContext();
            var session = new TestSession(); // a simple in-memory session store (see below)
            _httpContext.Session = session;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };
        }

        [Fact]
        public async Task Create_Get_WhenUserNotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            // Session has no "Username" -> user not logged in

            // Act
            var result = _controller.Create() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Account", result.ControllerName);
        }

        [Fact]
        public async Task Create_Get_WhenUserLoggedIn_ReturnsView()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "TestUser"); // user is logged in

            // Act
            var result = _controller.Create() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ViewName); // The default View
        }

        [Fact]
        public async Task Create_Post_WhenUserNotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            // user is not logged in (no session "Username")
            var pet = new Pet { Id = 1, Name = "Doggo" };

            // Act
            var result = await _controller.Create(pet) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Account", result.ControllerName);
        }

        [Fact]
        public async Task Create_Post_WhenModelIsValid_CreatesPetAndRedirectsToIndex()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "TestUser");
            var user = new User { Id = 10, Username = "TestUser" };
            var pet = new Pet { Id = 1, Name = "Doggo" };

            _userServiceMock
                .Setup(s => s.GetUserByUsernameAsync("TestUser"))
                .ReturnsAsync(user);

            _petServiceMock
                .Setup(s => s.CreatePetAsync(pet))
                .Returns(Task.CompletedTask);

            _petServiceMock
                .Setup(s => s.AssignPetOwnerAsync(It.IsAny<PetOwner>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(pet) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Adoption", result.ControllerName);

            _petServiceMock.Verify(s => s.CreatePetAsync(pet), Times.Once);
            _petServiceMock.Verify(s => s.AssignPetOwnerAsync(It.Is<PetOwner>(
                po => po.UserId == user.Id && po.PetId == pet.Id
            )), Times.Once);
        }

        [Fact]
        public async Task Create_Post_WhenModelIsInvalid_ReturnsView()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "TestUser");
            _controller.ModelState.AddModelError("Name", "Required");

            var pet = new Pet(); // missing fields -> invalid

            // Act
            var result = await _controller.Create(pet) as ViewResult;

            // Assert
            Assert.NotNull(result);
            // Returns the same view with the invalid model
            Assert.Equal(pet, result.Model);
        }

        [Fact]
        public async Task Details_WhenPetNotFound_ReturnsErrorViewWithMessage()
        {
            // Arrange
            var petId = 999;
            _petServiceMock
                .Setup(s => s.GetPetByIdAsync(petId))
                .ReturnsAsync((Pet)null);

            // Act
            var result = await _controller.Details(petId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Error", result.ViewName);
            Assert.True(_controller.ViewBag.ErrorMessage == "Pet not found.");
        }

        [Fact]
        public async Task Details_WhenPetFound_ReturnsViewWithPetAndOtherViewBags()
        {
            // Arrange
            var petId = 1;
            var pet = new Pet { Id = petId, Name = "Cat" };

            _petServiceMock
                .Setup(s => s.GetPetByIdAsync(petId))
                .ReturnsAsync(pet);

            _adoptionRequestRepoMock
                .Setup(r => r.GetAdoptionRequestsByPetIdAsync(petId))
                .ReturnsAsync(new List<AdoptionRequest>());

            _adoptionServiceMock
                .Setup(a => a.GetAdoptionByPetIdAsync(petId))
                .ReturnsAsync((Adoption)null);

            // no user is logged in (by default)
            // -> isUserLoggedIn = false

            // Act
            var result = await _controller.Details(petId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(pet, result.Model);
            Assert.Equal("This pet is available for adoption.", _controller.ViewBag.AdoptionStatus);
            Assert.False(_controller.ViewBag.IsUserLoggedIn);
            Assert.Null(_controller.ViewBag.Adoption);
            Assert.Empty(_controller.ViewBag.AdoptionRequests);
            Assert.False(_controller.ViewBag.HasAdoptionRequest);
        }

        [Fact]
        public async Task Edit_Get_WhenPetNotFound_ReturnsErrorView()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "TestUser");
            var user = new User { Id = 10, Username = "TestUser" };
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync("TestUser")).ReturnsAsync(user);

            _petServiceMock.Setup(s => s.GetPetByIdAsync(999)).ReturnsAsync((Pet)null);

            // Act
            var result = await _controller.Edit(999) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Error", result.ViewName);
            Assert.Equal("Pet not found.", _controller.ViewBag.ErrorMessage);
        }

        [Fact]
        public async Task Edit_Get_WhenNotOwner_ReturnsErrorView()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "TestUser");
            var user = new User { Id = 10, Username = "TestUser" };
            var pet = new Pet { Id = 1, Name = "Doggo" };

            _userServiceMock.Setup(s => s.GetUserByUsernameAsync("TestUser")).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ReturnsAsync(pet);

            // The user is not the owner
            _petServiceMock.Setup(s => s.IsUserOwnerOfPetAsync(1, user.Id)).ReturnsAsync(false);

            // Act
            var result = await _controller.Edit(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Error", result.ViewName);
            Assert.Equal("You are not authorized to edit this pet.", _controller.ViewBag.ErrorMessage);
        }

        [Fact]
        public async Task Edit_Get_WhenPetAlreadyAdopted_ReturnsErrorView()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "TestUser");
            var user = new User { Id = 10, Username = "TestUser" };
            var pet = new Pet { Id = 1, Name = "Doggo" };
            var adoption = new Adoption { Id = 1, PetId = 1 };

            _userServiceMock.Setup(s => s.GetUserByUsernameAsync("TestUser")).ReturnsAsync(user);
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _petServiceMock.Setup(s => s.IsUserOwnerOfPetAsync(1, user.Id)).ReturnsAsync(true);
            // Adoption exists
            _adoptionServiceMock.Setup(a => a.GetAdoptionByPetIdAsync(1)).ReturnsAsync(adoption);

            // Act
            var result = await _controller.Edit(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Error", result.ViewName);
            Assert.Equal("This pet has already been adopted and cannot be edited.", _controller.ViewBag.ErrorMessage);
        }

        [Fact]
        public async Task Edit_Post_WhenPetIsUpdated_SendsEmailsToAllAdoptionRequestUsers()
        {
            // Arrange
            _httpContext.Session.SetString("Username", "TestUser");
            var user = new User { Id = 10, Username = "TestUser" };
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync("TestUser")).ReturnsAsync(user);

            var pet = new Pet { Id = 1, Name = "Doggo" };
            _petServiceMock.Setup(s => s.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _petServiceMock.Setup(s => s.IsUserOwnerOfPetAsync(1, user.Id)).ReturnsAsync(true);

            // Suppose we have two adoption requests
            var request1 = new AdoptionRequest { Id = 101, User = new User { Id = 201, Email = "request1@test.com" } };
            var request2 = new AdoptionRequest { Id = 102, User = new User { Id = 202, Email = "request2@test.com" } };

            _adoptionRequestRepoMock
                .Setup(r => r.GetAdoptionRequestsByPetIdAsync(1))
                .ReturnsAsync(new List<AdoptionRequest> { request1, request2 });

            _petServiceMock
                .Setup(s => s.UpdatePetAsync(1, It.IsAny<Pet>(), user.Id))
                .Returns(Task.CompletedTask);

            // Act
            var updatedPet = new Pet { Name = "Updated Doggo", IsNeutered = true };
            var result = await _controller.Edit(1, updatedPet) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Details", result.ActionName);
            Assert.Equal(1, result.RouteValues["id"]);

            // Verify UpdatePetAsync was called
            _petServiceMock.Verify(s => s.UpdatePetAsync(1, updatedPet, user.Id), Times.Once);

            // Verify emails were sent to the requesters
            _emailServiceMock.Verify(e => e.SendEmailAsync("request1@test.com",
                "The pet you requested adoption for has been updated",
                It.IsAny<string>()),
                Times.Once);
            _emailServiceMock.Verify(e => e.SendEmailAsync("request2@test.com",
                "The pet you requested adoption for has been updated",
                It.IsAny<string>()),
                Times.Once);
        }

        // Additional Delete and other tests can be written in the same pattern...
    }

    /// <summary>
    /// A simple in-memory session for testing. 
    /// Alternatively, you can use a library like "Microsoft.AspNetCore.Http.Features" 
    /// or write a custom mock session.
    /// </summary>
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = new Dictionary<string, byte[]>();

        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _sessionStorage.Keys;

        public void Clear() => _sessionStorage.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key)
        {
            if (_sessionStorage.ContainsKey(key))
                _sessionStorage.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            _sessionStorage[key] = value;
        }

        public bool TryGetValue(string key, out byte[] value) => _sessionStorage.TryGetValue(key, out value);
    }

