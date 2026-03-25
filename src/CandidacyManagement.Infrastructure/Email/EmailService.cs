using System.Net;
using System.Net.Mail;
using CandidacyManagement.Application.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CandidacyManagement.Infrastructure.Email;

/// <summary>
/// שירות שליחת דוא"ל באמצעות SMTP (תואם SendGrid SMTP relay)
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<NotificationDeliveryResult> SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"] ?? "localhost";
            var smtpPort = int.TryParse(_configuration["Email:SmtpPort"], out var port) ? port : 587;
            var smtpUser = _configuration["Email:SmtpUser"] ?? string.Empty;
            var smtpPassword = _configuration["Email:SmtpPassword"] ?? string.Empty;
            var fromAddress = _configuration["Email:FromAddress"] ?? "noreply@candidacy.gov.il";
            var fromName = _configuration["Email:FromName"] ?? "מערכת ניהול מועמדויות";
            var enableSsl = !string.Equals(_configuration["Email:EnableSsl"], "false", StringComparison.OrdinalIgnoreCase);

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (!string.IsNullOrEmpty(smtpUser))
            {
                client.Credentials = new NetworkCredential(smtpUser, smtpPassword);
            }

            var message = new MailMessage
            {
                From = new MailAddress(fromAddress, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(to));

            await client.SendMailAsync(message, cancellationToken);

            _logger.LogInformation("Email sent successfully to {Recipient}", to);
            return new NotificationDeliveryResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}", to);
            return new NotificationDeliveryResult(false, ex.Message);
        }
    }
}
