using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Progress;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Service.Services;

/// <summary>
/// Service for managing lesson video progress tracking
/// Handles highest-position tracking, completion detection, and reward points
/// </summary>
public class ProgressService : IProgressService
{
    private readonly IUserProgressRepository _userProgressRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ILessonCompletionRepository _lessonCompletionRepository;
    private readonly IUserCourseEnrollmentRepository _enrollmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProgressService> _logger;

    public ProgressService(
        IUserProgressRepository userProgressRepository,
        ILessonRepository lessonRepository,
        ILessonCompletionRepository lessonCompletionRepository,
        IUserCourseEnrollmentRepository enrollmentRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProgressService> logger)
    {
        _userProgressRepository = userProgressRepository;
        _lessonRepository = lessonRepository;
        _lessonCompletionRepository = lessonCompletionRepository;
        _enrollmentRepository = enrollmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current progress for a user on a specific lesson
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="lessonId">Lesson identifier</param>
    /// <returns>Progress DTO or null if no progress exists</returns>
    public async Task<ProgressDto?> GetProgressAsync(Guid userId, Guid lessonId)
    {
        _logger.LogDebug("Getting progress for UserId={UserId}, LessonId={LessonId}", userId, lessonId);

        var progress = await _userProgressRepository.GetByUserAndLessonAsync(userId, lessonId);

        if (progress == null)
        {
            _logger.LogDebug("No progress found for UserId={UserId}, LessonId={LessonId}", userId, lessonId);
            return null;
        }

        // Log progress retrieval with structured data (T320)
        _logger.LogInformation(
            "Progress retrieved: UserId={UserId}, LessonId={LessonId}, " +
            "CompletionPercentage={CompletionPercentage}%, IsCompleted={IsCompleted}, " +
            "LastWatchedPosition={LastWatchedPosition}s, TotalWatchTime={TotalWatchTime}s",
            userId, lessonId, progress.CompletionPercentage, progress.IsCompleted,
            progress.LastWatchedPosition, progress.TotalWatchTime);

        return new ProgressDto
        {
            LessonId = progress.LessonId,
            LastWatchedPosition = progress.LastWatchedPosition,
            CompletionPercentage = progress.CompletionPercentage,
            IsCompleted = progress.IsCompleted,
            CompletedAt = progress.CompletedAt,
            TotalWatchTime = progress.TotalWatchTime
        };
    }

    /// <summary>
    /// Updates the watch progress for a user on a lesson
    /// Implements highest-position tracking and 80% completion detection
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="lessonId">Lesson identifier</param>
    /// <param name="updateDto">Progress update data containing watch position</param>
    /// <returns>Result indicating success, completion percentage, and points awarded</returns>
    public async Task<UpdateProgressResultDto> UpdateProgressAsync(Guid userId, Guid lessonId, UpdateProgressDto updateDto)
    {
        var startTime = DateTime.UtcNow;

        // Log progress update with structured data (T320-T321)
        _logger.LogInformation(
            "Progress update started: UserId={UserId}, LessonId={LessonId}, " +
            "WatchPosition={WatchPosition}s, Timestamp={Timestamp}",
            userId, lessonId, updateDto.WatchPosition, startTime);

        // Get or create progress
        var progress = await _userProgressRepository.GetByUserAndLessonAsync(userId, lessonId);
        bool isNewProgress = false;

        if (progress == null)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                _logger.LogWarning("Lesson not found: LessonId={LessonId}", lessonId);
                throw new InvalidOperationException($"Lesson {lessonId} not found");
            }

            progress = UserProgress.Create(userId, lessonId);
            await _userProgressRepository.AddAsync(progress);
            isNewProgress = true;

            _logger.LogInformation(
                "New progress record created: UserId={UserId}, LessonId={LessonId}, LessonTitle={LessonTitle}",
                userId, lessonId, lesson.Title);
        }

        // Get lesson to calculate completion percentage
        var currentLesson = await _lessonRepository.GetByIdAsync(lessonId);
        if (currentLesson == null)
        {
            throw new InvalidOperationException($"Lesson {lessonId} not found");
        }

        // Use VideoDuration if available, fallback to Duration * 60
        var totalDuration = currentLesson.VideoDuration.HasValue && currentLesson.VideoDuration.Value > 0
            ? currentLesson.VideoDuration.Value
            : (currentLesson.Duration * 60);
        if (totalDuration <= 0)
        {
            _logger.LogWarning("Invalid video duration for lesson {LessonId}", lessonId);
            totalDuration = 1; // Prevent division by zero
        }

        // Update position (highest-position tracking logic in domain entity)
        progress.UpdatePosition(updateDto.WatchPosition, totalDuration);

        var result = new UpdateProgressResultDto
        {
            Success = true,
            CompletionPercentage = progress.CompletionPercentage,
            PointsAwarded = 0,
            IsNewlyCompleted = false
        };

        // Check for completion (80% threshold)
        if (!progress.IsCompleted && progress.CompletionPercentage >= 80m)
        {
            // Mark as complete
            progress.MarkComplete(DateTime.UtcNow);

            // Log completion event with full context (T321)
            _logger.LogInformation(
                "Lesson completed: UserId={UserId}, LessonId={LessonId}, LessonTitle={LessonTitle}, " +
                "CompletionPercentage={CompletionPercentage}%, TotalWatchTime={TotalWatchTime}s, " +
                "VideoDuration={VideoDuration}s, CompletedAt={CompletedAt}",
                userId, lessonId, currentLesson.Title, progress.CompletionPercentage,
                progress.TotalWatchTime, totalDuration, DateTime.UtcNow);

            // Check if points already claimed to prevent duplicates
            if (!progress.RewardPointsClaimed)
            {
                // Award points - Note: Actual point awarding would be handled by RewardService
                // For now, we just record the points that should be awarded
                var pointsAwarded = currentLesson.RewardPoints;
                if (pointsAwarded > 0)
                {
                    // TODO: Integrate with RewardService when available
                    // await _rewardService.AwardPointsAsync(userId, pointsAwarded, $"Completed lesson: {currentLesson.Title}");
                    progress.ClaimRewardPoints();
                    result.PointsAwarded = pointsAwarded;

                    // Log points award (T321)
                    _logger.LogInformation(
                        "Points awarded: UserId={UserId}, LessonId={LessonId}, " +
                        "PointsAwarded={PointsAwarded}, Reason=LessonCompletion",
                        userId, lessonId, pointsAwarded);
                }

                // Create completion record
                var completion = LessonCompletion.Create(userId, lessonId, pointsAwarded, progress.CompletionPercentage);
                await _lessonCompletionRepository.AddAsync(completion);

                result.IsNewlyCompleted = true;
            }
        }

        // Save changes - only update if not new
        if (!isNewProgress)
        {
            await _userProgressRepository.UpdateAsync(progress);
        }
        await _unitOfWork.SaveChangesAsync(); // Save lesson progress first

        // Update overall course progress
        await UpdateCourseEnrollmentProgressAsync(userId, currentLesson.CourseId);

        // Log performance metrics (T322)
        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
        _logger.LogDebug(
            "Progress update completed: UserId={UserId}, LessonId={LessonId}, " +
            "CompletionPercentage={CompletionPercentage}%, Duration={DurationMs}ms, " +
            "IsNewlyCompleted={IsNewlyCompleted}",
            userId, lessonId, progress.CompletionPercentage, duration, result.IsNewlyCompleted);

        return result;
    }

    public async Task<bool> MarkCompleteAsync(Guid userId, Guid lessonId)
    {
        _logger.LogInformation("Manually marking lesson {LessonId} complete for user {UserId}", lessonId, userId);

        var progress = await _userProgressRepository.GetByUserAndLessonAsync(userId, lessonId);
        if (progress == null)
        {
            _logger.LogWarning("Cannot mark complete: No progress found for user {UserId}, lesson {LessonId}", userId, lessonId);
            return false;
        }

        if (progress.IsCompleted)
        {
            _logger.LogDebug("Lesson {LessonId} already marked complete for user {UserId}", lessonId, userId);
            return false;
        }

        progress.MarkComplete(DateTime.UtcNow);
        await _userProgressRepository.UpdateAsync(progress);
        await _unitOfWork.SaveChangesAsync();

        // Update overall course progress
        var lesson = await _lessonRepository.GetByIdAsync(lessonId);
        if (lesson != null)
        {
            await UpdateCourseEnrollmentProgressAsync(userId, lesson.CourseId);
        }

        _logger.LogInformation("Lesson {LessonId} manually marked complete for user {UserId}", lessonId, userId);
        return true;
    }

    /// <summary>
    /// Updates the total course enrollment progress based on completed lessons
    /// </summary>
    private async Task UpdateCourseEnrollmentProgressAsync(Guid userId, Guid courseId)
    {
        try
        {
            // Find enrollment
            var enrollment = await _enrollmentRepository.FirstOrDefaultAsync(
                e => e.UserId == userId && e.CourseId == courseId);

            if (enrollment == null)
            {
                _logger.LogWarning("No enrollment found for UserId={UserId}, CourseId={CourseId} while updating progress", userId, courseId);
                return;
            }

            // Calculate progress
            var totalLessons = await _lessonRepository.CountAsync(l => l.CourseId == courseId);
            
            if (totalLessons == 0) return;

            // Get all progress records for this course to calculate granular percentage
            var courseProgressRecords = await _userProgressRepository.GetUserProgressByCourseAsync(
                userId, courseId);

            // Sum up completion percentages from all progress records
            // Note: Some lessons might not have progress records yet (0%)
            decimal totalCompletionSum = courseProgressRecords.Sum(p => p.CompletionPercentage);
            
            // Count completed lessons for logging/analytics
            int completedLessons = courseProgressRecords.Count(p => p.IsCompleted);

            // Calculate average progress across all lessons
            decimal courseProgress = totalCompletionSum / totalLessons;

            // Update enrollment using domain method
            enrollment.UpdateProgress(courseProgress);
            enrollment.UpdateAccess();

            await _enrollmentRepository.UpdateAsync(enrollment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Updated course progress: UserId={UserId}, CourseId={CourseId}, Progress={Progress}%, CompletedLessons={Completed}/{Total}",
                userId, courseId, courseProgress, completedLessons, totalLessons);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course progress for UserId={UserId}, CourseId={CourseId}", userId, courseId);
            // Don't throw, as this is a background sync operation and shouldn't fail the lesson update request
        }
    }
}
