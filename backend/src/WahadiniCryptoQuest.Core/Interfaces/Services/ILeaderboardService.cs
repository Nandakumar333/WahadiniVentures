using WahadiniCryptoQuest.Core.DTOs.Reward;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Interfaces.Services;

/// <summary>
/// Service interface for leaderboard operations
/// Implements T063: Cached leaderboards with personal rank tracking
/// </summary>
public interface ILeaderboardService
{
    /// <summary>
    /// Gets the leaderboard for a specific period with caching
    /// </summary>
    /// <param name="period">Leaderboard period (Weekly, Monthly, AllTime)</param>
    /// <param name="limit">Number of top users to return (default: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of leaderboard entries</returns>
    Task<IReadOnlyList<LeaderboardEntryDto>> GetLeaderboardAsync(
        LeaderboardPeriod period,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current user's rank for a specific period
    /// Real-time calculation without caching
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="period">Leaderboard period</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User's rank information</returns>
    Task<UserRankDto> GetUserRankAsync(
        Guid userId,
        LeaderboardPeriod period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the leaderboard cache for a specific period
    /// </summary>
    /// <param name="period">Period to clear (null = all periods)</param>
    void ClearCache(LeaderboardPeriod? period = null);
}
