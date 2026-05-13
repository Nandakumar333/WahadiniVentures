using MediatR;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Service.Course.Queries;

/// <summary>
/// Query for getting paginated list of courses with optional filters
/// </summary>
public class GetCoursesQuery : IRequest<PagedResult<CourseDto>>
{
    /// <summary>
    /// Filter by category ID
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Filter by difficulty level
    /// </summary>
    public DifficultyLevel? DifficultyLevel { get; set; }

    /// <summary>
    /// Filter by premium status
    /// </summary>
    public bool? IsPremium { get; set; }

    /// <summary>
    /// Search term for title and description
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Include unpublished courses (admin only)
    /// </summary>
    public bool IncludeUnpublished { get; set; } = false;
}
