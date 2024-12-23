using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Http;

public class PetControllerTests
{
    private readonly PetController _controller;
    private readonly Mock<IPetService> _mockPetService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IAdoptionService> _mockAdoptionService;
    private readonly Mock<IAdoptionRequestRepository> _mockAdoptionRequestRepository;
    private readonly Mock<IEmailService> _mockEmailService;

    public PetControllerTests()
    {
        _mockPetService = new Mock<IPetService>();
        _mockUserService = new Mock<IUserService>();
        _mockAdoptionService = new Mock<IAdoptionService>();
        _mockAdoptionRequestRepository = new Mock<IAdoptionRequestRepository>();
        _mockEmailService = new Mock<IEmailService>();

        _controller = new PetController(
            _mockPetService.Object,
            _mockUserService.Object,
            _mockAdoptionService.Object,
            _mockAdoptionRequestRepository.Object,
            _mockEmailService.Object
        );
    }

    private void SetUpMockForLoggedInUser(string username)
    {
        var mockUser = new User { Id = 1, Username = username };
        _mockUserService.Setup(service => service.GetUserByUsernameAsync(username)).ReturnsAsync(mockUser);

        // Mock the session data
        _controller.ControllerContext = new ControllerContext();
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();
        _controller.ControllerContext.HttpContext.Session = new Mock<ISession>().Object;
        _controller.ControllerContext.HttpContext.Session.SetString("Username", username);
    }

    [Fact]
    public async Task Create_Get_ReturnsView_WhenUserIsLoggedIn()
    {
        // Arrange
        SetUpMockForLoggedInUser("testuser");

        // Act
        var result = _controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);  // Default view
    }

    [Fact]
    public async Task Create_Post_CreatesPetAndRedirects_WhenModelIsValid()
    {
        // Arrange
        SetUpMockForLoggedInUser("testuser");
        var pet = new Pet { Id = 1, Name = "Buddy" };

        _mockPetService.Setup(service => service.CreatePetAsync(pet)).Returns(Task.CompletedTask);
        _mockPetService.Setup(service => service.AssignPetOwnerAsync(It.IsAny<PetOwner>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(pet);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Adoption", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Create_Post_ReturnsView_WhenModelIsInvalid()
    {
        // Arrange
        SetUpMockForLoggedInUser("testuser");
        var pet = new Pet { Id = 1, Name = "Buddy" };
        _controller.ModelState.AddModelError("Name", "Required");

        // Act
        var result = await _controller.Create(pet);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(pet, viewResult.Model);
    }

    [Fact]
    public async Task Details_ReturnsError_WhenPetNotFound()
    {
        // Arrange
        SetUpMockForLoggedInUser("testuser");
        _mockPetService.Setup(service => service.GetPetByIdAsync(It.IsAny<int>())).ReturnsAsync((Pet)null);

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", viewResult.ViewName);
        Assert.Equal("Pet not found.", viewResult.ViewData["ErrorMessage"]);
    }

    [Fact]
    public async Task Details_ReturnsView_WhenPetIsFound()
    {
        // Arrange
        SetUpMockForLoggedInUser("testuser");
        var pet = new Pet { Id = 1, Name = "Buddy" };
        _mockPetService.Setup(service => service.GetPetByIdAsync(1)).ReturnsAsync(pet);
        _mockAdoptionService.Setup(service => service.GetAdoptionByPetIdAsync(1)).ReturnsAsync((Adoption)null);
        _mockAdoptionRequestRepository.Setup(repo => repo.GetAdoptionRequestsByPetIdAsync(1)).ReturnsAsync(new List<AdoptionRequest>());

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(pet, viewResult.Model);
    }

    [Fact]
    public async Task Edit_Get_ReturnsError_WhenPetNotFound()
    {
        // Arrange
        SetUpMockForLoggedInUser("testuser");
        _mockPetService.Setup(service => service.GetPetByIdAsync(It.IsAny<int>())).ReturnsAsync((Pet)null);

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", viewResult.ViewName);
        Assert.Equal("Pet not found.", viewResult.ViewData["ErrorMessage"]);
    }

    [Fact]
    public async Task Edit_Post_UpdatesPetAndRedirects_WhenModelIsValid()
    {
        // Arrange
        SetUpMockForLoggedInUser("testuser");
        var pet = new Pet { Id = 1, Name = "Buddy" };
        var updatedPet = new Pet { Id = 1, Name = "UpdatedBuddy" };

        _mockPetService.Setup(service => service.GetPetByIdAsync(1)).ReturnsAsync(pet);
        _mockPetService.Setup(service => service.UpdatePetAsync(1, updatedPet, It.IsAny<int>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Edit(1, updatedPet);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
        Assert.Equal(1, redirectResult.RouteValues["id"]);
    }

    [Fact]
    public async Task Delete_Get_ReturnsError_WhenPetNotFound()
    {
        // Arrange
        SetUpMockForLoggedInUser("testuser");
        _mockPetService.Setup(service => service.GetPetByIdAsync(It.IsAny<int>())).ReturnsAsync((Pet)null);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", viewResult.ViewName);
        Assert.Equal("Pet not found.", viewResult.ViewData["ErrorMessage"]);
    }

    [Fact]
    public async Task Delete_Post_DeletesPetAndRedirects_WhenUserIsAuthorized()
    {
        // Arrange
        SetUpMockForLoggedInUser("testuser");
        var pet = new Pet { Id = 1, Name = "Buddy" };

        _mockPetService.Setup(service => service.GetPetByIdAsync(1)).ReturnsAsync(pet);
        _mockPetService.Setup(service => service.DeletePetAsync(1, It.IsAny<int>())).Returns(Task.CompletedTask);
        _mockAdoptionRequestRepository.Setup(repo => repo.GetAdoptionRequestsByPetIdAsync(1)).ReturnsAsync(new List<AdoptionRequest>());

        // Act
        var result = await _controller.DeleteConfirmed(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Adoption", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Delete_Post_ReturnsError_WhenUserIsNotAuthorized()
    {
        // Arrange
        SetUpMockForLoggedInUser("testuser");
        var pet = new Pet { Id = 1, Name = "Buddy" };
        _mockPetService.Setup(service => service.GetPetByIdAsync(1)).ReturnsAsync(pet);
        _mockPetService.Setup(service => service.IsUserOwnerOfPetAsync(1, 1)).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteConfirmed(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result); // Redirect bekliyorsunuz
        Assert.Equal("Error", redirectResult.ActionName);
        Assert.Equal("Pet not found.", redirectResult.RouteValues["ErrorMessage"]);
    }
    }

