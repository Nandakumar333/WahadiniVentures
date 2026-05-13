using MediatR;

namespace WahadiniCryptoQuest.Service.Commands.Admin;

/// <summary>
/// Command to soft delete a course
/// T101: US4 - Course Content Management
/// </summary>
public class DeleteCourseCommand : IRequest<Unit>
{
    public Guid CourseId { get; set; }
    public Guid AdminUserId { get; set; }
}
