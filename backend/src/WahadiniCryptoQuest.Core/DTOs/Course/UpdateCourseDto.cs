using System.ComponentModel.DataAnnotations;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.DTOs.Course;

/// <summary>
/// Data transfer object for course updates
/// </summary>
public record UpdateCourseDto
{
    [Required(ErrorMessage = "Course ID is required")]
    public Guid Id { get; init; }

    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; init; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public Guid CategoryId { get; init; }

    public DifficultyLevel DifficultyLevel { get; init; } = DifficultyLevel.Beginner;

    [MaxLength(500, ErrorMessage = "Thumbnail URL cannot exceed 500 characters")]
    public string? ThumbnailUrl { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Estimated duration must be greater than 0")]
    public int EstimatedDuration { get; init; }

    public bool IsPremium { get; init; } = false;

    [Range(0, int.MaxValue, ErrorMessage = "Reward points cannot be negative")]
    public int RewardPoints { get; init; } = 0;

    public bool IsPublished { get; init; } = false;
}
