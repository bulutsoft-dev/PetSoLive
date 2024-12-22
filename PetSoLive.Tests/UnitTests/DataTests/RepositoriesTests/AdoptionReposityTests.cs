using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class AdoptionRepositoryTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly AdoptionRepository _adoptionRepository;

    public AdoptionRepositoryTests()
    {
        _dbContextMock = new Mock<ApplicationDbContext>();
        _adoptionRepository = new AdoptionRepository(_dbContextMock.Object);
    }

    [Fact]
    public async Task AddAsync_ShouldAddAdoption_WhenValidAdoptionPassed()
    {
        // Arrange
        var adoption = new Adoption
        {
            Id = 1,
            PetId = 100,
            UserId = 200,
            AdoptionDate = DateTime.UtcNow,
            Status = AdoptionStatus.Approved
        };

        // Mock DbSet
        var mockSet = new Mock<DbSet<Adoption>>();
        _dbContextMock.Setup(m => m.Adoptions).Returns(mockSet.Object);

        // Act
        await _adoptionRepository.AddAsync(adoption);

        // Assert
        mockSet.Verify(m => m.AddAsync(It.IsAny<Adoption>(), default), Times.Once);
        _dbContextMock.Verify(m => m.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetAdoptionByPetIdAsync_ShouldReturnAdoption_WhenPetIdIsValid()
    {
        // Arrange
        int petId = 100;
        var adoption = new Adoption
        {
            Id = 1,
            PetId = petId,
            UserId = 200,
            AdoptionDate = DateTime.UtcNow,
            Status = AdoptionStatus.Approved
        };

        var mockSet = new Mock<DbSet<Adoption>>();
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Adoption, bool>>>(), default))
               .ReturnsAsync(adoption);

        _dbContextMock.Setup(m => m.Adoptions).Returns(mockSet.Object);

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
        var mockSet = new Mock<DbSet<Adoption>>();
        mockSet.Setup(m => m.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Adoption, bool>>>(), default))
               .ReturnsAsync(true);

        _dbContextMock.Setup(m => m.Adoptions).Returns(mockSet.Object);

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
            Id = 1,
            PetId = petId,
            UserId = userId,
            Message = "I want to adopt this pet.",
            Status = AdoptionStatus.Pending,
            RequestDate = DateTime.UtcNow
        };

        var mockSet = new Mock<DbSet<AdoptionRequest>>();
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AdoptionRequest, bool>>>(), default))
               .ReturnsAsync(adoptionRequest);

        _dbContextMock.Setup(m => m.AdoptionRequests).Returns(mockSet.Object);

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
        var mockSet = new Mock<DbSet<AdoptionRequest>>();
        mockSet.Setup(m => m.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AdoptionRequest, bool>>>(), default))
               .ReturnsAsync(true);

        _dbContextMock.Setup(m => m.AdoptionRequests).Returns(mockSet.Object);

        // Act
        var result = await _adoptionRepository.HasUserAlreadyRequestedAdoptionAsync(userId, petId);

        // Assert
        Assert.True(result);
    }
}
