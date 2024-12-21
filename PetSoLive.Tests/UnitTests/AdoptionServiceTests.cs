using System;
using System.Threading.Tasks;
using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Business.Services;
using Xunit;

namespace PetSoLive.Tests.UnitTests
{
    public class AdoptionServiceTests
    {
        private readonly Mock<IAdoptionRepository> _mockAdoptionRepository; // IAdoptionRepository kullanılıyor
        private readonly AdoptionService _adoptionService;

        public AdoptionServiceTests()
        {
            _mockAdoptionRepository = new Mock<IAdoptionRepository>(); // Mock IAdoptionRepository
            _adoptionService = new AdoptionService(_mockAdoptionRepository.Object);
        }

        [Fact]
        public async Task CreateAdoption_ShouldThrowException_IfPetAlreadyAdopted()
        {
            // Arrange
            var petId = 1;
            var adoption = new Adoption
            {
                PetId = petId,
                UserId = 2,
                AdoptionDate = DateTime.Now,
                Status = Core.Enums.AdoptionStatus.Pending
            };

            _mockAdoptionRepository
                .Setup(repo => repo.IsPetAlreadyAdoptedAsync(petId))
                .ReturnsAsync(true); // Pet zaten evlat edinilmiş

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _adoptionService.CreateAdoptionAsync(adoption)
            );

            Assert.Equal("This pet has already been adopted.", exception.Message);
        }

        [Fact]
        public async Task CreateAdoption_ShouldAddAdoption_IfPetNotAdopted()
        {
            // Arrange
            var petId = 1;
            var adoption = new Adoption
            {
                PetId = petId,
                UserId = 2,
                AdoptionDate = DateTime.Now,
                Status = Core.Enums.AdoptionStatus.Pending
            };

            _mockAdoptionRepository
                .Setup(repo => repo.IsPetAlreadyAdoptedAsync(petId))
                .ReturnsAsync(false); // Pet evlat edinilmemiş

            _mockAdoptionRepository
                .Setup(repo => repo.AddAsync(adoption))
                .Returns(Task.CompletedTask); // Başarılı ekleme simülasyonu

            // Act
            await _adoptionService.CreateAdoptionAsync(adoption);

            // Assert
            _mockAdoptionRepository.Verify(repo => repo.AddAsync(adoption), Times.Once);
        }
    }
}
