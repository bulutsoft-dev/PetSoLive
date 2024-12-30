using System;
using Xunit;
using PetSoLive.Core.Entities;

namespace PetSoLive.Tests.UnitTests.EntitiyTests;

public class LostPetAdTests
{
        [Fact]
        public void LostPetAd_ShouldHaveValidProperties_WhenInitialized()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var user = new User
            {
                Id = 10,
                Username = "PetLover",
                Email = "petlover@example.com"
            };

            var lostPetAd = new LostPetAd
            {
                Id = 1,
                PetName = "Rex",
                Description = "Lost dog near the park",
                LastSeenDate = new DateTime(2023, 12, 25),
                ImageUrl = "http://example.com/images/rex.jpg",
                UserId = user.Id,
                User = user,
                LastSeenCity = "TestCity",
                LastSeenDistrict = "TestDistrict",
                CreatedAt = now
            };

            // Act & Assert
            Assert.Equal(1, lostPetAd.Id);
            Assert.Equal("Rex", lostPetAd.PetName);
            Assert.Equal("Lost dog near the park", lostPetAd.Description);
            Assert.Equal(new DateTime(2023, 12, 25), lostPetAd.LastSeenDate);
            Assert.Equal("http://example.com/images/rex.jpg", lostPetAd.ImageUrl);
            Assert.Equal(10, lostPetAd.UserId);

            Assert.NotNull(lostPetAd.User);
            Assert.Equal("PetLover", lostPetAd.User.Username);

            Assert.Equal("TestCity", lostPetAd.LastSeenCity);
            Assert.Equal("TestDistrict", lostPetAd.LastSeenDistrict);
            Assert.Equal(now, lostPetAd.CreatedAt);  // Confirm we can set a custom time
        }

        [Fact]
        public void LostPetAd_LastSeenLocation_ShouldReturnCombinedCityAndDistrict()
        {
            // Arrange
            var lostPetAd = new LostPetAd
            {
                LastSeenCity = "BigCity",
                LastSeenDistrict = "Downtown"
            };

            // Act
            var location = lostPetAd.LastSeenLocation;

            // Assert
            Assert.Equal("BigCity, Downtown", location);
        }

        [Fact]
        public void LostPetAd_CreatedAt_DefaultsToNowIfNotSet()
        {
            // Arrange
            // If we don't explicitly set CreatedAt, it defaults to DateTime.Now.
            var lostPetAd = new LostPetAd
            {
                PetName = "Kitty"
            };

            // Act
            var createdAt = lostPetAd.CreatedAt;

            // Assert
            // Since comparing DateTime.Now can be tricky, let's just assert it's within the last few seconds.
            var timeDifference = DateTime.Now - createdAt;
            Assert.True(timeDifference.TotalSeconds < 5, "CreatedAt should default to a value close to DateTime.Now");
        }
}