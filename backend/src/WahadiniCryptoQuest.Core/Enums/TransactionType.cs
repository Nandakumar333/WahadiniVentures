namespace WahadiniCryptoQuest.Core.Enums;

/// <summary>
/// Types of reward transactions
/// </summary>
public enum TransactionType
{
    LessonCompletion,    // Points earned from completing a lesson (50 points)
    TaskApproval,        // Points earned from task approval (100 points)
    CourseCompletion,    // Points earned from completing a course (500 points)
    DailyStreak,         // Points from daily login streak (5+ points based on streak)
    ReferralBonus,       // Points from successful referral (200 points)
    AchievementBonus,    // Bonus points from unlocking achievements (0-250 points)
    AdminBonus,          // Admin-awarded bonus points (variable)
    AdminPenalty,        // Admin penalty points (negative, variable)
    Redemption           // Points redeemed for discounts/rewards (negative)
}
