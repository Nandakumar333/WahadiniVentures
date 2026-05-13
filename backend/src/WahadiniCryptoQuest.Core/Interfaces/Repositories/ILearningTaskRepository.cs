using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for LearningTask entity with task-specific operations
/// Note: Renamed from ITaskRepository to avoid ambiguity withTask
/// </summary>
public interface ILearningTaskRepository : IRepository<LearningTask>
{
    /// <summary>
    /// Gets all tasks for a specific lesson ordered by OrderIndex
    /// </summary>
    /// <param name="lessonId">Lesson ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of tasks ordered by OrderIndex</returns>
   Task<IEnumerable<LearningTask>> GetByLessonIdOrderedAsync(Guid lessonId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all tasks of a specific type
    /// </summary>
    /// <param name="taskType">Task type filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of tasks of the specified type</returns>
   Task<IEnumerable<LearningTask>> GetByTypeAsync(TaskType taskType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all required tasks for a specific lesson
    /// </summary>
    /// <param name="lessonId">Lesson ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of required tasks</returns>
   Task<IEnumerable<LearningTask>> GetRequiredTasksAsync(Guid lessonId, CancellationToken cancellationToken = default);
}
