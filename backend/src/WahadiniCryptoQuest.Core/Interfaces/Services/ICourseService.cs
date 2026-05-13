using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Interfaces.Services;

/// <summary>
/// Service interface for course-related operations
/// </summary>
public interface ICourseService
{
    /// <summary>
    /// Gets a paginated list of courses with optional filters
    /// </summary>
    /// <param name="categoryId">Filter by category ID</param>
    /// <param name="difficultyLevel">Filter by difficulty level</param>
    /// <param name="isPremium">Filter by premium status</param>
    /// <param name="searchTerm">Search in title and description</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="includeUnpublished">Include unpublished courses (admin only)</param>
    /// <returns>Paginated list of courses</returns>
    Task<PagedResult<CourseDto>> GetCoursesAsync(
        Guid? categoryId,
        DifficultyLevel? difficultyLevel,
        bool? isPremium,
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default,
        bool includeUnpublished = false);

    /// <summary>
    /// Gets detailed information about a specific course
    /// </summary>
    /// <param name="courseId">Course ID</param>
    /// <param name="userId">Current user ID (for enrollment status)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Course details with lessons and enrollment status</returns>
    Task<CourseDetailDto?> GetCourseDetailAsync(
        Guid courseId,
        Guid? userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enrolls a user in a course
    /// </summary>
    /// <param name="courseId">Course ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if enrollment successful, false if already enrolled</returns>
    Task<bool> EnrollUserAsync(
        Guid courseId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all courses a user is enrolled in
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="status">Optional filter by completion status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of enrolled courses with progress</returns>
    Task<IEnumerable<EnrolledCourseDto>> GetUserCoursesAsync(
        Guid userId,
        CompletionStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new course (admin only)
    /// </summary>
    /// <param name="createDto">Course creation data</param>
    /// <param name="createdByUserId">ID of user creating the course</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created course DTO</returns>
    Task<CourseDto> CreateCourseAsync(
        CreateCourseDto createDto,
        Guid createdByUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing course (admin only)
    /// </summary>
    /// <param name="updateDto">Course update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated course DTO</returns>
    Task<CourseDto?> UpdateCourseAsync(
        UpdateCourseDto updateDto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a course (soft delete, admin only)
    /// </summary>
    /// <param name="courseId">Course ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteCourseAsync(
        Guid courseId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a course (admin only)
    /// </summary>
    /// <param name="courseId">Course ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if published successfully</returns>
    Task<bool> PublishCourseAsync(
        Guid courseId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments the view count for a course
    /// </summary>
    /// <param name="courseId">Course ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task IncrementViewCountAsync(
        Guid courseId,
        CancellationToken cancellationToken = default);
}
