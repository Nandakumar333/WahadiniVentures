using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for EmailVerificationToken entity
/// Defines data access operations for email verification tokens
/// </summary>
public interface IEmailVerificationTokenRepository : IRepository<EmailVerificationToken>
{
    /// <summary>
    /// Finds a valid verification token by token value
    /// </summary>
    /// <param name="token">Token value to search for</param>
    /// <returns>Valid token if found, null otherwise</returns>
    Task<EmailVerificationToken?> GetValidTokenAsync(string token);

    /// <summary>
    /// Finds a verification token by token value regardless of validity
    /// </summary>
    /// <param name="token">Token value to search for</param>
    /// <returns>Token if found, null otherwise</returns>
    Task<EmailVerificationToken?> GetByTokenAsync(string token);

    /// <summary>
    /// Finds verification tokens for a specific user
    /// </summary>
    /// <param name="userId">User ID to search for</param>
    /// <returns>List of tokens for the user</returns>
    Task<List<EmailVerificationToken>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets the most recent valid token for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Most recent valid token if found, null otherwise</returns>
    Task<EmailVerificationToken?> GetLatestValidTokenForUserAsync(Guid userId);

    /// <summary>
    /// Marks all existing tokens for a user as used
    /// Called when creating a new verification token to invalidate old ones
    /// </summary>
    /// <param name="userId">User ID</param>
    Task InvalidateAllUserTokensAsync(Guid userId);

    /// <summary>
    /// Gets expired tokens that haven't been cleaned up
    /// Used for background cleanup tasks
    /// </summary>
    /// <param name="olderThan">Expiration threshold</param>
    /// <returns>List of expired tokens</returns>
    Task<List<EmailVerificationToken>> GetExpiredTokensAsync(DateTime olderThan);

    /// <summary>
    /// Deletes expired tokens (cleanup operation)
    /// </summary>
    /// <param name="olderThan">Expiration threshold</param>
    /// <returns>Number of tokens deleted</returns>
    Task<int> DeleteExpiredTokensAsync(DateTime olderThan);

    /// <summary>
    /// Checks if a user has any valid verification tokens
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>True if user has valid tokens, false otherwise</returns>
    Task<bool> HasValidTokenAsync(Guid userId);
}
