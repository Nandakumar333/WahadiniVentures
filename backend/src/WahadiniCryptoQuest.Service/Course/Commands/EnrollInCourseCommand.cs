using MediatR;

namespace WahadiniCryptoQuest.Service.Course.Commands;

/// <summary>
/// Command to enroll a user in a course
/// </summary>
public class EnrollInCourseCommand : IRequest<bool>
{
    /// <summary>
    /// The ID of the course to enroll in
    /// </summary>
    public Guid CourseId { get; set; }

    /// <summary>
    /// The ID of the user enrolling
    /// </summary>
    public Guid UserId { get; set; }

    public EnrollInCourseCommand(Guid courseId, Guid userId)
    {
        CourseId = courseId;
        UserId = userId;
    }
}
