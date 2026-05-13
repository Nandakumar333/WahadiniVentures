using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Commands.Subscription;
using System.Text.Json;

namespace WahadiniCryptoQuest.Service.Handlers.Subscription;

/// <summary>
/// Handler for processing Stripe webhook events with idempotency and retry logic
/// </summary>
public class ProcessWebhookEventHandler : IRequestHandler<ProcessWebhookEventCommand, bool>
{
    private readonly IWebhookEventRepository _webhookEventRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionHistoryRepository _historyRepository;
    private readonly IDiscountCodeRepository _discountCodeRepository;
    private readonly IUserDiscountRedemptionRepository _redemptionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessWebhookEventHandler> _logger;

    public ProcessWebhookEventHandler(
        IWebhookEventRepository webhookEventRepository,
        ISubscriptionRepository subscriptionRepository,
        ISubscriptionHistoryRepository historyRepository,
        IDiscountCodeRepository discountCodeRepository,
        IUserDiscountRedemptionRepository redemptionRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProcessWebhookEventHandler> logger)
    {
        _webhookEventRepository = webhookEventRepository;
        _subscriptionRepository = subscriptionRepository;
        _historyRepository = historyRepository;
        _discountCodeRepository = discountCodeRepository;
        _redemptionRepository = redemptionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ProcessWebhookEventCommand request, CancellationToken cancellationToken)
    {
        // Check if event already processed (idempotency)
        var existingEvent = await _webhookEventRepository.GetByStripeEventIdAsync(request.StripeEventId);
        if (existingEvent != null)
        {
            if (await _webhookEventRepository.IsEventProcessedAsync(request.StripeEventId))
            {
                _logger.LogInformation("Webhook event {EventId} already processed. Marking as duplicate.", request.StripeEventId);

                if (existingEvent.Status == WebhookProcessingStatus.Pending ||
                    existingEvent.Status == WebhookProcessingStatus.Processing)
                {
                    existingEvent.MarkDuplicate();
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                return true; // Already processed, return success
            }

            // Event exists but failed - retry
            existingEvent.StartProcessing();
        }
        else
        {
            // Create new webhook event record
            existingEvent = WebhookEvent.Create(
                request.StripeEventId,
                request.EventType,
                request.EventCreatedAt,
                request.PayloadJson
            );
            existingEvent.StartProcessing();
            await _webhookEventRepository.AddAsync(existingEvent);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            // Route to appropriate handler based on event type
            var processed = await RouteEventAsync(request.EventType, request.PayloadJson, cancellationToken);

            if (processed)
            {
                existingEvent.MarkProcessed();
                _logger.LogInformation("Successfully processed webhook event {EventId} of type {EventType}",
                    request.StripeEventId, request.EventType);
            }
            else
            {
                existingEvent.MarkFailed("Event type not handled or processing returned false");
                _logger.LogWarning("Webhook event {EventId} of type {EventType} was not processed",
                    request.StripeEventId, request.EventType);
            }
        }
        catch (Exception ex)
        {
            existingEvent.MarkFailed(ex.Message);
            _logger.LogError(ex, "Error processing webhook event {EventId} of type {EventType}",
                request.StripeEventId, request.EventType);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw; // Re-throw to trigger Stripe retry
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<bool> RouteEventAsync(string eventType, string payloadJson, CancellationToken cancellationToken)
    {
        return eventType switch
        {
            "checkout.session.completed" => await HandleCheckoutSessionCompletedAsync(payloadJson, cancellationToken),
            "invoice.payment_succeeded" => await HandleInvoicePaymentSucceededAsync(payloadJson, cancellationToken),
            "invoice.payment_failed" => await HandleInvoicePaymentFailedAsync(payloadJson, cancellationToken),
            "customer.subscription.updated" => await HandleSubscriptionUpdatedAsync(payloadJson, cancellationToken),
            "customer.subscription.deleted" => await HandleSubscriptionDeletedAsync(payloadJson, cancellationToken),
            _ => false // Unhandled event type
        };
    }

    private async Task<bool> HandleCheckoutSessionCompletedAsync(string payloadJson, CancellationToken cancellationToken)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(payloadJson);
            var data = jsonDoc.RootElement.GetProperty("data").GetProperty("object");

            var stripeSubscriptionId = data.GetProperty("subscription").GetString();
            var customerId = data.GetProperty("customer").GetString();
            var clientReferenceId = data.TryGetProperty("client_reference_id", out var refId)
                ? refId.GetString()
                : null;

            // Extract discount code from metadata if present
            string? discountCode = null;
            if (data.TryGetProperty("metadata", out var metadata))
            {
                if (metadata.TryGetProperty("discount_code", out var codeProperty))
                {
                    discountCode = codeProperty.GetString();
                }
            }

            if (string.IsNullOrEmpty(stripeSubscriptionId))
            {
                _logger.LogWarning("Checkout session completed but no subscription ID found");
                return false;
            }

            // Check if subscription already exists
            var existingSubscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId);
            if (existingSubscription != null)
            {
                _logger.LogInformation("Subscription {StripeSubscriptionId} already exists, skipping creation",
                    stripeSubscriptionId);
                return true;
            }

            // Parse user ID from client_reference_id (set during checkout)
            if (string.IsNullOrEmpty(clientReferenceId) || !Guid.TryParse(clientReferenceId, out var userId))
            {
                _logger.LogError("Cannot create subscription: Invalid or missing client_reference_id");
                return false;
            }

            // Get subscription period dates
            var periodStart = DateTime.UtcNow;
            var periodEnd = DateTime.UtcNow.AddMonths(1); // Default, will be updated by subscription.updated

            // Create subscription - will be updated with full details by subscription.updated webhook
            var subscription = Core.Entities.Subscription.Create(
                userId,
                customerId ?? string.Empty,
                stripeSubscriptionId,
                "price_placeholder", // Will be updated by subscription.updated webhook
                SubscriptionTier.MonthlyPremium, // Will be updated by subscription.updated webhook
                "USD", // Will be updated by subscription.updated webhook
                periodStart,
                periodEnd
            );

            await _subscriptionRepository.AddAsync(subscription);

            // Record discount code usage if provided
            if (!string.IsNullOrEmpty(discountCode))
            {
                await RecordDiscountUsageAsync(userId, discountCode, subscription.Id, cancellationToken);
            }

            // Record creation in history
            var notes = string.IsNullOrEmpty(discountCode)
                ? $"Created from checkout session {stripeSubscriptionId}"
                : $"Created from checkout session {stripeSubscriptionId} with discount code {discountCode}";

            var history = SubscriptionHistory.RecordCreation(
                subscription.Id,
                subscription.Tier,
                subscription.CurrentPeriodEnd,
                "Stripe Webhook",
                notes
            );
            await _historyRepository.AddAsync(history);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created subscription {SubscriptionId} for user {UserId} from checkout session",
                subscription.Id, userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling checkout.session.completed event");
            throw;
        }
    }

    private async Task<bool> HandleInvoicePaymentSucceededAsync(string payloadJson, CancellationToken cancellationToken)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(payloadJson);
            var data = jsonDoc.RootElement.GetProperty("data").GetProperty("object");

            var stripeSubscriptionId = data.GetProperty("subscription").GetString();
            if (string.IsNullOrEmpty(stripeSubscriptionId))
            {
                return false; // Not a subscription invoice
            }

            var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {StripeSubscriptionId} not found for invoice payment",
                    stripeSubscriptionId);
                return false;
            }

            // Get period dates from invoice
            var periodStart = DateTimeOffset.FromUnixTimeSeconds(
                data.GetProperty("period_start").GetInt64()).UtcDateTime;
            var periodEnd = DateTimeOffset.FromUnixTimeSeconds(
                data.GetProperty("period_end").GetInt64()).UtcDateTime;

            var oldStatus = subscription.Status;

            // Activate or renew subscription
            if (subscription.Status == SubscriptionStatus.Incomplete ||
                subscription.Status == SubscriptionStatus.PastDue)
            {
                subscription.Activate(periodStart, periodEnd);

                var history = SubscriptionHistory.RecordActivation(
                    subscription.Id,
                    oldStatus,
                    subscription.CurrentPeriodEnd,
                    "Stripe Webhook",
                    stripeSubscriptionId,
                    "Activated from invoice payment"
                );
                await _historyRepository.AddAsync(history);
            }
            else
            {
                var previousPeriodEnd = subscription.CurrentPeriodEnd;
                subscription.Renew(periodEnd);

                var history = SubscriptionHistory.RecordRenewal(
                    subscription.Id,
                    previousPeriodEnd,
                    subscription.CurrentPeriodEnd,
                    "Stripe Webhook",
                    stripeSubscriptionId,
                    "Renewed from invoice payment"
                );
                await _historyRepository.AddAsync(history);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Processed payment succeeded for subscription {SubscriptionId}", subscription.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling invoice.payment_succeeded event");
            throw;
        }
    }

    private async Task<bool> HandleInvoicePaymentFailedAsync(string payloadJson, CancellationToken cancellationToken)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(payloadJson);
            var data = jsonDoc.RootElement.GetProperty("data").GetProperty("object");

            var stripeSubscriptionId = data.GetProperty("subscription").GetString();
            if (string.IsNullOrEmpty(stripeSubscriptionId))
            {
                return false;
            }

            var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {StripeSubscriptionId} not found for failed payment",
                    stripeSubscriptionId);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.MarkPastDue();

            var history = SubscriptionHistory.RecordPastDue(
                subscription.Id,
                "Stripe Webhook",
                stripeSubscriptionId,
                "Marked past due after payment failure"
            );
            await _historyRepository.AddAsync(history);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("Marked subscription {SubscriptionId} as past due after payment failure",
                subscription.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling invoice.payment_failed event");
            throw;
        }
    }

    private async Task<bool> HandleSubscriptionUpdatedAsync(string payloadJson, CancellationToken cancellationToken)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(payloadJson);
            var data = jsonDoc.RootElement.GetProperty("data").GetProperty("object");

            var stripeSubscriptionId = data.GetProperty("id").GetString();
            if (string.IsNullOrEmpty(stripeSubscriptionId))
            {
                return false;
            }

            var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {StripeSubscriptionId} not found for update",
                    stripeSubscriptionId);
                return false;
            }

            var stripeStatus = data.GetProperty("status").GetString();
            var cancelAtPeriodEnd = data.GetProperty("cancel_at_period_end").GetBoolean();
            var currentPeriodStart = DateTimeOffset.FromUnixTimeSeconds(
                data.GetProperty("current_period_start").GetInt64()).UtcDateTime;
            var currentPeriodEnd = DateTimeOffset.FromUnixTimeSeconds(
                data.GetProperty("current_period_end").GetInt64()).UtcDateTime;

            // Parse Stripe status to our enum
            var status = stripeStatus?.ToLower() switch
            {
                "active" => SubscriptionStatus.Active,
                "past_due" => SubscriptionStatus.PastDue,
                "canceled" => SubscriptionStatus.Canceled,
                "incomplete" => SubscriptionStatus.Incomplete,
                _ => SubscriptionStatus.Incomplete
            };

            // Update subscription from Stripe data
            subscription.UpdateFromStripe(
                status,
                stripeStatus ?? "active",
                currentPeriodStart,
                currentPeriodEnd,
                cancelAtPeriodEnd
            );

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated subscription {SubscriptionId} from Stripe webhook", subscription.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling customer.subscription.updated event");
            throw;
        }
    }

    private async Task<bool> HandleSubscriptionDeletedAsync(string payloadJson, CancellationToken cancellationToken)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(payloadJson);
            var data = jsonDoc.RootElement.GetProperty("data").GetProperty("object");

            var stripeSubscriptionId = data.GetProperty("id").GetString();
            if (string.IsNullOrEmpty(stripeSubscriptionId))
            {
                return false;
            }

            var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {StripeSubscriptionId} not found for deletion",
                    stripeSubscriptionId);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Expire();

            var history = SubscriptionHistory.RecordExpiration(
                subscription.Id,
                oldStatus,
                "Stripe Webhook",
                stripeSubscriptionId,
                "Expired from Stripe deletion"
            );
            await _historyRepository.AddAsync(history);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Expired subscription {SubscriptionId} after Stripe deletion", subscription.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling customer.subscription.deleted event");
            throw;
        }
    }

    /// <summary>
    /// Records discount code usage when checkout completes
    /// </summary>
    private async Task RecordDiscountUsageAsync(Guid userId, string discountCode, Guid subscriptionId, CancellationToken cancellationToken)
    {
        try
        {
            // Find the discount code
            var discount = await _discountCodeRepository.GetByCodeAsync(discountCode, cancellationToken);
            if (discount == null)
            {
                _logger.LogWarning("Discount code {Code} not found when recording usage for user {UserId}",
                    discountCode, userId);
                return;
            }

            // Check if already redeemed (idempotency)
            var alreadyRedeemed = await _redemptionRepository.HasUserRedeemedDiscountAsync(userId, discount.Id, cancellationToken);
            if (alreadyRedeemed)
            {
                _logger.LogInformation("User {UserId} already has redemption for discount {DiscountId}, skipping",
                    userId, discount.Id);
                return;
            }

            // Create redemption record
            var redemption = new UserDiscountRedemption
            {
                UserId = userId,
                DiscountCodeId = discount.Id,
                RedeemedAt = DateTime.UtcNow,
                UsedInSubscription = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            };

            await _redemptionRepository.AddAsync(redemption);

            // Increment discount code usage counter
            discount.IncrementRedemptions();

            _logger.LogInformation("Recorded discount code {Code} redemption for user {UserId} on subscription {SubscriptionId}",
                discountCode, userId, subscriptionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording discount usage for code {Code} and user {UserId}",
                discountCode, userId);
            // Don't throw - discount tracking failure shouldn't break subscription creation
        }
    }
}
