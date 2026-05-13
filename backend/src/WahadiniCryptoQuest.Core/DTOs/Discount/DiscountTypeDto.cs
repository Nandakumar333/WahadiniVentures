namespace WahadiniCryptoQuest.Core.DTOs.Discount;

/// <summary>
/// DTO for displaying available discount types to users
/// </summary>
public class DiscountTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int DiscountPercentage { get; set; }
    public int RequiredPoints { get; set; }
    public int MaxRedemptions { get; set; }
    public int CurrentRedemptions { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public bool CanAfford { get; set; } // Based on user's current points
    public bool CanRedeem { get; set; } // Overall availability
}
