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
    public class UserControllerTests
    {
        private readonly Mock<IServiceManager> _serviceManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _mapperMock = new Mock<IMapper>();
            _controller = new UserController(_serviceManagerMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithListOfUserDto()
        {
            // Arrange
            var users = new List<User> { new User { Id = 1 }, new User { Id = 2 } };
            var userDtos = new List<UserDto> { new UserDto { Id = 1 }, new UserDto { Id = 2 } };
            _serviceManagerMock.Setup(s => s.UserService.GetAllUsersAsync()).ReturnsAsync(users);
            _mapperMock.Setup(m => m.Map<IEnumerable<UserDto>>(users)).Returns(userDtos);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(userDtos, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WithUserDto()
        {
            // Arrange
            int id = 1;
            var user = new User { Id = id };
            var userDto = new UserDto { Id = id };
            _serviceManagerMock.Setup(s => s.UserService.GetUserByIdAsync(id)).ReturnsAsync(user);
            _mapperMock.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(userDto, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenUserIsNull()
        {
            // Arrange
            int id = 2;
            _serviceManagerMock.Setup(s => s.UserService.GetUserByIdAsync(id)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_OnSuccess()
        {
            // Arrange
            int id = 1;
            var userDto = new UserDto { Id = id };
            var user = new User { Id = id };
            _mapperMock.Setup(m => m.Map<User>(userDto)).Returns(user);
            _serviceManagerMock.Setup(s => s.UserService.UpdateUserAsync(user)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(id, userDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
} 