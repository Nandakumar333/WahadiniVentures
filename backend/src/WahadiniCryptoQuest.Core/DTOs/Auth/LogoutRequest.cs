using System.ComponentModel.DataAnnotations;

namespace WahadiniCryptoQuest.Core.DTOs.Auth;

/// <summary>
/// Request model for user logout operation
/// Contains the refresh token to be revoked
/// </summary>
public class LogoutRequest
{
    /// <summary>
    /// The refresh token to revoke during logout
    /// </summary>
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}
