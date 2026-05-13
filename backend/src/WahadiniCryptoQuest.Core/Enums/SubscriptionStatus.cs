namespace WahadiniCryptoQuest.Core.Enums;

/// <summary>
/// Subscription status values aligned with Stripe subscription statuses
/// https://stripe.com/docs/billing/subscriptions/overview#subscription-statuses
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>
    /// Subscription is active and user has full premium access
    /// Stripe status: 'active'
    /// </summary>
    Active = 0,

    /// <summary>
    /// Payment failed but within grace period (3 days)
    /// User retains premium access during grace period
    /// Stripe status: 'past_due'
    /// </summary>
    PastDue = 1,

    /// <summary>
    /// Subscription cancelled but active until period end
    /// User retains premium access until CurrentPeriodEnd
    /// Stripe status: 'canceled' with cancel_at_period_end = true
    /// </summary>
    Canceled = 2,

    /// <summary>
    /// Initial payment failed or incomplete
    /// Stripe status: 'incomplete' or 'incomplete_expired'
    /// </summary>
    Incomplete = 3,

    /// <summary>
    /// Subscription expired, user downgraded to Free tier
    /// Used when past_due grace period expires or period ends after cancellation
    /// </summary>
    Expired = 4
}
