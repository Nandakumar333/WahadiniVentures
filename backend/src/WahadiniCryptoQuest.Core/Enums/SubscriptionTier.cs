namespace WahadiniCryptoQuest.Core.Enums;

/// <summary>
/// Subscription tier options for the crypto learning platform
/// Aligned with Stripe subscription products
/// </summary>
public enum SubscriptionTier
{
    /// <summary>
    /// Free tier with limited access to courses and content
    /// No payment required, default tier for all users
    /// </summary>
    Free = 0,

    /// <summary>
    /// Monthly premium subscription with full access to all content
    /// Billed monthly at currency-specific pricing (e.g., $9.99/month USD)
    /// </summary>
    MonthlyPremium = 1,

    /// <summary>
    /// Yearly premium subscription with full access to all content
    /// Billed annually with ~17% savings compared to monthly (e.g., $99/year USD)
    /// Equivalent to 2 months free
    /// </summary>
    YearlyPremium = 2
}