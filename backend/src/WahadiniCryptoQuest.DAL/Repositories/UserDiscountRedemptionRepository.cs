using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Repository implementation for UserDiscountRedemption entity
/// </summary>
public class UserDiscountRedemptionRepository : Repository<UserDiscountRedemption>, IUserDiscountRedemptionRepository
{
    public UserDiscountRedemptionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> HasUserRedeemedDiscountAsync(
        Guid userId,
        Guid discountCodeId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(r => r.UserId == userId && r.DiscountCodeId == discountCodeId, cancellationToken);
    }

    public async Task<(IEnumerable<UserDiscountRedemption> Items, int TotalCount)> GetUserRedemptionsAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(r => r.DiscountCode)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RedeemedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<UserDiscountRedemption>> GetRedemptionsForAnalyticsAsync(
        Guid? discountCodeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(r => r.DiscountCode)
            .AsQueryable();

        if (discountCodeId.HasValue)
        {
            query = query.Where(r => r.DiscountCodeId == discountCodeId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserDiscountRedemption>> GetByDiscountCodeIdAsync(
        Guid discountCodeId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.DiscountCodeId == discountCodeId)
            .OrderBy(r => r.RedeemedAt)
            .ToListAsync(cancellationToken);
    }
}
