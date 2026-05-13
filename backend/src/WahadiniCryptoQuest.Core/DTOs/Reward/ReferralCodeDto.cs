namespace WahadiniCryptoQuest.Core.DTOs.Reward;

/// <summary>
/// User's referral code information
/// </summary>
public record ReferralCodeDto
{
    public string ReferralCode { get; init; } = string.Empty;
    public int SuccessfulReferrals { get; init; }
    public int TotalPointsEarned { get; init; }
}
