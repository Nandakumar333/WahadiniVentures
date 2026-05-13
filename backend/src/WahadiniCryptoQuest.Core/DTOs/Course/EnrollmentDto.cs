namespace WahadiniCryptoQuest.Core.DTOs.Course;

/// <summary>
/// Data transfer object for course enrollment
/// </summary>
public record EnrollmentDto
{
    public Guid CourseId { get; init; }
    public Guid UserId { get; init; }
    public DateTime EnrolledAt { get; init; }
    public decimal CompletionPercentage { get; init; }
    public bool IsCompleted { get; init; }
}
