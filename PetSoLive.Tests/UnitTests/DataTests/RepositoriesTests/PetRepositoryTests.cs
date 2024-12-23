using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Data;
using Xunit;

public class PetRepositoryTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddAsync_Should_Add_Pet_To_Database()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var repository = new PetRepository(context);
        var pet = new Pet
        {
            Id = 1,
            Name = "Fluffy",
            Age = 2,
            Breed = "Golden Retriever",
            Color = "Golden",
            Description = "Friendly dog",
            Gender = "Male",
            ImageUrl = "https://example.com/fluffy.jpg",
            MicrochipId = "123456789",
            Species = "Dog",
            VaccinationStatus = "Up-to-date",
            DateOfBirth = DateTime.UtcNow.AddYears(-2),
            IsNeutered = true
        };

        // Act
        await repository.AddAsync(pet);
        var petsInDb = await context.Pets.ToListAsync();

        // Assert
        Assert.Single(petsInDb);
        Assert.Equal("Fluffy", petsInDb[0].Name);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Pets()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var repository = new PetRepository(context);
        context.Pets.AddRange(
            new Pet
            {
                Id = 1,
                Name = "Fluffy",
                Age = 2,
                Breed = "Golden Retriever",
                Color = "Golden",
                Description = "Friendly dog",
                Gender = "Male",
                ImageUrl = "https://example.com/fluffy.jpg",
                MicrochipId = "123456789",
                Species = "Dog",
                VaccinationStatus = "Up-to-date",
                DateOfBirth = DateTime.UtcNow.AddYears(-2),
                IsNeutered = true
            },
            new Pet
            {
                Id = 2,
                Name = "Buddy",
                Age = 3,
                Breed = "Labrador",
                Color = "Black",
                Description = "Energetic dog",
                Gender = "Male",
                ImageUrl = "https://example.com/buddy.jpg",
                MicrochipId = "987654321",
                Species = "Dog",
                VaccinationStatus = "Up-to-date",
                DateOfBirth = DateTime.UtcNow.AddYears(-3),
                IsNeutered = false
            }
        );
        await context.SaveChangesAsync();

        // Act
        var pets = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, pets.Count);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Correct_Pet()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var repository = new PetRepository(context);
        var pet = new Pet
        {
            Id = 1,
            Name = "Fluffy",
            Age = 2,
            Breed = "Golden Retriever",
            Color = "Golden",
            Description = "Friendly dog",
            Gender = "Male",
            ImageUrl = "https://example.com/fluffy.jpg",
            MicrochipId = "123456789",
            Species = "Dog",
            VaccinationStatus = "Up-to-date",
            DateOfBirth = DateTime.UtcNow.AddYears(-2),
            IsNeutered = true
        };
        context.Pets.Add(pet);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fluffy", result.Name);
    }

    [Fact]
    public async Task GetPetOwnersAsync_Should_Return_PetOwners_For_Given_PetId()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var repository = new PetRepository(context);
        context.PetOwners.AddRange(
            new PetOwner { PetId = 1, UserId = 1, OwnershipDate = DateTime.UtcNow },
            new PetOwner { PetId = 2, UserId = 2, OwnershipDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act
        var petOwners = await repository.GetPetOwnersAsync(1);

        // Assert
        Assert.Single(petOwners);
        Assert.Equal(1, petOwners[0].UserId);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Existing_Pet()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var repository = new PetRepository(context);
        var pet = new Pet
        {
            Id = 1,
            Name = "Fluffy",
            Age = 2,
            Breed = "Golden Retriever",
            Color = "Golden",
            Description = "Friendly dog",
            Gender = "Male",
            ImageUrl = "https://example.com/fluffy.jpg",
            MicrochipId = "123456789",
            Species = "Dog",
            VaccinationStatus = "Up-to-date",
            DateOfBirth = DateTime.UtcNow.AddYears(-2),
            IsNeutered = true
        };
        context.Pets.Add(pet);
        await context.SaveChangesAsync();

        // Act
        pet.Name = "FluffyUpdated";
        await repository.UpdateAsync(pet);
        var updatedPet = await context.Pets.FindAsync(1);

        // Assert
        Assert.Equal("FluffyUpdated", updatedPet.Name);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Pet_From_Database()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var repository = new PetRepository(context);
        var pet = new Pet
        {
            Id = 1,
            Name = "Fluffy",
            Age = 2,
            Breed = "Golden Retriever",
            Color = "Golden",
            Description = "Friendly dog",
            Gender = "Male",
            ImageUrl = "https://example.com/fluffy.jpg",
            MicrochipId = "123456789",
            Species = "Dog",
            VaccinationStatus = "Up-to-date",
            DateOfBirth = DateTime.UtcNow.AddYears(-2),
            IsNeutered = true
        };
        context.Pets.Add(pet);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(pet);
        var petsInDb = await context.Pets.ToListAsync();

        // Assert
        Assert.Empty(petsInDb);
    }
}
