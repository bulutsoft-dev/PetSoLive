using System;
using Xunit;
using PetSoLive.Core.Entities;
namespace PetSoLive.Tests.UnitTests.EntitiyTests;

    public class CommentTests
    {
        [Fact]
        public void Comment_ShouldHaveValidProperties_WhenAllAreSet()
        {
            // Arrange
            var now = DateTime.UtcNow;

            // Minimal stubs for related entities
            var helpRequest = new HelpRequest
            {
                Id = 10,
                Title = "Test Help Request"
            };

            var user = new User
            {
                Id = 1,
                Username = "TestUser"
            };

            var veterinarian = new Veterinarian
            {
                Id = 100,
                Qualifications = "DVM",
                ClinicAddress = "123 Vet Lane"
            };

            // Create a Comment with all fields assigned
            var comment = new Comment
            {
                Id = 5,
                HelpRequestId = helpRequest.Id,
                HelpRequest = helpRequest,
                UserId = user.Id,
                User = user,
                VeterinarianId = veterinarian.Id,
                Veterinarian = veterinarian,
                Content = "This is a test comment.",
                CreatedAt = now
            };

            // Act & Assert
            Assert.Equal(5, comment.Id);
            Assert.Equal(10, comment.HelpRequestId);
            Assert.Equal(1, comment.UserId);
            Assert.Equal(100, comment.VeterinarianId);
            Assert.Equal("This is a test comment.", comment.Content);
            Assert.Equal(now, comment.CreatedAt);

            // Navigation properties
            Assert.NotNull(comment.HelpRequest);
            Assert.Equal("Test Help Request", comment.HelpRequest.Title);

            Assert.NotNull(comment.User);
            Assert.Equal("TestUser", comment.User.Username);

            Assert.NotNull(comment.Veterinarian);
            Assert.Equal("DVM", comment.Veterinarian.Qualifications);
        }

        [Fact]
        public void Comment_CanBeCreatedWithoutVeterinarian()
        {
            // Arrange
            var now = DateTime.UtcNow;

            var helpRequest = new HelpRequest
            {
                Id = 20,
                Title = "Emergency Help Request"
            };

            var user = new User
            {
                Id = 2,
                Username = "AnotherUser"
            };

            // Veterinarian is purposely omitted to show it's optional
            var comment = new Comment
            {
                Id = 6,
                HelpRequestId = helpRequest.Id,
                HelpRequest = helpRequest,
                UserId = user.Id,
                User = user,
                VeterinarianId = null,
                Veterinarian = null, 
                Content = "No veterinarian assigned",
                CreatedAt = now
            };

            // Act & Assert
            Assert.Equal(6, comment.Id);
            Assert.Equal(20, comment.HelpRequestId);
            Assert.Equal(2, comment.UserId);
            Assert.Null(comment.VeterinarianId);
            Assert.Null(comment.Veterinarian);
            Assert.Equal("No veterinarian assigned", comment.Content);
            Assert.Equal(now, comment.CreatedAt);
        }
}