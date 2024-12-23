using Microsoft.AspNetCore.Mvc;
using PetSoLive.Web.Controllers;
using Xunit;

namespace PetSoLive.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            // Initialize the HomeController
            _controller = new HomeController();
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