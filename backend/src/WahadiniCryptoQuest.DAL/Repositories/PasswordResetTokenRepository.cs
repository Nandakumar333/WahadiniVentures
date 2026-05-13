using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Repository implementation for password reset token operations
/// Provides data access for password reset token management
/// </summary>
public class PasswordResetTokenRepository : Repository<PasswordResetToken>, IPasswordResetTokenRepository
{
    private new readonly ApplicationDbContext _context;

    public PasswordResetTokenRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a password reset token by its hashed token value
    /// </summary>
    /// <param name="hashedToken">The hashed token string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PasswordResetToken if found, null otherwise</returns>
    public async Task<PasswordResetToken?> GetByHashedTokenAsync(string hashedToken, CancellationToken cancellationToken = default)
    {
        return await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.HashedToken == hashedToken, cancellationToken);
    }

    /// <summary>
    /// Gets all active (valid) password reset tokens for a specific user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active password reset tokens</returns>
    public async Task<IEnumerable<PasswordResetToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.PasswordResetTokens
            .Include(t => t.User)
            .Where(t => t.UserId == userId && 
                       !t.IsUsed && 
                       t.ExpiresAt > now && 
                       !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all password reset tokens for a specific user (including inactive ones)
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all password reset tokens for the user</returns>
    public async Task<IEnumerable<PasswordResetToken>> GetAllTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.PasswordResetTokens
            .Include(t => t.User)
            .Where(t => t.UserId == userId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Marks a password reset token as used
    /// </summary>
    /// <param name="tokenId">Token ID to mark as used</param>
    /// <param name="usedBy">Who used the token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token was found and marked as used, false otherwise</returns>
    public async Task<bool> MarkTokenAsUsedAsync(Guid tokenId, string usedBy = "System", CancellationToken cancellationToken = default)
    {
        var token = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Id == tokenId && !t.IsDeleted, cancellationToken);

        if (token == null)
            return false;

        token.MarkAsUsed(usedBy);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Invalidates all password reset tokens for a specific user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="invalidatedBy">Who invalidated the tokens</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of tokens invalidated</returns>
    public async Task<int> InvalidateAllUserTokensAsync(Guid userId, string invalidatedBy = "System", CancellationToken cancellationToken = default)
    {
        var activeTokens = await _context.PasswordResetTokens
            .Where(t => t.UserId == userId && 
                       !t.IsUsed && 
                       t.ExpiresAt > DateTime.UtcNow && 
                       !t.IsDeleted)
            .ToListAsync(cancellationToken);

        var count = 0;
        foreach (var token in activeTokens)
        {
            token.MarkAsUsed(invalidatedBy);
            count++;
        }

        if (count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return count;
    }

    /// <summary>
    /// Removes expired password reset tokens from the database (cleanup operation)
    /// </summary>
    /// <param name="cutoffDate">Remove tokens that expired before this date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of tokens removed</returns>
    public async Task<int> RemoveExpiredTokensAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        var expiredTokens = await _context.PasswordResetTokens
            .Where(t => t.ExpiresAt < cutoffDate || t.IsDeleted)
            .ToListAsync(cancellationToken);

        if (expiredTokens.Any())
        {
            _context.PasswordResetTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return expiredTokens.Count;
    }

    /// <summary>
    /// Gets the count of active password reset tokens for a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of active tokens</returns>
    public async Task<int> GetActiveTokenCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.PasswordResetTokens
            .CountAsync(t => t.UserId == userId && 
                           !t.IsUsed && 
                           t.ExpiresAt > now && 
                           !t.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Checks if a password reset token exists and is valid by raw token
    /// </summary>
    /// <param name="rawToken">The raw token string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PasswordResetToken if valid, null otherwise</returns>
    public async Task<PasswordResetToken?> GetValidTokenAsync(string rawToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawToken))
            return null;

        // Hash the provided token to search for it
        var hashedToken = HashToken(rawToken);
        
        var now = DateTime.UtcNow;
        var token = await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.HashedToken == hashedToken && 
                                    !t.IsUsed && 
                                    t.ExpiresAt > now && 
                                    !t.IsDeleted, cancellationToken);

        // Double-check with domain logic
        if (token != null && !token.MatchesToken(rawToken))
        {
            return null;
        }

        return token;
    }

    /// <summary>
    /// Gets the most recent password reset token for a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Most recent token if found, null otherwise</returns>
    public async Task<PasswordResetToken?> GetLatestTokenByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.PasswordResetTokens
            .Include(t => t.User)
            .Where(t => t.UserId == userId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Hashes a token using SHA-256 for secure storage
    /// </summary>
    /// <param name="token">Raw token to hash</param>
    /// <returns>Hashed token</returns>
    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashedBytes);
    }
}
