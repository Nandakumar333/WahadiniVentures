using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.DTOs.Subscription;

/// <summary>
/// DTO for subscription status response
/// </summary>
public record SubscriptionStatusDto
{
    public Guid Id { get; init; }
    public SubscriptionTier Tier { get; init; }
    public SubscriptionStatus Status { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public DateTime CurrentPeriodStart { get; init; }
    public DateTime CurrentPeriodEnd { get; init; }
    public bool IsCancelledAtPeriodEnd { get; init; }
    public DateTime? CancelledAt { get; init; }
    public bool HasPremiumAccess { get; init; }
    public bool IsInGracePeriod { get; init; }
}
