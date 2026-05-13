using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for UserProgress entity with progress tracking operations
/// </summary>
public interface IUserProgressRepository : IRepository<UserProgress>
{
    /// <summary>
    /// Gets user progress for a specific lesson
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="lessonId">Lesson ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User progress entity or null if not found</returns>
    Task<UserProgress?> GetByUserAndLessonAsync(Guid userId, Guid lessonId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts (inserts or updates) user progress for a lesson
    /// If progress exists for (UserId, LessonId), updates it; otherwise, inserts new record
    /// </summary>
    /// <param name="progress">User progress entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Upserted user progress entity</returns>
    Task<UserProgress> UpsertProgressAsync(UserProgress progress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all user progress records for a specific course
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="courseId">Course ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user progress records for all lessons in the course</returns>
    Task<IEnumerable<UserProgress>> GetUserProgressByCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default);
}
