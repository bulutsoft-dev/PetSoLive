using Moq;
using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PetSoLive.Infrastructure.Repositories.Tests
{
    public class PetRepositoryTests
    {
        private readonly Mock<ApplicationDbContext> _dbContextMock;
        private readonly Mock<DbSet<Pet>> _mockPetSet;
        private readonly Mock<DbSet<PetOwner>> _mockPetOwnerSet;
        private readonly PetRepository _petRepository;

        public PetRepositoryTests()
        {
            _dbContextMock = new Mock<ApplicationDbContext>();
            _mockPetSet = new Mock<DbSet<Pet>>();
            _mockPetOwnerSet = new Mock<DbSet<PetOwner>>();
            _petRepository = new PetRepository(_dbContextMock.Object);

            // Setup DbSets in DbContext mock
            _dbContextMock.Setup(db => db.Pets).Returns(_mockPetSet.Object);
            _dbContextMock.Setup(db => db.PetOwners).Returns(_mockPetOwnerSet.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldAddPet_WhenPetIsNotNull()
        {
            // Arrange
            var pet = new Pet { Id = 1, Name = "Buddy" };

            // Mock the AddAsync method
            var mockSet = new Mock<DbSet<Pet>>();
            _dbContextMock.Setup(m => m.Pets).Returns(mockSet.Object);

            // Mock AddAsync to ensure it behaves like the real method
            mockSet.Setup(m => m.AddAsync(It.IsAny<Pet>(), default))
                .ReturnsAsync((Pet pet, CancellationToken token) => null); // Returning null to match the signature

            // Act
            await _petRepository.AddAsync(pet);

            // Assert
            mockSet.Verify(m => m.AddAsync(It.IsAny<Pet>(), default), Times.Once);
            _dbContextMock.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }


        [Fact]
        public async Task GetAllAsync_ShouldReturnAllPets()
        {
            // Arrange
            var pets = new List<Pet>
            {
                new Pet { Id = 1, Name = "Buddy" },
                new Pet { Id = 2, Name = "Max" }
            }.AsQueryable();

            _mockPetSet.As<IQueryable<Pet>>().Setup(m => m.Provider).Returns(pets.Provider);
            _mockPetSet.As<IQueryable<Pet>>().Setup(m => m.Expression).Returns(pets.Expression);
            _mockPetSet.As<IQueryable<Pet>>().Setup(m => m.ElementType).Returns(pets.ElementType);
            _mockPetSet.As<IQueryable<Pet>>().Setup(m => m.GetEnumerator()).Returns(pets.GetEnumerator());

            // Act
            var result = await _petRepository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnPet_WhenPetExists()
        {
            // Arrange
            int petId = 1;
            var pet = new Pet { Id = petId, Name = "Buddy" };

            _mockPetSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Pet, bool>>>(), default))
                .ReturnsAsync(pet);

            // Act
            var result = await _petRepository.GetByIdAsync(petId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(petId, result.Id);
        }

        [Fact]
        public async Task GetPetOwnersAsync_ShouldReturnPetOwners_WhenPetHasOwners()
        {
            // Arrange
            int petId = 1;
            var petOwners = new List<PetOwner>
            {
                new PetOwner { PetId = petId, UserId = 2 },
                new PetOwner { PetId = petId, UserId = 3 }
            };

            _mockPetOwnerSet.Setup(m => m.Where(It.IsAny<System.Linq.Expressions.Expression<System.Func<PetOwner, bool>>>()))
                .Returns(petOwners.AsQueryable());

            // Act
            var result = await _petRepository.GetPetOwnersAsync(petId);

            // Assert
            Assert.Equal(2, result.Count);
        }
        [Fact]
        public async Task UpdateAsync_ShouldUpdatePet_WhenPetExists()
        {
            // Arrange
            var pet = new Pet { Id = 1, Name = "Buddy" };

            // Mock DbSet Update
            _mockPetSet.Setup(m => m.Update(It.IsAny<Pet>()));

            // Act
            await _petRepository.UpdateAsync(pet);

            // Assert
            _mockPetSet.Verify(m => m.Update(It.IsAny<Pet>()), Times.Once);
            _dbContextMock.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemovePet_WhenPetExists()
        {
            // Arrange
            var pet = new Pet { Id = 1, Name = "Buddy" };

            // Mock DbSet Remove
            _mockPetSet.Setup(m => m.Remove(It.IsAny<Pet>()));

            // Act
            await _petRepository.DeleteAsync(pet);

            // Assert
            _mockPetSet.Verify(m => m.Remove(It.IsAny<Pet>()), Times.Once);
            _dbContextMock.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

    }
}
