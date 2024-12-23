using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Business.Services;
using System.Net.Mail;
using System.Threading.Tasks;
using Xunit;

namespace PetSoLive.Tests.UnitTests
{
    public class EmailServiceTests
    {
        private readonly EmailService _emailService;
        private readonly SmtpSettings _smtpSettings;

        public EmailServiceTests()
        {
            _smtpSettings = new SmtpSettings
            {
                Host = "smtp.gmail.com",
                Port = 587,
                Username = "furkanbtng@gmail.com",
                Password = "lflq tzfv emph uzop",
                FromEmail = "furkanbtng@gmail.com",
                EnableSsl = true
            };

            _emailService = new EmailService(_smtpSettings);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldSendEmail_WhenValidParameters()
        {
            // Arrange
            var to = "furkanbtng@gmail.com";
            var subject = "Test Subject";
            var body = "Test Body";

            // Act & Assert
            await _emailService.SendEmailAsync(to, subject, body);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldThrowArgumentNullException_WhenToIsNull()
        {
            // Arrange
            var subject = "Test Subject";
            var body = "Test Body";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _emailService.SendEmailAsync(null, subject, body));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldThrowArgumentNullException_WhenSubjectIsNull()
        {
            // Arrange
            var to = "furkanbtng@gmail.com";
            var body = "Test Body";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _emailService.SendEmailAsync(to, null, body));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldThrowArgumentNullException_WhenBodyIsNull()
        {
            // Arrange
            var to = "furkanbtng@gmail.com";
            var subject = "Test Subject";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _emailService.SendEmailAsync(to, subject, null));
        }
    }
}
