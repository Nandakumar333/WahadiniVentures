using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.DTOs.Task;

/// <summary>
/// Data transfer object for user task submission status
/// Used to return submission details including review status and feedback
/// </summary>
public record TaskSubmissionStatusDto
{
    public Guid? SubmissionId { get; init; }
    public Guid TaskId { get; init; }
    public Guid UserId { get; init; }
    public SubmissionStatus? Status { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public string? FeedbackText { get; init; }
    public int RewardPointsAwarded { get; init; }
    public bool HasSubmitted { get; init; }
}
