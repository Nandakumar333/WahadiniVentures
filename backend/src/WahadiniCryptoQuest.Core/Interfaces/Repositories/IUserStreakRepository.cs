using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for UserStreak entity with streak management operations
/// </summary>
public interface IUserStreakRepository : IRepository<UserStreak>
{
    /// <summary>
    /// Gets a user's streak record by user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User's streak record if exists, null otherwise</returns>
    Task<UserStreak?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active streaks (current streak > 0)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of users with active streaks</returns>
    Task<IEnumerable<UserStreak>> GetActiveStreaksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets top streaks by longest streak value
    /// </summary>
    /// <param name="limit">Number of top streaks to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of top longest streaks</returns>
    Task<IEnumerable<UserStreak>> GetTopLongestStreaksAsync(int limit, CancellationToken cancellationToken = default);
}
