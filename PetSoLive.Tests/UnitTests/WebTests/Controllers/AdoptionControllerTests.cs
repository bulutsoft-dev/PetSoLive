using Moq;
using Xunit;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PetSoLive.Web.Controllers;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;

namespace PetSoLive.Tests.Controllers
{
    public class AdoptionControllerTests
    {
        private readonly Mock<IServiceManager> _serviceManagerMock;
        private readonly Mock<IStringLocalizer<AdoptionController>> _localizerMock;
        private readonly Mock<HttpContext> _httpContextMock;
        private readonly Mock<ISession> _sessionMock;
        private readonly AdoptionController _controller;

        public AdoptionControllerTests()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _localizerMock = new Mock<IStringLocalizer<AdoptionController>>();
            _sessionMock = new Mock<ISession>();
            _httpContextMock = new Mock<HttpContext>();
            _httpContextMock.Setup(x => x.Session).Returns(_sessionMock.Object);

            _controller = new AdoptionController(
                _serviceManagerMock.Object,
                _localizerMock.Object
            );
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContextMock.Object
            };
        }

        [Fact]
        public async Task Adopt_ShouldRedirectToLogin_WhenUserIsNotLoggedIn()
        {
            // Arrange
            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns(false);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(x => x.Session).Returns(sessionMock.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };

            // Act
            var result = await _controller.Adopt(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Adopt_ShouldReturnNotFound_WhenPetDoesNotExist()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) =>
                {
                    value = key == "Username" ? Encoding.UTF8.GetBytes("user1") : null;
                    return true;
                });

            var context = new DefaultHttpContext
            {
                Session = mockSession.Object
            };

            _controller.ControllerContext.HttpContext = context;

            _serviceManagerMock.Setup(m => m.PetService.GetPetByIdAsync(It.IsAny<int>())).ReturnsAsync((Pet)null);
            _serviceManagerMock.Setup(m => m.UserService.GetUserByUsernameAsync(It.IsAny<string>())).ReturnsAsync(new User { Id = 1, Username = "user1" });
            var adoptionServiceMock = new Mock<IAdoptionService>();
            _serviceManagerMock.Setup(m => m.AdoptionService).Returns(adoptionServiceMock.Object);
            adoptionServiceMock.Setup(m => m.GetAdoptionRequestByUserAndPetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((AdoptionRequest)null);

            // Act
            var result = await _controller.Adopt(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Index_ShouldReturnViewWithPets()
        {
            // Arrange
            var pets = new[] {
                new Pet { Id = 1, Name = "Dog" },
                new Pet { Id = 2, Name = "Cat" }
            };
            _serviceManagerMock.Setup(m => m.PetService.GetAllPetsAsync()).ReturnsAsync(pets);
            _localizerMock.Setup(l => l["AvailablePetsTitle"]).Returns(new LocalizedString("AvailablePetsTitle", "Available Pets"));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Pet[]>(viewResult.Model);
            Assert.Equal(2, model.Length);
        }

        [Fact]
        public async Task Adopt_ShouldReturnAdoptionRequestExistsView_WhenUserAlreadyRequestedAdoption()
        {
            // Arrange
            var pet = new Pet { Id = 1, Name = "Dog" };
            var user = new User { Id = 1, Username = "john_doe" };

            _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                .Returns(true)
                .Callback((string key, out byte[] value) =>
                {
                    value = Encoding.UTF8.GetBytes("john_doe");
                });

            _serviceManagerMock.Setup(m => m.PetService.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _serviceManagerMock.Setup(m => m.UserService.GetUserByUsernameAsync("john_doe")).ReturnsAsync(user);
            _serviceManagerMock.Setup(m => m.AdoptionService.GetAdoptionRequestByUserAndPetAsync(user.Id, pet.Id))
                .ReturnsAsync(new AdoptionRequest());

            // Act
            var result = await _controller.Adopt(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("AdoptionRequestExists", viewResult.ViewName);
        }

        [Fact]
        public async Task Adopt_ShouldCreateAdoptionRequestAndSendEmail_WhenRequestIsValid()
        {
            // Arrange
            var pet = new Pet { Id = 1, Name = "Dog" };
            var user = new User { Id = 1, Username = "john_doe", Email = "john@example.com" };

            var sessionData = Encoding.UTF8.GetBytes("john_doe");
            _sessionMock.Setup(s => s.TryGetValue("Username", out sessionData)).Returns(true);

            var context = new DefaultHttpContext();
            context.Session = _sessionMock.Object;

            _controller.ControllerContext.HttpContext = context;
            _serviceManagerMock.Setup(m => m.PetService.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _serviceManagerMock.Setup(m => m.UserService.GetUserByUsernameAsync("john_doe")).ReturnsAsync(user);
            _serviceManagerMock.Setup(m => m.AdoptionService.GetAdoptionRequestByUserAndPetAsync(user.Id, pet.Id)).ReturnsAsync((AdoptionRequest)null);
            _serviceManagerMock.Setup(m => m.UserService.UpdateUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            _serviceManagerMock.Setup(m => m.AdoptionService.CreateAdoptionRequestAsync(It.IsAny<AdoptionRequest>())).Returns(Task.CompletedTask);
            _serviceManagerMock.Setup(m => m.PetOwnerService.GetPetOwnerByPetIdAsync(It.IsAny<int>())).ReturnsAsync((PetOwner)null);
            _serviceManagerMock.Setup(m => m.EmailService.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Adopt(1, "John Doe", "john@example.com", "1234567890", "123 Street", DateTime.Now, "I want to adopt this dog");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal("Pet", redirectResult.ControllerName);
            _serviceManagerMock.Verify(m => m.AdoptionService.CreateAdoptionRequestAsync(It.IsAny<AdoptionRequest>()), Times.Once());
            _serviceManagerMock.Verify(m => m.EmailService.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task ApproveRequest_ShouldReturnNotFound_WhenRequestDoesNotExist()
        {
            // Arrange
            _serviceManagerMock.Setup(m => m.AdoptionRequestService.GetAdoptionRequestByIdAsync(It.IsAny<int>())).ReturnsAsync((AdoptionRequest)null);

            // Act
            var result = await _controller.ApproveRequest(1, 1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ApproveRequest_ShouldReturnUnauthorized_WhenUserIsNotOwner()
        {
            // Arrange
            var pet = new Pet { Id = 1, Name = "Dog", PetOwners = new List<PetOwner> { new PetOwner { UserId = 2 } } };
            var adoptionRequest = new AdoptionRequest { PetId = pet.Id, Status = AdoptionStatus.Pending, UserId = 1 };
            _serviceManagerMock.Setup(m => m.AdoptionRequestService.GetAdoptionRequestByIdAsync(1)).ReturnsAsync(adoptionRequest);
            _serviceManagerMock.Setup(m => m.PetService.GetPetByIdAsync(1)).ReturnsAsync(pet);

            var claims = new[] { new Claim(ClaimTypes.Name, "1") };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = userPrincipal };

            // Act
            var result = await _controller.ApproveRequest(1, 1);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task ApproveRequest_ShouldApproveRequestAndSendEmails()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "john_doe",
                Email = "john@example.com"
            };

            var pet = new Pet
            {
                Id = 1,
                Name = "Dog",
                PetOwners = new List<PetOwner>
                {
                    new PetOwner
                    {
                        UserId = 1,
                        User = user
                    }
                }
            };

            var adoptionRequest = new AdoptionRequest
            {
                Id = 1,
                PetId = pet.Id,
                Status = AdoptionStatus.Pending,
                UserId = 1,
                User = user
            };

            _serviceManagerMock.Setup(m => m.AdoptionRequestService.GetAdoptionRequestByIdAsync(1)).ReturnsAsync(adoptionRequest);
            _serviceManagerMock.Setup(m => m.PetService.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _serviceManagerMock.Setup(m => m.AdoptionRequestService.UpdateAdoptionRequestAsync(It.IsAny<AdoptionRequest>())).Returns(Task.CompletedTask);
            _serviceManagerMock.Setup(m => m.AdoptionRequestService.GetPendingRequestsByPetIdAsync(It.IsAny<int>())).ReturnsAsync(new List<AdoptionRequest>());
            _serviceManagerMock.Setup(m => m.AdoptionService.CreateAdoptionAsync(It.IsAny<Adoption>())).Returns(Task.CompletedTask);
            _serviceManagerMock.Setup(m => m.EmailService.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var claims = new[] { new Claim(ClaimTypes.Name, "1") };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = userPrincipal };

            // Act
            var result = await _controller.ApproveRequest(1, 1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _serviceManagerMock.Verify(m => m.AdoptionRequestService.UpdateAdoptionRequestAsync(It.Is<AdoptionRequest>(a => a.Status == AdoptionStatus.Approved)), Times.Once());
            _serviceManagerMock.Verify(m => m.AdoptionRequestService.GetPendingRequestsByPetIdAsync(It.IsAny<int>()), Times.Once());
            _serviceManagerMock.Verify(m => m.AdoptionService.CreateAdoptionAsync(It.IsAny<Adoption>()), Times.Once());
            _serviceManagerMock.Verify(m => m.EmailService.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }
    }
}