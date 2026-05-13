using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Service.Course.Commands;

/// <summary>
/// Command to create a new course
/// </summary>
public class CreateCourseCommand : IRequest<CourseDto>
{
    /// <summary>
    /// Course title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Course description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Course content in markdown format
    /// </summary>
    public string? ContentMarkdown { get; set; }

    /// <summary>
    /// Category ID
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Difficulty level
    /// </summary>
    public DifficultyLevel DifficultyLevel { get; set; }

    /// <summary>
    /// Whether the course is premium
    /// </summary>
    public bool IsPremium { get; set; }

    /// <summary>
    /// Thumbnail URL
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Reward points for completing the course
    /// </summary>
    public int RewardPoints { get; set; }

    /// <summary>
    /// Estimated duration in minutes
    /// </summary>
    public int EstimatedDurationMinutes { get; set; }

    /// <summary>
    /// ID of the user creating the course (admin)
    /// </summary>
    public Guid CreatedByUserId { get; set; }
}
