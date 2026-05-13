using MediatR;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Queries.Admin;

namespace WahadiniCryptoQuest.Service.Handlers.Admin;

/// <summary>
/// Handler for getting paginated courses with admin filters
/// T096: US4 - Course Content Management
/// </summary>
public class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, PaginatedCoursesDto>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IUserCourseEnrollmentRepository _enrollmentRepository;

    public GetCoursesQueryHandler(
        ICourseRepository courseRepository,
        IUserCourseEnrollmentRepository enrollmentRepository)
    {
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<PaginatedCoursesDto> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
    {
        // Use existing search method with adapted parameters
        var allCourses = await _courseRepository.SearchCoursesAsync(
            searchTerm: request.SearchTerm ?? "",
            difficulty: null,
            isPremium: null,
            page: 1,
            pageSize: 10000, // Get all for filtering
            cancellationToken: cancellationToken
        );

        // Apply additional admin-specific filters
        var filtered = allCourses.Items.AsQueryable();

        if (request.CategoryId.HasValue)
        {
            filtered = filtered.Where(c => c.CategoryId == request.CategoryId.Value);
        }

        if (request.IsPublished.HasValue)
        {
            filtered = filtered.Where(c => c.IsPublished == request.IsPublished.Value);
        }

        var totalCount = filtered.Count();

        // Apply pagination
        var courses = filtered
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Get enrollment counts by querying all enrollments
        var enrollments = await _enrollmentRepository.GetAllAsync();
        var enrollmentCounts = enrollments
            .GroupBy(e => e.CourseId)
            .ToDictionary(g => g.Key, g => g.Count());

        var courseDtos = courses.Select(c => new Core.DTOs.Admin.CourseListDto
        {
            Id = c.Id,
            Title = c.Title,
            Category = c.Category?.Name ?? "Uncategorized",
            Difficulty = c.DifficultyLevel,
            IsPublished = c.IsPublished,
            CreatedAt = c.CreatedAt,
            TotalLessons = c.Lessons?.Count ?? 0,
            EnrollmentCount = enrollmentCounts.GetValueOrDefault(c.Id, 0)
        }).ToList();

        return new PaginatedCoursesDto
        {
            Courses = courseDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
