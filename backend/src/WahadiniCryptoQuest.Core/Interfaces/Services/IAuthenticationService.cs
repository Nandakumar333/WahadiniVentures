using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces;

/// <summary>
/// Interface for authentication operations
/// Handles JWT token generation, validation, and user authentication logic
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Generates JWT access token for authenticated user
    /// </summary>
    /// <param name="user">Authenticated user</param>
    /// <returns>JWT token string</returns>
    Task<string> GenerateAccessTokenAsync(User user);

    /// <summary>
    /// Generates refresh token for user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Refresh token string</returns>
    Task<string> GenerateRefreshTokenAsync(Guid userId);

    /// <summary>
    /// Validates and refreshes JWT token using refresh token
    /// </summary>
    /// <param name="refreshToken">Refresh token string</param>
    /// <returns>New access token if valid, null otherwise</returns>
    Task<string?> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Validates JWT token and extracts user claims
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>ClaimsPrincipal if valid, null otherwise</returns>
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token);

    /// <summary>
    /// Revokes refresh token (logout)
    /// </summary>
    /// <param name="refreshToken">Refresh token to revoke</param>
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Gets token expiration time
    /// </summary>
    /// <returns>Token expiration DateTime</returns>
    DateTime GetTokenExpiration();
}