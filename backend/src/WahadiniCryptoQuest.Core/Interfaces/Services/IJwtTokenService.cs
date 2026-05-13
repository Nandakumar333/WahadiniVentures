using System.Security.Claims;
using WahadiniCryptoQuest.Core.DTOs.Auth;

namespace WahadiniCryptoQuest.Core.Interfaces.Services;

/// <summary>
/// Interface for JWT token operations including generation, validation, and refresh
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a new access token for the authenticated user with roles and permissions
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="email">The user's email address</param>
    /// <param name="username">The user's username</param>
    /// <param name="roles">The user's assigned roles</param>
    /// <param name="emailConfirmed">Whether the user's email is confirmed</param>
    /// <param name="permissions">Optional list of permissions to include in token</param>
    /// <returns>Access token string</returns>
    Task<string> GenerateAccessTokenAsync(Guid userId, string email, string username, IEnumerable<string> roles, bool emailConfirmed = false, IEnumerable<string>? permissions = null);

    /// <summary>
    /// Generates a new refresh token for session management
    /// </summary>
    /// <returns>Refresh token string</returns>
    Task<string> GenerateRefreshTokenAsync();

    /// <summary>
    /// Validates an access token and returns the claims principal
    /// </summary>
    /// <param name="token">The JWT access token to validate</param>
    /// <returns>Claims principal if token is valid, null otherwise</returns>
    Task<ClaimsPrincipal?> ValidateAccessTokenAsync(string token);

    /// <summary>
    /// Extracts claims from a token without validating expiration
    /// </summary>
    /// <param name="token">The JWT token to extract claims from</param>
    /// <returns>Claims principal with token data</returns>
    Task<ClaimsPrincipal?> GetPrincipalFromTokenAsync(string token);

    /// <summary>
    /// Gets the remaining time until token expiration
    /// </summary>
    /// <param name="token">The JWT token to check</param>
    /// <returns>TimeSpan until expiration, null if token is invalid</returns>
    TimeSpan? GetTokenRemainingLifetime(string token);
}