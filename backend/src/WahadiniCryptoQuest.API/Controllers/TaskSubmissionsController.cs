using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using WahadiniCryptoQuest.API.Controllers;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.DTOs.Task;
using WahadiniCryptoQuest.Core.Interfaces;
using WahadiniCryptoQuest.Service.Services;

namespace WahadiniCryptoQuest.API.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TaskSubmissionsController : ControllerBase
{
    private readonly ITaskSubmissionService _submissionService;
    private readonly IFileStorageService _fileStorageService;

    public TaskSubmissionsController(
        ITaskSubmissionService submissionService,
        IFileStorageService fileStorageService)
    {
        _submissionService = submissionService;
        _fileStorageService = fileStorageService;
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<ActionResult<Result<TaskSubmissionResponseDto>>> SubmitTask(
        Guid id,
        [FromForm] TaskSubmissionRequest request, // Changed to FromForm for multipart
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        // Ensure ID matches
        if (id != request.TaskId)
        {
            return BadRequest(Result<TaskSubmissionResponseDto>.Failure("Task ID mismatch"));
        }

        // Get UserId
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        // Handle File Upload for Screenshot
        if (request.TaskType == WahadiniCryptoQuest.Core.Enums.TaskType.Screenshot)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(Result<TaskSubmissionResponseDto>.Failure("File required for screenshot task"));
            }

            // Basic validation
            if (file.Length > 5 * 1024 * 1024) // 5MB
            {
                return BadRequest(Result<TaskSubmissionResponseDto>.Failure("File too large (max 5MB)"));
            }

            // Save file
            try
            {
                var filePath = await _fileStorageService.SaveFileAsync(file.OpenReadStream(), file.FileName, $"tasks/{userId}/{id}", cancellationToken);

                // Update SubmissionData with file path JSON
                request.SubmissionData = System.Text.Json.JsonSerializer.Serialize(new { filePath, mimeType = file.ContentType, size = file.Length });
            }
            catch (Exception ex)
            {
                return StatusCode(500, Result<TaskSubmissionResponseDto>.Failure($"File upload failed: {ex.Message}"));
            }
        }

        var result = await _submissionService.SubmitTaskAsync(request, userId, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return result.ErrorCode switch
        {
            "TASK_NOT_FOUND" => NotFound(result),
            "DUPLICATE_SUBMISSION" => Conflict(result),
            _ => BadRequest(result)
        };
    }

    [HttpGet("my-submissions")]
    public async Task<ActionResult<Result<IEnumerable<UserTaskSubmissionDto>>>> GetMySubmissions(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var result = await _submissionService.GetMySubmissionsAsync(userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets submission status for a specific task
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Submission status or null if never submitted</returns>
    [HttpGet("{taskId:guid}/submission-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<TaskSubmissionStatusDto>>> GetSubmissionStatus(
        Guid taskId,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var result = await _submissionService.GetSubmissionStatusAsync(taskId, userId, cancellationToken);
        return Ok(result);
    }
}
