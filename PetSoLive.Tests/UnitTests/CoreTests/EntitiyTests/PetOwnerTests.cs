using PetSoLive.Core.Entities;
using Xunit;

namespace PetSoLive.Tests.UnitTests.EntitiyTests;

public class PetOwnerTests
{
    [Fact]
    public void PetOwner_ShouldHaveValidRelationships_WhenInitialized()
    {
        // Arrange
        var petOwner = new PetOwner
        {
            PetId = 1,
            UserId = 1,
            OwnershipDate = DateTime.UtcNow,
            Pet = new Pet { Id = 1, Name = "Fluffy" },
            User = new User { Id = 1, Username = "JohnDoe" }
        };

        // Act & Assert
        Assert.Equal(1, petOwner.PetId);
        Assert.Equal(1, petOwner.UserId);
        Assert.True(petOwner.OwnershipDate <= DateTime.UtcNow);
        Assert.NotNull(petOwner.Pet);
        Assert.Equal("Fluffy", petOwner.Pet.Name);
        Assert.NotNull(petOwner.User);
        Assert.Equal("JohnDoe", petOwner.User.Username);
    }
}
