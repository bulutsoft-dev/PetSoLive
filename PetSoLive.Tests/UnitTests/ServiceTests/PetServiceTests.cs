using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PetSoLive.Tests.UnitTests
{
    public class PetServiceTests
    {
        private readonly Mock<IPetRepository> _mockPetRepository;
        private readonly Mock<IPetOwnerRepository> _mockPetOwnerRepository;
        private readonly PetService _petService;

        public PetServiceTests()
        {
            _mockPetRepository = new Mock<IPetRepository>();
            _mockPetOwnerRepository = new Mock<IPetOwnerRepository>();
            _petService = new PetService(_mockPetRepository.Object, _mockPetOwnerRepository.Object);
        }

        [Fact]
        public async Task CreatePetAsync_ShouldThrowArgumentNullException_WhenPetIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _petService.CreatePetAsync(null));
        }

        [Fact]
        public async Task CreatePetAsync_ShouldCallAddAsync_WhenPetIsValid()
        {
            // Arrange
            var pet = new Pet { Id = 1, Name = "Fluffy" };

            // Act
            await _petService.CreatePetAsync(pet);

            // Assert
            _mockPetRepository.Verify(r => r.AddAsync(It.Is<Pet>(p => p.Name == "Fluffy")), Times.Once);
        }

        [Fact]
        public async Task GetPetByIdAsync_ShouldThrowKeyNotFoundException_WhenPetNotFound()
        {
            // Arrange
            var petId = 1;
            _mockPetRepository.Setup(r => r.GetByIdAsync(petId)).ReturnsAsync((Pet)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _petService.GetPetByIdAsync(petId));
        }

        [Fact]
        public async Task GetPetByIdAsync_ShouldReturnPet_WhenPetIsFound()
        {
            // Arrange
            var petId = 1;
            var pet = new Pet { Id = petId, Name = "Fluffy" };
            _mockPetRepository.Setup(r => r.GetByIdAsync(petId)).ReturnsAsync(pet);

            // Act
            var result = await _petService.GetPetByIdAsync(petId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Fluffy", result.Name);
        }

        [Fact]
        public async Task IsUserOwnerOfPetAsync_ShouldReturnTrue_WhenUserIsOwner()
        {
            // Arrange
            var petId = 1;
            var userId = 1;
            var petOwners = new List<PetOwner>
            {
                new PetOwner { PetId = petId, UserId = userId }
            };

            _mockPetRepository.Setup(r => r.GetPetOwnersAsync(petId)).ReturnsAsync(petOwners);

            // Act
            var result = await _petService.IsUserOwnerOfPetAsync(petId, userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdatePetAsync_ShouldThrowKeyNotFoundException_WhenPetNotFound()
        {
            // Arrange
            var petId = 1;
            var updatedPet = new Pet { Id = petId, Name = "Fluffy" };
            _mockPetRepository.Setup(r => r.GetByIdAsync(petId)).ReturnsAsync((Pet)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _petService.UpdatePetAsync(petId, updatedPet, 1));
        }

        [Fact]
        public async Task UpdatePetAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotOwner()
        {
            // Arrange
            var petId = 1;
            var updatedPet = new Pet { Id = petId, Name = "Fluffy" };
            var existingPet = new Pet { Id = petId, Name = "Fluffy" };

            _mockPetRepository.Setup(r => r.GetByIdAsync(petId)).ReturnsAsync(existingPet);
            _mockPetRepository.Setup(r => r.GetPetOwnersAsync(petId)).ReturnsAsync(new List<PetOwner>());

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _petService.UpdatePetAsync(petId, updatedPet, 2));
        }

        [Fact]
        public async Task AssignPetOwnerAsync_ShouldThrowArgumentNullException_WhenPetOwnerIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _petService.AssignPetOwnerAsync(null));
        }

        [Fact]
        public async Task AssignPetOwnerAsync_ShouldCallAddAsync_WhenPetOwnerIsValid()
        {
            // Arrange
            var petOwner = new PetOwner { PetId = 1, UserId = 1 };

            // Act
            await _petService.AssignPetOwnerAsync(petOwner);

            // Assert
            _mockPetOwnerRepository.Verify(r => r.AddAsync(It.Is<PetOwner>(po => po.PetId == 1 && po.UserId == 1)), Times.Once);
            _mockPetOwnerRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeletePetAsync_ShouldThrowKeyNotFoundException_WhenPetNotFound()
        {
            // Arrange
            var petId = 1;
            _mockPetRepository.Setup(r => r.GetByIdAsync(petId)).ReturnsAsync((Pet)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _petService.DeletePetAsync(petId, 1));
        }

        [Fact]
        public async Task DeletePetAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotOwner()
        {
            // Arrange
            var petId = 1;
            var pet = new Pet { Id = petId, Name = "Fluffy" };
            _mockPetRepository.Setup(r => r.GetByIdAsync(petId)).ReturnsAsync(pet);
            _mockPetRepository.Setup(r => r.GetPetOwnersAsync(petId)).ReturnsAsync(new List<PetOwner>());

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _petService.DeletePetAsync(petId, 2));
        }

        [Fact]
        public async Task DeletePetAsync_ShouldDeletePet_WhenUserIsOwner()
        {
            // Arrange
            var petId = 1;
            var pet = new Pet { Id = petId, Name = "Fluffy" };
            _mockPetRepository.Setup(r => r.GetByIdAsync(petId)).ReturnsAsync(pet);
            _mockPetRepository.Setup(r => r.GetPetOwnersAsync(petId)).ReturnsAsync(new List<PetOwner>
            {
                new PetOwner { PetId = petId, UserId = 1 }
            });

            // Act
            await _petService.DeletePetAsync(petId, 1);

            // Assert
            _mockPetRepository.Verify(r => r.DeleteAsync(It.Is<Pet>(p => p.Id == petId)), Times.Once);
        }
    }
}
