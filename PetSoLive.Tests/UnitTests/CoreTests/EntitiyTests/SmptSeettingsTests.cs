using Xunit;
using PetSoLive.Core.Entities;
namespace PetSoLive.Tests.UnitTests.EntitiyTests;

public class SmptSeettingsTests
{
    [Fact]
    public void SmtpSettings_ShouldHaveValidProperties_WhenInitialized()
    {
        // Arrange
        var smtpSettings = new SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 587,
            Username = "testuser",
            Password = "testpassword",
            FromEmail = "noreply@example.com",
            EnableSsl = true
        };

        // Act & Assert
        Assert.Equal("smtp.example.com", smtpSettings.Host);
        Assert.Equal(587, smtpSettings.Port);
        Assert.Equal("testuser", smtpSettings.Username);
        Assert.Equal("testpassword", smtpSettings.Password);
        Assert.Equal("noreply@example.com", smtpSettings.FromEmail);
        Assert.True(smtpSettings.EnableSsl);
    }

    [Fact]
    public void SmtpSettings_CanDisableSsl()
    {
        // Arrange
        var smtpSettings = new SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 25,
            EnableSsl = false
        };

        // Act & Assert
        Assert.False(smtpSettings.EnableSsl);
    }
}