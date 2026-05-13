# Data Model: Admin Dashboard

**Feature**: 009-admin-dashboard  
**Date**: December 16, 2025  
**Architecture**: Clean Architecture - Domain Layer

## Overview

This document defines the data model for the Admin Dashboard feature following WahadiniCryptoQuest's domain-driven design patterns. The admin dashboard primarily aggregates and displays existing entities rather than introducing many new domain entities.

## Domain Entities

### 1. AuditLogEntry (New Entity)

**Purpose**: Immutable record of all administrative actions for accountability and compliance.

**Domain Model**:
```csharp
namespace WahadiniCryptoQuest.Core.Entities
{
    public class AuditLogEntry : BaseEntity
    {
        // Entity Identity
        public Guid Id { get; private set; }
        
        // Audit Information
        public Guid AdminUserId { get; private set; }
        public string ActionType { get; private set; } // e.g., "BanUser", "ApproveTask", "UpdateCourse"
        public string ResourceType { get; private set; } // e.g., "User", "TaskSubmission", "Course"
        public string ResourceId { get; private set; }
        public string BeforeValue { get; private set; } // JSON snapshot
        public string AfterValue { get; private set; } // JSON snapshot
        public string IpAddress { get; private set; }
        public string UserAgent { get; private set; }
        
        // Timestamps (from BaseEntity)
        public DateTime CreatedAt { get; private set; }
        
        // Navigation Properties
        public virtual User AdminUser { get; private set; }
        
        // Factory Method
        public static AuditLogEntry Create(
            Guid adminUserId,
            string actionType,
            string resourceType,
            string resourceId,
            string beforeValue,
            string afterValue,
            string ipAddress,
            string userAgent)
        {
            if (adminUserId == Guid.Empty)
                throw new ArgumentException("Admin user ID cannot be empty", nameof(adminUserId));
            if (string.IsNullOrWhiteSpace(actionType))
                throw new ArgumentException("Action type is required", nameof(actionType));
            if (string.IsNullOrWhiteSpace(resourceType))
                throw new ArgumentException("Resource type is required", nameof(resourceType));
                
            return new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                AdminUserId = adminUserId,
                ActionType = actionType,
                ResourceType = resourceType,
                ResourceId = resourceId,
                BeforeValue = beforeValue,
                AfterValue = afterValue,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow
            };
        }
        
        // Private constructor (enforce factory method)
        private AuditLogEntry() { }
    }
}
```

**Validation Rules**:
- AdminUserId must reference valid admin user
- ActionType must be from predefined enum
- ResourceType must be from predefined enum
- CreatedAt is immutable (set on creation)
- No updates or deletes allowed (append-only)

**Database Schema**:
```sql
CREATE TABLE AuditLogs (
    Id UUID PRIMARY KEY,
    AdminUserId UUID NOT NULL REFERENCES AspNetUsers(Id),
    ActionType VARCHAR(100) NOT NULL,
    ResourceType VARCHAR(100) NOT NULL,
    ResourceId VARCHAR(255) NOT NULL,
    BeforeValue TEXT,
    AfterValue TEXT,
    IpAddress VARCHAR(45),
    UserAgent VARCHAR(500),
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IX_AuditLogs_AdminUserId ON AuditLogs(AdminUserId);
CREATE INDEX IX_AuditLogs_ActionType ON AuditLogs(ActionType);
CREATE INDEX IX_AuditLogs_CreatedAt ON AuditLogs(CreatedAt);
CREATE INDEX IX_AuditLogs_ResourceType_ResourceId ON AuditLogs(ResourceType, ResourceId);
```

---

### 2. UserNotification (New Entity)

**Purpose**: Store in-app notifications for users (task review results, announcements).

**Domain Model**:
```csharp
namespace WahadiniCryptoQuest.Core.Entities
{
    public class UserNotification : BaseEntity
    {
        // Entity Identity
        public Guid Id { get; private set; }
        
        // Notification Data
        public Guid UserId { get; private set; }
        public string Type { get; private set; } // "TaskApproved", "TaskRejected", "Announcement"
        public string Title { get; private set; }
        public string Message { get; private set; }
        public string RelatedEntityType { get; private set; } // "TaskSubmission", "Course", etc.
        public string RelatedEntityId { get; private set; }
        public bool IsRead { get; private set; }
        public DateTime? ReadAt { get; private set; }
        
        // Timestamps
        public DateTime CreatedAt { get; private set; }
        
        // Navigation Properties
        public virtual User User { get; private set; }
        
        // Factory Method
        public static UserNotification Create(
            Guid userId,
            string type,
            string title,
            string message,
            string relatedEntityType = null,
            string relatedEntityId = null)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required", nameof(title));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message is required", nameof(message));
                
            return new UserNotification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                RelatedEntityType = relatedEntityType,
                RelatedEntityId = relatedEntityId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
        }
        
        // Domain Methods
        public void MarkAsRead()
        {
            if (!IsRead)
            {
                IsRead = true;
                ReadAt = DateTime.UtcNow;
            }
        }
        
        // Private constructor
        private UserNotification() { }
    }
}
```

**Validation Rules**:
- UserId must reference valid user
- Type must be from predefined enum
- Title max length: 200 characters
- Message max length: 1000 characters
- IsRead can only transition from false to true

**Database Schema**:
```sql
CREATE TABLE UserNotifications (
    Id UUID PRIMARY KEY,
    UserId UUID NOT NULL REFERENCES AspNetUsers(Id),
    Type VARCHAR(50) NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Message VARCHAR(1000) NOT NULL,
    RelatedEntityType VARCHAR(100),
    RelatedEntityId VARCHAR(255),
    IsRead BOOLEAN NOT NULL DEFAULT FALSE,
    ReadAt TIMESTAMP,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IX_UserNotifications_UserId_IsRead ON UserNotifications(UserId, IsRead);
CREATE INDEX IX_UserNotifications_CreatedAt ON UserNotifications(CreatedAt);
```

---

### 3. DiscountCode (Extend Existing)

**Purpose**: Admin-created promotional codes for subscription discounts.

**Note**: This entity likely exists already. Document expected structure:

**Domain Model**:
```csharp
namespace WahadiniCryptoQuest.Core.Entities
{
    public class DiscountCode : BaseEntity
    {
        // Entity Identity
        public Guid Id { get; private set; }
        
        // Code Details
        public string Code { get; private set; } // Unique code string
        public DiscountType DiscountType { get; private set; } // Percentage or FixedAmount
        public decimal DiscountValue { get; private set; }
        public DateTime ExpirationDate { get; private set; }
        public int UsageLimit { get; private set; } // 0 = unlimited
        public int UsageCount { get; private set; }
        public bool IsActive { get; private set; }
        
        // Audit
        public Guid CreatedByAdminId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        
        // Navigation Properties
        public virtual User CreatedByAdmin { get; private set; }
        public virtual ICollection<DiscountRedemption> Redemptions { get; private set; }
        
        // Factory Method
        public static DiscountCode Create(
            string code,
            DiscountType discountType,
            decimal discountValue,
            DateTime expirationDate,
            int usageLimit,
            Guid createdByAdminId)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Code is required", nameof(code));
            if (discountValue <= 0)
                throw new ArgumentException("Discount value must be positive", nameof(discountValue));
            if (expirationDate <= DateTime.UtcNow)
                throw new ArgumentException("Expiration date must be in the future", nameof(expirationDate));
            if (usageLimit < 0)
                throw new ArgumentException("Usage limit cannot be negative", nameof(usageLimit));
                
            return new DiscountCode
            {
                Id = Guid.NewGuid(),
                Code = code.ToUpperInvariant(),
                DiscountType = discountType,
                DiscountValue = discountValue,
                ExpirationDate = expirationDate,
                UsageLimit = usageLimit,
                UsageCount = 0,
                IsActive = true,
                CreatedByAdminId = createdByAdminId,
                CreatedAt = DateTime.UtcNow
            };
        }
        
        // Domain Methods
        public bool CanRedeem()
        {
            if (!IsActive) return false;
            if (ExpirationDate < DateTime.UtcNow) return false;
            if (UsageLimit > 0 && UsageCount >= UsageLimit) return false;
            return true;
        }
        
        public void IncrementUsage()
        {
            if (!CanRedeem())
                throw new InvalidOperationException("Code cannot be redeemed");
            UsageCount++;
        }
        
        public void Deactivate()
        {
            IsActive = false;
        }
        
        // Private constructor
        private DiscountCode() { }
    }
    
    public enum DiscountType
    {
        Percentage = 0,
        FixedAmount = 1
    }
}
```

**Validation Rules**:
- Code must be unique (case-insensitive)
- Code format: Alphanumeric, 6-20 characters
- DiscountValue: 0-100 for Percentage, > 0 for FixedAmount
- ExpirationDate must be future date
- UsageLimit >= 0

---

### 4. PointAdjustment (New Entity)

**Purpose**: Record manual point balance adjustments by admins.

**Domain Model**:
```csharp
namespace WahadiniCryptoQuest.Core.Entities
{
    public class PointAdjustment : BaseEntity
    {
        // Entity Identity
        public Guid Id { get; private set; }
        
        // Adjustment Details
        public Guid UserId { get; private set; }
        public int PreviousBalance { get; private set; }
        public int AdjustmentAmount { get; private set; } // Can be negative
        public int NewBalance { get; private set; }
        public string Reason { get; private set; }
        
        // Audit
        public Guid AdminUserId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        
        // Navigation Properties
        public virtual User User { get; private set; }
        public virtual User AdminUser { get; private set; }
        
        // Factory Method
        public static PointAdjustment Create(
            Guid userId,
            int previousBalance,
            int adjustmentAmount,
            string reason,
            Guid adminUserId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Reason is required", nameof(reason));
            if (previousBalance + adjustmentAmount < 0)
                throw new ArgumentException("Adjustment would result in negative balance", nameof(adjustmentAmount));
                
            return new PointAdjustment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PreviousBalance = previousBalance,
                AdjustmentAmount = adjustmentAmount,
                NewBalance = previousBalance + adjustmentAmount,
                Reason = reason,
                AdminUserId = adminUserId,
                CreatedAt = DateTime.UtcNow
            };
        }
        
        // Private constructor
        private PointAdjustment() { }
    }
}
```

**Database Schema**:
```sql
CREATE TABLE PointAdjustments (
    Id UUID PRIMARY KEY,
    UserId UUID NOT NULL REFERENCES AspNetUsers(Id),
    PreviousBalance INTEGER NOT NULL,
    AdjustmentAmount INTEGER NOT NULL,
    NewBalance INTEGER NOT NULL,
    Reason VARCHAR(500) NOT NULL,
    AdminUserId UUID NOT NULL REFERENCES AspNetUsers(Id),
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IX_PointAdjustments_UserId ON PointAdjustments(UserId);
CREATE INDEX IX_PointAdjustments_AdminUserId ON PointAdjustments(AdminUserId);
```

---

## Value Objects

### 1. AdminDashboardStats

**Purpose**: Aggregate statistics for dashboard display.

```csharp
namespace WahadiniCryptoQuest.Core.ValueObjects
{
    public class AdminDashboardStats
    {
        public int TotalUsers { get; set; }
        public int ActiveSubscribers { get; set; }
        public decimal MonthlyRecurringRevenue { get; set; }
        public int PendingTasks { get; set; }
        public List<DataPoint> RevenueTrend { get; set; }
        public List<DataPoint> UserGrowthTrend { get; set; }
    }
    
    public class DataPoint
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
    }
}
```

### 2. UserSummary

**Purpose**: Condensed user information for admin list views.

```csharp
namespace WahadiniCryptoQuest.Core.ValueObjects
{
    public class UserSummary
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string SubscriptionTier { get; set; }
        public string AccountStatus { get; set; } // Active, Banned, PendingVerification
        public DateTime SignupDate { get; set; }
        public int PointsBalance { get; set; }
    }
}
```

---

## Extended Entities (Modifications to Existing)

### User Entity Extensions

**Add Properties**:
```csharp
public partial class User
{
    // Admin-specific properties
    public bool IsBanned { get; set; }
    public DateTime? BannedAt { get; set; }
    public Guid? BannedByAdminId { get; set; }
    public string BanReason { get; set; }
    
    // Navigation
    public virtual User BannedByAdmin { get; set; }
}
```

### TaskSubmission Entity (Already Exists)

**Expected Properties for Admin Review**:
```csharp
public class TaskSubmission
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TaskId { get; set; }
    public string Status { get; set; } // Pending, Approved, Rejected
    public string ContentType { get; set; } // Text, Screenshot, Quiz
    public string SubmissionData { get; set; } // JSON
    public string AdminFeedback { get; set; }
    public Guid? ReviewedByAdminId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime SubmittedAt { get; set; }
}
```

---

## Materialized Views

### AdminDashboardStatsView

**Purpose**: Pre-calculated dashboard metrics for performance.

```sql
CREATE MATERIALIZED VIEW admin_dashboard_stats_view AS
SELECT 
    (SELECT COUNT(*) FROM AspNetUsers WHERE IsDeleted = false) as total_users,
    (SELECT COUNT(*) FROM AspNetUsers WHERE SubscriptionTier != 'Free' AND IsDeleted = false) as active_subscribers,
    (SELECT COALESCE(SUM(Amount), 0) 
     FROM Subscriptions 
     WHERE Status = 'Active' 
     AND BillingCycle = 'Monthly') as monthly_recurring_revenue,
    (SELECT COUNT(*) FROM TaskSubmissions WHERE Status = 'Pending') as pending_tasks,
    CURRENT_TIMESTAMP as last_updated;

CREATE UNIQUE INDEX ON admin_dashboard_stats_view (last_updated);

-- Refresh every 5 minutes via scheduled job
```

---

## Relationships

```
User (1) ----< (N) AuditLogEntry (as AdminUser)
User (1) ----< (N) UserNotification
User (1) ----< (N) PointAdjustment (as User)
User (1) ----< (N) PointAdjustment (as AdminUser)
User (1) ----< (N) DiscountCode (as CreatedByAdmin)
User (1) ----< (N) TaskSubmission (as User)
User (1) ----< (N) TaskSubmission (as ReviewedByAdmin)

DiscountCode (1) ----< (N) DiscountRedemption
```

---

## Migration Strategy

**Phase 1: Schema Creation**
1. Create AuditLogs table
2. Create UserNotifications table
3. Create PointAdjustments table
4. Extend Users table (IsBanned, BannedAt, BannedByAdminId, BanReason)
5. Extend TaskSubmissions table (AdminFeedback, ReviewedByAdminId, ReviewedAt)

**Phase 2: Indexes**
1. Create performance indexes on new tables
2. Create composite indexes for common query patterns

**Phase 3: Materialized Views**
1. Create admin_dashboard_stats_view
2. Set up refresh schedule

**Migration Script**:
```sql
-- 009-admin-dashboard-schema.sql
BEGIN;

-- AuditLogs table
CREATE TABLE IF NOT EXISTS AuditLogs (
    -- schema as defined above
);

-- UserNotifications table
CREATE TABLE IF NOT EXISTS UserNotifications (
    -- schema as defined above
);

-- PointAdjustments table
CREATE TABLE IF NOT EXISTS PointAdjustments (
    -- schema as defined above
);

-- Extend Users table
ALTER TABLE AspNetUsers 
ADD COLUMN IF NOT EXISTS IsBanned BOOLEAN DEFAULT FALSE,
ADD COLUMN IF NOT EXISTS BannedAt TIMESTAMP,
ADD COLUMN IF NOT EXISTS BannedByAdminId UUID REFERENCES AspNetUsers(Id),
ADD COLUMN IF NOT EXISTS BanReason VARCHAR(500);

-- Extend TaskSubmissions table
ALTER TABLE TaskSubmissions
ADD COLUMN IF NOT EXISTS AdminFeedback VARCHAR(1000),
ADD COLUMN IF NOT EXISTS ReviewedByAdminId UUID REFERENCES AspNetUsers(Id),
ADD COLUMN IF NOT EXISTS ReviewedAt TIMESTAMP;

-- Create indexes
CREATE INDEX IF NOT EXISTS IX_AuditLogs_AdminUserId ON AuditLogs(AdminUserId);
CREATE INDEX IF NOT EXISTS IX_UserNotifications_UserId_IsRead ON UserNotifications(UserId, IsRead);
CREATE INDEX IF NOT EXISTS IX_PointAdjustments_UserId ON PointAdjustments(UserId);

COMMIT;
```

---

## Summary

**New Entities**: 3 (AuditLogEntry, UserNotification, PointAdjustment)  
**Extended Entities**: 2 (User, TaskSubmission)  
**Value Objects**: 2 (AdminDashboardStats, UserSummary)  
**Materialized Views**: 1 (admin_dashboard_stats_view)

All entities follow WahadiniCryptoQuest domain patterns:
- ✅ Factory methods for creation
- ✅ Private setters for encapsulation
- ✅ Domain validation in entity methods
- ✅ BaseEntity inheritance for common properties
- ✅ Soft delete support
- ✅ Audit timestamps
