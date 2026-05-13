using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Repository implementation for DiscountCode entity
/// </summary>
public class DiscountCodeRepository : Repository<DiscountCode>, IDiscountCodeRepository
{
    public DiscountCodeRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets discount code by unique code string
    /// </summary>
    public async Task<DiscountCode?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Code == code, cancellationToken);
    }

    /// <summary>
    /// Gets all active discount codes (IsActive=true, not expired)
    /// </summary>
    public async Task<IEnumerable<DiscountCode>> GetActiveCodesAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _dbSet
            .AsNoTracking()
            .Where(d => d.IsActive && d.ExpiryDate > now)
            .OrderBy(d => d.RequiredPoints)
            .ToListAsync(cancellationToken);
    }
}
