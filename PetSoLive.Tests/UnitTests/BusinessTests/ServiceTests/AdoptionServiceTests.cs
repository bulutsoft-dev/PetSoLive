using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System;
using System.Threading.Tasks;
using PetSoLive.Core.Enums;
using Xunit;

namespace PetSoLive.Tests.UnitTests
{
    public class AdoptionServiceTests
    {
        private readonly Mock<IAdoptionRepository> _mockRepository;
        private readonly AdoptionService _service;

        public AdoptionServiceTests()
        {
            _mockRepository = new Mock<IAdoptionRepository>();
            _service = new AdoptionService(_mockRepository.Object);
        }

        [Fact]
        public async Task CreateAdoptionAsync_ShouldThrowException_WhenPetIsAlreadyAdopted()
        {
            // Arrange
            var adoption = new Adoption
            {
                PetId = 1,
                UserId = 1,
                AdoptionDate = DateTime.Now,
                Status = AdoptionStatus.Approved
            };

            _mockRepository.Setup(r => r.IsPetAlreadyAdoptedAsync(adoption.PetId)).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAdoptionAsync(adoption));
        }

        [Fact]
        public async Task CreateAdoptionAsync_ShouldCreateAdoption_WhenPetIsNotAdopted()
        {
            // Arrange
            var adoption = new Adoption
            {
                PetId = 1,
                UserId = 1,
                AdoptionDate = DateTime.Now,
                Status = AdoptionStatus.Approved
            };

            _mockRepository.Setup(r => r.IsPetAlreadyAdoptedAsync(adoption.PetId)).ReturnsAsync(false);
            _mockRepository.Setup(r => r.AddAsync(adoption)).Returns(Task.CompletedTask);

            // Act
            await _service.CreateAdoptionAsync(adoption);

            // Assert
            _mockRepository.Verify(r => r.AddAsync(adoption), Times.Once);
        }

        [Fact]
        public async Task GetAdoptionByPetIdAsync_ShouldReturnAdoption_WhenAdoptionExists()
        {
            // Arrange
            var adoption = new Adoption
            {
                PetId = 1,
                UserId = 1,
                AdoptionDate = DateTime.Now,
                Status = AdoptionStatus.Approved
            };

            _mockRepository.Setup(r => r.GetAdoptionByPetIdAsync(adoption.PetId)).ReturnsAsync(adoption);

            // Act
            var result = await _service.GetAdoptionByPetIdAsync(adoption.PetId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(adoption.PetId, result.PetId);
        }

        [Fact]
        public async Task CreateAdoptionRequestAsync_ShouldThrowException_WhenPetIsAlreadyAdopted()
        {
            // Arrange
            var adoptionRequest = new AdoptionRequest
            {
                PetId = 1,
                UserId = 1,
                Message = "I would love to adopt this pet.",
                Status = AdoptionStatus.Pending,
                RequestDate = DateTime.Now
            };

            _mockRepository.Setup(r => r.GetAdoptionByPetIdAsync(adoptionRequest.PetId)).ReturnsAsync(new Adoption());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAdoptionRequestAsync(adoptionRequest));
        }

        [Fact]
        public async Task CreateAdoptionRequestAsync_ShouldCreateRequest_WhenPetIsNotAdopted()
        {
            // Arrange
            var adoptionRequest = new AdoptionRequest
            {
                PetId = 1,
                UserId = 1,
                Message = "I would love to adopt this pet.",
                Status = AdoptionStatus.Pending,
                RequestDate = DateTime.Now
            };

            _mockRepository.Setup(r => r.GetAdoptionByPetIdAsync(adoptionRequest.PetId)).ReturnsAsync((Adoption)null);
            _mockRepository.Setup(r => r.AddAsync(adoptionRequest)).Returns(Task.CompletedTask);

            // Act
            await _service.CreateAdoptionRequestAsync(adoptionRequest);

            // Assert
            _mockRepository.Verify(r => r.AddAsync(adoptionRequest), Times.Once);
        }

        [Fact]
        public async Task GetAdoptionRequestByUserAndPetAsync_ShouldReturnAdoptionRequest_WhenExists()
        {
            // Arrange
            var adoptionRequest = new AdoptionRequest
            {
                PetId = 1,
                UserId = 1,
                Message = "I would love to adopt this pet.",
                Status = AdoptionStatus.Pending,
                RequestDate = DateTime.Now
            };

            _mockRepository.Setup(r => r.GetAdoptionRequestByUserAndPetAsync(adoptionRequest.UserId, adoptionRequest.PetId)).ReturnsAsync(adoptionRequest);

            // Act
            var result = await _service.GetAdoptionRequestByUserAndPetAsync(adoptionRequest.UserId, adoptionRequest.PetId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(adoptionRequest.UserId, result.UserId);
            Assert.Equal(adoptionRequest.PetId, result.PetId);
        }
    }
}
