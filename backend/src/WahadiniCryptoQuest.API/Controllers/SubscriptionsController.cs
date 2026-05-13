using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using WahadiniCryptoQuest.Core.DTOs.Subscription;
using WahadiniCryptoQuest.Service.Commands.Subscription;
using WahadiniCryptoQuest.Service.Queries.Subscription;

namespace WahadiniCryptoQuest.API.Controllers;

/// <summary>
/// Subscription management endpoints
/// Handles checkout, status, and pricing
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(IMediator mediator, ILogger<SubscriptionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create Stripe checkout session for subscription purchase
    /// </summary>
    /// <param name="dto">Checkout session details</param>
    /// <returns>Checkout session URL</returns>
    [HttpPost("checkout")]
    [Authorize]
    [ProducesResponseType(typeof(CheckoutSessionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CheckoutSessionResponseDto>> CreateCheckoutSession([FromBody] CreateCheckoutSessionDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var command = new CreateCheckoutSessionCommand
        {
            UserId = userId,
            Tier = dto.Tier,
            CurrencyCode = dto.CurrencyCode,
            DiscountCode = dto.DiscountCode
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid checkout request from user {UserId}", userId);
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Checkout operation failed for user {UserId}", userId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get current user's subscription status
    /// </summary>
    /// <returns>Subscription status or null if no subscription</returns>
    [HttpGet("status")]
    [Authorize]
    [ProducesResponseType(typeof(SubscriptionStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SubscriptionStatusDto>> GetSubscriptionStatus()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var query = new GetSubscriptionStatusQuery { UserId = userId };
        var result = await _mediator.Send(query);

        if (result == null)
            return NoContent();

        return Ok(result);
    }

    /// <summary>
    /// Get all available currency pricings
    /// Public endpoint for pricing page display
    /// </summary>
    /// <returns>List of active currency pricings</returns>
    [HttpGet("pricing")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CurrencyPricingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CurrencyPricingDto>>> GetPricing()
    {
        var query = new GetActiveCurrencyPricingsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Validate a discount code for checkout
    /// </summary>
    /// <param name="dto">Discount code and tier</param>
    /// <returns>Validation result with discount amount</returns>
    [HttpPost("validate-discount")]
    [Authorize]
    [ProducesResponseType(typeof(DiscountValidationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DiscountValidationResultDto>> ValidateDiscount([FromBody] ValidateDiscountRequestDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var command = new ValidateDiscountForCheckoutCommand
        {
            UserId = userId,
            DiscountCode = dto.DiscountCode,
            Tier = dto.Tier
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Create billing portal session for subscription management
    /// </summary>
    /// <param name="dto">Return URL after portal session</param>
    /// <returns>Portal session URL</returns>
    [HttpPost("portal")]
    [Authorize]
    [ProducesResponseType(typeof(PortalSessionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PortalSessionResponseDto>> CreatePortalSession([FromBody] CreatePortalSessionDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var command = new CreatePortalSessionCommand
        {
            UserId = userId,
            ReturnUrl = dto.ReturnUrl
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Portal session creation failed for user {UserId}", userId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cancel subscription at period end
    /// </summary>
    /// <param name="dto">Cancellation reason</param>
    /// <returns>Success status</returns>
    [HttpPost("cancel")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CancelSubscription([FromBody] CancelSubscriptionDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var command = new CancelSubscriptionCommand
        {
            UserId = userId,
            Reason = dto.Reason
        };

        try
        {
            await _mediator.Send(command);
            return Ok(new { message = "Subscription cancelled successfully. You will retain access until the end of your billing period." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cancellation failed for user {UserId}", userId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Reactivate a cancelled subscription
    /// </summary>
    /// <returns>Success status</returns>
    [HttpPost("reactivate")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> ReactivateSubscription()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var command = new ReactivateSubscriptionCommand
        {
            UserId = userId
        };

        try
        {
            await _mediator.Send(command);
            return Ok(new { message = "Subscription reactivated successfully. You will not be charged until the next billing period." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Reactivation failed for user {UserId}", userId);
            return BadRequest(new { error = ex.Message });
        }
    }
}

/// <summary>
/// DTO for creating portal session
/// </summary>
public class CreatePortalSessionDto
{
    public string ReturnUrl { get; set; } = string.Empty;
}

/// <summary>
/// DTO for cancelling subscription
/// </summary>
public class CancelSubscriptionDto
{
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// DTO for discount validation request
/// </summary>
public class ValidateDiscountRequestDto
{
    public string DiscountCode { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;
}
