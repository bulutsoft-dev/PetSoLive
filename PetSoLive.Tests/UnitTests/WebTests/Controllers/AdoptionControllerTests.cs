using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Interfaces;
using PetSoLive.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PetSoLive.Web.Tests
{
    public class AdoptionControllerTests
    {
        private readonly Mock<IAdoptionService> _mockAdoptionService;
        private readonly Mock<IPetService> _mockPetService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IPetOwnerService> _mockPetOwnerService;
        private readonly Mock<IAdoptionRequestRepository> _mockAdoptionRequestRepository;
        private readonly Mock<IAdoptionRequestService> _mockAdoptionRequestService;
        private readonly AdoptionController _controller;

        public AdoptionControllerTests()
        {
            _mockAdoptionService = new Mock<IAdoptionService>();
            _mockPetService = new Mock<IPetService>();
            _mockUserService = new Mock<IUserService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockPetOwnerService = new Mock<IPetOwnerService>();
            _mockAdoptionRequestRepository = new Mock<IAdoptionRequestRepository>();
            _mockAdoptionRequestService = new Mock<IAdoptionRequestService>();

            _controller = new AdoptionController(
                _mockAdoptionService.Object,
                _mockAdoptionRequestService.Object,
                _mockPetService.Object,
                _mockUserService.Object,
                _mockEmailService.Object,
                _mockPetOwnerService.Object,
                _mockAdoptionRequestRepository.Object
            );
        }

        [Fact]
        public async Task Index_ReturnsViewWithPets()
        {
            // Arrange
            var pets = new List<Pet>
            {
                new Pet { Id = 1, Name = "Buddy" },
                new Pet { Id = 2, Name = "Max" }
            };
            _mockPetService.Setup(s => s.GetAllPetsAsync()).ReturnsAsync(pets);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Pet>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Adopt_Get_ReturnsViewForAdoption()
        {
            // Arrange
            var petId = 1;
            var username = "testuser";
            var pet = new Pet { Id = petId, Name = "Buddy" };
            var user = new User { Id = 1, Username = username };
            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
            _controller.ControllerContext.HttpContext.Session.SetString("Username", username);
            _mockUserService.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _mockPetService.Setup(s => s.GetPetByIdAsync(petId)).ReturnsAsync(pet);
            _mockAdoptionService.Setup(s => s.GetAdoptionRequestByUserAndPetAsync(user.Id, petId)).ReturnsAsync((AdoptionRequest)null);

            // Act
            var result = await _controller.Adopt(petId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Buddy", viewResult.ViewData["PetName"]);
        }

        [Fact]
        public async Task Adopt_Post_CreatesAdoptionRequest_RedirectsToPetDetails()
        {
            // Arrange
            var petId = 1;
            var username = "testuser";
            var pet = new Pet { Id = petId, Name = "Buddy" };
            var user = new User { Id = 1, Username = username };
            var adoptionRequest = new AdoptionRequest
            {
                PetId = petId,
                UserId = user.Id,
                Status = AdoptionStatus.Pending,
                RequestDate = DateTime.Now
            };

            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
            _controller.ControllerContext.HttpContext.Session.SetString("Username", username);
            _mockUserService.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);
            _mockPetService.Setup(s => s.GetPetByIdAsync(petId)).ReturnsAsync(pet);
            _mockAdoptionService.Setup(s => s.GetAdoptionRequestByUserAndPetAsync(user.Id, petId)).ReturnsAsync((AdoptionRequest)null);
            _mockAdoptionService.Setup(s => s.CreateAdoptionRequestAsync(It.IsAny<AdoptionRequest>())).Returns(Task.CompletedTask);
            _mockEmailService.Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Adopt(petId, "John Doe", "john@example.com", "1234567890", "Some address", DateTime.Now, "I love this pet!");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal("Pet", redirectResult.ControllerName);
        }

        [Fact]
        public async Task ApproveRequest_ReturnsRedirect_WhenRequestIsApproved()
        {
            // Arrange
            var adoptionRequestId = 1;
            var petId = 1;
            var adoptionRequest = new AdoptionRequest { Id = adoptionRequestId, PetId = petId, Status = AdoptionStatus.Pending };
            var pet = new Pet { Id = petId, Name = "Buddy" };
            var user = new User { Id = 1, Username = "owner" };
            var petOwner = new PetOwner { UserId = 1, PetId = petId };

            _mockAdoptionRequestService.Setup(s => s.GetAdoptionRequestByIdAsync(adoptionRequestId)).ReturnsAsync(adoptionRequest);
            _mockPetService.Setup(s => s.GetPetByIdAsync(petId)).ReturnsAsync(pet);
            _mockPetOwnerService.Setup(s => s.GetPetOwnerByPetIdAsync(petId)).ReturnsAsync(petOwner);
            _mockAdoptionRequestService.Setup(s => s.UpdateAdoptionRequestAsync(It.IsAny<AdoptionRequest>())).Returns(Task.CompletedTask);
            _mockAdoptionService.Setup(s => s.CreateAdoptionAsync(It.IsAny<Adoption>())).Returns(Task.CompletedTask);
            _mockEmailService.Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ApproveRequest(adoptionRequestId, petId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
    }
}
