using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for password reset token operations
/// Provides contract for password reset token data access and management
/// </summary>
public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken>
{
    /// <summary>
    /// Gets a password reset token by its hashed token value
    /// </summary>
    /// <param name="hashedToken">The hashed token string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PasswordResetToken if found, null otherwise</returns>
   Task<PasswordResetToken?> GetByHashedTokenAsync(string hashedToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active (valid) password reset tokens for a specific user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active password reset tokens</returns>
   Task<IEnumerable<PasswordResetToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all password reset tokens for a specific user (including inactive ones)
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all password reset tokens for the user</returns>
   Task<IEnumerable<PasswordResetToken>> GetAllTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a password reset token as used
    /// </summary>
    /// <param name="tokenId">Token ID to mark as used</param>
    /// <param name="usedBy">Who used the token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token was found and marked as used, false otherwise</returns>
   Task<bool> MarkTokenAsUsedAsync(Guid tokenId, string usedBy = "System", CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all password reset tokens for a specific user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="invalidatedBy">Who invalidated the tokens</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of tokens invalidated</returns>
   Task<int> InvalidateAllUserTokensAsync(Guid userId, string invalidatedBy = "System", CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes expired password reset tokens from the database (cleanup operation)
    /// </summary>
    /// <param name="cutoffDate">Remove tokens that expired before this date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of tokens removed</returns>
   Task<int> RemoveExpiredTokensAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of active password reset tokens for a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of active tokens</returns>
   Task<int> GetActiveTokenCountAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a password reset token exists and is valid by raw token
    /// </summary>
    /// <param name="rawToken">The raw token string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PasswordResetToken if valid, null otherwise</returns>
   Task<PasswordResetToken?> GetValidTokenAsync(string rawToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most recent password reset token for a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Most recent token if found, null otherwise</returns>
   Task<PasswordResetToken?> GetLatestTokenByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}