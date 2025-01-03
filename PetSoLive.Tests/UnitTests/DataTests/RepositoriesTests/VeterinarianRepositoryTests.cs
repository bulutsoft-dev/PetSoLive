using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

// Adjust to your actual project namespaces
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Interfaces;
using PetSoLive.Data;
using PetSoLive.Infrastructure.Repositories;

namespace PetSoLive.Tests.UnitTests.DataTests.RepositoriesTests;

public class VeterinarianRepositoryTests
{
     private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB name per test
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetByUserIdAsync_WhenVetExists_ReturnsVet()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            IVeterinarianRepository repository = new VeterinarianRepository(context);

            var vet = new Veterinarian
            {
                Id = 1,
                UserId = 123,
                Status = VeterinarianStatus.Pending,
                ClinicAddress = "Clinic Address",
                ClinicPhoneNumber = "555-2222",
                Qualifications = "Qualifications",
            };
            context.Veterinarians.Add(vet);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByUserIdAsync(123);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(123, result.UserId);
        }

        [Fact]
        public async Task GetByUserIdAsync_WhenVetNotFound_ReturnsNull()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            IVeterinarianRepository repository = new VeterinarianRepository(context);

            // No veterinarian for user 999

            // Act
            var result = await repository.GetByUserIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_WhenCalled_SavesVetToDatabase()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            IVeterinarianRepository repository = new VeterinarianRepository(context);

            var vet = new Veterinarian
            {
                Id = 2,
                UserId = 456,
                Qualifications = "Vet Qualifications",
                ClinicAddress = "123 Vet Street",
                ClinicPhoneNumber = "555-1234",
                Status = VeterinarianStatus.Pending
            };

            // Act
            var createdVet = await repository.CreateAsync(vet);

            // Assert
            Assert.NotNull(createdVet);
            Assert.Equal(2, createdVet.Id);

            var savedVet = await context.Veterinarians.FindAsync(2);
            Assert.NotNull(savedVet);
            Assert.Equal("Vet Qualifications", savedVet.Qualifications);
        }

        [Fact]
        public async Task UpdateAsync_WhenCalled_UpdatesVetInDatabase()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            IVeterinarianRepository repository = new VeterinarianRepository(context);

            var vet = new Veterinarian
            {
                Id = 3,
                UserId = 789,
                Qualifications = "Original Quals",
                Status = VeterinarianStatus.Pending,
                ClinicAddress = "123 Vet Street",
                ClinicPhoneNumber = "555-2222",
            };
            context.Veterinarians.Add(vet);
            await context.SaveChangesAsync();

            // Act
            vet.Qualifications = "Updated Quals";
            vet.Status = VeterinarianStatus.Approved;
            await repository.UpdateAsync(vet);

            // Assert
            var updatedVet = await context.Veterinarians.FindAsync(3);
            Assert.NotNull(updatedVet);
            Assert.Equal("Updated Quals", updatedVet.Qualifications);
            Assert.Equal(VeterinarianStatus.Approved, updatedVet.Status);
        }

        [Fact]
        public async Task GetAllAsync_WhenCalled_ReturnsAllVetsWithUsers()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            IVeterinarianRepository repository = new VeterinarianRepository(context);

            var user = new User { Id = 1, Username = "VetUser", Email = "VetUser@gmail.com", Address = "aaa",PasswordHash = "pass", PhoneNumber = "555-111", ProfileImageUrl = "ImageUrl"};
            context.Users.Add(user);
            var vet = new Veterinarian
            {
                Id = 10,
                UserId = user.Id,
                Status = VeterinarianStatus.Pending,
                Qualifications = "Vet Qualifications",
                ClinicAddress = "123 Vet Street",
                ClinicPhoneNumber = "555-1234",
                
            };
            context.Veterinarians.Add(vet);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            var vetList = result.ToList();
            Assert.Single(vetList);
            Assert.Equal(10, vetList[0].Id);
            Assert.NotNull(vetList[0].User);
            Assert.Equal("VetUser", vetList[0].User.Username);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCalled_ReturnsVet()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            IVeterinarianRepository repository = new VeterinarianRepository(context);

            var vet = new Veterinarian
            {
                Id = 20,
                UserId = 999,
                Status = VeterinarianStatus.Pending,
                Qualifications = "Vet Qualifications",
                ClinicAddress = "123 Vet Street",
                ClinicPhoneNumber = "555-1234",
            };
            context.Veterinarians.Add(vet);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(20);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20, result.Id);
            Assert.Equal(999, result.UserId);
        }

        [Fact]
        public async Task GetAllVeterinariansAsync_WhenCalled_ReturnsOnlyApprovedVets()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            IVeterinarianRepository repository = new VeterinarianRepository(context);

            var vetPending = new Veterinarian { Id = 100, Status = VeterinarianStatus.Pending, ClinicAddress = "aaa", ClinicPhoneNumber = "555-1234" , Qualifications = "1"};
            var vetApproved = new Veterinarian { Id = 101, Status = VeterinarianStatus.Approved,ClinicAddress = "aaa", ClinicPhoneNumber = "555-1234" , Qualifications = "1" };
            var vetRejected = new Veterinarian { Id = 102, Status = VeterinarianStatus.Rejected,ClinicAddress = "aaa", ClinicPhoneNumber = "555-1234" , Qualifications = "1" };

            context.Veterinarians.AddRange(vetPending, vetApproved, vetRejected);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetAllVeterinariansAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result); // Only the approved one
            Assert.Equal(101, result.First().Id);
        }
        
        [Fact]
        public async Task GetApprovedByUserIdAsync_WhenVetExistsAndApproved_ReturnsApprovedVet()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            IVeterinarianRepository repository = new VeterinarianRepository(context);

            var vet = new Veterinarian
            {
                Id = 1,
                UserId = 123,
                Status = VeterinarianStatus.Approved,
                ClinicAddress = "Clinic Address",
                ClinicPhoneNumber = "555-2222",
                Qualifications = "Qualifications",
            };
            context.Veterinarians.Add(vet);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetApprovedByUserIdAsync(123);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(123, result.UserId);
            Assert.Equal(VeterinarianStatus.Approved, result.Status);
        }

        [Fact]
        public async Task GetApprovedByUserIdAsync_WhenVetExistsButNotApproved_ReturnsNull()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            IVeterinarianRepository repository = new VeterinarianRepository(context);

            var vet = new Veterinarian
            {
                Id = 2,
                UserId = 456,
                Status = VeterinarianStatus.Pending, // Not approved
                ClinicAddress = "Clinic Address",
                ClinicPhoneNumber = "555-2222",
                Qualifications = "Qualifications",
            };
            context.Veterinarians.Add(vet);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetApprovedByUserIdAsync(456);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetApprovedByUserIdAsync_WhenVetDoesNotExist_ReturnsNull()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            IVeterinarianRepository repository = new VeterinarianRepository(context);

            // No veterinarian for user 999

            // Act
            var result = await repository.GetApprovedByUserIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        
        
}