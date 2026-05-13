using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.DTOs.Task;

public class TaskSubmissionRequest
{
    public Guid TaskId { get; set; }
    public string SubmissionData { get; set; } = "{}"; // JSON string
    public TaskType TaskType { get; set; }
    public string? Notes { get; set; }
}

public class TaskSubmissionResponseDto
{
    public Guid SubmissionId { get; set; }
    public SubmissionStatus Status { get; set; }
    public int PointsAwarded { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class UserTaskSubmissionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TaskId { get; set; }
    public string SubmissionData { get; set; } = "{}";
    public SubmissionStatus Status { get; set; }
    public string? FeedbackText { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int RewardPointsAwarded { get; set; }
    public LearningTaskDto? Task { get; set; }
}
