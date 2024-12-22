using Moq;
using PetSoLive.Business.Services;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace PetSoLive.Tests.UnitTests
{
    public class AdoptionRequestServiceTests
    {
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly AdoptionRequestService _service;

        public AdoptionRequestServiceTests()
        {
            _mockContext = new Mock<ApplicationDbContext>();
            _service = new AdoptionRequestService(_mockContext.Object);
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
                RequestDate = DateTime.Now
            };

            _mockContext.Setup(c => c.AdoptionRequests
                .FirstOrDefaultAsync(ar => ar.Id == 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(adoptionRequest);

            // Act
            var result = await _service.GetAdoptionRequestByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(adoptionRequest.Id, result.Id);
            Assert.Equal(adoptionRequest.Status, result.Status);
        }

        [Fact]
        public async Task GetAdoptionRequestsByPetIdAsync_ShouldReturnRequests_WhenRequestsExist()
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

            _mockContext.Setup(c => c.AdoptionRequests
                .Where(ar => ar.PetId == 1))
                .Returns(adoptionRequests.AsQueryable());

            // Act
            var result = await _service.GetAdoptionRequestsByPetIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task UpdateAdoptionRequestAsync_ShouldUpdateRequest_WhenValid()
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

            _mockContext.Setup(c => c.AdoptionRequests.Update(adoptionRequest));
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            await _service.UpdateAdoptionRequestAsync(adoptionRequest);

            // Assert
            _mockContext.Verify(c => c.AdoptionRequests.Update(adoptionRequest), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdatePetAsync_ShouldUpdatePet_WhenValid()
        {
            // Arrange
            var pet = new Pet
            {
                Id = 1,
                Name = "Fluffy",
                Species = "Cat",
                Breed = "Persian",
                Age = 3,
                Gender = "Female",
                Weight = 4.5,
                Color = "White",
                DateOfBirth = new DateTime(2020, 5, 10),
                Description = "Friendly and playful.",
                VaccinationStatus = "Up to date",
                MicrochipId = "123456789",
                IsNeutered = true,
                ImageUrl = "url_to_image"
            };

            _mockContext.Setup(c => c.Pets.Update(pet));
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            await _service.UpdatePetAsync(pet);

            // Assert
            _mockContext.Verify(c => c.Pets.Update(pet), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
