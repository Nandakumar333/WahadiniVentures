namespace WahadiniCryptoQuest.Core.DTOs.Reward;

/// <summary>
/// Leaderboard with user rankings
/// </summary>
public record LeaderboardDto
{
    /// <summary>
    /// Period for the leaderboard (Weekly, Monthly, AllTime)
    /// </summary>
    public string Period { get; init; } = string.Empty;

    public List<LeaderboardEntryDto> Entries { get; init; } = new();

    public LeaderboardEntryDto? UserRank { get; init; }
}
