using System.ComponentModel.DataAnnotations;

namespace WahadiniCryptoQuest.Core.DTOs.Auth;

/// <summary>
/// Data Transfer Object for email confirmation requests
/// Used when user clicks on email verification link
/// </summary>
public class EmailConfirmationDto
{
    /// <summary>
    /// User ID from the verification link
    /// Required for identifying which user to confirm
    /// </summary>
    [Required(ErrorMessage = "User ID is required")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Email verification token from the verification link
    /// Must be the exact token sent in the verification email
    /// </summary>
    [Required(ErrorMessage = "Verification token is required")]
    [StringLength(500, ErrorMessage = "Token cannot exceed 500 characters")]
    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// Data Transfer Object for resending email confirmation
/// Used when user requests a new verification email
/// </summary>
public class ResendEmailConfirmationDto
{
    /// <summary>
    /// User's email address to resend confirmation to
    /// Must be a valid, registered email address
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(320, ErrorMessage = "Email address cannot exceed 320 characters")]
    public string Email { get; set; } = string.Empty;
}