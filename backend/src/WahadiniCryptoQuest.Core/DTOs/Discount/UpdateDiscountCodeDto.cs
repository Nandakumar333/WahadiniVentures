using System.ComponentModel.DataAnnotations;

namespace WahadiniCryptoQuest.Core.DTOs.Discount;

/// <summary>
/// DTO for updating an existing discount code
/// </summary>
public class UpdateDiscountCodeDto
{
    [Range(1, 100)]
    public int? DiscountPercentage { get; set; }

    [Range(1, 1000000)]
    public int? RequiredPoints { get; set; }

    [Range(0, int.MaxValue)]
    public int? MaxRedemptions { get; set; }

    public DateTime? ExpiryDate { get; set; }
}
