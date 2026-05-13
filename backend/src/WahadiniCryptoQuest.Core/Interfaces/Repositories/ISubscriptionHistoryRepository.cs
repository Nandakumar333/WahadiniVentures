using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for SubscriptionHistory entity
/// Provides history-specific query methods for audit and analytics
/// </summary>
public interface ISubscriptionHistoryRepository : IRepository<SubscriptionHistory>
{
    /// <summary>
    /// Get full history for a subscription
    /// </summary>
    Task<List<SubscriptionHistory>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get history by change type (for analytics)
    /// </summary>
    Task<List<SubscriptionHistory>> GetByChangeTypeAsync(string changeType, DateTime since, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user's subscription history across all their subscriptions
    /// </summary>
    Task<List<SubscriptionHistory>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get history entries triggered by webhook
    /// </summary>
    Task<List<SubscriptionHistory>> GetByWebhookEventIdAsync(string webhookEventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get cancellation history for analytics
    /// </summary>
    Task<List<SubscriptionHistory>> GetCancellationsAsync(DateTime since, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get upgrade/downgrade history for analytics
    /// </summary>
    Task<List<SubscriptionHistory>> GetTierChangesAsync(DateTime since, CancellationToken cancellationToken = default);
}
