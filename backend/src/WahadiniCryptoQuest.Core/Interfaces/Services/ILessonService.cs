using WahadiniCryptoQuest.Core.DTOs.Course;

namespace WahadiniCryptoQuest.Core.Interfaces.Services;

/// <summary>
/// Service interface for lesson management operations
/// Handles CRUD operations and lesson reordering
/// </summary>
public interface ILessonService
{
    /// <summary>
    /// Creates a new lesson for a course
    /// </summary>
    /// <param name="createLessonDto">Lesson creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created lesson DTO</returns>
    Task<LessonDto> CreateLessonAsync(CreateLessonDto createLessonDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing lesson
    /// </summary>
    /// <param name="lessonId">Lesson identifier</param>
    /// <param name="updateLessonDto">Updated lesson data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated lesson DTO</returns>
    Task<LessonDto> UpdateLessonAsync(Guid lessonId, UpdateLessonDto updateLessonDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a lesson
    /// </summary>
    /// <param name="lessonId">Lesson identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteLessonAsync(Guid lessonId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reorders lessons within a course
    /// Updates OrderIndex to create a gap-free sequence
    /// </summary>
    /// <param name="courseId">Course identifier</param>
    /// <param name="lessonOrderMap">Dictionary mapping lessonId to new orderIndex</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if reordering was successful</returns>
    Task<bool> ReorderLessonsAsync(Guid courseId, Dictionary<Guid, int> lessonOrderMap, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a lesson by ID
    /// </summary>
    /// <param name="lessonId">Lesson identifier</param>
    /// <param name="includeTasks">Whether to include tasks in the response</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Lesson DTO or null if not found</returns>
    Task<LessonDto?> GetLessonByIdAsync(Guid lessonId, bool includeTasks = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all lessons for a course
    /// </summary>
    /// <param name="courseId">Course identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of lesson DTOs ordered by OrderIndex</returns>
    Task<List<LessonDto>> GetLessonsByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default);
}
