using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SistemaTurnos.Application.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace SistemaTurnos.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try 
            {
                var email = new MimeMessage();
                var fromAddress = _configuration["EmailSettings:FromAddress"];
                var fromName = _configuration["EmailSettings:FromName"] ?? "Sistema de Turnos";

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

                // For demo purposes, we accept any certificate. In PROD remove this line.
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

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
                // Log exception (or rethrow if critical)
                // For now we just print to console to avoid breaking the flow if email fails
                Console.WriteLine($"[Error al enviar email]: {ex.Message}");
            }
        }
    }
}
