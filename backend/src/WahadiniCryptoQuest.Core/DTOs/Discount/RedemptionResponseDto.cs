namespace WahadiniCryptoQuest.Core.DTOs.Discount;

/// <summary>
/// Response DTO after successful discount redemption
/// </summary>
public class RedemptionResponseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int DiscountPercentage { get; set; }
    public int PointsDeducted { get; set; }
    public int RemainingPoints { get; set; }
    public DateTime RedeemedAt { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Message { get; set; } = string.Empty;
}
