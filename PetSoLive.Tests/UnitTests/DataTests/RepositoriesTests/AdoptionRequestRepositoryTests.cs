using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Data;
using PetSoLive.Infrastructure.Repositories;
using Xunit;

namespace PetSoLive.Tests.Repositories
{
    public class AdoptionRequestRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AdoptionRequestRepository _repository;

        public AdoptionRequestRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique database for each test
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new AdoptionRequestRepository(_context);

            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task GetAdoptionRequestsByPetIdAsync_ShouldReturnRequestsForPet()
        {
            // Arrange
            var petId = 1;
            var userId = 1;

            // Create and add Pet and User entities
            var pet = new Pet
            {
                Id = petId,
                Name = "Buddy",
                Species = "Dog",
                Breed = "Golden Retriever",
                Age = 3,
                Gender = "Male",
                Weight = 30.5,
                Color = "Golden",
                Description = "Friendly dog",
                DateOfBirth = DateTime.UtcNow.AddYears(-3),
                VaccinationStatus = "Up-to-date",
                MicrochipId = "12345",
                IsNeutered = true,
                ImageUrl = "http://example.com/pet.jpg"
            };

            var user = new User
            {
                Id = userId,
                Username = "TestUser",
                Email = "testuser@example.com",
                PasswordHash = "hashedpassword",
                PhoneNumber = "1234567890",
                Address = "Test Address",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                ProfileImageUrl = "http://example.com/profile.jpg"
            };

            var adoptionRequests = new List<AdoptionRequest>
            {
                new AdoptionRequest { Id = 1, PetId = petId, UserId = userId, Status = AdoptionStatus.Pending, RequestDate = DateTime.Now, Pet = pet, User = user },
                new AdoptionRequest { Id = 2, PetId = petId, UserId = userId, Status = AdoptionStatus.Approved, RequestDate = DateTime.Now, Pet = pet, User = user }
            };

            _context.Pets.Add(pet);
            _context.Users.Add(user);
            _context.AdoptionRequests.AddRange(adoptionRequests);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAdoptionRequestsByPetIdAsync(petId);

            // Assert
            Assert.Equal(2, result.Count); // Verify two requests for the pet
            Assert.All(result, r => Assert.Equal(petId, r.PetId)); // Ensure each request is for the correct pet
        }

        [Fact]
        public async Task GetPendingRequestsByPetIdAsync_ShouldReturnOnlyPendingRequests()
        {
            // Arrange
            var petId = 1;
            var userId = 1;

            // Create and add Pet and User entities
            var pet = new Pet
            {
                Id = petId,
                Name = "Buddy",
                Species = "Dog",
                Breed = "Golden Retriever",
                Age = 3,
                Gender = "Male",
                Weight = 30.5,
                Color = "Golden",
                Description = "Friendly dog",
                DateOfBirth = DateTime.UtcNow.AddYears(-3),
                VaccinationStatus = "Up-to-date",
                MicrochipId = "12345",
                IsNeutered = true,
                ImageUrl = "http://example.com/pet.jpg"
            };

            var user = new User
            {
                Id = userId,
                Username = "TestUser",
                Email = "testuser@example.com",
                PasswordHash = "hashedpassword",
                PhoneNumber = "1234567890",
                Address = "Test Address",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                ProfileImageUrl = "http://example.com/profile.jpg"
            };

            var adoptionRequests = new List<AdoptionRequest>
            {
                new AdoptionRequest { Id = 1, PetId = petId, UserId = userId, Status = AdoptionStatus.Pending, RequestDate = DateTime.Now, Pet = pet, User = user },
                new AdoptionRequest { Id = 2, PetId = petId, UserId = userId, Status = AdoptionStatus.Approved, RequestDate = DateTime.Now, Pet = pet, User = user }
            };

            _context.Pets.Add(pet);
            _context.Users.Add(user);
            _context.AdoptionRequests.AddRange(adoptionRequests);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetPendingRequestsByPetIdAsync(petId);

            // Assert
            Assert.Single(result); // Only one request should be "Pending"
            Assert.Equal(AdoptionStatus.Pending, result.First().Status); // Ensure the status is "Pending"
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectRequest()
        {
            // Arrange
            var petId = 1;
            var userId = 1;

            // Create and add Pet and User entities
            var pet = new Pet
            {
                Id = petId,
                Name = "Buddy",
                Species = "Dog",
                Breed = "Golden Retriever",
                Age = 3,
                Gender = "Male",
                Weight = 30.5,
                Color = "Golden",
                Description = "Friendly dog",
                DateOfBirth = DateTime.UtcNow.AddYears(-3),
                VaccinationStatus = "Up-to-date",
                MicrochipId = "12345",
                IsNeutered = true,
                ImageUrl = "http://example.com/pet.jpg"
            };

            var user = new User
            {
                Id = userId,
                Username = "TestUser",
                Email = "testuser@example.com",
                PasswordHash = "hashedpassword",
                PhoneNumber = "1234567890",
                Address = "Test Address",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                ProfileImageUrl = "http://example.com/profile.jpg"
            };

            var adoptionRequest = new AdoptionRequest
            {
                Id = 1,
                PetId = petId,
                UserId = userId,
                Status = AdoptionStatus.Pending,
                RequestDate = DateTime.Now,
                Pet = pet,
                User = user
            };

            _context.Pets.Add(pet);
            _context.Users.Add(user);
            _context.AdoptionRequests.Add(adoptionRequest);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result); // Ensure the result is not null
            Assert.Equal(adoptionRequest.Id, result.Id); // Verify the correct adoption request is returned
            Assert.Equal(AdoptionStatus.Pending, result.Status); // Verify the status is "Pending"
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNullIfNotFound()
        {
            // Act
            var result = await _repository.GetByIdAsync(99); // ID 99 doesn't exist

            // Assert
            Assert.Null(result); // Ensure null is returned when the request doesn't exist
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateRequestStatus()
        {
            // Arrange
            var petId = 1;
            var userId = 1;

            // Create and add Pet and User entities
            var pet = new Pet
            {
                Id = petId,
                Name = "Buddy",
                Species = "Dog",
                Breed = "Golden Retriever",
                Age = 3,
                Gender = "Male",
                Weight = 30.5,
                Color = "Golden",
                Description = "Friendly dog",
                DateOfBirth = DateTime.UtcNow.AddYears(-3),
                VaccinationStatus = "Up-to-date",
                MicrochipId = "12345",
                IsNeutered = true,
                ImageUrl = "http://example.com/pet.jpg"
            };

            var user = new User
            {
                Id = userId,
                Username = "TestUser",
                Email = "testuser@example.com",
                PasswordHash = "hashedpassword",
                PhoneNumber = "1234567890",
                Address = "Test Address",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                ProfileImageUrl = "http://example.com/profile.jpg"
            };

            var adoptionRequest = new AdoptionRequest
            {
                Id = 1,
                PetId = petId,
                UserId = userId,
                Status = AdoptionStatus.Pending,
                RequestDate = DateTime.Now,
                Pet = pet,
                User = user
            };

            _context.Pets.Add(pet);
            _context.Users.Add(user);
            _context.AdoptionRequests.Add(adoptionRequest);
            await _context.SaveChangesAsync();

            // Act
            adoptionRequest.Status = AdoptionStatus.Approved; // Update the status to Approved
            await _repository.UpdateAsync(adoptionRequest);

            // Assert
            var updatedRequest = await _context.AdoptionRequests.FindAsync(1);
            Assert.Equal(AdoptionStatus.Approved, updatedRequest.Status); // Ensure the status was updated to "Approved"
        }
    }
}
