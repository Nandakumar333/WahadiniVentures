using MediatR;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using WahadiniCryptoQuest.Service.Commands.Subscription;

namespace WahadiniCryptoQuest.API.Controllers;

/// <summary>
/// Handles Stripe webhook events for subscription lifecycle management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IMediator mediator,
        IConfiguration configuration,
        ILogger<WebhooksController> logger)
    {
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Stripe webhook endpoint - receives and processes Stripe events
    /// </summary>
    /// <returns>200 OK if event received, 400 if signature invalid</returns>
    [HttpPost("stripe")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            var stripeSignature = Request.Headers["Stripe-Signature"].ToString();
            var webhookSecret = _configuration["Stripe:WebhookSecret"];

            if (string.IsNullOrEmpty(webhookSecret))
            {
                _logger.LogError("Stripe webhook secret not configured");
                return StatusCode(500, new { error = "Webhook secret not configured" });
            }

            // Verify webhook signature
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                stripeSignature,
                webhookSecret,
                throwOnApiVersionMismatch: false
            );

            _logger.LogInformation("Received Stripe webhook: {EventType} ({EventId})",
                stripeEvent.Type, stripeEvent.Id);

            // Process event asynchronously via MediatR
            var command = new ProcessWebhookEventCommand
            {
                StripeEventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                PayloadJson = json,
                EventCreatedAt = stripeEvent.Created
            };

            await _mediator.Send(command);

            // Always return 200 to Stripe immediately
            // Actual processing happens in background
            return Ok(new { received = true });
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook signature verification failed");
            return BadRequest(new { error = "Invalid signature" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");

            // Still return 200 to prevent Stripe from retrying
            // Event will be marked as failed in database
            return Ok(new { received = true, error = "Processing error" });
        }
    }
}
