using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Services;

namespace PetSoLive.Tests.UnitTests;

public class VeterinarianServiceTests
{
    private readonly Mock<IVeterinarianRepository> _veterinarianRepositoryMock;
        private readonly VeterinarianService _veterinarianService;

        public VeterinarianServiceTests()
        {
            _veterinarianRepositoryMock = new Mock<IVeterinarianRepository>();
            _veterinarianService = new VeterinarianService(_veterinarianRepositoryMock.Object);
        }

        [Fact]
        public async Task RegisterVeterinarianAsync_WhenCalled_CreatesAndReturnsVeterinarian()
        {
            // Arrange
            var userId = 1;
            var qualifications = "DVM";
            var clinicAddress = "123 Test St";
            var clinicPhone = "555-1234";
            var createdVet = new Veterinarian
            {
                Id = 10,
                UserId = userId,
                Qualifications = qualifications,
                ClinicAddress = clinicAddress,
                ClinicPhoneNumber = clinicPhone,
                AppliedDate = DateTime.UtcNow,
                Status = VeterinarianStatus.Pending
            };

            _veterinarianRepositoryMock
                .Setup(repo => repo.CreateAsync(It.IsAny<Veterinarian>()))
                .ReturnsAsync(createdVet);

            // Act
            var result = await _veterinarianService.RegisterVeterinarianAsync(
                userId, 
                qualifications, 
                clinicAddress, 
                clinicPhone
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(qualifications, result.Qualifications);
            Assert.Equal(clinicAddress, result.ClinicAddress);
            Assert.Equal(clinicPhone, result.ClinicPhoneNumber);
            Assert.Equal(VeterinarianStatus.Pending, result.Status);

            _veterinarianRepositoryMock.Verify(
                repo => repo.CreateAsync(It.Is<Veterinarian>(v =>
                    v.UserId == userId &&
                    v.Qualifications == qualifications &&
                    v.ClinicAddress == clinicAddress &&
                    v.ClinicPhoneNumber == clinicPhone &&
                    v.Status == VeterinarianStatus.Pending
                )),
                Times.Once
            );
        }

        [Fact]
        public async Task GetByUserIdAsync_WhenCalled_ReturnsVeterinarian()
        {
            // Arrange
            var userId = 123;
            var expectedVet = new Veterinarian
            {
                Id = 1,
                UserId = userId
            };

            _veterinarianRepositoryMock
                .Setup(repo => repo.GetByUserIdAsync(userId))
                .ReturnsAsync(expectedVet);

            // Act
            var result = await _veterinarianService.GetByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);

            _veterinarianRepositoryMock.Verify(
                repo => repo.GetByUserIdAsync(userId),
                Times.Once
            );
        }

        [Fact]
        public async Task GetByIdAsync_WhenCalled_ReturnsVeterinarian()
        {
            // Arrange
            var vetId = 10;
            var expectedVet = new Veterinarian
            {
                Id = vetId,
                UserId = 123
            };

            _veterinarianRepositoryMock
                .Setup(repo => repo.GetByIdAsync(vetId))
                .ReturnsAsync(expectedVet);

            // Act
            var result = await _veterinarianService.GetByIdAsync(vetId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(vetId, result.Id);

            _veterinarianRepositoryMock.Verify(
                repo => repo.GetByIdAsync(vetId),
                Times.Once
            );
        }

        [Fact]
        public async Task ApproveVeterinarianAsync_WhenVetIsFound_UpdatesStatusToApproved()
        {
            // Arrange
            var vetId = 10;
            var veterinarian = new Veterinarian
            {
                Id = vetId,
                Status = VeterinarianStatus.Pending
            };

            _veterinarianRepositoryMock
                .Setup(repo => repo.GetByIdAsync(vetId))
                .ReturnsAsync(veterinarian);

            _veterinarianRepositoryMock
                .Setup(repo => repo.UpdateAsync(It.IsAny<Veterinarian>()))
                .Returns(Task.CompletedTask);

            // Act
            await _veterinarianService.ApproveVeterinarianAsync(vetId);

            // Assert
            Assert.Equal(VeterinarianStatus.Approved, veterinarian.Status);

            _veterinarianRepositoryMock.Verify(
                repo => repo.GetByIdAsync(vetId),
                Times.Once
            );
            _veterinarianRepositoryMock.Verify(
                repo => repo.UpdateAsync(It.Is<Veterinarian>(v => v.Status == VeterinarianStatus.Approved)),
                Times.Once
            );
        }

        [Fact]
        public async Task ApproveVeterinarianAsync_WhenVetNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var vetId = 999;
            _veterinarianRepositoryMock
                .Setup(repo => repo.GetByIdAsync(vetId))
                .ReturnsAsync((Veterinarian)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _veterinarianService.ApproveVeterinarianAsync(vetId)
            );
        }

        [Fact]
        public async Task RejectVeterinarianAsync_WhenVetIsFound_UpdatesStatusToRejected()
        {
            // Arrange
            var vetId = 20;
            var veterinarian = new Veterinarian
            {
                Id = vetId,
                Status = VeterinarianStatus.Pending
            };

            _veterinarianRepositoryMock
                .Setup(repo => repo.GetByIdAsync(vetId))
                .ReturnsAsync(veterinarian);

            _veterinarianRepositoryMock
                .Setup(repo => repo.UpdateAsync(It.IsAny<Veterinarian>()))
                .Returns(Task.CompletedTask);

            // Act
            await _veterinarianService.RejectVeterinarianAsync(vetId);

            // Assert
            Assert.Equal(VeterinarianStatus.Rejected, veterinarian.Status);

            _veterinarianRepositoryMock.Verify(
                repo => repo.GetByIdAsync(vetId),
                Times.Once
            );
            _veterinarianRepositoryMock.Verify(
                repo => repo.UpdateAsync(It.Is<Veterinarian>(v => v.Status == VeterinarianStatus.Rejected)),
                Times.Once
            );
        }

        [Fact]
        public async Task RejectVeterinarianAsync_WhenVetNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var vetId = 999;
            _veterinarianRepositoryMock
                .Setup(repo => repo.GetByIdAsync(vetId))
                .ReturnsAsync((Veterinarian)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _veterinarianService.RejectVeterinarianAsync(vetId)
            );
        }

        [Fact]
        public async Task GetAllVeterinariansAsync_WhenCalled_ReturnsListOfVeterinarians()
        {
            // Arrange
            var veterinarianList = new List<Veterinarian>
            {
                new Veterinarian { Id = 1, UserId = 101 },
                new Veterinarian { Id = 2, UserId = 102 }
            };

            _veterinarianRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(veterinarianList);

            // Act
            var result = await _veterinarianService.GetAllVeterinariansAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Collection(result,
                v => Assert.Equal(101, v.UserId),
                v => Assert.Equal(102, v.UserId)
            );

            _veterinarianRepositoryMock.Verify(
                repo => repo.GetAllAsync(),
                Times.Once
            );
        }
}