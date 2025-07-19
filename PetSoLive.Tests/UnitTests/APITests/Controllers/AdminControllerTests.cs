using Xunit;
using Moq;
using Petsolive.API.Controllers;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using PetSoLive.Data;
using Microsoft.EntityFrameworkCore;

namespace PetSoLive.Tests.UnitTests.APITests.Controllers
{
    public class AdminControllerTests
    {
        private readonly Mock<IServiceManager> _serviceManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ApplicationDbContext _dbContext;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _mapperMock = new Mock<IMapper>();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ApplicationDbContext(options);
            _controller = new AdminController(_serviceManagerMock.Object, _mapperMock.Object, _dbContext);
        }

        [Fact]
        public async Task IsUserAdmin_ReturnsOkTrue_WhenUserIsAdmin()
        {
            // Arrange
            int userId = 1;
            _serviceManagerMock.Setup(s => s.AdminService.IsUserAdminAsync(userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.IsUserAdmin(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True((bool)okResult.Value);
        }

        [Fact]
        public async Task IsUserAdmin_ReturnsOkFalse_WhenUserIsNotAdmin()
        {
            // Arrange
            int userId = 2;
            _serviceManagerMock.Setup(s => s.AdminService.IsUserAdminAsync(userId)).ReturnsAsync(false);

            // Act
            var result = await _controller.IsUserAdmin(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.False((bool)okResult.Value);
        }

        [Fact]
        public async Task IsUserAdmin_ThrowsException_Returns500()
        {
            // Arrange
            int userId = 3;
            _serviceManagerMock.Setup(s => s.AdminService.IsUserAdminAsync(userId)).ThrowsAsync(new Exception("DB error"));

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(() => _controller.IsUserAdmin(userId));

            // Assert
            Assert.Equal("DB error", exception.Message);
        }
    }
} 