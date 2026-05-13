using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.DTOs.Subscription;

/// <summary>
/// DTO for subscription history entry
/// </summary>
public record SubscriptionHistoryDto
{
    public Guid Id { get; init; }
    public string ChangeType { get; init; } = string.Empty;
    public SubscriptionTier? PreviousTier { get; init; }
    public SubscriptionTier? NewTier { get; init; }
    public SubscriptionStatus? PreviousStatus { get; init; }
    public SubscriptionStatus? NewStatus { get; init; }
    public DateTime? PreviousPeriodEnd { get; init; }
    public DateTime? NewPeriodEnd { get; init; }
    public string? Notes { get; init; }
    public string? TriggeredBy { get; init; }
    public DateTime CreatedAt { get; init; }
}
