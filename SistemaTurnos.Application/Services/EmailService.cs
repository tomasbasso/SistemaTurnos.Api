using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using SistemaTurnos.Application.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace SistemaTurnos.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try 
            {
                var email = new MimeMessage();
                var fromAddress = _configuration["EmailSettings:FromAddress"];
                var fromName = _configuration["EmailSettings:FromName"] ?? "Sistema de Turnos";

                // If FromAddress is not set, skip sending emails (useful for test environments)
                if (string.IsNullOrWhiteSpace(fromAddress))
                {
                    _logger.LogWarning("Email not sent: EmailSettings:FromAddress not configured.");
                    return;
                }

                email.From.Add(new MailboxAddress(fromName, fromAddress));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = body
                };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                var host = _configuration["EmailSettings:Host"];
                var port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
                var user = _configuration["EmailSettings:Username"];
                var pass = _configuration["EmailSettings:Password"];

                // Removed insecure certificate validation for Production safety.
                // smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                
                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
                {
                    await smtp.AuthenticateAsync(user, pass);
                }

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email a {To}: {Message}", to, ex.Message);
            }
        }
    }
}
