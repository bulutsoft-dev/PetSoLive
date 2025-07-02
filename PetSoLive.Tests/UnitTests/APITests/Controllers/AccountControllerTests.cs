using Xunit;
using Moq;
using Petsolive.API.Controllers;
using Petsolive.API.DTOs;
using Petsolive.API.Helpers;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetSoLive.Tests.UnitTests.APITests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<IServiceManager> _serviceManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Petsolive.API.Controllers.AccountController _controller;

        public AccountControllerTests()
        {
            // Set required JWT environment variables for JwtHelper
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "MySuperSecretKey123!MySuperSecretKey123!");
            Environment.SetEnvironmentVariable("JWT_ISSUER", "PetsoliveAPI");
            Environment.SetEnvironmentVariable("JWT_AUDIENCE", "PetsoliveClient");
            Environment.SetEnvironmentVariable("JWT_EXPIRE_MINUTES", "60");

            _serviceManagerMock = new Mock<IServiceManager>();
            _mapperMock = new Mock<IMapper>();
            _controller = new Petsolive.API.Controllers.AccountController(_serviceManagerMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenModelIsValid()
        {
            // Arrange
            var registerDto = new RegisterDto { Username = "test", Password = "1234", Email = "test@example.com" };
            var user = new User { Username = "test", Email = "test@example.com" };
            _mapperMock.Setup(m => m.Map<User>(registerDto)).Returns(user);
            _serviceManagerMock.Setup(s => s.UserService.RegisterAsync(user)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("User registered successfully", okResult.Value.ToString());
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            var registerDto = new RegisterDto();
            _controller.ModelState.AddModelError("Username", "Required");

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid input data", badRequest.Value.ToString());
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_OnArgumentException()
        {
            // Arrange
            var registerDto = new RegisterDto { Username = "test" };
            var user = new User { Username = "test" };
            _mapperMock.Setup(m => m.Map<User>(registerDto)).Returns(user);
            _serviceManagerMock.Setup(s => s.UserService.RegisterAsync(user)).ThrowsAsync(new ArgumentException("Username exists"));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Username exists", badRequest.Value.ToString());
        }

        [Fact]
        public async Task Login_ReturnsOk_WithTokenAndUser_WhenCredentialsAreValid()
        {
            // Arrange
            var loginDto = new AuthDto { Username = "test", Password = "1234" };
            var user = new User { Id = 1, Username = "test", Roles = new List<string> { "User" } };
            var userDto = new UserDto { Id = 1, Username = "test" };

            _serviceManagerMock.Setup(s => s.UserService.AuthenticateAsync(loginDto.Username, loginDto.Password)).ReturnsAsync(user);
            _mapperMock.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<AuthResponseDto>(okResult.Value);
            Assert.NotNull(response.Token);
            Assert.Equal(userDto.Username, response.User.Username);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var loginDto = new AuthDto { Username = "test", Password = "wrong" };
            _serviceManagerMock.Setup(s => s.UserService.AuthenticateAsync(loginDto.Username, loginDto.Password)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("Invalid credentials", unauthorized.Value);
        }
    }
} 