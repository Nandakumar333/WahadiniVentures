namespace WahadiniCryptoQuest.Core.DTOs.Reward;

/// <summary>
/// Achievement badge information
/// </summary>
public record AchievementDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string IconUrl { get; init; } = string.Empty;
    public int PointBonus { get; init; }
    public bool IsUnlocked { get; init; }
    public DateTime? UnlockedAt { get; init; }
    public int Progress { get; init; } // 0-100 percentage
    public bool? IsNotified { get; init; }
    public int DisplayOrder { get; init; } = 0;
}
