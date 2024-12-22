using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Xunit;

public class PetControllerTests
{
    private readonly Mock<IPetService> _mockPetService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IAdoptionService> _mockAdoptionService;
    private readonly Mock<IAdoptionRequestRepository> _mockAdoptionRequestRepository;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly PetController _controller;

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
            _mockEmailService.Object);
    }

    [Fact]
    public async Task Create_WhenUserIsNotLoggedIn_ShouldRedirectToLogin()
    {
        // Arrange
        _controller.ControllerContext.HttpContext = new DefaultHttpContext(); // Simulate a request without a logged-in user

        // Act
        var result =  _controller.Create();  // Await the async method

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectToActionResult.ActionName);
    }





    [Fact]
    public void Create_WhenModelStateIsInvalid_ShouldReturnViewWithPet()
    {
        // Arrange
        var pet = new Pet { Name = "Dog" };
        _controller.ModelState.AddModelError("Name", "Required");

        // Act
        var result = _controller.Create(pet).Result; // Use .Result or .GetAwaiter().GetResult()

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(pet, viewResult.Model);
    }

    [Fact]
    public void Create_WhenPetIsValid_ShouldCreatePetAndRedirectToAdoptionIndex()
    {
        // Arrange
        var pet = new Pet { Name = "Dog" };
        var user = new User { Id = 1, Username = "testuser" };
        _mockUserService.Setup(u => u.GetUserByUsernameAsync(It.IsAny<string>())).ReturnsAsync(user);
        _mockPetService.Setup(p => p.CreatePetAsync(It.IsAny<Pet>())).Returns(Task.CompletedTask);

        // Act
        var result = _controller.Create(pet).Result; // Use .Result or .GetAwaiter().GetResult()

        // Assert
        _mockPetService.Verify(p => p.CreatePetAsync(It.IsAny<Pet>()), Times.Once);
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Adoption", redirectResult.ControllerName);
    }

    [Fact]
    public void Details_WhenPetDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        _mockPetService.Setup(p => p.GetPetByIdAsync(It.IsAny<int>())).ReturnsAsync((Pet)null);

        // Act
        var result = _controller.Details(1).Result; // Use .Result or .GetAwaiter().GetResult()

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Edit_WhenPetDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        _mockPetService.Setup(p => p.GetPetByIdAsync(It.IsAny<int>())).ReturnsAsync((Pet)null);

        // Act
        var result = _controller.Edit(1).Result; // Use .Result or .GetAwaiter().GetResult()

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Delete_WhenPetDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        _mockPetService.Setup(p => p.GetPetByIdAsync(It.IsAny<int>())).ReturnsAsync((Pet)null);

        // Act
        var result = _controller.Delete(1).Result; // Use .Result or .GetAwaiter().GetResult()

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void DeleteConfirmed_WhenPetIsValid_ShouldDeletePetAndRedirectToAdoptionIndex()
    {
        // Arrange
        var pet = new Pet { Id = 1, Name = "Dog" };
        var user = new User { Id = 1, Username = "testuser" };
        _mockPetService.Setup(p => p.GetPetByIdAsync(It.IsAny<int>())).ReturnsAsync(pet);
        _mockUserService.Setup(u => u.GetUserByUsernameAsync(It.IsAny<string>())).ReturnsAsync(user);
        _mockPetService.Setup(p => p.DeletePetAsync(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);

        // Act
        var result = _controller.DeleteConfirmed(1).Result; // Use .Result or .GetAwaiter().GetResult()

        // Assert
        _mockPetService.Verify(p => p.DeletePetAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Adoption", redirectResult.ControllerName);
    }

    [Fact]
    public void Edit_WhenPetIsNotOwnedByUser_ShouldReturnError()
    {
        // Arrange
        var pet = new Pet { Id = 1, Name = "Dog" };
        var user = new User { Id = 1, Username = "testuser" };
        _mockPetService.Setup(p => p.GetPetByIdAsync(It.IsAny<int>())).ReturnsAsync(pet);
        _mockUserService.Setup(u => u.GetUserByUsernameAsync(It.IsAny<string>())).ReturnsAsync(user);
        _mockPetService.Setup(p => p.IsUserOwnerOfPetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);

        // Act
        var result = _controller.Edit(1).Result;

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("You are not authorized to edit this pet.", viewResult.ViewData["ErrorMessage"]);
    }

    [Fact]
    public void Edit_WhenPetIsAdopted_ShouldReturnError()
    {
        // Arrange
        var pet = new Pet { Id = 1, Name = "Dog" };
        var user = new User { Id = 1, Username = "testuser" };
        _mockPetService.Setup(p => p.GetPetByIdAsync(It.IsAny<int>())).ReturnsAsync(pet);
        _mockUserService.Setup(u => u.GetUserByUsernameAsync(It.IsAny<string>())).ReturnsAsync(user);
        _mockAdoptionService.Setup(a => a.GetAdoptionByPetIdAsync(It.IsAny<int>())).ReturnsAsync(new Adoption());

        // Act
        var result = _controller.Edit(1).Result;

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("This pet has already been adopted and cannot be edited.", viewResult.ViewData["ErrorMessage"]);
    }
}
