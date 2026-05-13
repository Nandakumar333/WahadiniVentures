using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WahadiniCryptoQuest.Core.DTOs.Common;
using WahadiniCryptoQuest.Core.DTOs.Reward;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Service.Commands.Rewards;
using WahadiniCryptoQuest.Service.Queries.Rewards;

namespace WahadiniCryptoQuest.API.Controllers;

/// <summary>
/// Admin-only endpoints for manual reward point adjustments and audit
/// </summary>
[ApiController]
[Route("api/v1/admin/rewards")]
[Authorize(Roles = "Admin")]
public class AdminRewardsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IRewardService _rewardService;

    public AdminRewardsController(IMediator mediator, IRewardService rewardService)
    {
        _mediator = mediator;
        _rewardService = rewardService;
    }

    /// <summary>
    /// Award points to a user with admin audit trail
    /// </summary>
    /// <param name="request">Award details</param>
    /// <returns>Transaction ID</returns>
    [HttpPost("award")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AwardPoints([FromBody] AdminAwardPointsRequest request)
    {
        var adminUserId = GetCurrentUserId();

        var command = new AwardPointsCommand(
            request.UserId,
            request.Amount,
            TransactionType.AdminBonus,
            request.Description,
            null,
            null,
            adminUserId
        );

        var transactionId = await _mediator.Send(command);
        return Ok(new
        {
            TransactionId = transactionId,
            Message = $"Successfully awarded {request.Amount} points to user {request.UserId}"
        });
    }

    /// <summary>
    /// Deduct points from a user with justification and admin audit trail
    /// </summary>
    /// <param name="request">Deduction details with justification</param>
    /// <returns>Transaction ID</returns>
    [HttpPost("deduct")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeductPoints([FromBody] AdminDeductPointsRequest request)
    {
        var adminUserId = GetCurrentUserId();

        // Validate justification length (minimum 10 characters)
        if (request.Justification.Length < 10)
        {
            return BadRequest(new { Error = "Justification must be at least 10 characters long" });
        }

        var command = new AwardPointsCommand(
            request.UserId,
            -request.Amount, // Negative amount for deduction
            TransactionType.AdminPenalty,
            request.Justification,
            null,
            null,
            adminUserId
        );

        var transactionId = await _mediator.Send(command);
        return Ok(new
        {
            TransactionId = transactionId,
            Message = $"Successfully deducted {request.Amount} points from user {request.UserId}"
        });
    }

    /// <summary>
    /// Get full transaction history for a specific user with admin actions highlighted
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
    /// <param name="cursor">Cursor for next page (optional)</param>
    /// <returns>Paginated transaction history with admin audit information</returns>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(PaginatedResult<AdminTransactionHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<AdminTransactionHistoryDto>>> GetUserTransactionHistory(
        Guid userId,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? cursor = null)
    {
        var query = new GetAdminTransactionHistoryQuery(userId)
        {
            PageSize = Math.Min(pageSize, 100),
            Cursor = cursor
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return Guid.Parse(userIdClaim);
    }
}
