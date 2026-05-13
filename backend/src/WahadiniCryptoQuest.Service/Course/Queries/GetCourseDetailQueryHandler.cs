using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Course.Queries;

/// <summary>
/// Handler for GetCourseDetailQuery
/// </summary>
public class GetCourseDetailQueryHandler : IRequestHandler<GetCourseDetailQuery, CourseDetailDto?>
{
    private readonly ICourseService _courseService;
    private readonly ILogger<GetCourseDetailQueryHandler> _logger;

    public GetCourseDetailQueryHandler(
        ICourseService courseService,
        ILogger<GetCourseDetailQueryHandler> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }

    public async Task<CourseDetailDto?> Handle(GetCourseDetailQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling GetCourseDetailQuery for CourseId={CourseId}, UserId={UserId}",
            request.CourseId,
            request.UserId);

        var courseDetail = await _courseService.GetCourseDetailAsync(
            request.CourseId,
            request.UserId,
            cancellationToken);

        if (courseDetail == null)
        {
            _logger.LogWarning("Course not found. CourseId={CourseId}", request.CourseId);
            return null;
        }

        _logger.LogInformation(
            "Successfully retrieved course details. CourseId={CourseId}, LessonCount={LessonCount}",
            request.CourseId,
            courseDetail.Lessons?.Count ?? 0);

        return courseDetail;
    }
}
