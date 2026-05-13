using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WahadiniCryptoQuest.API.Attributes;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.API.Controllers;

/// <summary>
/// Controller for lesson management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LessonsController : ControllerBase
{
    private readonly ILessonService _lessonService;
    private readonly ILogger<LessonsController> _logger;

    public LessonsController(ILessonService lessonService, ILogger<LessonsController> logger)
    {
        _lessonService = lessonService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a lesson by ID
    /// </summary>
    /// <param name="id">Lesson ID</param>
    /// <param name="includeTasks">Whether to include tasks in the response (default: false)</param>
    /// <returns>Lesson details</returns>
    /// <response code="200">Returns the lesson</response>
    /// <response code="404">Lesson not found</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLesson(Guid id, [FromQuery] bool includeTasks = false)
    {
        _logger.LogInformation("GetLesson request for LessonId={LessonId}, IncludeTasks={IncludeTasks}", id, includeTasks);

        var lesson = await _lessonService.GetLessonByIdAsync(id, includeTasks);

        if (lesson == null)
        {
            _logger.LogWarning("Lesson not found. LessonId={LessonId}", id);
            return NotFound(new { error = $"Lesson with ID {id} not found" });
        }

        return Ok(lesson);
    }

    /// <summary>
    /// Gets all lessons for a specific course
    /// </summary>
    /// <param name="courseId">Course ID</param>
    /// <returns>List of lessons</returns>
    /// <response code="200">Returns the lessons</response>
    [HttpGet("course/{courseId}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLessonsByCourse(Guid courseId)
    {
        _logger.LogInformation("GetLessonsByCourse request for CourseId={CourseId}", courseId);

        var lessons = await _lessonService.GetLessonsByCourseIdAsync(courseId);

        return Ok(lessons);
    }

    /// <summary>
    /// Creates a new lesson (Admin only)
    /// </summary>
    /// <param name="courseId">Course ID to add the lesson to</param>
    /// <param name="createDto">Lesson creation data</param>
    /// <returns>Created lesson</returns>
    /// <response code="201">Lesson created successfully</response>
    /// <response code="400">Invalid lesson data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpPost("course/{courseId}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(RequestsPerMinute = 10, BurstCapacity = 2)] // T182: Limit admin lesson creation
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CreateLesson(Guid courseId, [FromBody] CreateLessonDto createDto)
    {
        _logger.LogInformation("CreateLesson request for CourseId={CourseId}, Title={Title}", courseId, createDto.Title);

        // Get user ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Invalid or missing user ID in claims");
            return Unauthorized(new { error = "User authentication required" });
        }

        try
        {
            // Create a new DTO with the course ID from the route
            var lessonDto = new CreateLessonDto
            {
                CourseId = courseId,
                Title = createDto.Title,
                Description = createDto.Description,
                YouTubeVideoId = createDto.YouTubeVideoId,
                Duration = createDto.Duration,
                OrderIndex = createDto.OrderIndex,
                IsPremium = createDto.IsPremium,
                RewardPoints = createDto.RewardPoints,
                ContentMarkdown = createDto.ContentMarkdown
            };

            var result = await _lessonService.CreateLessonAsync(lessonDto);

            _logger.LogInformation("Lesson created successfully. LessonId={LessonId}", result.Id);
            return CreatedAtAction(nameof(GetLesson), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid lesson creation operation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lesson");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while creating the lesson" });
        }
    }

    /// <summary>
    /// Updates an existing lesson (Admin only)
    /// </summary>
    /// <param name="id">Lesson ID to update</param>
    /// <param name="updateDto">Updated lesson data</param>
    /// <returns>Updated lesson</returns>
    /// <response code="200">Lesson updated successfully</response>
    /// <response code="400">Invalid lesson data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="404">Lesson not found</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(RequestsPerMinute = 10, BurstCapacity = 2)] // T182: Limit admin lesson updates
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateLesson(Guid id, [FromBody] UpdateLessonDto updateDto)
    {
        _logger.LogInformation("UpdateLesson request for LessonId={LessonId}", id);

        // Get user ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Invalid or missing user ID in claims");
            return Unauthorized(new { error = "User authentication required" });
        }

        try
        {
            var result = await _lessonService.UpdateLessonAsync(id, updateDto);

            _logger.LogInformation("Lesson updated successfully. LessonId={LessonId}", result.Id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid lesson update operation. LessonId={LessonId}", id);

            if (ex.Message.Contains("not found"))
            {
                return NotFound(new { error = ex.Message });
            }

            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lesson. LessonId={LessonId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while updating the lesson" });
        }
    }

    /// <summary>
    /// Soft deletes a lesson (Admin only)
    /// </summary>
    /// <param name="id">Lesson ID to delete</param>
    /// <returns>Deletion result</returns>
    /// <response code="204">Lesson deleted successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="404">Lesson not found</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(RequestsPerMinute = 10, BurstCapacity = 2)] // T182: Limit admin lesson deletions
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteLesson(Guid id)
    {
        _logger.LogInformation("DeleteLesson request for LessonId={LessonId}", id);

        // Get user ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Invalid or missing user ID in claims");
            return Unauthorized(new { error = "User authentication required" });
        }

        try
        {
            var success = await _lessonService.DeleteLessonAsync(id);

            if (!success)
            {
                _logger.LogWarning("Lesson not found. LessonId={LessonId}", id);
                return NotFound(new { error = $"Lesson with ID {id} not found" });
            }

            _logger.LogInformation("Lesson deleted successfully. LessonId={LessonId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lesson. LessonId={LessonId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while deleting the lesson" });
        }
    }

    /// <summary>
    /// Reorders lessons within a course (Admin only)
    /// </summary>
    /// <param name="reorderDto">Lesson reordering data</param>
    /// <returns>Reordering result</returns>
    /// <response code="200">Lessons reordered successfully</response>
    /// <response code="400">Invalid reorder data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpPut("reorder")]
    [Authorize(Roles = "Admin")]
    [RateLimit(RequestsPerMinute = 10, BurstCapacity = 2)] // T182: Limit admin lesson reordering
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ReorderLessons([FromBody] ReorderLessonsDto reorderDto)
    {
        _logger.LogInformation("ReorderLessons request for CourseId={CourseId}", reorderDto.CourseId);

        // Get user ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Invalid or missing user ID in claims");
            return Unauthorized(new { error = "User authentication required" });
        }

        try
        {
            // Convert LessonOrderDto array to Dictionary
            var lessonOrderMap = reorderDto.LessonOrders
                .ToDictionary(lo => lo.LessonId, lo => lo.OrderIndex);

            var success = await _lessonService.ReorderLessonsAsync(reorderDto.CourseId, lessonOrderMap);

            if (!success)
            {
                _logger.LogWarning("Failed to reorder lessons. CourseId={CourseId}", reorderDto.CourseId);
                return BadRequest(new { error = "Failed to reorder lessons" });
            }

            _logger.LogInformation("Lessons reordered successfully. CourseId={CourseId}", reorderDto.CourseId);
            return Ok(new { message = "Lessons reordered successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid lesson reorder operation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering lessons");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while reordering lessons" });
        }
    }
}
