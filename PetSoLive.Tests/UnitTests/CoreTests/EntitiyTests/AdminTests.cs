using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using PetSoLive.Core.Entities; // Adjust to match your namespace

namespace PetSoLive.Tests.UnitTests.EntitiyTests;

public class AdminTests
{
    [Fact]
    public void Admin_ShouldHaveValidProperties_WhenInitialized()
    {
        // Arrange
        var user = new User
        {
            Id = 10,
            Username = "AdminUser",
            Email = "adminuser@example.com",
            CreatedDate = DateTime.UtcNow
        };

        var admin = new Admin
        {
            Id = 1,
            UserId = user.Id,
            CreatedDate = new DateTime(2022, 1, 1),
            User = user
        };

        // Act & Assert
        Assert.Equal(1, admin.Id);
        Assert.Equal(10, admin.UserId);
        Assert.Equal(new DateTime(2022, 1, 1), admin.CreatedDate);
            
        // Check the navigation property
        Assert.NotNull(admin.User);
        Assert.Equal(10, admin.User.Id);
        Assert.Equal("AdminUser", admin.User.Username);
        Assert.Equal("adminuser@example.com", admin.User.Email);
    }
}

