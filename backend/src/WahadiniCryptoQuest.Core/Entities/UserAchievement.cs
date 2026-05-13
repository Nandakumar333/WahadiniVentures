using System.ComponentModel.DataAnnotations;
using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Core.Entities;

public class UserAchievement : BaseEntity
{
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string AchievementId { get; set; } = string.Empty; // From AchievementCatalog

    public DateTime UnlockedAt { get; set; }

    // For progressive achievements (e.g., "Complete 50 lessons")
    public int Progress { get; set; }
    public bool IsUnlocked { get; set; }

    // Whether user has been notified about this achievement unlock
    public bool Notified { get; set; }

    public virtual User User { get; set; } = null!;

    public static UserAchievement Create(Guid userId, string achievementId)
    {
        return new UserAchievement
        {
            UserId = userId,
            AchievementId = achievementId,
            UnlockedAt = DateTime.UtcNow,
            IsUnlocked = true,
            Progress = 100,
            Notified = false
        };
    }

    public void MarkAsNotified()
    {
        Notified = true;
    }
}
