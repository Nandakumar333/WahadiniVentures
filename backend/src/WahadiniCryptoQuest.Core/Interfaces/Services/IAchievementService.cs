using WahadiniCryptoQuest.Core.DTOs.Reward;

namespace WahadiniCryptoQuest.Core.Interfaces.Services;

/// <summary>
/// Service interface for achievement tracking and unlocking
/// T078: Achievement evaluation and bonus point awarding
/// </summary>
public interface IAchievementService
{
    /// <summary>
    /// Checks user's progress against all achievement criteria and unlocks eligible achievements
    /// Awards bonus points for newly unlocked achievements
    /// </summary>
    /// <param name="userId">User ID to check achievements for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of newly unlocked achievement IDs</returns>
    Task<IEnumerable<string>> CheckAndUnlockAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all achievements with user's unlock status and progress
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of achievements with unlock status and progress percentage</returns>
    Task<IEnumerable<AchievementDto>> GetUserAchievementsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unnotified achievements for badge display
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of achievements user hasn't been notified about</returns>
    Task<IEnumerable<AchievementDto>> GetUnnotifiedAchievementsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks achievements as notified (user has seen the unlock notification)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="achievementIds">List of achievement IDs to mark as notified</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task MarkAsNotifiedAsync(Guid userId, IEnumerable<string> achievementIds, CancellationToken cancellationToken = default);
}
