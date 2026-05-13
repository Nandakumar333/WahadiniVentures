using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Course.Queries;

/// <summary>
/// Handler for GetCoursesQuery
/// </summary>
public class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, PagedResult<CourseDto>>
{
    private readonly ICourseService _courseService;
    private readonly ILogger<GetCoursesQueryHandler> _logger;

    public GetCoursesQueryHandler(
        ICourseService courseService,
        ILogger<GetCoursesQueryHandler> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }

    public async Task<PagedResult<CourseDto>> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling GetCoursesQuery - Page={Page}, PageSize={PageSize}, CategoryId={CategoryId}, DifficultyLevel={DifficultyLevel}, IsPremium={IsPremium}, SearchTerm={SearchTerm}, IncludeUnpublished={IncludeUnpublished}",
            request.Page, request.PageSize, request.CategoryId, request.DifficultyLevel, request.IsPremium, request.SearchTerm, request.IncludeUnpublished);

        try
        {
            var result = await _courseService.GetCoursesAsync(
                request.CategoryId,
                request.DifficultyLevel,
                request.IsPremium,
                request.SearchTerm,
                request.Page,
                request.PageSize,
                cancellationToken,
                request.IncludeUnpublished); // Pass the flag

            _logger.LogInformation(
                "Successfully retrieved {Count} courses out of {TotalCount}",
                result.Items.Count(), result.TotalCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetCoursesQuery");
            throw;
        }
    }
}
