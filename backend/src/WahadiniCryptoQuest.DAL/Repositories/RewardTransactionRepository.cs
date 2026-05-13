using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Repository implementation for RewardTransaction entity
/// </summary>
public class RewardTransactionRepository : Repository<RewardTransaction>, IRewardTransactionRepository
{
    public RewardTransactionRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets user's transaction history with pagination
    /// </summary>
    public async Task<PagedResult<RewardTransaction>> GetUserTransactionHistoryAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt);

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<RewardTransaction>(items, totalItems, page, pageSize);
    }

    /// <summary>
    /// Gets transactions within a date range
    /// </summary>
    public async Task<IEnumerable<RewardTransaction>> GetTransactionsByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets an existing transaction by reference (for idempotency)
    /// </summary>
    public async Task<RewardTransaction?> GetByReferenceAsync(
        Guid userId,
        string referenceId,
        string referenceType,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.UserId == userId
                && t.ReferenceId == referenceId
                && t.ReferenceType == referenceType,
                cancellationToken);
    }

    /// <summary>
    /// Gets queryable for advanced filtering and cursor-based pagination
    /// </summary>
    public IQueryable<RewardTransaction> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }
}
