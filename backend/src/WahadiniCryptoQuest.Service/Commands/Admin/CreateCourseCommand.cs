using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Admin;

namespace WahadiniCryptoQuest.Service.Commands.Admin;

/// <summary>
/// Command to create a new course
/// T097: US4 - Course Content Management
/// </summary>
public class CreateCourseCommand : IRequest<Guid>
{
    public CourseFormDto CourseData { get; set; } = null!;
    public Guid AdminUserId { get; set; }
}
