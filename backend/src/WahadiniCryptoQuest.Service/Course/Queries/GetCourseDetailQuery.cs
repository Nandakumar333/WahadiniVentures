using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Course;

namespace WahadiniCryptoQuest.Service.Course.Queries;

/// <summary>
/// Query to get detailed course information including lessons
/// </summary>
public class GetCourseDetailQuery : IRequest<CourseDetailDto?>
{
    /// <summary>
    /// The ID of the course to retrieve
    /// </summary>
    public Guid CourseId { get; set; }

    /// <summary>
    /// Optional user ID to check enrollment status and progress
    /// </summary>
    public Guid? UserId { get; set; }

    public GetCourseDetailQuery(Guid courseId, Guid? userId = null)
    {
        CourseId = courseId;
        UserId = userId;
    }
}
