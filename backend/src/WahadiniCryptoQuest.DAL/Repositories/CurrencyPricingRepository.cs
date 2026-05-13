using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

public class CurrencyPricingRepository : Repository<CurrencyPricing>, ICurrencyPricingRepository
{
    public CurrencyPricingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<CurrencyPricing?> GetByCurrencyCodeAsync(string currencyCode, CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyPricings
            .Where(cp => cp.CurrencyCode == currencyCode.ToUpperInvariant() && !cp.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<CurrencyPricing>> GetActiveCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyPricings
            .Where(cp => cp.IsActive && !cp.IsDeleted)
            .OrderBy(cp => cp.CurrencyCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CurrencyPricing>> GetAllIncludingInactiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyPricings
            .Where(cp => !cp.IsDeleted)
            .OrderBy(cp => cp.CurrencyCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<CurrencyPricing?> GetByStripePriceIdAsync(string stripePriceId, CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyPricings
            .Where(cp => cp.StripePriceId == stripePriceId && !cp.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string currencyCode, CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyPricings
            .AnyAsync(cp => cp.CurrencyCode == currencyCode.ToUpperInvariant(), cancellationToken);
    }
}
