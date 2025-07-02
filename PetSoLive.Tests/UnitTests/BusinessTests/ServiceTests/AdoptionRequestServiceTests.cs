using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Interfaces;
using PetSoLive.Business.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PetSoLive.Tests.UnitTests
{
    public class AdoptionRequestServiceTests
    {
        private readonly Mock<IAdoptionRequestRepository> _adoptionRequestRepositoryMock;
        private readonly AdoptionRequestService _service;

        public AdoptionRequestServiceTests()
        {
            _adoptionRequestRepositoryMock = new Mock<IAdoptionRequestRepository>();
            _service = new AdoptionRequestService(_adoptionRequestRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAdoptionRequestByIdAsync_ShouldReturnAdoptionRequest_WhenRequestExists()
        {
            // Arrange
            var adoptionRequest = new AdoptionRequest
            {
                Id = 1,
                PetId = 1,
                UserId = 1,
                Message = "I want to adopt this pet.",
                Status = AdoptionStatus.Pending,
                RequestDate = DateTime.Now,
                User = new User { Id = 1, Username = "testuser" },
                Pet = new Pet { Id = 1, Name = "Fluffy" }
            };

            _adoptionRequestRepositoryMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(adoptionRequest);

            // Act
            var result = await _service.GetAdoptionRequestByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(adoptionRequest.Id, result.Id);
            Assert.Equal(adoptionRequest.Message, result.Message);
            Assert.Equal(adoptionRequest.Status, result.Status);
            Assert.NotNull(result.User);
            Assert.NotNull(result.Pet);
        }

        [Fact]
        public async Task GetAdoptionRequestByIdAsync_ShouldThrowKeyNotFoundException_WhenRequestDoesNotExist()
        {
            // Arrange
            _adoptionRequestRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((AdoptionRequest)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetAdoptionRequestByIdAsync(999));
        }

        [Fact]
        public async Task GetPendingRequestsByPetIdAsync_ShouldReturnRequests_WhenRequestsExist()
        {
            // Arrange
            var adoptionRequests = new List<AdoptionRequest>
            {
                new AdoptionRequest
                {
                    Id = 1,
                    PetId = 1,
                    UserId = 1,
                    Message = "I want to adopt this pet.",
                    Status = AdoptionStatus.Pending,
                    RequestDate = DateTime.Now
                },
                new AdoptionRequest
                {
                    Id = 2,
                    PetId = 1,
                    UserId = 2,
                    Message = "I also want to adopt this pet.",
                    Status = AdoptionStatus.Pending,
                    RequestDate = DateTime.Now
                }
            };

            _adoptionRequestRepositoryMock.Setup(repo => repo.GetPendingRequestsByPetIdAsync(1))
                .ReturnsAsync(adoptionRequests);

            // Act
            var result = await _service.GetPendingRequestsByPetIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(AdoptionStatus.Pending, r.Status));
        }

        [Fact]
        public async Task GetPendingRequestsByPetIdAsync_ShouldReturnEmptyList_WhenNoRequestsExist()
        {
            // Arrange
            _adoptionRequestRepositoryMock.Setup(repo => repo.GetPendingRequestsByPetIdAsync(1))
                .ReturnsAsync(new List<AdoptionRequest>());

            // Act
            var result = await _service.GetPendingRequestsByPetIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task UpdateAdoptionRequestAsync_ShouldCallRepositoryUpdate_WhenValid()
        {
            // Arrange
            var adoptionRequest = new AdoptionRequest
            {
                Id = 1,
                PetId = 1,
                UserId = 1,
                Message = "I want to adopt this pet.",
                Status = AdoptionStatus.Pending,
                RequestDate = DateTime.Now
            };

            _adoptionRequestRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<AdoptionRequest>()))
                .Returns(Task.CompletedTask);

            adoptionRequest.Status = AdoptionStatus.Approved;

            // Act
            await _service.UpdateAdoptionRequestAsync(adoptionRequest);

            // Assert
            _adoptionRequestRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<AdoptionRequest>(r => r.Id == 1 && r.Status == AdoptionStatus.Approved)), Times.Once());
        }
    }
}