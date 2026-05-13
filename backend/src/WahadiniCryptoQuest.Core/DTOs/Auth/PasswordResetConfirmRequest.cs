using System.ComponentModel.DataAnnotations;

namespace WahadiniCryptoQuest.Core.DTOs.Auth;

/// <summary>
/// Request model for confirming password reset with new password
/// Contains reset token and new password
/// </summary>
public class PasswordResetConfirmRequest
{
    /// <summary>
    /// Password reset token received via email
    /// </summary>
    [Required(ErrorMessage = "Reset token is required")]
    [StringLength(500, ErrorMessage = "Reset token format is invalid")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// New password for the user account
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", 
        ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, one number and one special character")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Confirmation of the new password
    /// </summary>
    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare("NewPassword", ErrorMessage = "Password and confirmation do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Client information for tracking and security purposes
    /// </summary>
    [StringLength(500, ErrorMessage = "Client info cannot exceed 500 characters")]
    public string? ClientInfo { get; set; }

    /// <summary>
    /// IP address of the client making the request
    /// </summary>
    [StringLength(45, ErrorMessage = "IP address cannot exceed 45 characters")]
    public string? IpAddress { get; set; }
}