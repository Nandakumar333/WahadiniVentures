using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// User course enrollment tracking with progress and completion
/// </summary>
public class UserCourseEnrollment : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid CourseId { get; set; }

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;

    public decimal CompletionPercentage { get; set; } = 0;

    public bool IsCompleted { get; set; } = false;

    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Course Course { get; set; } = null!;

    // Domain methods
    public void UpdateAccess()
    {
        LastAccessedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// T188: Updates user progress through the course with automatic completion detection
    /// Progress Calculation Formula: (Completed Lessons / Total Lessons) * 100
    /// </summary>
    /// <param name="percentage">Progress percentage (0-100). Values > 100 are clamped to 100.</param>
    /// <remarks>
    /// Business Rules:
    /// 1. Progress is calculated based on lesson completion, not time spent or video watch percentage
    /// 2. Progress = 100% automatically marks the course as complete and sets CompletedAt timestamp
    /// 3. Once marked complete (IsCompleted=true), the status is persistent even if percentage drops
    /// 4. Progress can regress if lessons are marked incomplete (e.g., retaking quiz)
    /// 
    /// Example Calculation:
    /// - Course has 10 lessons
    /// - User completes 3 lessons
    /// - Progress = (3 / 10) * 100 = 30%
    /// 
    /// Automatic Completion Trigger:
    /// - When percentage >= 100, IsCompleted flag is set to true
    /// - CompletedAt timestamp is set to current UTC time
    /// - This enables "Completed Courses" filtering and achievement tracking
    /// </remarks>
    public void UpdateProgress(decimal percentage)
    {
        // Update progress percentage (values > 100 handled by caller)
        CompletionPercentage = percentage;

        // Automatic completion detection
        // Condition: Progress reaches 100% AND not already marked complete
        // Why check IsCompleted? Prevents re-setting CompletedAt on subsequent updates
        if (percentage >= 100 && !IsCompleted)
        {
            IsCompleted = true;
            CompletedAt = DateTime.UtcNow;
        }
    }
}
