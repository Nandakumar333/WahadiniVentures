using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Service.Course.Queries;

/// <summary>
/// Query for getting user's enrolled courses with optional completion status filter
/// </summary>
public class GetUserCoursesQuery : IRequest<IEnumerable<EnrolledCourseDto>>
{
    /// <summary>
    /// User ID whose enrolled courses to retrieve
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Optional filter by completion status (NotStarted, InProgress, Completed)
    /// </summary>
    public CompletionStatus? Status { get; set; }
}
