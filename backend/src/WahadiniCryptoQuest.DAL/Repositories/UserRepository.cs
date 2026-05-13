using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// User repository implementation
/// Provides data access operations for User entity
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    private new readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
    }

    public async Task<User?> GetByIdWithRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRoleEnum role, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.Role == role && !u.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.IsActive && !u.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(userId);
        if (user != null)
        {
            user.RecordLogin();
            _dbSet.Update(user);
        }
    }

    // Override GetByIdAsync to exclude soft-deleted users
    public override async Task<User?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
    }

    // Override GetAllAsync to exclude soft-deleted users
    public override async Task<List<User>> GetAllAsync()
    {
        return await _dbSet
            .Where(u => !u.IsDeleted)
            .ToListAsync();
    }

    // Override FindAsync to exclude soft-deleted users by default
    public override async Task<List<User>> FindAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate)
    {
        return await _dbSet
            .Where(predicate)
            .Where(u => !u.IsDeleted)
            .ToListAsync();
    }

    // Override FirstOrDefaultAsync to exclude soft-deleted users
    public override async Task<User?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate)
    {
        return await _dbSet
            .Where(predicate)
            .Where(u => !u.IsDeleted)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets queryable for complex queries (e.g., leaderboard)
    /// Excludes soft-deleted users
    /// </summary>
    public IQueryable<User> GetQueryable()
    {
        return _dbSet.Where(u => !u.IsDeleted);
    }

    /// <summary>
    /// Gets a user by their referral code
    /// </summary>
    public async Task<User?> GetByReferralCodeAsync(string referralCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.ReferralCode == referralCode && !u.IsDeleted, cancellationToken);
    }
}
