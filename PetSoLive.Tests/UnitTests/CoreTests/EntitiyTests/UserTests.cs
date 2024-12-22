using PetSoLive.Core.Entities;
using Xunit;

namespace PetSoLive.Tests.UnitTests.EntitiyTests;

public class UserTests
{
    [Fact]
    public void User_ShouldHaveValidProperties_WhenInitialized()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "JohnDoe",
            Email = "johndoe@example.com",
            PasswordHash = "hashedPassword",
            PhoneNumber = "123-456-7890",
            Address = "123 Main St",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            LastLoginDate = DateTime.UtcNow,
            ProfileImageUrl = "profile.jpg"
        };

        // Act & Assert
        Assert.Equal(1, user.Id);
        Assert.Equal("JohnDoe", user.Username);
        Assert.Equal("johndoe@example.com", user.Email);
        Assert.Equal("hashedPassword", user.PasswordHash);
        Assert.Equal("123-456-7890", user.PhoneNumber);
        Assert.Equal("123 Main St", user.Address);
        Assert.Equal(new DateTime(1990, 1, 1), user.DateOfBirth);
        Assert.True(user.IsActive);
        Assert.True(user.CreatedDate <= DateTime.UtcNow);
        Assert.NotNull(user.LastLoginDate);
        Assert.Equal("profile.jpg", user.ProfileImageUrl);
    }
}
