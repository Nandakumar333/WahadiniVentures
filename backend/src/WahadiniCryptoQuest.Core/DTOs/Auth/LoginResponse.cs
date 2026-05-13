using WahadiniCryptoQuest.Core.DTOs;

namespace WahadiniCryptoQuest.Core.DTOs.Auth;

/// <summary>
/// Response model for successful user login
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Indicates if the login was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// JWT access token for API authentication
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token for obtaining new access tokens
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Access token expiration time in seconds
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Authenticated user information
    /// </summary>
    public UserDto? User { get; set; }

    /// <summary>
    /// Additional message (e.g., for partial success scenarios)
    /// </summary>
    public string? Message { get; set; }
}