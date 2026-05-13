using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Repository implementation for LessonCompletion entity
/// </summary>
public class LessonCompletionRepository : Repository<LessonCompletion>, ILessonCompletionRepository
{
    public LessonCompletionRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets lesson completion record by user and lesson
    /// </summary>
    public async Task<LessonCompletion?> GetByUserAndLessonAsync(Guid userId, Guid lessonId)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(lc => lc.UserId == userId && lc.LessonId == lessonId);
    }

    /// <summary>
    /// Adds a new lesson completion record
    /// </summary>
    public new async Task<LessonCompletion> AddAsync(LessonCompletion completion)
    {
        await _dbSet.AddAsync(completion);
        return completion;
    }

    /// <summary>
    /// Checks if a completion record exists for user and lesson
    /// </summary>
    public async Task<bool> ExistsAsync(Guid userId, Guid lessonId)
    {
        return await _dbSet
            .AnyAsync(lc => lc.UserId == userId && lc.LessonId == lessonId);
    }
}
