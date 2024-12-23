using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Data;
using System;
using System.Threading.Tasks;
using Xunit;

public class AdoptionRepositoryTests
{
    private readonly AdoptionRepository _adoptionRepository;
    private readonly ApplicationDbContext _dbContext;

    public AdoptionRepositoryTests()
    {
        // Using in-memory database for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _adoptionRepository = new AdoptionRepository(_dbContext);

        // Ensure the database is clean before each test
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();
    }

    [Fact]
    public async Task AddAsync_ShouldAddAdoption_WhenValidAdoptionPassed()
    {
        // Arrange
        var adoption = new Adoption
        {
            Id = 1,  // Ensure unique ID for each test
            PetId = 100,
            UserId = 200,
            AdoptionDate = DateTime.UtcNow,
            Status = AdoptionStatus.Approved
        };

        // Act
        await _adoptionRepository.AddAsync(adoption);

        // Assert
        var addedAdoption = await _dbContext.Adoptions.FindAsync(1);
        Assert.NotNull(addedAdoption);
        Assert.Equal(adoption.PetId, addedAdoption.PetId);
    }

    [Fact]
    public async Task GetAdoptionByPetIdAsync_ShouldReturnAdoption_WhenPetIdIsValid()
    {
        // Arrange
        int petId = 100;
        var adoption = new Adoption
        {
            Id = 1,  // Ensure unique ID for each test
            PetId = petId,
            UserId = 200,
            AdoptionDate = DateTime.UtcNow,
            Status = AdoptionStatus.Approved
        };

        await _dbContext.Adoptions.AddAsync(adoption);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _adoptionRepository.GetAdoptionByPetIdAsync(petId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(petId, result.PetId);
    }

    [Fact]
    public async Task IsPetAlreadyAdoptedAsync_ShouldReturnTrue_WhenPetIsAdopted()
    {
        // Arrange
        int petId = 100;
        var adoption = new Adoption
        {
            Id = 1,  // Ensure unique ID for each test
            PetId = petId,
            UserId = 200,
            AdoptionDate = DateTime.UtcNow,
            Status = AdoptionStatus.Approved
        };

        await _dbContext.Adoptions.AddAsync(adoption);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _adoptionRepository.IsPetAlreadyAdoptedAsync(petId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetAdoptionRequestByUserAndPetAsync_ShouldReturnAdoptionRequest_WhenValidUserAndPetId()
    {
        // Arrange
        int userId = 200;
        int petId = 100;
        var adoptionRequest = new AdoptionRequest
        {
            Id = 1,  // Ensure unique ID for each test
            PetId = petId,
            UserId = userId,
            Message = "I want to adopt this pet.",
            Status = AdoptionStatus.Pending,
            RequestDate = DateTime.UtcNow
        };

        await _dbContext.AdoptionRequests.AddAsync(adoptionRequest);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _adoptionRepository.GetAdoptionRequestByUserAndPetAsync(userId, petId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(petId, result.PetId);
    }

    [Fact]
    public async Task HasUserAlreadyRequestedAdoptionAsync_ShouldReturnTrue_WhenUserHasRequestedAdoption()
    {
        // Arrange
        int userId = 200;
        int petId = 100;
        var adoptionRequest = new AdoptionRequest
        {
            Id = 1,  // Ensure unique ID for each test
            PetId = petId,
            UserId = userId,
            Message = "I want to adopt this pet.",
            Status = AdoptionStatus.Pending,
            RequestDate = DateTime.UtcNow
        };

        await _dbContext.AdoptionRequests.AddAsync(adoptionRequest);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _adoptionRepository.HasUserAlreadyRequestedAdoptionAsync(userId, petId);

        // Assert
        Assert.True(result);
    }
}
