using MediatR;

namespace WahadiniCryptoQuest.Service.Course.Commands;

/// <summary>
/// Command to publish a course
/// Business rule: Course must have at least one lesson to be published
/// </summary>
public class PublishCourseCommand : IRequest<bool>
{
    /// <summary>
    /// Course ID to publish
    /// </summary>
    public Guid CourseId { get; set; }

    /// <summary>
    /// ID of the admin user publishing the course
    /// </summary>
    public Guid PublishedByUserId { get; set; }

    public PublishCourseCommand(Guid courseId, Guid publishedByUserId)
    {
        CourseId = courseId;
        PublishedByUserId = publishedByUserId;
    }
}
