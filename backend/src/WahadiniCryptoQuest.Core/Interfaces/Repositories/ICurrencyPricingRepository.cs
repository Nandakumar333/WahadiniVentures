using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for CurrencyPricing entity
/// Provides currency-specific query methods
/// </summary>
public interface ICurrencyPricingRepository : IRepository<CurrencyPricing>
{
    /// <summary>
    /// Get pricing by currency code
    /// </summary>
    Task<CurrencyPricing?> GetByCurrencyCodeAsync(string currencyCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active currencies
    /// </summary>
    Task<List<CurrencyPricing>> GetActiveCurrenciesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all currency pricings including inactive ones (admin only)
    /// </summary>
    Task<List<CurrencyPricing>> GetAllIncludingInactiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get currency by Stripe price ID
    /// </summary>
    Task<CurrencyPricing?> GetByStripePriceIdAsync(string stripePriceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if currency code exists
    /// </summary>
    Task<bool> ExistsAsync(string currencyCode, CancellationToken cancellationToken = default);
}
