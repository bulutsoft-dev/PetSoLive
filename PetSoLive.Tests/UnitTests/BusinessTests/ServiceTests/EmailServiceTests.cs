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
        private readonly SmtpSettings _smtpSettings;
        private readonly Mock<ISmtpClient> _smtpClientMock;
        private readonly EmailService _emailService;

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
            _smtpClientMock = new Mock<ISmtpClient>();
            _emailService = new EmailService(_smtpSettings, _smtpClientMock.Object);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldSendEmail_WhenValidParameters()
        {
            // Arrange
            var to = "furkanbtng@gmail.com";
            var subject = "Test Subject";
            var body = "Test Body";
            _smtpClientMock.Setup(x => x.SendMailAsync(It.IsAny<MailMessage>())).Returns(Task.CompletedTask);

            // Act
            await _emailService.SendEmailAsync(to, subject, body);

            // Assert
            _smtpClientMock.Verify(x => x.SendMailAsync(It.Is<MailMessage>(m => m.To[0].Address == to && m.Subject == subject && m.Body == body)), Times.Once);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldThrowArgumentNullException_WhenToIsNull()
        {
            var subject = "Test Subject";
            var body = "Test Body";
            await Assert.ThrowsAsync<ArgumentNullException>(() => _emailService.SendEmailAsync(null, subject, body));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldThrowArgumentNullException_WhenSubjectIsNull()
        {
            var to = "furkanbtng@gmail.com";
            var body = "Test Body";
            await Assert.ThrowsAsync<ArgumentNullException>(() => _emailService.SendEmailAsync(to, null, body));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldThrowArgumentNullException_WhenBodyIsNull()
        {
            var to = "furkanbtng@gmail.com";
            var subject = "Test Subject";
            await Assert.ThrowsAsync<ArgumentNullException>(() => _emailService.SendEmailAsync(to, subject, null));
        }
    }
}
