using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.DTOs.Course;

/// <summary>
/// Data transfer object for enrolled courses with progress tracking
/// </summary>
public record EnrolledCourseDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public DifficultyLevel DifficultyLevel { get; init; }
    public bool IsPremium { get; init; }
    public string? ThumbnailUrl { get; init; }
    public int RewardPoints { get; init; }
    public int EstimatedDuration { get; init; }
    public decimal ProgressPercentage { get; init; }
    public CompletionStatus CompletionStatus { get; init; }
    public DateTime? LastAccessedDate { get; init; }
}
