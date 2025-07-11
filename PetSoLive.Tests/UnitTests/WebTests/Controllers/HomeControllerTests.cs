using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using PetSoLive.Web.Controllers;
using Xunit;
using Microsoft.Extensions.Logging;
using PetSoLive.Data;

namespace PetSoLive.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            // Mock localizer
            var localizerMock = new Mock<IStringLocalizer<HomeController>>();
            localizerMock.Setup(l => l["Home Page"]).Returns(new LocalizedString("Home Page", "Home Page"));
            var loggerMock = new Mock<ILogger<HomeController>>();
            var dbContextMock = new Mock<ApplicationDbContext>();

            _controller = new HomeController(localizerMock.Object, loggerMock.Object, dbContextMock.Object);
        }

        [Fact]
        public void Index_ReturnsView()
        {
            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);  // Verify it returns a ViewResult
            Assert.Null(viewResult.ViewName);  // Ensure the default view is returned
        }

        [Fact]
        public void About_ReturnsView()
        {
            // Act
            var result = _controller.About();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);  // Verify it returns a ViewResult
            Assert.Null(viewResult.ViewName);  // Ensure the default view is returned
        }
    }
}