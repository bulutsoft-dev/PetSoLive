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
    public class EmailServiceIntegrationTests
    {
        private readonly Mock<SmtpClient> _smtpClientMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly SmtpSettings _smtpSettings;

        public EmailServiceIntegrationTests()
        {
            _smtpClientMock = new Mock<SmtpClient>();
            _smtpSettings = new SmtpSettings
            {
                Host = "smtp.office365.com",
                Port = 587,
                Username = "your@domain.com",
                Password = "your-password",
                FromEmail = "your-email@domain.com",
                EnableSsl = true
            };

            _emailServiceMock = new Mock<IEmailService>();
        }

        [Fact]
        public async Task SendEmailAsync_ShouldSendEmailSuccessfully()
        {
            // Arrange: EmailService'ı test etmek için Mock SMTP client kullanıyoruz
            var emailService = new EmailService(_smtpSettings);

            var to = "furkanbtng@gmail.com";
            var subject = "Test Subject";
            var body = "Test Body";

            // Mocking SendMailAsync method to simulate successful email sending
            _smtpClientMock.Setup(client => client.SendMailAsync(It.IsAny<MailMessage>())).Returns(Task.CompletedTask);

            // Act: E-posta gönderme işlemi
            await emailService.SendEmailAsync(to, subject, body);

            // Assert: SMTP client'ın SendMailAsync metodunun çağrıldığını doğruluyoruz
            _smtpClientMock.Verify(client => client.SendMailAsync(It.IsAny<MailMessage>()), Times.Once);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldThrowException_WhenEmailSendingFails()
        {
            // Arrange: EmailService'ı test etmek için Mock SMTP client kullanıyoruz
            var emailService = new EmailService(_smtpSettings);

            var to = "furkanbtng@gmail.com";
            var subject = "Test Subject";
            var body = "Test Body";

            // Mocking SendMailAsync to simulate an error when sending email
            _smtpClientMock.Setup(client => client.SendMailAsync(It.IsAny<MailMessage>())).Throws(new SmtpException("SMTP error"));

            // Act & Assert: E-posta gönderme sırasında hata fırlatılmasını bekliyoruz
            await Assert.ThrowsAsync<SmtpException>(() => emailService.SendEmailAsync(to, subject, body));
        }
    }
}
