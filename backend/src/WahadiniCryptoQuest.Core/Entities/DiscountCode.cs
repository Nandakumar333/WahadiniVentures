using System.ComponentModel.DataAnnotations;
using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Discount code for point-based subscription discounts
/// </summary>
public class DiscountCode : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    public int DiscountPercentage { get; set; }

    public int RequiredPoints { get; set; }

    public int MaxRedemptions { get; set; } = 0; // 0 = unlimited

    public int CurrentRedemptions { get; set; } = 0;

    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; } = true;

    // Admin Dashboard - Extended Properties
    /// <summary>
    /// Maximum number of times this code can be used (admin-managed)
    /// </summary>
    public int UsageLimit { get; set; } = 0; // 0 = unlimited

    /// <summary>
    /// Current number of times this code has been used
    /// </summary>
    public int UsageCount { get; set; } = 0;

    /// <summary>
    /// ID of the admin who created this discount code
    /// </summary>
    public new Guid? CreatedBy { get; set; }

    // Navigation properties
    public virtual ICollection<UserDiscountRedemption> Redemptions { get; set; } = new List<UserDiscountRedemption>();

    // Domain methods
    public bool CanRedeem(int userPoints)
    {
        if (!IsActive) return false;
        if (ExpiryDate.HasValue && ExpiryDate < DateTime.UtcNow) return false;
        if (userPoints < RequiredPoints) return false;
        if (MaxRedemptions > 0 && CurrentRedemptions >= MaxRedemptions) return false;
        return true;
    }

    public void IncrementRedemptions()
    {
        if (MaxRedemptions > 0 && CurrentRedemptions >= MaxRedemptions)
            throw new InvalidOperationException("Maximum redemptions reached");

        CurrentRedemptions++;
    }
}
