using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Subscription entity
/// Provides subscription-specific query methods
/// </summary>
public interface ISubscriptionRepository : IRepository<Subscription>
{
    /// <summary>
    /// Get active subscription for a user
    /// </summary>
    Task<Subscription?> GetActiveSubscriptionByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get subscription by Stripe subscription ID
    /// </summary>
    Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get subscription by Stripe customer ID
    /// </summary>
    Task<Subscription?> GetByStripeCustomerIdAsync(string stripeCustomerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all subscriptions scheduled for cancellation that have reached period end
    /// Used for cleanup jobs
    /// </summary>
    Task<List<Subscription>> GetExpiredCancellationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all subscriptions in grace period (PastDue) older than grace period
    /// Used for downgrade jobs
    /// </summary>
    Task<List<Subscription>> GetExpiredGracePeriodSubscriptionsAsync(int gracePeriodDays = 3, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user has any subscription (active or past)
    /// </summary>
    Task<bool> HasSubscriptionHistoryAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get subscription with full history
    /// </summary>
    Task<Subscription?> GetWithHistoryAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
}
