using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Service.Commands.Admin;
using CourseEntity = WahadiniCryptoQuest.Core.Entities.Course;

namespace WahadiniCryptoQuest.Service.Handlers.Admin;

/// <summary>
/// Handler for creating a new course
/// T098: US4 - Course Content Management
/// </summary>
public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Guid>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateCourseCommandHandler> _logger;

    public CreateCourseCommandHandler(
        ICourseRepository courseRepository,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork,
        ILogger<CreateCourseCommandHandler> logger)
    {
        _courseRepository = courseRepository;
        _auditLogService = auditLogService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        var course = new CourseEntity
        {
            Title = request.CourseData.Title,
            Description = request.CourseData.Description,
            CategoryId = request.CourseData.CategoryId,
            ThumbnailUrl = request.CourseData.ThumbnailUrl,
            DifficultyLevel = request.CourseData.Difficulty,
            IsPremium = request.CourseData.IsPremium,
            IsPublished = request.CourseData.IsPublished,
            CreatedByUserId = request.AdminUserId,
            ViewCount = 0
        };

        await _courseRepository.AddAsync(course);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log audit entry
        await _auditLogService.LogActionAsync(
            adminUserId: request.AdminUserId,
            actionType: "CreateCourse",
            resourceType: "Course",
            resourceId: course.Id.ToString(),
            beforeValue: null,
            afterValue: System.Text.Json.JsonSerializer.Serialize(new
            {
                course.Title,
                course.CategoryId,
                course.DifficultyLevel,
                course.IsPremium,
                course.IsPublished
            }),
            ipAddress: "system",
            cancellationToken: cancellationToken
        );

        _logger.LogInformation("Course created: {CourseId} by admin {AdminId}", course.Id, request.AdminUserId);

        return course.Id;
    }
}
