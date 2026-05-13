namespace WahadiniCryptoQuest.Core.DTOs.Reward;

/// <summary>
/// Referral information
/// </summary>
public record ReferralDto
{
    public Guid Id { get; init; }
    public Guid ReferrerId { get; init; }
    public Guid ReferredUserId { get; init; }
    public string ReferredUserName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public int PointsAwarded { get; init; }
}
