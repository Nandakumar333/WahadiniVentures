namespace WahadiniCryptoQuest.Core.Domain;

/// <summary>
/// Static catalog of all available achievements with unlock criteria and rewards
/// MVP: 8 achievements as per specification
/// </summary>
public static class AchievementCatalog
{
    public class AchievementDefinition
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public int BonusPoints { get; init; }
        public Func<UserAchievementProgress, bool> UnlockCriteria { get; init; } = _ => false;
        public string Icon { get; init; } = "🏆";
        public int DisplayOrder { get; init; }
    }

    public class UserAchievementProgress
    {
        public int LessonsCompleted { get; set; }
        public int TasksCompleted { get; set; }
        public int CoursesCompleted { get; set; }
        public int TotalPointsEarned { get; set; }
        public int CurrentStreak { get; set; }
        public int ReferralCount { get; set; }
    }

    /// <summary>
    /// MVP Achievement Definitions (8 achievements as per spec)
    /// </summary>
    public static readonly IReadOnlyList<AchievementDefinition> AllAchievements = new List<AchievementDefinition>
    {
        new AchievementDefinition
        {
            Id = "first-steps",
            Name = "First Steps",
            Description = "Complete your first lesson",
            BonusPoints = 0,
            Icon = "🎯",
            DisplayOrder = 1,
            UnlockCriteria = progress => progress.LessonsCompleted >= 1
        },
        new AchievementDefinition
        {
            Id = "task-master",
            Name = "Task Master",
            Description = "Complete your first task",
            BonusPoints = 0,
            Icon = "✅",
            DisplayOrder = 2,
            UnlockCriteria = progress => progress.TasksCompleted >= 1
        },
        new AchievementDefinition
        {
            Id = "course-champion",
            Name = "Course Champion",
            Description = "Complete your first course",
            BonusPoints = 50,
            Icon = "🎓",
            DisplayOrder = 3,
            UnlockCriteria = progress => progress.CoursesCompleted >= 1
        },
        new AchievementDefinition
        {
            Id = "triple-threat",
            Name = "Triple Threat",
            Description = "Complete 3 courses",
            BonusPoints = 100,
            Icon = "🔥",
            DisplayOrder = 4,
            UnlockCriteria = progress => progress.CoursesCompleted >= 3
        },
        new AchievementDefinition
        {
            Id = "point-hoarder",
            Name = "Point Hoarder",
            Description = "Earn 5,000 total points",
            BonusPoints = 0,
            Icon = "💎",
            DisplayOrder = 5,
            UnlockCriteria = progress => progress.TotalPointsEarned >= 5000
        },
        new AchievementDefinition
        {
            Id = "streak-warrior",
            Name = "Streak Warrior",
            Description = "Maintain a 7-day login streak",
            BonusPoints = 25,
            Icon = "⚡",
            DisplayOrder = 6,
            UnlockCriteria = progress => progress.CurrentStreak >= 7
        },
        new AchievementDefinition
        {
            Id = "social-butterfly",
            Name = "Social Butterfly",
            Description = "Refer 3 successful users",
            BonusPoints = 150,
            Icon = "🦋",
            DisplayOrder = 7,
            UnlockCriteria = progress => progress.ReferralCount >= 3
        },
        new AchievementDefinition
        {
            Id = "century-club",
            Name = "Century Club",
            Description = "Complete 100 lessons",
            BonusPoints = 250,
            Icon = "💯",
            DisplayOrder = 8,
            UnlockCriteria = progress => progress.LessonsCompleted >= 100
        }
    };

    /// <summary>
    /// Get achievement by ID
    /// </summary>
    public static AchievementDefinition? GetById(string achievementId)
    {
        return AllAchievements.FirstOrDefault(a => a.Id == achievementId);
    }

    /// <summary>
    /// Get all achievements ordered by display order
    /// </summary>
    public static IEnumerable<AchievementDefinition> GetAll()
    {
        return AllAchievements.OrderBy(a => a.DisplayOrder);
    }

    /// <summary>
    /// Evaluate which achievements a user should unlock based on their progress
    /// </summary>
    public static IEnumerable<AchievementDefinition> EvaluateUnlocks(UserAchievementProgress progress)
    {
        return AllAchievements.Where(achievement => achievement.UnlockCriteria(progress));
    }
}
