using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Linq.Expressions;
using System.Threading;

namespace PetSoLive.Infrastructure.Repositories.Tests
{
    public class AdoptionRequestRepositoryTests
    {
        private readonly Mock<ApplicationDbContext> _dbContextMock;
        private readonly AdoptionRequestRepository _adoptionRequestRepository;

        public AdoptionRequestRepositoryTests()
        {
            _dbContextMock = new Mock<ApplicationDbContext>();
            _adoptionRequestRepository = new AdoptionRequestRepository(_dbContextMock.Object);
        }

        [Fact]
        public async Task GetAdoptionRequestsByPetIdAsync_ShouldReturnRequests_WhenPetIdIsValid()
        {
            // Arrange
            int petId = 100;
            var adoptionRequests = new List<AdoptionRequest>
            {
                new AdoptionRequest { Id = 1, PetId = petId, UserId = 200, Status = AdoptionStatus.Pending },
                new AdoptionRequest { Id = 2, PetId = petId, UserId = 201, Status = AdoptionStatus.Approved }
            };

            var mockSet = new Mock<DbSet<AdoptionRequest>>();
            mockSet.Setup(m => m.Where(It.IsAny<Expression<Func<AdoptionRequest, bool>>>()))
                   .Returns(adoptionRequests.AsQueryable());
            _dbContextMock.Setup(m => m.AdoptionRequests).Returns(mockSet.Object);

            // Act
            var result = await _adoptionRequestRepository.GetAdoptionRequestsByPetIdAsync(petId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(petId, r.PetId));
        }

        [Fact]
        public async Task GetPendingRequestsByPetIdAsync_ShouldReturnPendingRequests_WhenPetIdIsValid()
        {
            // Arrange
            int petId = 100;
            var adoptionRequests = new List<AdoptionRequest>
            {
                new AdoptionRequest { Id = 1, PetId = petId, UserId = 200, Status = AdoptionStatus.Pending },
                new AdoptionRequest { Id = 2, PetId = petId, UserId = 201, Status = AdoptionStatus.Approved }
            };

            var mockSet = new Mock<DbSet<AdoptionRequest>>();
            mockSet.Setup(m => m.Where(It.IsAny<Expression<Func<AdoptionRequest, bool>>>()))
                   .Returns(adoptionRequests.Where(r => r.PetId == petId && r.Status == AdoptionStatus.Pending).AsQueryable());
            _dbContextMock.Setup(m => m.AdoptionRequests).Returns(mockSet.Object);

            // Act
            var result = await _adoptionRequestRepository.GetPendingRequestsByPetIdAsync(petId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(AdoptionStatus.Pending, result.First().Status);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnAdoptionRequest_WhenIdIsValid()
        {
            // Arrange
            int adoptionRequestId = 1;
            var adoptionRequest = new AdoptionRequest
            {
                Id = adoptionRequestId,
                PetId = 100,
                UserId = 200,
                Status = AdoptionStatus.Pending
            };

            var mockSet = new Mock<DbSet<AdoptionRequest>>();
            mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Expression<Func<AdoptionRequest, bool>>>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(adoptionRequest);
            _dbContextMock.Setup(m => m.AdoptionRequests).Returns(mockSet.Object);

            // Act
            var result = await _adoptionRequestRepository.GetByIdAsync(adoptionRequestId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(adoptionRequestId, result.Id);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateAdoptionRequest_WhenValidRequestIsPassed()
        {
            // Arrange
            var adoptionRequest = new AdoptionRequest
            {
                Id = 1,
                PetId = 100,
                UserId = 200,
                Status = AdoptionStatus.Pending
            };

            var mockSet = new Mock<DbSet<AdoptionRequest>>();
            _dbContextMock.Setup(m => m.AdoptionRequests).Returns(mockSet.Object);
            _dbContextMock.Setup(m => m.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            await _adoptionRequestRepository.UpdateAsync(adoptionRequest);

            // Assert
            mockSet.Verify(m => m.Update(It.IsAny<AdoptionRequest>()), Times.Once);
            _dbContextMock.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }
    }
}
