using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System.Net.Mail;
using System.Threading.Tasks;
using PetSoLive.Business.Services;
using Xunit;

namespace PetSoLive.Tests.UnitTests
{
public class EmailServiceTests
{
    private readonly Mock<IEmailSender> _mockEmailSender;
    private readonly EmailService _emailService;
    private readonly SmtpSettings _smtpSettings;

    public EmailServiceTests()
    {
        _mockEmailSender = new Mock<IEmailSender>();
        _smtpSettings = new SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 587,
            Username = "testuser",
            Password = "testpassword",
            FromEmail = "no-reply@example.com",
            EnableSsl = true
        };

        // Correctly pass the mock of IEmailSender and the SmtpSettings to the constructor
        _emailService = new EmailService(_smtpSettings);
    }

    [Fact]
    public async Task SendEmailAsync_ShouldSendEmail_WhenValidParameters()
    {
        // Arrange
        var to = "recipient@example.com";
        var subject = "Test Subject";
        var body = "Test Body";

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_smtpSettings.FromEmail),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(to);

        // Act
        await _emailService.SendEmailAsync(to, subject, body);

        // Assert
        _mockEmailSender.Verify(m => m.SendAsync(It.Is<MailMessage>(mm => mm.Subject == subject && mm.Body == body)), Times.Once);
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
        var to = "recipient@example.com";
        var body = "Test Body";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _emailService.SendEmailAsync(to, null, body));
    }

    [Fact]
    public async Task SendEmailAsync_ShouldThrowArgumentNullException_WhenBodyIsNull()
    {
        // Arrange
        var to = "recipient@example.com";
        var subject = "Test Subject";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _emailService.SendEmailAsync(to, subject, null));
    }
}

    
    public interface IEmailSender
    {
        Task SendAsync(MailMessage mailMessage);
    }
}
