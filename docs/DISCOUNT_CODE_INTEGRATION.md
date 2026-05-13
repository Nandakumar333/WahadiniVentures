# Discount Code Integration for Subscriptions

This document describes how discount codes are applied to subscription checkouts.

## Overview

Users can apply reward-earned discount codes when purchasing premium subscriptions. The discount is applied as a percentage off the subscription price using Stripe coupons.

## Backend Implementation

### API Endpoints

#### Validate Discount Code
```http
POST /api/subscriptions/validate-discount
Authorization: Bearer {token}
Content-Type: application/json

{
  "discountCode": "SAVE20",
  "tier": "MonthlyPremium"
}
```

**Response:**
```json
{
  "isValid": true,
  "discountAmount": 20,
  "stripeCouponId": null,
  "errorMessage": null
}
```

**Validation Rules:**
- Discount code must exist and be active
- Discount code must not be expired
- User must not have already redeemed this code
- Code must not have reached its usage limit

#### Create Checkout with Discount
```http
POST /api/subscriptions/checkout
Authorization: Bearer {token}
Content-Type: application/json

{
  "tier": "MonthlyPremium",
  "currencyCode": "USD",
  "discountCode": "SAVE20"
}
```

### Flow

1. **Frontend validates discount code** (on blur or button click)
   - Calls `/api/subscriptions/validate-discount`
   - Shows discount percentage to user
   - Displays discounted price preview

2. **User clicks "Get Started"**
   - Frontend calls `/api/subscriptions/checkout` with discount code
   - Backend validates discount again (security)
   - If invalid, returns error 400

3. **Backend creates Stripe checkout session**
   - Validates discount code using `ValidateDiscountForCheckoutHandler`
   - Extracts discount percentage (e.g., 20 for 20% off)
   - Creates one-time Stripe coupon with that percentage
   - Applies coupon to checkout session
   - Returns checkout URL to frontend

4. **User completes checkout**
   - Redirected to Stripe Checkout
   - Sees discounted price
   - Completes payment

5. **Webhook processes checkout.session.completed**
   - Creates subscription record
   - Records discount usage (future enhancement)

## Frontend Components

### DiscountCodeInput Component

Location: `frontend/src/components/subscription/DiscountCodeInput.tsx`

**Props:**
- `tier`: Subscription tier (`MonthlyPremium` or `YearlyPremium`)
- `onDiscountValidated`: Callback with (code, percentage)
- `onDiscountCleared`: Callback when discount removed

**Features:**
- Real-time validation on blur
- Loading indicator during API call
- Success/error visual feedback
- Displays discount percentage when valid
- "Remove" button to clear discount

**Usage:**
```tsx
<DiscountCodeInput
  tier="MonthlyPremium"
  onDiscountValidated={(code, percentage) => {
    setDiscountCode(code);
    setDiscountPercentage(percentage);
  }}
  onDiscountCleared={() => {
    setDiscountCode('');
    setDiscountPercentage(0);
  }}
/>
```

### Updated PricingCard Component

**New Props:**
- `discountPercentage?: number` - Percentage to apply (0-100)

**Visual Changes:**
- Shows original price with strikethrough when discount applied
- Shows discounted price in green
- Displays "X% discount applied!" message

## Stripe Integration

### Coupon Creation

When a discount code is validated, the backend creates a one-time Stripe coupon:

```csharp
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
```

**Key Properties:**
- `PercentOff`: Percentage discount (e.g., 20 for 20% off)
- `Duration`: "once" - applies only to first payment
- `Metadata`: Tracks which user created the coupon

### Checkout Session

The coupon is applied to the checkout session:

```csharp
options.Discounts = new List<Stripe.Checkout.SessionDiscountOptions>
{
    new Stripe.Checkout.SessionDiscountOptions
    {
        Coupon = coupon.Id
    }
};
```

## Testing

### Manual Testing

1. **Create a discount code** in the database:
```sql
INSERT INTO "DiscountCodes" ("Code", "DiscountPercentage", "RequiredPoints", "MaxRedemptions", "IsActive", "CreatedAt", "UpdatedAt")
VALUES ('SAVE20', 20, 100, 10, true, NOW(), NOW());
```

2. **Test validation endpoint**:
```bash
curl -X POST http://localhost:5000/api/subscriptions/validate-discount \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"discountCode": "SAVE20", "tier": "MonthlyPremium"}'
```

3. **Test checkout with discount**:
- Navigate to /pricing
- Enter discount code "SAVE20"
- Verify discount percentage shows
- Verify prices update with strikethrough
- Click "Get Started"
- Verify Stripe Checkout shows discounted price

### Test Cases

- ✅ Valid discount code shows percentage
- ✅ Invalid code shows error message
- ✅ Expired code shows "expired" error
- ✅ Already redeemed code shows "already used" error
- ✅ Code at max redemptions shows "usage limit" error
- ✅ Discounted price calculated correctly
- ✅ Checkout session created with coupon
- ✅ Stripe shows discount at checkout

## Future Enhancements

### 24-Hour Reservation System

Currently, discount codes are validated at checkout but not reserved. Future implementation should:

1. **Add reservation fields** to `UserDiscountRedemption`:
```csharp
public DiscountRedemptionStatus Status { get; set; } // Reserved, Redeemed, Expired
public DateTime? ReservedAt { get; set; }
public DateTime? ReservationExpiresAt { get; set; }
```

2. **Create reservation on validation**:
- When frontend validates, create Reserved record
- Expires after 24 hours
- Prevents double-booking of limited codes

3. **Background cleanup job**:
- Runs hourly
- Deletes/expires reservations older than 24 hours
- Frees up codes for other users

4. **Mark as redeemed on webhook**:
- When `checkout.session.completed` webhook fires
- Find reservation for user+code
- Update status to Redeemed
- Set `UsedInSubscription = true`

### Discount Tracking

Track discount usage in subscription webhooks:

```csharp
// In HandleCheckoutSessionCompletedAsync
if (session.Metadata.TryGetValue("discount_percentage", out var discount))
{
    // Record discount usage
    // Update SubscriptionHistory with discount info
}
```

## Security Considerations

- ✅ Discount validation happens server-side (not client-side only)
- ✅ Authorization required for validation endpoint
- ✅ Discount re-validated at checkout (can't bypass validation)
- ✅ Stripe coupons created on-the-fly (no pre-shared Stripe IDs)
- ⚠️ No reservation system yet (codes not locked during checkout)

## Known Limitations

1. **No reservation system**: Multiple users can validate the same limited-use code simultaneously
2. **No usage tracking**: Discount usage not recorded in SubscriptionHistory
3. **No expiration on Stripe coupons**: Coupons persist in Stripe (doesn't affect billing)
4. **Single validation per checkout**: Discount validated once at checkout start, not refreshed if user waits

## Related Files

### Backend
- `ValidateDiscountForCheckoutCommand.cs` - Validation command
- `ValidateDiscountForCheckoutHandler.cs` - Validation logic
- `CreateCheckoutSessionHandler.cs` - Integrates discount validation
- `SubscriptionsController.cs` - API endpoint
- `StripePaymentGateway.cs` - Stripe coupon creation
- `IPaymentGateway.cs` - Interface update

### Frontend
- `DiscountCodeInput.tsx` - Input component
- `PricingCard.tsx` - Shows discounted prices
- `PricingPage.tsx` - Integrates discount flow
- `useSubscription.ts` - API hooks
