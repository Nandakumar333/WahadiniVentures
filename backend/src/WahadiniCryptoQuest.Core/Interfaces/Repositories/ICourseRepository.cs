using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Course entity with course-specific operations
/// </summary>
public interface ICourseRepository : IRepository<Course>
{
    /// <summary>
    /// Gets a course with all its lessons eagerly loaded
    /// </summary>
    /// <param name="id">Course ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Course with lessons, or null if not found</returns>
   Task<Course?> GetWithLessonsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all courses in a specific category with pagination
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of courses in the category</returns>
   Task<PagedResult<Course>> GetByCategoryAsync(Guid categoryId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches courses by title, difficulty level, and premium status
    /// </summary>
    /// <param name="searchTerm">Search term for course title</param>
    /// <param name="difficulty">Optional difficulty level filter</param>
    /// <param name="isPremium">Optional premium status filter</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of matching courses</returns>
   Task<PagedResult<Course>> SearchCoursesAsync(string searchTerm, DifficultyLevel? difficulty, bool? isPremium, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments the view count for a course
    /// </summary>
    /// <param name="id">Course ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
   Task IncrementViewCountAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets courses enrolled by a specific user
    /// </summary>
   Task<IEnumerable<Course>> GetEnrolledCoursesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all courses for admin panel (including unpublished)
    /// </summary>
   Task<PagedResult<Course>> GetAllCoursesForAdminAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if user is enrolled in a course
    /// </summary>
   Task<bool> IsUserEnrolledAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default);
}
