using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using Stripe.BillingPortal;
using WahadiniCryptoQuest.Core.Interfaces;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.DAL.Services;

/// <summary>
/// Stripe implementation of payment gateway abstraction
/// Handles checkout sessions, portal sessions, and subscription management
/// </summary>
public class StripePaymentGateway : IPaymentGateway
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentGateway> _logger;
    private readonly Stripe.Checkout.SessionService _sessionService;
    private readonly Stripe.BillingPortal.SessionService _portalSessionService;
    private readonly SubscriptionService _subscriptionService;

    public StripePaymentGateway(
        IConfiguration configuration,
        ILogger<StripePaymentGateway> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Initialize Stripe API key
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe SecretKey not configured");

        // Initialize Stripe services
        _sessionService = new Stripe.Checkout.SessionService();
        _portalSessionService = new Stripe.BillingPortal.SessionService();
        _subscriptionService = new SubscriptionService();
    }

    public async Task<(string SessionId, string CheckoutUrl)> CreateCheckoutSessionAsync(
        Guid userId,
        string email,
        SubscriptionTier tier,
        string currencyCode,
        decimal? discountPercentage = null,
        string? discountCode = null,
        string? successUrl = null,
        string? cancelUrl = null)
    {
        try
        {
            if (tier == SubscriptionTier.Free)
                throw new ArgumentException("Cannot create checkout for Free tier", nameof(tier));

            // Get price ID from configuration
            var priceId = GetPriceId(tier, currencyCode);
            if (string.IsNullOrEmpty(priceId))
                throw new InvalidOperationException($"No price configured for {tier} in {currencyCode}");

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                Mode = "subscription",
                CustomerEmail = email,
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                {
                    new Stripe.Checkout.SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1
                    }
                },
                SuccessUrl = successUrl ?? _configuration["Stripe:SuccessUrl"] ?? $"{_configuration["AppSettings:FrontendUrl"]}/subscription/success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = cancelUrl ?? _configuration["Stripe:CancelUrl"] ?? $"{_configuration["AppSettings:FrontendUrl"]}/subscription/cancel",
                ClientReferenceId = userId.ToString(), // Link session to our user
                Metadata = new Dictionary<string, string>
                {
                    { "user_id", userId.ToString() },
                    { "tier", tier.ToString() },
                    { "currency", currencyCode }
                },
                SubscriptionData = new Stripe.Checkout.SessionSubscriptionDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { "user_id", userId.ToString() },
                        { "tier", tier.ToString() }
                    }
                },
                PaymentMethodTypes = new List<string> { "card" },
                BillingAddressCollection = "required",
                AllowPromotionCodes = !discountPercentage.HasValue, // Disable if using our discount system
                ExpiresAt = DateTime.UtcNow.AddMinutes(30) // 30-minute session timeout
            };

            // Apply discount if provided
            if (discountPercentage.HasValue && discountPercentage.Value > 0)
            {
                // Create a one-time Stripe coupon for this checkout
                var couponService = new Stripe.CouponService();
                var couponOptions = new Stripe.CouponCreateOptions
                {
                    PercentOff = discountPercentage.Value,
                    Duration = "once", // Apply only to first payment
                    Name = $"Reward Discount {discountPercentage.Value}%",
                    Metadata = new Dictionary<string, string>
                    {
                        { "user_id", userId.ToString() },
                        { "created_at", DateTime.UtcNow.ToString("O") }
                    }
                };

                var coupon = await couponService.CreateAsync(couponOptions);

                _logger.LogInformation("Created Stripe coupon {CouponId} with {Percentage}% discount for user {UserId}",
                    coupon.Id, discountPercentage.Value, userId);

                options.Discounts = new List<Stripe.Checkout.SessionDiscountOptions>
                {
                    new Stripe.Checkout.SessionDiscountOptions
                    {
                        Coupon = coupon.Id
                    }
                };

                // Add discount info to metadata
                options.Metadata["discount_percentage"] = discountPercentage.Value.ToString();
                if (!string.IsNullOrEmpty(discountCode))
                {
                    options.Metadata["discount_code"] = discountCode;
                }
            }

            var session = await _sessionService.CreateAsync(options);

            _logger.LogInformation(
                "Created Stripe checkout session {SessionId} for user {UserId} - {Tier} {Currency}",
                session.Id, userId, tier, currencyCode);

            return (session.Id, session.Url);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe API error creating checkout session for user {UserId}", userId);
            throw new InvalidOperationException($"Failed to create checkout session: {ex.Message}", ex);
        }
    }

    public async Task<string> CreatePortalSessionAsync(
        string stripeCustomerId,
        string returnUrl)
    {
        try
        {
            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = stripeCustomerId,
                ReturnUrl = returnUrl
            };

            var session = await _portalSessionService.CreateAsync(options);

            _logger.LogInformation(
                "Created Stripe portal session for customer {CustomerId}",
                stripeCustomerId);

            return session.Url;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe API error creating portal session for customer {CustomerId}", stripeCustomerId);
            throw new InvalidOperationException("Failed to create portal session", ex);
        }
    }

    public async Task<StripeSubscriptionDetails?> GetSubscriptionAsync(string stripeSubscriptionId)
    {
        try
        {
            var subscription = await _subscriptionService.GetAsync(stripeSubscriptionId);

            if (subscription == null)
                return null;

            return new StripeSubscriptionDetails
            {
                Id = subscription.Id,
                CustomerId = subscription.CustomerId,
                PriceId = subscription.Items.Data[0].Price.Id,
                Status = subscription.Status,
                CurrentPeriodStart = subscription.CurrentPeriodStart,
                CurrentPeriodEnd = subscription.CurrentPeriodEnd,
                CancelAtPeriodEnd = subscription.CancelAtPeriodEnd,
                CanceledAt = subscription.CanceledAt
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe API error retrieving subscription {SubscriptionId}", stripeSubscriptionId);
            return null;
        }
    }

    public async Task<bool> CancelSubscriptionAsync(string stripeSubscriptionId, bool cancelAtPeriodEnd = true)
    {
        try
        {
            var options = new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = cancelAtPeriodEnd
            };

            await _subscriptionService.UpdateAsync(stripeSubscriptionId, options);

            _logger.LogInformation(
                "Cancelled Stripe subscription {SubscriptionId} at period end",
                stripeSubscriptionId);

            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe API error cancelling subscription {SubscriptionId}", stripeSubscriptionId);
            return false;
        }
    }

    public async Task<bool> ReactivateSubscriptionAsync(string stripeSubscriptionId)
    {
        try
        {
            var options = new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = false
            };

            await _subscriptionService.UpdateAsync(stripeSubscriptionId, options);

            _logger.LogInformation(
                "Reactivated Stripe subscription {SubscriptionId}",
                stripeSubscriptionId);

            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe API error reactivating subscription {SubscriptionId}", stripeSubscriptionId);
            return false;
        }
    }

    /// <summary>
    /// Get Stripe Price ID from configuration based on tier and currency
    /// </summary>
    private string? GetPriceId(SubscriptionTier tier, string currencyCode)
    {
        var configKey = tier switch
        {
            SubscriptionTier.MonthlyPremium => $"Stripe:Prices:{currencyCode}:Monthly",
            SubscriptionTier.YearlyPremium => $"Stripe:Prices:{currencyCode}:Yearly",
            _ => null
        };

        return configKey != null ? _configuration[configKey] : null;
    }
}
