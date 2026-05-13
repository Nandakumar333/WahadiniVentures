using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Repository implementation for Lesson entity
/// </summary>
public class LessonRepository : Repository<Lesson>, ILessonRepository
{
    public LessonRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets a lesson by ID, excluding soft-deleted and inactive lessons
    /// </summary>
    public override async Task<Lesson?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id && l.IsActive && !l.IsDeleted);
    }

    /// <summary>
    /// Gets lesson with all tasks eagerly loaded
    /// </summary>
    public async Task<Lesson?> GetWithTasksAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(l => l.Tasks.OrderBy(t => t.OrderIndex))
            .FirstOrDefaultAsync(l => l.Id == lessonId && l.IsActive, cancellationToken);
    }

    /// <summary>
    /// Gets all lessons for a course ordered by OrderIndex
    /// </summary>
    public async Task<IEnumerable<Lesson>> GetByCourseIdOrderedAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.CourseId == courseId && l.IsActive && !l.IsDeleted)
            .OrderBy(l => l.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all lessons for a specific course
    /// </summary>
    public async Task<List<Lesson>> GetByCourseIdAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(l => l.CourseId == courseId && l.IsActive && !l.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets lesson with course included
    /// </summary>
    public async Task<Lesson?> GetLessonWithCourseAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.Id == lessonId && l.IsActive && !l.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets next lesson in sequence
    /// </summary>
    public async Task<Lesson?> GetNextLessonAsync(Guid courseId, int currentOrderIndex, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.CourseId == courseId && l.IsActive && !l.IsDeleted && l.OrderIndex > currentOrderIndex)
            .OrderBy(l => l.OrderIndex)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Gets previous lesson in sequence
    /// </summary>
    public async Task<Lesson?> GetPreviousLessonAsync(Guid courseId, int currentOrderIndex, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.CourseId == courseId && l.IsActive && !l.IsDeleted && l.OrderIndex < currentOrderIndex)
            .OrderByDescending(l => l.OrderIndex)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Reorders lessons in a course
    /// </summary>
    public async Task ReorderLessonsAsync(Guid courseId, Dictionary<Guid, int> lessonOrderMap, CancellationToken cancellationToken = default)
    {
        var lessons = await _dbSet
            .Where(l => l.CourseId == courseId && lessonOrderMap.Keys.Contains(l.Id))
            .ToListAsync(cancellationToken);

        foreach (var lesson in lessons)
        {
            if (lessonOrderMap.TryGetValue(lesson.Id, out int newOrderIndex))
            {
                lesson.OrderIndex = newOrderIndex;
                lesson.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Gets maximum order index for a course
    /// </summary>
    public async Task<int> GetMaxOrderIndexAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        var maxOrder = await _dbSet
            .Where(l => l.CourseId == courseId && l.IsActive && !l.IsDeleted)
            .MaxAsync(l => (int?)l.OrderIndex, cancellationToken);

        return maxOrder ?? 0;
    }
}
