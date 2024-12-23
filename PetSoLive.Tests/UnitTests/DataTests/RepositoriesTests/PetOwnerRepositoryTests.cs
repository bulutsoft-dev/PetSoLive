using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Data;
using Xunit;

public class PetOwnerRepositoryTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddAsync_Should_Add_PetOwner_To_Database()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var repository = new PetOwnerRepository(context);
        var petOwner = new PetOwner
        {
            PetId = 1,
            UserId = 1,
            OwnershipDate = DateTime.UtcNow
        };

        // Act
        await repository.AddAsync(petOwner);
        await repository.SaveChangesAsync();
        var petOwnersInDb = await context.PetOwners.ToListAsync();

        // Assert
        Assert.Single(petOwnersInDb);
        Assert.Equal(1, petOwnersInDb[0].PetId);
        Assert.Equal(1, petOwnersInDb[0].UserId);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Persist_Changes()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var repository = new PetOwnerRepository(context);
        var petOwner = new PetOwner
        {
            PetId = 2,
            UserId = 3,
            OwnershipDate = DateTime.UtcNow
        };
        await repository.AddAsync(petOwner);

        // Act
        await repository.SaveChangesAsync();
        var petOwnersInDb = await context.PetOwners.ToListAsync();

        // Assert
        Assert.Single(petOwnersInDb);
        Assert.Equal(2, petOwnersInDb[0].PetId);
        Assert.Equal(3, petOwnersInDb[0].UserId);
    }

    [Fact]
public async Task GetPetOwnerByPetIdAsync_Should_Return_Correct_PetOwner()
{
    // Arrange
    var context = CreateInMemoryDbContext();
    var repository = new PetOwnerRepository(context);

    // Create and add a User with required properties
    var user = new User
    {
        Id = 1,
        Username = "TestUser",
        Email = "testuser@example.com",
        PasswordHash = "hashedpassword",  // Add missing required properties
        PhoneNumber = "1234567890",       // Add missing required properties
        Address = "Test Address",         // Add missing required properties
        IsActive = true,
        CreatedDate = DateTime.UtcNow,
        ProfileImageUrl = "http://example.com/profile.jpg" // Add missing required properties
    };
    context.Users.Add(user);
    await context.SaveChangesAsync(); // Save the user to the database

    // Create and add a Pet with required properties
    var pet = new Pet
    {
        Id = 1,
        Name = "Buddy",
        Species = "Dog",
        Breed = "Golden Retriever",
        Age = 3,
        Gender = "Male",
        Weight = 30.5,
        Color = "Golden", // Add missing required property
        Description = "Friendly dog", // Add missing required property
        DateOfBirth = DateTime.UtcNow.AddYears(-3),
        VaccinationStatus = "Up-to-date",
        MicrochipId = "12345",
        IsNeutered = true,
        ImageUrl = "http://example.com/pet.jpg"
    };
    context.Pets.Add(pet);
    await context.SaveChangesAsync(); // Save the pet to the database

    // Create and add a PetOwner
    var petOwner = new PetOwner
    {
        PetId = 1,
        UserId = 1,
        OwnershipDate = DateTime.UtcNow
    };
    context.PetOwners.Add(petOwner);
    await context.SaveChangesAsync(); // Save the petOwner to the database

    // Act
    var result = await repository.GetPetOwnerByPetIdAsync(1);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(1, result.PetId);
    Assert.Equal(1, result.UserId);
}



    [Fact]
    public async Task GetPetOwnerByPetIdAsync_Should_Return_Null_If_Not_Found()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var repository = new PetOwnerRepository(context);

        // Act
        var result = await repository.GetPetOwnerByPetIdAsync(99);

        // Assert
        Assert.Null(result);
    }
}
