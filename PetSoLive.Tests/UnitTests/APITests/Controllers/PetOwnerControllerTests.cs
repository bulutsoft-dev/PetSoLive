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
    public class PetOwnerControllerTests
    {
        private readonly Mock<IServiceManager> _serviceManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly PetOwnerController _controller;

        public PetOwnerControllerTests()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _mapperMock = new Mock<IMapper>();
            _controller = new PetOwnerController(_serviceManagerMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetByPetId_ReturnsOk_WithPetOwnerDto()
        {
            // Arrange
            int petId = 1;
            var petOwner = new PetOwner { PetId = petId };
            var petOwnerDto = new PetOwnerDto { PetId = petId };
            _serviceManagerMock.Setup(s => s.PetOwnerService.GetPetOwnerByPetIdAsync(petId)).ReturnsAsync(petOwner);
            _mapperMock.Setup(m => m.Map<PetOwnerDto>(petOwner)).Returns(petOwnerDto);

            // Act
            var result = await _controller.GetByPetId(petId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(petOwnerDto, okResult.Value);
        }

        [Fact]
        public async Task GetByPetId_ReturnsNotFound_WhenPetOwnerIsNull()
        {
            // Arrange
            int petId = 2;
            _serviceManagerMock.Setup(s => s.PetOwnerService.GetPetOwnerByPetIdAsync(petId)).ReturnsAsync((PetOwner)null);

            // Act
            var result = await _controller.GetByPetId(petId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
} 