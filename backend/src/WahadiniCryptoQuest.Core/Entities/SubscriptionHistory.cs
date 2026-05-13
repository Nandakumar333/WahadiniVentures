using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Audit trail for subscription state changes
/// Tracks tier upgrades/downgrades, cancellations, and renewals
/// Immutable log for compliance and analytics
/// </summary>
public class SubscriptionHistory : BaseEntity
{
    // Private constructor for EF Core
    private SubscriptionHistory()
    {
        ChangeType = string.Empty;
    }

    // Association
    public Guid SubscriptionId { get; private set; }
    public virtual Subscription Subscription { get; private set; } = null!;

    // State Change Details
    public string ChangeType { get; private set; } // Created, Activated, Renewed, Canceled, Expired, PastDue, Upgraded, Downgraded
    public SubscriptionTier? PreviousTier { get; private set; }
    public SubscriptionTier? NewTier { get; private set; }
    public SubscriptionStatus? PreviousStatus { get; private set; }
    public SubscriptionStatus? NewStatus { get; private set; }
    public DateTime? PreviousPeriodEnd { get; private set; }
    public DateTime? NewPeriodEnd { get; private set; }

    // Metadata
    public string? Notes { get; private set; } // Additional context (e.g., "User cancelled via Billing Portal")
    public string? TriggeredBy { get; private set; } // UserId, System, Stripe Webhook
    public string? WebhookEventId { get; private set; } // Reference to webhook that triggered change

    /// <summary>
    /// Factory Method - Record subscription creation
    /// </summary>
    public static SubscriptionHistory RecordCreation(
        Guid subscriptionId,
        SubscriptionTier tier,
        DateTime periodEnd,
        string triggeredBy,
        string? notes = null)
    {
        var history = new SubscriptionHistory
        {
            SubscriptionId = subscriptionId,
            ChangeType = "Created",
            NewTier = tier,
            NewStatus = SubscriptionStatus.Active,
            NewPeriodEnd = periodEnd,
            TriggeredBy = triggeredBy,
            Notes = notes,
            CreatedBy = triggeredBy,
            UpdatedBy = triggeredBy
        };

        return history;
    }

    /// <summary>
    /// Factory Method - Record subscription activation
    /// </summary>
    public static SubscriptionHistory RecordActivation(
        Guid subscriptionId,
        SubscriptionStatus previousStatus,
        DateTime periodEnd,
        string triggeredBy,
        string? webhookEventId = null,
        string? notes = null)
    {
        var history = new SubscriptionHistory
        {
            SubscriptionId = subscriptionId,
            ChangeType = "Activated",
            PreviousStatus = previousStatus,
            NewStatus = SubscriptionStatus.Active,
            NewPeriodEnd = periodEnd,
            TriggeredBy = triggeredBy,
            WebhookEventId = webhookEventId,
            Notes = notes,
            CreatedBy = triggeredBy,
            UpdatedBy = triggeredBy
        };

        return history;
    }

    /// <summary>
    /// Factory Method - Record subscription renewal
    /// </summary>
    public static SubscriptionHistory RecordRenewal(
        Guid subscriptionId,
        DateTime previousPeriodEnd,
        DateTime newPeriodEnd,
        string triggeredBy,
        string? webhookEventId = null,
        string? notes = null)
    {
        var history = new SubscriptionHistory
        {
            SubscriptionId = subscriptionId,
            ChangeType = "Renewed",
            PreviousPeriodEnd = previousPeriodEnd,
            NewPeriodEnd = newPeriodEnd,
            NewStatus = SubscriptionStatus.Active,
            TriggeredBy = triggeredBy,
            WebhookEventId = webhookEventId,
            Notes = notes,
            CreatedBy = triggeredBy,
            UpdatedBy = triggeredBy
        };

        return history;
    }

    /// <summary>
    /// Factory Method - Record subscription cancellation
    /// </summary>
    public static SubscriptionHistory RecordCancellation(
        Guid subscriptionId,
        DateTime periodEnd,
        string triggeredBy,
        string? webhookEventId = null,
        string? notes = null)
    {
        var history = new SubscriptionHistory
        {
            SubscriptionId = subscriptionId,
            ChangeType = "Canceled",
            PreviousStatus = SubscriptionStatus.Active,
            NewStatus = SubscriptionStatus.Canceled,
            NewPeriodEnd = periodEnd,
            TriggeredBy = triggeredBy,
            WebhookEventId = webhookEventId,
            Notes = notes,
            CreatedBy = triggeredBy,
            UpdatedBy = triggeredBy
        };

        return history;
    }

    /// <summary>
    /// Factory Method - Record subscription cancellation with status
    /// </summary>
    public static SubscriptionHistory RecordCancellation(
        Guid subscriptionId,
        SubscriptionStatus currentStatus,
        DateTime periodEnd,
        string triggeredBy,
        string? notes = null)
    {
        var history = new SubscriptionHistory
        {
            SubscriptionId = subscriptionId,
            ChangeType = "CancelScheduled",
            PreviousStatus = currentStatus,
            NewStatus = currentStatus, // Status doesn't change until period end
            NewPeriodEnd = periodEnd,
            TriggeredBy = triggeredBy,
            Notes = notes ?? "Subscription scheduled for cancellation at period end",
            CreatedBy = triggeredBy,
            UpdatedBy = triggeredBy
        };

        return history;
    }

    /// <summary>
    /// Factory Method - Record subscription reactivation
    /// </summary>
    public static SubscriptionHistory RecordReactivation(
        Guid subscriptionId,
        SubscriptionStatus currentStatus,
        DateTime periodEnd,
        string triggeredBy,
        string? notes = null)
    {
        var history = new SubscriptionHistory
        {
            SubscriptionId = subscriptionId,
            ChangeType = "Reactivated",
            PreviousStatus = currentStatus,
            NewStatus = currentStatus,
            NewPeriodEnd = periodEnd,
            TriggeredBy = triggeredBy,
            Notes = notes ?? "Subscription reactivated - cancellation undone",
            CreatedBy = triggeredBy,
            UpdatedBy = triggeredBy
        };

        return history;
    }

    /// <summary>
    /// Factory Method - Record subscription expiration
    /// </summary>
    public static SubscriptionHistory RecordExpiration(
        Guid subscriptionId,
        SubscriptionStatus previousStatus,
        string triggeredBy,
        string? webhookEventId = null,
        string? notes = null)
    {
        var history = new SubscriptionHistory
        {
            SubscriptionId = subscriptionId,
            ChangeType = "Expired",
            PreviousStatus = previousStatus,
            NewStatus = SubscriptionStatus.Expired,
            TriggeredBy = triggeredBy,
            WebhookEventId = webhookEventId,
            Notes = notes,
            CreatedBy = triggeredBy,
            UpdatedBy = triggeredBy
        };

        return history;
    }

    /// <summary>
    /// Factory Method - Record past due status
    /// </summary>
    public static SubscriptionHistory RecordPastDue(
        Guid subscriptionId,
        string triggeredBy,
        string? webhookEventId = null,
        string? notes = null)
    {
        var history = new SubscriptionHistory
        {
            SubscriptionId = subscriptionId,
            ChangeType = "PastDue",
            PreviousStatus = SubscriptionStatus.Active,
            NewStatus = SubscriptionStatus.PastDue,
            TriggeredBy = triggeredBy,
            WebhookEventId = webhookEventId,
            Notes = notes,
            CreatedBy = triggeredBy,
            UpdatedBy = triggeredBy
        };

        return history;
    }

    /// <summary>
    /// Factory Method - Record tier upgrade
    /// </summary>
    public static SubscriptionHistory RecordUpgrade(
        Guid subscriptionId,
        SubscriptionTier previousTier,
        SubscriptionTier newTier,
        string triggeredBy,
        string? webhookEventId = null,
        string? notes = null)
    {
        var history = new SubscriptionHistory
        {
            SubscriptionId = subscriptionId,
            ChangeType = "Upgraded",
            PreviousTier = previousTier,
            NewTier = newTier,
            TriggeredBy = triggeredBy,
            WebhookEventId = webhookEventId,
            Notes = notes,
            CreatedBy = triggeredBy,
            UpdatedBy = triggeredBy
        };

        return history;
    }

    /// <summary>
    /// Factory Method - Record tier downgrade
    /// </summary>
    public static SubscriptionHistory RecordDowngrade(
        Guid subscriptionId,
        SubscriptionTier previousTier,
        SubscriptionTier newTier,
        string triggeredBy,
        string? webhookEventId = null,
        string? notes = null)
    {
        var history = new SubscriptionHistory
        {
            SubscriptionId = subscriptionId,
            ChangeType = "Downgraded",
            PreviousTier = previousTier,
            NewTier = newTier,
            TriggeredBy = triggeredBy,
            WebhookEventId = webhookEventId,
            Notes = notes,
            CreatedBy = triggeredBy,
            UpdatedBy = triggeredBy
        };

        return history;
    }
}
