using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for ReferralAttribution entity with referral tracking operations
/// </summary>
public interface IReferralAttributionRepository : IRepository<ReferralAttribution>
{
    /// <summary>
    /// Gets a referral attribution by invitee user ID
    /// </summary>
    /// <param name="inviteeUserId">Invitee user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Referral attribution if exists, null otherwise</returns>
    Task<ReferralAttribution?> GetByInviteeUserIdAsync(Guid inviteeUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all referrals made by a specific user (inviter)
    /// </summary>
    /// <param name="inviterUserId">Inviter user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of referrals made by the user</returns>
    Task<IEnumerable<ReferralAttribution>> GetReferralsByInviterAsync(Guid inviterUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets successful referrals for a user (invitee has completed conversion event)
    /// </summary>
    /// <param name="inviterUserId">Inviter user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of successful referrals with conversion events</returns>
    Task<IEnumerable<ReferralAttribution>> GetSuccessfulReferralsAsync(Guid inviterUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets referral count for leaderboard (successful referrals only)
    /// </summary>
    /// <param name="inviterUserId">Inviter user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of successful referrals</returns>
    Task<int> GetSuccessfulReferralCountAsync(Guid inviterUserId, CancellationToken cancellationToken = default);
}
