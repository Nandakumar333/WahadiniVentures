using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Aggregate root for subscription management
/// Represents a user's premium membership with Stripe integration
/// Follows Domain-Driven Design with encapsulation and factory methods
/// </summary>
public class Subscription : BaseEntity
{
    // Private constructor for EF Core
    private Subscription()
    {
        CurrencyCode = string.Empty;
        StripeCustomerId = string.Empty;
        StripeSubscriptionId = string.Empty;
        StripePriceId = string.Empty;
    }

    // Identity
    public Guid UserId { get; private set; }

    // Payment Provider Integration
    public string StripeCustomerId { get; private set; }
    public string StripeSubscriptionId { get; private set; }
    public string StripePriceId { get; private set; }

    // Subscription Details
    public SubscriptionTier Tier { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public string CurrencyCode { get; private set; } // ISO 4217 (USD, INR, EUR, JPY, GBP)

    // Billing Cycle
    public DateTime CurrentPeriodStart { get; private set; }
    public DateTime CurrentPeriodEnd { get; private set; }
    public bool IsCancelledAtPeriodEnd { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    // Navigation Properties
    public virtual User User { get; private set; } = null!;

    /// <summary>
    /// Factory Method - Create new subscription from successful checkout
    /// </summary>
    public static Subscription Create(
        Guid userId,
        string stripeCustomerId,
        string stripeSubscriptionId,
        string stripePriceId,
        SubscriptionTier tier,
        string currencyCode,
        DateTime periodStart,
        DateTime periodEnd)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(stripeCustomerId))
            throw new ArgumentException("Stripe Customer ID is required", nameof(stripeCustomerId));
        if (string.IsNullOrWhiteSpace(stripeSubscriptionId))
            throw new ArgumentException("Stripe Subscription ID is required", nameof(stripeSubscriptionId));
        if (string.IsNullOrWhiteSpace(stripePriceId))
            throw new ArgumentException("Stripe Price ID is required", nameof(stripePriceId));
        if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Length != 3)
            throw new ArgumentException("Currency code must be 3-character ISO 4217 code", nameof(currencyCode));
        if (periodEnd <= periodStart)
            throw new ArgumentException("Period end must be after period start", nameof(periodEnd));

        var subscription = new Subscription
        {
            UserId = userId,
            StripeCustomerId = stripeCustomerId,
            StripeSubscriptionId = stripeSubscriptionId,
            StripePriceId = stripePriceId,
            Tier = tier,
            Status = SubscriptionStatus.Active,
            CurrencyCode = currencyCode.ToUpperInvariant(),
            CurrentPeriodStart = periodStart,
            CurrentPeriodEnd = periodEnd,
            IsCancelledAtPeriodEnd = false,
            CreatedBy = userId.ToString(),
            UpdatedBy = userId.ToString()
        };

        return subscription;
    }

    /// <summary>
    /// Activate subscription after successful payment
    /// </summary>
    public void Activate(DateTime periodStart, DateTime periodEnd)
    {
        if (Status == SubscriptionStatus.Active && !IsCancelledAtPeriodEnd)
            throw new InvalidOperationException("Subscription is already active");

        Status = SubscriptionStatus.Active;
        CurrentPeriodStart = periodStart;
        CurrentPeriodEnd = periodEnd;
        IsCancelledAtPeriodEnd = false;
        CancelledAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark subscription for cancellation at period end
    /// User retains premium access until CurrentPeriodEnd
    /// </summary>
    public void CancelAtPeriodEnd()
    {
        if (Status != SubscriptionStatus.Active)
            throw new InvalidOperationException("Only active subscriptions can be cancelled");
        if (IsCancelledAtPeriodEnd)
            throw new InvalidOperationException("Subscription is already scheduled for cancellation");

        IsCancelledAtPeriodEnd = true;
        CancelledAt = DateTime.UtcNow;
        Status = SubscriptionStatus.Canceled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivate subscription that was scheduled for cancellation
    /// </summary>
    public void Reactivate()
    {
        if (!IsCancelledAtPeriodEnd)
            throw new InvalidOperationException("Subscription is not scheduled for cancellation");
        if (DateTime.UtcNow > CurrentPeriodEnd)
            throw new InvalidOperationException("Cannot reactivate expired subscription");

        IsCancelledAtPeriodEnd = false;
        CancelledAt = null;
        Status = SubscriptionStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Renew subscription for next billing period
    /// Called when invoice.payment_succeeded webhook received
    /// </summary>
    public void Renew(DateTime newPeriodEnd)
    {
        if (Status != SubscriptionStatus.Active && Status != SubscriptionStatus.PastDue)
            throw new InvalidOperationException($"Cannot renew subscription in {Status} status");
        if (newPeriodEnd <= CurrentPeriodEnd)
            throw new ArgumentException("New period end must be after current period end", nameof(newPeriodEnd));

        CurrentPeriodStart = CurrentPeriodEnd;
        CurrentPeriodEnd = newPeriodEnd;
        Status = SubscriptionStatus.Active;
        IsCancelledAtPeriodEnd = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark subscription as past due when payment fails
    /// User retains access during 3-day grace period
    /// </summary>
    public void MarkPastDue()
    {
        if (Status != SubscriptionStatus.Active)
            throw new InvalidOperationException($"Cannot mark {Status} subscription as past due");

        Status = SubscriptionStatus.PastDue;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Expire subscription after grace period or cancellation period end
    /// User loses premium access and downgrades to Free tier
    /// </summary>
    public void Expire()
    {
        if (Status == SubscriptionStatus.Expired)
            throw new InvalidOperationException("Subscription is already expired");

        Status = SubscriptionStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update Stripe metadata when subscription changes externally
    /// </summary>
    public void UpdateFromStripe(
        SubscriptionStatus newStatus,
        string stripePriceId,
        DateTime periodStart,
        DateTime periodEnd,
        bool cancelAtPeriodEnd)
    {
        Status = newStatus;
        StripePriceId = stripePriceId;
        CurrentPeriodStart = periodStart;
        CurrentPeriodEnd = periodEnd;
        IsCancelledAtPeriodEnd = cancelAtPeriodEnd;
        if (cancelAtPeriodEnd && CancelledAt == null)
        {
            CancelledAt = DateTime.UtcNow;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if subscription provides premium access
    /// True during Active and PastDue states (grace period)
    /// Also true for Canceled subscriptions until period end
    /// </summary>
    public bool HasPremiumAccess()
    {
        return Status switch
        {
            SubscriptionStatus.Active => true,
            SubscriptionStatus.PastDue => true, // Grace period
            SubscriptionStatus.Canceled => DateTime.UtcNow < CurrentPeriodEnd, // Until period end
            _ => false
        };
    }

    /// <summary>
    /// Check if subscription is in grace period after payment failure
    /// </summary>
    public bool IsInGracePeriod()
    {
        return Status == SubscriptionStatus.PastDue;
    }
}
