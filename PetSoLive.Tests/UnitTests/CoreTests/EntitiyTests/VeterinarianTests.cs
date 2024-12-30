using System;
using System.Collections.Generic;
using Xunit;
using PetSoLive.Core.Entities;  // Adjust namespace as needed
using PetSoLive.Core.Enums;
namespace PetSoLive.Tests.UnitTests.EntitiyTests;

public class VeterinarianTests
{
    [Fact]
        public void Veterinarian_ShouldHaveValidProperties_WhenInitialized()
        {
            // Arrange
            var user = new User
            {
                Id = 10,
                Username = "VetUser",
                Email = "vetuser@example.com"
            };

            var comments = new List<Comment>
            {
                new Comment { Id = 1, Content = "Sample Comment 1" },
                new Comment { Id = 2, Content = "Sample Comment 2" }
            };

            var appliedDate = new DateTime(2023, 12, 1);

            var vet = new Veterinarian
            {
                Id = 1,
                UserId = user.Id,
                User = user,
                Qualifications = "DVM",
                ClinicAddress = "123 Vet Street",
                ClinicPhoneNumber = "555-1234",
                AppliedDate = appliedDate,
                Status = VeterinarianStatus.Pending,
                Comments = comments
            };

            // Act & Assert
            Assert.Equal(1, vet.Id);
            Assert.Equal(10, vet.UserId);
            Assert.NotNull(vet.User);
            Assert.Equal("VetUser", vet.User.Username);
            Assert.Equal("DVM", vet.Qualifications);
            Assert.Equal("123 Vet Street", vet.ClinicAddress);
            Assert.Equal("555-1234", vet.ClinicPhoneNumber);
            Assert.Equal(appliedDate, vet.AppliedDate);
            Assert.Equal(VeterinarianStatus.Pending, vet.Status);

            // Comments collection
            Assert.NotNull(vet.Comments);
            Assert.Equal(2, vet.Comments.Count);
            Assert.Contains(vet.Comments, c => c.Id == 1);
            Assert.Contains(vet.Comments, c => c.Id == 2);
        }

        [Fact]
        public void Veterinarian_CommentsCanBeNullOrEmpty_ByDefault()
        {
            // Arrange
            var vet = new Veterinarian
            {
                Id = 2,
                UserId = 20,
                Qualifications = "Expert in small animals"
                // Comments not assigned explicitly
            };

            // Act & Assert
            Assert.Null(vet.Comments);
    }
}