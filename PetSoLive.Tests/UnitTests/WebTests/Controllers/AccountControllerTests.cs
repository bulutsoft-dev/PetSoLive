using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PetSoLive.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<IServiceManager> _serviceManagerMock;
        private readonly Mock<IStringLocalizer<AccountController>> _localizerMock;
        private readonly Mock<ISession> _sessionMock;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _localizerMock = new Mock<IStringLocalizer<AccountController>>();
            _sessionMock = new Mock<ISession>();

            var httpContext = new DefaultHttpContext
            {
                Session = _sessionMock.Object
            };

            _controller = new AccountController(_serviceManagerMock.Object, _localizerMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };
        }

        [Fact]
        public void Login_Get_ReturnsView()
        {
            // Act
            var result = _controller.Login();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName); // Default view (Login.cshtml)
        }

        [Fact]
        public async Task Login_Post_EmptyCredentials_ReturnsViewWithError()
        {
            // Arrange
            string username = "";
            string password = "";

            // Act
            var result = await _controller.Login(username, password);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains(_controller.ModelState[""].Errors, e => e.ErrorMessage == "Username and password are required.");
        }

        [Fact]
        public async Task Login_Post_InvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            string username = "testuser";
            string password = "wrongpassword";
            _serviceManagerMock.Setup(m => m.UserService.AuthenticateAsync(username, password)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.Login(username, password);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains(_controller.ModelState[""].Errors, e => e.ErrorMessage == "Invalid username or password.");
        }

        [Fact]
        public async Task Login_Post_ValidCredentials_RedirectsToHome()
        {
            // Arrange
            string username = "testuser";
            string password = "correctpassword";
            var user = new User { Id = 1, Username = username };

            _serviceManagerMock.Setup(m => m.UserService.AuthenticateAsync(username, password)).ReturnsAsync(user);

            // Session mock'lama
            _sessionMock.Setup(s => s.SetString(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            _sessionMock.Setup(s => s.SetInt32(It.IsAny<string>(), It.IsAny<int>())).Verifiable();

            // Act
            var result = await _controller.Login(username, password);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
            _sessionMock.Verify(s => s.SetString("Username", username), Times.Once());
            _sessionMock.Verify(s => s.SetInt32("UserId", user.Id), Times.Once());
        }

        [Fact]
        public void Register_Get_ReturnsViewWithCities()
        {
            // Act
            var result = _controller.Register();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName); // Default view (Register.cshtml)
            Assert.NotNull(viewResult.ViewData["Cities"]);
            Assert.IsType<List<string>>(viewResult.ViewData["Districts"]);
            Assert.Empty((List<string>)viewResult.ViewData["Districts"]);
        }

        [Fact]
        public async Task Register_Post_InvalidModelState_ReturnsViewWithCitiesAndDistricts()
        {
            // Arrange
            _controller.ModelState.AddModelError("Username", "Username is required.");
            string city = "TestCity";
            var userData = new
            {
                Username = "",
                Email = "test@example.com",
                Password = "password",
                PhoneNumber = "1234567890",
                Address = "123 Test St.",
                DateOfBirth = DateTime.Now.AddYears(-30),
                City = city,
                District = "TestDistrict"
            };

            // Simüle edilmiş CityList verisi
            var districts = new List<string> { "District1", "District2" };
            CityList.GetDistrictsByCity = (c) => c == city ? districts : new List<string>();

            // Act
            var result = await _controller.Register(
                userData.Username,
                userData.Email,
                userData.Password,
                userData.PhoneNumber,
                userData.Address,
                userData.DateOfBirth,
                userData.City,
                userData.District
            );

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.NotNull(viewResult.ViewData["Cities"]);
            Assert.Equal(districts, viewResult.ViewData["Districts"]);
        }

        [Fact]
        public async Task Register_Post_ValidModelState_RedirectsToLogin()
        {
            // Arrange
            var userData = new
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password",
                PhoneNumber = "1234567890",
                Address = "123 Test St.",
                DateOfBirth = DateTime.Now.AddYears(-30),
                City = "TestCity",
                District = "TestDistrict"
            };

            _serviceManagerMock.Setup(m => m.UserService.RegisterAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            // Simüle edilmiş CityList verisi
            CityList.GetDistrictsByCity = (c) => new List<string>();

            // Act
            var result = await _controller.Register(
                userData.Username,
                userData.Email,
                userData.Password,
                userData.PhoneNumber,
                userData.Address,
                userData.DateOfBirth,
                userData.City,
                userData.District
            );

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            _serviceManagerMock.Verify(m => m.UserService.RegisterAsync(It.Is<User>(u =>
                u.Username == userData.Username &&
                u.Email == userData.Email &&
                u.PasswordHash == userData.Password &&
                u.PhoneNumber == userData.PhoneNumber &&
                u.Address == userData.Address &&
                u.DateOfBirth == userData.DateOfBirth &&
                u.City == userData.City &&
                u.District == userData.District &&
                u.ProfileImageUrl == "https://www.petsolive.com.tr/"
            )), Times.Once());
        }

        [Fact]
        public void GetDistricts_ReturnsJsonWithDistricts()
        {
            // Arrange
            string city = "TestCity";
            var districts = new List<string> { "District1", "District2" };
            CityList.GetDistrictsByCity = (c) => c == city ? districts : new List<string>();

            // Act
            var result = _controller.GetDistricts(city);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal(districts, jsonResult.Value);
        }

        [Fact]
        public void Logout_ClearsSession_RedirectsToLogin()
        {
            // Arrange
            _sessionMock.Setup(s => s.Clear()).Verifiable();

            // Act
            var result = _controller.Logout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            _sessionMock.Verify(s => s.Clear(), Times.Once());
        }
    }

    // CityList için geçici simülasyon
    public static class CityList
    {
        public static List<string> Cities => new List<string> { "TestCity" };
        public static Func<string, List<string>> GetDistrictsByCity { get; set; } = (city) => new List<string>();
    }
}