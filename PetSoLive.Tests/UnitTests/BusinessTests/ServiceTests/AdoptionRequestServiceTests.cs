using Microsoft.EntityFrameworkCore;
using PetSoLive.Business.Services;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Data;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetSoLive.Tests.UnitTests
{
    public class AdoptionRequestServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AdoptionRequestService _service;

        public AdoptionRequestServiceTests()
        {
            // Create a unique In-Memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique database for each test
                .Options;

            _context = new ApplicationDbContext(options);
            _service = new AdoptionRequestService(_context);
        }

        public void Dispose()
        {
            // Clean up the database between tests
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetAdoptionRequestByIdAsync_ShouldReturnAdoptionRequest_WhenRequestExists()
        {
            // Arrange
            var adoptionRequest = new AdoptionRequest
            {
                PetId = 1,
                UserId = 1,
                Message = "I want to adopt this pet.",
                Status = AdoptionStatus.Pending,
                RequestDate = DateTime.Now
            };

            // Add the adoption request to the in-memory database and save changes
            _context.AdoptionRequests.Add(adoptionRequest);
            await _context.SaveChangesAsync();

            // Ensure that the AdoptionRequest has been added to the database and the ID is correctly set
            var savedRequest = await _context.AdoptionRequests
                .FirstOrDefaultAsync(ar => ar.UserId == adoptionRequest.UserId && ar.PetId == adoptionRequest.PetId);

            Assert.NotNull(savedRequest);  // Verify the request was saved correctly
            Assert.NotEqual(0, savedRequest.Id);  // Verify that the ID is set (not zero)

            // Act: Retrieve the adoption request by its generated ID
            var result = await _service.GetAdoptionRequestByIdAsync(savedRequest.Id);

            // Debug log (Optional): Verify the result
            if (result == null)
            {
                Console.WriteLine("No adoption request found with ID: " + savedRequest.Id);
            }

            // Assert: Ensure the result is not null and the ID matches
            Assert.NotNull(result);  
            Assert.Equal(savedRequest.Id, result.Id);  
            Assert.Equal(savedRequest.Status, result.Status);  
        }


        [Fact]
        public async Task GetAdoptionRequestsByPetIdAsync_ShouldReturnRequests_WhenRequestsExist()
        {
            // Arrange
            var adoptionRequests = new List<AdoptionRequest>
            {
                new AdoptionRequest
                {
                    PetId = 1,
                    UserId = 1,
                    Message = "I want to adopt this pet.",
                    Status = AdoptionStatus.Pending,
                    RequestDate = DateTime.Now
                },
                new AdoptionRequest
                {
                    PetId = 1,
                    UserId = 2,
                    Message = "I also want to adopt this pet.",
                    Status = AdoptionStatus.Pending,
                    RequestDate = DateTime.Now
                }
            };

            _context.AdoptionRequests.AddRange(adoptionRequests);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAdoptionRequestsByPetIdAsync(1);

            // Assert
            Assert.NotNull(result);  // Ensure the result is not null
            Assert.Equal(2, result.Count);  // Verify that two requests were returned
        }

        [Fact]
        public async Task UpdateAdoptionRequestAsync_ShouldUpdateRequest_WhenValid()
        {
            // Arrange
            var adoptionRequest = new AdoptionRequest
            {
                PetId = 1,
                UserId = 1,
                Message = "I want to adopt this pet.",
                Status = AdoptionStatus.Pending,
                RequestDate = DateTime.Now
            };

            _context.AdoptionRequests.Add(adoptionRequest);
            await _context.SaveChangesAsync();

            adoptionRequest.Status = AdoptionStatus.Approved;

            // Act
            await _service.UpdateAdoptionRequestAsync(adoptionRequest);

            // Assert
            var updatedRequest = await _context.AdoptionRequests.FindAsync(adoptionRequest.Id);
            Assert.NotNull(updatedRequest);  // Ensure the request was updated
            Assert.Equal(AdoptionStatus.Approved, updatedRequest.Status);  // Verify the status was updated
        }

        [Fact]
        public async Task UpdatePetAsync_ShouldUpdatePet_WhenValid()
        {
            // Arrange
            var pet = new Pet
            {
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

            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            pet.Name = "Fluffy Updated";

            // Act
            await _service.UpdatePetAsync(pet);

            // Assert
            var updatedPet = await _context.Pets.FindAsync(pet.Id);
            Assert.NotNull(updatedPet);  // Ensure the pet was updated
            Assert.Equal("Fluffy Updated", updatedPet.Name);  // Verify the name was updated
        }
    }
}
