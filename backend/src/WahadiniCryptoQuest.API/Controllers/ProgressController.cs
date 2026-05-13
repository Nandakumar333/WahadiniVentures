using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WahadiniCryptoQuest.Core.DTOs.Progress;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.API.Controllers;

/// <summary>
/// Controller for lesson progress tracking operations
/// </summary>
[ApiController]
[Route("api/lessons")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progressService;
    private readonly ILessonRepository _lessonRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ProgressController> _logger;

    public ProgressController(
        IProgressService progressService,
        ILessonRepository lessonRepository,
        IUserRepository userRepository,
        ILogger<ProgressController> logger)
    {
        _progressService = progressService;
        _lessonRepository = lessonRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current user's progress for a specific lesson
    /// </summary>
    /// <param name="lessonId">Lesson ID (GUID format)</param>
    /// <returns>Progress information or null if no progress exists</returns>
    /// <response code="200">Returns the progress data including watch position, completion percentage, and completion status</response>
    /// <response code="401">User not authenticated - valid JWT token required</response>
    /// <response code="403">User does not have access to premium content - upgrade subscription required</response>
    /// <response code="404">Lesson not found - invalid lesson ID provided</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/lessons/3fa85f64-5717-4562-b3fc-2c963f66afa6/progress
    ///     Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
    ///
    /// Sample response (200 OK):
    ///
    ///     {
    ///       "lessonId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "lastWatchedPosition": 245,
    ///       "completionPercentage": 81.67,
    ///       "isCompleted": true,
    ///       "completedAt": "2025-11-16T14:30:00Z",
    ///       "totalWatchTime": 320
    ///     }
    ///
    /// Sample response when no progress exists (200 OK):
    ///
    ///     null
    ///
    /// Sample error response (403 Forbidden - Premium content):
    ///
    ///     {
    ///       "error": "This is premium content. Please upgrade your subscription to access this lesson.",
    ///       "isPremiumContent": true,
    ///       "lessonTitle": "Advanced Blockchain Concepts"
    ///     }
    ///
    /// </remarks>
    [HttpGet("{lessonId}/progress")]
    [SwaggerOperation(
        Summary = "Get lesson progress",
        Description = "Retrieves the authenticated user's progress for a specific lesson. Returns null if no progress has been saved yet.",
        Tags = new[] { "Progress Tracking" }
    )]
    [SwaggerResponse(200, "Progress data retrieved successfully", typeof(ProgressDto))]
    [SwaggerResponse(401, "Unauthorized - JWT token missing or invalid")]
    [SwaggerResponse(403, "Forbidden - Premium content requires subscription upgrade")]
    [SwaggerResponse(404, "Lesson not found")]
    [ProducesResponseType(typeof(ProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProgress(Guid lessonId)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("GetProgress request for UserId={UserId}, LessonId={LessonId}", userId, lessonId);

        // Check if lesson exists
        var lesson = await _lessonRepository.GetByIdAsync(lessonId);
        if (lesson == null)
        {
            _logger.LogWarning("Lesson not found. LessonId={LessonId}", lessonId);
            return NotFound(new { error = $"Lesson with ID {lessonId} not found" });
        }

        // Check premium access control
        if (lesson.IsPremium)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError("User not found. UserId={UserId}", userId);
                return NotFound(new { error = "User not found" });
            }

            // Check if user has premium or admin access
            if (user.Role != UserRoleEnum.Premium && user.Role != UserRoleEnum.Admin)
            {
                _logger.LogWarning("Access denied to premium lesson. UserId={UserId}, LessonId={LessonId}, UserRole={UserRole}",
                    userId, lessonId, user.Role);
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    error = "This is premium content. Please upgrade your subscription to access this lesson.",
                    isPremiumContent = true,
                    lessonTitle = lesson.Title
                });
            }
        }

        var progress = await _progressService.GetProgressAsync(userId, lessonId);

        if (progress == null)
        {
            return NoContent(); // Return 204 for no progress
        }

        return Ok(progress);
    }

    /// <summary>
    /// Updates the current user's progress for a specific lesson
    /// </summary>
    /// <param name="lessonId">Lesson ID</param>
    /// <param name="dto">Progress update data</param>
    /// <returns>Update result with completion status</returns>
    /// <response code="200">Progress updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have access to premium content</response>
    /// <response code="404">Lesson not found</response>
    /// <response code="429">Too many requests - rate limit exceeded</response>
    [HttpPut("{lessonId}/progress")]
    [EnableRateLimiting("progress-update")]
    [ProducesResponseType(typeof(UpdateProgressResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateProgress(Guid lessonId, [FromBody] UpdateProgressDto dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid UpdateProgress request. Errors: {Errors}",
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        _logger.LogInformation("UpdateProgress request for UserId={UserId}, LessonId={LessonId}, WatchPosition={WatchPosition}",
            userId, lessonId, dto.WatchPosition);

        try
        {
            // Check if lesson exists
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                _logger.LogWarning("Lesson not found. LessonId={LessonId}", lessonId);
                return NotFound(new { error = $"Lesson with ID {lessonId} not found" });
            }

            // Check premium access control
            if (lesson.IsPremium)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogError("User not found. UserId={UserId}", userId);
                    return NotFound(new { error = "User not found" });
                }

                // Check if user has premium or admin access
                if (user.Role != UserRoleEnum.Premium && user.Role != UserRoleEnum.Admin)
                {
                    _logger.LogWarning("Access denied to premium lesson. UserId={UserId}, LessonId={LessonId}, UserRole={UserRole}",
                        userId, lessonId, user.Role);
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        error = "This is premium content. Please upgrade your subscription to access this lesson.",
                        isPremiumContent = true,
                        lessonTitle = lesson.Title
                    });
                }
            }

            var result = await _progressService.UpdateProgressAsync(userId, lessonId, dto);

            _logger.LogInformation("Progress updated successfully. UserId={UserId}, LessonId={LessonId}, CompletionPercentage={CompletionPercentage}, IsNewlyCompleted={IsNewlyCompleted}",
                userId, lessonId, result.CompletionPercentage, result.IsNewlyCompleted);

            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Lesson not found. UserId={UserId}, LessonId={LessonId}", userId, lessonId);
            return NotFound(new { error = $"Lesson with ID {lessonId} not found" });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Lesson not found. UserId={UserId}, LessonId={LessonId}", userId, lessonId);
            return NotFound(new { error = $"Lesson with ID {lessonId} not found" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error. UserId={UserId}, LessonId={LessonId}", userId, lessonId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating progress. UserId={UserId}, LessonId={LessonId}", userId, lessonId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
        }
    }

    /// <summary>
    /// Extracts the current user's ID from JWT claims
    /// </summary>
    /// <returns>User ID as Guid</returns>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogError("Unable to extract user ID from JWT claims");
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        return userId;
    }
}
