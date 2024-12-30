using System;
using Xunit;
using PetSoLive.Core.Entities;

namespace PetSoLive.Tests.UnitTests.EntitiyTests;

public class AssistanceTest
{
    [Fact]
    public void Assistance_ShouldHaveValidProperties_WhenInitialized()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var assistance = new Assistance
        {
            Id = 1,
            Title = "Need Vet Services",
            Description = "We need help for a dog surgery",
            DateCreated = now,
            ContactInfo = "vet@example.com"
        };

        // Act & Assert
        Assert.Equal(1, assistance.Id);
        Assert.Equal("Need Vet Services", assistance.Title);
        Assert.Equal("We need help for a dog surgery", assistance.Description);
        Assert.Equal(now, assistance.DateCreated);
        Assert.Equal("vet@example.com", assistance.ContactInfo);
    }
}