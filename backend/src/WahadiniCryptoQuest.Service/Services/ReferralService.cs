using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Reward;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Exceptions;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Services;

/// <summary>
/// T085: Referral service implementation for code validation and completion processing
/// </summary>
public class ReferralService : IReferralService
{
    private readonly IUserRepository _userRepository;
    private readonly IReferralAttributionRepository _referralRepository;
    private readonly IRewardService _rewardService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReferralService> _logger;

    private const int DefaultReferralBonus = 200; // Default from spec

    public ReferralService(
        IUserRepository userRepository,
        IReferralAttributionRepository referralRepository,
        IRewardService rewardService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<ReferralService> logger)
    {
        _userRepository = userRepository;
        _referralRepository = referralRepository;
        _rewardService = rewardService;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// T086: Validates referral code and returns inviter information
    /// </summary>
    public async Task<(bool IsValid, string? InviterUsername)> ValidateReferralCodeAsync(
        string referralCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(referralCode))
        {
            return (false, null);
        }

        // Find user with this referral code
        var inviter = await _userRepository.GetByReferralCodeAsync(referralCode, cancellationToken);

        if (inviter == null)
        {
            _logger.LogWarning("Invalid referral code attempted: {ReferralCode}", referralCode);
            return (false, null);
        }

        _logger.LogInformation("Valid referral code {ReferralCode} for user {InviterEmail}",
            referralCode, inviter.Email);

        return (true, inviter.FullName);
    }

    /// <summary>
    /// T087: Processes referral completion (invitee completes first course)
    /// Awards bonus points to inviter
    /// </summary>
    public async Task<int> ProcessReferralCompletionAsync(
        Guid inviteeUserId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing referral completion for invitee {InviteeUserId}", inviteeUserId);

        // Find referral attribution
        var referral = await _referralRepository.GetByInviteeUserIdAsync(inviteeUserId, cancellationToken);

        if (referral == null)
        {
            _logger.LogInformation("No referral attribution found for user {InviteeUserId}", inviteeUserId);
            return 0;
        }

        if (referral.RewardClaimed)
        {
            _logger.LogInformation("Referral {ReferralId} already claimed", referral.Id);
            return 0;
        }

        var bonusPoints = _configuration.GetValue<int>("RewardSettings:ReferralBonus", DefaultReferralBonus);

        using (var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken))
        {
            try
            {
                // Award points to inviter
                await _rewardService.AwardPointsAsync(
                    referral.ReferrerId,
                    bonusPoints,
                    TransactionType.ReferralBonus,
                    $"Referral bonus: {referral.ReferredUserId} completed first course",
                    referral.Id.ToString(),
                    "Referral",
                    null,
                    cancellationToken
                );

                // Mark referral as fulfilled
                referral.RewardClaimed = true;
                referral.RewardClaimedAt = DateTime.UtcNow;
                await _referralRepository.UpdateAsync(referral);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Referral {ReferralId} completed: Awarded {Points} points to inviter {InviterId}",
                    referral.Id, bonusPoints, referral.ReferrerId);

                return bonusPoints;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to process referral completion for invitee {InviteeUserId}", inviteeUserId);
                throw;
            }
        }
    }

    /// <summary>
    /// Gets referral information for a user
    /// </summary>
    public async Task<ReferralCodeDto> GetReferralInfoAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {userId} not found");
        }

        var referrals = await _referralRepository.GetReferralsByInviterAsync(userId, cancellationToken);
        var successfulReferrals = await _referralRepository.GetSuccessfulReferralCountAsync(userId, cancellationToken);

        // Calculate total points earned from referrals
        var totalPointsEarned = successfulReferrals * _configuration.GetValue<int>("RewardSettings:ReferralBonus", DefaultReferralBonus);

        return new ReferralCodeDto
        {
            ReferralCode = user.ReferralCode ?? string.Empty, // Handle null referral code
            SuccessfulReferrals = successfulReferrals,
            TotalPointsEarned = totalPointsEarned
        };
    }

    /// <summary>
    /// Gets referral history for a user
    /// </summary>
    public async Task<IEnumerable<ReferralDto>> GetReferralHistoryAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var referrals = await _referralRepository.GetReferralsByInviterAsync(userId, cancellationToken);
        var bonusPoints = _configuration.GetValue<int>("RewardSettings:ReferralBonus", DefaultReferralBonus);

        return referrals.Select(r => new ReferralDto
        {
            Id = r.Id,
            ReferrerId = r.ReferrerId,
            ReferredUserId = r.ReferredUserId,
            ReferredUserName = r.ReferredUser?.FullName ?? "Unknown",
            Status = r.RewardClaimed ? "Completed" : "Pending",
            CreatedAt = r.ReferredAt,
            CompletedAt = r.RewardClaimedAt,
            PointsAwarded = r.RewardClaimed ? bonusPoints : 0
        });
    }

    /// <summary>
    /// Creates referral attribution when user signs up with referral code
    /// </summary>
    public async Task CreateReferralAttributionAsync(
        Guid inviteeUserId,
        string referralCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(referralCode))
        {
            return;
        }

        // Validate referral code
        var (isValid, _) = await ValidateReferralCodeAsync(referralCode, cancellationToken);
        if (!isValid)
        {
            _logger.LogWarning("Attempted to create referral with invalid code: {ReferralCode}", referralCode);
            return;
        }

        // Get inviter user
        var inviter = await _userRepository.GetByReferralCodeAsync(referralCode, cancellationToken);
        if (inviter == null || inviter.Id == inviteeUserId)
        {
            _logger.LogWarning("Self-referral or invalid inviter for code: {ReferralCode}", referralCode);
            return;
        }

        // Check if invitee already has a referral attribution
        var existing = await _referralRepository.GetByInviteeUserIdAsync(inviteeUserId, cancellationToken);
        if (existing != null)
        {
            _logger.LogInformation("User {InviteeUserId} already has a referral attribution", inviteeUserId);
            return;
        }

        // Create referral attribution
        var referral = ReferralAttribution.Create(inviter.Id, inviteeUserId);
        await _referralRepository.AddAsync(referral);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created referral attribution: Inviter {InviterId}, Invitee {InviteeUserId}, Code {ReferralCode}",
            inviter.Id, inviteeUserId, referralCode);
    }
}
