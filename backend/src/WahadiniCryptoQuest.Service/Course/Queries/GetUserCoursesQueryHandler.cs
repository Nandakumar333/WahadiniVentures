using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Course.Queries;

/// <summary>
/// Handler for GetUserCoursesQuery
/// </summary>
public class GetUserCoursesQueryHandler : IRequestHandler<GetUserCoursesQuery, IEnumerable<EnrolledCourseDto>>
{
    private readonly ICourseService _courseService;
    private readonly ILogger<GetUserCoursesQueryHandler> _logger;

    public GetUserCoursesQueryHandler(
        ICourseService courseService,
        ILogger<GetUserCoursesQueryHandler> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }

    public async Task<IEnumerable<EnrolledCourseDto>> Handle(GetUserCoursesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling GetUserCoursesQuery - UserId={UserId}, Status={Status}",
            request.UserId, request.Status);

        try
        {
            var result = await _courseService.GetUserCoursesAsync(
                request.UserId,
                request.Status,
                cancellationToken);

            _logger.LogInformation(
                "Successfully retrieved {Count} enrolled courses for UserId={UserId}",
                result.Count(), request.UserId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetUserCoursesQuery for UserId={UserId}", request.UserId);
            throw;
        }
    }
}
