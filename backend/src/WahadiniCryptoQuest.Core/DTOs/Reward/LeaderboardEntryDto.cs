namespace WahadiniCryptoQuest.Core.DTOs.Reward;

/// <summary>
/// Single leaderboard entry
/// </summary>
public record LeaderboardEntryDto
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Points { get; init; }
    public int Rank { get; init; }
    public string? AvatarUrl { get; init; }
}
