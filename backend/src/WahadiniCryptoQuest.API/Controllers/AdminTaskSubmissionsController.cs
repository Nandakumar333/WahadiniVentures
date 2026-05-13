using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WahadiniCryptoQuest.API.Controllers; // For Request DTO
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Interfaces;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;

namespace WahadiniCryptoQuest.API.Controllers;

[ApiController]
[Route("api/admin/tasks/submissions")]
[Authorize(Roles = "Admin")] // Assuming Role-based auth
public class AdminTaskSubmissionsController : ControllerBase
{
    private readonly ITaskSubmissionService _submissionService;
    private readonly IUserTaskSubmissionRepository _submissionRepository;

    public AdminTaskSubmissionsController(
        ITaskSubmissionService submissionService,
        IUserTaskSubmissionRepository submissionRepository)
    {
        _submissionService = submissionService;
        _submissionRepository = submissionRepository;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingSubmissions(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _submissionRepository.GetPendingReviewQueueAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/review")]
    public async Task<IActionResult> ReviewSubmission(
        Guid id,
        [FromBody] ReviewSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        // Get Reviewer ID
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var reviewerId))
        {
            return Unauthorized();
        }

        var result = await _submissionService.ReviewSubmissionAsync(
            id, 
            reviewerId, 
            request.IsApproved, 
            request.Feedback, 
            request.PointsAwarded, 
            request.Version, 
            cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(result),
            "CONCURRENCY_CONFLICT" => Conflict(result),
            "INVALID_STATUS" => BadRequest(result),
            _ => BadRequest(result)
        };
    }
    
    [HttpPost("bulk-review")]
    public async Task<IActionResult> BulkReview(
        [FromBody] BulkReviewRequest request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var reviewerId))
        {
            return Unauthorized();
        }

        var result = await _submissionService.BulkReviewAsync(
            request.SubmissionIds,
            reviewerId,
            request.IsApproved,
            request.Feedback,
            request.PointsAwarded,
            cancellationToken);

        return Ok(result);
    }
}

