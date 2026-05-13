using System.ComponentModel.DataAnnotations;

namespace WahadiniCryptoQuest.Core.DTOs.Auth;

/// <summary>
/// Request model for initiating password reset process
/// Contains email address for password reset
/// </summary>
public class PasswordResetRequest
{
    /// <summary>
    /// Email address of the user requesting password reset
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
    public string Email { get; set; } = string.Empty;

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