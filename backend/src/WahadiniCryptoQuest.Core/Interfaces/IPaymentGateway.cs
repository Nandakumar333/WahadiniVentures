namespace WahadiniCryptoQuest.Core.Interfaces;

/// <summary>
/// Abstraction for payment provider operations
/// Enables testability and potential future provider switching
/// Current implementation: Stripe
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Create Stripe Checkout Session for subscription purchase
    /// </summary>
    /// <param name="userId">User purchasing subscription</param>
    /// <param name="email">User email for prefilling checkout</param>
    /// <param name="tier">Subscription tier (MonthlyPremium or YearlyPremium)</param>
    /// <param name="currencyCode">ISO 4217 currency code (USD, INR, EUR, JPY, GBP)</param>
    /// <param name="discountPercentage">Optional discount percentage (e.g., 20 for 20% off)</param>
    /// <param name="discountCode">Optional discount code for tracking redemption</param>
    /// <param name="successUrl">Redirect URL after successful checkout</param>
    /// <param name="cancelUrl">Redirect URL if user cancels checkout</param>
    /// <returns>Checkout session ID and URL</returns>
    Task<(string SessionId, string CheckoutUrl)> CreateCheckoutSessionAsync(
        Guid userId,
        string email,
        Core.Enums.SubscriptionTier tier,
        string currencyCode,
        decimal? discountPercentage = null,
        string? discountCode = null,
        string? successUrl = null,
        string? cancelUrl = null);

    /// <summary>
    /// Create Stripe Customer Portal Session for subscription management
    /// </summary>
    /// <param name="stripeCustomerId">Stripe customer ID</param>
    /// <param name="returnUrl">URL to return to after portal session</param>
    /// <returns>Portal session URL</returns>
    Task<string> CreatePortalSessionAsync(
        string stripeCustomerId,
        string returnUrl);

    /// <summary>
    /// Retrieve subscription details from Stripe
    /// </summary>
    /// <param name="stripeSubscriptionId">Stripe subscription ID</param>
    /// <returns>Subscription details or null if not found</returns>
    Task<StripeSubscriptionDetails?> GetSubscriptionAsync(string stripeSubscriptionId);

    /// <summary>
    /// Cancel subscription at period end (user retains access until period ends)
    /// </summary>
    /// <param name="stripeSubscriptionId">Stripe subscription ID</param>
    /// <param name="cancelAtPeriodEnd">If true, cancel at period end. If false, cancel immediately</param>
    /// <returns>True if cancelled successfully</returns>
    Task<bool> CancelSubscriptionAsync(string stripeSubscriptionId, bool cancelAtPeriodEnd = true);

    /// <summary>
    /// Reactivate subscription that was scheduled for cancellation
    /// </summary>
    /// <param name="stripeSubscriptionId">Stripe subscription ID</param>
    /// <returns>True if reactivated successfully</returns>
    Task<bool> ReactivateSubscriptionAsync(string stripeSubscriptionId);
}

/// <summary>
/// Stripe subscription details returned from payment gateway
/// </summary>
public record StripeSubscriptionDetails
{
    public required string Id { get; init; }
    public required string CustomerId { get; init; }
    public required string PriceId { get; init; }
    public required string Status { get; init; } // active, past_due, canceled, incomplete, trialing
    public required DateTime CurrentPeriodStart { get; init; }
    public required DateTime CurrentPeriodEnd { get; init; }
    public required bool CancelAtPeriodEnd { get; init; }
    public DateTime? CanceledAt { get; init; }
}
