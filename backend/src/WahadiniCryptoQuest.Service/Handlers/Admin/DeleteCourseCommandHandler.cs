using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Service.Commands.Admin;

namespace WahadiniCryptoQuest.Service.Handlers.Admin;

/// <summary>
/// Handler for soft deleting a course
/// T102: US4 - Course Content Management
/// </summary>
public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand, Unit>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCourseCommandHandler> _logger;

    public DeleteCourseCommandHandler(
        ICourseRepository courseRepository,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork,
        ILogger<DeleteCourseCommandHandler> logger)
    {
        _courseRepository = courseRepository;
        _auditLogService = auditLogService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(request.CourseId);
        if (course == null)
        {
            throw new InvalidOperationException($"Course not found: {request.CourseId}");
        }

        // Soft delete - preserve data for enrolled users
        course.IsDeleted = true;
        course.DeletedAt = DateTime.UtcNow;

        await _courseRepository.UpdateAsync(course);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log audit entry
        await _auditLogService.LogActionAsync(
            adminUserId: request.AdminUserId,
            actionType: "DeleteCourse",
            resourceType: "Course",
            resourceId: course.Id.ToString(),
            beforeValue: System.Text.Json.JsonSerializer.Serialize(new { course.Title, IsDeleted = false }),
            afterValue: System.Text.Json.JsonSerializer.Serialize(new { course.Title, IsDeleted = true }),
            ipAddress: "system",
            cancellationToken: cancellationToken
        );

        _logger.LogInformation("Course deleted: {CourseId} by admin {AdminId}", course.Id, request.AdminUserId);

        return Unit.Value;
    }
}
