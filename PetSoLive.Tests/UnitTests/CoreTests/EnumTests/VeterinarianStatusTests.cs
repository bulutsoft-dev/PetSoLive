using System;
using Xunit;

namespace PetSoLive.Tests.UnitTests.EnumTests;

public class VeterinarianStatusTests
{
    [Fact]
    public void VeterinarianStatus_Values_ShouldMatchExpectedIntegers()
    {
        // By default:
        // Pending = 0, Approved = 1, Rejected = 2
        Assert.Equal(0, (int)VeterinarianStatus.Pending);
        Assert.Equal(1, (int)VeterinarianStatus.Approved);
        Assert.Equal(2, (int)VeterinarianStatus.Rejected);
    }

    [Theory]
    [InlineData("Pending", VeterinarianStatus.Pending)]
    [InlineData("Approved", VeterinarianStatus.Approved)]
    [InlineData("Rejected", VeterinarianStatus.Rejected)]
    public void VeterinarianStatus_ParseFromString_ReturnsCorrectEnum(string input, VeterinarianStatus expected)
    {
        // Act
        var parseSuccess = Enum.TryParse<VeterinarianStatus>(input, out var result);

        // Assert
        Assert.True(parseSuccess, $"Failed to parse '{input}'");
        Assert.Equal(expected, result);
    }
}

// Your enum might actually be in another file/namespace, included here for completeness:
public enum VeterinarianStatus
{
    Pending,  // Beklemede
    Approved, // Onaylandı
    Rejected  // Reddedildi

}