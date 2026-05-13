namespace WahadiniCryptoQuest.Core.DTOs.Subscription;

/// <summary>
/// DTO for currency pricing display
/// </summary>
public record CurrencyPricingDto
{
    public Guid Id { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public decimal MonthlyPrice { get; init; }
    public decimal YearlyPrice { get; init; }
    public decimal YearlySavingsPercent { get; init; }
    public string CurrencySymbol { get; init; } = string.Empty;
    public bool ShowDecimal { get; init; }
    public int DecimalPlaces { get; init; }
    public bool IsActive { get; init; }
    public string FormattedMonthlyPrice { get; init; } = string.Empty;
    public string FormattedYearlyPrice { get; init; } = string.Empty;
}

/// <summary>
/// DTO for creating/updating currency pricing (admin only)
/// </summary>
public record UpsertCurrencyPricingDto
{
    public string CurrencyCode { get; init; } = string.Empty;
    public string StripePriceId { get; init; } = string.Empty;
    public decimal MonthlyPrice { get; init; }
    public decimal YearlyPrice { get; init; }
    public string CurrencySymbol { get; init; } = string.Empty;
    public bool ShowDecimal { get; init; }
    public int DecimalPlaces { get; init; }
}
