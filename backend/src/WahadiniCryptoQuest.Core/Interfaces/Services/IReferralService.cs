using WahadiniCryptoQuest.Core.DTOs.Reward;

namespace WahadiniCryptoQuest.Core.Interfaces.Services;

/// <summary>
/// Service interface for referral tracking and validation
/// T084: Referral code validation and completion processing
/// </summary>
public interface IReferralService
{
    /// <summary>
    /// T086: Validates a referral code and returns inviter information
    /// </summary>
    /// <param name="referralCode">Referral code to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with inviter username if valid</returns>
    Task<(bool IsValid, string? InviterUsername)> ValidateReferralCodeAsync(string referralCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// T087: Processes referral completion (invitee completes first course)
    /// Awards bonus points to inviter and marks referral as fulfilled
    /// </summary>
    /// <param name="inviteeUserId">User who completed the qualifying action</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Points awarded to inviter (0 if no referral exists or already fulfilled)</returns>
    Task<int> ProcessReferralCompletionAsync(Guid inviteeUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets referral information for a user (their referral code and stats)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Referral code DTO with statistics</returns>
    Task<ReferralCodeDto> GetReferralInfoAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets referral history for a user (people they've referred)
    /// </summary>
    /// <param name="userId">User ID (inviter)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of referral records</returns>
    Task<IEnumerable<ReferralDto>> GetReferralHistoryAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a referral attribution when a new user signs up with a referral code
    /// </summary>
    /// <param name="inviteeUserId">New user ID</param>
    /// <param name="referralCode">Referral code used during signup</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CreateReferralAttributionAsync(Guid inviteeUserId, string referralCode, CancellationToken cancellationToken = default);
}
