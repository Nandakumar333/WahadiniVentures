using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace WahadiniCryptoQuest.DAL.Services;

/// <summary>
/// Email service implementation using SMTP
/// Production implementation should use SendGrid, AWS SES, or similar service
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Sends email verification message to user with user ID and token
    /// </summary>
    public async Task<bool> SendEmailVerificationAsync(string email, string firstName, Guid userId, string token)
    {
        // Validate token is not null or empty
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("SendEmailVerificationAsync called with null or empty token for email: {Email}", email);
            return false;
        }

        var subject = "Confirm your WahadiniCryptoQuest account";
        var confirmationLink = $"{GetFrontendUrl()}/confirm-email?userId={userId}&token={Uri.EscapeDataString(token)}";
        
        var body = $@"
            <html>
            <body>
                <h2>Welcome to WahadiniCryptoQuest, {firstName}!</h2>
                <p>Thank you for registering. Please confirm your email address by clicking the link below:</p>
                <p><a href='{confirmationLink}' style='background-color: #4CAF50; color: white; padding: 14px 25px; text-decoration: none; display: inline-block; border-radius: 4px;'>Confirm Email Address</a></p>
                <p>Or copy and paste this link in your browser:</p>
                <p style='word-break: break-all;'>{confirmationLink}</p>
                <p>This verification link will expire in 24 hours for security reasons.</p>
                <p>If you didn't create this account, please ignore this email.</p>
                <p>Best regards,<br>The WahadiniCryptoQuest Team</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    /// <summary>
    /// Sends password reset email to user
    /// </summary>
    public async Task<bool> SendPasswordResetEmailAsync(string email, string userName, string resetToken, DateTime expiresAt)
    {
        var subject = "Reset your WahadiniCryptoQuest password";
        var resetLink = $"{GetFrontendUrl()}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(resetToken)}";
        var expiresInHours = Math.Round((expiresAt - DateTime.UtcNow).TotalHours, 1);
        
        var body = $@"
            <html>
            <body>
                <h2>Password Reset Request</h2>
                <p>Hi {System.Net.WebUtility.HtmlEncode(userName)},</p>
                <p>You have requested to reset your password. Click the link below to set a new password:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>This link will expire in {expiresInHours} hours for security reasons.</p>
                <p>If you didn't request this reset, please ignore this email.</p>
                <p>Best regards,<br>The WahadiniCryptoQuest Team</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    /// <summary>
    /// Sends welcome email to newly registered user
    /// </summary>
    public async Task<bool> SendWelcomeEmailAsync(string email, string firstName)
    {
        var subject = "Welcome to WahadiniCryptoQuest!";
        
        var body = $@"
            <html>
            <body>
                <h2>Welcome to WahadiniCryptoQuest, {firstName}!</h2>
                <p>Your account has been successfully created and verified.</p>
                <p>Get ready to embark on your cryptocurrency learning journey!</p>
                <p>Start exploring our interactive lessons and challenges.</p>
                <p><a href='{GetFrontendUrl()}/dashboard'>Go to Dashboard</a></p>
                <p>Best regards,<br>The WahadiniCryptoQuest Team</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    /// <summary>
    /// Sends general email using MailKit SMTP client
    /// </summary>
    public async Task<bool> SendEmailAsync(string email, string subject, string body)
    {
        try
        {
            // For development, just log the email instead of actually sending
            if (IsDevelopmentEnvironment())
            {
                _logger.LogInformation("Email would be sent to {Email} with subject: {Subject}", email, subject);
                _logger.LogInformation("Email body: {Body}", body);
                return true;
            }

            // Create the email message
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _configuration["Email:FromName"] ?? "WahadiniCryptoQuest",
                _configuration["Email:FromAddress"] ?? "noreply@wahadinicryptoquest.com"));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();

            // Send the email using MailKit SMTP client
            using var client = new SmtpClient();
            
            // Get SMTP configuration
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.TryParse(_configuration["Email:SmtpPort"], out var port) ? port : 587;
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPass = _configuration["Email:SmtpPass"];

            if (string.IsNullOrWhiteSpace(smtpHost))
            {
                _logger.LogWarning("SMTP configuration missing. Email not sent to {Email}", email);
                return false;
            }

            // Connect to SMTP server
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);

            // Authenticate if credentials are provided
            if (!string.IsNullOrWhiteSpace(smtpUser) && !string.IsNullOrWhiteSpace(smtpPass))
            {
                await client.AuthenticateAsync(smtpUser, smtpPass);
            }

            // Send the message
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", email);
            return false;
        }
    }

    private string GetFrontendUrl()
    {
        return _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
    }

    private bool IsDevelopmentEnvironment()
    {
        var environment = _configuration["ASPNETCORE_ENVIRONMENT"];
        return string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);
    }
}