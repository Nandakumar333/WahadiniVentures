using System.ComponentModel.DataAnnotations;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Course entity representing crypto learning courses
/// </summary>
public class Course : SoftDeletableEntity
{
    public Guid CategoryId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Beginner;

    public int EstimatedDuration { get; set; } // in minutes

    public bool IsPremium { get; set; } = false;

    public int RewardPoints { get; set; } = 0;

    public bool IsPublished { get; set; } = false;

    public int ViewCount { get; set; } = 0;

    public Guid? CreatedByUserId { get; set; }

    // Navigation properties
    public virtual Category Category { get; set; } = null!;
    public virtual User? CreatedByUser { get; set; }
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    public virtual ICollection<UserCourseEnrollment> Enrollments { get; set; } = new List<UserCourseEnrollment>();

    // Domain methods

    /// <summary>
    /// T188: Publishes the course, making it visible to users
    /// Business Rule: Course must have at least one lesson to be published
    /// This ensures users can access meaningful content before a course goes live
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when attempting to publish a course with zero lessons</exception>
    public void Publish()
    {
        // Validation: Prevent publishing empty courses
        // Rationale: Users enrolling in a course expect immediate learning content
        if (!Lessons.Any())
            throw new InvalidOperationException("Cannot publish course without lessons");

        IsPublished = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;
    }
}
