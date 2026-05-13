using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.DTOs.Admin;

/// <summary>
/// DTO for admin task review request
/// T043: US2 - Task Review Workflow
/// </summary>
public class TaskReviewRequestDto
{
    /// <summary>
    /// Review status (Approved or Rejected)
    /// </summary>
    public SubmissionStatus Status { get; set; }

    /// <summary>
    /// Admin feedback for the submission
    /// Required if rejected, optional if approved
    /// </summary>
    public string? Feedback { get; set; }
}
