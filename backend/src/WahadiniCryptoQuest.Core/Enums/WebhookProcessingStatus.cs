namespace WahadiniCryptoQuest.Core.Enums;

/// <summary>
/// Webhook event processing status for idempotency and audit tracking
/// </summary>
public enum WebhookProcessingStatus
{
    /// <summary>
    /// Webhook event received and queued for processing
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Webhook event is currently being processed
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Webhook event processed successfully
    /// All database changes committed
    /// </summary>
    Processed = 2,

    /// <summary>
    /// Webhook event processing failed
    /// Database changes rolled back
    /// Will trigger Stripe retry (returned HTTP 500)
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Webhook event is duplicate (idempotency check)
    /// No processing performed
    /// </summary>
    Duplicate = 4
}
