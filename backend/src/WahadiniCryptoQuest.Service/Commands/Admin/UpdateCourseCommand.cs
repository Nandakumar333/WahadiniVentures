using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Admin;

namespace WahadiniCryptoQuest.Service.Commands.Admin;

/// <summary>
/// Command to update an existing course
/// T099: US4 - Course Content Management
/// </summary>
public class UpdateCourseCommand : IRequest<Unit>
{
    public Guid CourseId { get; set; }
    public CourseFormDto CourseData { get; set; } = null!;
    public Guid AdminUserId { get; set; }
}
