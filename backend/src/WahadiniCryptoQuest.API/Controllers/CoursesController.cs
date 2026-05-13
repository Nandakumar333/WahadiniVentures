using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WahadiniCryptoQuest.API.Attributes;
using WahadiniCryptoQuest.API.Authorization;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Service.Course.Commands;
using WahadiniCryptoQuest.Service.Course.Queries;

namespace WahadiniCryptoQuest.API.Controllers;

/// <summary>
/// Controller for course management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(IMediator mediator, ILogger<CoursesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets a paginated list of courses with optional filters
    /// </summary>
    /// <param name="categoryId">Filter by category ID</param>
    /// <param name="difficultyLevel">Filter by difficulty level (0=Beginner, 1=Intermediate, 2=Advanced, 3=Expert)</param>
    /// <param name="isPremium">Filter by premium status</param>
    /// <param name="search">Search term for title and description</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 50)</param>
    /// <returns>Paginated list of courses</returns>
    /// <response code="200">Returns the paginated list of courses</response>
    /// <response code="400">Invalid query parameters</response>
    [HttpGet]
    [AllowAnonymous] // Allow anonymous access to browse courses
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCourses(
        [FromQuery] Guid? categoryId,
        [FromQuery] DifficultyLevel? difficultyLevel,
        [FromQuery] bool? isPremium,
        [FromQuery] string? search,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null)
    {
        // Apply defaults and validate
        var pageNumber = page ?? 1;
        var itemsPerPage = pageSize ?? 10;

        _logger.LogInformation(
            "GetCourses request: CategoryId={CategoryId}, DifficultyLevel={DifficultyLevel}, IsPremium={IsPremium}, Search={Search}, Page={Page}, PageSize={PageSize}",
            categoryId, difficultyLevel, isPremium, search, pageNumber, itemsPerPage);

        // Validate pagination parameters
        if (page.HasValue && page.Value < 1)
        {
            _logger.LogWarning("Invalid page number: {Page}", page.Value);
            return BadRequest(new { error = "Page number must be greater than 0" });
        }

        if (pageSize.HasValue && (pageSize.Value < 1 || pageSize.Value > 50))
        {
            _logger.LogWarning("Invalid page size: {PageSize}", pageSize.Value);
            return BadRequest(new { error = "Page size must be between 1 and 50" });
        }

        var query = new GetCoursesQuery
        {
            CategoryId = categoryId,
            DifficultyLevel = difficultyLevel,
            IsPremium = isPremium,
            SearchTerm = search,
            Page = pageNumber,
            PageSize = itemsPerPage
        };

        var result = await _mediator.Send(query);

        // Add caching headers for course list (T169 - 5 minutes cache)
        Response.Headers.CacheControl = "public, max-age=300";
        Response.Headers.Append("Vary", "Accept-Encoding");

        // Add pagination metadata to response headers
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        Response.Headers.Append("X-Page-Number", result.PageNumber.ToString());
        Response.Headers.Append("X-Page-Size", result.PageSize.ToString());
        Response.Headers.Append("X-Total-Pages", result.TotalPages.ToString());

        return Ok(new
        {
            items = result.Items,
            totalCount = result.TotalCount,
            pageNumber = result.PageNumber,
            pageSize = result.PageSize,
            totalPages = result.TotalPages,
            hasPreviousPage = result.HasPreviousPage,
            hasNextPage = result.HasNextPage
        });
    }

    /// <summary>
    /// Gets all courses including unpublished ones (Admin only)
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
    /// <returns>Paginated list of all courses for admin</returns>
    /// <response code="200">Returns the paginated list of all courses</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAdminCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("GetAdminCourses request: Page={Page}, PageSize={PageSize}", page, pageSize);

        // Validate pagination parameters
        if (page < 1)
        {
            return BadRequest(new { error = "Page number must be greater than 0" });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { error = "Page size must be between 1 and 100" });
        }

        var query = new GetCoursesQuery
        {
            Page = page,
            PageSize = pageSize,
            IncludeUnpublished = true // Admin sees all courses
        };

        var result = await _mediator.Send(query);

        // Add pagination metadata to response headers
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        Response.Headers.Append("X-Page-Number", result.PageNumber.ToString());
        Response.Headers.Append("X-Page-Size", result.PageSize.ToString());
        Response.Headers.Append("X-Total-Pages", result.TotalPages.ToString());

        return Ok(new
        {
            items = result.Items,
            totalCount = result.TotalCount,
            page = result.PageNumber,
            pageSize = result.PageSize,
            totalPages = result.TotalPages,
            hasPreviousPage = result.HasPreviousPage,
            hasNextPage = result.HasNextPage
        });
    }

    /// <summary>
    /// Gets all courses the authenticated user is enrolled in with progress tracking
    /// </summary>
    /// <param name="status">Optional filter by completion status (0=NotStarted, 1=InProgress, 2=Completed)</param>
    /// <returns>List of enrolled courses with progress information</returns>
    /// <response code="200">Returns the user's enrolled courses</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("my-courses")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyCourses([FromQuery] CompletionStatus? status)
    {
        _logger.LogInformation("GetMyCourses request with Status={Status}", status);

        // Get user ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Invalid or missing user ID in claims");
            return Unauthorized(new { error = "User authentication required" });
        }

        try
        {
            var query = new GetUserCoursesQuery
            {
                UserId = userId,
                Status = status
            };

            var courses = await _mediator.Send(query);

            _logger.LogInformation("Successfully retrieved {Count} enrolled courses for UserId={UserId}", courses.Count(), userId);
            return Ok(courses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving enrolled courses for UserId={UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while retrieving your courses" });
        }
    }

    /// <summary>
    /// Gets detailed information about a specific course
    /// </summary>
    /// <param name="id">The course ID</param>
    /// <returns>Course details including lessons and enrollment status</returns>
    /// <response code="200">Returns the course details</response>
    /// <response code="404">Course not found</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseDetail(Guid id)
    {
        _logger.LogInformation("GetCourseDetail request for CourseId={CourseId}", id);

        // Get user ID if authenticated
        Guid? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUserId))
            {
                userId = parsedUserId;
            }
        }

        var query = new GetCourseDetailQuery(id, userId);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            _logger.LogWarning("Course not found. CourseId={CourseId}", id);
            return NotFound(new { error = $"Course with ID {id} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Enrolls the authenticated user in a course
    /// </summary>
    /// <param name="id">The course ID to enroll in</param>
    /// <returns>Enrollment result</returns>
    /// <response code="201">User successfully enrolled</response>
    /// <response code="400">Invalid enrollment request</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Course not found</response>
    /// <response code="409">User already enrolled in this course</response>
    [HttpPost("{id}/enroll")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> EnrollInCourse(Guid id)
    {
        _logger.LogInformation("EnrollInCourse request for CourseId={CourseId}", id);

        // Get user ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Invalid or missing user ID in claims");
            return Unauthorized(new { error = "User authentication required" });
        }

        try
        {
            var command = new EnrollInCourseCommand(id, userId);
            var success = await _mediator.Send(command);

            if (!success)
            {
                _logger.LogWarning("User already enrolled. UserId={UserId}, CourseId={CourseId}", userId, id);
                return Conflict(new { error = "You are already enrolled in this course" });
            }

            _logger.LogInformation("User successfully enrolled. UserId={UserId}, CourseId={CourseId}", userId, id);
            return StatusCode(StatusCodes.Status201Created, new
            {
                message = "Successfully enrolled in course",
                courseId = id,
                userId = userId
            });
        }
        catch (Core.Exceptions.PremiumAccessDeniedException ex)
        {
            _logger.LogWarning(ex, "Premium access denied for enrollment. CourseId={CourseId}", id);
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                error = ex.Message,
                requiresUpgrade = true,
                upgradeUrl = "/upgrade"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid enrollment operation. CourseId={CourseId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enrolling user. CourseId={CourseId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while processing your enrollment" });
        }
    }

    /// <summary>
    /// Creates a new course (Admin only)
    /// </summary>
    /// <param name="createDto">Course creation data</param>
    /// <returns>Created course</returns>
    /// <response code="201">Course created successfully</response>
    /// <response code="400">Invalid course data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RequirePermission("courses:create")] // Added permission check
    [RateLimit(RequestsPerMinute = 10, BurstCapacity = 2)] // T182: Limit admin course creation to 10/min
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)] // Rate limit response
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto createDto)
    {
        _logger.LogInformation("CreateCourse request for Title={Title}", createDto.Title);

        // Get user ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Invalid or missing user ID in claims");
            return Unauthorized(new { error = "User authentication required" });
        }

        try
        {
            var command = new CreateCourseCommand
            {
                Title = createDto.Title,
                Description = createDto.Description,
                ContentMarkdown = null, // Not in DTO, set to null
                CategoryId = createDto.CategoryId,
                DifficultyLevel = createDto.DifficultyLevel,
                IsPremium = createDto.IsPremium,
                ThumbnailUrl = createDto.ThumbnailUrl,
                RewardPoints = createDto.RewardPoints,
                EstimatedDurationMinutes = createDto.EstimatedDuration,
                CreatedByUserId = userId
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation("Course created successfully. CourseId={CourseId}", result.Id);
            return CreatedAtAction(nameof(GetCourseDetail), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid course creation operation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while creating the course" });
        }
    }

    /// <summary>
    /// Updates an existing course (Admin only)
    /// </summary>
    /// <param name="id">Course ID to update</param>
    /// <param name="updateDto">Updated course data</param>
    /// <returns>Updated course</returns>
    /// <response code="200">Course updated successfully</response>
    /// <response code="400">Invalid course data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="404">Course not found</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(RequestsPerMinute = 10, BurstCapacity = 2)] // T182: Limit admin course updates
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateCourseDto updateDto)
    {
        _logger.LogInformation("UpdateCourse request for CourseId={CourseId}", id);

        // Get user ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Invalid or missing user ID in claims");
            return Unauthorized(new { error = "User authentication required" });
        }

        try
        {
            var command = new UpdateCourseCommand
            {
                CourseId = id,
                Title = updateDto.Title,
                Description = updateDto.Description,
                ContentMarkdown = null, // Not in DTO, set to null
                CategoryId = updateDto.CategoryId,
                DifficultyLevel = updateDto.DifficultyLevel,
                IsPremium = updateDto.IsPremium,
                ThumbnailUrl = updateDto.ThumbnailUrl,
                RewardPoints = updateDto.RewardPoints,
                EstimatedDurationMinutes = updateDto.EstimatedDuration,
                UpdatedByUserId = userId
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation("Course updated successfully. CourseId={CourseId}", result.Id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid course update operation. CourseId={CourseId}", id);

            if (ex.Message.Contains("not found"))
            {
                return NotFound(new { error = ex.Message });
            }

            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course. CourseId={CourseId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while updating the course" });
        }
    }

    /// <summary>
    /// Soft deletes a course (Admin only)
    /// </summary>
    /// <param name="id">Course ID to delete</param>
    /// <returns>Deletion result</returns>
    /// <response code="204">Course deleted successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="404">Course not found</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(RequestsPerMinute = 10, BurstCapacity = 2)] // T182: Limit admin course deletions
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteCourse(Guid id)
    {
        _logger.LogInformation("DeleteCourse request for CourseId={CourseId}", id);

        // Get user ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Invalid or missing user ID in claims");
            return Unauthorized(new { error = "User authentication required" });
        }

        try
        {
            var command = new DeleteCourseCommand(id, userId);
            var success = await _mediator.Send(command);

            if (!success)
            {
                _logger.LogWarning("Course not found. CourseId={CourseId}", id);
                return NotFound(new { error = $"Course with ID {id} not found" });
            }

            _logger.LogInformation("Course deleted successfully. CourseId={CourseId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting course. CourseId={CourseId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while deleting the course" });
        }
    }

    /// <summary>
    /// Publishes a course (Admin only)
    /// </summary>
    /// <param name="id">Course ID to publish</param>
    /// <returns>Publishing result</returns>
    /// <response code="200">Course published successfully</response>
    /// <response code="400">Cannot publish (no lessons or other validation failed)</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="404">Course not found</response>
    [HttpPut("{id}/publish")]
    [Authorize(Roles = "Admin")]
    [RequireAllPermissions("courses:read", "courses:publish")] // Requires BOTH permissions (AND logic)
    [RateLimit(RequestsPerMinute = 10, BurstCapacity = 2)] // T182: Limit admin course publishing
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PublishCourse(Guid id)
    {
        _logger.LogInformation("PublishCourse request for CourseId={CourseId}", id);

        // Get user ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Invalid or missing user ID in claims");
            return Unauthorized(new { error = "User authentication required" });
        }

        try
        {
            var command = new PublishCourseCommand(id, userId);
            var success = await _mediator.Send(command);

            if (!success)
            {
                _logger.LogWarning("Course not found. CourseId={CourseId}", id);
                return NotFound(new { error = $"Course with ID {id} not found" });
            }

            _logger.LogInformation("Course published successfully. CourseId={CourseId}", id);
            return Ok(new { message = "Course published successfully", courseId = id });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot publish course. CourseId={CourseId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing course. CourseId={CourseId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while publishing the course" });
        }
    }
}
