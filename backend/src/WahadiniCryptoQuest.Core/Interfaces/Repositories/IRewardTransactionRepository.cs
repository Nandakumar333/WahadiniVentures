using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for RewardTransaction entity with transaction history operations
/// </summary>
public interface IRewardTransactionRepository : IRepository<RewardTransaction>
{
    /// <summary>
    /// Gets a user's transaction history (paginated)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of user's reward transactions</returns>
    Task<PagedResult<RewardTransaction>> GetUserTransactionHistoryAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all transactions within a specific date range
    /// </summary>
    /// <param name="from">Start date</param>
    /// <param name="to">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of transactions within the date range</returns>
    Task<IEnumerable<RewardTransaction>> GetTransactionsByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an existing transaction by reference (for idempotency checks)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="referenceId">Reference ID (e.g., LessonId, TaskId)</param>
    /// <param name="referenceType">Reference type (e.g., "Lesson", "Task")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Existing transaction or null if not found</returns>
    Task<RewardTransaction?> GetByReferenceAsync(Guid userId, string referenceId, string referenceType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets queryable for advanced filtering and pagination (for cursor-based pagination)
    /// </summary>
    /// <returns>IQueryable for reward transactions</returns>
    IQueryable<RewardTransaction> GetQueryable();
}
