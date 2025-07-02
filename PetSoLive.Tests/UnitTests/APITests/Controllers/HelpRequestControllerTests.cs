using Xunit;
using Moq;
using Petsolive.API.Controllers;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Petsolive.API.DTOs;
using PetSoLive.Core.Entities;

namespace PetSoLive.Tests.UnitTests.APITests.Controllers
{
    public class HelpRequestControllerTests
    {
        private readonly Mock<IServiceManager> _serviceManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly HelpRequestController _controller;

        public HelpRequestControllerTests()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _mapperMock = new Mock<IMapper>();
            _controller = new HelpRequestController(_serviceManagerMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithListOfHelpRequestDto()
        {
            // Arrange
            var helpRequests = new List<HelpRequest> { new HelpRequest { Id = 1 }, new HelpRequest { Id = 2 } };
            var helpRequestDtos = new List<HelpRequestDto> { new HelpRequestDto { Id = 1 }, new HelpRequestDto { Id = 2 } };
            _serviceManagerMock.Setup(s => s.HelpRequestService.GetHelpRequestsAsync()).ReturnsAsync(helpRequests);
            _mapperMock.Setup(m => m.Map<IEnumerable<HelpRequestDto>>(helpRequests)).Returns(helpRequestDtos);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(helpRequestDtos, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WithHelpRequestDto()
        {
            // Arrange
            int id = 1;
            var helpRequest = new HelpRequest { Id = id };
            var helpRequestDto = new HelpRequestDto { Id = id };
            _serviceManagerMock.Setup(s => s.HelpRequestService.GetHelpRequestByIdAsync(id)).ReturnsAsync(helpRequest);
            _mapperMock.Setup(m => m.Map<HelpRequestDto>(helpRequest)).Returns(helpRequestDto);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(helpRequestDto, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenHelpRequestIsNull()
        {
            // Arrange
            int id = 2;
            _serviceManagerMock.Setup(s => s.HelpRequestService.GetHelpRequestByIdAsync(id)).ReturnsAsync((HelpRequest)null);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsOk_OnSuccess()
        {
            // Arrange
            var dto = new HelpRequestDto { Id = 0 };
            var entity = new HelpRequest { Id = 1 };
            _mapperMock.Setup(m => m.Map<HelpRequest>(dto)).Returns(entity);
            _serviceManagerMock.Setup(s => s.HelpRequestService.CreateHelpRequestAsync(entity)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var dto = new HelpRequestDto();
            _controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = await _controller.Create(dto);

            // Assert
            Assert.IsType<BadRequestResult>(result); // Controller'da bu yok, eklenirse bu test çalışır
        }

        [Fact]
        public async Task Create_ThrowsException_WhenServiceFails()
        {
            // Arrange
            var dto = new HelpRequestDto { Id = 0 };
            var entity = new HelpRequest { Id = 1 };
            _mapperMock.Setup(m => m.Map<HelpRequest>(dto)).Returns(entity);
            _serviceManagerMock.Setup(s => s.HelpRequestService.CreateHelpRequestAsync(entity)).ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Create(dto));
        }

        [Fact]
        public async Task Update_ReturnsNoContent_OnSuccess()
        {
            // Arrange
            int id = 1;
            var dto = new HelpRequestDto { Id = id };
            var entity = new HelpRequest { Id = id };
            _mapperMock.Setup(m => m.Map<HelpRequest>(dto)).Returns(entity);
            _serviceManagerMock.Setup(s => s.HelpRequestService.UpdateHelpRequestAsync(entity)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(id, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            int id = 1;
            var dto = new HelpRequestDto();
            _controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = await _controller.Update(id, dto);

            // Assert
            Assert.IsType<BadRequestResult>(result); // Controller'da bu yok, eklenirse bu test çalışır
        }

        [Fact]
        public async Task Update_ThrowsException_WhenServiceFails()
        {
            // Arrange
            int id = 1;
            var dto = new HelpRequestDto { Id = id };
            var entity = new HelpRequest { Id = id };
            _mapperMock.Setup(m => m.Map<HelpRequest>(dto)).Returns(entity);
            _serviceManagerMock.Setup(s => s.HelpRequestService.UpdateHelpRequestAsync(entity)).ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Update(id, dto));
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_OnSuccess()
        {
            // Arrange
            int id = 1;
            _serviceManagerMock.Setup(s => s.HelpRequestService.DeleteHelpRequestAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ThrowsException_WhenServiceFails()
        {
            // Arrange
            int id = 2;
            _serviceManagerMock.Setup(s => s.HelpRequestService.DeleteHelpRequestAsync(id)).ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Delete(id));
        }
    }
} 