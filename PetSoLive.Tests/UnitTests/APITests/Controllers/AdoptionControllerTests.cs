using Xunit;
using Moq;
using Petsolive.API.Controllers;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Petsolive.API.DTOs;
using PetSoLive.Core.Entities;

namespace PetSoLive.Tests.UnitTests.APITests.Controllers
{
    public class AdoptionControllerTests
    {
        private readonly Mock<IServiceManager> _serviceManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AdoptionController _controller;

        public AdoptionControllerTests()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _mapperMock = new Mock<IMapper>();
            _controller = new AdoptionController(_serviceManagerMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetByPetId_ReturnsOk_WithAdoptionDto()
        {
            // Arrange
            int petId = 1;
            var adoption = new Adoption { Id = 10, PetId = petId };
            var adoptionDto = new AdoptionDto { Id = 10 };
            _serviceManagerMock.Setup(s => s.AdoptionService.GetAdoptionByPetIdAsync(petId)).ReturnsAsync(adoption);
            _mapperMock.Setup(m => m.Map<AdoptionDto>(adoption)).Returns(adoptionDto);

            // Act
            var result = await _controller.GetByPetId(petId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(adoptionDto, okResult.Value);
        }

        [Fact]
        public async Task GetByPetId_ReturnsNotFound_WhenAdoptionIsNull()
        {
            // Arrange
            int petId = 2;
            _serviceManagerMock.Setup(s => s.AdoptionService.GetAdoptionByPetIdAsync(petId)).ReturnsAsync((Adoption)null);

            // Act
            var result = await _controller.GetByPetId(petId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsOk_OnSuccess()
        {
            // Arrange
            var adoptionDto = new AdoptionDto { Id = 1 };
            var adoption = new Adoption { Id = 1 };
            _mapperMock.Setup(m => m.Map<Adoption>(adoptionDto)).Returns(adoption);
            _serviceManagerMock.Setup(s => s.AdoptionService.CreateAdoptionAsync(adoption)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(adoptionDto);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var adoptionDto = new AdoptionDto();
            _controller.ModelState.AddModelError("PetId", "Required");

            // Act
            var result = await _controller.Create(adoptionDto);

            // Assert
            Assert.IsType<BadRequestResult>(result); // Controller'da bu yok, eklenirse bu test çalışır
        }

        [Fact]
        public async Task Create_ThrowsException_Returns500OrThrows()
        {
            // Arrange
            var adoptionDto = new AdoptionDto { Id = 1 };
            var adoption = new Adoption { Id = 1 };
            _mapperMock.Setup(m => m.Map<Adoption>(adoptionDto)).Returns(adoption);
            _serviceManagerMock.Setup(s => s.AdoptionService.CreateAdoptionAsync(adoption)).ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Create(adoptionDto));
        }
    }
} 