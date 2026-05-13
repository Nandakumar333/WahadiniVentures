using MediatR;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Service.Commands.Admin;

/// <summary>
/// Command to review a task submission (approve/reject)
/// T046: US2 - Task Review Workflow
/// </summary>
public class ReviewTaskCommand : IRequest<Unit>
{
    /// <summary>
    /// Task submission ID to review
    /// </summary>
    public Guid SubmissionId { get; set; }

    /// <summary>
    /// Review status (Approved or Rejected)
    /// </summary>
    public SubmissionStatus Status { get; set; }

    /// <summary>
    /// Admin feedback
    /// </summary>
    public string? Feedback { get; set; }

    /// <summary>
    /// ID of admin performing the review
    /// </summary>
    public Guid AdminUserId { get; set; }
}
