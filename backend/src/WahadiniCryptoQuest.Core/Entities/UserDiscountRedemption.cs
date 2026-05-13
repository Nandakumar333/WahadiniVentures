using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Tracks discount code redemptions by users
/// </summary>
public class UserDiscountRedemption : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid DiscountCodeId { get; set; }

    public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;

    public bool UsedInSubscription { get; set; } = false;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual DiscountCode DiscountCode { get; set; } = null!;
}
