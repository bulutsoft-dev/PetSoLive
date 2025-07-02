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
        private readonly AdoptionRequestController _controller;

        public AdoptionRequestControllerTests()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _mapperMock = new Mock<IMapper>();
            _controller = new AdoptionRequestController(_serviceManagerMock.Object, _mapperMock.Object);
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
    }
} 