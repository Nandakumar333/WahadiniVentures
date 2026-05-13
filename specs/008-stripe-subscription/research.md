# Research: Stripe Subscription Management

**Feature**: 008-stripe-subscription  
**Date**: 2025-12-09  
**Purpose**: Resolve technical unknowns and establish implementation patterns

## Clarifications Resolved

### 1. Payment Failure Grace Period (FR-021)

**Decision**: 3-day grace period before automatic downgrade to Free tier

**Rationale**:
- Industry standard for subscription services (Netflix, Spotify use 3-7 days)
- Stripe's Smart Retries typically complete within 3 days (default retry schedule)
- Balances user experience (reduces involuntary churn) with revenue protection
- Provides sufficient time for users to update payment methods
- Aligns with Stripe's recommended practices for dunning management

**Implementation**: 
- Configure Stripe Smart Retries with 3 retry attempts over 3 days
- Listen for `invoice.payment_failed` webhook after final retry
- Trigger downgrade process via `customer.subscription.deleted` webhook
- Send email notification on first failure, reminder on day 2, final notice on day 3

**Alternatives Considered**:
- 7-day grace period: More user-friendly but extends revenue loss window
- 1-day grace period: Aggressive, may increase support tickets and user frustration
- Immediate downgrade: Poor user experience, doesn't account for temporary card issues

---

### 2. Mid-Cycle Plan Changes (FR-022)

**Decision**: Allow immediate upgrade/downgrade with Stripe-managed proration

**Rationale**:
- Maximizes flexibility for users to switch plans based on changing needs
- Stripe automatically calculates prorated refunds/charges, reducing implementation complexity
- Industry standard practice (most SaaS platforms allow immediate changes)
- Encourages upsells by removing barriers to upgrading
- Handles edge cases automatically (timing, partial months, currency conversion)

**Implementation**:
- Upgrades: Create new checkout session with prorated immediate charge
- Downgrades: Use `subscription_schedule` to apply changes at period end OR allow immediate downgrade with prorated credit
- Stripe Configuration: Enable proration behavior in Stripe Dashboard settings
- UI: Clearly communicate proration amounts before user confirms change
- Webhook handling: Process `customer.subscription.updated` events for plan changes

**Alternatives Considered**:
- Changes at renewal only: Simpler but poor UX, users must wait for current period to end
- Upgrades immediate, downgrades at period end: Hybrid approach but inconsistent user experience
- Manual proration calculation: Complex, error-prone, Stripe handles this better

---

## Technology Decisions

### Multi-Currency Implementation

**Decision**: Use Stripe Prices API with currency-specific Price IDs

**Rationale**:
- Stripe natively supports 135+ currencies with automatic conversion
- Each currency needs dedicated Price ID (e.g., `price_usd_monthly`, `price_inr_monthly`)
- Allows region-specific pricing strategy (not just currency conversion)
- Stripe handles currency formatting, decimal places, and symbol placement
- Simplifies tax calculation and compliance per region

**Implementation Pattern**:
```csharp
// Backend: Currency Pricing Entity
public class CurrencyPricing
{
    public int Id { get; set; }
    public string CurrencyCode { get; set; } // ISO 4217 (USD, INR, EUR, etc.)
    public string StripePriceIdMonthly { get; set; } // Stripe Price ID
    public string StripePriceIdYearly { get; set; }
    public decimal MonthlyPrice { get; set; } // For display only
    public decimal YearlyPrice { get; set; }
    public string CurrencySymbol { get; set; } // $, ₹, €, ¥
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// Currency Detection Strategy:
// 1. Check user's saved preference (if authenticated)
// 2. Browser Accept-Language header
// 3. IP-based geolocation (MaxMind GeoIP2 or similar)
// 4. Default to USD if no match found
```

**Alternatives Considered**:
- Single Price ID with dynamic currency conversion: Less control, doesn't support region-specific pricing
- Manual currency conversion: Complex, requires exchange rate management and updates
- Third-party currency service: Adds dependency, Stripe's built-in solution is sufficient

---

### Discount Code Integration

**Decision**: Use Stripe Promotion Codes with metadata linking to reward system

**Rationale**:
- Stripe Promotion Codes provide native discount functionality
- Supports percentage-off and fixed-amount discounts
- Handles redemption tracking and usage limits automatically
- Metadata field stores WahadiniCryptoQuest reward code ID for bidirectional linking
- Prevents duplicate redemptions via Stripe's built-in validation

**Implementation Pattern**:
```csharp
// Create Stripe Promotion Code when user earns reward
var couponOptions = new CouponCreateOptions
{
    PercentOff = 20, // From reward system
    Duration = "once", // Single-use for subscription
    Currency = "usd", // Or specific currency
    Metadata = new Dictionary<string, string>
    {
        { "reward_code_id", rewardCode.Id.ToString() },
        { "user_id", user.Id.ToString() }
    }
};

var promotionCodeOptions = new PromotionCodeCreateOptions
{
    Coupon = coupon.Id,
    Code = rewardCode.Code, // User-facing code
    MaxRedemptions = 1,
    Metadata = couponOptions.Metadata
};

// Apply at checkout
var sessionOptions = new SessionCreateOptions
{
    Discounts = new List<SessionDiscountOptions>
    {
        new SessionDiscountOptions { PromotionCode = promotionCode.Id }
    }
};
```

**Alternatives Considered**:
- Manual discount calculation: Complex, error-prone, doesn't leverage Stripe features
- Separate discount code system: Duplicate logic, synchronization issues
- Fixed discount amounts only: Less flexible, percentage discounts more valuable for varying currency prices

---

### Webhook Security and Idempotency

**Decision**: Stripe webhook signature verification with idempotency key tracking

**Rationale**:
- Stripe-Signature header contains HMAC-SHA256 signature for authenticity verification
- Protects against replay attacks and unauthorized webhook submissions
- Idempotency keys prevent duplicate event processing if webhook retries
- Stores webhook events in database for audit trail and debugging
- Supports event replay for failed processing

**Implementation Pattern**:
```csharp
// Webhook Controller
[HttpPost("stripe")]
public async Task<IActionResult> HandleStripeWebhook()
{
    var json = await new StreamReader(Request.Body).ReadToEndAsync();
    var stripeSignature = Request.Headers["Stripe-Signature"];
    
    try
    {
        // Verify signature
        var stripeEvent = EventUtility.ConstructEvent(
            json, 
            stripeSignature, 
            _webhookSecret
        );
        
        // Check idempotency
        var existingEvent = await _webhookRepository
            .GetByStripeEventIdAsync(stripeEvent.Id);
            
        if (existingEvent != null)
        {
            _logger.LogWarning($"Duplicate webhook event: {stripeEvent.Id}");
            return Ok(); // Already processed
        }
        
        // Store event
        var webhookEvent = new WebhookEvent
        {
            StripeEventId = stripeEvent.Id,
            EventType = stripeEvent.Type,
            Payload = json,
            ProcessedAt = null,
            CreatedAt = DateTime.UtcNow
        };
        
        await _webhookRepository.CreateAsync(webhookEvent);
        
        // Process event
        await ProcessWebhookEventAsync(stripeEvent, webhookEvent);
        
        webhookEvent.ProcessedAt = DateTime.UtcNow;
        await _webhookRepository.UpdateAsync(webhookEvent);
        
        return Ok();
    }
    catch (StripeException ex)
    {
        _logger.LogError($"Stripe webhook error: {ex.Message}");
        return BadRequest();
    }
}
```

**Alternatives Considered**:
- Trust all webhook requests: Severe security risk, unacceptable
- Custom signature scheme: Reinventing the wheel, Stripe's solution is proven
- Synchronous webhook processing: Risk of timeout, async with queue is better for reliability

---

## Best Practices Research

### Clean Architecture for Payment Integration

**Pattern**: Isolate Stripe-specific code in Infrastructure layer

**Structure**:
```
Domain Layer (Core):
- Entities: Subscription, CurrencyPricing, WebhookEvent
- Interfaces: ISubscriptionService, IPaymentGateway
- Domain Events: SubscriptionCreatedEvent, SubscriptionCancelledEvent

Application Layer (Service):
- Use Cases: CreateCheckoutSessionUseCase, ProcessWebhookUseCase
- DTOs: CheckoutSessionDto, SubscriptionStatusDto
- Command/Query Handlers: CreateCheckoutCommand, GetSubscriptionQuery

Infrastructure Layer:
- StripePaymentGateway implementing IPaymentGateway
- StripeSubscriptionService implementing ISubscriptionService
- Repositories: SubscriptionRepository, CurrencyPricingRepository

Presentation Layer (API):
- SubscriptionController (user-facing endpoints)
- WebhookController (Stripe webhook receiver)
- DTOs/Requests/Responses for API contracts
```

**Benefits**:
- Testable: Mock IPaymentGateway for unit tests
- Flexible: Can swap Stripe for different provider without changing business logic
- Maintainable: Clear separation of concerns
- Domain-focused: Business rules in domain entities, not in Stripe code

---

### Entity Framework Configuration

**Pattern**: Separate entity configuration classes with proper indexing

**Implementation**:
```csharp
public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.StripeCustomerId)
            .HasMaxLength(255)
            .IsRequired(false);
            
        builder.Property(s => s.StripeSubscriptionId)
            .HasMaxLength(255)
            .IsRequired(false);
            
        builder.Property(s => s.CurrencyCode)
            .HasMaxLength(3) // ISO 4217
            .IsRequired();
            
        builder.Property(s => s.Status)
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>(); // Enum to string
            
        // Indexes for performance
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.StripeCustomerId);
        builder.HasIndex(s => s.StripeSubscriptionId);
        builder.HasIndex(s => new { s.UserId, s.Status });
        
        // Relationships
        builder.HasOne(s => s.User)
            .WithOne()
            .HasForeignKey<Subscription>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Soft delete
        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
```

---

### React Query for Subscription State

**Pattern**: Optimistic updates with server-side validation

**Implementation**:
```typescript
// hooks/subscription/useSubscription.ts
export const useSubscription = () => {
  return useQuery({
    queryKey: ['subscription', 'status'],
    queryFn: subscriptionService.getStatus,
    staleTime: 5 * 60 * 1000, // 5 minutes
    cacheTime: 10 * 60 * 1000, // 10 minutes
    refetchOnWindowFocus: true
  });
};

export const useCreateCheckout = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: subscriptionService.createCheckout,
    onSuccess: () => {
      queryClient.invalidateQueries(['subscription']);
    },
    onError: (error) => {
      toast.error('Failed to create checkout session');
    }
  });
};
```

**Benefits**:
- Automatic caching reduces API calls
- Optimistic updates for better UX
- Automatic refetching on window focus
- Built-in loading and error states

---

## Security Considerations

### PCI Compliance

**Approach**: Never handle credit card data

**Implementation**:
- All payment forms hosted by Stripe Checkout (PCI Level 1 compliant)
- Only store Stripe Customer ID and Subscription ID (non-sensitive tokens)
- Never log or transmit credit card numbers
- Use Stripe.js for client-side tokenization if custom forms needed

### Webhook Endpoint Protection

**Layers**:
1. Signature verification (HMAC-SHA256)
2. IP whitelist for Stripe webhook IPs (optional)
3. Rate limiting to prevent abuse
4. Idempotency checks to prevent duplicate processing
5. Audit logging for all webhook events

### Currency Tampering Prevention

**Protection**:
- Price validation: Always verify price server-side, never trust client
- Currency matching: Ensure checkout currency matches user's selected currency
- Immutable checkout sessions: Once created, cannot be modified by client
- Metadata validation: Store user ID and currency in session metadata, verify on webhook

---

## Performance Optimization

### Database Indexes

```sql
-- Critical indexes for subscription queries
CREATE INDEX idx_subscriptions_user_id ON Subscriptions(UserId);
CREATE INDEX idx_subscriptions_stripe_customer_id ON Subscriptions(StripeCustomerId);
CREATE INDEX idx_subscriptions_stripe_subscription_id ON Subscriptions(StripeSubscriptionId);
CREATE INDEX idx_subscriptions_status_period_end ON Subscriptions(Status, PeriodEnd);

-- Currency pricing lookups
CREATE INDEX idx_currency_pricing_code_active ON CurrencyPricing(CurrencyCode, IsActive);

-- Webhook event tracking
CREATE INDEX idx_webhook_events_stripe_id ON WebhookEvents(StripeEventId);
CREATE INDEX idx_webhook_events_type_created ON WebhookEvents(EventType, CreatedAt);
```

### Caching Strategy

**Implementation**:
- Cache currency pricing configurations (Redis/Memory cache, 1-hour TTL)
- Cache subscription status per user (5-minute TTL)
- Invalidate subscription cache on webhook events
- Cache Stripe product/price metadata (daily refresh)

---

## Testing Strategy

### Unit Tests

**Focus Areas**:
- Subscription entity business logic (state transitions)
- Currency formatting and validation
- Discount code validation
- Webhook signature verification (mocked)
- Domain event handlers

### Integration Tests

**Critical Paths**:
- Checkout session creation with various currencies
- Webhook event processing (use Stripe test mode events)
- Subscription status synchronization
- Currency detection logic
- Access control based on subscription tier

### End-to-End Tests

**User Journeys**:
1. Complete checkout flow: Select plan → Apply discount → Pay → Verify access
2. Subscription management: Cancel → Verify access until period end → Verify downgrade
3. Payment failure: Simulate failed payment → Verify grace period → Verify downgrade
4. Currency switching: Change currency → Verify pricing updates → Complete checkout

---

## Monitoring and Observability

### Key Metrics

- Checkout abandonment rate (goal: <30%)
- Webhook processing latency (goal: <2 seconds)
- Subscription churn rate
- Payment failure rate by currency
- Discount code redemption rate
- Average time to checkout completion

### Logging Requirements

```csharp
// Structured logging with Serilog
_logger.LogInformation(
    "Checkout session created: {UserId}, {Currency}, {PlanType}, {DiscountApplied}",
    userId, 
    currencyCode, 
    planType, 
    discountCode != null
);

_logger.LogWarning(
    "Payment failed: {UserId}, {SubscriptionId}, {Reason}",
    userId,
    subscriptionId,
    failureReason
);
```

### Alerts

- Webhook processing failures (> 5% error rate)
- Payment gateway downtime
- Abnormal subscription cancellation spike
- Currency pricing configuration errors

---

## Migration Considerations

### Existing Users

**Strategy**:
- All existing users default to Free tier
- Grandfathered pricing for early adopters (optional)
- Migration script to populate StripeCustomerId for existing users (run lazily on first checkout)

### Database Migration

```sql
-- Migration: Add Stripe fields to Users table
ALTER TABLE Users ADD COLUMN StripeCustomerId VARCHAR(255) NULL;
ALTER TABLE Users ADD COLUMN StripeSubscriptionId VARCHAR(255) NULL;
ALTER TABLE Users ADD COLUMN SubscriptionStatus VARCHAR(50) DEFAULT 'free';
ALTER TABLE Users ADD COLUMN SubscriptionPeriodEnd TIMESTAMP NULL;
ALTER TABLE Users ADD COLUMN CancelAtPeriodEnd BOOLEAN DEFAULT FALSE;
ALTER TABLE Users ADD COLUMN CurrencyCode VARCHAR(3) DEFAULT 'USD';

-- Create index for performance
CREATE INDEX idx_users_stripe_customer_id ON Users(StripeCustomerId);
CREATE INDEX idx_users_subscription_status ON Users(SubscriptionStatus);
```

---

## Rollback Plan

**Failure Scenarios**:
1. Stripe integration issues: Disable premium features, all users remain on Free tier
2. Webhook processing failures: Queue events for manual review and replay
3. Currency conversion errors: Fall back to USD pricing
4. Database migration failures: Rollback migration, restore from backup

**Feature Flags**:
- `ENABLE_SUBSCRIPTIONS`: Master switch for subscription feature
- `ENABLE_MULTI_CURRENCY`: Enable/disable currency selection
- `ENABLE_DISCOUNT_CODES`: Enable/disable discount code functionality
- `ENABLE_STRIPE_WEBHOOKS`: Toggle webhook processing

---

## Conclusion

All technical unknowns have been resolved with industry-standard patterns and Stripe best practices. Implementation is ready to proceed with:
- 3-day grace period for payment failures
- Immediate plan changes with Stripe-managed proration
- Multi-currency support using Stripe Prices API
- Stripe Promotion Codes for discount integration
- Webhook security with signature verification and idempotency
- Clean Architecture separation of concerns
- Comprehensive testing and monitoring strategy
