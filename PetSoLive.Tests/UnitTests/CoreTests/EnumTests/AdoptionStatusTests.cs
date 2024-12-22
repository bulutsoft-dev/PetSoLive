using PetSoLive.Core.Enums;
using Xunit;

namespace PetSoLive.Tests.UnitTests.EnumTests;
public class AdoptionStatusTests
{
    [Fact]
    public void AdoptionStatus_ShouldHaveCorrectValues()
    {
        // Act & Assert
        Assert.Equal(0, (int)AdoptionStatus.Pending);   // Pending = 0
        Assert.Equal(1, (int)AdoptionStatus.Approved);  // Approved = 1
        Assert.Equal(2, (int)AdoptionStatus.Rejected);  // Rejected = 2
    }

    [Fact]
    public void AdoptionStatus_ShouldContainAllValues()
    {
        // Act & Assert
        var statusValues = Enum.GetValues(typeof(AdoptionStatus)).Cast<AdoptionStatus>().ToArray();

        Assert.Contains(AdoptionStatus.Pending, statusValues);
        Assert.Contains(AdoptionStatus.Approved, statusValues);
        Assert.Contains(AdoptionStatus.Rejected, statusValues);
    }

    [Fact]
    public void AdoptionStatus_ShouldReturnCorrectStringRepresentation()
    {
        // Act & Assert
        Assert.Equal("Pending", AdoptionStatus.Pending.ToString());
        Assert.Equal("Approved", AdoptionStatus.Approved.ToString());
        Assert.Equal("Rejected", AdoptionStatus.Rejected.ToString());
    }
}
