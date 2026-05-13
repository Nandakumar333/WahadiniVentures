namespace WahadiniCryptoQuest.Core.DTOs.Subscription;

/// <summary>
/// DTO for creating Stripe checkout session
/// </summary>
public record CreateCheckoutSessionDto
{
    public string Tier { get; init; } = string.Empty; // "MonthlyPremium" or "YearlyPremium"
    public string CurrencyCode { get; init; } = "USD"; // ISO 4217
    public string? DiscountCode { get; init; }
}

/// <summary>
/// Response DTO with checkout session URL
/// </summary>
public record CheckoutSessionResponseDto
{
    public string SessionId { get; init; } = string.Empty;
    public string CheckoutUrl { get; init; } = string.Empty;
}
