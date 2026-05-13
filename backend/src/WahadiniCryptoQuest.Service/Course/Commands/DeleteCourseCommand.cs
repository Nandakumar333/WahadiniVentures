using MediatR;

namespace WahadiniCryptoQuest.Service.Course.Commands;

/// <summary>
/// Command to soft delete a course
/// </summary>
public class DeleteCourseCommand : IRequest<bool>
{
    /// <summary>
    /// Course ID to delete
    /// </summary>
    public Guid CourseId { get; set; }

    /// <summary>
    /// ID of the admin user performing the deletion
    /// </summary>
    public Guid DeletedByUserId { get; set; }

    public DeleteCourseCommand(Guid courseId, Guid deletedByUserId)
    {
        CourseId = courseId;
        DeletedByUserId = deletedByUserId;
    }
}
