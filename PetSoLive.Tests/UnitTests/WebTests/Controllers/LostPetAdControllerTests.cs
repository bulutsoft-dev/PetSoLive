using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Web.Controllers;
using Xunit;

namespace PetSoLive.Tests.Controllers;

public class LostPetAdControllerTests
{
    
    private readonly Mock<ILostPetAdService> _lostPetAdServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IUserService> _userServiceMock;

    private readonly LostPetAdController _controller;
    private readonly DefaultHttpContext _httpContext;

    public LostPetAdControllerTests()
    {
        _lostPetAdServiceMock = new Mock<ILostPetAdService>();
        _emailServiceMock = new Mock<IEmailService>();
        _userServiceMock = new Mock<IUserService>();

        _controller = new LostPetAdController(
            _lostPetAdServiceMock.Object,
            _userServiceMock.Object,
            _emailServiceMock.Object,
            null
        );

        // Create a test HttpContext with an in-memory session
        _httpContext = new DefaultHttpContext
        {
            Session = new LostPetTestSession() 
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContext
        };
    }

    #region GetDistrictsByCity
    [Fact]
    public void GetDistrictsByCity_WhenCalled_ReturnsJsonResultWithDistricts()
    {
        // Arrange
        var city = "İzmir";

        // Act
        var result = _controller.GetDistrictsByCity(city) as JsonResult;

        // Assert
        Assert.NotNull(result);
        Assert.IsType<JsonResult>(result);
        var districts = result.Value as List<string>;
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
        // No "Username" in session => not logged in

        // Act
        var result = _controller.Create() as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Login", result.ActionName);
        Assert.Equal("Account", result.ControllerName);
    }

    [Fact]
    public void Create_Get_WhenLoggedIn_ReturnsViewWithCities()
    {
        // Arrange
        _httpContext.Session.SetString("Username", "TestUser"); // user is logged in

        // Act
        var result = _controller.Create() as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.ViewName);  // Default view
        Assert.True(result.ViewData.ContainsKey("Cities"));
        Assert.True(result.ViewData.ContainsKey("Districts"));
    }
    #endregion
    
    [Fact]
    public async Task Create_Post_WhenNotLoggedIn_RedirectsToLogin()
    {
        // Arrange
        var lostPetAd = new LostPetAd { PetName = "Fluffy" };
        var city = "İzmir";
        var district = "Bornova";

        // Not logged in => no "Username" in session

        // Act
        var result = await _controller.Create(lostPetAd, city, district) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Login", result.ActionName);
        Assert.Equal("Account", result.ControllerName);
    }


    

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
        _lostPetAdServiceMock.Setup(s => s.GetAllLostPetAdsAsync())
            .ReturnsAsync(ads);

        // Act
        var result = await _controller.Index() as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ads, result.Model);
    }
    #endregion

    #region Details

    [Fact]
    public async Task Details_WhenAdFound_ReturnsViewWithAd()
    {
        // Arrange
        var ad = new LostPetAd { Id = 10, PetName = "Birdy", User = new User { Username = "AdOwner" } };
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10))
            .ReturnsAsync(ad);
        _httpContext.Session.SetString("Username", "AnotherUser");

        // Act
        var result = await _controller.Details(10) as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ad, result.Model);
        Assert.Equal("AnotherUser", _controller.ViewBag.CurrentUser);
    }
    #endregion

    #region Edit (GET)
    [Fact]
    public async Task Edit_Get_WhenNotLoggedIn_RedirectsToLogin()
    {
        // Arrange
        // no "Username" => not logged in

        // Act
        var result = await _controller.Edit(1) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Login", result.ActionName);
        Assert.Equal("Account", result.ControllerName);
    }
    
    

    [Fact]
    public async Task Edit_Get_WhenValidOwner_ReturnsViewWithAd()
    {
        // Arrange
        _httpContext.Session.SetString("Username", "AdOwner");
        var ad = new LostPetAd
        {
            Id = 10,
            User = new User { Username = "AdOwner" },
            LastSeenCity = "Manisa"
        };
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10)).ReturnsAsync(ad);

        // Act
        var result = await _controller.Edit(10) as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ad, result.Model);
        Assert.True(result.ViewData.ContainsKey("Cities"));
        Assert.True(result.ViewData.ContainsKey("Districts"));
    }
    #endregion
    
    

    [Fact]
    public async Task Delete_Get_WhenValidOwner_ReturnsView()
    {
        // Arrange
        _httpContext.Session.SetString("Username", "Owner");
        var ad = new LostPetAd { Id = 10, User = new User { Username = "Owner" } };
        _lostPetAdServiceMock.Setup(s => s.GetLostPetAdByIdAsync(10))
            .ReturnsAsync(ad);

        // Act
        var result = await _controller.Delete(10) as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ad, result.Model);
    }
}

#region TestSession

/// <summary>
/// A simple in-memory ISession for test usage.
/// </summary>
public class LostPetTestSession : ISession
{
    private readonly Dictionary<string, byte[]> _storage = new Dictionary<string, byte[]>();

    public bool IsAvailable => true;
    public string Id => Guid.NewGuid().ToString();
    public IEnumerable<string> Keys => _storage.Keys;

    public void Clear() => _storage.Clear();
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Remove(string key)
    {
        if (_storage.ContainsKey(key))
            _storage.Remove(key);
    }

    public void Set(string key, byte[] value) => _storage[key] = value;

    public bool TryGetValue(string key, out byte[] value) => _storage.TryGetValue(key, out value);
}
#endregion