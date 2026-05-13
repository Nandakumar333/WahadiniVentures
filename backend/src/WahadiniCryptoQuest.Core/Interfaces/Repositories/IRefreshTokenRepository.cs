using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for refresh token operations
/// Provides contract for refresh token data access and management
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>
    /// Gets a refresh token by its token string
    /// </summary>
    /// <param name="token">The refresh token string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>RefreshToken if found, null otherwise</returns>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active (valid) refresh tokens for a specific user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active refresh tokens</returns>
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all refresh tokens for a specific user (including inactive ones)
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all refresh tokens for the user</returns>
    Task<IEnumerable<RefreshToken>> GetAllTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a specific refresh token
    /// </summary>
    /// <param name="token">The refresh token string to revoke</param>
    /// <param name="revokedBy">Who revoked the token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token was found and revoked, false otherwise</returns>
    Task<bool> RevokeTokenAsync(string token, string revokedBy = "System", CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all refresh tokens for a specific user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="revokedBy">Who revoked the tokens</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of tokens revoked</returns>
    Task<int> RevokeAllUserTokensAsync(Guid userId, string revokedBy = "System", CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes expired tokens from the database (cleanup operation)
    /// </summary>
    /// <param name="cutoffDate">Remove tokens that expired before this date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of tokens removed</returns>
    Task<int> RemoveExpiredTokensAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of active refresh tokens for a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of active tokens</returns>
    Task<int> GetActiveTokenCountAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a refresh token exists and is valid
    /// </summary>
    /// <param name="token">The refresh token string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token exists and is valid</returns>
    Task<bool> IsValidTokenAsync(string token, CancellationToken cancellationToken = default);
}