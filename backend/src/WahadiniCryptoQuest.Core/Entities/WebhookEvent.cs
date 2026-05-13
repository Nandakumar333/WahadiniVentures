using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Stores Stripe webhook events for idempotency and audit trail
/// Prevents duplicate processing via unique StripeEventId
/// Supports automatic retry for failed events
/// </summary>
public class WebhookEvent : BaseEntity
{
    // Private constructor for EF Core
    private WebhookEvent()
    {
        StripeEventId = string.Empty;
        EventType = string.Empty;
        PayloadJson = string.Empty;
    }

    // Stripe Event Details
    public string StripeEventId { get; private set; } // Unique Stripe event ID (evt_*)
    public string EventType { get; private set; } // checkout.session.completed, customer.subscription.updated, etc.
    public DateTime EventCreatedAt { get; private set; } // Timestamp from Stripe

    // Processing Status
    public WebhookProcessingStatus Status { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? ErrorMessage { get; private set; }

    // Payload Storage
    public string PayloadJson { get; private set; } // Full JSON payload for audit

    // Association (optional - may not always map to a subscription)
    public Guid? SubscriptionId { get; private set; }
    public virtual Subscription? Subscription { get; private set; }

    /// <summary>
    /// Factory Method - Create webhook event record from incoming Stripe event
    /// </summary>
    public static WebhookEvent Create(
        string stripeEventId,
        string eventType,
        DateTime eventCreatedAt,
        string payloadJson,
        Guid? subscriptionId = null)
    {
        if (string.IsNullOrWhiteSpace(stripeEventId))
            throw new ArgumentException("Stripe Event ID is required", nameof(stripeEventId));
        if (!stripeEventId.StartsWith("evt_"))
            throw new ArgumentException("Invalid Stripe Event ID format (must start with evt_)", nameof(stripeEventId));
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type is required", nameof(eventType));
        if (string.IsNullOrWhiteSpace(payloadJson))
            throw new ArgumentException("Payload JSON is required", nameof(payloadJson));

        var webhookEvent = new WebhookEvent
        {
            StripeEventId = stripeEventId,
            EventType = eventType,
            EventCreatedAt = eventCreatedAt,
            PayloadJson = payloadJson,
            Status = WebhookProcessingStatus.Pending,
            RetryCount = 0,
            SubscriptionId = subscriptionId,
            CreatedBy = "System",
            UpdatedBy = "System"
        };

        return webhookEvent;
    }

    /// <summary>
    /// Mark event as duplicate (already processed)
    /// </summary>
    public void MarkDuplicate()
    {
        if (Status != WebhookProcessingStatus.Pending)
            throw new InvalidOperationException($"Cannot mark {Status} event as duplicate");

        Status = WebhookProcessingStatus.Duplicate;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Begin processing webhook event
    /// </summary>
    public void StartProcessing()
    {
        if (Status != WebhookProcessingStatus.Pending && Status != WebhookProcessingStatus.Failed)
            throw new InvalidOperationException($"Cannot start processing {Status} event");

        Status = WebhookProcessingStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark event as successfully processed
    /// </summary>
    public void MarkProcessed(Guid? subscriptionId = null)
    {
        if (Status != WebhookProcessingStatus.Processing)
            throw new InvalidOperationException($"Cannot mark {Status} event as processed");

        Status = WebhookProcessingStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
        ErrorMessage = null;
        if (subscriptionId.HasValue)
        {
            SubscriptionId = subscriptionId.Value;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark event as failed with error details
    /// Increments retry count for Stripe automatic retry logic
    /// </summary>
    public void MarkFailed(string errorMessage)
    {
        if (Status != WebhookProcessingStatus.Processing)
            throw new InvalidOperationException($"Cannot mark {Status} event as failed");
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message is required when marking event as failed", nameof(errorMessage));

        Status = WebhookProcessingStatus.Failed;
        FailedAt = DateTime.UtcNow;
        ErrorMessage = TruncateErrorMessage(errorMessage, 2000); // Prevent excessively long error messages
        RetryCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if event has exceeded maximum retry attempts
    /// Stripe automatically retries failed webhooks for 3 days
    /// </summary>
    public bool HasExceededMaxRetries(int maxRetries = 10)
    {
        return RetryCount >= maxRetries;
    }

    /// <summary>
    /// Check if event is eligible for retry
    /// </summary>
    public bool CanRetry(int maxRetries = 10)
    {
        return Status == WebhookProcessingStatus.Failed && !HasExceededMaxRetries(maxRetries);
    }

    /// <summary>
    /// Truncate error message to prevent database overflow
    /// </summary>
    private static string TruncateErrorMessage(string message, int maxLength)
    {
        if (message.Length <= maxLength)
            return message;

        return message.Substring(0, maxLength - 3) + "...";
    }

    /// <summary>
    /// Check if event requires idempotency key verification
    /// </summary>
    public bool RequiresIdempotencyCheck()
    {
        return Status == WebhookProcessingStatus.Pending;
    }
}
