# API Contract: Subscription Management

**Feature**: 008-stripe-subscription  
**Date**: 2025-12-09  
**Base URL**: `/api/subscriptions`  
**Authentication**: Required (JWT Bearer Token)

## Overview

RESTful API endpoints for managing user subscriptions with multi-currency support, discount code application, and Stripe payment integration.

---

## Endpoints

### 1. Get Available Plans

**Purpose**: Retrieve subscription plans with currency-specific pricing

**Endpoint**: `GET /api/subscriptions/plans`

**Authentication**: Optional (returns same plans for authenticated/unauthenticated users)

**Query Parameters**:
- `currency` (string, optional): ISO 4217 currency code (USD, INR, EUR, etc.)
  - If omitted, detects from user's locale or defaults to USD

**Request Headers**:
```http
Accept: application/json
Accept-Language: en-US (optional, for currency detection)
```

**Response**: `200 OK`

```json
{
  "plans": [
    {
      "tier": "Free",
      "name": "Free Plan",
      "currency": "USD",
      "currencySymbol": "$",
      "monthlyPrice": 0,
      "yearlyPrice": 0,
      "features": [
        "Access to free courses",
        "Limited tasks per day",
        "Community support"
      ],
      "stripePriceIdMonthly": null,
      "stripePriceIdYearly": null
    },
    {
      "tier": "MonthlyPremium",
      "name": "Monthly Premium",
      "currency": "USD",
      "currencySymbol": "$",
      "monthlyPrice": 9.99,
      "yearlyPrice": null,
      "features": [
        "All courses access",
        "Unlimited tasks",
        "Priority support",
        "No ads"
      ],
      "stripePriceIdMonthly": "price_1234Monthly",
      "stripePriceIdYearly": null
    },
    {
      "tier": "YearlyPremium",
      "name": "Yearly Premium",
      "currency": "USD",
      "currencySymbol": "$",
      "monthlyPrice": null,
      "yearlyPrice": 99.00,
      "yearlySavings": 19.88,
      "savingsPercentage": 17,
      "features": [
        "All Monthly Premium features",
        "2 months free (17% savings)",
        "Exclusive content",
        "1-on-1 consultation"
      ],
      "stripePriceIdMonthly": null,
      "stripePriceIdYearly": "price_1234Yearly"
    }
  ],
  "availableCurrencies": [
    {
      "code": "USD",
      "name": "US Dollar",
      "symbol": "$"
    },
    {
      "code": "INR",
      "name": "Indian Rupee",
      "symbol": "₹"
    },
    {
      "code": "EUR",
      "name": "Euro",
      "symbol": "€"
    }
  ],
  "detectedCurrency": "USD"
}
```

**Error Responses**:
- `400 Bad Request`: Invalid currency code
```json
{
  "error": "InvalidCurrency",
  "message": "Currency code 'XYZ' is not supported",
  "supportedCurrencies": ["USD", "INR", "EUR", "JPY", "GBP"]
}
```

---

### 2. Get Current Subscription Status

**Purpose**: Retrieve authenticated user's subscription details

**Endpoint**: `GET /api/subscriptions/status`

**Authentication**: Required

**Request Headers**:
```http
Authorization: Bearer {jwt_token}
Accept: application/json
```

**Response**: `200 OK`

```json
{
  "subscription": {
    "id": 123,
    "userId": 456,
    "tier": "MonthlyPremium",
    "status": "Active",
    "currency": "USD",
    "currencySymbol": "$",
    "currentPeriodStart": "2025-12-01T00:00:00Z",
    "currentPeriodEnd": "2026-01-01T00:00:00Z",
    "cancelAtPeriodEnd": false,
    "cancelledAt": null,
    "hasActiveAccess": true,
    "isGracePeriod": false,
    "stripeCustomerId": "cus_abc123",
    "stripeSubscriptionId": "sub_xyz789"
  },
  "billing": {
    "nextBillingDate": "2026-01-01T00:00:00Z",
    "nextBillingAmount": 9.99,
    "lastPaymentDate": "2025-12-01T00:00:00Z",
    "lastPaymentAmount": 9.99
  }
}
```

**Free Tier Response**: `200 OK`
```json
{
  "subscription": {
    "tier": "Free",
    "status": "Active",
    "hasActiveAccess": true
  },
  "billing": null
}
```

**Error Responses**:
- `401 Unauthorized`: Missing or invalid JWT token
- `404 Not Found`: User has no subscription record

---

### 3. Create Checkout Session

**Purpose**: Initiate Stripe checkout session for subscription purchase

**Endpoint**: `POST /api/subscriptions/checkout`

**Authentication**: Required

**Request Headers**:
```http
Authorization: Bearer {jwt_token}
Content-Type: application/json
Accept: application/json
```

**Request Body**:
```json
{
  "priceId": "price_1234Monthly",
  "currency": "USD",
  "discountCode": "REWARD20OFF",
  "successUrl": "https://wahadini.com/subscription/success",
  "cancelUrl": "https://wahadini.com/subscription/cancel"
}
```

**Field Descriptions**:
- `priceId` (string, required): Stripe Price ID from plans endpoint
- `currency` (string, required): ISO 4217 currency code
- `discountCode` (string, optional): Reward-based discount code
- `successUrl` (string, optional): Redirect URL after successful payment
- `cancelUrl` (string, optional): Redirect URL if user cancels

**Response**: `200 OK`

```json
{
  "sessionId": "cs_test_a1b2c3d4",
  "sessionUrl": "https://checkout.stripe.com/c/pay/cs_test_a1b2c3d4",
  "expiresAt": "2025-12-09T12:30:00Z"
}
```

**Error Responses**:

- `400 Bad Request`: Invalid request data
```json
{
  "error": "InvalidRequest",
  "message": "Price ID is required",
  "field": "priceId"
}
```

- `400 Bad Request`: Invalid discount code
```json
{
  "error": "InvalidDiscountCode",
  "message": "Discount code 'INVALID123' is not valid or has already been used"
}
```

- `409 Conflict`: User already has active subscription
```json
{
  "error": "ExistingSubscription",
  "message": "You already have an active subscription. Please cancel or upgrade instead.",
  "currentTier": "MonthlyPremium"
}
```

- `503 Service Unavailable`: Stripe service error
```json
{
  "error": "PaymentGatewayUnavailable",
  "message": "Payment service is temporarily unavailable. Please try again later."
}
```

---

### 4. Create Billing Portal Session

**Purpose**: Generate link to Stripe Customer Portal for subscription management

**Endpoint**: `POST /api/subscriptions/portal`

**Authentication**: Required

**Request Headers**:
```http
Authorization: Bearer {jwt_token}
Content-Type: application/json
Accept: application/json
```

**Request Body**:
```json
{
  "returnUrl": "https://wahadini.com/profile"
}
```

**Response**: `200 OK`

```json
{
  "portalUrl": "https://billing.stripe.com/session/abc123",
  "expiresAt": "2025-12-09T12:00:00Z"
}
```

**Error Responses**:

- `400 Bad Request`: User has no Stripe customer
```json
{
  "error": "NoStripeCustomer",
  "message": "You must have an active or past subscription to access billing portal"
}
```

- `401 Unauthorized`: Missing or invalid JWT token

---

### 5. Cancel Subscription

**Purpose**: Schedule subscription for cancellation at period end

**Endpoint**: `POST /api/subscriptions/cancel`

**Authentication**: Required

**Request Headers**:
```http
Authorization: Bearer {jwt_token}
Content-Type: application/json
Accept: application/json
```

**Request Body**:
```json
{
  "reason": "Too expensive",
  "feedback": "Optional feedback text"
}
```

**Response**: `200 OK`

```json
{
  "success": true,
  "message": "Subscription will be cancelled at period end",
  "subscription": {
    "tier": "MonthlyPremium",
    "status": "Active",
    "cancelAtPeriodEnd": true,
    "accessUntil": "2026-01-01T00:00:00Z"
  }
}
```

**Error Responses**:

- `400 Bad Request`: No active subscription
```json
{
  "error": "NoActiveSubscription",
  "message": "You don't have an active subscription to cancel"
}
```

- `409 Conflict`: Already scheduled for cancellation
```json
{
  "error": "AlreadyCancelled",
  "message": "Subscription is already scheduled for cancellation"
}
```

---

### 6. Reactivate Subscription

**Purpose**: Remove cancellation schedule and continue subscription

**Endpoint**: `POST /api/subscriptions/reactivate`

**Authentication**: Required

**Request Headers**:
```http
Authorization: Bearer {jwt_token}
Accept: application/json
```

**Response**: `200 OK`

```json
{
  "success": true,
  "message": "Subscription reactivated successfully",
  "subscription": {
    "tier": "MonthlyPremium",
    "status": "Active",
    "cancelAtPeriodEnd": false,
    "nextBillingDate": "2026-01-01T00:00:00Z"
  }
}
```

**Error Responses**:

- `400 Bad Request`: Subscription not scheduled for cancellation
```json
{
  "error": "NotScheduledForCancellation",
  "message": "Subscription is not scheduled for cancellation"
}
```

---

### 7. Validate Discount Code

**Purpose**: Validate discount code before applying at checkout

**Endpoint**: `POST /api/subscriptions/validate-discount`

**Authentication**: Required

**Request Headers**:
```http
Authorization: Bearer {jwt_token}
Content-Type: application/json
Accept: application/json
```

**Request Body**:
```json
{
  "code": "REWARD20OFF",
  "priceId": "price_1234Monthly"
}
```

**Response**: `200 OK`

```json
{
  "valid": true,
  "discount": {
    "code": "REWARD20OFF",
    "type": "percentage",
    "amount": 20,
    "originalPrice": 9.99,
    "discountedPrice": 7.99,
    "savings": 2.00,
    "currency": "USD",
    "currencySymbol": "$"
  }
}
```

**Invalid Code Response**: `200 OK`
```json
{
  "valid": false,
  "reason": "Code has already been used"
}
```

**Error Responses**:
- `400 Bad Request`: Missing required fields
- `401 Unauthorized`: Invalid authentication

---

## Admin Endpoints

### 8. Get All Currency Pricing (Admin)

**Endpoint**: `GET /api/admin/subscriptions/currencies`

**Authentication**: Required (Admin role)

**Authorization**: `[Authorize(Roles = "Admin")]`

**Response**: `200 OK`

```json
{
  "currencies": [
    {
      "id": 1,
      "currencyCode": "USD",
      "currencySymbol": "$",
      "currencyName": "US Dollar",
      "monthlyPrice": 9.99,
      "yearlyPrice": 99.00,
      "stripePriceIdMonthly": "price_1234Monthly",
      "stripePriceIdYearly": "price_1234Yearly",
      "isActive": true,
      "isDefault": true,
      "createdAt": "2025-01-01T00:00:00Z",
      "updatedAt": null
    },
    {
      "id": 2,
      "currencyCode": "INR",
      "currencySymbol": "₹",
      "currencyName": "Indian Rupee",
      "monthlyPrice": 750.00,
      "yearlyPrice": 7500.00,
      "stripePriceIdMonthly": "price_5678Monthly",
      "stripePriceIdYearly": "price_5678Yearly",
      "isActive": true,
      "isDefault": false,
      "createdAt": "2025-06-01T00:00:00Z",
      "updatedAt": "2025-12-01T00:00:00Z"
    }
  ]
}
```

**Error Responses**:
- `401 Unauthorized`: Missing or invalid JWT token
- `403 Forbidden`: User is not an admin

---

### 9. Create/Update Currency Pricing (Admin)

**Endpoint**: `POST /api/admin/subscriptions/currencies`

**Authentication**: Required (Admin role)

**Authorization**: `[Authorize(Roles = "Admin")]`

**Request Body**:
```json
{
  "currencyCode": "EUR",
  "currencySymbol": "€",
  "currencyName": "Euro",
  "monthlyPrice": 8.99,
  "yearlyPrice": 89.00,
  "stripePriceIdMonthly": "price_9012Monthly",
  "stripePriceIdYearly": "price_9012Yearly",
  "localeCode": "en-EU",
  "decimalPlaces": 2
}
```

**Response**: `201 Created`

```json
{
  "id": 3,
  "currencyCode": "EUR",
  "currencySymbol": "€",
  "currencyName": "Euro",
  "monthlyPrice": 8.99,
  "yearlyPrice": 89.00,
  "stripePriceIdMonthly": "price_9012Monthly",
  "stripePriceIdYearly": "price_9012Yearly",
  "localeCode": "en-EU",
  "decimalPlaces": 2,
  "isActive": true,
  "isDefault": false,
  "createdAt": "2025-12-09T10:00:00Z"
}
```

**Error Responses**:

- `400 Bad Request`: Validation error
```json
{
  "error": "ValidationError",
  "message": "Yearly price must be less than 12 times monthly price",
  "field": "yearlyPrice"
}
```

- `409 Conflict`: Currency already exists
```json
{
  "error": "CurrencyExists",
  "message": "Currency 'EUR' is already configured"
}
```

- `403 Forbidden`: User is not an admin

---

## Webhook Endpoint (Stripe)

### 10. Stripe Webhook Handler

**Endpoint**: `POST /api/webhooks/stripe`

**Authentication**: Stripe signature verification (not JWT)

**Request Headers**:
```http
Content-Type: application/json
Stripe-Signature: t=1234567890,v1=abc123def456...
```

**Request Body**: Raw Stripe Event JSON

**Supported Events**:
- `checkout.session.completed`: New subscription created
- `customer.subscription.updated`: Subscription modified
- `customer.subscription.deleted`: Subscription ended
- `invoice.payment_succeeded`: Successful payment
- `invoice.payment_failed`: Failed payment

**Response**: `200 OK`

```json
{
  "received": true
}
```

**Error Responses**:
- `400 Bad Request`: Invalid signature or malformed payload
- `500 Internal Server Error`: Processing error (Stripe will retry)

---

## Data Transfer Objects (DTOs)

### Request DTOs

```csharp
// CreateCheckoutSessionRequest.cs
public class CreateCheckoutSessionRequest
{
    [Required]
    public string PriceId { get; set; }
    
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; }
    
    public string DiscountCode { get; set; }
    
    [Url]
    public string SuccessUrl { get; set; }
    
    [Url]
    public string CancelUrl { get; set; }
}

// CreatePortalSessionRequest.cs
public class CreatePortalSessionRequest
{
    [Required]
    [Url]
    public string ReturnUrl { get; set; }
}

// CancelSubscriptionRequest.cs
public class CancelSubscriptionRequest
{
    [Required]
    [StringLength(500)]
    public string Reason { get; set; }
    
    [StringLength(2000)]
    public string Feedback { get; set; }
}

// ValidateDiscountRequest.cs
public class ValidateDiscountRequest
{
    [Required]
    public string Code { get; set; }
    
    [Required]
    public string PriceId { get; set; }
}

// CreateCurrencyPricingRequest.cs (Admin)
public class CreateCurrencyPricingRequest
{
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string CurrencyCode { get; set; }
    
    [Required]
    public string CurrencySymbol { get; set; }
    
    [Required]
    public string CurrencyName { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal MonthlyPrice { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal YearlyPrice { get; set; }
    
    [Required]
    public string StripePriceIdMonthly { get; set; }
    
    [Required]
    public string StripePriceIdYearly { get; set; }
    
    [Required]
    public string LocaleCode { get; set; }
    
    [Range(0, 4)]
    public int DecimalPlaces { get; set; } = 2;
}
```

### Response DTOs

```csharp
// SubscriptionPlanResponse.cs
public class SubscriptionPlanResponse
{
    public string Tier { get; set; }
    public string Name { get; set; }
    public string Currency { get; set; }
    public string CurrencySymbol { get; set; }
    public decimal? MonthlyPrice { get; set; }
    public decimal? YearlyPrice { get; set; }
    public decimal? YearlySavings { get; set; }
    public int? SavingsPercentage { get; set; }
    public List<string> Features { get; set; }
    public string StripePriceIdMonthly { get; set; }
    public string StripePriceIdYearly { get; set; }
}

// SubscriptionStatusResponse.cs
public class SubscriptionStatusResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Tier { get; set; }
    public string Status { get; set; }
    public string Currency { get; set; }
    public string CurrencySymbol { get; set; }
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? CancelledAt { get; set; }
    public bool HasActiveAccess { get; set; }
    public bool IsGracePeriod { get; set; }
}

// CheckoutSessionResponse.cs
public class CheckoutSessionResponse
{
    public string SessionId { get; set; }
    public string SessionUrl { get; set; }
    public DateTime ExpiresAt { get; set; }
}

// PortalSessionResponse.cs
public class PortalSessionResponse
{
    public string PortalUrl { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

---

## HTTP Status Codes

- `200 OK`: Successful request
- `201 Created`: Resource created successfully
- `400 Bad Request`: Invalid request data or business rule violation
- `401 Unauthorized`: Missing or invalid authentication
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `409 Conflict`: Resource conflict (e.g., duplicate subscription)
- `500 Internal Server Error`: Unexpected server error
- `503 Service Unavailable`: External service (Stripe) unavailable

---

## Rate Limiting

- Standard endpoints: 100 requests per minute per user
- Checkout endpoint: 10 requests per minute per user
- Webhook endpoint: No rate limit (Stripe-controlled)
- Admin endpoints: 50 requests per minute per admin

---

## Versioning

API Version: `v1`

Future versions will use URL versioning: `/api/v2/subscriptions/...`

---

## Security

- All endpoints (except webhook) require valid JWT Bearer token
- Webhook endpoint validates Stripe signature using `Stripe-Signature` header
- HTTPS required for all requests
- CORS configured for frontend domain only
- Rate limiting to prevent abuse
- Input validation using FluentValidation
- SQL injection prevention via Entity Framework parameterized queries
