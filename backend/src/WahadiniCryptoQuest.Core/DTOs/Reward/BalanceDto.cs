namespace WahadiniCryptoQuest.Core.DTOs.Reward;

/// <summary>
/// User's current point balance and lifetime earnings
/// </summary>
public record BalanceDto
{
    public int CurrentPoints { get; init; }
    public int TotalEarned { get; init; }
    public int Rank { get; init; }
}
