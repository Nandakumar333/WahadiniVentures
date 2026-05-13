using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Repository implementation for UserAchievement entity
/// </summary>
public class UserAchievementRepository : Repository<UserAchievement>, IUserAchievementRepository
{
    public UserAchievementRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all achievements unlocked by a specific user
    /// </summary>
    public async Task<IEnumerable<UserAchievement>> GetUserAchievementsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(ua => ua.UserId == userId)
            .OrderByDescending(ua => ua.UnlockedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a user has already unlocked a specific achievement
    /// </summary>
    public async Task<bool> HasAchievementAsync(Guid userId, string achievementId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId, cancellationToken);
    }

    /// <summary>
    /// Gets unnotified achievements for a user (for badge display)
    /// </summary>
    public async Task<IEnumerable<UserAchievement>> GetUnnotifiedAchievementsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(ua => ua.UserId == userId && !ua.Notified)
            .OrderBy(ua => ua.UnlockedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Marks achievements as notified
    /// </summary>
    public async Task MarkAsNotifiedAsync(Guid userId, IEnumerable<string> achievementIds, CancellationToken cancellationToken = default)
    {
        var achievements = await _dbSet
            .Where(ua => ua.UserId == userId && achievementIds.Contains(ua.AchievementId))
            .ToListAsync(cancellationToken);

        foreach (var achievement in achievements)
        {
            achievement.MarkAsNotified();
        }

        _dbSet.UpdateRange(achievements);
    }
}
