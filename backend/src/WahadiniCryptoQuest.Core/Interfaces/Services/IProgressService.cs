using System;
using System.Threading.Tasks;
using WahadiniCryptoQuest.Core.DTOs.Progress;

namespace WahadiniCryptoQuest.Core.Interfaces.Services;

/// <summary>
/// Service interface for managing lesson video progress
/// </summary>
public interface IProgressService
{
    /// <summary>
    /// Gets user progress for a specific lesson
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="lessonId">Lesson ID</param>
    /// <returns>Progress DTO or null if not found</returns>
    Task<ProgressDto?> GetProgressAsync(Guid userId, Guid lessonId);

    /// <summary>
    /// Updates user progress for a lesson
    /// Handles highest-position tracking, completion detection (80% threshold),
    /// and point awards automatically
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="lessonId">Lesson ID</param>
    /// <param name="updateDto">Progress update data</param>
    /// <returns>Update result with completion status and points awarded</returns>
    Task<UpdateProgressResultDto> UpdateProgressAsync(Guid userId, Guid lessonId, UpdateProgressDto updateDto);

    /// <summary>
    /// Manually marks a lesson as complete
    /// Used for edge cases or admin actions
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="lessonId">Lesson ID</param>
    /// <returns>True if successful, false if already complete</returns>
    Task<bool> MarkCompleteAsync(Guid userId, Guid lessonId);
}
