using WahadiniCryptoQuest.Core.DTOs;

namespace WahadiniCryptoQuest.Core.DTOs.Auth;

/// <summary>
/// Data transfer object for authentication response containing tokens and user data
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// JWT access token for API authentication
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token for obtaining new access tokens
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// When the access token expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Authenticated user information
    /// </summary>
    public UserDto User { get; set; } = null!;
}
