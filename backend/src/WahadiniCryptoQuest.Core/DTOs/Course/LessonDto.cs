using WahadiniCryptoQuest.Core.DTOs.Task;

namespace WahadiniCryptoQuest.Core.DTOs.Course;

/// <summary>
/// Lesson data transfer object
/// </summary>
public record LessonDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string YouTubeVideoId { get; init; } = string.Empty;
    public int Duration { get; init; }
    public int OrderIndex { get; init; }
    public bool IsPremium { get; init; }
    public int RewardPoints { get; init; }
    public string? ContentMarkdown { get; init; }
    public List<LearningTaskDto>? Tasks { get; init; }
}
