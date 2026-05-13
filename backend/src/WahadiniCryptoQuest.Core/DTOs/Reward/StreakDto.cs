namespace WahadiniCryptoQuest.Core.DTOs.Reward;

/// <summary>
/// User's daily login streak information
/// </summary>
public record StreakDto
{
    public int CurrentStreak { get; init; }
    public int LongestStreak { get; init; }
    public DateTime LastLoginDate { get; init; }
    public int BonusPointsAwarded { get; init; }
    public int? NextMilestoneAt { get; init; }
}
