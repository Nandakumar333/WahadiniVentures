using System.ComponentModel.DataAnnotations;

namespace WahadiniCryptoQuest.Core.DTOs.Discount;

/// <summary>
/// DTO for creating a new discount code
/// </summary>
public class CreateDiscountCodeDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Code { get; set; } = string.Empty;

    [Range(1, 100)]
    public int DiscountPercentage { get; set; }

    [Range(1, 1000000)]
    public int RequiredPoints { get; set; }

    [Range(0, int.MaxValue)]
    public int MaxRedemptions { get; set; } = 0; // 0 = unlimited

    public DateTime? ExpiryDate { get; set; }
}
