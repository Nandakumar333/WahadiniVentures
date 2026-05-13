using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Repository implementation for UserStreak entity
/// </summary>
public class UserStreakRepository : Repository<UserStreak>, IUserStreakRepository
{
    public UserStreakRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets a user's streak record by user ID
    /// </summary>
    public async Task<UserStreak?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
    }

    /// <summary>
    /// Gets all active streaks (current streak > 0)
    /// </summary>
    public async Task<IEnumerable<UserStreak>> GetActiveStreaksAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.CurrentStreak > 0)
            .OrderByDescending(s => s.CurrentStreak)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets top streaks by longest streak value
    /// </summary>
    public async Task<IEnumerable<UserStreak>> GetTopLongestStreaksAsync(int limit, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .OrderByDescending(s => s.LongestStreak)
            .ThenByDescending(s => s.CurrentStreak)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
