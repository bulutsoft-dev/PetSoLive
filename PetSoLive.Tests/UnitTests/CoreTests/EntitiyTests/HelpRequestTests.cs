using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums; 

namespace PetSoLive.Tests.UnitTests.EntitiyTests;

public class HelpRequestTests
{
            [Fact]
        public void HelpRequest_ShouldHaveValidProperties_WhenInitialized()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var helpRequest = new HelpRequest
            {
                Id = 1,
                Title = "Need help with injured dog",
                Description = "A stray dog was found injured and needs medical attention",
                EmergencyLevel = EmergencyLevel.High,
                CreatedAt = now,
                UserId = 10,
                Location = "123 Main Street",
                ContactName = "John Doe",
                ContactPhone = "555-1234",
                ContactEmail = "john@example.com",
                ImageUrl = "http://example.com/image.jpg",
                Status = HelpRequestStatus.Active
            };

            // Act & Assert
            Assert.Equal(1, helpRequest.Id);
            Assert.Equal("Need help with injured dog", helpRequest.Title);
            Assert.Equal("A stray dog was found injured and needs medical attention", helpRequest.Description);
            Assert.Equal(EmergencyLevel.High, helpRequest.EmergencyLevel);
            Assert.Equal(now, helpRequest.CreatedAt);
            Assert.Equal(10, helpRequest.UserId);
            Assert.Equal("123 Main Street", helpRequest.Location);
            Assert.Equal("John Doe", helpRequest.ContactName);
            Assert.Equal("555-1234", helpRequest.ContactPhone);
            Assert.Equal("john@example.com", helpRequest.ContactEmail);
            Assert.Equal("http://example.com/image.jpg", helpRequest.ImageUrl);
            Assert.Equal(HelpRequestStatus.Active, helpRequest.Status);
            Assert.Null(helpRequest.Comments); // default is null if not assigned
        }

        [Fact]
        public void HelpRequest_ValidModel_ShouldPassValidation()
        {
            // Arrange
            var validHelpRequest = new HelpRequest
            {
                Title = "Need help with injured dog",
                Description = "Dog found near Main Street, appears severely injured.",
                EmergencyLevel = EmergencyLevel.Medium,
                CreatedAt = DateTime.UtcNow,
                UserId = 1,
                Location = "Main Street Park",
                ContactName = "Jane Smith",
                ContactPhone = "+1-202-555-0147",
                ContactEmail = "jane@example.com",
                Status = HelpRequestStatus.Active
            };

            // Act
            var validationResults = ValidateModel(validHelpRequest);

            // Assert
            Assert.Empty(validationResults); // No validation errors should appear
        }

        [Fact]
        public void HelpRequest_MissingRequiredFields_ShouldFailValidation()
        {
            // Arrange
            var invalidHelpRequest = new HelpRequest
            {
                // Title is missing
                // Description is missing
                // EmergencyLevel is missing
                CreatedAt = DateTime.UtcNow,
                UserId = 2,
                // Location is missing
                Status = HelpRequestStatus.Active
            };

            // Act
            var validationResults = ValidateModel(invalidHelpRequest);
            var errorMessages = validationResults.Select(r => r.ErrorMessage).ToList();

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains("Title is required.", errorMessages);
            Assert.Contains("Description is required.", errorMessages);
            Assert.Contains("Location is required.", errorMessages);
        }

        [Fact]
        public void HelpRequest_TitleTooLong_ShouldFailValidation()
        {
            // Arrange
            var tooLongTitle = new string('A', 101); // 101 characters
            var helpRequest = new HelpRequest
            {
                Title = tooLongTitle,
                Description = "Some description",
                EmergencyLevel = EmergencyLevel.Low,
                CreatedAt = DateTime.UtcNow,
                UserId = 1,
                Location = "Short location",
                Status = HelpRequestStatus.Active
            };

            // Act
            var validationResults = ValidateModel(helpRequest);
            var errorMessages = validationResults.Select(r => r.ErrorMessage).ToList();

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains("Title can't be longer than 100 characters.", errorMessages);
        }

        [Fact]
        public void HelpRequest_DescriptionTooLong_ShouldFailValidation()
        {
            // Arrange
            var tooLongDescription = new string('D', 501); // 501 characters
            var helpRequest = new HelpRequest
            {
                Title = "Valid Title",
                Description = tooLongDescription,
                EmergencyLevel = EmergencyLevel.Low,
                CreatedAt = DateTime.UtcNow,
                UserId = 1,
                Location = "Main Street",
                Status = HelpRequestStatus.Active
            };

            // Act
            var validationResults = ValidateModel(helpRequest);
            var errorMessages = validationResults.Select(r => r.ErrorMessage).ToList();

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains("Description can't be longer than 500 characters.", errorMessages);
        }

        [Fact]
        public void HelpRequest_LocationTooLong_ShouldFailValidation()
        {
            // Arrange
            var tooLongLocation = new string('L', 201); // 201 characters
            var helpRequest = new HelpRequest
            {
                Title = "Valid Title",
                Description = "Valid Description",
                EmergencyLevel = EmergencyLevel.High,
                CreatedAt = DateTime.UtcNow,
                UserId = 1,
                Location = tooLongLocation,
                Status = HelpRequestStatus.Active
            };

            // Act
            var validationResults = ValidateModel(helpRequest);
            var errorMessages = validationResults.Select(r => r.ErrorMessage).ToList();

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains("Location can't be longer than 200 characters.", errorMessages);
        }

        [Fact]
        public void HelpRequest_InvalidPhone_ShouldFailValidation()
        {
            // Arrange
            var helpRequest = new HelpRequest
            {
                Title = "Test Title",
                Description = "Test Description",
                EmergencyLevel = EmergencyLevel.High,
                CreatedAt = DateTime.UtcNow,
                UserId = 1,
                Location = "Valid Location",
                Status = HelpRequestStatus.Active,
                ContactPhone = "NotAValidPhone" // Fails [Phone] validation
            };

            // Act
            var validationResults = ValidateModel(helpRequest);
            var errorMessages = validationResults.Select(r => r.ErrorMessage).ToList();

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains("Invalid phone number.", errorMessages);
        }

        [Fact]
        public void HelpRequest_InvalidEmail_ShouldFailValidation()
        {
            // Arrange
            var helpRequest = new HelpRequest
            {
                Title = "Test Title",
                Description = "Test Description",
                EmergencyLevel = EmergencyLevel.High,
                CreatedAt = DateTime.UtcNow,
                UserId = 1,
                Location = "Valid Location",
                Status = HelpRequestStatus.Active,
                ContactEmail = "NotAValidEmail" // Fails [EmailAddress] validation
            };

            // Act
            var validationResults = ValidateModel(helpRequest);
            var errorMessages = validationResults.Select(r => r.ErrorMessage).ToList();

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains("Invalid email address.", errorMessages);
        }

        /// <summary>
        /// Helper method to execute Data Annotations validation manually.
        /// </summary>
        private List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, null, null);

            Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);

            return validationResults;
        }
}