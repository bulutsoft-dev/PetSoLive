
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
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _service = new AdoptionRequestService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetAdoptionRequestByIdAsync_ShouldReturnAdoptionRequest_WhenRequestExists()
        {
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                PhoneNumber = "1234567890",
                Address = "123 Test St",
                DateOfBirth = DateTime.Now.AddYears(-30),
                IsActive = true,
                CreatedDate = DateTime.Now,
                ProfileImageUrl = "https://example.com/profile.jpg"
            };
            _context.Users.Add(user);

            var adoptionRequest = new AdoptionRequest
            {
                PetId = 1,
                UserId = user.Id,
                Message = "I want to adopt this pet.",
                Status = AdoptionStatus.Pending,
                RequestDate = DateTime.Now
            };

            _context.AdoptionRequests.Add(adoptionRequest);
            await _context.SaveChangesAsync();

            var result = await _service.GetAdoptionRequestByIdAsync(adoptionRequest.Id);

            Assert.NotNull(result);
            Assert.Equal(adoptionRequest.Id, result.Id);
        }

        [Fact]
        public async Task GetAdoptionRequestsByPetIdAsync_ShouldReturnRequests_WhenRequestsExist()
        {
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

            var result = await _service.GetAdoptionRequestsByPetIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task UpdateAdoptionRequestAsync_ShouldUpdateRequest_WhenValid()
        {
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

            await _service.UpdateAdoptionRequestAsync(adoptionRequest);

            var updatedRequest = await _context.AdoptionRequests.FindAsync(adoptionRequest.Id);
            Assert.NotNull(updatedRequest);
            Assert.Equal(AdoptionStatus.Approved, updatedRequest.Status);
        }

        [Fact]
        public async Task UpdatePetAsync_ShouldUpdatePet_WhenValid()
        {
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

            await _service.UpdatePetAsync(pet);

            var updatedPet = await _context.Pets.FindAsync(pet.Id);
            Assert.NotNull(updatedPet);
            Assert.Equal("Fluffy Updated", updatedPet.Name);
        }
    }
}
