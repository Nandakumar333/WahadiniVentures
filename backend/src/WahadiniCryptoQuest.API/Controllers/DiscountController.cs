using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WahadiniCryptoQuest.Core.DTOs.Discount;
using WahadiniCryptoQuest.Service.Discount.Commands;
using WahadiniCryptoQuest.Service.Discount.Queries;

namespace WahadiniCryptoQuest.API.Controllers;

/// <summary>
/// API controller for discount code operations (user endpoints)
/// Provides endpoints for browsing available discounts, redeeming discount codes, and viewing redemption history
/// </summary>
/// <remarks>
/// All endpoints require authentication. Users can only access their own redemption data.
/// Discount redemption is an atomic operation with point deduction and transaction logging.
/// </remarks>
[ApiController]
[Route("api/discounts")]
[Authorize]
public class DiscountController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DiscountController> _logger;

    public DiscountController(IMediator mediator, ILogger<DiscountController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all active discount codes available for redemption by the authenticated user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>List of available discount codes with user-specific eligibility information</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/discounts/available
    ///     Authorization: Bearer {token}
    /// 
    /// Returns discount codes with information about:
    /// - Discount percentage and required points
    /// - User's affordability status (canAfford: true/false)
    /// - Availability status (active, expired, sold out)
    /// - Redemption limits and current usage
    /// </remarks>
    /// <response code="200">Returns list of available discount codes (may be empty)</response>
    /// <response code="401">User is not authenticated or token is invalid</response>
    /// <response code="404">User account not found</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("available")]
    [ProducesResponseType(typeof(List<DiscountTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<DiscountTypeDto>>> GetAvailableDiscounts(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user credentials");
        }

        try
        {
            var query = new GetAvailableDiscountsQuery(userId);
            var discounts = await _mediator.Send(query, cancellationToken);

            _logger.LogInformation("Retrieved {Count} available discounts for user {UserId}", discounts.Count, userId);

            return Ok(discounts);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User {UserId} not found", userId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available discounts for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving discounts" });
        }
    }

    /// <summary>
    /// Redeems a discount code by deducting required points from user's balance
    /// </summary>
    /// <param name="id">The unique identifier of the discount code to redeem</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Redemption response containing the issued discount code and updated point balance</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/discounts/3fa85f64-5717-4562-b3fc-2c963f66afa6/redeem
    ///     Authorization: Bearer {token}
    /// 
    /// This operation is atomic and includes:
    /// - Point balance validation and deduction
    /// - Discount availability checks (active, not expired, within redemption limit)
    /// - Duplicate redemption prevention (one per user per code)
    /// - Transaction logging for audit trail
    /// - Concurrency conflict handling with retry logic
    /// 
    /// Redemption will fail if:
    /// - User has insufficient points
    /// - Discount is inactive or expired
    /// - Maximum redemption limit reached
    /// - User has already redeemed this code
    /// </remarks>
    /// <response code="200">Discount successfully redeemed. Returns code and updated balance</response>
    /// <response code="400">Redemption validation failed (insufficient points, already redeemed, etc.)</response>
    /// <response code="401">User is not authenticated or token is invalid</response>
    /// <response code="404">Discount code or user account not found</response>
    /// <response code="409">Concurrency conflict. Another transaction modified the same data</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPost("{id}/redeem")]
    [ProducesResponseType(typeof(RedemptionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RedemptionResponseDto>> RedeemDiscount(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user credentials");
        }

        try
        {
            var command = new RedeemDiscountCommand(userId, id);
            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation(
                "User {UserId} successfully redeemed discount code {Code}",
                userId,
                result.Code);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Discount {DiscountId} or User {UserId} not found", id, userId);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid redemption attempt by user {UserId} for discount {DiscountId}", userId, id);
            return BadRequest(new { message = ex.Message });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict during redemption for user {UserId}", userId);
            return Conflict(new { message = "This discount was just redeemed by another user. Please try again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error redeeming discount {DiscountId} for user {UserId}", id, userId);
            return StatusCode(500, new { message = "An error occurred while processing your redemption" });
        }
    }

    /// <summary>
    /// Retrieves paginated redemption history for the authenticated user
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve (default: 1, minimum: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, range: 1-100)</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Paginated collection of user's redeemed discount codes with metadata</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/discounts/my-redemptions?pageNumber=1&amp;pageSize=10
    ///     Authorization: Bearer {token}
    /// 
    /// Returns:
    /// - List of redeemed discount codes with redemption dates
    /// - Discount details (code, percentage, expiry date)
    /// - Usage status (whether code was used in subscription)
    /// - Pagination metadata (total count, page info)
    /// 
    /// Results are ordered by redemption date (most recent first).
    /// </remarks>
    /// <response code="200">Returns paginated list of redemptions (may be empty for new users)</response>
    /// <response code="401">User is not authenticated or token is invalid</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("my-redemptions")]
    [ProducesResponseType(typeof(PaginatedRedemptionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedRedemptionsDto>> GetMyRedemptions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user credentials");
        }

        try
        {
            var query = new GetMyRedemptionsQuery(userId, pageNumber, pageSize);
            var result = await _mediator.Send(query, cancellationToken);

            _logger.LogInformation(
                "Retrieved redemption history for user {UserId}: Page {PageNumber}, Count {Count}",
                userId,
                pageNumber,
                result.Items.Count);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving redemption history for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving your redemptions" });
        }
    }
}
