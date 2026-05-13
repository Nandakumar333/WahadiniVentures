using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for UserAchievement entity with achievement tracking operations
/// </summary>
public interface IUserAchievementRepository : IRepository<UserAchievement>
{
    /// <summary>
    /// Gets all achievements unlocked by a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user's unlocked achievements</returns>
    Task<IEnumerable<UserAchievement>> GetUserAchievementsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has already unlocked a specific achievement
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="achievementId">Achievement ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if achievement is already unlocked, false otherwise</returns>
    Task<bool> HasAchievementAsync(Guid userId, string achievementId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unnotified achievements for a user (for badge display)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of achievements that user hasn't been notified about</returns>
    Task<IEnumerable<UserAchievement>> GetUnnotifiedAchievementsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks achievements as notified
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="achievementIds">List of achievement IDs to mark as notified</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task MarkAsNotifiedAsync(Guid userId, IEnumerable<string> achievementIds, CancellationToken cancellationToken = default);
}
