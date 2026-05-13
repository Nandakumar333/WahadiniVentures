# Data Model: Stripe Subscription Management

**Feature**: 008-stripe-subscription  
**Date**: 2025-12-09  
**Architecture**: Clean Architecture with Domain-Driven Design

## Overview

This document defines the domain entities, value objects, and their relationships for the Stripe subscription management feature. All entities follow Clean Architecture principles with encapsulation, factory methods, and domain events.

---

## Domain Entities

### 1. Subscription (Aggregate Root)

**Purpose**: Represents a user's premium membership with payment provider integration

**Responsibilities**:
- Manage subscription lifecycle (activate, cancel, renew, expire)
- Track payment provider identifiers and synchronization
- Enforce business rules for subscription status transitions
- Emit domain events for subscription state changes

**Properties**:

```csharp
public class Subscription : BaseEntity
{
    // Identity
    public int Id { get; private set; }
    public int UserId { get; private set; }
    
    // Payment Provider Integration
    public string StripeCustomerId { get; private set; }
    public string StripeSubscriptionId { get; private set; }
    public string StripePriceId { get; private set; }
    
    // Subscription Details
    public SubscriptionTier Tier { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public string CurrencyCode { get; private set; } // ISO 4217 (USD, INR, EUR, etc.)
    
    // Billing Cycle
    public DateTime CurrentPeriodStart { get; private set; }
    public DateTime CurrentPeriodEnd { get; private set; }
    public bool CancelAtPeriodEnd { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    
    // Audit
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    
    // Navigation Properties
    public virtual User User { get; private set; }
    public virtual ICollection<SubscriptionHistory> History { get; private set; }
    
    // Private constructor for EF Core
    private Subscription() 
    {
        History = new List<SubscriptionHistory>();
    }
    
    // Factory Method - Create New Subscription
    public static Subscription Create(
        int userId,
        string stripeCustomerId,
        string stripeSubscriptionId,
        string stripePriceId,
        SubscriptionTier tier,
        string currencyCode,
        DateTime periodStart,
        DateTime periodEnd)
    {
        var subscription = new Subscription
        {
            UserId = userId,
            StripeCustomerId = stripeCustomerId,
            StripeSubscriptionId = stripeSubscriptionId,
            StripePriceId = stripePriceId,
            Tier = tier,
            Status = SubscriptionStatus.Active,
            CurrencyCode = currencyCode,
            CurrentPeriodStart = periodStart,
            CurrentPeriodEnd = periodEnd,
            CancelAtPeriodEnd = false,
            CreatedAt = DateTime.UtcNow
        };
        
        subscription.AddDomainEvent(new SubscriptionCreatedEvent(subscription));
        return subscription;
    }
    
    // Business Logic Methods
    
    public void Activate(DateTime periodStart, DateTime periodEnd)
    {
        if (Status == SubscriptionStatus.Active)
            throw new InvalidOperationException("Subscription is already active");
            
        Status = SubscriptionStatus.Active;
        CurrentPeriodStart = periodStart;
        CurrentPeriodEnd = periodEnd;
        CancelAtPeriodEnd = false;
        CancelledAt = null;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SubscriptionActivatedEvent(this));
        RecordHistory("Subscription activated");
    }
    
    public void ScheduleCancellation()
    {
        if (Status != SubscriptionStatus.Active)
            throw new InvalidOperationException("Can only cancel active subscriptions");
            
        if (CancelAtPeriodEnd)
            throw new InvalidOperationException("Subscription already scheduled for cancellation");
            
        CancelAtPeriodEnd = true;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SubscriptionCancellationScheduledEvent(this));
        RecordHistory("Subscription cancellation scheduled");
    }
    
    public void Reactivate()
    {
        if (!CancelAtPeriodEnd)
            throw new InvalidOperationException("Subscription is not scheduled for cancellation");
            
        if (Status != SubscriptionStatus.Active)
            throw new InvalidOperationException("Can only reactivate active subscriptions");
            
        CancelAtPeriodEnd = false;
        CancelledAt = null;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SubscriptionReactivatedEvent(this));
        RecordHistory("Subscription reactivated");
    }
    
    public void Renew(DateTime newPeriodStart, DateTime newPeriodEnd)
    {
        if (Status != SubscriptionStatus.Active)
            throw new InvalidOperationException("Can only renew active subscriptions");
            
        CurrentPeriodStart = newPeriodStart;
        CurrentPeriodEnd = newPeriodEnd;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SubscriptionRenewedEvent(this));
        RecordHistory("Subscription renewed");
    }
    
    public void MarkPastDue()
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new InvalidOperationException("Cannot mark cancelled subscription as past due");
            
        Status = SubscriptionStatus.PastDue;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SubscriptionPaymentFailedEvent(this));
        RecordHistory("Payment failed - marked past due");
    }
    
    public void Cancel(string reason)
    {
        Status = SubscriptionStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SubscriptionCancelledEvent(this, reason));
        RecordHistory($"Subscription cancelled: {reason}");
    }
    
    public void Expire()
    {
        if (Status != SubscriptionStatus.Active && Status != SubscriptionStatus.PastDue)
            throw new InvalidOperationException("Only active or past due subscriptions can expire");
            
        Status = SubscriptionStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SubscriptionExpiredEvent(this));
        RecordHistory("Subscription expired");
    }
    
    public void UpdateTier(SubscriptionTier newTier, string newStripePriceId)
    {
        if (Status != SubscriptionStatus.Active)
            throw new InvalidOperationException("Can only update tier for active subscriptions");
            
        var oldTier = Tier;
        Tier = newTier;
        StripePriceId = newStripePriceId;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SubscriptionTierChangedEvent(this, oldTier, newTier));
        RecordHistory($"Tier changed from {oldTier} to {newTier}");
    }
    
    public bool HasActiveAccess()
    {
        return Status == SubscriptionStatus.Active && 
               CurrentPeriodEnd > DateTime.UtcNow;
    }
    
    public bool IsGracePeriod()
    {
        return Status == SubscriptionStatus.PastDue && 
               CurrentPeriodEnd > DateTime.UtcNow;
    }
    
    private void RecordHistory(string description)
    {
        History.Add(SubscriptionHistory.Create(Id, description, Status));
    }
}
```

**Enumerations**:

```csharp
public enum SubscriptionTier
{
    Free = 0,
    MonthlyPremium = 1,
    YearlyPremium = 2
}

public enum SubscriptionStatus
{
    Active = 1,
    PastDue = 2,
    Cancelled = 3,
    Incomplete = 4
}
```

**Validation Rules**:
- UserId must reference existing user
- StripeCustomerId and StripeSubscriptionId required for premium tiers
- CurrencyCode must be valid ISO 4217 format (3 characters)
- CurrentPeriodEnd must be after CurrentPeriodStart
- Cannot cancel already cancelled subscription
- Cannot activate expired subscription without renewal

---

### 2. CurrencyPricing (Entity)

**Purpose**: Admin-configurable pricing for subscription plans across multiple currencies

**Responsibilities**:
- Store currency-specific pricing information
- Link to Stripe Price IDs for checkout session creation
- Support dynamic pricing strategies per region
- Track pricing history for audit purposes

**Properties**:

```csharp
public class CurrencyPricing : BaseEntity
{
    // Identity
    public int Id { get; private set; }
    
    // Currency Information
    public string CurrencyCode { get; private set; } // ISO 4217 (USD, INR, EUR, JPY, GBP)
    public string CurrencySymbol { get; private set; } // $, ₹, €, ¥, £
    public string CurrencyName { get; private set; } // US Dollar, Indian Rupee, etc.
    
    // Stripe Integration
    public string StripePriceIdMonthly { get; private set; }
    public string StripePriceIdYearly { get; private set; }
    
    // Display Prices (for UI, actual charge handled by Stripe)
    public decimal MonthlyPrice { get; private set; }
    public decimal YearlyPrice { get; private set; }
    
    // Locale Information
    public string LocaleCode { get; private set; } // en-US, hi-IN, fr-FR
    public int DecimalPlaces { get; private set; } // 2 for most, 0 for JPY
    
    // Status
    public bool IsActive { get; private set; }
    public bool IsDefault { get; private set; } // USD is default
    
    // Audit
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    
    // Private constructor
    private CurrencyPricing() { }
    
    // Factory Method
    public static CurrencyPricing Create(
        string currencyCode,
        string currencySymbol,
        string currencyName,
        string stripePriceIdMonthly,
        string stripePriceIdYearly,
        decimal monthlyPrice,
        decimal yearlyPrice,
        string localeCode,
        int decimalPlaces)
    {
        ValidateCurrencyCode(currencyCode);
        ValidatePrices(monthlyPrice, yearlyPrice);
        
        return new CurrencyPricing
        {
            CurrencyCode = currencyCode.ToUpper(),
            CurrencySymbol = currencySymbol,
            CurrencyName = currencyName,
            StripePriceIdMonthly = stripePriceIdMonthly,
            StripePriceIdYearly = stripePriceIdYearly,
            MonthlyPrice = monthlyPrice,
            YearlyPrice = yearlyPrice,
            LocaleCode = localeCode,
            DecimalPlaces = decimalPlaces,
            IsActive = true,
            IsDefault = false,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    // Business Logic
    
    public void UpdatePricing(
        decimal newMonthlyPrice,
        decimal newYearlyPrice,
        string newStripePriceIdMonthly,
        string newStripePriceIdYearly)
    {
        ValidatePrices(newMonthlyPrice, newYearlyPrice);
        
        MonthlyPrice = newMonthlyPrice;
        YearlyPrice = newYearlyPrice;
        StripePriceIdMonthly = newStripePriceIdMonthly;
        StripePriceIdYearly = newStripePriceIdYearly;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CurrencyPricingUpdatedEvent(this));
    }
    
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Deactivate()
    {
        if (IsDefault)
            throw new InvalidOperationException("Cannot deactivate default currency");
            
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetAsDefault()
    {
        IsDefault = true;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public string FormatPrice(decimal amount)
    {
        var formatted = amount.ToString($"F{DecimalPlaces}");
        return $"{CurrencySymbol}{formatted}";
    }
    
    public decimal CalculateYearlySavings()
    {
        return (MonthlyPrice * 12) - YearlyPrice;
    }
    
    public decimal CalculateSavingsPercentage()
    {
        var monthlyAnnual = MonthlyPrice * 12;
        return monthlyAnnual > 0 ? ((monthlyAnnual - YearlyPrice) / monthlyAnnual) * 100 : 0;
    }
    
    private static void ValidateCurrencyCode(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Length != 3)
            throw new ArgumentException("Currency code must be 3 characters (ISO 4217)");
    }
    
    private static void ValidatePrices(decimal monthly, decimal yearly)
    {
        if (monthly <= 0)
            throw new ArgumentException("Monthly price must be greater than zero");
            
        if (yearly <= 0)
            throw new ArgumentException("Yearly price must be greater than zero");
            
        if (yearly >= monthly * 12)
            throw new ArgumentException("Yearly price should be less than 12 months of monthly price");
    }
}
```

**Validation Rules**:
- Currency code must be exactly 3 uppercase characters (ISO 4217)
- Monthly and yearly prices must be positive
- Yearly price should provide savings (less than monthly × 12)
- Only one currency can be marked as default
- Cannot deactivate default currency
- Stripe Price IDs must be valid format

---

### 3. WebhookEvent (Entity)

**Purpose**: Audit trail and idempotency tracking for payment provider webhooks

**Responsibilities**:
- Store raw webhook payloads for debugging
- Prevent duplicate event processing via idempotency
- Track processing status and errors
- Support event replay for failed processing

**Properties**:

```csharp
public class WebhookEvent : BaseEntity
{
    // Identity
    public int Id { get; private set; }
    
    // Stripe Event Information
    public string StripeEventId { get; private set; } // Idempotency key
    public string EventType { get; private set; } // checkout.session.completed, etc.
    
    // Payload
    public string PayloadJson { get; private set; } // Raw JSON for replay
    
    // Processing Status
    public WebhookProcessingStatus Status { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string ErrorMessage { get; private set; }
    public int RetryCount { get; private set; }
    
    // Metadata
    public string RelatedEntityType { get; private set; } // Subscription, User, etc.
    public string RelatedEntityId { get; private set; }
    
    // Audit
    public DateTime CreatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    
    private WebhookEvent() { }
    
    public static WebhookEvent Create(string stripeEventId, string eventType, string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(stripeEventId))
            throw new ArgumentException("Stripe event ID is required");
            
        return new WebhookEvent
        {
            StripeEventId = stripeEventId,
            EventType = eventType,
            PayloadJson = payloadJson,
            Status = WebhookProcessingStatus.Pending,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public void MarkAsProcessing()
    {
        Status = WebhookProcessingStatus.Processing;
    }
    
    public void MarkAsProcessed(string relatedEntityType, string relatedEntityId)
    {
        Status = WebhookProcessingStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
        RelatedEntityType = relatedEntityType;
        RelatedEntityId = relatedEntityId;
    }
    
    public void MarkAsFailed(string errorMessage)
    {
        Status = WebhookProcessingStatus.Failed;
        ErrorMessage = errorMessage;
        RetryCount++;
    }
    
    public bool CanRetry()
    {
        return Status == WebhookProcessingStatus.Failed && RetryCount < 3;
    }
}

public enum WebhookProcessingStatus
{
    Pending = 1,
    Processing = 2,
    Processed = 3,
    Failed = 4
}
```

---

### 4. SubscriptionHistory (Entity)

**Purpose**: Track subscription lifecycle events for audit and debugging

**Properties**:

```csharp
public class SubscriptionHistory : BaseEntity
{
    public int Id { get; private set; }
    public int SubscriptionId { get; private set; }
    public string Description { get; private set; }
    public SubscriptionStatus StatusBefore { get; private set; }
    public SubscriptionStatus StatusAfter { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    public virtual Subscription Subscription { get; private set; }
    
    private SubscriptionHistory() { }
    
    public static SubscriptionHistory Create(
        int subscriptionId,
        string description,
        SubscriptionStatus currentStatus)
    {
        return new SubscriptionHistory
        {
            SubscriptionId = subscriptionId,
            Description = description,
            StatusAfter = currentStatus,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

---

## Value Objects

### Money (Future Enhancement)

```csharp
public class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string CurrencyCode { get; private set; }
    
    private Money(decimal amount, string currencyCode)
    {
        Amount = amount;
        CurrencyCode = currencyCode;
    }
    
    public static Money Create(decimal amount, string currencyCode)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative");
            
        return new Money(amount, currencyCode.ToUpper());
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return CurrencyCode;
    }
}
```

---

## Domain Events

```csharp
public class SubscriptionCreatedEvent : DomainEvent
{
    public Subscription Subscription { get; }
    
    public SubscriptionCreatedEvent(Subscription subscription)
    {
        Subscription = subscription;
    }
}

public class SubscriptionActivatedEvent : DomainEvent
{
    public Subscription Subscription { get; }
    
    public SubscriptionActivatedEvent(Subscription subscription)
    {
        Subscription = subscription;
    }
}

public class SubscriptionCancelledEvent : DomainEvent
{
    public Subscription Subscription { get; }
    public string Reason { get; }
    
    public SubscriptionCancelledEvent(Subscription subscription, string reason)
    {
        Subscription = subscription;
        Reason = reason;
    }
}

public class SubscriptionExpiredEvent : DomainEvent
{
    public Subscription Subscription { get; }
    
    public SubscriptionExpiredEvent(Subscription subscription)
    {
        Subscription = subscription;
    }
}

public class SubscriptionPaymentFailedEvent : DomainEvent
{
    public Subscription Subscription { get; }
    
    public SubscriptionPaymentFailedEvent(Subscription subscription)
    {
        Subscription = subscription;
    }
}

public class CurrencyPricingUpdatedEvent : DomainEvent
{
    public CurrencyPricing CurrencyPricing { get; }
    
    public CurrencyPricingUpdatedEvent(CurrencyPricing currencyPricing)
    {
        CurrencyPricing = currencyPricing;
    }
}
```

---

## Entity Relationships

```
User (existing entity)
  ├─> Subscription (1:1)
  │     ├─> SubscriptionHistory (1:many)
  │     └─> User (many:1)
  │
  └─> DiscountCode (existing, from Rewards system)

CurrencyPricing (independent)
  └─> No direct relationships (referenced by currency code)

WebhookEvent (independent)
  └─> No direct relationships (metadata only)
```

---

## Database Schema (PostgreSQL)

```sql
-- Subscriptions Table
CREATE TABLE Subscriptions (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    StripeCustomerId VARCHAR(255),
    StripeSubscriptionId VARCHAR(255),
    StripePriceId VARCHAR(255),
    Tier VARCHAR(50) NOT NULL,
    Status VARCHAR(50) NOT NULL,
    CurrencyCode VARCHAR(3) NOT NULL,
    CurrentPeriodStart TIMESTAMP NOT NULL,
    CurrentPeriodEnd TIMESTAMP NOT NULL,
    CancelAtPeriodEnd BOOLEAN NOT NULL DEFAULT FALSE,
    CancelledAt TIMESTAMP NULL,
    CreatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMP NULL,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    
    CONSTRAINT UC_User_Subscription UNIQUE (UserId),
    CONSTRAINT CK_CurrencyCode CHECK (LENGTH(CurrencyCode) = 3)
);

CREATE INDEX IX_Subscriptions_UserId ON Subscriptions(UserId);
CREATE INDEX IX_Subscriptions_StripeCustomerId ON Subscriptions(StripeCustomerId);
CREATE INDEX IX_Subscriptions_StripeSubscriptionId ON Subscriptions(StripeSubscriptionId);
CREATE INDEX IX_Subscriptions_Status_PeriodEnd ON Subscriptions(Status, CurrentPeriodEnd);

-- Currency Pricing Table
CREATE TABLE CurrencyPricing (
    Id SERIAL PRIMARY KEY,
    CurrencyCode VARCHAR(3) NOT NULL UNIQUE,
    CurrencySymbol VARCHAR(10) NOT NULL,
    CurrencyName VARCHAR(100) NOT NULL,
    StripePriceIdMonthly VARCHAR(255) NOT NULL,
    StripePriceIdYearly VARCHAR(255) NOT NULL,
    MonthlyPrice DECIMAL(18, 2) NOT NULL,
    YearlyPrice DECIMAL(18, 2) NOT NULL,
    LocaleCode VARCHAR(10) NOT NULL,
    DecimalPlaces INTEGER NOT NULL DEFAULT 2,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    IsDefault BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMP NULL,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    
    CONSTRAINT CK_MonthlyPrice CHECK (MonthlyPrice > 0),
    CONSTRAINT CK_YearlyPrice CHECK (YearlyPrice > 0)
);

CREATE INDEX IX_CurrencyPricing_Code_Active ON CurrencyPricing(CurrencyCode, IsActive);
CREATE UNIQUE INDEX IX_CurrencyPricing_Default ON CurrencyPricing(IsDefault) WHERE IsDefault = TRUE;

-- Webhook Events Table
CREATE TABLE WebhookEvents (
    Id SERIAL PRIMARY KEY,
    StripeEventId VARCHAR(255) NOT NULL UNIQUE,
    EventType VARCHAR(100) NOT NULL,
    PayloadJson TEXT NOT NULL,
    Status VARCHAR(50) NOT NULL,
    ProcessedAt TIMESTAMP NULL,
    ErrorMessage TEXT NULL,
    RetryCount INTEGER NOT NULL DEFAULT 0,
    RelatedEntityType VARCHAR(100) NULL,
    RelatedEntityId VARCHAR(100) NULL,
    CreatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX IX_WebhookEvents_StripeEventId ON WebhookEvents(StripeEventId);
CREATE INDEX IX_WebhookEvents_EventType_Created ON WebhookEvents(EventType, CreatedAt);
CREATE INDEX IX_WebhookEvents_Status ON WebhookEvents(Status);

-- Subscription History Table
CREATE TABLE SubscriptionHistory (
    Id SERIAL PRIMARY KEY,
    SubscriptionId INTEGER NOT NULL REFERENCES Subscriptions(Id) ON DELETE CASCADE,
    Description VARCHAR(500) NOT NULL,
    StatusBefore VARCHAR(50),
    StatusAfter VARCHAR(50) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX IX_SubscriptionHistory_SubscriptionId ON SubscriptionHistory(SubscriptionId);
CREATE INDEX IX_SubscriptionHistory_CreatedAt ON SubscriptionHistory(CreatedAt);
```

---

## Aggregate Boundaries

**Subscription Aggregate**:
- Root: Subscription
- Entities: SubscriptionHistory
- Value Objects: None (future: Money)
- Invariants: 
  - User can only have one active subscription
  - Period end must be after period start
  - Cannot cancel already cancelled subscription

**CurrencyPricing Aggregate**:
- Root: CurrencyPricing
- Entities: None
- Value Objects: None
- Invariants:
  - Only one default currency
  - Yearly price must provide savings
  - Currency code must be unique

---

## Migration Path

1. **Add columns to existing Users table** (optional, can keep separate):
   - StripeCustomerId
   - CurrencyPreference

2. **Create new tables**:
   - Subscriptions
   - CurrencyPricing
   - WebhookEvents
   - SubscriptionHistory

3. **Seed data**:
   - Insert default currency pricing (USD)
   - Optional: Insert other currencies (INR, EUR, etc.)

4. **Backfill existing users**:
   - Set all existing users to Free tier
   - StripeCustomerId will be populated on first checkout
