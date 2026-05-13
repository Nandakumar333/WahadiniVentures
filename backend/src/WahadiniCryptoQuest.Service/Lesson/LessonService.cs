using AutoMapper;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Lesson;

/// <summary>
/// Service implementation for lesson management operations
/// </summary>
public class LessonService : ILessonService
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<LessonService> _logger;

    public LessonService(
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IMapper mapper,
        ILogger<LessonService> logger)
    {
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<LessonDto> CreateLessonAsync(
        CreateLessonDto createLessonDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Creating lesson for CourseId={CourseId}, Title={Title}",
                createLessonDto.CourseId, createLessonDto.Title);

            // Verify course exists
            var course = await _courseRepository.GetByIdAsync(createLessonDto.CourseId);
            if (course == null)
            {
                throw new InvalidOperationException($"Course with Id {createLessonDto.CourseId} not found");
            }

            // Validate YouTube video ID format (11 characters)
            if (string.IsNullOrWhiteSpace(createLessonDto.YouTubeVideoId) ||
                createLessonDto.YouTubeVideoId.Length != 11)
            {
                throw new ArgumentException("YouTube video ID must be exactly 11 characters", nameof(createLessonDto.YouTubeVideoId));
            }

            // Map DTO to entity
            var lesson = _mapper.Map<WahadiniCryptoQuest.Core.Entities.Lesson>(createLessonDto);

            // Auto-increment OrderIndex if not specified
            if (lesson.OrderIndex == 0)
            {
                var maxOrderIndex = await _lessonRepository.GetMaxOrderIndexAsync(createLessonDto.CourseId, cancellationToken);
                lesson.OrderIndex = maxOrderIndex + 1;
                _logger.LogInformation("Auto-assigned OrderIndex={OrderIndex} for new lesson", lesson.OrderIndex);
            }

            // CreatedAt is set in BaseEntity constructor, just update UpdatedAt
            lesson.UpdatedAt = DateTime.UtcNow;

            var createdLesson = await _lessonRepository.AddAsync(lesson);

            _logger.LogInformation(
                "Successfully created lesson with Id={LessonId}, OrderIndex={OrderIndex}",
                createdLesson.Id, createdLesson.OrderIndex);

            return _mapper.Map<LessonDto>(createdLesson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating lesson for CourseId={CourseId}",
                createLessonDto.CourseId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<LessonDto> UpdateLessonAsync(
        Guid lessonId,
        UpdateLessonDto updateLessonDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating lesson with Id={LessonId}", lessonId);

            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                throw new InvalidOperationException($"Lesson with Id {lessonId} not found");
            }

            // Validate YouTube video ID if changed
            if (!string.IsNullOrWhiteSpace(updateLessonDto.YouTubeVideoId) &&
                updateLessonDto.YouTubeVideoId != lesson.YouTubeVideoId)
            {
                if (updateLessonDto.YouTubeVideoId.Length != 11)
                {
                    throw new ArgumentException("YouTube video ID must be exactly 11 characters", nameof(updateLessonDto.YouTubeVideoId));
                }
            }

            // Map updated fields
            _mapper.Map(updateLessonDto, lesson);
            lesson.UpdatedAt = DateTime.UtcNow;

            await _lessonRepository.UpdateAsync(lesson);

            _logger.LogInformation("Successfully updated lesson with Id={LessonId}", lessonId);

            return _mapper.Map<LessonDto>(lesson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lesson with Id={LessonId}", lessonId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteLessonAsync(
        Guid lessonId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting lesson with Id={LessonId}", lessonId);

            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                _logger.LogWarning("Lesson not found with Id={LessonId}", lessonId);
                return false;
            }

            // Soft delete
            lesson.IsDeleted = true;
            lesson.DeletedAt = DateTime.UtcNow;
            await _lessonRepository.UpdateAsync(lesson);

            _logger.LogInformation("Successfully soft-deleted lesson with Id={LessonId}", lessonId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lesson with Id={LessonId}", lessonId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ReorderLessonsAsync(
        Guid courseId,
        Dictionary<Guid, int> lessonOrderMap,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Reordering {Count} lessons for CourseId={CourseId}",
                lessonOrderMap.Count, courseId);

            // T188: Lesson Reordering Algorithm
            // Purpose: Allow admins to drag-drop lessons in course editor and persist new order
            // Business Rules:
            // 1. All OrderIndex values must form a gap-free sequence starting from 1 (e.g., 1,2,3,4...)
            // 2. No duplicate OrderIndex values allowed
            // 3. All lessons in the map must belong to the specified course
            // 4. Reordering is atomic - all lessons updated together or none updated

            // Step 1: Verify course exists
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new InvalidOperationException($"Course with Id {courseId} not found");
            }

            // Step 2: Get all lessons for the course and filter to those being reordered
            var lessons = await _lessonRepository.GetByCourseIdAsync(courseId, cancellationToken);
            var lessonsToUpdate = lessons.Where(l => lessonOrderMap.ContainsKey(l.Id)).ToList();

            // Step 3: Security Check - Ensure all lesson IDs in the map belong to this course
            // This prevents unauthorized reordering of lessons from other courses
            if (lessonsToUpdate.Count != lessonOrderMap.Count)
            {
                throw new InvalidOperationException("Some lesson IDs in the order map do not belong to the specified course");
            }

            // Step 4: Validate gap-free sequence (1,2,3,4...)
            // Why: Gap-free ordering ensures UI can reliably display "Lesson 1", "Lesson 2", etc.
            // without complex logic to handle gaps. Also simplifies "Next Lesson" navigation.
            var newOrderIndices = lessonOrderMap.Values.OrderBy(x => x).ToList();
            for (int i = 0; i < newOrderIndices.Count; i++)
            {
                // Expected sequence: 1,2,3,4,5...
                // Reject sequences like: 1,3,4 (gap at 2) or 2,3,4 (doesn't start at 1)
                if (newOrderIndices[i] != i + 1)
                {
                    throw new ArgumentException($"Order indices must form a gap-free sequence starting from 1. Expected {i + 1}, got {newOrderIndices[i]}");
                }
            }

            // Step 5: Apply new OrderIndex to each lesson and update timestamp
            foreach (var lesson in lessonsToUpdate)
            {
                lesson.OrderIndex = lessonOrderMap[lesson.Id];
                lesson.UpdatedAt = DateTime.UtcNow;
                await _lessonRepository.UpdateAsync(lesson);
            }

            _logger.LogInformation(
                "Successfully reordered {Count} lessons for CourseId={CourseId}",
                lessonsToUpdate.Count, courseId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error reordering lessons for CourseId={CourseId}",
                courseId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<LessonDto?> GetLessonByIdAsync(
        Guid lessonId,
        bool includeTasks = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting lesson with Id={LessonId}, IncludeTasks={IncludeTasks}", lessonId, includeTasks);

            var lesson = includeTasks
                ? await _lessonRepository.GetWithTasksAsync(lessonId, cancellationToken)
                : await _lessonRepository.GetByIdAsync(lessonId);

            if (lesson == null)
            {
                _logger.LogWarning("Lesson not found with Id={LessonId}", lessonId);
                return null;
            }

            return _mapper.Map<LessonDto>(lesson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lesson with Id={LessonId}", lessonId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<LessonDto>> GetLessonsByCourseIdAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting lessons for CourseId={CourseId}", courseId);

            var lessons = await _lessonRepository.GetByCourseIdAsync(courseId);

            return _mapper.Map<List<LessonDto>>(lessons);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lessons for CourseId={CourseId}", courseId);
            throw;
        }
    }
}
