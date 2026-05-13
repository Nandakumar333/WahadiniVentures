using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Admin;

namespace WahadiniCryptoQuest.Service.Queries.Admin;

/// <summary>
/// Query to get paginated courses for admin dashboard
/// T095: US4 - Course Content Management
/// </summary>
public class GetCoursesQuery : IRequest<PaginatedCoursesDto>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? CategoryId { get; set; }
    public bool? IsPublished { get; set; }
    public string? SearchTerm { get; set; }
}

/// <summary>
/// Paginated course list response
/// </summary>
public class PaginatedCoursesDto
{
    public List<CourseListDto> Courses { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
