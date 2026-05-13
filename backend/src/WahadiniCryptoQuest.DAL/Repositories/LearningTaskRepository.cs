using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Repository implementation for LearningTask entity
/// </summary>
public class LearningTaskRepository : Repository<LearningTask>, ILearningTaskRepository
{
    public LearningTaskRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all tasks for a lesson ordered by OrderIndex
    /// </summary>
    public async Task<IEnumerable<LearningTask>> GetByLessonIdOrderedAsync(
        Guid lessonId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.LessonId == lessonId)
            .OrderBy(t => t.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets tasks filtered by task type
    /// </summary>
    public async Task<IEnumerable<LearningTask>> GetByTypeAsync(
        TaskType taskType,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.TaskType == taskType)
            .OrderBy(t => t.Title)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets required tasks (IsRequired = true) for a lesson
    /// </summary>
    public async Task<IEnumerable<LearningTask>> GetRequiredTasksAsync(
        Guid lessonId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.LessonId == lessonId && t.IsRequired)
            .OrderBy(t => t.OrderIndex)
            .ToListAsync(cancellationToken);
    }
}
