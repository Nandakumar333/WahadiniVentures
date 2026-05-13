using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WahadiniCryptoQuest.Core.DTOs.Admin;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Service.Commands.Admin;
using WahadiniCryptoQuest.Service.Queries.Admin;
using System.Security.Claims;

namespace WahadiniCryptoQuest.API.Controllers.Admin;

/// <summary>
/// Controller for administrative dashboard operations.
/// Handles audit logs, notifications, and admin-specific functionality.
/// T015: AdminController skeleton with authorization and dependency injection
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly IAnalyticsService _analyticsService;
    private readonly IMediator _mediator;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IAuditLogService auditLogService,
        INotificationService notificationService,
        IAnalyticsService analyticsService,
        IMediator mediator,
        ILogger<AdminController> logger)
    {
        _auditLogService = auditLogService;
        _notificationService = notificationService;
        _analyticsService = analyticsService;
        _mediator = mediator;
        _logger = logger;
    }

    #region Dashboard Statistics (US1 - Platform Health Overview)

    /// <summary>
    /// Retrieves comprehensive admin dashboard statistics.
    /// GET /api/admin/stats
    /// T033: Dashboard stats endpoint with KPIs and 30-day trends
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(AdminStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminStatsDto>> GetDashboardStats(CancellationToken cancellationToken)
    {
        try
        {
            var stats = await _analyticsService.GetDashboardStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin dashboard statistics");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving dashboard statistics" });
        }
    }

    #endregion

    #region Task Review Endpoints (US2 - Task Review Workflow)

    /// <summary>
    /// Retrieves pending task submissions for review.
    /// GET /api/admin/tasks/pending
    /// T049: Get pending tasks endpoint with filtering and pagination
    /// </summary>
    [HttpGet("tasks/pending")]
    [ProducesResponseType(typeof(List<PendingTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<PendingTaskDto>>> GetPendingTasks(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] Guid? courseId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetPendingTasksQuery
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                CourseId = courseId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending task submissions");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving pending tasks" });
        }
    }

    /// <summary>
    /// Reviews a task submission (approve/reject).
    /// POST /api/admin/tasks/{submissionId}/review
    /// T050: Review task endpoint with points, notifications, audit logging
    /// </summary>
    [HttpPost("tasks/{submissionId:guid}/review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> ReviewTask(
        Guid submissionId,
        [FromBody] TaskReviewRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var adminUserId = GetCurrentUserId();

            var command = new ReviewTaskCommand
            {
                SubmissionId = submissionId,
                Status = request.Status,
                Feedback = request.Feedback,
                AdminUserId = adminUserId
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid task review operation for submission {SubmissionId}", submissionId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing task submission {SubmissionId}", submissionId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while reviewing the task" });
        }
    }

    #endregion

    #region User Management Endpoints (US3 - User Account Management)

    /// <summary>
    /// Retrieves paginated list of users with filters.
    /// GET /api/admin/users
    /// T070: User management endpoints
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(PaginatedUsersDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedUsersDto>> GetUsers(
        [FromQuery] string? searchTerm,
        [FromQuery] int? role,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isBanned,
        [FromQuery] bool? emailConfirmed,
        [FromQuery] bool? hasActiveSubscription,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new Service.Queries.Admin.GetUsersQuery
            {
                SearchTerm = searchTerm,
                Role = role.HasValue ? (Core.Enums.UserRoleEnum)role.Value : null,
                IsActive = isActive,
                IsBanned = isBanned,
                EmailConfirmed = emailConfirmed,
                HasActiveSubscription = hasActiveSubscription,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users list");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving users" });
        }
    }

    /// <summary>
    /// Retrieves detailed information for a specific user.
    /// GET /api/admin/users/{id}
    /// T070: User management endpoints
    /// </summary>
    [HttpGet("users/{id:guid}")]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserDetailDto>> GetUserById(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new Service.Queries.Admin.GetUserByIdQuery { UserId = id };
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null)
                return NotFound(new { message = $"User {id} not found" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving user details" });
        }
    }

    /// <summary>
    /// Updates a user's role.
    /// PUT /api/admin/users/{id}/role
    /// T070: User management endpoints
    /// </summary>
    [HttpPut("users/{id:guid}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> UpdateUserRole(
        Guid id,
        [FromBody] UpdateUserRoleDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var adminUserId = GetCurrentUserId();

            var command = new Service.Commands.Admin.UpdateUserRoleCommand
            {
                UserId = id,
                NewRole = request.Role,
                Reason = request.Reason,
                AdminUserId = adminUserId
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid user role update for user {UserId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role for user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating user role" });
        }
    }

    /// <summary>
    /// Bans a user account.
    /// POST /api/admin/users/{id}/ban
    /// T070: User management endpoints
    /// </summary>
    [HttpPost("users/{id:guid}/ban")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> BanUser(
        Guid id,
        [FromBody] BanUserDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var adminUserId = GetCurrentUserId();

            var command = new Service.Commands.Admin.BanUserCommand
            {
                UserId = id,
                Reason = request.Reason,
                AdminUserId = adminUserId
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid ban operation for user {UserId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error banning user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while banning user" });
        }
    }

    /// <summary>
    /// Unbans a user account.
    /// POST /api/admin/users/{id}/unban
    /// T070: User management endpoints
    /// </summary>
    [HttpPost("users/{id:guid}/unban")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> UnbanUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var adminUserId = GetCurrentUserId();

            var command = new Service.Commands.Admin.UnbanUserCommand
            {
                UserId = id,
                AdminUserId = adminUserId
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid unban operation for user {UserId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unbanning user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while unbanning user" });
        }
    }

    #endregion

    #region Audit Log Endpoints

    /// <summary>
    /// Retrieves audit logs with filtering and pagination.
    /// GET /api/admin/audit-logs
    /// </summary>
    [HttpGet("audit-logs")]
    [ProducesResponseType(typeof(PagedResultDto<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResultDto<AuditLogDto>>> GetAuditLogs(
        [FromQuery] AuditLogFilterDto filters,
        CancellationToken cancellationToken)
    {
        var result = await _auditLogService.GetAuditLogsAsync(filters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific audit log entry by ID.
    /// GET /api/admin/audit-logs/{id}
    /// </summary>
    [HttpGet("audit-logs/{id:guid}")]
    [ProducesResponseType(typeof(AuditLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuditLogDto>> GetAuditLogById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _auditLogService.GetAuditLogByIdAsync(id, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Audit log with ID {id} not found" });

        return Ok(result);
    }

    /// <summary>
    /// Retrieves audit history for a specific resource.
    /// GET /api/admin/audit-logs/resource/{resourceType}/{resourceId}
    /// </summary>
    [HttpGet("audit-logs/resource/{resourceType}/{resourceId}")]
    [ProducesResponseType(typeof(PagedResultDto<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResultDto<AuditLogDto>>> GetResourceHistory(
        string resourceType,
        string resourceId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _auditLogService.GetResourceHistoryAsync(
            resourceType, resourceId, pageNumber, pageSize, cancellationToken);

        return Ok(result);
    }

    #endregion

    #region Notification Endpoints

    /// <summary>
    /// Retrieves unread notifications for the authenticated user.
    /// GET /api/admin/notifications/unread
    /// </summary>
    [HttpGet("notifications/unread")]
    [ProducesResponseType(typeof(PagedResultDto<UserNotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResultDto<UserNotificationDto>>> GetUnreadNotifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var result = await _notificationService.GetUnreadNotificationsAsync(
            userId, pageNumber, pageSize, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets the count of unread notifications for the authenticated user.
    /// GET /api/admin/notifications/unread-count
    /// </summary>
    [HttpGet("notifications/unread-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> GetUnreadNotificationCount(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);

        return Ok(count);
    }

    /// <summary>
    /// Marks a notification as read.
    /// PUT /api/admin/notifications/{id}/mark-read
    /// </summary>
    [HttpPut("notifications/{id:guid}/mark-read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> MarkNotificationAsRead(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var success = await _notificationService.MarkAsReadAsync(id, userId, cancellationToken);

        if (!success)
            return NotFound(new { message = $"Notification with ID {id} not found or unauthorized" });

        return NoContent();
    }

    /// <summary>
    /// Marks all notifications as read for the authenticated user.
    /// PUT /api/admin/notifications/mark-all-read
    /// </summary>
    [HttpPut("notifications/mark-all-read")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> MarkAllNotificationsAsRead(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var count = await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);

        return Ok(count);
    }

    #endregion

    #region Course Management Endpoints (US4 - Course Content Management)

    /// <summary>
    /// Retrieves paginated list of courses with admin filters.
    /// GET /api/admin/courses
    /// T111: Get courses endpoint with pagination and filters
    /// </summary>
    [HttpGet("courses")]
    [ProducesResponseType(typeof(PaginatedCoursesDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedCoursesDto>> GetCourses(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool? isPublished = null,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetCoursesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                CategoryId = categoryId,
                IsPublished = isPublished,
                SearchTerm = searchTerm
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving courses");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving courses" });
        }
    }

    /// <summary>
    /// Creates a new course.
    /// POST /api/admin/courses
    /// T112: Create course endpoint
    /// </summary>
    [HttpPost("courses")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateCourse(
        [FromBody] CourseFormDto courseData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var adminUserId = GetCurrentUserId();
            var command = new CreateCourseCommand
            {
                CourseData = courseData,
                AdminUserId = adminUserId
            };

            var courseId = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetCourses), new { id = courseId }, courseId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while creating the course" });
        }
    }

    /// <summary>
    /// Updates an existing course.
    /// PUT /api/admin/courses/{id}
    /// T113: Update course endpoint
    /// </summary>
    [HttpPut("courses/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCourse(
        Guid id,
        [FromBody] CourseFormDto courseData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var adminUserId = GetCurrentUserId();
            var command = new UpdateCourseCommand
            {
                CourseId = id,
                CourseData = courseData,
                AdminUserId = adminUserId
            };

            await _mediator.Send(command, cancellationToken);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course {CourseId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating the course" });
        }
    }

    /// <summary>
    /// Soft deletes a course.
    /// DELETE /api/admin/courses/{id}
    /// T114: Delete course endpoint
    /// </summary>
    [HttpDelete("courses/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCourse(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var adminUserId = GetCurrentUserId();
            var command = new DeleteCourseCommand
            {
                CourseId = id,
                AdminUserId = adminUserId
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting course {CourseId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while deleting the course" });
        }
    }

    #endregion

    #region Discount Management Endpoints (US5)

    /// <summary>
    /// Get redemption logs for a specific discount code
    /// T145: US5 - Reward System Management
    /// </summary>
    [HttpGet("discounts/{code}/redemptions")]
    public async Task<ActionResult<List<RedemptionLogDto>>> GetRedemptions(
        string code,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        try
        {
            var query = new GetRedemptionsQuery
            {
                Code = code,
                DateFrom = dateFrom,
                DateTo = dateTo
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving redemptions for code: {Code}", code);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving redemptions" });
        }
    }

    /// <summary>
    /// Manually adjust a user's point balance
    /// T146: US5 - Reward System Management
    /// </summary>
    [HttpPost("users/{userId}/points/adjust")]
    public async Task<ActionResult<int>> AdjustUserPoints(
        Guid userId,
        [FromBody] AdjustPointsRequestDto request)
    {
        try
        {
            var adminUserId = GetCurrentUserId();
            var command = new AdjustPointsCommand
            {
                UserId = userId,
                AdjustmentAmount = request.AdjustmentAmount,
                Reason = request.Reason,
                AdminUserId = adminUserId
            };

            var newBalance = await _mediator.Send(command);
            return Ok(new { newBalance });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid point adjustment for user {UserId}", userId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting points for user {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while adjusting user points" });
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Extracts the current user ID from JWT claims.
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Failed to extract user ID from JWT claims");
            throw new UnauthorizedAccessException("Invalid user authentication");
        }

        return userId;
    }

    #endregion
}
