using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Repository implementation for refresh token operations
/// Provides data access methods for refresh token management
/// </summary>
public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    private new readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a refresh token by its token string
    /// </summary>
    /// <param name="token">The refresh token string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>RefreshToken if found, null otherwise</returns>
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets all active (valid) refresh tokens for a specific user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active refresh tokens</returns>
    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .Where(rt => rt.UserId == userId && 
                        !rt.IsUsed && 
                        !rt.IsRevoked && 
                        rt.ExpiresAt > DateTime.UtcNow && 
                        !rt.IsDeleted)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all refresh tokens for a specific user (including inactive ones)
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all refresh tokens for the user</returns>
    public async Task<IEnumerable<RefreshToken>> GetAllTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .Where(rt => rt.UserId == userId && !rt.IsDeleted)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Revokes a specific refresh token
    /// </summary>
    /// <param name="token">The refresh token string to revoke</param>
    /// <param name="revokedBy">Who revoked the token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token was found and revoked, false otherwise</returns>
    public async Task<bool> RevokeTokenAsync(string token, string revokedBy = "System", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        var refreshToken = await GetByTokenAsync(token, cancellationToken);
        if (refreshToken == null)
            return false;

        refreshToken.Revoke(revokedBy);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Revokes all refresh tokens for a specific user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="revokedBy">Who revoked the tokens</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of tokens revoked</returns>
    public async Task<int> RevokeAllUserTokensAsync(Guid userId, string revokedBy = "System", CancellationToken cancellationToken = default)
    {
        var activeTokens = await GetActiveTokensByUserIdAsync(userId, cancellationToken);
        var count = 0;

        foreach (var token in activeTokens)
        {
            token.Revoke(revokedBy);
            count++;
        }

        if (count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return count;
    }

    /// <summary>
    /// Removes expired tokens from the database (cleanup operation)
    /// </summary>
    /// <param name="cutoffDate">Remove tokens that expired before this date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of tokens removed</returns>
    public async Task<int> RemoveExpiredTokensAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt < cutoffDate && !rt.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var token in expiredTokens)
        {
            token.SoftDelete();
        }

        if (expiredTokens.Any())
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return expiredTokens.Count;
    }

    /// <summary>
    /// Gets the count of active refresh tokens for a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of active tokens</returns>
    public async Task<int> GetActiveTokenCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .CountAsync(rt => rt.UserId == userId && 
                             !rt.IsUsed && 
                             !rt.IsRevoked && 
                             rt.ExpiresAt > DateTime.UtcNow && 
                             !rt.IsDeleted, 
                       cancellationToken);
    }

    /// <summary>
    /// Checks if a refresh token exists and is valid
    /// </summary>
    /// <param name="token">The refresh token string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token exists and is valid</returns>
    public async Task<bool> IsValidTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        var refreshToken = await GetByTokenAsync(token, cancellationToken);
        return refreshToken?.IsValid == true;
    }
}
