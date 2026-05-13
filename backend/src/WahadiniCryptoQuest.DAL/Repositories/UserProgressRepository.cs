using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Repository implementation for UserProgress entity
/// </summary>
public class UserProgressRepository : Repository<UserProgress>, IUserProgressRepository
{
    public UserProgressRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets user progress for a specific lesson
    /// </summary>
    public async Task<UserProgress?> GetByUserAndLessonAsync(
        Guid userId,
        Guid lessonId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId, cancellationToken);
    }

    /// <summary>
    /// Upserts (inserts or updates) user progress for a lesson
    /// </summary>
    public async Task<UserProgress> UpsertProgressAsync(
        UserProgress progress,
        CancellationToken cancellationToken = default)
    {
        var existingProgress = await _dbSet
            .FirstOrDefaultAsync(p => p.UserId == progress.UserId && p.LessonId == progress.LessonId, cancellationToken);

        if (existingProgress != null)
        {
            // Update existing progress
            existingProgress.UpdateProgress(progress.VideoWatchTimeSeconds, progress.CompletionPercentage);
            _dbSet.Update(existingProgress);
            return existingProgress;
        }

        // Add new progress
        await _dbSet.AddAsync(progress, cancellationToken);
        return progress;
    }

    /// <summary>
    /// Gets all user progress for a specific course
    /// </summary>
    public async Task<IEnumerable<UserProgress>> GetUserProgressByCourseAsync(
        Guid userId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(p => p.Lesson)
            .Where(p => p.UserId == userId && p.Lesson.CourseId == courseId)
            .OrderBy(p => p.Lesson.OrderIndex)
            .ToListAsync(cancellationToken);
    }
}
