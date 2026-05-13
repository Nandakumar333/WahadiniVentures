using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for WebhookEvent entity
/// Provides webhook-specific query methods for idempotency and monitoring
/// </summary>
public interface IWebhookEventRepository : IRepository<WebhookEvent>
{
    /// <summary>
    /// Get webhook event by Stripe event ID (for idempotency check)
    /// </summary>
    Task<WebhookEvent?> GetByStripeEventIdAsync(string stripeEventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if webhook event has been processed (for quick idempotency check)
    /// </summary>
    Task<bool> IsEventProcessedAsync(string stripeEventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get failed webhook events eligible for retry
    /// </summary>
    Task<List<WebhookEvent>> GetFailedEventsForRetryAsync(int maxRetries = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get webhook events by subscription ID
    /// </summary>
    Task<List<WebhookEvent>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recent webhook events by type for monitoring
    /// </summary>
    Task<List<WebhookEvent>> GetRecentByEventTypeAsync(string eventType, int count = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get webhook processing statistics for monitoring
    /// </summary>
    Task<Dictionary<WebhookProcessingStatus, int>> GetProcessingStatsAsync(DateTime since, CancellationToken cancellationToken = default);
}
