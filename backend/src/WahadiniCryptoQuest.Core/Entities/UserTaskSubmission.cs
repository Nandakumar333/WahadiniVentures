using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// User task submission with flexible JSONB submission data
/// </summary>
public class UserTaskSubmission : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid TaskId { get; set; }

    /// <summary>
    /// JSONB column for flexible submission data structure
    /// Examples: Quiz answers, Screenshot URL, Wallet signature
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string SubmissionData { get; set; } = "{}";

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }

    public Guid? ReviewedByUserId { get; set; }

    public string? FeedbackText { get; set; }

    public int RewardPointsAwarded { get; set; } = 0;

    [Timestamp]
    public byte[] Version { get; set; } = Array.Empty<byte>();

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual LearningTask Task { get; set; } = null!;
    public virtual User? ReviewedBy { get; set; }

    // Domain methods
    public void Approve(Guid reviewerId, int points, string? feedback = null)
    {
        Status = SubmissionStatus.Approved;
        ReviewedAt = DateTime.UtcNow;
        ReviewedByUserId = reviewerId;
        RewardPointsAwarded = points;
        FeedbackText = feedback;
    }

    public void Reject(Guid reviewerId, string feedback)
    {
        if (string.IsNullOrWhiteSpace(feedback))
            throw new ArgumentException("Feedback required for rejection");

        Status = SubmissionStatus.Rejected;
        ReviewedAt = DateTime.UtcNow;
        ReviewedByUserId = reviewerId;
        FeedbackText = feedback;
    }
}
