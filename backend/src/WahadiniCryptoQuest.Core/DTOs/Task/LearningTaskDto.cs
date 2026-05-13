using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.DTOs.Task;

/// <summary>
/// Data transfer object for LearningTask entity
/// Used when returning task information to clients
/// </summary>
public record LearningTaskDto
{
    public Guid Id { get; init; }
    public Guid LessonId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public TaskType TaskType { get; init; }
    public string TaskData { get; init; } = "{}";
    public int RewardPoints { get; init; }
    public int? TimeLimit { get; init; }
    public int OrderIndex { get; init; }
    public bool IsRequired { get; init; }
}
