using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WahadiniCryptoQuest.Core.DTOs.Discount;
using WahadiniCryptoQuest.Service.Discount.Commands;
using WahadiniCryptoQuest.Service.Discount.Queries;

namespace WahadiniCryptoQuest.API.Controllers;

/// <summary>
/// Admin-only endpoints for managing discount codes
/// </summary>
[ApiController]
[Route("api/admin/discounts")]
[Authorize(Roles = "Admin")]
public class AdminDiscountController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminDiscountController> _logger;

    public AdminDiscountController(IMediator mediator, ILogger<AdminDiscountController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all discount codes (admin only)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all discount codes</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<AdminDiscountTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<AdminDiscountTypeDto>>> GetAllDiscountCodes(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin retrieving all discount codes");

        try
        {
            var query = new GetAllDiscountCodesQuery();
            var result = await _mediator.Send(query, cancellationToken);

            _logger.LogInformation("Retrieved {Count} discount codes", result.Count);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve discount codes");
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Failed to retrieve discount codes" });
        }
    }

    /// <summary>
    /// Create a new discount code
    /// </summary>
    /// <param name="request">Discount code creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created discount code</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AdminDiscountTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminDiscountTypeDto>> CreateDiscountCode(
        [FromBody] CreateDiscountCodeDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin creating discount code: {Code}", request.Code);

        try
        {
            var command = new CreateDiscountTypeCommand
            {
                Code = request.Code,
                DiscountPercentage = request.DiscountPercentage,
                RequiredPoints = request.RequiredPoints,
                MaxRedemptions = request.MaxRedemptions,
                ExpiryDate = request.ExpiryDate
            };

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Successfully created discount code {Code} with ID {Id}", result.Code, result.Id);

            return CreatedAtAction(nameof(CreateDiscountCode), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning(ex, "Discount code {Code} already exists", request.Code);
            return Conflict(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create discount code {Code}", request.Code);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Failed to create discount code" });
        }
    }

    /// <summary>
    /// Update an existing discount code
    /// </summary>
    /// <param name="id">Discount code ID</param>
    /// <param name="request">Update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated discount code</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AdminDiscountTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminDiscountTypeDto>> UpdateDiscountCode(
        Guid id,
        [FromBody] UpdateDiscountCodeDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin updating discount code: {Id}", id);

        try
        {
            var command = new UpdateDiscountTypeCommand(id)
            {
                DiscountPercentage = request.DiscountPercentage,
                RequiredPoints = request.RequiredPoints,
                MaxRedemptions = request.MaxRedemptions,
                ExpiryDate = request.ExpiryDate
            };

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Successfully updated discount code {Id}", id);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Discount code {Id} not found", id);
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update discount code {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Failed to update discount code" });
        }
    }

    /// <summary>
    /// Soft delete a discount code (preserves historical redemptions)
    /// </summary>
    /// <param name="id">Discount code ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDiscountCode(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin soft deleting discount code: {Id}", id);

        try
        {
            var command = new DeleteDiscountTypeCommand(id);
            await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Successfully soft deleted discount code {Id}", id);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Discount code {Id} not found", id);
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to soft delete discount code {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Failed to delete discount code" });
        }
    }

    /// <summary>
    /// Activate a discount code
    /// </summary>
    /// <param name="id">Discount code ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateDiscountCode(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin activating discount code: {Id}", id);

        try
        {
            var command = new ActivateDiscountCommand(id);
            await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Successfully activated discount code {Id}", id);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Discount code {Id} not found", id);
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate discount code {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Failed to activate discount code" });
        }
    }

    /// <summary>
    /// Deactivate a discount code
    /// </summary>
    /// <param name="id">Discount code ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateDiscountCode(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin deactivating discount code: {Id}", id);

        try
        {
            var command = new DeactivateDiscountCommand(id);
            await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Successfully deactivated discount code {Id}", id);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Discount code {Id} not found", id);
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate discount code {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Failed to deactivate discount code" });
        }
    }

    /// <summary>
    /// Get analytics for a specific discount code
    /// </summary>
    /// <param name="id">Discount code ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Discount analytics</returns>
    [HttpGet("{id:guid}/analytics")]
    [ProducesResponseType(typeof(DiscountAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DiscountAnalyticsDto>> GetDiscountAnalytics(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin retrieving analytics for discount code: {Id}", id);

        try
        {
            var query = new GetDiscountAnalyticsQuery { DiscountCodeId = id };
            var result = await _mediator.Send(query, cancellationToken);

            _logger.LogInformation(
                "Retrieved analytics for discount {Code}: {Redemptions} redemptions",
                result.Code, result.TotalRedemptions);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Discount code {Id} not found", id);
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve analytics for discount code {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Failed to retrieve analytics" });
        }
    }

    /// <summary>
    /// Get summary analytics for all discount codes
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Analytics summary</returns>
    [HttpGet("analytics/summary")]
    [ProducesResponseType(typeof(AnalyticsSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AnalyticsSummaryDto>> GetAnalyticsSummary(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin retrieving analytics summary");

        try
        {
            var query = new GetAnalyticsSummaryQuery();
            var result = await _mediator.Send(query, cancellationToken);

            _logger.LogInformation(
                "Retrieved analytics summary: {Redemptions} total redemptions, {Points} points burned",
                result.TotalRedemptions, result.TotalPointsBurned);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve analytics summary");
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Failed to retrieve analytics summary" });
        }
    }
}
