using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Lesson entity with lesson-specific operations
/// </summary>
public interface ILessonRepository : IRepository<Lesson>
{
    /// <summary>
    /// Gets a lesson with all its tasks eagerly loaded
    /// </summary>
    /// <param name="id">Lesson ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Lesson with tasks, or null if not found</returns>
    Task<Lesson?> GetWithTasksAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all lessons for a specific course ordered by OrderIndex
    /// </summary>
    /// <param name="courseId">Course ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of lessons ordered by OrderIndex</returns>
    Task<IEnumerable<Lesson>> GetByCourseIdOrderedAsync(Guid courseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all lessons for a specific course (not necessarily ordered)
    /// </summary>
    /// <param name="courseId">Course ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of lessons</returns>
    Task<List<Lesson>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets lesson with course included
    /// </summary>
    Task<Lesson?> GetLessonWithCourseAsync(Guid lessonId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets next lesson in sequence
    /// </summary>
    Task<Lesson?> GetNextLessonAsync(Guid courseId, int currentOrderIndex, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets previous lesson in sequence
    /// </summary>
    Task<Lesson?> GetPreviousLessonAsync(Guid courseId, int currentOrderIndex, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reorders lessons in a course
    /// </summary>
    Task ReorderLessonsAsync(Guid courseId, Dictionary<Guid, int> lessonOrderMap, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets maximum order index for a course
    /// </summary>
    Task<int> GetMaxOrderIndexAsync(Guid courseId, CancellationToken cancellationToken = default);
}
