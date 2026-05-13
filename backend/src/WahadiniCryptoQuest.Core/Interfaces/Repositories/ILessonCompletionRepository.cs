using System;
using System.Threading.Tasks;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for LessonCompletion entity
/// </summary>
public interface ILessonCompletionRepository
{
    Task<LessonCompletion?> GetByUserAndLessonAsync(Guid userId, Guid lessonId);
    Task<LessonCompletion> AddAsync(LessonCompletion completion);
    Task<bool> ExistsAsync(Guid userId, Guid lessonId);
}
