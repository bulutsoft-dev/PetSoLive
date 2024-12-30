using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

// Adjust these namespaces to match your project
using PetSoLive.Core.Entities;      // Where LostPetAd entity resides
using PetSoLive.Data;
using PetSoLive.Core.Interfaces;
using PetSoLive.Data.Repositories;  // If your repository is in this namespace

namespace PetSoLive.Tests.UnitTests.DataTests.RepositoriesTests;

public class LostPetRepositoryTests
{
     private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task CreateLostPetAdAsync_WhenCalled_AddsLostPetAdToDatabase()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            ILostPetAdRepository repository = new LostPetAdRepository(context);

            var lostPetAd = new LostPetAd
            {
                Id = 1,
                PetName = "Fluffy",
                Description = "White cat with blue eyes",
                LastSeenDate = DateTime.UtcNow.AddDays(-1),
                LastSeenCity = "CityX",
                LastSeenDistrict = "DistrictY",
                CreatedAt = DateTime.UtcNow,
                ImageUrl = "imageUrl",
                UserId = 123
            };

            // Act
            await repository.CreateLostPetAdAsync(lostPetAd);

            // Assert
            var inserted = await context.LostPetAds.FindAsync(1);
            Assert.NotNull(inserted);
            Assert.Equal("Fluffy", inserted.PetName);
            Assert.Equal("White cat with blue eyes", inserted.Description);
            Assert.Equal("CityX", inserted.LastSeenCity);
            Assert.Equal("DistrictY", inserted.LastSeenDistrict);
            Assert.Equal(123, inserted.UserId);
        }

        [Fact]
        public async Task GetAllAsync_WhenCalled_ReturnsAllLostPetAds()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            ILostPetAdRepository repository = new LostPetAdRepository(context);

            var ads = new List<LostPetAd>
            {
                new LostPetAd { Id = 1, PetName = "Cat1", Description = "White cat with blue eyes", ImageUrl = "imageUrl", LastSeenDate = DateTime.UtcNow.AddDays(-1) },
                new LostPetAd { Id = 2, PetName = "Cat2", Description = "White cat with blue eyes", ImageUrl = "imageUrl", LastSeenDate = DateTime.UtcNow.AddDays(-1) }
            };
            context.LostPetAds.AddRange(ads);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, ad => ad.PetName == "Cat1");
            Assert.Contains(list, ad => ad.PetName == "Cat2");
        }

        [Fact]
        public async Task GetByIdAsync_WhenAdExists_ReturnsLostPetAd()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            ILostPetAdRepository repository = new LostPetAdRepository(context);

            var ad = new LostPetAd { Id = 10, PetName = "Doggo",Description = "White cat with blue eyes", ImageUrl = "imageUrl", LastSeenDate = DateTime.UtcNow.AddDays(-1) };
            context.LostPetAds.Add(ad);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Doggo", result.PetName);
        }

        [Fact]
        public async Task UpdateLostPetAdAsync_WhenCalled_UpdatesLostPetAdInDatabase()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            ILostPetAdRepository repository = new LostPetAdRepository(context);

            var ad = new LostPetAd
            {
                Id = 2,
                PetName = "Lost Kitty",
                Description = "Initial description",
                ImageUrl = "imageUrl",
            };
            context.LostPetAds.Add(ad);
            await context.SaveChangesAsync();

            // Act
            ad.Description = "Updated description";
            await repository.UpdateLostPetAdAsync(ad);

            // Assert
            var updatedAd = await context.LostPetAds.FindAsync(2);
            Assert.NotNull(updatedAd);
            Assert.Equal("Updated description", updatedAd.Description);
        }

        [Fact]
        public async Task DeleteLostPetAdAsync_WhenCalled_RemovesLostPetAdFromDatabase()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            ILostPetAdRepository repository = new LostPetAdRepository(context);

            var ad = new LostPetAd { Id = 3, PetName = "DogLost", Description = "White cat with blue eyes", ImageUrl = "imageUrl" };
            context.LostPetAds.Add(ad);
            await context.SaveChangesAsync();

            // Act
            await repository.DeleteLostPetAdAsync(ad);

            // Assert
            var deletedAd = await context.LostPetAds.FindAsync(3);
            Assert.Null(deletedAd);
        }
}