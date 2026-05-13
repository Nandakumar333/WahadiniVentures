namespace WahadiniCryptoQuest.Core.DTOs.Auth;

/// <summary>
/// Response model for password reset operations
/// Provides status and user feedback for password reset requests
/// </summary>
public class PasswordResetResponse
{
    /// <summary>
    /// Indicates if the password reset operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// User-friendly message about the operation result
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Indicates if an email was sent (for reset requests)
    /// </summary>
    public bool EmailSent { get; set; }

    /// <summary>
    /// Time when the reset token expires (for security info)
    /// </summary>
    public DateTime? TokenExpiresAt { get; set; }

    /// <summary>
    /// Creates a successful password reset request response
    /// </summary>
    /// <param name="message">Success message</param>
    /// <param name="emailSent">Whether email was sent</param>
    /// <param name="tokenExpiresAt">When the token expires</param>
    /// <returns>Successful response</returns>
    public static PasswordResetResponse CreateRequestSuccess(string message, bool emailSent = true, DateTime? tokenExpiresAt = null)
    {
        return new PasswordResetResponse
        {
            Success = true,
            Message = message,
            EmailSent = emailSent,
            TokenExpiresAt = tokenExpiresAt
        };
    }

    /// <summary>
    /// Creates a successful password reset confirmation response
    /// </summary>
    /// <param name="message">Success message</param>
    /// <returns>Successful response</returns>
    public static PasswordResetResponse CreateConfirmSuccess(string message)
    {
        return new PasswordResetResponse
        {
            Success = true,
            Message = message,
            EmailSent = false
        };
    }

    /// <summary>
    /// Creates a failed password reset response
    /// </summary>
    /// <param name="errorMessage">Error description</param>
    /// <returns>Failed response</returns>
    public static PasswordResetResponse CreateFailure(string errorMessage)
    {
        return new PasswordResetResponse
        {
            Success = false,
            ErrorMessage = errorMessage,
            EmailSent = false
        };
    }
}