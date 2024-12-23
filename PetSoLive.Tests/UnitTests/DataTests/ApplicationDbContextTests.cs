using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetSoLive.Core.Entities;
using PetSoLive.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using PetSoLive.Core.Enums;
using Xunit;

namespace PetSoLive.Tests
{
    public class ApplicationDbContextTests
    {
        private DbContextOptions<ApplicationDbContext> CreateInMemoryDbContextOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
        }

        [Fact]
        public async Task AddUser_Should_Add_User_To_Db()
        {
            // Arrange
            var options = CreateInMemoryDbContextOptions();
            var context = new ApplicationDbContext(options);

            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                PhoneNumber = "1234567890",
                Address = "Test Address",
                DateOfBirth = DateTime.Now.AddYears(-25),
                IsActive = true,
                CreatedDate = DateTime.Now,
                ProfileImageUrl = "http://example.com/profile.jpg"
            };

            // Act
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Assert
            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
            Assert.NotNull(savedUser);
            Assert.Equal("testuser", savedUser.Username);
        }

        [Fact]
        public async Task AddAdoptionRequest_Should_Add_AdoptionRequest_To_Db()
        {
            // Arrange
            var options = CreateInMemoryDbContextOptions();
            var context = new ApplicationDbContext(options);

            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                PhoneNumber = "1234567890",
                Address = "Test Address",
                DateOfBirth = DateTime.Now.AddYears(-25),
                IsActive = true,
                CreatedDate = DateTime.Now,
                ProfileImageUrl = "http://example.com/profile.jpg"
            };

            var pet = new Pet
            {
                Name = "Fluffy",
                Species = "Cat",
                Breed = "Persian",
                Age = 3,
                Gender = "Female",
                Weight = 4.5,
                Color = "White",
                DateOfBirth = DateTime.Now.AddYears(-3),
                Description = "Cute cat",
                VaccinationStatus = "Up to date",
                MicrochipId = "1234567890",
                IsNeutered = true,
                ImageUrl = "http://example.com/fluffy.jpg"
            };

            var adoptionRequest = new AdoptionRequest
            {
                Pet = pet,
                User = user,
                Message = "I would love to adopt this pet.",
                Status = AdoptionStatus.Pending,
                RequestDate = DateTime.Now
            };

            // Act
            context.Users.Add(user);
            context.Pets.Add(pet);
            context.AdoptionRequests.Add(adoptionRequest);
            await context.SaveChangesAsync();

            // Assert
            var savedRequest = await context.AdoptionRequests
                .FirstOrDefaultAsync(ar => ar.UserId == user.Id && ar.PetId == pet.Id);
            Assert.NotNull(savedRequest);
            Assert.Equal(AdoptionStatus.Pending, savedRequest.Status);
        }

        [Fact]
        public async Task AddPetOwner_Should_Associate_Pet_With_User()
        {
            // Arrange
            var options = CreateInMemoryDbContextOptions();
            var context = new ApplicationDbContext(options);

            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                PhoneNumber = "1234567890",
                Address = "Test Address",
                DateOfBirth = DateTime.Now.AddYears(-25),
                IsActive = true,
                CreatedDate = DateTime.Now,
                ProfileImageUrl = "http://example.com/profile.jpg"
            };

            var pet = new Pet
            {
                Name = "Fluffy",
                Species = "Cat",
                Breed = "Persian",
                Age = 3,
                Gender = "Female",
                Weight = 4.5,
                Color = "White",
                DateOfBirth = DateTime.Now.AddYears(-3),
                Description = "Cute cat",
                VaccinationStatus = "Up to date",
                MicrochipId = "1234567890",
                IsNeutered = true,
                ImageUrl = "http://example.com/fluffy.jpg"
            };

            var petOwner = new PetOwner
            {
                Pet = pet,
                User = user,
                OwnershipDate = DateTime.Now
            };

            // Act
            context.Users.Add(user);
            context.Pets.Add(pet);
            context.PetOwners.Add(petOwner);
            await context.SaveChangesAsync();

            // Assert
            var savedPetOwner = await context.PetOwners
                .FirstOrDefaultAsync(po => po.UserId == user.Id && po.PetId == pet.Id);
            Assert.NotNull(savedPetOwner);
            Assert.Equal(user.Id, savedPetOwner.UserId);
            Assert.Equal(pet.Id, savedPetOwner.PetId);
        }
    }
}
