using Xunit;
using Moq;
using Petsolive.API.Controllers;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Petsolive.API.DTOs;
using PetSoLive.Core.Entities;

namespace PetSoLive.Tests.UnitTests.APITests.Controllers
{
    public class AdoptionRequestControllerTests
    {
        private readonly Mock<IServiceManager> _serviceManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly AdoptionRequestController _controller;

        public AdoptionRequestControllerTests()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _mapperMock = new Mock<IMapper>();
            _emailServiceMock = new Mock<IEmailService>();
            _serviceManagerMock.SetupGet(s => s.EmailService).Returns(_emailServiceMock.Object);
            _controller = new AdoptionRequestController(_serviceManagerMock.Object, _mapperMock.Object, null);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WithAdoptionRequestDto()
        {
            // Arrange
            int id = 1;
            var request = new AdoptionRequest { Id = id };
            var requestDto = new AdoptionRequestDto { Id = id };
            _serviceManagerMock.Setup(s => s.AdoptionRequestService.GetAdoptionRequestByIdAsync(id)).ReturnsAsync(request);
            _mapperMock.Setup(m => m.Map<AdoptionRequestDto>(request)).Returns(requestDto);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(requestDto, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenRequestIsNull()
        {
            // Arrange
            int id = 2;
            _serviceManagerMock.Setup(s => s.AdoptionRequestService.GetAdoptionRequestByIdAsync(id)).ReturnsAsync((AdoptionRequest)null);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_SendsEmailToPetOwnerAndUser()
        {
            // Arrange
            var dto = new AdoptionRequestDto { PetId = 1, UserId = 2 };
            var entity = new AdoptionRequest { PetId = 1, UserId = 2 };
            var petOwner = new PetOwner { User = new User { Email = "owner@mail.com" } };
            var pet = new Pet { Name = "Petty" };
            var user = new User { Email = "user@mail.com" };
            _mapperMock.Setup(m => m.Map<AdoptionRequest>(dto)).Returns(entity);
            _serviceManagerMock.Setup(s => s.PetOwnerService.GetPetOwnerByPetIdAsync(1)).ReturnsAsync(petOwner);
            _serviceManagerMock.Setup(s => s.PetService.GetPetByIdAsync(1)).ReturnsAsync(pet);
            _serviceManagerMock.Setup(s => s.UserService.GetUserByIdAsync(2)).ReturnsAsync(user);
            _serviceManagerMock.Setup(s => s.AdoptionService.CreateAdoptionRequestAsync(It.IsAny<AdoptionRequest>())).Returns(Task.CompletedTask);
            // Act
            var result = await _controller.Create(dto);
            // Assert
            _emailServiceMock.Verify(e => e.SendEmailAsync("owner@mail.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _emailServiceMock.Verify(e => e.SendEmailAsync("user@mail.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
} 