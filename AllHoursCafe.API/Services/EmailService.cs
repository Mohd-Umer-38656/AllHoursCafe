using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;

namespace AllHoursCafe.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                // Log email details for debugging
                _logger.LogInformation($"Preparing to send email to: {to}");
                _logger.LogInformation($"Subject: {subject}");

                var emailConfig = _configuration.GetSection("EmailSettings");
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(emailConfig["SenderName"], emailConfig["SenderEmail"]));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                {
                    bodyBuilder.HtmlBody = body;
                }
                else
                {
                    bodyBuilder.TextBody = body;
                }

                message.Body = bodyBuilder.ToMessageBody();

                // Check if we have SMTP settings configured
                if (string.IsNullOrEmpty(emailConfig["SmtpServer"]) ||
                    string.IsNullOrEmpty(emailConfig["Username"]) ||
                    string.IsNullOrEmpty(emailConfig["Password"]))
                {
                    // Fall back to logging if SMTP is not configured
                    _logger.LogWarning("SMTP settings not fully configured. Email not sent. Logging instead.");
                    _logger.LogInformation($"Email would have been sent to: {to}");
                    _logger.LogInformation($"Subject: {subject}");
                    _logger.LogInformation($"Body: {body}");
                    return;
                }

                using (var client = new SmtpClient())
                {
                    try
                    {
                        // Connect to SMTP server
                        _logger.LogInformation($"Connecting to SMTP server: {emailConfig["SmtpServer"]}");
                        await client.ConnectAsync(
                            emailConfig["SmtpServer"],
                            int.Parse(emailConfig["Port"]),
                            MailKit.Security.SecureSocketOptions.StartTls);

                        // Gmail requires disabling OAuth2 for app passwords
                        client.AuthenticationMechanisms.Remove("XOAUTH2");

                        // Authenticate
                        _logger.LogInformation("Authenticating with SMTP server");
                        await client.AuthenticateAsync(emailConfig["Username"], emailConfig["Password"]);

                        // Send email
                        _logger.LogInformation("Sending email");
                        await client.SendAsync(message);

                        // Disconnect
                        await client.DisconnectAsync(true);
                        _logger.LogInformation("Email sent successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during SMTP operations");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {EmailAddress}", to);
                throw;
            }
        }
    }
}
