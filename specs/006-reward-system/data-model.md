# Data Model: Reward Points, Leaderboard & Achievements

**Feature**: 006-reward-system  
**Date**: 2025-12-04  
**Status**: Complete

## Overview

This document defines the domain entities, relationships, validation rules, and state transitions for the reward gamification system following Clean Architecture and Domain-Driven Design principles.

---

## Domain Entities

### 1. RewardTransaction (Immutable Ledger)

**Purpose**: Append-only audit trail of all point changes

**Properties**:
- `Id` (Guid, Primary Key) - Unique transaction identifier
- `UserId` (Guid, Foreign Key → User, Required, Indexed) - User who received/spent points
- `Amount` (int, Required) - Point delta (positive = earn, negative = spend/penalty)
- `TransactionType` (enum, Required) - Category of transaction
- `ReferenceId` (Guid?, Optional) - Reference to source entity (LessonId, TaskId, etc.)
- `Description` (string, Optional, Max 500 chars) - Human-readable transaction context
- `AdminUserId` (Guid?, Optional, Foreign Key → User) - Admin who created manual adjustment (Bonus/Penalty only)
- `CreatedAt` (DateTime, Required, Indexed) - UTC timestamp (immutable)
- `BalanceAfter` (int, Required) - Snapshot of user's balance after this transaction (denormalized for auditing)

**Transaction Types** (Enum):
- `LessonCompletion`
- `TaskApproval`
- `CourseCompletion`
- `DailyStreak`
- `ReferralBonus`
- `AdminBonus`
- `AdminPenalty`
- `Redemption` (future: discount purchases)

**Relationships**:
- Many-to-One with `User` (navigation property: `User`)

**Business Rules**:
- Once created, transactions CANNOT be modified or deleted (immutability)
- Amount can be negative (redemptions, penalties) but User balance must never go below zero
- CreatedAt defaults to `DateTime.UtcNow` on creation
- TransactionType determines whether ReferenceId is required

**Indexes**:
- `IX_RewardTransaction_UserId_CreatedAt` (composite, DESC) for efficient history queries
- `IX_RewardTransaction_TransactionType` for analytics

**Entity Implementation Pattern**:
```csharp
public class RewardTransaction : BaseEntity
{
    private RewardTransaction() { } // EF Core
    
    public static RewardTransaction Create(
        Guid userId,
        int amount,
        TransactionType type,
        Guid? referenceId = null,
        string? description = null,
        int balanceAfter = 0)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId is required");
            
        if (amount == 0)
            throw new DomainException("Amount cannot be zero");
            
        return new RewardTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            TransactionType = type,
            ReferenceId = referenceId,
            Description = description,
            BalanceAfter = balanceAfter,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public Guid UserId { get; private set; }
    public int Amount { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public string? Description { get; private set; }
    public int BalanceAfter { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    // Navigation
    public User User { get; private set; } = null!;
}
```

---

### 2. User (Extended from Existing)

**Purpose**: Aggregate root for user data with reward point balances

**New Properties** (added to existing User entity):
- `CurrentPoints` (int, Required, Default 0, Indexed) - Current redeemable point balance
- `TotalPointsEarned` (int, Required, Default 0, Indexed) - Lifetime points earned (never decreases)
- `ReferralCode` (string, Required, Unique, Indexed, Max 12 chars) - User's unique referral code

**Existing Properties** (relevant to rewards):
- `Id` (Guid, Primary Key)
- `Username` (string, Required, Unique)
- `Email` (string, Required, Unique)
- `CreatedAt` (DateTime, Required)

**Relationships**:
- One-to-Many with `RewardTransaction` (navigation: `Transactions`)
- One-to-Many with `UserAchievement` (navigation: `Achievements`)
- One-to-One with `UserStreak` (navigation: `Streak`)
- One-to-Many with `ReferralAttribution` as Inviter (navigation: `ReferralsMade`)
- One-to-Many with `ReferralAttribution` as Invitee (navigation: `ReferralsReceived`)

**Business Rules**:
- CurrentPoints must never be negative (validated before redemptions)
- TotalPointsEarned only increases (monotonic)
- ReferralCode generated on user registration (8 alphanumeric characters)
- Points updated atomically with transaction creation (database transaction)

**Domain Methods**:
```csharp
public class User : BaseEntity
{
    // ... existing properties ...
    
    public int CurrentPoints { get; private set; }
    public int TotalPointsEarned { get; private set; }
    public string ReferralCode { get; private set; } = null!;
    
    public void AwardPoints(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Award amount must be positive");
            
        CurrentPoints += amount;
        TotalPointsEarned += amount;
    }
    
    public void DeductPoints(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Deduction amount must be positive");
            
        if (CurrentPoints < amount)
            throw new DomainException("Insufficient points");
            
        CurrentPoints -= amount;
    }
    
    public void AdjustPoints(int delta, string reason)
    {
        if (delta > 0)
            AwardPoints(delta);
        else if (delta < 0)
            DeductPoints(Math.Abs(delta));
    }
    
    // Navigation properties
    public ICollection<RewardTransaction> Transactions { get; private set; } = new List<RewardTransaction>();
    public ICollection<UserAchievement> Achievements { get; private set; } = new List<UserAchievement>();
    public UserStreak? Streak { get; private set; }
}
```

**Indexes**:
- `IX_User_CurrentPoints_DESC` for leaderboard queries
- `IX_User_TotalPointsEarned_DESC` for all-time leaderboard
- `IX_User_ReferralCode` (unique) for referral lookups

---

### 3. UserStreak

**Purpose**: Track daily login streaks for bonus point awards

**Properties**:
- `UserId` (Guid, Primary Key, Foreign Key → User) - One streak record per user
- `CurrentStreak` (int, Required, Default 0) - Consecutive days logged in
- `LongestStreak` (int, Required, Default 0) - Personal best streak record
- `LastLoginDate` (DateTime, Required) - Date component only (UTC), used for streak calculation
- `UpdatedAt` (DateTime, Required) - Last modification timestamp

**Relationships**:
- One-to-One with `User` (navigation: `User`)

**Business Rules**:
- LastLoginDate stored as UTC date (time component ignored)
- CurrentStreak increments by 1 if login occurs next UTC day
- CurrentStreak resets to 1 if gap > 1 day
- LongestStreak updated if CurrentStreak exceeds it
- Multiple logins same UTC day = no-op

**State Transitions**:
```
┌─────────────────┐
│  No Streak (0)  │
└────────┬────────┘
         │ First login
         ▼
┌─────────────────┐
│   Streak: 1     │
└────────┬────────┘
         │ Next day login
         ▼
┌─────────────────┐
│   Streak: N     │◄──┐
└────────┬────────┘   │ Consecutive login
         │            │
         ├────────────┘
         │ Missed day (gap > 1)
         ▼
┌─────────────────┐
│   Streak: 1     │ (Reset)
└─────────────────┘
```

**Domain Methods**:
```csharp
public class UserStreak : BaseEntity
{
    private UserStreak() { } // EF Core
    
    public static UserStreak Create(Guid userId)
    {
        return new UserStreak
        {
            UserId = userId,
            CurrentStreak = 0,
            LongestStreak = 0,
            LastLoginDate = DateTime.UtcNow.Date,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    public Guid UserId { get; private set; }
    public int CurrentStreak { get; private set; }
    public int LongestStreak { get; private set; }
    public DateTime LastLoginDate { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    public StreakUpdate ProcessLogin()
    {
        var today = DateTime.UtcNow.Date;
        var daysSinceLastLogin = (today - LastLoginDate).Days;
        
        int pointsAwarded = 0;
        bool streakIncreased = false;
        
        if (daysSinceLastLogin == 1)
        {
            CurrentStreak++;
            streakIncreased = true;
            pointsAwarded = CalculateStreakBonus(CurrentStreak);
        }
        else if (daysSinceLastLogin > 1)
        {
            CurrentStreak = 1;
            streakIncreased = true;
            pointsAwarded = CalculateStreakBonus(1);
        }
        // daysSinceLastLogin == 0: Already logged in today
        
        if (CurrentStreak > LongestStreak)
            LongestStreak = CurrentStreak;
            
        LastLoginDate = today;
        UpdatedAt = DateTime.UtcNow;
        
        return new StreakUpdate(CurrentStreak, pointsAwarded, streakIncreased);
    }
    
    private int CalculateStreakBonus(int streak)
    {
        int basePoints = 5;
        int bonusMultiplier = streak / 7; // Bonus every 7 days
        return basePoints + (bonusMultiplier * 10);
    }
    
    // Navigation
    public User User { get; private set; } = null!;
}

public record StreakUpdate(int CurrentStreak, int PointsAwarded, bool StreakChanged);
```

---

### 4. AchievementDefinition (Code-Only, Not in DB)

**Purpose**: Define achievement criteria and metadata in code

**Properties** (C# class, not entity):
- `Id` (string, Unique) - Achievement identifier (e.g., "FIRST_STEPS")
- `Name` (string) - Display name
- `Description` (string) - User-facing description
- `Criteria` (Func<User, bool>) - Evaluation function
- `PointBonus` (int) - Points awarded on unlock
- `Icon` (string) - Emoji or icon identifier
- `DisplayOrder` (int) - Sorting order in UI

**Achievement Catalog** (Static):
```csharp
public static class AchievementCatalog
{
    public static readonly List<AchievementDefinition> All = new()
    {
        new AchievementDefinition
        {
            Id = "FIRST_STEPS",
            Name = "First Steps",
            Description = "Complete your first lesson",
            Criteria = user => user.CompletedLessons >= 1,
            PointBonus = 10,
            Icon = "🎓",
            DisplayOrder = 1
        },
        new AchievementDefinition
        {
            Id = "SPEED_LEARNER",
            Name = "Speed Learner",
            Description = "Complete 3 lessons in one day",
            Criteria = user => user.LessonsCompletedToday >= 3,
            PointBonus = 25,
            Icon = "⚡",
            DisplayOrder = 2
        },
        new AchievementDefinition
        {
            Id = "COURSE_MASTER",
            Name = "Course Master",
            Description = "Complete your first course",
            Criteria = user => user.CompletedCourses >= 1,
            PointBonus = 50,
            Icon = "🏆",
            DisplayOrder = 3
        },
        new AchievementDefinition
        {
            Id = "DEDICATED_STUDENT",
            Name = "Dedicated Student",
            Description = "Maintain a 7-day login streak",
            Criteria = user => user.Streak?.CurrentStreak >= 7,
            PointBonus = 75,
            Icon = "🔥",
            DisplayOrder = 4
        },
        new AchievementDefinition
        {
            Id = "POINT_HOARDER",
            Name = "Point Hoarder",
            Description = "Accumulate 1,000 total points",
            Criteria = user => user.TotalPointsEarned >= 1000,
            PointBonus = 100,
            Icon = "💎",
            DisplayOrder = 5
        },
        new AchievementDefinition
        {
            Id = "REFERRAL_CHAMPION",
            Name = "Referral Champion",
            Description = "Refer 5 friends who complete a course",
            Criteria = user => user.SuccessfulReferrals >= 5,
            PointBonus = 200,
            Icon = "🤝",
            DisplayOrder = 6
        }
    };
}
```

---

### 5. UserAchievement

**Purpose**: Track user-specific achievement unlock state

**Properties**:
- `UserId` (Guid, Composite Primary Key, Foreign Key → User)
- `AchievementId` (string, Composite Primary Key) - References AchievementDefinition.Id
- `UnlockedAt` (DateTime, Required) - UTC timestamp of unlock
- `Notified` (bool, Default false) - Whether user has seen unlock notification

**Relationships**:
- Many-to-One with `User` (navigation: `User`)

**Business Rules**:
- Composite key prevents duplicate unlocks
- AchievementId must exist in AchievementCatalog (validated in application layer)
- UnlockedAt immutable after creation
- Notified flag used for UI badge display logic

**Entity Implementation**:
```csharp
public class UserAchievement : BaseEntity
{
    private UserAchievement() { } // EF Core
    
    public static UserAchievement Create(Guid userId, string achievementId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId is required");
            
        if (string.IsNullOrWhiteSpace(achievementId))
            throw new DomainException("AchievementId is required");
            
        return new UserAchievement
        {
            UserId = userId,
            AchievementId = achievementId,
            UnlockedAt = DateTime.UtcNow,
            Notified = false
        };
    }
    
    public Guid UserId { get; private set; }
    public string AchievementId { get; private set; } = null!;
    public DateTime UnlockedAt { get; private set; }
    public bool Notified { get; private set; }
    
    public void MarkAsNotified()
    {
        Notified = true;
    }
    
    // Navigation
    public User User { get; private set; } = null!;
}
```

**Indexes**:
- Primary Key: `PK_UserAchievement_UserId_AchievementId`
- `IX_UserAchievement_UnlockedAt` for recent unlock queries

---

### 6. ReferralAttribution

**Purpose**: Track referral relationships and reward fulfillment

**Properties**:
- `Id` (Guid, Primary Key)
- `InviterId` (Guid, Foreign Key → User, Required, Indexed) - User who referred
- `InviteeId` (Guid, Foreign Key → User, Required, Unique) - User who was referred (one referral per invitee)
- `ReferralCode` (string, Required, Indexed) - Code used for attribution
- `Status` (enum, Required, Default Pending) - Referral fulfillment state
- `CreatedAt` (DateTime, Required) - When referral attribution occurred
- `FulfilledAt` (DateTime?, Optional) - When referral bonus was awarded

**Referral Status** (Enum):
- `Pending` - Invitee registered but hasn't completed qualifying action
- `Fulfilled` - Qualifying action completed, bonus awarded
- `Expired` - Referral expired without fulfillment (future: time limit)
- `Canceled` - Invitee account deleted or flagged

**Relationships**:
- Many-to-One with `User` as Inviter (navigation: `Inviter`)
- Many-to-One with `User` as Invitee (navigation: `Invitee`)

**Business Rules**:
- InviteeId must be unique (user can only be referred once)
- InviterId ≠ InviteeId (self-referral prevention)
- Status transition: Pending → Fulfilled (one-way)
- FulfilledAt set when status changes to Fulfilled
- Bonus awarded when invitee completes first course

**State Transitions**:
```
┌──────────┐
│ Pending  │
└─────┬────┘
      │ Invitee completes first course
      ▼
┌──────────┐
│Fulfilled │
└──────────┘
```

**Entity Implementation**:
```csharp
public class ReferralAttribution : BaseEntity
{
    private ReferralAttribution() { } // EF Core
    
    public static ReferralAttribution Create(Guid inviterId, Guid inviteeId, string code)
    {
        if (inviterId == Guid.Empty || inviteeId == Guid.Empty)
            throw new DomainException("Valid InviterId and InviteeId required");
            
        if (inviterId == inviteeId)
            throw new DomainException("Self-referral not allowed");
            
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Referral code required");
            
        return new ReferralAttribution
        {
            Id = Guid.NewGuid(),
            InviterId = inviterId,
            InviteeId = inviteeId,
            ReferralCode = code,
            Status = ReferralStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public Guid InviterId { get; private set; }
    public Guid InviteeId { get; private set; }
    public string ReferralCode { get; private set; } = null!;
    public ReferralStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? FulfilledAt { get; private set; }
    
    public void MarkAsFulfilled()
    {
        if (Status != ReferralStatus.Pending)
            throw new DomainException("Can only fulfill pending referrals");
            
        Status = ReferralStatus.Fulfilled;
        FulfilledAt = DateTime.UtcNow;
    }
    
    // Navigation
    public User Inviter { get; private set; } = null!;
    public User Invitee { get; private set; } = null!;
}
```

**Indexes**:
- `IX_ReferralAttribution_InviteeId` (unique) to prevent duplicate referrals
- `IX_ReferralAttribution_InviterId_Status` for inviter dashboard queries
- `IX_ReferralAttribution_ReferralCode` for code lookup

---

### 7. LeaderboardSnapshot (Optional Future Enhancement)

**Purpose**: Materialized leaderboard data for weekly/monthly rankings

**Properties**:
- `Id` (Guid, Primary Key)
- `Period` (enum: Weekly, Monthly, AllTime)
- `StartDate` (DateTime, Required)
- `EndDate` (DateTime, Required)
- `Rankings` (JSON, Required) - Serialized leaderboard data
- `GeneratedAt` (DateTime, Required)

**Note**: For MVP, leaderboards calculated on-demand with caching. This entity supports future optimization if needed.

---

## Entity Relationships Diagram

```
┌──────────┐         ┌────────────────────┐
│   User   │◄────────┤ RewardTransaction  │
│          │ 1     * │                    │
│ +Points  │         │ +Amount            │
│ +Referral│         │ +Type              │
│  Code    │         │ +CreatedAt         │
└────┬─────┘         └────────────────────┘
     │
     │ 1                                1
     ├──────────────────────────────────┐
     │                                  │
     ▼ *                                ▼ 1
┌──────────────┐              ┌──────────────┐
│UserAchievement│              │ UserStreak   │
│              │              │              │
│+AchievementId│              │+CurrentStreak│
│+UnlockedAt   │              │+LastLoginDate│
└──────────────┘              └──────────────┘

     ┌─────────────────────┐
     │ReferralAttribution  │
     │                     │
     │Inviter ─────►  User │
     │Invitee ─────►  User │
     │+Status              │
     └─────────────────────┘
```

---

## Database Schema (PostgreSQL)

```sql
-- Reward Transactions (Immutable Ledger)
CREATE TABLE RewardTransactions (
    Id UUID PRIMARY KEY,
    UserId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    Amount INT NOT NULL,
    TransactionType VARCHAR(50) NOT NULL,
    ReferenceId UUID NULL,
    Description VARCHAR(500) NULL,
    BalanceAfter INT NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX IX_RewardTransactions_UserId_CreatedAt 
ON RewardTransactions (UserId, CreatedAt DESC);

CREATE INDEX IX_RewardTransactions_TransactionType 
ON RewardTransactions (TransactionType);

-- User Extensions (Add columns to existing Users table)
ALTER TABLE Users ADD COLUMN CurrentPoints INT NOT NULL DEFAULT 0;
ALTER TABLE Users ADD COLUMN TotalPointsEarned INT NOT NULL DEFAULT 0;
ALTER TABLE Users ADD COLUMN ReferralCode VARCHAR(12) NOT NULL DEFAULT '';

CREATE INDEX IX_Users_CurrentPoints ON Users (CurrentPoints DESC);
CREATE INDEX IX_Users_TotalPointsEarned ON Users (TotalPointsEarned DESC);
CREATE UNIQUE INDEX IX_Users_ReferralCode ON Users (ReferralCode);

-- User Streaks
CREATE TABLE UserStreaks (
    UserId UUID PRIMARY KEY REFERENCES Users(Id) ON DELETE CASCADE,
    CurrentStreak INT NOT NULL DEFAULT 0,
    LongestStreak INT NOT NULL DEFAULT 0,
    LastLoginDate DATE NOT NULL,
    UpdatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX IX_UserStreaks_CurrentStreak ON UserStreaks (CurrentStreak DESC);

-- User Achievements
CREATE TABLE UserAchievements (
    UserId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    AchievementId VARCHAR(50) NOT NULL,
    UnlockedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    Notified BOOLEAN NOT NULL DEFAULT FALSE,
    PRIMARY KEY (UserId, AchievementId)
);

CREATE INDEX IX_UserAchievements_UnlockedAt ON UserAchievements (UnlockedAt DESC);

-- Referral Attributions
CREATE TABLE ReferralAttributions (
    Id UUID PRIMARY KEY,
    InviterId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    InviteeId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    ReferralCode VARCHAR(12) NOT NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    FulfilledAt TIMESTAMP WITH TIME ZONE NULL,
    CONSTRAINT CHK_ReferralAttribution_NoSelfReferral CHECK (InviterId != InviteeId)
);

CREATE UNIQUE INDEX IX_ReferralAttributions_InviteeId 
ON ReferralAttributions (InviteeId);

CREATE INDEX IX_ReferralAttributions_InviterId_Status 
ON ReferralAttributions (InviterId, Status);

CREATE INDEX IX_ReferralAttributions_ReferralCode 
ON ReferralAttributions (ReferralCode);
```

---

## Validation Rules Summary

| Entity | Rule | Enforced By |
|--------|------|-------------|
| RewardTransaction | Amount ≠ 0 | Domain method |
| RewardTransaction | Amount within bounds [-10000, 10000] | Domain validation (NFR-010) |
| RewardTransaction | UserId exists | Foreign key constraint |
| RewardTransaction | Immutability | No update/delete operations |
| RewardTransaction | Description max 500 chars | Column constraint |
| User | CurrentPoints ≥ 0 | Domain method (DeductPoints) |
| User | TotalPointsEarned monotonic | Domain method (AwardPoints only) |
| User | ReferralCode unique | Unique index |
| User | ReferralCode format [A-Z0-9]{8} | Domain generation |
| UserStreak | LastLoginDate is UTC date | Domain method |
| UserStreak | CurrentStreak ≥ 0 | Domain method |
| UserStreak | LongestStreak ≥ CurrentStreak | Domain method |
| UserAchievement | AchievementId in catalog | Application layer |
| UserAchievement | No duplicates | Composite primary key |
| ReferralAttribution | InviterId ≠ InviteeId | Check constraint + domain |
| ReferralAttribution | InviteeId unique (one referral per user) | Unique index |
| ReferralAttribution | Status transitions valid (Pending→Fulfilled) | Domain method |
| ReferralAttribution | ReferralCode matches Inviter's code | Application validation |

---

## Database Constraints & Indexes

### Foreign Key Cascade Behaviors
- `RewardTransactions.UserId → Users.Id`: **NO ACTION** (preserve transaction history even if user deleted)
- `UserStreaks.UserId → Users.Id`: **CASCADE** (delete streak with user)
- `UserAchievements.UserId → Users.Id`: **CASCADE** (delete achievements with user)
- `ReferralAttributions.InviterId → Users.Id`: **CASCADE** (delete referrals with inviter)
- `ReferralAttributions.InviteeId → Users.Id`: **CASCADE** (delete referrals with invitee)

### Check Constraints
```sql
ALTER TABLE RewardTransactions 
ADD CONSTRAINT CHK_RewardTransaction_AmountBounds 
CHECK (Amount BETWEEN -10000 AND 10000);

ALTER TABLE Users 
ADD CONSTRAINT CHK_User_CurrentPointsNonNegative 
CHECK (CurrentPoints >= 0);

ALTER TABLE Users 
ADD CONSTRAINT CHK_User_TotalPointsEarnedNonNegative 
CHECK (TotalPointsEarned >= 0);

ALTER TABLE UserStreaks 
ADD CONSTRAINT CHK_UserStreak_StreaksValid 
CHECK (CurrentStreak >= 0 AND LongestStreak >= CurrentStreak);
```

### Composite Indexes for Performance
```sql
-- Leaderboard queries (all-time)
CREATE INDEX IX_Users_TotalPointsEarned_CreatedAt 
ON Users (TotalPointsEarned DESC, CreatedAt ASC);

-- Leaderboard queries (weekly/monthly via transaction aggregation)
CREATE INDEX IX_RewardTransactions_CreatedAt_Amount_UserId 
ON RewardTransactions (CreatedAt DESC, Amount, UserId) 
WHERE Amount > 0;

-- Transaction history pagination
CREATE INDEX IX_RewardTransactions_UserId_CreatedAt_Id 
ON RewardTransactions (UserId, CreatedAt DESC, Id);

-- Achievement unlock analytics
CREATE INDEX IX_UserAchievements_AchievementId_UnlockedAt 
ON UserAchievements (AchievementId, UnlockedAt DESC);
```

---

## Idempotency & Deduplication

**Strategy**: Prevent duplicate point awards for same action using unique constraint on deduplication key.

**Implementation**:
```sql
-- Deduplication for lesson/task/course completions
CREATE UNIQUE INDEX IX_RewardTransactions_Deduplication 
ON RewardTransactions (UserId, ReferenceId, TransactionType) 
WHERE ReferenceId IS NOT NULL 
  AND TransactionType IN ('LessonCompletion', 'TaskApproval', 'CourseCompletion');

-- Application layer checks deduplication before inserting
-- If duplicate key violation, return existing transaction (idempotent)
```

**Deduplication Window**: 24 hours (enforced in application layer by querying recent transactions)

---

## Concurrency Control

**Optimistic Locking on User Entity**:
```csharp
public class User : BaseEntity
{
    [Timestamp]
    public byte[] RowVersion { get; private set; } = null!;
    
    // Properties and methods...
}
```

**EF Core Configuration**:
```csharp
builder.Property(u => u.RowVersion)
    .IsRowVersion()
    .IsConcurrencyToken();
```

**Conflict Resolution**: Retry with exponential backoff (3 attempts: 10ms, 50ms, 100ms)

---

**Status**: Data model design complete with validation rules, constraints, indexes, idempotency, and concurrency control. Proceed to contract definitions.
