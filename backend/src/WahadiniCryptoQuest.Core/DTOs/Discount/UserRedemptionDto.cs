namespace WahadiniCryptoQuest.Core.DTOs.Discount;

/// <summary>
/// DTO for user's discount redemption history
/// </summary>
public class UserRedemptionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int DiscountPercentage { get; set; }
    public DateTime RedeemedAt { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool UsedInSubscription { get; set; }
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate < DateTime.UtcNow;
}
