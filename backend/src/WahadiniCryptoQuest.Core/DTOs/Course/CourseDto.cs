using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.DTOs.Course;

/// <summary>
/// Course data transfer object for list views
/// </summary>
public record CourseDto
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
    public int ViewCount { get; init; }
}
