using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PetSoLive.Tests.UnitTests
{
    public class PetOwnerServiceTests
    {
        private readonly Mock<IPetOwnerRepository> _mockPetOwnerRepository;
        private readonly PetOwnerService _petOwnerService;

        public PetOwnerServiceTests()
        {
            _mockPetOwnerRepository = new Mock<IPetOwnerRepository>();
            _petOwnerService = new PetOwnerService(_mockPetOwnerRepository.Object);
        }

        [Fact]
        public async Task GetPetOwnerAsync_ShouldReturnPetOwner_WhenPetOwnerExists()
        {
            // Arrange
            var petId = 1;
            var expectedPetOwner = new PetOwner
            {
                PetId = petId,
                UserId = 1,
                OwnershipDate = DateTime.Now,
                Pet = new Pet { Id = petId, Name = "Fluffy", Species = "Cat" },
                User = new User { Id = 1, Username = "john_doe" }
            };

            // Setting up the mock repository to return the expected pet owner
            _mockPetOwnerRepository
                .Setup(r => r.GetPetOwnerByPetIdAsync(petId))
                .ReturnsAsync(expectedPetOwner);

            // Act
            var result = await _petOwnerService.GetPetOwnerAsync(petId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPetOwner.PetId, result.PetId);
            Assert.Equal(expectedPetOwner.UserId, result.UserId);
            Assert.Equal(expectedPetOwner.Pet.Name, result.Pet.Name);
            Assert.Equal(expectedPetOwner.User.Username, result.User.Username);
        }

        [Fact]
        public async Task GetPetOwnerAsync_ShouldThrowException_WhenPetOwnerNotFound()
        {
            // Arrange
            var petId = 1;

            // Setting up the mock repository to return null for the pet owner
            _mockPetOwnerRepository
                .Setup(r => r.GetPetOwnerByPetIdAsync(petId))
                .ReturnsAsync((PetOwner)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _petOwnerService.GetPetOwnerAsync(petId));
        }

        [Fact]
        public async Task GetPetOwnerByPetIdAsync_ShouldReturnPetOwner_WhenPetOwnerExists()
        {
            // Arrange
            var petId = 1;
            var expectedPetOwner = new PetOwner
            {
                PetId = petId,
                UserId = 1,
                OwnershipDate = DateTime.Now,
                Pet = new Pet { Id = petId, Name = "Fluffy", Species = "Cat" },
                User = new User { Id = 1, Username = "jane_doe" }
            };

            // Setting up the mock repository to return the expected pet owner
            _mockPetOwnerRepository
                .Setup(r => r.GetPetOwnerByPetIdAsync(petId))
                .ReturnsAsync(expectedPetOwner);

            // Act
            var result = await _petOwnerService.GetPetOwnerByPetIdAsync(petId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPetOwner.PetId, result.PetId);
            Assert.Equal(expectedPetOwner.UserId, result.UserId);
            Assert.Equal(expectedPetOwner.Pet.Name, result.Pet.Name);
            Assert.Equal(expectedPetOwner.User.Username, result.User.Username);
        }

        [Fact]
        public async Task GetPetOwnerByPetIdAsync_ShouldThrowInvalidOperationException_WhenPetOwnerNotFound()
        {
            // Arrange
            var petId = 1;

            // Setting up the mock repository to return null for the pet owner
            _mockPetOwnerRepository
                .Setup(r => r.GetPetOwnerByPetIdAsync(petId))
                .ReturnsAsync((PetOwner)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _petOwnerService.GetPetOwnerByPetIdAsync(petId));
        }
    }
}
