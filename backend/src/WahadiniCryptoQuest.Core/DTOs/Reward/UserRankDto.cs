using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.DTOs.Reward;

/// <summary>
/// User's rank information for a specific period
/// </summary>
public record UserRankDto
{
    public int Rank { get; init; }
    public int Points { get; init; }
    public int TotalUsers { get; init; }
    public LeaderboardPeriod Period { get; init; }
}
