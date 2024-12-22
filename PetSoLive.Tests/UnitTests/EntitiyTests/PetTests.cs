using PetSoLive.Core.Entities;
using Xunit;

public class PetTests
{
    [Fact]
    public void Pet_ShouldHaveDefaultValues_WhenInitialized()
    {
        // Arrange & Act
        var pet = new Pet();

        // Assert
        Assert.Equal(0, pet.Id);
        Assert.Null(pet.Name);
        Assert.Null(pet.Species);
        Assert.Null(pet.Breed);
        Assert.Equal(0, pet.Age);
        Assert.Null(pet.Gender);
        Assert.Equal(0.0, pet.Weight);
        Assert.Null(pet.Color);
        Assert.Equal(DateTime.MinValue, pet.DateOfBirth);
        Assert.Null(pet.Description);
        Assert.Null(pet.VaccinationStatus);
        Assert.Null(pet.MicrochipId);
        Assert.Null(pet.IsNeutered);
        Assert.Null(pet.ImageUrl);
        Assert.Empty(pet.AdoptionRequests);
        Assert.Empty(pet.PetOwners);
    }
}