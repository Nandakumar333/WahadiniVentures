using WahadiniCryptoQuest.Core.DTOs.Reward;

namespace WahadiniCryptoQuest.Core.Interfaces.Services;

/// <summary>
/// Service for managing user daily login streaks and bonus rewards
/// </summary>
public interface IStreakService
{
    /// <summary>
    /// Processes a user's login to update streak tracking and award bonus points
    /// </summary>
    /// <param name="userId">The user who logged in</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated streak information with bonus points awarded</returns>
    Task<StreakDto> ProcessLoginAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current streak information for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current streak details</returns>
    Task<StreakDto> GetUserStreakAsync(Guid userId, CancellationToken cancellationToken = default);
}
