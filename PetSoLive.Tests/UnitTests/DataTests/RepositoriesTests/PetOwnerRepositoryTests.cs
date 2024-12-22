using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;
using System.Linq.Expressions;

namespace PetSoLive.Infrastructure.Repositories.Tests
{
    public class PetOwnerRepositoryTests
    {
        private readonly Mock<ApplicationDbContext> _dbContextMock;
        private readonly PetOwnerRepository _petOwnerRepository;

        public PetOwnerRepositoryTests()
        {
            _dbContextMock = new Mock<ApplicationDbContext>();
            _petOwnerRepository = new PetOwnerRepository(_dbContextMock.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldAddPetOwner_WhenPetOwnerIsValid()
        {
            // Arrange
            var petOwner = new PetOwner
            {
                PetId = 1,
                UserId = 2,
                OwnershipDate = DateTime.Now
            };

            var mockSet = new Mock<DbSet<PetOwner>>();
            _dbContextMock.Setup(m => m.PetOwners).Returns(mockSet.Object);

            // Act
            await _petOwnerRepository.AddAsync(petOwner);

            // Assert
            mockSet.Verify(m => m.AddAsync(It.IsAny<PetOwner>(), default), Times.Once);
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldSaveChanges_WhenCalled()
        {
            // Arrange
            var petOwner = new PetOwner
            {
                PetId = 1,
                UserId = 2,
                OwnershipDate = DateTime.Now
            };

            var mockSet = new Mock<DbSet<PetOwner>>();
            _dbContextMock.Setup(m => m.PetOwners).Returns(mockSet.Object);
            _dbContextMock.Setup(m => m.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            await _petOwnerRepository.SaveChangesAsync();

            // Assert
            _dbContextMock.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task GetPetOwnerByPetIdAsync_ShouldReturnPetOwner_WhenPetIdIsValid()
        {
            // Arrange
            int petId = 1;
            var petOwner = new PetOwner
            {
                PetId = petId,
                UserId = 2,
                OwnershipDate = DateTime.Now,
                Pet = new Pet { Id = petId, Name = "Buddy" },
                User = new User { Id = 2, Username = "john_doe" }
            };

            var petOwners = new List<PetOwner> { petOwner }.AsQueryable();
            var mockSet = new Mock<DbSet<PetOwner>>();

            // Mock the FirstOrDefaultAsync to return the petOwner
            mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Expression<Func<PetOwner, bool>>>(), default))
                   .ReturnsAsync(petOwner);

            // Ensure the PetOwners DbSet is set up correctly
            _dbContextMock.Setup(m => m.PetOwners).Returns(mockSet.Object);

            // Act
            var result = await _petOwnerRepository.GetPetOwnerByPetIdAsync(petId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(petId, result.PetId);
        }

        [Fact]
        public async Task GetPetOwnerByPetIdAsync_ShouldReturnNull_WhenPetIdIsInvalid()
        {
            // Arrange
            int petId = 999;
            var mockSet = new Mock<DbSet<PetOwner>>();

            // Mock FirstOrDefaultAsync to return null when the petId is invalid
            mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Expression<Func<PetOwner, bool>>>(), default))
                   .ReturnsAsync((PetOwner)null);

            _dbContextMock.Setup(m => m.PetOwners).Returns(mockSet.Object);

            // Act
            var result = await _petOwnerRepository.GetPetOwnerByPetIdAsync(petId);

            // Assert
            Assert.Null(result);
        }
    }
}
