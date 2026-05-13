using System.ComponentModel.DataAnnotations;

namespace WahadiniCryptoQuest.Core.DTOs.Course;

/// <summary>
/// Data transfer object for lesson updates
/// </summary>
public record UpdateLessonDto
{
    [Required(ErrorMessage = "Lesson ID is required")]
    public Guid Id { get; init; }

    [Required(ErrorMessage = "Course ID is required")]
    public Guid CourseId { get; init; }

    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; init; } = string.Empty;

    [Required(ErrorMessage = "YouTube Video ID is required")]
    [RegularExpression(@"^[a-zA-Z0-9_-]{11}$", ErrorMessage = "YouTube Video ID must be exactly 11 alphanumeric characters")]
    public string YouTubeVideoId { get; init; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than 0")]
    public int Duration { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Order index must be greater than 0")]
    public int OrderIndex { get; init; }

    public bool IsPremium { get; init; } = false;

    [Range(0, int.MaxValue, ErrorMessage = "Reward points cannot be negative")]
    public int RewardPoints { get; init; } = 0;

    public string? ContentMarkdown { get; init; }
}
