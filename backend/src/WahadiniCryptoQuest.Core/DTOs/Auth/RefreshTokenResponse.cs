namespace WahadiniCryptoQuest.Core.DTOs.Auth;

/// <summary>
/// Response model for token refresh operation
/// Contains the new tokens and expiration information
/// </summary>
public class RefreshTokenResponse
{
    /// <summary>
    /// Indicates if the token refresh operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// New access token (JWT) for API authentication
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// New refresh token for future refresh operations
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Access token expiration time in seconds from now
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Exact expiration date and time (UTC) for the access token
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Token type (typically "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Error message if Success is false
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Creates a successful refresh token response
    /// </summary>
    /// <param name="accessToken">New access token</param>
    /// <param name="refreshToken">New refresh token</param>
    /// <param name="expiresAt">Expiration time</param>
    /// <returns>Successful response</returns>
    public static RefreshTokenResponse CreateSuccess(string accessToken, string refreshToken, DateTime expiresAt)
    {
        var expiresIn = (int)(expiresAt - DateTime.UtcNow).TotalSeconds;
        
        return new RefreshTokenResponse
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiresIn,
            ExpiresAt = expiresAt,
            TokenType = "Bearer"
        };
    }

    /// <summary>
    /// Creates a failed refresh token response
    /// </summary>
    /// <param name="errorMessage">Error description</param>
    /// <returns>Failed response</returns>
    public static RefreshTokenResponse CreateFailure(string errorMessage)
    {
        return new RefreshTokenResponse
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}