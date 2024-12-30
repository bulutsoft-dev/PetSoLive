using System;
using Xunit;
using PetSoLive.Core.Entities;
namespace PetSoLive.Tests.UnitTests.EntitiyTests;

public class AnnouncementTests
{
    [Fact]
    public void Announcement_ShouldHaveValidProperties_WhenInitialized()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var announcement = new Announcement
        {
            Id = 1,
            Title = "New Features Released",
            Description = "We are happy to announce new features...",
            CreatedAt = now
        };

        // Act & Assert
        Assert.Equal(1, announcement.Id);
        Assert.Equal("New Features Released", announcement.Title);
        Assert.Equal("We are happy to announce new features...", announcement.Description);
        Assert.Equal(now, announcement.CreatedAt);
    }
}