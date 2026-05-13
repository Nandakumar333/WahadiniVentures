namespace WahadiniCryptoQuest.Core.Interfaces;

/// <summary>
/// Interface for email operations
/// Follows Clean Architecture - defined in Core layer for dependency inversion
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends email verification message to user with user ID and token
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="firstName">User's first name for personalization</param>
    /// <param name="userId">User ID for confirmation link</param>
    /// <param name="token">Email verification token</param>
    Task<bool> SendEmailVerificationAsync(string email, string firstName, Guid userId, string token);

    /// <summary>
    /// Sends password reset email to user
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="userName">User's name for personalization</param>
    /// <param name="resetToken">Password reset token</param>
    /// <param name="expiresAt">When the token expires</param>
    Task<bool> SendPasswordResetEmailAsync(string email, string userName, string resetToken, DateTime expiresAt);

    /// <summary>
    /// Sends welcome email to newly registered user
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="firstName">User's first name</param>
    Task<bool> SendWelcomeEmailAsync(string email, string firstName);

    /// <summary>
    /// Sends general notification email
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (HTML)</param>
    Task<bool> SendEmailAsync(string email, string subject, string body);
}