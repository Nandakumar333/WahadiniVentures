using Microsoft.Extensions.Configuration;
using WahadiniCryptoQuest.Core.DTOs.Reward;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Services;

/// <summary>
/// Service for managing user daily login streaks and bonus point rewards
/// </summary>
public class StreakService : IStreakService
{
    private readonly IUserStreakRepository _streakRepository;
    private readonly IRewardService _rewardService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public StreakService(
        IUserStreakRepository streakRepository,
        IRewardService rewardService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _streakRepository = streakRepository;
        _rewardService = rewardService;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    /// <summary>
    /// Processes a user's login to update streak tracking and award bonus points
    /// Implements UTC date-based streak logic with escalating milestone bonuses
    /// </summary>
    public async Task<StreakDto> ProcessLoginAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var streak = await _streakRepository.GetByUserIdAsync(userId, cancellationToken);

        // Create new streak record if user doesn't have one
        if (streak == null)
        {
            streak = UserStreak.Create(userId);
            await _streakRepository.AddAsync(streak);
        }

        var bonusPoints = 0;
        var lastLoginDate = streak.LastLoginDate.Date;

        // Calculate days since last login
        var daysSinceLastLogin = (today - lastLoginDate).Days;

        // Same day login - no streak update, no bonus
        if (daysSinceLastLogin == 0)
        {
            return new StreakDto
            {
                CurrentStreak = streak.CurrentStreak,
                LongestStreak = streak.LongestStreak,
                LastLoginDate = streak.LastLoginDate,
                BonusPointsAwarded = 0,
                NextMilestoneAt = CalculateNextMilestone(streak.CurrentStreak)
            };
        }

        // Consecutive day login - increment streak
        if (daysSinceLastLogin == 1)
        {
            streak.CurrentStreak++;
            streak.LastLoginDate = today;

            // Update longest streak if current exceeds it
            if (streak.CurrentStreak > streak.LongestStreak)
            {
                streak.LongestStreak = streak.CurrentStreak;
            }

            // Calculate and award bonus points
            bonusPoints = CalculateStreakBonus(streak.CurrentStreak);

            if (bonusPoints > 0)
            {
                await _rewardService.AwardPointsAsync(
                    userId,
                    bonusPoints,
                    TransactionType.DailyStreak,
                    $"Daily streak bonus - Day {streak.CurrentStreak}",
                    $"streak_{today:yyyyMMdd}",
                    "Streak",
                    null,
                    cancellationToken
                );
            }
        }
        // Streak broken - reset to 1
        else if (daysSinceLastLogin > 1)
        {
            streak.CurrentStreak = 1;
            streak.LastLoginDate = today;

            // Award base bonus for day 1
            bonusPoints = GetBaseBonus();

            if (bonusPoints > 0)
            {
                await _rewardService.AwardPointsAsync(
                    userId,
                    bonusPoints,
                    TransactionType.DailyStreak,
                    "Daily streak bonus - Day 1",
                    $"streak_{today:yyyyMMdd}",
                    "Streak",
                    null,
                    cancellationToken
                );
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new StreakDto
        {
            CurrentStreak = streak.CurrentStreak,
            LongestStreak = streak.LongestStreak,
            LastLoginDate = streak.LastLoginDate,
            BonusPointsAwarded = bonusPoints,
            NextMilestoneAt = CalculateNextMilestone(streak.CurrentStreak)
        };
    }

    /// <summary>
    /// Gets the current streak information for a user without processing login
    /// </summary>
    public async Task<StreakDto> GetUserStreakAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var streak = await _streakRepository.GetByUserIdAsync(userId, cancellationToken);

        if (streak == null)
        {
            // Return empty streak for new users
            return new StreakDto
            {
                CurrentStreak = 0,
                LongestStreak = 0,
                LastLoginDate = DateTime.UtcNow.Date.AddDays(-1),
                BonusPointsAwarded = 0,
                NextMilestoneAt = 5 // First milestone
            };
        }

        return new StreakDto
        {
            CurrentStreak = streak.CurrentStreak,
            LongestStreak = streak.LongestStreak,
            LastLoginDate = streak.LastLoginDate,
            BonusPointsAwarded = 0,
            NextMilestoneAt = CalculateNextMilestone(streak.CurrentStreak)
        };
    }

    /// <summary>
    /// Calculates streak bonus points based on current streak count
    /// Returns base bonus + milestone bonus if applicable
    /// </summary>
    private int CalculateStreakBonus(int currentStreak)
    {
        var baseBonus = GetBaseBonus();
        var milestoneBonus = GetMilestoneBonus(currentStreak);
        return baseBonus + milestoneBonus;
    }

    /// <summary>
    /// Gets the base bonus points awarded for any daily login
    /// </summary>
    private int GetBaseBonus()
    {
        return _configuration.GetValue<int>("RewardSettings:Streaks:BaseBonus", 5);
    }

    /// <summary>
    /// Gets milestone bonus if current streak matches a milestone threshold
    /// </summary>
    private int GetMilestoneBonus(int currentStreak)
    {
        var milestones = _configuration.GetSection("RewardSettings:Streaks:Milestones");
        var milestoneValue = milestones.GetValue<int>(currentStreak.ToString(), 0);
        return milestoneValue;
    }

    /// <summary>
    /// Calculates the next milestone threshold for progress display
    /// </summary>
    private int? CalculateNextMilestone(int currentStreak)
    {
        var milestoneDays = new[] { 5, 10, 30, 100 };
        return milestoneDays.FirstOrDefault(m => m > currentStreak);
    }
}
