using System.Net;
using System.Net.Mail;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;

public interface ISmtpClient : IDisposable
{
    Task SendMailAsync(MailMessage message);
}

public class SmtpClientWrapper : ISmtpClient
{
    private readonly SmtpClient _client;
    public SmtpClientWrapper(SmtpClient client) => _client = client;
    public Task SendMailAsync(MailMessage message) => _client.SendMailAsync(message);
    public void Dispose() => _client.Dispose();
}

namespace PetSoLive.Business.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;
        private readonly ISmtpClient _smtpClient;

        public EmailService(SmtpSettings settings, ISmtpClient smtpClient)
        {
            _settings = settings;
            _smtpClient = smtpClient;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrEmpty(to)) throw new ArgumentNullException(nameof(to));
            if (string.IsNullOrEmpty(subject)) throw new ArgumentNullException(nameof(subject));
            if (string.IsNullOrEmpty(body)) throw new ArgumentNullException(nameof(body));

            var mail = new MailMessage(_settings.FromEmail, to, subject, body);
            mail.IsBodyHtml = true;
            await _smtpClient.SendMailAsync(mail);
        }
    }
}