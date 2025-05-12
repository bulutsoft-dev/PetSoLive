using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Web.Controllers;
using Xunit;

namespace PetSoLive.Tests.Controllers;

public class LostPetAdControllerTests
{
    private readonly Mock<IServiceManager> _serviceManagerMock;
    private readonly Mock<IStringLocalizer<LostPetAdController>> _localizerMock;
    private readonly Mock<ILostPetAdService> _lostPetAdServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ISession> _sessionMock;
    private readonly LostPetAdController _controller;
    private readonly DefaultHttpContext _httpContext;

    public LostPetAdControllerTests()
    {
        _serviceManagerMock = new Mock<IServiceManager>();
        _localizerMock = new Mock<IStringLocalizer<LostPetAdController>>();
        _lostPetAdServiceMock = new Mock<ILostPetAdService>();
        _userServiceMock = new Mock<IUserService>();
        _emailServiceMock = new Mock<IEmailService>();
        _sessionMock = new Mock<ISession>();

        // Setup IServiceManager to return mocked services
        _serviceManagerMock.SetupGet(m => m.LostPetAdService).Returns(_lostPetAdServiceMock.Object);
        _serviceManagerMock.SetupGet(m => m.UserService).Returns(_userServiceMock.Object);
        _serviceManagerMock.SetupGet(m => m.EmailService).Returns(_emailServiceMock.Object);

        // Setup IStringLocalizer
        _localizerMock.Setup(l => l[It.IsAny<string>()])
            .Returns<string>(name => new LocalizedString(name, name));
        _localizerMock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns<string, object[]>((name, args) => new LocalizedString(name, string.Format(name, args)));

        _httpContext = new DefaultHttpContext
        {
            Session = _sessionMock.Object
        };

        _controller = new LostPetAdController(_serviceManagerMock.Object, _localizerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            }
        };
    }

    #region GetDistrictsByCity
    [Fact]
    public void GetDistrictsByCity_WhenCalled_ReturnsJsonResultWithDistricts()
    {
        // Arrange
        var city = "İzmir";

        // Act
        var result = _controller.GetDistrictsByCity(city);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var districts = Assert.IsType<List<string>>(jsonResult.Value);
        Assert.NotNull(districts);
        Assert.Contains("Karşıyaka", districts);
        Assert.Contains("Konak", districts);
        Assert.Contains("Bornova", districts);
    }
    #endregion

    #region Create (GET)
    [Fact]
    public void Create_Get_WhenNotLoggedIn_RedirectsToLogin()
    {
        // Arrange
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

        // Act
        var result = _controller.Create();

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("Account", redirectResult.ControllerName);
    }

    [Fact]
    public void Create_Get_WhenLoggedIn_ReturnsViewWithCities()
    {
        // Arrange
        var username = "TestUser";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));

        // Act
        var result = _controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
        Assert.True(viewResult.ViewData.ContainsKey("Cities"));
        Assert.True(viewResult.ViewData.ContainsKey("Districts"));
        Assert.IsType<List<string>>(viewResult.ViewData["Districts"]);
        Assert.Empty((List<string>)viewResult.ViewData["Districts"]);
    }
    #endregion

    #region Create (POST)
    [Fact]
    public async Task Create_Post_WhenNotLoggedIn_RedirectsToLogin()
    {
        // Arrange
        var lostPetAd = new LostPetAd { PetName = "Fluffy" };
        var city = "İzmir";
        var district = "Bornova";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

        // Act
        var result = await _controller.Create(lostPetAd, city, district);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("Account", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Create_Post_WhenValid_CreatesAdAndRedirects()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);

        var lostPetAd = new LostPetAd
        {
            PetName = "Fluffy",
            Description = "Lost cat",
            LastSeenDate = DateTime.Now,
            ImageUrl = "fluffy.jpg"
        };
        var city = "İzmir";
        var district = "Bornova";
        _lostPetAdServiceMock.Setup(s => s.CreateLostPetAdAsync(It.IsAny<LostPetAd>(), city, district))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(lostPetAd, city, district);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        _lostPetAdServiceMock.Verify(s => s.CreateLostPetAdAsync(It.Is<LostPetAd>(ad =>
            ad.UserId == user.Id &&
            ad.PetName == "Fluffy" &&
            ad.LastSeenCity == "İzmir" &&
            ad.LastSeenDistrict == "Bornova"
        ), city, district), Times.Once());
        Assert.Equal("AdCreatedSuccess", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task Create_Post_WhenCityOrDistrictMissing_ReturnsViewWithError()
    {
        // Arrange
        var username = "TestUser";
        var user = new User { Id = 1, Username = username };
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));
        _userServiceMock.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);

        var lostPetAd = new LostPetAd { PetName = "Fluffy" };
        var city = "";
        var district = "Bornova";

        // Act
        var result = await _controller.Create(lostPetAd, city, district);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(lostPetAd, viewResult.Model);
        Assert.True(viewResult.ViewData.ContainsKey("Cities"));
        Assert.True(viewResult.ViewData.ContainsKey("Districts"));
        Assert.Empty((List<string>)viewResult.ViewData["Districts"]);
        Assert.Equal("CityAndDistrictRequired", _controller.TempData["ErrorMessage"]);
    }
    #endregion

    #region Index
    [Fact]
    public async Task Index_WhenCalled_ReturnsViewWithLostPetAds()
    {
        // Arrange
        var ads = new List<LostPetAd>
        {
            new LostPetAd { Id = 1, PetName = "Cat" },
            new LostPetAd { Id = 2, PetName = "Dog" }
        };
        _lostPetAdServiceMock.Setup(s => s.GetAllLostPetAdsAsync()).ReturnsAsync(ads);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(ads, viewResult.Model);
    }

    [Fact]
    public async Task Index_WhenAdsNull_ReturnsViewWithEmptyListAndError()
    {
        // Arrange
        _lostPetAdServiceMock.Setup(s => s.GetAllLostPetAdsAsync()).ReturnsAsync((List<LostPetAd>)null);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<LostPetAd>>(viewResult.Model);
        Assert.Empty(model);
        Assert.Equal("RetrieveAdsError", _controller.TempData["ErrorMessage"]);
    }
    #endregion

    #region Details
    [Fact]
    public async Task Details_WhenAdNotFound_RedirectsToIndexWithError()
    {
        // Arrange
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10)).ReturnsAsync((LostPetAd)null);

        // Act
        var result = await _controller.Details(10);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("AdNotFound", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Details_WhenAdFound_ReturnsViewWithAd()
    {
        // Arrange
        var ad = new LostPetAd
        {
            Id = 10,
            PetName = "Birdy",
            User = new User { Username = "AdOwner" }
        };
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10)).ReturnsAsync(ad);
        var username = "AnotherUser";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));

        // Act
        var result = await _controller.Details(10);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(ad, viewResult.Model);
        Assert.Equal("AnotherUser", viewResult.ViewData["CurrentUser"]);
    }
    #endregion

    #region Edit (GET)
    [Fact]
    public async Task Edit_Get_WhenNotLoggedIn_RedirectsToLogin()
    {
        // Arrange
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("Account", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Edit_Get_WhenAdNotFound_RedirectsToIndexWithError()
    {
        // Arrange
        var username = "AdOwner";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10)).ReturnsAsync((LostPetAd)null);

        // Act
        var result = await _controller.Edit(10);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("AdNotFound", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Edit_Get_WhenNotOwner_RedirectsToIndexWithError()
    {
        // Arrange
        var username = "NotOwner";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));
        var ad = new LostPetAd
        {
            Id = 10,
            User = new User { Username = "AdOwner" },
            LastSeenCity = "Manisa"
        };
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10)).ReturnsAsync(ad);

        // Act
        var result = await _controller.Edit(10);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("EditPermissionDenied", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Edit_Get_WhenValidOwner_ReturnsViewWithAd()
    {
        // Arrange
        var username = "AdOwner";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));
        var ad = new LostPetAd
        {
            Id = 10,
            User = new User { Username = "AdOwner" },
            LastSeenCity = "Manisa"
        };
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10)).ReturnsAsync(ad);

        // Act
        var result = await _controller.Edit(10);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(ad, viewResult.Model);
        Assert.True(viewResult.ViewData.ContainsKey("Cities"));
        Assert.True(viewResult.ViewData.ContainsKey("Districts"));
        Assert.IsType<List<string>>(viewResult.ViewData["Districts"]);
    }
    #endregion

    #region Edit (POST)
    [Fact]
    public async Task Edit_Post_WhenNotLoggedIn_RedirectsToLogin()
    {
        // Arrange
        var ad = new LostPetAd { Id = 10 };
        var city = "İzmir";
        var district = "Bornova";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

        // Act
        var result = await _controller.Edit(10, ad, city, district);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("Account", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Edit_Post_WhenIdMismatch_RedirectsToIndexWithError()
    {
        // Arrange
        var username = "AdOwner";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));
        var ad = new LostPetAd { Id = 11 }; // Mismatched ID
        var city = "İzmir";
        var district = "Bornova";

        // Act
        var result = await _controller.Edit(10, ad, city, district);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("InvalidAdId", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Edit_Post_WhenAdNotFound_RedirectsToIndexWithError()
    {
        // Arrange
        var username = "AdOwner";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));
        var ad = new LostPetAd { Id = 10 };
        var city = "İzmir";
        var district = "Bornova";
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10)).ReturnsAsync((LostPetAd)null);

        // Act
        var result = await _controller.Edit(10, ad, city, district);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("AdNotFound", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Edit_Post_WhenNotOwner_RedirectsToIndexWithError()
    {
        // Arrange
        var username = "NotOwner";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));
        var existingAd = new LostPetAd
        {
            Id = 10,
            User = new User { Username = "AdOwner" }
        };
        var updatedAd = new LostPetAd { Id = 10 };
        var city = "İzmir";
        var district = "Bornova";
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10)).ReturnsAsync(existingAd);

        // Act
        var result = await _controller.Edit(10, updatedAd, city, district);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("EditPermissionDenied", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Edit_Post_WhenValid_UpdatesAdAndRedirects()
    {
        // Arrange
        var username = "AdOwner";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));
        var existingAd = new LostPetAd
        {
            Id = 10,
            PetName = "OldName",
            Description = "OldDesc",
            LastSeenCity = "OldCity",
            LastSeenDistrict = "OldDistrict",
            ImageUrl = "old.jpg",
            LastSeenDate = DateTime.Now.AddDays(-1),
            User = new User { Username = "AdOwner" }
        };
        var updatedAd = new LostPetAd
        {
            Id = 10,
            PetName = "NewName",
            Description = "NewDesc",
            ImageUrl = "new.jpg",
            LastSeenDate = DateTime.Now
        };
        var city = "İzmir";
        var district = "Bornova";
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10)).ReturnsAsync(existingAd);
        _lostPetAdServiceMock.Setup(s => s.UpdateLostPetAdAsync(It.IsAny<LostPetAd>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Edit(10, updatedAd, city, district);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
        Assert.Equal(10, redirectResult.RouteValues["id"]);
        _lostPetAdServiceMock.Verify(s => s.UpdateLostPetAdAsync(It.Is<LostPetAd>(ad =>
            ad.PetName == "NewName" &&
            ad.Description == "NewDesc" &&
            ad.LastSeenCity == "İzmir" &&
            ad.LastSeenDistrict == "Bornova" &&
            ad.ImageUrl == "new.jpg"
        )), Times.Once());
        Assert.Equal("AdUpdatedSuccess", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task Edit_Post_WhenUpdateFails_RedirectsToIndexWithError()
    {
        // Arrange
        var username = "AdOwner";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));
        var existingAd = new LostPetAd
        {
            Id = 10,
            User = new User { Username = "AdOwner" }
        };
        var updatedAd = new LostPetAd { Id = 10 };
        var city = "İzmir";
        var district = "Bornova";
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10)).ReturnsAsync(existingAd);
        _lostPetAdServiceMock.Setup(s => s.UpdateLostPetAdAsync(It.IsAny<LostPetAd>()))
            .ThrowsAsync(new Exception("Update failed"));

        // Act
        var result = await _controller.Edit(10, updatedAd, city, district);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.StartsWith("UpdateAdError", (string)_controller.TempData["ErrorMessage"]);
    }
    #endregion

    #region Delete (GET)
    [Fact]
    public async Task Delete_Get_WhenNotLoggedIn_RedirectsToIndexWithError()
    {
        // Arrange
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

        // Act
        var result = await _controller.Delete(10);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("AdNotFound", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Delete_Get_WhenAdNotFound_RedirectsToIndexWithError()
    {
        // Arrange
        var username = "Owner";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10)).ReturnsAsync((LostPetAd)null);

        // Act
        var result = await _controller.Delete(10);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("AdNotFound", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Delete_Get_WhenUserNull_RedirectsToIndexWithError()
    {
        // Arrange
        var username = "Owner";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));
        var ad = new LostPetAd { Id = 10, User = null };
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10)).ReturnsAsync(ad);

        // Act
        var result = await _controller.Delete(10);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("UserNotFound", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Delete_Get_WhenNotOwner_RedirectsToIndexWithError()
    {
        // Arrange
        var username = "NotOwner";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));
        var ad = new LostPetAd { Id = 10, User = new User { Username = "Owner" } };
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10)).ReturnsAsync(ad);

        // Act
        var result = await _controller.Delete(10);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("DeletePermissionDenied", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Delete_Get_WhenValidOwner_ReturnsView()
    {
        // Arrange
        var username = "Owner";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));
        var ad = new LostPetAd { Id = 10, User = new User { Username = "Owner" } };
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10)).ReturnsAsync(ad);

        // Act
        var result = await _controller.Delete(10);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(ad, viewResult.Model);
        Assert.Equal("DeleteConfirmation", _controller.TempData["DeleteMessage"]);
    }
    #endregion

    #region Delete (POST)
    [Fact]
    public async Task Delete_Post_WhenNotLoggedIn_RedirectsToIndexWithError()
    {
        // Arrange
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny)).Returns(false);

        // Act
        var result = await _controller.DeleteConfirmed(10);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("AdNotFound", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Delete_Post_WhenAdNotFound_RedirectsToIndexWithError()
    {
        // Arrange
        var username = "Owner";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10)).ReturnsAsync((LostPetAd)null);

        // Act
        var result = await _controller.DeleteConfirmed(10);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("AdNotFound", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Delete_Post_WhenUserNull_RedirectsToIndexWithError()
    {
        // Arrange
        var username = "Owner";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));
        var ad = new LostPetAd { Id = 10, User = null };
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10)).ReturnsAsync(ad);

        // Act
        var result = await _controller.DeleteConfirmed(10);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("UserNotFound", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Delete_Post_WhenNotOwner_RedirectsToIndexWithError()
    {
        // Arrange
        var username = "NotOwner";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));
        var ad = new LostPetAd { Id = 10, User = new User { Username = "Owner" } };
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10)).ReturnsAsync(ad);

        // Act
        var result = await _controller.DeleteConfirmed(10);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("DeletePermissionDenied", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Delete_Post_WhenValid_DeletesAdAndRedirects()
    {
        // Arrange
        var username = "Owner";
        _sessionMock.Setup(s => s.TryGetValue("Username", out It.Ref<byte[]>.IsAny))
            .Returns(true)
            .Callback((string key, out byte[] value) => value = Encoding.UTF8.GetBytes(username));
        var ad = new LostPetAd { Id = 10, User = new User { Username = "Owner" } };
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10)).ReturnsAsync(ad);
        _lostPetAdServiceMock.Setup(s => s.DeleteLostPetAdAsync(It.IsAny<LostPetAd>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteConfirmed(10);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        _lostPetAdServiceMock.Verify(s => s.DeleteLostPetAdAsync(ad), Times.Once());
        Assert.Equal("AdDeletedSuccess", _controller.TempData["SuccessMessage"]);
    }
    #endregion
}