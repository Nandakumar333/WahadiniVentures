using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Repository implementation for ReferralAttribution entity
/// </summary>
public class ReferralAttributionRepository : Repository<ReferralAttribution>, IReferralAttributionRepository
{
    public ReferralAttributionRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets a referral attribution by invitee user ID
    /// </summary>
    public async Task<ReferralAttribution?> GetByInviteeUserIdAsync(Guid inviteeUserId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ReferredUserId == inviteeUserId, cancellationToken);
    }

    /// <summary>
    /// Gets all referrals made by a specific user (inviter)
    /// </summary>
    public async Task<IEnumerable<ReferralAttribution>> GetReferralsByInviterAsync(Guid inviterUserId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.ReferrerId == inviterUserId)
            .OrderByDescending(r => r.ReferredAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets successful referrals for a user (invitee has completed conversion event)
    /// </summary>
    public async Task<IEnumerable<ReferralAttribution>> GetSuccessfulReferralsAsync(Guid inviterUserId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.ReferrerId == inviterUserId && r.RewardClaimed)
            .OrderByDescending(r => r.RewardClaimedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets referral count for leaderboard (successful referrals only)
    /// </summary>
    public async Task<int> GetSuccessfulReferralCountAsync(Guid inviterUserId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .CountAsync(r => r.ReferrerId == inviterUserId && r.RewardClaimed, cancellationToken);
    }
}
