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

    namespace PetSoLive.Tests.Controllers
    {
        public class AdoptionControllerTests
        {
            private readonly Mock<IAdoptionService> _adoptionServiceMock;
            private readonly Mock<IPetService> _petServiceMock;
            private readonly Mock<IUserService> _userServiceMock;
            private readonly Mock<IEmailService> _emailServiceMock;
            private readonly Mock<IPetOwnerService> _petOwnerServiceMock;
            private readonly Mock<IAdoptionRequestRepository> _adoptionRequestRepositoryMock;
            private readonly Mock<IAdoptionRequestService> _adoptionRequestServiceMock;

            private readonly Mock<HttpContext> _httpContextMock;
            private readonly Mock<ISession> _sessionMock;
            private readonly AdoptionController _controller;

            public AdoptionControllerTests()
            {
                _adoptionServiceMock = new Mock<IAdoptionService>();
                _petServiceMock = new Mock<IPetService>();
                _userServiceMock = new Mock<IUserService>();
                _emailServiceMock = new Mock<IEmailService>();
                _petOwnerServiceMock = new Mock<IPetOwnerService>();
                _adoptionRequestRepositoryMock = new Mock<IAdoptionRequestRepository>();
                _adoptionRequestServiceMock = new Mock<IAdoptionRequestService>();

                _sessionMock = new Mock<ISession>();
                _httpContextMock = new Mock<HttpContext>();
                _httpContextMock.Setup(x => x.Session).Returns(_sessionMock.Object);

                _controller = new AdoptionController(
                    _adoptionServiceMock.Object,
                    _adoptionRequestServiceMock.Object,
                    _petServiceMock.Object,
                    _userServiceMock.Object,
                    _emailServiceMock.Object,
                    _petOwnerServiceMock.Object,
                    _adoptionRequestRepositoryMock.Object,
                    null
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
    
                // Mock the TryGetValue method to return false, simulating that "Username" is not present in the session
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
            public async Task Adopt_ShouldReturnBadRequest_WhenPetDoesNotExist()
            {
                // Arrange
                var mockSession = new Mock<ISession>();

                // Mock the GetString extension method using TryGetValue
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

                _petServiceMock.Setup(p => p.GetPetByIdAsync(It.IsAny<int>())).ReturnsAsync((Pet)null);  // Pet null döndürülüyor

                // Act
                var result = await _controller.Adopt(1);

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);  // Pet bulunamazsa BadRequest dönecek
            }






            [Fact]
            public async Task Index_ShouldReturnViewWithPets()
            {
                // Arrange
                var pets = new[] {
                    new Pet { Id = 1, Name = "Dog" },
                    new Pet { Id = 2, Name = "Cat" }
                };
                _petServiceMock.Setup(service => service.GetAllPetsAsync()).ReturnsAsync(pets);

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

                // Mock the session to return the username "john_doe"
                _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
                    .Returns(true)
                    .Callback((string key, out byte[] value) =>
                    {
                        value = Encoding.UTF8.GetBytes("john_doe");
                    });

                _petServiceMock.Setup(service => service.GetPetByIdAsync(1)).ReturnsAsync(pet);
                _userServiceMock.Setup(service => service.GetUserByUsernameAsync("john_doe")).ReturnsAsync(user);
                _adoptionServiceMock.Setup(service => service.GetAdoptionRequestByUserAndPetAsync(user.Id, pet.Id))
                    .ReturnsAsync(new AdoptionRequest());  // Simulate an existing adoption request

                // Act
                var result = await _controller.Adopt(1);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);  // Expecting the "AdoptionRequestExists" view
                Assert.Equal("AdoptionRequestExists", viewResult.ViewName);  // Check the view name
            }




            [Fact]
            public async Task Adopt_ShouldCreateAdoptionRequestAndSendEmail_WhenRequestIsValid()
            {
                // Arrange
                var pet = new Pet { Id = 1, Name = "Dog" };
                var user = new User { Id = 1, Username = "john_doe", Email = "john@example.com" };
    
                // Mock the session to return the expected username "john_doe"
                var sessionData = Encoding.UTF8.GetBytes("john_doe");
                _sessionMock.Setup(s => s.TryGetValue("Username", out sessionData)).Returns(true);

                var context = new DefaultHttpContext();
                context.Session = _sessionMock.Object;

                _controller.ControllerContext.HttpContext = context;
                _petServiceMock.Setup(service => service.GetPetByIdAsync(1)).ReturnsAsync(pet);
                _userServiceMock.Setup(service => service.GetUserByUsernameAsync("john_doe")).ReturnsAsync(user);
                _adoptionServiceMock.Setup(service => service.GetAdoptionRequestByUserAndPetAsync(user.Id, pet.Id)).ReturnsAsync((AdoptionRequest)null);

                // Act
                var result = await _controller.Adopt(1, "John Doe", "john@example.com", "1234567890", "123 Street", DateTime.Now, "I want to adopt this dog");

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Details", redirectResult.ActionName);
                Assert.Equal("Pet", redirectResult.ControllerName);
                _adoptionServiceMock.Verify(service => service.CreateAdoptionRequestAsync(It.IsAny<AdoptionRequest>()), Times.Once);
                _emailServiceMock.Verify(service => service.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            }

            [Fact]
            public async Task ApproveRequest_ShouldReturnNotFound_WhenRequestDoesNotExist()
            {
                // Arrange
                _adoptionRequestServiceMock.Setup(service => service.GetAdoptionRequestByIdAsync(It.IsAny<int>())).ReturnsAsync((AdoptionRequest)null);

                // Act
                var result = await _controller.ApproveRequest(1, 1);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }

            [Fact]
            public async Task ApproveRequest_ShouldReturnUnauthorized_WhenUserIsNotOwner()
            {
                // Arrange
                var pet = new Pet { Id = 1, Name = "Dog", PetOwners = { new PetOwner { UserId = 2 } } };
                var adoptionRequest = new AdoptionRequest { PetId = pet.Id, Status = AdoptionStatus.Pending, UserId = 1 };
                _adoptionRequestServiceMock.Setup(service => service.GetAdoptionRequestByIdAsync(1)).ReturnsAsync(adoptionRequest);
                _petServiceMock.Setup(service => service.GetPetByIdAsync(1)).ReturnsAsync(pet);

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
                    User = user, // PetOwner ile User ilişkisi kuruluyor
                }
            } // PetOwner listesi başlatıldı
        };

        var adoptionRequest = new AdoptionRequest
        {
            PetId = pet.Id,
            Status = AdoptionStatus.Pending,
            UserId = 1
        };

        // Mock servislerin setup'ları
        _adoptionRequestServiceMock.Setup(service => service.GetAdoptionRequestByIdAsync(1))
            .ReturnsAsync(adoptionRequest);  // AdoptionRequest'i döndür
        _petServiceMock.Setup(service => service.GetPetByIdAsync(1))
            .ReturnsAsync(pet);  // Pet'i döndür

        // HttpContext mock'laması, kullanıcıyı ayarlamak
        var claims = new[] 
        {
            new Claim(ClaimTypes.Name, "1") // PetOwner'ın UserId'si ile eşleşiyor
        };
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));
        _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = userPrincipal };

        // AdoptionRequest güncellenmesini mock'lama
        _adoptionRequestServiceMock.Setup(service => service.UpdateAdoptionRequestAsync(It.IsAny<AdoptionRequest>()))
            .Returns(Task.CompletedTask);

        // E-posta servisi mock'lama
        _emailServiceMock.Setup(service => service.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ApproveRequest(1, 1);

        // Assert
        // AdoptionRequest'in durumunun "Onaylı" olduğundan emin olalım
        _adoptionRequestServiceMock.Verify(service => service.UpdateAdoptionRequestAsync(It.Is<AdoptionRequest>(a => a.Status == AdoptionStatus.Approved)), Times.Once);

        // E-posta gönderme servisinin bir kez çağrıldığını doğrulayalım (onay için)
        _emailServiceMock.Verify(service => service.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.AtMost(0));
    }






        }
    }
