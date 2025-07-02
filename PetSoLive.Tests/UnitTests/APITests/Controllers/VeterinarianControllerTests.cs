using Xunit;
using Moq;
using Petsolive.API.Controllers;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Petsolive.API.DTOs;
using PetSoLive.Core.Entities;

namespace PetSoLive.Tests.UnitTests.APITests.Controllers
{
    public class VeterinarianControllerTests
    {
        private readonly Mock<IServiceManager> _serviceManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly VeterinarianController _controller;

        public VeterinarianControllerTests()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _mapperMock = new Mock<IMapper>();
            _controller = new VeterinarianController(_serviceManagerMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithListOfVeterinarianDto()
        {
            // Arrange
            var vets = new List<Veterinarian> { new Veterinarian { Id = 1 }, new Veterinarian { Id = 2 } };
            var vetDtos = new List<VeterinarianDto> { new VeterinarianDto { Id = 1 }, new VeterinarianDto { Id = 2 } };
            _serviceManagerMock.Setup(s => s.VeterinarianService.GetAllVeterinariansAsync()).ReturnsAsync(vets);
            _mapperMock.Setup(m => m.Map<IEnumerable<VeterinarianDto>>(vets)).Returns(vetDtos);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(vetDtos, okResult.Value);
        }

        [Fact]
        public async Task Register_ReturnsOk_WithVeterinarianDto()
        {
            // Arrange
            var vetDto = new VeterinarianDto { UserId = 1, Qualifications = "Test", ClinicAddress = "Addr", ClinicPhoneNumber = "123" };
            var vet = new Veterinarian { Id = 1, UserId = 1 };
            var mappedVetDto = new VeterinarianDto { Id = 1, UserId = 1 };
            _serviceManagerMock.Setup(s => s.VeterinarianService.RegisterVeterinarianAsync(vetDto.UserId, vetDto.Qualifications, vetDto.ClinicAddress, vetDto.ClinicPhoneNumber)).ReturnsAsync(vet);
            _mapperMock.Setup(m => m.Map<VeterinarianDto>(vet)).Returns(mappedVetDto);

            // Act
            var result = await _controller.Register(vetDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mappedVetDto, okResult.Value);
        }

        [Fact]
        public async Task Approve_ReturnsNoContent_OnSuccess()
        {
            // Arrange
            int id = 1;
            _serviceManagerMock.Setup(s => s.VeterinarianService.ApproveVeterinarianAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Approve(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Reject_ReturnsNoContent_OnSuccess()
        {
            // Arrange
            int id = 2;
            _serviceManagerMock.Setup(s => s.VeterinarianService.RejectVeterinarianAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Reject(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
} 