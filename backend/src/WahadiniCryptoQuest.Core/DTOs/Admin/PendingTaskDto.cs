using System;

namespace WahadiniCryptoQuest.Core.DTOs.Admin;

/// <summary>
/// DTO for pending task submissions awaiting admin review
/// T042: US2 - Task Review Workflow
/// </summary>
public class PendingTaskDto
{
    /// <summary>
    /// Task submission ID
    /// </summary>
    public Guid SubmissionId { get; set; }

    /// <summary>
    /// User who submitted the task
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Username of submitter
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Task ID
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Task title
    /// </summary>
    public string TaskTitle { get; set; } = string.Empty;

    /// <summary>
    /// Course name the task belongs to
    /// </summary>
    public string CourseName { get; set; } = string.Empty;

    /// <summary>
    /// When the task was submitted
    /// </summary>
    public DateTime SubmittedAt { get; set; }

    /// <summary>
    /// Type of content submitted (Text/URL/File)
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Submission content/data
    /// </summary>
    public string SubmissionData { get; set; } = string.Empty;

    /// <summary>
    /// Points reward for this task
    /// </summary>
    public int PointReward { get; set; }
}
