using Xunit;
using Moq;
using Petsolive.API.Controllers;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetSoLive.API.DTOs;
using PetSoLive.Core.Entities;
using Petsolive.API.Helpers;

namespace PetSoLive.Tests.UnitTests.APITests.Controllers
{
    public class LostPetAdControllerTests
    {
        private readonly Mock<IServiceManager> _serviceManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ImgBBHelper> _imgBBHelperMock;
        private readonly Mock<ILostPetAdService> _lostPetAdServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly LostPetAdController _controller;

        public LostPetAdControllerTests()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _mapperMock = new Mock<IMapper>();
            _emailServiceMock = new Mock<IEmailService>();
            _imgBBHelperMock = new Mock<ImgBBHelper>("dummy_api_key");
            _lostPetAdServiceMock = new Mock<ILostPetAdService>();
            _userServiceMock = new Mock<IUserService>();
            _serviceManagerMock.SetupGet(s => s.EmailService).Returns(_emailServiceMock.Object);
            _serviceManagerMock.SetupGet(s => s.LostPetAdService).Returns(_lostPetAdServiceMock.Object);
            _serviceManagerMock.SetupGet(s => s.UserService).Returns(_userServiceMock.Object);
            _controller = new LostPetAdController(_serviceManagerMock.Object, _mapperMock.Object, _imgBBHelperMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithListOfLostPetAdDto()
        {
            // Arrange
            var ads = new List<LostPetAd> { new LostPetAd { Id = 1 }, new LostPetAd { Id = 2 } };
            var adDtos = new List<LostPetAdDto> { new LostPetAdDto { Id = 1 }, new LostPetAdDto { Id = 2 } };
            _lostPetAdServiceMock.Setup(s => s.GetAllLostPetAdsAsync()).ReturnsAsync(ads);
            _mapperMock.Setup(m => m.Map<IEnumerable<LostPetAdDto>>(ads)).Returns(adDtos);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(adDtos, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WithLostPetAdDto()
        {
            // Arrange
            int id = 1;
            var ad = new LostPetAd { Id = id };
            var adDto = new LostPetAdDto { Id = id };
            _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(id)).ReturnsAsync(ad);
            _mapperMock.Setup(m => m.Map<LostPetAdDto>(ad)).Returns(adDto);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(adDto, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenAdIsNull()
        {
            // Arrange
            int id = 2;
            _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(id)).ReturnsAsync((LostPetAd)null);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsOk_OnSuccess()
        {
            // Arrange
            var dto = new LostPetAdDto { Id = 0, LastSeenCity = "City", LastSeenDistrict = "District" };
            var entity = new LostPetAd { Id = 1 };
            _mapperMock.Setup(m => m.Map<LostPetAd>(dto)).Returns(entity);
            _lostPetAdServiceMock.Setup(s => s.CreateLostPetAdAsync(entity, dto.LastSeenCity, dto.LastSeenDistrict)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(dto, null);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var dto = new LostPetAdDto();
            _controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = await _controller.Create(dto, null);

            // Assert
            Assert.IsType<BadRequestResult>(result); // Controller'da bu yok, eklenirse bu test çalışır
        }

        [Fact]
        public async Task Create_ThrowsException_WhenServiceFails()
        {
            // Arrange
            var dto = new LostPetAdDto { Id = 0, LastSeenCity = "City", LastSeenDistrict = "District" };
            var entity = new LostPetAd { Id = 1 };
            _mapperMock.Setup(m => m.Map<LostPetAd>(dto)).Returns(entity);
            _lostPetAdServiceMock.Setup(s => s.CreateLostPetAdAsync(entity, dto.LastSeenCity, dto.LastSeenDistrict)).ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Create(dto, null));
        }

        [Fact]
        public async Task Create_SendsEmailToUser()
        {
            var dto = new LostPetAdDto { Id = 0, LastSeenCity = "City", LastSeenDistrict = "District", UserId = 2 };
            var entity = new LostPetAd { Id = 1, UserId = 2 };
            var user = new User { Id = 2, Email = "user@mail.com" };
            _mapperMock.Setup(m => m.Map<LostPetAd>(dto)).Returns(entity);
            _lostPetAdServiceMock.Setup(s => s.CreateLostPetAdAsync(entity, dto.LastSeenCity, dto.LastSeenDistrict)).Returns(Task.CompletedTask);
            _userServiceMock.Setup(s => s.GetUserByIdAsync(2)).ReturnsAsync(user);
            var result = await _controller.Create(dto, null);
            _emailServiceMock.Verify(e => e.SendEmailAsync("user@mail.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_OnSuccess()
        {
            // Arrange
            int id = 1;
            var dto = new LostPetAdDto { Id = id };
            var entity = new LostPetAd { Id = id };
            _mapperMock.Setup(m => m.Map<LostPetAd>(dto)).Returns(entity);
            _lostPetAdServiceMock.Setup(s => s.UpdateLostPetAdAsync(entity)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(id, dto, null);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            int id = 1;
            var dto = new LostPetAdDto();
            _controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = await _controller.Update(id, dto, null);

            // Assert
            Assert.IsType<BadRequestResult>(result); // Controller'da bu yok, eklenirse bu test çalışır
        }

        [Fact]
        public async Task Update_ThrowsException_WhenServiceFails()
        {
            // Arrange
            int id = 1;
            var dto = new LostPetAdDto { Id = id };
            var entity = new LostPetAd { Id = id };
            _mapperMock.Setup(m => m.Map<LostPetAd>(dto)).Returns(entity);
            _lostPetAdServiceMock.Setup(s => s.UpdateLostPetAdAsync(entity)).ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Update(id, dto, null));
        }

        [Fact]
        public async Task Update_SendsEmailToUser()
        {
            int id = 1;
            var dto = new LostPetAdDto { Id = id, UserId = 2 };
            var entity = new LostPetAd { Id = id, UserId = 2 };
            var user = new User { Id = 2, Email = "user@mail.com" };
            _mapperMock.Setup(m => m.Map<LostPetAd>(dto)).Returns(entity);
            _lostPetAdServiceMock.Setup(s => s.UpdateLostPetAdAsync(entity)).Returns(Task.CompletedTask);
            _userServiceMock.Setup(s => s.GetUserByIdAsync(2)).ReturnsAsync(user);
            var result = await _controller.Update(id, dto, null);
            _emailServiceMock.Verify(e => e.SendEmailAsync("user@mail.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_OnSuccess()
        {
            // Arrange
            int id = 1;
            var ad = new LostPetAd { Id = id };
            _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(id)).ReturnsAsync(ad);
            _lostPetAdServiceMock.Setup(s => s.DeleteLostPetAdAsync(ad)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenAdIsNull()
        {
            // Arrange
            int id = 2;
            _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(id)).ReturnsAsync((LostPetAd)null);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ThrowsException_WhenServiceFails()
        {
            // Arrange
            int id = 3;
            var ad = new LostPetAd { Id = id };
            _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(id)).ReturnsAsync(ad);
            _lostPetAdServiceMock.Setup(s => s.DeleteLostPetAdAsync(ad)).ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Delete(id));
        }

        [Fact]
        public async Task Delete_SendsEmailToUser()
        {
            int id = 1;
            var ad = new LostPetAd { Id = id, UserId = 2 };
            var user = new User { Id = 2, Email = "user@mail.com" };
            _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(id)).ReturnsAsync(ad);
            _lostPetAdServiceMock.Setup(s => s.DeleteLostPetAdAsync(ad)).Returns(Task.CompletedTask);
            _userServiceMock.Setup(s => s.GetUserByIdAsync(2)).ReturnsAsync(user);
            var result = await _controller.Delete(id);
            _emailServiceMock.Verify(e => e.SendEmailAsync("user@mail.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
} 