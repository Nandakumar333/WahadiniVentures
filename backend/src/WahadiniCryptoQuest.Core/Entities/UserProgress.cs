using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Represents a user's progress through a specific lesson video
/// Implements highest-position tracking for skip-ahead behavior
/// </summary>
public class UserProgress : BaseEntity
{
    public Guid UserId { get; private set; }

    public Guid LessonId { get; private set; }

    /// <summary>
    /// Last watched position in seconds (highest position reached)
    /// </summary>
    public int LastWatchedPosition { get; private set; }

    /// <summary>
    /// Legacy field: Use LastWatchedPosition instead
    /// </summary>
    public int VideoWatchTimeSeconds { get; private set; }

    /// <summary>
    /// Completion percentage (0-100)
    /// </summary>
    public decimal CompletionPercentage { get; private set; }

    /// <summary>
    /// Total accumulated watch time in seconds for analytics
    /// </summary>
    public int TotalWatchTime { get; private set; }

    public bool IsCompleted { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public bool RewardPointsClaimed { get; private set; }

    public DateTime LastUpdatedAt { get; private set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Lesson Lesson { get; set; } = null!;

    // Private constructor for EF Core
    private UserProgress() { }

    /// <summary>
    /// Factory method to create new progress tracking
    /// </summary>
    public static UserProgress Create(Guid userId, Guid lessonId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (lessonId == Guid.Empty)
            throw new ArgumentException("Lesson ID cannot be empty", nameof(lessonId));

        var now = DateTime.UtcNow;

        return new UserProgress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LessonId = lessonId,
            LastWatchedPosition = 0,
            VideoWatchTimeSeconds = 0,
            CompletionPercentage = 0,
            TotalWatchTime = 0,
            IsCompleted = false,
            CompletedAt = null,
            RewardPointsClaimed = false,
            CreatedAt = now,
            UpdatedAt = now,
            LastUpdatedAt = now
        };
    }

    /// <summary>
    /// Updates watch position using highest-position tracking logic
    /// Forward seeks update position, backward seeks do not
    /// </summary>
    public void UpdatePosition(int position, int totalDurationSeconds)
    {
        if (position < 0)
            throw new ArgumentException("Position cannot be negative", nameof(position));

        if (totalDurationSeconds <= 0)
            throw new ArgumentException("Total duration must be positive", nameof(totalDurationSeconds));

        // Only update if new position is higher (forward seek)
        if (position > LastWatchedPosition)
        {
            LastWatchedPosition = Math.Min(position, totalDurationSeconds);
            VideoWatchTimeSeconds = LastWatchedPosition; // Keep legacy field in sync
        }

        // Calculate completion percentage
        CompletionPercentage = Math.Min(100m, (decimal)LastWatchedPosition / totalDurationSeconds * 100);

        // Increment watch time (5-second interval)
        TotalWatchTime += 5;

        LastUpdatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Legacy method for compatibility
    /// </summary>
    public void UpdateProgress(int position, decimal percentage)
    {
        VideoWatchTimeSeconds = position;
        LastWatchedPosition = position;
        CompletionPercentage = percentage;
        LastUpdatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        if (percentage >= 100 && !IsCompleted)
        {
            IsCompleted = true;
            CompletedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Marks lesson as complete (80% threshold reached)
    /// </summary>
    public void MarkComplete(DateTime completedAt)
    {
        if (IsCompleted)
            return; // Already completed

        if (CompletionPercentage < 80)
            throw new InvalidOperationException("Cannot mark complete: watch percentage below 80% threshold");

        IsCompleted = true;
        CompletedAt = completedAt;
        LastUpdatedAt = completedAt;
        UpdatedAt = completedAt;
    }

    /// <summary>
    /// Marks reward points as claimed (prevents duplicate awards)
    /// </summary>
    public void ClaimRewardPoints()
    {
        if (!IsCompleted)
            throw new InvalidOperationException("Cannot claim points: lesson not completed");

        if (RewardPointsClaimed)
            return; // Already claimed

        RewardPointsClaimed = true;
        LastUpdatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Legacy method for compatibility
    /// </summary>
    public void ClaimRewards()
    {
        ClaimRewardPoints();
    }
}
