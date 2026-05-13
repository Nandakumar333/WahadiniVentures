using System.ComponentModel.DataAnnotations;

namespace WahadiniCryptoQuest.Core.DTOs.Auth;

/// <summary>
/// Request model for token refresh operation
/// Contains the refresh token to be used for generating new access token
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// The refresh token string to exchange for a new access token
    /// </summary>
    [Required(ErrorMessage = "Refresh token is required")]
    [StringLength(1000, ErrorMessage = "Refresh token is too long")]
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Optional device information for tracking
    /// </summary>
    [StringLength(500, ErrorMessage = "Device info is too long")]
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// Optional IP address for security tracking
    /// </summary>
    [StringLength(45, ErrorMessage = "IP address is too long")] // IPv6 max length
    public string? IpAddress { get; set; }
}