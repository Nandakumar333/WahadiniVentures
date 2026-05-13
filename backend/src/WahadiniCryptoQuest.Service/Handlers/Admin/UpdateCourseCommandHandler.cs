using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Service.Commands.Admin;

namespace WahadiniCryptoQuest.Service.Handlers.Admin;

/// <summary>
/// Handler for updating a course
/// T100: US4 - Course Content Management
/// </summary>
public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, Unit>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCourseCommandHandler> _logger;

    public UpdateCourseCommandHandler(
        ICourseRepository courseRepository,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork,
        ILogger<UpdateCourseCommandHandler> logger)
    {
        _courseRepository = courseRepository;
        _auditLogService = auditLogService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(request.CourseId);
        if (course == null)
        {
            throw new InvalidOperationException($"Course not found: {request.CourseId}");
        }

        // Capture before state
        var beforeState = System.Text.Json.JsonSerializer.Serialize(new
        {
            course.Title,
            course.Description,
            course.CategoryId,
            course.DifficultyLevel,
            course.IsPremium,
            course.IsPublished
        });

        // Update properties
        course.Title = request.CourseData.Title;
        course.Description = request.CourseData.Description;
        course.CategoryId = request.CourseData.CategoryId;
        course.ThumbnailUrl = request.CourseData.ThumbnailUrl;
        course.DifficultyLevel = request.CourseData.Difficulty;
        course.IsPremium = request.CourseData.IsPremium;
        course.IsPublished = request.CourseData.IsPublished;

        await _courseRepository.UpdateAsync(course);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log audit entry
        var afterState = System.Text.Json.JsonSerializer.Serialize(new
        {
            course.Title,
            course.Description,
            course.CategoryId,
            course.DifficultyLevel,
            course.IsPremium,
            course.IsPublished
        });

        await _auditLogService.LogActionAsync(
            adminUserId: request.AdminUserId,
            actionType: "UpdateCourse",
            resourceType: "Course",
            resourceId: course.Id.ToString(),
            beforeValue: beforeState,
            afterValue: afterState,
            ipAddress: "system",
            cancellationToken: cancellationToken
        );

        _logger.LogInformation("Course updated: {CourseId} by admin {AdminId}", course.Id, request.AdminUserId);

        return Unit.Value;
    }
}
