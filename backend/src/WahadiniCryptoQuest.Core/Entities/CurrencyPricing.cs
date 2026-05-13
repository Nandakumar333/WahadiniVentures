using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Stores pricing information for different currencies and subscription tiers
/// Enables multi-currency support with regional pricing
/// Admin-configurable for dynamic market adjustments
/// </summary>
public class CurrencyPricing : BaseEntity
{
    // Private constructor for EF Core
    private CurrencyPricing()
    {
        CurrencyCode = string.Empty;
        StripePriceId = string.Empty;
    }

    // Currency Details
    public string CurrencyCode { get; private set; } // ISO 4217 (USD, INR, EUR, JPY, GBP)
    public string StripePriceId { get; private set; } // Stripe Price ID for this currency/tier combination

    // Pricing
    public decimal MonthlyPrice { get; private set; }
    public decimal YearlyPrice { get; private set; }
    public decimal YearlySavingsPercent { get; private set; } // Calculated discount % (typically ~17%)

    // Display & Formatting
    public string CurrencySymbol { get; private set; } = string.Empty; // $, ₹, €, ¥, £
    public bool ShowDecimal { get; private set; } // false for JPY (¥), true for USD ($)
    public int DecimalPlaces { get; private set; } // 0 for JPY, 2 for USD/EUR/GBP

    // Status
    public bool IsActive { get; private set; } // Allow disabling currencies without deletion

    /// <summary>
    /// Factory Method - Create new currency pricing configuration
    /// </summary>
    public static CurrencyPricing Create(
        string currencyCode,
        string stripePriceId,
        decimal monthlyPrice,
        decimal yearlyPrice,
        string currencySymbol,
        bool showDecimal,
        int decimalPlaces,
        Guid adminUserId)
    {
        if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Length != 3)
            throw new ArgumentException("Currency code must be 3-character ISO 4217 code", nameof(currencyCode));
        if (string.IsNullOrWhiteSpace(stripePriceId))
            throw new ArgumentException("Stripe Price ID is required", nameof(stripePriceId));
        if (monthlyPrice <= 0)
            throw new ArgumentException("Monthly price must be greater than zero", nameof(monthlyPrice));
        if (yearlyPrice <= 0)
            throw new ArgumentException("Yearly price must be greater than zero", nameof(yearlyPrice));
        if (yearlyPrice >= monthlyPrice * 12)
            throw new ArgumentException("Yearly price must be discounted from monthly equivalent", nameof(yearlyPrice));
        if (string.IsNullOrWhiteSpace(currencySymbol))
            throw new ArgumentException("Currency symbol is required", nameof(currencySymbol));
        if (decimalPlaces < 0 || decimalPlaces > 4)
            throw new ArgumentException("Decimal places must be between 0 and 4", nameof(decimalPlaces));

        var savingsPercent = Math.Round(
            ((monthlyPrice * 12 - yearlyPrice) / (monthlyPrice * 12)) * 100,
            2
        );

        var pricing = new CurrencyPricing
        {
            CurrencyCode = currencyCode.ToUpperInvariant(),
            StripePriceId = stripePriceId,
            MonthlyPrice = monthlyPrice,
            YearlyPrice = yearlyPrice,
            YearlySavingsPercent = savingsPercent,
            CurrencySymbol = currencySymbol,
            ShowDecimal = showDecimal,
            DecimalPlaces = decimalPlaces,
            IsActive = true,
            CreatedBy = adminUserId.ToString(),
            UpdatedBy = adminUserId.ToString()
        };

        return pricing;
    }

    /// <summary>
    /// Update pricing configuration (admin operation)
    /// </summary>
    public void UpdatePricing(
        decimal monthlyPrice,
        decimal yearlyPrice,
        string stripePriceId,
        Guid adminUserId)
    {
        if (monthlyPrice <= 0)
            throw new ArgumentException("Monthly price must be greater than zero", nameof(monthlyPrice));
        if (yearlyPrice <= 0)
            throw new ArgumentException("Yearly price must be greater than zero", nameof(yearlyPrice));
        if (yearlyPrice >= monthlyPrice * 12)
            throw new ArgumentException("Yearly price must be discounted from monthly equivalent", nameof(yearlyPrice));
        if (string.IsNullOrWhiteSpace(stripePriceId))
            throw new ArgumentException("Stripe Price ID is required", nameof(stripePriceId));

        MonthlyPrice = monthlyPrice;
        YearlyPrice = yearlyPrice;
        StripePriceId = stripePriceId;
        YearlySavingsPercent = Math.Round(
            ((monthlyPrice * 12 - yearlyPrice) / (monthlyPrice * 12)) * 100,
            2
        );
        UpdatedBy = adminUserId.ToString();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update display formatting (admin operation)
    /// </summary>
    public void UpdateFormatting(
        string currencySymbol,
        bool showDecimal,
        int decimalPlaces,
        Guid adminUserId)
    {
        if (string.IsNullOrWhiteSpace(currencySymbol))
            throw new ArgumentException("Currency symbol is required", nameof(currencySymbol));
        if (decimalPlaces < 0 || decimalPlaces > 4)
            throw new ArgumentException("Decimal places must be between 0 and 4", nameof(decimalPlaces));

        CurrencySymbol = currencySymbol;
        ShowDecimal = showDecimal;
        DecimalPlaces = decimalPlaces;
        UpdatedBy = adminUserId.ToString();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activate currency for public use
    /// </summary>
    public void Activate(Guid adminUserId)
    {
        if (IsActive)
            throw new InvalidOperationException("Currency pricing is already active");

        IsActive = true;
        UpdatedBy = adminUserId.ToString();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivate currency (soft disable, does not affect existing subscriptions)
    /// </summary>
    public void Deactivate(Guid adminUserId)
    {
        if (!IsActive)
            throw new InvalidOperationException("Currency pricing is already inactive");

        IsActive = false;
        UpdatedBy = adminUserId.ToString();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Format price for display with proper decimal handling
    /// </summary>
    public string FormatPrice(decimal price)
    {
        if (!ShowDecimal || DecimalPlaces == 0)
            return $"{CurrencySymbol}{Math.Round(price, 0):N0}";

        var format = "N" + DecimalPlaces;
        return $"{CurrencySymbol}{price.ToString(format)}";
    }
}
