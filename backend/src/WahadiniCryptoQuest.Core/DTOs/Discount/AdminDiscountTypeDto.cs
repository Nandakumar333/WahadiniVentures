namespace WahadiniCryptoQuest.Core.DTOs.Discount;

/// <summary>
/// Admin DTO for discount code with full details
/// </summary>
public class AdminDiscountTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int DiscountPercentage { get; set; }
    public int RequiredPoints { get; set; }
    public int MaxRedemptions { get; set; }
    public int CurrentRedemptions { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
