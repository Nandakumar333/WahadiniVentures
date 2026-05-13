using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Repository implementation for point adjustment operations.
/// </summary>
public class PointAdjustmentRepository : Repository<PointAdjustment>, IPointAdjustmentRepository
{
    private new readonly ApplicationDbContext _context;

    public PointAdjustmentRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<(List<PointAdjustment> Items, int TotalCount)> GetByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.Timestamp);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<PointAdjustment> Items, int TotalCount)> GetByAdminUserIdAsync(
        Guid adminUserId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(p => p.AdminUserId == adminUserId)
            .OrderByDescending(p => p.Timestamp);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
