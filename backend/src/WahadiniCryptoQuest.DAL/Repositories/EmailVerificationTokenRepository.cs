using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// EmailVerificationToken repository implementation
/// Provides data access operations for EmailVerificationToken entity
/// </summary>
public class EmailVerificationTokenRepository : Repository<EmailVerificationToken>, IEmailVerificationTokenRepository
{
    private new readonly ApplicationDbContext _context;

    public EmailVerificationTokenRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<EmailVerificationToken?> GetValidTokenAsync(string token)
    {
        return await _dbSet
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token && 
                               !t.IsUsed && 
                               t.ExpiresAt > DateTime.UtcNow &&
                               !t.IsDeleted);
    }

    public async Task<EmailVerificationToken?> GetByTokenAsync(string token)
    {
        return await _dbSet
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token && !t.IsDeleted);
    }

    public async Task<List<EmailVerificationToken>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(t => t.UserId == userId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<EmailVerificationToken?> GetLatestValidTokenForUserAsync(Guid userId)
    {
        return await _dbSet
            .Where(t => t.UserId == userId && 
                       !t.IsUsed && 
                       t.ExpiresAt > DateTime.UtcNow && 
                       !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task InvalidateAllUserTokensAsync(Guid userId)
    {
        var tokens = await _dbSet
            .Where(t => t.UserId == userId && 
                       !t.IsUsed && 
                       !t.IsDeleted)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.MarkAsUsed();
        }

        if (tokens.Any())
        {
            _dbSet.UpdateRange(tokens);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<EmailVerificationToken>> GetExpiredTokensAsync(DateTime olderThan)
    {
        return await _dbSet
            .Where(t => t.ExpiresAt < olderThan && !t.IsDeleted)
            .ToListAsync();
    }

    public async Task<int> DeleteExpiredTokensAsync(DateTime olderThan)
    {
        var expiredTokens = await GetExpiredTokensAsync(olderThan);
        
        if (expiredTokens.Any())
        {
            _dbSet.RemoveRange(expiredTokens);
            return await _context.SaveChangesAsync();
        }

        return 0;
    }

    public async Task<bool> HasValidTokenAsync(Guid userId)
    {
        return await _dbSet
            .AnyAsync(t => t.UserId == userId && 
                          !t.IsUsed && 
                          t.ExpiresAt > DateTime.UtcNow && 
                          !t.IsDeleted);
    }

    // Override GetByIdAsync to exclude soft-deleted tokens
    public override async Task<EmailVerificationToken?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    // Override GetAllAsync to exclude soft-deleted tokens
    public override async Task<List<EmailVerificationToken>> GetAllAsync()
    {
        return await _dbSet
            .Include(t => t.User)
            .Where(t => !t.IsDeleted)
            .ToListAsync();
    }

    // Override FindAsync to exclude soft-deleted tokens by default
    public override async Task<List<EmailVerificationToken>> FindAsync(System.Linq.Expressions.Expression<Func<EmailVerificationToken, bool>> predicate)
    {
        return await _dbSet
            .Include(t => t.User)
            .Where(predicate)
            .Where(t => !t.IsDeleted)
            .ToListAsync();
    }
}
