using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Domain;
using WahadiniCryptoQuest.Core.DTOs.Reward;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Exceptions;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Services;

/// <summary>
/// Achievement service implementation for tracking and unlocking achievements
/// T079-T081: Achievement evaluation logic with AchievementCatalog integration and bonus point awarding
/// </summary>
public class AchievementService : IAchievementService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserAchievementRepository _achievementRepository;
    private readonly IRewardService _rewardService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AchievementService> _logger;

    public AchievementService(
        IUserRepository userRepository,
        IUserAchievementRepository achievementRepository,
        IRewardService rewardService,
        IUnitOfWork unitOfWork,
        ILogger<AchievementService> logger)
    {
        _userRepository = userRepository;
        _achievementRepository = achievementRepository;
        _rewardService = rewardService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// T080: Implements achievement evaluation logic using AchievementCatalog
    /// T081: Awards bonus points (0-250) for unlocked achievements
    /// </summary>
    public async Task<IEnumerable<string>> CheckAndUnlockAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking achievements for user {UserId}", userId);

        // Get user with related data
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {userId} not found");
        }

        // Get user's already unlocked achievements
        var unlockedAchievements = await _achievementRepository.GetUserAchievementsAsync(userId, cancellationToken);
        var unlockedIds = unlockedAchievements.Select(a => a.AchievementId).ToHashSet();

        // Build progress snapshot (you'll need to add these fields/calculations)
        var progress = new AchievementCatalog.UserAchievementProgress
        {
            LessonsCompleted = 0, // TODO: Get from user's completed lessons count
            TasksCompleted = 0,    // TODO: Get from user's completed tasks count
            CoursesCompleted = 0,  // TODO: Get from user's completed courses count
            TotalPointsEarned = user.TotalPointsEarned,
            CurrentStreak = 0,     // TODO: Get from UserStreak
            ReferralCount = 0      // TODO: Get from ReferralAttribution count
        };

        // Evaluate achievements
        var eligibleAchievements = AchievementCatalog.EvaluateUnlocks(progress);
        var newUnlocks = eligibleAchievements.Where(a => !unlockedIds.Contains(a.Id)).ToList();

        if (!newUnlocks.Any())
        {
            _logger.LogInformation("No new achievements to unlock for user {UserId}", userId);
            return Enumerable.Empty<string>();
        }

        var newlyUnlockedIds = new List<string>();

        using (var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken))
        {
            try
            {
                foreach (var achievement in newUnlocks)
                {
                    // Create unlock record
                    var userAchievement = UserAchievement.Create(userId, achievement.Id);
                    await _achievementRepository.AddAsync(userAchievement);

                    // Award bonus points if configured
                    if (achievement.BonusPoints > 0)
                    {
                        await _rewardService.AwardPointsAsync(
                            userId,
                            achievement.BonusPoints,
                            TransactionType.AchievementBonus,
                            $"Achievement unlocked: {achievement.Name}",
                            userAchievement.Id.ToString(),
                            "Achievement",
                            null,
                            cancellationToken
                        );

                        _logger.LogInformation(
                            "Awarded {Points} bonus points for achievement {AchievementId} to user {UserId}",
                            achievement.BonusPoints, achievement.Id, userId);
                    }

                    newlyUnlockedIds.Add(achievement.Id);

                    _logger.LogInformation(
                        "Unlocked achievement {AchievementId} ({AchievementName}) for user {UserId}",
                        achievement.Id, achievement.Name, userId);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Successfully unlocked {Count} achievements for user {UserId}",
                    newlyUnlockedIds.Count, userId);

                return newlyUnlockedIds;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to unlock achievements for user {UserId}", userId);
                throw;
            }
        }
    }

    /// <summary>
    /// Gets all achievements with unlock status and progress calculation
    /// </summary>
    public async Task<IEnumerable<AchievementDto>> GetUserAchievementsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {userId} not found");
        }

        var unlockedAchievements = await _achievementRepository.GetUserAchievementsAsync(userId, cancellationToken);
        var unlockedDict = unlockedAchievements.ToDictionary(a => a.AchievementId);

        // Build progress snapshot for progress calculation
        var progress = new AchievementCatalog.UserAchievementProgress
        {
            LessonsCompleted = 0, // TODO: Get from user's completed lessons count
            TasksCompleted = 0,
            CoursesCompleted = 0,
            TotalPointsEarned = user.TotalPointsEarned,
            CurrentStreak = 0,
            ReferralCount = 0
        };

        var allAchievements = AchievementCatalog.GetAll();
        var achievementDtos = new List<AchievementDto>();

        foreach (var achievement in allAchievements)
        {
            var isUnlocked = unlockedDict.ContainsKey(achievement.Id);
            var unlockedAt = isUnlocked ? unlockedDict[achievement.Id].UnlockedAt : (DateTime?)null;
            var notified = isUnlocked && unlockedDict[achievement.Id].Notified;

            // Calculate progress percentage (0-100)
            var progressPercentage = CalculateProgress(achievement, progress);

            achievementDtos.Add(new AchievementDto
            {
                Id = achievement.Id,
                Name = achievement.Name,
                Description = achievement.Description,
                Category = "General", // TODO: Add category to AchievementDefinition
                IconUrl = achievement.Icon,
                PointBonus = achievement.BonusPoints,
                IsUnlocked = isUnlocked,
                UnlockedAt = unlockedAt,
                Progress = progressPercentage,
                IsNotified = notified,
                DisplayOrder = achievement.DisplayOrder
            });
        }

        return achievementDtos.OrderBy(a => a.DisplayOrder).ThenBy(a => a.Name);
    }

    /// <summary>
    /// Gets achievements that user hasn't been notified about
    /// </summary>
    public async Task<IEnumerable<AchievementDto>> GetUnnotifiedAchievementsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unnotified = await _achievementRepository.GetUnnotifiedAchievementsAsync(userId, cancellationToken);

        return unnotified.Select(ua =>
        {
            var definition = AchievementCatalog.GetById(ua.AchievementId);
            return new AchievementDto
            {
                Id = ua.AchievementId,
                Name = definition?.Name ?? "Unknown",
                Description = definition?.Description ?? "",
                Category = "General",
                IconUrl = definition?.Icon ?? "🏆",
                PointBonus = definition?.BonusPoints ?? 0,
                IsUnlocked = true,
                UnlockedAt = ua.UnlockedAt,
                Progress = 100,
                IsNotified = false,
                DisplayOrder = definition?.DisplayOrder ?? 999
            };
        });
    }

    /// <summary>
    /// Marks achievements as notified
    /// </summary>
    public async Task MarkAsNotifiedAsync(Guid userId, IEnumerable<string> achievementIds, CancellationToken cancellationToken = default)
    {
        await _achievementRepository.MarkAsNotifiedAsync(userId, achievementIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Marked {Count} achievements as notified for user {UserId}",
            achievementIds.Count(), userId);
    }

    /// <summary>
    /// Calculate progress percentage (0-100) for an achievement
    /// </summary>
    private int CalculateProgress(AchievementCatalog.AchievementDefinition achievement, AchievementCatalog.UserAchievementProgress progress)
    {
        // Simple heuristic: if criteria is met, 100%, otherwise estimate based on known thresholds
        if (achievement.UnlockCriteria(progress))
        {
            return 100;
        }

        // For specific achievements, calculate partial progress
        // This is a simplified implementation - you may want more sophisticated logic
        return achievement.Id switch
        {
            "first-steps" => Math.Min(progress.LessonsCompleted * 100, 100),
            "task-master" => Math.Min(progress.TasksCompleted * 100, 100),
            "course-champion" => Math.Min(progress.CoursesCompleted * 100, 100),
            "triple-threat" => Math.Min((progress.CoursesCompleted * 100) / 3, 100),
            "point-hoarder" => Math.Min((progress.TotalPointsEarned * 100) / 5000, 100),
            "streak-warrior" => Math.Min((progress.CurrentStreak * 100) / 7, 100),
            "social-butterfly" => Math.Min((progress.ReferralCount * 100) / 3, 100),
            "century-club" => Math.Min(progress.LessonsCompleted, 100),
            _ => 0
        };
    }
}
