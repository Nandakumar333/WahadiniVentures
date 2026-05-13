using System.ComponentModel.DataAnnotations;
using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Tracks user's daily login streaks for bonus point calculations
/// </summary>
public class UserStreak : BaseEntity
{
    public Guid UserId { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }

    /// <summary>
    /// DATE type (UTC date only, no time) for last login date
    /// PostgreSQL DATE type enables efficient date-based queries
    /// </summary>
    public DateTime LastLoginDate { get; set; }
    public DateTime? LastStreakFreezeUsedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public static UserStreak Create(Guid userId)
    {
        return new UserStreak
        {
            UserId = userId,
            CurrentStreak = 0,
            LongestStreak = 0,
            LastLoginDate = DateTime.UtcNow.Date.AddDays(-1) // Initialize as yesterday so first login counts
        };
    }
}
