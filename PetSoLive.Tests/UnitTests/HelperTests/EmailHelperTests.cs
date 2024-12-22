using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using Xunit;

public class EmailHelperTests
{
    private readonly EmailHelper _emailHelper;

    public EmailHelperTests()
    {
        _emailHelper = new EmailHelper();
    }

    [Fact]
    public void GenerateAdoptionRequestEmailBody_ShouldGenerateCorrectEmail()
    {
        // Arrange
        var user = new User { Username = "JohnDoe", Email = "johndoe@example.com" };
        var pet = new Pet { Name = "Buddy", Species = "Dog", Breed = "Labrador" };
        var adoptionRequest = new AdoptionRequest { Message = "I would love to adopt Buddy!", Status = AdoptionStatus.Pending };

        // Act
        var emailBody = _emailHelper.GenerateAdoptionRequestEmailBody(user, pet, adoptionRequest);

        // Assert
        Assert.Contains(user.Username, emailBody);    // Check that username is included
        Assert.Contains(pet.Name, emailBody);         // Check that pet's name is included
        Assert.Contains(adoptionRequest.Message, emailBody);  // Check that the adoption request message is included
        Assert.Contains("<link rel='stylesheet' type='text/css' href='https://yourdomain.com/path/to/email.css'>", emailBody);  // Check for CSS link
        Assert.Contains("<strong>Status:</strong> Pending", emailBody);  // Check adoption status
    }

    [Fact]
    public void GenerateAdoptionRequestConfirmationEmailBody_ShouldGenerateCorrectEmail()
    {
        // Arrange
        var user = new User { Username = "JaneDoe", Email = "janedoe@example.com" };
        var pet = new Pet { Name = "Max", Species = "Cat", Breed = "Persian" };

        // Act
        var emailBody = _emailHelper.GenerateAdoptionRequestConfirmationEmailBody(user, pet);

        // Assert
        Assert.Contains("Thank you for your adoption request", emailBody);  // Check confirmation message
        Assert.Contains(user.Username, emailBody);  // Check user name is in the email
        Assert.Contains(pet.Name, emailBody);  // Check pet name is in the email
    }

    [Fact]
    public void GenerateRejectionEmailBody_ShouldGenerateCorrectEmail()
    {
        // Arrange
        var user = new User { Username = "MikeSmith", Email = "mikesmith@example.com" };
        var pet = new Pet { Name = "Charlie", Species = "Dog", Breed = "Beagle" };

        // Act
        var emailBody = _emailHelper.GenerateRejectionEmailBody(user, pet);

        // Assert
        Assert.Contains("Your Adoption Request for Charlie Has Been Rejected", emailBody);  // Check rejection message
        Assert.Contains(user.Username, emailBody);  // Check user name is in the email
        Assert.Contains(pet.Name, emailBody);  // Check pet name is in the email
    }

    // Other email body generation methods can also have similar tests
}
