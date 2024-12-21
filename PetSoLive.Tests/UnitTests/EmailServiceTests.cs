using Moq;
using PetSoLive.Business.Services;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Xunit;

namespace PetSoLive.Tests
{
    public class EmailServiceTests
    {
        private readonly Mock<SmtpClient> _smtpClientMock;
        private readonly SmtpSettings _smtpSettings;
        private readonly EmailService _emailService;

        public EmailServiceTests()
        {
            // Initialize mock SMTP client and settings
            _smtpClientMock = new Mock<SmtpClient>();
            _smtpSettings = new SmtpSettings
            {
                Host = "smtp.office365.com",
                Port = 587,
                Username = "210316011@ogr.cbu.edu.tr",
                Password = "TayBay99!",
                FromEmail = "210316011@ogr.cbu.edu.tr",
                EnableSsl = true
            };

            // Create EmailService instance with mocked dependencies
            _emailService = new EmailService(_smtpSettings);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldSendEmailSuccessfully()
        {
            // Arrange
            var to = "furkanbtng@gmail.com";
            var subject = "Test Subject";
            var body = "Test Body";

            // Mocking SendMailAsync method to simulate successful email sending
            _smtpClientMock.Setup(client => client.SendMailAsync(It.IsAny<MailMessage>())).Returns(Task.CompletedTask);

            // Act: Send the email
            await _emailService.SendEmailAsync(to, subject, body);

            // Assert: Verify that SendMailAsync was called once
            _smtpClientMock.Verify(client => client.SendMailAsync(It.IsAny<MailMessage>()), Times.Once);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldThrowException_WhenEmailSendingFails()
        {
            // Arrange
            var to = "furkanbtng@gmail.com";
            var subject = "Test Subject";
            var body = "Test Body";

            // Mocking SendMailAsync to simulate an error when sending email
            _smtpClientMock.Setup(client => client.SendMailAsync(It.IsAny<MailMessage>())).Throws(new SmtpException("SMTP error"));

            // Act & Assert: Expect an exception to be thrown
            await Assert.ThrowsAsync<SmtpException>(() => _emailService.SendEmailAsync(to, subject, body));
        }
    }
}
