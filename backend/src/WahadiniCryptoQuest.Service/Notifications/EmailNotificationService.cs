using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Admin;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Notifications;

/// <summary>
/// Service for sending email notifications using email templates.
/// Handles task review notifications and administrative action alerts.
/// T013: EmailNotificationService with MailKit and task review templates
/// </summary>
public class EmailNotificationService
{
    private readonly IEmailService _emailService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly IConfiguration _configuration;

    public EmailNotificationService(
        IEmailService emailService,
        IUserRepository userRepository,
        ILogger<EmailNotificationService> logger,
        IConfiguration configuration)
    {
        _emailService = emailService;
        _userRepository = userRepository;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Sends an email notification using a template.
    /// </summary>
    public async Task<bool> SendEmailAsync(
        Guid userId,
        string subject,
        string templateName,
        Dictionary<string, string> templateData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get user details
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Attempted to send email to non-existent user {UserId}", userId);
                return false;
            }

            // Generate email body from template
            var emailBody = GenerateEmailBody(templateName, templateData);

            // Send email using the existing email service
            var sent = await _emailService.SendEmailAsync(user.Email, subject, emailBody);

            if (sent)
            {
                _logger.LogInformation("Email sent successfully to {Email} using template {Template}",
                    user.Email, templateName);
            }
            else
            {
                _logger.LogWarning("Failed to send email to {Email} using template {Template}",
                    user.Email, templateName);
            }

            return sent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to user {UserId} with template {Template}",
                userId, templateName);
            return false;
        }
    }

    /// <summary>
    /// Generates email body HTML from template name and data.
    /// </summary>
    private string GenerateEmailBody(string templateName, Dictionary<string, string> templateData)
    {
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";

        return templateName switch
        {
            "TaskApproved" => GenerateTaskApprovedTemplate(templateData, frontendUrl),
            "TaskRejected" => GenerateTaskRejectedTemplate(templateData, frontendUrl),
            "AccountBanned" => GenerateAccountBannedTemplate(templateData, frontendUrl),
            "AccountUnbanned" => GenerateAccountUnbannedTemplate(templateData, frontendUrl),
            "PointAdjustment" => GeneratePointAdjustmentTemplate(templateData, frontendUrl),
            _ => GenerateGenericTemplate(templateData, frontendUrl)
        };
    }

    private string GenerateTaskApprovedTemplate(Dictionary<string, string> data, string frontendUrl)
    {
        var userName = data.GetValueOrDefault("UserName", "User");
        var taskTitle = data.GetValueOrDefault("TaskTitle", "Task");
        var pointsAwarded = data.GetValueOrDefault("PointsAwarded", "0");

        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #4CAF50;'>Task Approved! 🎉</h2>
                    <p>Hi {System.Net.WebUtility.HtmlEncode(userName)},</p>
                    <p>Great news! Your submission for <strong>{System.Net.WebUtility.HtmlEncode(taskTitle)}</strong> has been approved.</p>
                    <div style='background-color: #f0f8ff; padding: 15px; border-left: 4px solid #4CAF50; margin: 20px 0;'>
                        <p style='margin: 0;'><strong>Points Awarded:</strong> {pointsAwarded}</p>
                    </div>
                    <p>Keep up the excellent work! Continue your learning journey with more tasks and challenges.</p>
                    <p style='margin-top: 30px;'>
                        <a href='{frontendUrl}/dashboard' 
                           style='background-color: #4CAF50; color: white; padding: 12px 24px; 
                                  text-decoration: none; display: inline-block; border-radius: 4px;'>
                            View Dashboard
                        </a>
                    </p>
                    <p style='margin-top: 30px; color: #666; font-size: 14px;'>
                        Best regards,<br>
                        The WahadiniCryptoQuest Team
                    </p>
                </div>
            </body>
            </html>";
    }

    private string GenerateTaskRejectedTemplate(Dictionary<string, string> data, string frontendUrl)
    {
        var userName = data.GetValueOrDefault("UserName", "User");
        var taskTitle = data.GetValueOrDefault("TaskTitle", "Task");
        var feedback = data.GetValueOrDefault("Feedback", "Please review the requirements and try again.");

        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #FF9800;'>Task Needs Revision</h2>
                    <p>Hi {System.Net.WebUtility.HtmlEncode(userName)},</p>
                    <p>Your submission for <strong>{System.Net.WebUtility.HtmlEncode(taskTitle)}</strong> requires some improvements before it can be approved.</p>
                    <div style='background-color: #fff3e0; padding: 15px; border-left: 4px solid #FF9800; margin: 20px 0;'>
                        <p style='margin: 0;'><strong>Feedback:</strong></p>
                        <p style='margin: 10px 0 0 0;'>{System.Net.WebUtility.HtmlEncode(feedback)}</p>
                    </div>
                    <p>Don't worry! Use this feedback to improve your submission and try again.</p>
                    <p style='margin-top: 30px;'>
                        <a href='{frontendUrl}/tasks' 
                           style='background-color: #FF9800; color: white; padding: 12px 24px; 
                                  text-decoration: none; display: inline-block; border-radius: 4px;'>
                            Resubmit Task
                        </a>
                    </p>
                    <p style='margin-top: 30px; color: #666; font-size: 14px;'>
                        Best regards,<br>
                        The WahadiniCryptoQuest Team
                    </p>
                </div>
            </body>
            </html>";
    }

    private string GenerateAccountBannedTemplate(Dictionary<string, string> data, string frontendUrl)
    {
        var userName = data.GetValueOrDefault("UserName", "User");
        var reason = data.GetValueOrDefault("Reason", "Terms of Service violation");

        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #f44336;'>Account Suspended</h2>
                    <p>Hi {System.Net.WebUtility.HtmlEncode(userName)},</p>
                    <p>Your WahadiniCryptoQuest account has been suspended due to the following reason:</p>
                    <div style='background-color: #ffebee; padding: 15px; border-left: 4px solid #f44336; margin: 20px 0;'>
                        <p style='margin: 0;'><strong>Reason:</strong> {System.Net.WebUtility.HtmlEncode(reason)}</p>
                    </div>
                    <p>If you believe this is a mistake or would like to appeal, please contact support@wahadini.com.</p>
                    <p style='margin-top: 30px; color: #666; font-size: 14px;'>
                        WahadiniCryptoQuest Team
                    </p>
                </div>
            </body>
            </html>";
    }

    private string GenerateAccountUnbannedTemplate(Dictionary<string, string> data, string frontendUrl)
    {
        var userName = data.GetValueOrDefault("UserName", "User");

        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #4CAF50;'>Account Reinstated</h2>
                    <p>Hi {System.Net.WebUtility.HtmlEncode(userName)},</p>
                    <p>Good news! Your WahadiniCryptoQuest account has been reinstated and you can now access all platform features.</p>
                    <p style='margin-top: 30px;'>
                        <a href='{frontendUrl}/dashboard' 
                           style='background-color: #4CAF50; color: white; padding: 12px 24px; 
                                  text-decoration: none; display: inline-block; border-radius: 4px;'>
                            Access Your Account
                        </a>
                    </p>
                    <p style='margin-top: 30px; color: #666; font-size: 14px;'>
                        Best regards,<br>
                        The WahadiniCryptoQuest Team
                    </p>
                </div>
            </body>
            </html>";
    }

    private string GeneratePointAdjustmentTemplate(Dictionary<string, string> data, string frontendUrl)
    {
        var userName = data.GetValueOrDefault("UserName", "User");
        var adjustment = data.GetValueOrDefault("Adjustment", "0");
        var reason = data.GetValueOrDefault("Reason", "Manual adjustment");
        var newBalance = data.GetValueOrDefault("NewBalance", "0");

        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #2196F3;'>Points Balance Adjusted</h2>
                    <p>Hi {System.Net.WebUtility.HtmlEncode(userName)},</p>
                    <p>Your points balance has been adjusted by an administrator.</p>
                    <div style='background-color: #e3f2fd; padding: 15px; border-left: 4px solid #2196F3; margin: 20px 0;'>
                        <p style='margin: 0;'><strong>Adjustment:</strong> {adjustment} points</p>
                        <p style='margin: 10px 0 0 0;'><strong>New Balance:</strong> {newBalance} points</p>
                        <p style='margin: 10px 0 0 0;'><strong>Reason:</strong> {System.Net.WebUtility.HtmlEncode(reason)}</p>
                    </div>
                    <p style='margin-top: 30px;'>
                        <a href='{frontendUrl}/rewards' 
                           style='background-color: #2196F3; color: white; padding: 12px 24px; 
                                  text-decoration: none; display: inline-block; border-radius: 4px;'>
                            View Rewards
                        </a>
                    </p>
                    <p style='margin-top: 30px; color: #666; font-size: 14px;'>
                        Best regards,<br>
                        The WahadiniCryptoQuest Team
                    </p>
                </div>
            </body>
            </html>";
    }

    private string GenerateGenericTemplate(Dictionary<string, string> data, string frontendUrl)
    {
        var userName = data.GetValueOrDefault("UserName", "User");
        var message = data.GetValueOrDefault("Message", "You have a new notification.");

        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2>Notification</h2>
                    <p>Hi {System.Net.WebUtility.HtmlEncode(userName)},</p>
                    <p>{System.Net.WebUtility.HtmlEncode(message)}</p>
                    <p style='margin-top: 30px;'>
                        <a href='{frontendUrl}/dashboard' 
                           style='background-color: #2196F3; color: white; padding: 12px 24px; 
                                  text-decoration: none; display: inline-block; border-radius: 4px;'>
                            Go to Dashboard
                        </a>
                    </p>
                    <p style='margin-top: 30px; color: #666; font-size: 14px;'>
                        Best regards,<br>
                        The WahadiniCryptoQuest Team
                    </p>
                </div>
            </body>
            </html>";
    }
}
