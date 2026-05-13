using System;
using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Represents the completion record of a lesson by a user
/// Immutable audit record for completion events
/// </summary>
public class LessonCompletion : IEntity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid LessonId { get; private set; }
    public DateTime CompletedAt { get; private set; }
    public int PointsAwarded { get; private set; }
    public decimal CompletionPercentage { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public virtual User? User { get; set; }
    public virtual Lesson? Lesson { get; set; }

    // Private constructor for EF Core
    private LessonCompletion() { }

    /// <summary>
    /// Factory method to create lesson completion record
    /// </summary>
    public static LessonCompletion Create(
        Guid userId,
        Guid lessonId,
        int pointsAwarded,
        decimal completionPercentage)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (lessonId == Guid.Empty)
            throw new ArgumentException("Lesson ID cannot be empty", nameof(lessonId));

        if (pointsAwarded < 0)
            throw new ArgumentException("Points awarded cannot be negative", nameof(pointsAwarded));

        if (completionPercentage < 0 || completionPercentage > 100)
            throw new ArgumentException("Completion percentage must be between 0 and 100", nameof(completionPercentage));

        var now = DateTime.UtcNow;

        return new LessonCompletion
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LessonId = lessonId,
            CompletedAt = now,
            PointsAwarded = pointsAwarded,
            CompletionPercentage = completionPercentage,
            CreatedAt = now
        };
    }
}
