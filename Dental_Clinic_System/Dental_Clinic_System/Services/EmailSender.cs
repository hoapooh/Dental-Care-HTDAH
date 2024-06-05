using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dental_Clinic_System.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var smtpClient = new SmtpClient(_configuration["Email:Smtp:Host"])
                {
                    Port = int.Parse(_configuration["Email:Smtp:Port"]),
                    Credentials = new NetworkCredential(_configuration["Email:Smtp:Username"], _configuration["Email:Smtp:Password"]),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["Email:FromAddress"], _configuration["Email:FromName"]),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(email);

                // Optional: Add additional headers to improve deliverability
                mailMessage.Headers.Add("X-Priority", "1");
                mailMessage.Headers.Add("X-MSMail-Priority", "High");
                mailMessage.Headers.Add("Importance", "High");

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent to {email} with subject {subject}");
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, $"Error sending email to {email} with subject {subject}");
                throw; // Re-throw the exception if you want the caller to handle it
            }
        }
    }
}
