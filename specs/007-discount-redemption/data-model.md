# Data Model: Point-Based Discount Redemption System

**Feature**: 007-discount-redemption  
**Date**: December 5, 2025  
**Status**: Phase 1 Complete

## Overview

This document defines the domain entities, relationships, and business rules for the discount redemption system. The design follows Domain-Driven Design principles with rich domain models containing business logic and validation.

---

## Entity Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                                                                              │
│   ┌──────────────┐                    ┌───────────────────────────┐         │
│   │     User     │                    │      DiscountType         │         │
│   │   (Existing) │                    │        (NEW)              │         │
│   ├──────────────┤                    ├───────────────────────────┤         │
│   │ Id (PK)      │                    │ Id (PK)                   │         │
│   │ Email        │                    │ Name                      │         │
│   │ PointsBalance│◄───────┐           │ Code                      │         │
│   │ RowVersion   │        │           │ Description               │         │
│   └──────────────┘        │           │ PointCost                 │         │
│                           │           │ DiscountPercentage        │         │
│                           │           │ IsActive                  │         │
│                           │           │ ExpiryDate?               │         │
│                           │           │ MaxRedemptions            │         │
│                           │           │ CurrentRedemptions        │         │
│                           │           │ DurationDays              │         │
│                           │           │ IsUniqueCode              │         │
│                           │           │ CreatedAt                 │         │
│                           │           │ UpdatedAt                 │         │
│                           │           │ IsDeleted                 │         │
│                           │           └───────────────────────────┘         │
│                           │                      │                           │
│                           │                      │                           │
│                           │                      │                           │
│                           │           ┌──────────▼────────────────┐         │
│                           │           │  UserDiscountRedemption   │         │
│                           │           │         (NEW)             │         │
│                           │           ├───────────────────────────┤         │
│                           └───────────┤ Id (PK)                   │         │
│                                       │ UserId (FK)               │         │
│                                       │ DiscountTypeId (FK)       │         │
│                                       │ CodeIssued                │         │
│                                       │ RedeemedAt                │         │
│                                       │ ExpiryDate                │         │
│                                       │ IsUsed                    │         │
│                                       │ UsedAt?                   │         │
│                                       │ CreatedAt                 │         │
│                                       └───────────────────────────┘         │
│                                                  │                           │
│                                                  │                           │
│                                       ┌──────────▼────────────────┐         │
│                                       │   RewardTransaction       │         │
│                                       │      (Existing)           │         │
│                                       ├───────────────────────────┤         │
│                                       │ Id (PK)                   │         │
│                                       │ UserId (FK)               │         │
│                                       │ Amount (negative)         │         │
│                                       │ Type (PointsDeduction)    │         │
│                                       │ Description               │         │
│                                       │ CreatedAt                 │         │
│                                       └───────────────────────────┘         │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Domain Entities

### 1. DiscountType (NEW)

**Purpose**: Represents a discount campaign configuration that admins create and users can redeem.

**File**: `backend/src/WahadiniCryptoQuest.Core/Entities/DiscountType.cs`

#### Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| Id | Guid | Yes | Primary key |
| Name | string(100) | Yes | Display name (e.g., "10% Off Monthly") |
| Code | string(50) | Yes | Stripe Promotion Code (e.g., "SAVE10") |
| Description | string(500) | No | Detailed description for users |
| PointCost | int | Yes | Points required to redeem (must be > 0) |
| DiscountPercentage | decimal(5,2) | Yes | Discount amount (0-100) |
| IsActive | bool | Yes | Whether discount is currently available (default: true) |
| ExpiryDate | DateTime? | No | Campaign end date (null = no expiry) |
| MaxRedemptions | int | Yes | Global redemption limit (0 = unlimited) |
| CurrentRedemptions | int | Yes | Current redemption count (default: 0) |
| DurationDays | int | Yes | Days code is valid after redemption (default: 30) |
| IsUniqueCode | bool | Yes | Generate unique suffix per redemption (default: false) |
| CreatedAt | DateTime | Yes | Entity creation timestamp |
| UpdatedAt | DateTime | Yes | Last modification timestamp |
| IsDeleted | bool | Yes | Soft delete flag (default: false) |

#### Navigation Properties

```csharp
public virtual ICollection<UserDiscountRedemption> Redemptions { get; set; }
```

#### Business Rules & Validations

```csharp
public class DiscountType : BaseEntity
{
    // Properties...
    
    /// <summary>
    /// Determines if discount is currently available for redemption
    /// </summary>
    public bool IsAvailable()
    {
        if (!IsActive || IsDeleted) return false;
        if (ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow) return false;
        if (MaxRedemptions > 0 && CurrentRedemptions >= MaxRedemptions) return false;
        return true;
    }
    
    /// <summary>
    /// Generates code for redemption (static or unique based on configuration)
    /// </summary>
    public string GenerateCode(Guid redemptionId)
    {
        if (!IsUniqueCode) return Code;
        
        // Append last 8 characters of redemption GUID
        var suffix = redemptionId.ToString("N")[^8..].ToUpper();
        return $"{Code}-{suffix}";
    }
    
    /// <summary>
    /// Increments redemption counter (call within transaction)
    /// </summary>
    public void IncrementRedemptions()
    {
        CurrentRedemptions++;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Calculates expiry date for a new redemption
    /// </summary>
    public DateTime CalculateExpiryDate()
    {
        return DateTime.UtcNow.AddDays(DurationDays);
    }
    
    // Domain validations
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new DomainException("Discount name is required");
        if (string.IsNullOrWhiteSpace(Code))
            throw new DomainException("Discount code is required");
        if (PointCost <= 0)
            throw new DomainException("Point cost must be greater than zero");
        if (DiscountPercentage < 0 || DiscountPercentage > 100)
            throw new DomainException("Discount percentage must be between 0 and 100");
        if (DurationDays <= 0)
            throw new DomainException("Duration days must be greater than zero");
        if (MaxRedemptions < 0)
            throw new DomainException("Max redemptions cannot be negative");
    }
}
```

#### Factory Methods

```csharp
public static DiscountType Create(
    string name,
    string code,
    string description,
    int pointCost,
    decimal discountPercentage,
    int durationDays = 30,
    int maxRedemptions = 0,
    DateTime? expiryDate = null,
    bool isUniqueCode = false)
{
    var discount = new DiscountType
    {
        Id = Guid.NewGuid(),
        Name = name,
        Code = code,
        Description = description,
        PointCost = pointCost,
        DiscountPercentage = discountPercentage,
        DurationDays = durationDays,
        MaxRedemptions = maxRedemptions,
        ExpiryDate = expiryDate,
        IsUniqueCode = isUniqueCode,
        IsActive = true,
        CurrentRedemptions = 0,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        IsDeleted = false
    };
    
    discount.Validate();
    return discount;
}
```

---

### 2. UserDiscountRedemption (NEW)

**Purpose**: Records a user's redemption of a discount type, including the issued code and expiry.

**File**: `backend/src/WahadiniCryptoQuest.Core/Entities/UserDiscountRedemption.cs`

#### Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| Id | Guid | Yes | Primary key |
| UserId | Guid | Yes | Foreign key to User |
| DiscountTypeId | Guid | Yes | Foreign key to DiscountType |
| CodeIssued | string(100) | Yes | Actual code given to user (may include unique suffix) |
| RedeemedAt | DateTime | Yes | Redemption timestamp |
| ExpiryDate | DateTime | Yes | When this code expires |
| IsUsed | bool | Yes | Whether code was used at checkout (default: false) |
| UsedAt | DateTime? | No | When code was used (populated via Stripe webhook) |
| CreatedAt | DateTime | Yes | Entity creation timestamp |

#### Navigation Properties

```csharp
public virtual User User { get; set; }
public virtual DiscountType DiscountType { get; set; }
```

#### Business Rules & Validations

```csharp
public class UserDiscountRedemption : BaseEntity
{
    // Properties...
    
    /// <summary>
    /// Checks if redemption is still valid (not expired, not used)
    /// </summary>
    public bool IsValid()
    {
        return !IsUsed && ExpiryDate > DateTime.UtcNow;
    }
    
    /// <summary>
    /// Marks code as used (called via Stripe webhook integration)
    /// </summary>
    public void MarkAsUsed()
    {
        if (IsUsed)
            throw new DomainException("Code already marked as used");
        
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }
    
    // Domain validations
    public void Validate()
    {
        if (UserId == Guid.Empty)
            throw new DomainException("User ID is required");
        if (DiscountTypeId == Guid.Empty)
            throw new DomainException("Discount Type ID is required");
        if (string.IsNullOrWhiteSpace(CodeIssued))
            throw new DomainException("Code issued is required");
        if (ExpiryDate <= RedeemedAt)
            throw new DomainException("Expiry date must be after redemption date");
    }
}
```

#### Factory Methods

```csharp
public static UserDiscountRedemption Create(
    Guid userId,
    Guid discountTypeId,
    string codeIssued,
    DateTime expiryDate)
{
    var redemption = new UserDiscountRedemption
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        DiscountTypeId = discountTypeId,
        CodeIssued = codeIssued,
        RedeemedAt = DateTime.UtcNow,
        ExpiryDate = expiryDate,
        IsUsed = false,
        UsedAt = null,
        CreatedAt = DateTime.UtcNow
    };
    
    redemption.Validate();
    return redemption;
}
```

---

### 3. User (EXISTING - Extended)

**Purpose**: Existing user entity extended with points management for redemptions.

**File**: `backend/src/WahadiniCryptoQuest.Core/Entities/User.cs`

#### New Methods for Discount System

```csharp
public partial class User : BaseEntity
{
    // Existing properties...
    public int PointsBalance { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; } // For optimistic concurrency
    
    // NEW: Navigation property
    public virtual ICollection<UserDiscountRedemption> DiscountRedemptions { get; set; }
    
    /// <summary>
    /// Deducts points for discount redemption (domain method with validation)
    /// </summary>
    public void DeductPoints(int amount, string reason)
    {
        if (amount <= 0)
            throw new DomainException("Deduction amount must be positive");
        
        if (PointsBalance < amount)
            throw new DomainException($"Insufficient points. Required: {amount}, Available: {PointsBalance}");
        
        PointsBalance -= amount;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Checks if user has sufficient points for redemption
    /// </summary>
    public bool HasSufficientPoints(int requiredPoints)
    {
        return PointsBalance >= requiredPoints;
    }
}
```

---

### 4. RewardTransaction (EXISTING - Extended)

**Purpose**: Existing audit trail entity extended to record point deductions for redemptions.

**File**: `backend/src/WahadiniCryptoQuest.Core/Entities/RewardTransaction.cs`

#### New Transaction Type

```csharp
public enum RewardTransactionType
{
    PointsEarned = 1,
    PointsDeducted = 2, // NEW: For discount redemptions
    PointsAdjustment = 3
}
```

#### New Factory Method

```csharp
public static RewardTransaction CreateDeduction(
    Guid userId,
    int amount,
    string description)
{
    return new RewardTransaction
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Amount = -amount, // Negative for deductions
        Type = RewardTransactionType.PointsDeducted,
        Description = description,
        CreatedAt = DateTime.UtcNow
    };
}
```

---

## Entity Relationships

### One-to-Many Relationships

1. **User → UserDiscountRedemption** (1:N)
   - One user can have many redemptions
   - Foreign Key: `UserDiscountRedemption.UserId`
   - Delete Behavior: Restrict (preserve audit trail)

2. **DiscountType → UserDiscountRedemption** (1:N)
   - One discount type can have many redemptions
   - Foreign Key: `UserDiscountRedemption.DiscountTypeId`
   - Delete Behavior: Restrict (preserve redemption history)

3. **User → RewardTransaction** (1:N)
   - One user can have many transactions
   - Foreign Key: `RewardTransaction.UserId`
   - Delete Behavior: Restrict (audit trail)

---

## EF Core Configurations

### DiscountTypeConfiguration

**File**: `backend/src/WahadiniCryptoQuest.DAL/Configurations/DiscountTypeConfiguration.cs`

```csharp
public class DiscountTypeConfiguration : IEntityTypeConfiguration<DiscountType>
{
    public void Configure(EntityTypeBuilder<DiscountType> builder)
    {
        builder.ToTable("DiscountTypes");
        
        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(d => d.Code)
            .IsUnique()
            .HasDatabaseName("IX_DiscountTypes_Code");
        
        builder.Property(d => d.Description)
            .HasMaxLength(500);
        
        builder.Property(d => d.PointCost)
            .IsRequired();
        
        builder.Property(d => d.DiscountPercentage)
            .IsRequired()
            .HasPrecision(5, 2);
        
        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(d => d.ExpiryDate)
            .IsRequired(false);
        
        builder.Property(d => d.MaxRedemptions)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(d => d.CurrentRedemptions)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(d => d.DurationDays)
            .IsRequired()
            .HasDefaultValue(30);
        
        builder.Property(d => d.IsUniqueCode)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.Property(d => d.CreatedAt)
            .IsRequired();
        
        builder.Property(d => d.UpdatedAt)
            .IsRequired();
        
        builder.Property(d => d.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
        
        // Indexes for queries
        builder.HasIndex(d => new { d.IsActive, d.ExpiryDate, d.IsDeleted })
            .HasDatabaseName("IX_DiscountTypes_Availability");
        
        // Relationships
        builder.HasMany(d => d.Redemptions)
            .WithOne(r => r.DiscountType)
            .HasForeignKey(r => r.DiscountTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Query filter for soft delete
        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
```

### UserDiscountRedemptionConfiguration

**File**: `backend/src/WahadiniCryptoQuest.DAL/Configurations/UserDiscountRedemptionConfiguration.cs`

```csharp
public class UserDiscountRedemptionConfiguration : IEntityTypeConfiguration<UserDiscountRedemption>
{
    public void Configure(EntityTypeBuilder<UserDiscountRedemption> builder)
    {
        builder.ToTable("UserDiscountRedemptions");
        
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.UserId)
            .IsRequired();
        
        builder.Property(r => r.DiscountTypeId)
            .IsRequired();
        
        builder.Property(r => r.CodeIssued)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(r => r.RedeemedAt)
            .IsRequired();
        
        builder.Property(r => r.ExpiryDate)
            .IsRequired();
        
        builder.Property(r => r.IsUsed)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.Property(r => r.UsedAt)
            .IsRequired(false);
        
        builder.Property(r => r.CreatedAt)
            .IsRequired();
        
        // Indexes for queries
        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("IX_UserDiscountRedemptions_UserId");
        
        builder.HasIndex(r => new { r.UserId, r.RedeemedAt })
            .HasDatabaseName("IX_UserDiscountRedemptions_UserId_RedeemedAt");
        
        builder.HasIndex(r => new { r.UserId, r.DiscountTypeId })
            .HasDatabaseName("IX_UserDiscountRedemptions_UserId_DiscountTypeId");
        
        builder.HasIndex(r => r.DiscountTypeId)
            .HasDatabaseName("IX_UserDiscountRedemptions_DiscountTypeId");
        
        // Relationships
        builder.HasOne(r => r.User)
            .WithMany(u => u.DiscountRedemptions)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(r => r.DiscountType)
            .WithMany(d => d.Redemptions)
            .HasForeignKey(r => r.DiscountTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

---

## Database Constraints

### Check Constraints

```sql
-- Ensure point cost is positive
ALTER TABLE DiscountTypes ADD CONSTRAINT CK_DiscountTypes_PointCost_Positive 
    CHECK (PointCost > 0);

-- Ensure discount percentage is between 0 and 100
ALTER TABLE DiscountTypes ADD CONSTRAINT CK_DiscountTypes_Percentage_Range 
    CHECK (DiscountPercentage >= 0 AND DiscountPercentage <= 100);

-- Ensure duration days is positive
ALTER TABLE DiscountTypes ADD CONSTRAINT CK_DiscountTypes_DurationDays_Positive 
    CHECK (DurationDays > 0);

-- Ensure max redemptions is non-negative
ALTER TABLE DiscountTypes ADD CONSTRAINT CK_DiscountTypes_MaxRedemptions_NonNegative 
    CHECK (MaxRedemptions >= 0);

-- Ensure expiry date is after redemption date
ALTER TABLE UserDiscountRedemptions ADD CONSTRAINT CK_UserDiscountRedemptions_ExpiryAfterRedemption 
    CHECK (ExpiryDate > RedeemedAt);
```

---

## Seed Data

**File**: `backend/src/WahadiniCryptoQuest.DAL/Data/DiscountSeeder.cs`

```csharp
public static class DiscountSeeder
{
    public static void SeedDiscounts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DiscountType>().HasData(
            new DiscountType
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Name = "10% Off Monthly Subscription",
                Code = "SAVE10",
                Description = "Redeem 500 points for 10% off your next monthly subscription payment",
                PointCost = 500,
                DiscountPercentage = 10.00m,
                IsActive = true,
                ExpiryDate = null,
                MaxRedemptions = 0,
                CurrentRedemptions = 0,
                DurationDays = 30,
                IsUniqueCode = false,
                CreatedAt = new DateTime(2025, 12, 5),
                UpdatedAt = new DateTime(2025, 12, 5),
                IsDeleted = false
            },
            new DiscountType
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Name = "20% Off Monthly Subscription",
                Code = "SAVE20",
                Description = "Redeem 1000 points for 20% off your next monthly subscription payment",
                PointCost = 1000,
                DiscountPercentage = 20.00m,
                IsActive = true,
                ExpiryDate = null,
                MaxRedemptions = 0,
                CurrentRedemptions = 0,
                DurationDays = 30,
                IsUniqueCode = false,
                CreatedAt = new DateTime(2025, 12, 5),
                UpdatedAt = new DateTime(2025, 12, 5),
                IsDeleted = false
            },
            new DiscountType
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Name = "50% Off Annual Subscription",
                Code = "SAVE50",
                Description = "Redeem 5000 points for 50% off your annual subscription",
                PointCost = 5000,
                DiscountPercentage = 50.00m,
                IsActive = true,
                ExpiryDate = new DateTime(2026, 12, 31),
                MaxRedemptions = 100,
                CurrentRedemptions = 0,
                DurationDays = 90,
                IsUniqueCode = false,
                CreatedAt = new DateTime(2025, 12, 5),
                UpdatedAt = new DateTime(2025, 12, 5),
                IsDeleted = false
            }
        );
    }
}
```

---

## Migration Script Overview

**Migration Name**: `AddDiscountSystem`

### Up Migration Operations

1. Create `DiscountTypes` table with all columns and indexes
2. Create `UserDiscountRedemptions` table with all columns and indexes
3. Add foreign key constraints
4. Add check constraints
5. Insert seed data (3 initial discount types)
6. Update `Users` table with `DiscountRedemptions` navigation property (no DB change, just EF mapping)

### Down Migration Operations

1. Drop foreign key constraints
2. Drop `UserDiscountRedemptions` table
3. Drop `DiscountTypes` table

---

## State Transitions

### DiscountType State Machine

```
┌─────────┐
│ Created │ (IsActive=true, IsDeleted=false)
└────┬────┘
     │
     ├─→ [Admin Activates] → IsActive = true
     ├─→ [Admin Deactivates] → IsActive = false
     ├─→ [Expiry Date Passed] → Auto-excluded from queries
     ├─→ [Max Redemptions Reached] → Auto-excluded from queries
     └─→ [Admin Soft Deletes] → IsDeleted = true (never shown)
```

### UserDiscountRedemption State Machine

```
┌──────────┐
│ Redeemed │ (IsUsed=false)
└────┬─────┘
     │
     ├─→ [User Uses Code at Checkout] → IsUsed = true, UsedAt = timestamp
     └─→ [Expiry Date Passes] → Still exists but marked invalid
```

---

## Query Patterns

### Common Queries

**Get Available Discounts for User**:
```csharp
var available = await _dbContext.DiscountTypes
    .Where(d => d.IsActive &&
                (d.ExpiryDate == null || d.ExpiryDate > DateTime.UtcNow) &&
                (d.MaxRedemptions == 0 || d.CurrentRedemptions < d.MaxRedemptions))
    .OrderBy(d => d.PointCost)
    .ToListAsync();
```

**Check if User Already Redeemed Specific Discount**:
```csharp
var hasRedeemed = await _dbContext.UserDiscountRedemptions
    .AnyAsync(r => r.UserId == userId && r.DiscountTypeId == discountTypeId);
```

**Get User's Active Redemptions**:
```csharp
var redemptions = await _dbContext.UserDiscountRedemptions
    .Include(r => r.DiscountType)
    .Where(r => r.UserId == userId && r.ExpiryDate > DateTime.UtcNow)
    .OrderByDescending(r => r.RedeemedAt)
    .ToListAsync();
```

**Get Discount Analytics**:
```csharp
var stats = await _dbContext.UserDiscountRedemptions
    .Where(r => r.DiscountTypeId == discountTypeId)
    .GroupBy(r => 1)
    .Select(g => new
    {
        TotalRedemptions = g.Count(),
        UniqueUsers = g.Select(r => r.UserId).Distinct().Count()
    })
    .FirstOrDefaultAsync();
```

---

## Conclusion

The data model provides a solid foundation for the discount redemption system with:
- Rich domain entities containing business logic
- Proper relationships and constraints
- Optimistic concurrency support
- Audit trail via RewardTransaction integration
- Soft delete for DiscountType
- Query filters for performance

**Next Steps**: Generate API contracts (Phase 1 continuation).
