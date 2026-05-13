using System.ComponentModel.DataAnnotations;
using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Core.Entities;

public class ReferralAttribution : BaseEntity
{
    public Guid ReferrerId { get; set; }
    public Guid ReferredUserId { get; set; }
    
    public DateTime ReferredAt { get; set; }
    
    public bool RewardClaimed { get; set; }
    public DateTime? RewardClaimedAt { get; set; }

    public virtual User Referrer { get; set; } = null!;
    public virtual User ReferredUser { get; set; } = null!;

    public static ReferralAttribution Create(Guid referrerId, Guid referredUserId)
    {
        return new ReferralAttribution
        {
            ReferrerId = referrerId,
            ReferredUserId = referredUserId,
            ReferredAt = DateTime.UtcNow,
            RewardClaimed = false
        };
    }
}
