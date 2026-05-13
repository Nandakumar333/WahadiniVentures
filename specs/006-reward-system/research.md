# Research: Reward Points, Leaderboard & Achievements

**Feature**: 006-reward-system  
**Date**: 2025-12-04  
**Status**: Complete

## Overview

This document consolidates research findings and design decisions for implementing the gamification system in WahadiniCryptoQuest Platform. All technical decisions align with Clean Architecture principles and the existing tech stack.

---

## 1. Points Transaction Architecture

### Decision: Event-Sourced Immutable Ledger

**Rationale**:
- Provides complete audit trail for compliance and dispute resolution
- Enables balance recalculation from source of truth
- Supports temporal queries (balance at any point in time)
- Prevents data corruption through append-only operations
- Aligns with financial system best practices

**Implementation Pattern**:
- `RewardTransaction` entity as append-only ledger
- User balance as denormalized aggregate (performance optimization)
- Database transactions for atomic updates (ledger + balance)
- Periodic reconciliation jobs for data integrity verification

**Alternatives Considered**:
- **Direct balance updates only**: Rejected due to lack of auditability and inability to reconstruct transaction history
- **Separate audit log**: Rejected as it duplicates transaction data and creates synchronization risks
- **Blockchain-based**: Rejected due to excessive complexity and performance overhead for this use case

---

## 2. Concurrency Control Strategy

### Decision: Database Transactions with Optimistic Concurrency

**Rationale**:
- Prevents race conditions during simultaneous point operations
- Lightweight compared to pessimistic locking (row-level locks)
- PostgreSQL ACID guarantees ensure consistency
- Entity Framework Core native support via RowVersion/Timestamp
- Scales well under moderate contention

**Implementation Pattern**:
```csharp
// In RewardService.AwardPoints
using var transaction = await _dbContext.Database.BeginTransactionAsync();
try {
    var ledgerEntry = new RewardTransaction { /* ... */ };
    await _dbContext.RewardTransactions.AddAsync(ledgerEntry);
    
    var user = await _dbContext.Users.FindAsync(userId);
    user.CurrentPoints += amount; // RowVersion checks for conflicts
    user.TotalPointsEarned += amount;
    
    await _dbContext.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch (DbUpdateConcurrencyException) {
    await transaction.RollbackAsync();
    // Retry logic or return error
}
```

**Alternatives Considered**:
- **Pessimistic locking (SELECT FOR UPDATE)**: Rejected due to reduced throughput and potential deadlocks
- **Distributed locks (Redis)**: Rejected as overkill for single-database operations
- **Queue-based processing**: Rejected due to added complexity and latency for real-time point displays

---

## 3. Leaderboard Caching Strategy

### Decision: Multi-Tier Caching with IMemoryCache

**Rationale**:
- IMemoryCache provides low-latency in-process caching (sub-millisecond access)
- 15-minute cache TTL balances freshness with database load
- Separate cache keys per leaderboard type (weekly/monthly/all-time)
- Personal rank calculated on-demand for accuracy
- Preparation for Redis migration in future (same interface pattern)

**Implementation Pattern**:
```csharp
public async Task<LeaderboardDto> GetLeaderboard(LeaderboardType type)
{
    var cacheKey = $"Leaderboard_{type}_{DateTime.UtcNow:yyyyMMddHHmm}";
    
    if (!_cache.TryGetValue(cacheKey, out LeaderboardDto leaderboard))
    {
        leaderboard = await _dbContext.Users
            .OrderByDescending(u => GetPointsForPeriod(u, type))
            .Take(100)
            .ProjectTo<LeaderboardEntryDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
            
        _cache.Set(cacheKey, leaderboard, TimeSpan.FromMinutes(15));
    }
    
    return leaderboard;
}
```

**Cache Invalidation Strategy**:
- Time-based expiration (15 minutes for top 100)
- No manual invalidation on point awards (eventual consistency acceptable)
- Personal rank queries bypass cache (always fresh)

**Alternatives Considered**:
- **Redis distributed cache**: Deferred to future; IMemoryCache sufficient for MVP single-server deployment
- **Database materialized views**: Rejected due to PostgreSQL refresh overhead and complexity
- **No caching**: Rejected due to excessive database load for high-traffic endpoint

---

## 4. Daily Streak Timezone Handling

### Decision: UTC-Based Streak Calculation

**Rationale**:
- Eliminates timezone manipulation exploits (traveling across zones)
- Simplifies server-side logic (single timezone for all users)
- Industry standard for global applications (Discord, Duolingo use UTC)
- Frontend can display local time for user convenience
- Database `DATE` type stores UTC calendar day

**Implementation Pattern**:
```csharp
public async Task ProcessLogin(Guid userId)
{
    var streak = await _dbContext.UserStreaks.FindAsync(userId) 
                 ?? new UserStreak { UserId = userId };
    
    var today = DateTime.UtcNow.Date;
    var daysSinceLastLogin = (today - streak.LastLoginDate).Days;
    
    if (daysSinceLastLogin == 1) {
        streak.CurrentStreak++;
        await AwardStreakBonus(userId, streak.CurrentStreak);
    }
    else if (daysSinceLastLogin > 1) {
        streak.CurrentStreak = 1;
        await AwardStreakBonus(userId, 1);
    }
    // daysSinceLastLogin == 0: Already logged in today, no-op
    
    streak.LastLoginDate = today;
    streak.LongestStreak = Math.Max(streak.LongestStreak, streak.CurrentStreak);
    await _dbContext.SaveChangesAsync();
}
```

**User Communication**:
- Display "Login before XX:XX local time to maintain streak" in UI
- Calculate XX:XX as UTC midnight converted to user's local timezone
- Show countdown timer for urgency

**Alternatives Considered**:
- **User-specific timezones**: Rejected due to complexity and gaming potential
- **Server local time**: Rejected due to inconsistency across deployments and DST issues

---

## 5. Achievement System Architecture

### Decision: Hardcoded Definitions with Database State Tracking

**Rationale**:
- Achievement criteria defined in code (strongly typed, version controlled)
- User unlock state persisted in database (UserAchievements junction table)
- Enables retroactive achievement awards through background jobs
- Supports achievement updates without database schema changes
- Clear separation of definition (code) vs. state (data)

**Achievement Definition Pattern**:
```csharp
public static class AchievementDefinitions
{
    public static readonly Achievement FirstSteps = new()
    {
        Id = "FIRST_STEPS",
        Name = "First Steps",
        Description = "Complete your first lesson",
        Criteria = user => user.CompletedLessons >= 1,
        Points = 10,
        Icon = "🎓"
    };
    
    public static readonly Achievement CourseMaster = new()
    {
        Id = "COURSE_MASTER",
        Name = "Course Master",
        Description = "Complete 5 courses",
        Criteria = user => user.CompletedCourses >= 5,
        Points = 100,
        Icon = "🏆"
    };
    
    // Additional achievements...
}
```

**Evaluation Strategy**:
- Event-driven checks on relevant actions (lesson complete → check FirstSteps)
- Batch evaluation for retroactive awards (background job scans all users)
- Idempotent unlock operations (UserAchievement table enforces uniqueness)

**Alternatives Considered**:
- **Database-driven achievement definitions**: Rejected due to complexity of storing criteria/logic in DB
- **External rules engine**: Rejected as overkill for relatively simple milestone checks
- **No retroactive support**: Rejected to support future achievement additions without user disadvantage

---

## 6. Duplicate Prevention Strategy

### Decision: Composite Unique Indexes + Application-Level Checks

**Rationale**:
- Database constraints provide ultimate guarantee against duplicates
- Application checks provide early validation and better error messages
- Covers lesson completion, task submission, referral attribution
- Zero-trust approach (defense in depth)

**Implementation Pattern**:
```sql
-- Database constraint
CREATE UNIQUE INDEX IX_UserProgress_UserId_LessonId 
ON UserProgress (UserId, LessonId) WHERE CompletedAt IS NOT NULL;

-- Application check
var existingProgress = await _dbContext.UserProgress
    .AnyAsync(up => up.UserId == userId && up.LessonId == lessonId && up.CompletedAt != null);
    
if (existingProgress) {
    return Result.Failure("Lesson already completed");
}
```

**Covered Scenarios**:
- Lesson completion: UserProgress(UserId, LessonId) unique constraint
- Task submission: TaskSubmission(UserId, TaskId, Status=Approved) unique constraint
- Referral attribution: ReferralAttribution(InviteeId) unique constraint

**Alternatives Considered**:
- **Application-only checks**: Rejected due to race condition vulnerabilities
- **Distributed locks**: Rejected as overkill when database constraints suffice

---

## 7. Referral Tracking Architecture

### Decision: Code-Based Referral System with Attribution Tracking

**Rationale**:
- Each user assigned unique referral code on registration
- Code persisted in User.ReferralCode (indexed for lookup performance)
- ReferralAttribution table tracks inviter-invitee relationships
- Bonus awarded on first course completion (verified state transition)
- Prevents self-referral and circular attribution

**Implementation Pattern**:
```csharp
// Registration
var newUser = new User
{
    ReferralCode = GenerateUniqueCode(), // 8-char alphanumeric
    // ...
};

// Signup with referral
if (!string.IsNullOrEmpty(referralCode))
{
    var referrer = await _dbContext.Users
        .FirstOrDefaultAsync(u => u.ReferralCode == referralCode);
        
    if (referrer != null && referrer.Id != newUser.Id)
    {
        var attribution = new ReferralAttribution
        {
            InviterId = referrer.Id,
            InviteeId = newUser.Id,
            Code = referralCode,
            Status = ReferralStatus.Pending
        };
        await _dbContext.ReferralAttributions.AddAsync(attribution);
    }
}

// On first course completion
var attribution = await _dbContext.ReferralAttributions
    .FirstOrDefaultAsync(ra => ra.InviteeId == userId && ra.Status == ReferralStatus.Pending);
    
if (attribution != null)
{
    await _rewardService.AwardPoints(
        attribution.InviterId, 
        50, // Configurable referral bonus
        TransactionType.Referral,
        attribution.Id
    );
    
    attribution.Status = ReferralStatus.Fulfilled;
    await _dbContext.SaveChangesAsync();
}
```

**Alternatives Considered**:
- **Link-based tracking**: Rejected due to complexity of link generation and validation
- **Cookie-based tracking**: Rejected due to privacy concerns and browser limitations
- **No attribution**: Rejected as it fails to meet FR-008 (referral rewards)

---

## 8. API Response Pagination

### Decision: Cursor-Based Pagination for Transaction History

**Rationale**:
- Transaction history grows unbounded (append-only ledger)
- Cursor pagination prevents missed/duplicate records during insertions
- Better performance than offset pagination for large datasets
- Uses CreatedAt + Id composite cursor for total ordering

**Implementation Pattern**:
```csharp
public async Task<PaginatedResult<RewardTransactionDto>> GetTransactionHistory(
    Guid userId, 
    string? cursor = null, 
    int pageSize = 20)
{
    var query = _dbContext.RewardTransactions
        .Where(rt => rt.UserId == userId)
        .OrderByDescending(rt => rt.CreatedAt)
        .ThenByDescending(rt => rt.Id);
        
    if (!string.IsNullOrEmpty(cursor))
    {
        var (timestamp, id) = ParseCursor(cursor);
        query = query.Where(rt => 
            rt.CreatedAt < timestamp || 
            (rt.CreatedAt == timestamp && rt.Id < id)
        );
    }
    
    var transactions = await query
        .Take(pageSize + 1)
        .ProjectTo<RewardTransactionDto>(_mapper.ConfigurationProvider)
        .ToListAsync();
        
    var hasMore = transactions.Count > pageSize;
    if (hasMore) transactions.RemoveAt(pageSize);
    
    var nextCursor = hasMore 
        ? EncodeCursor(transactions.Last().CreatedAt, transactions.Last().Id)
        : null;
        
    return new PaginatedResult<RewardTransactionDto>
    {
        Items = transactions,
        NextCursor = nextCursor,
        HasMore = hasMore
    };
}
```

**Alternatives Considered**:
- **Offset pagination**: Rejected due to performance degradation on large offsets and consistency issues
- **Page number pagination**: Rejected for same reasons as offset pagination

---

## 9. Point Value Configuration

### Decision: Database-Backed Configuration with In-Memory Caching

**Rationale**:
- Point values change infrequently but need runtime updates without deployment
- Configuration table stores key-value pairs (e.g., "LessonCompletionPoints" = 50)
- IMemoryCache reduces database hits for frequent reads
- Admin UI for point value management (future scope)

**Implementation Pattern**:
```csharp
public class RewardConfiguration
{
    private readonly IMemoryCache _cache;
    private readonly AppDbContext _dbContext;
    
    public async Task<int> GetPointValue(string key)
    {
        if (!_cache.TryGetValue(key, out int value))
        {
            var config = await _dbContext.RewardConfigurations
                .FirstOrDefaultAsync(rc => rc.Key == key);
                
            value = config?.IntValue ?? GetDefaultValue(key);
            _cache.Set(key, value, TimeSpan.FromHours(1));
        }
        
        return value;
    }
}
```

**Default Values** (fallback if DB config missing):
- Lesson completion: 50 points
- Task approval: 100 points
- Course completion: 500 points
- Daily streak: 5 points + (streak * 2) bonus
- Referral: 250 points

**Alternatives Considered**:
- **Hardcoded values**: Rejected due to inflexibility and deployment overhead for changes
- **Environment variables**: Rejected due to poor auditability and difficult historical tracking
- **External configuration service**: Rejected as overkill for simple key-value pairs

---

## 10. Testing Strategy

### Unit Tests
- Point calculation logic (isolated from database)
- Streak increment/reset rules (date boundary conditions)
- Achievement criteria evaluation (threshold checks)
- Duplicate prevention logic

### Integration Tests
- End-to-end point award flows (lesson → transaction → balance)
- Leaderboard cache behavior (cache hits, expiration, freshness)
- Concurrent point operations (race condition verification)
- Referral attribution workflow (signup → completion → reward)

### Performance Tests
- Leaderboard query performance with 100K+ users
- Transaction history pagination with 10K+ transactions
- Concurrent point awards (100 simultaneous operations)

---

## Technology Alignment

All design decisions align with the WahadiniCryptoQuest tech stack:

- **.NET 8 C# + ASP.NET Core**: All services and controllers
- **Entity Framework Core 8.0**: Database access with optimistic concurrency
- **PostgreSQL 15+**: Leveraging JSONB for flexible achievement metadata
- **AutoMapper**: Entity-to-DTO mapping throughout
- **FluentValidation**: Request validation in API layer
- **MediatR**: CQRS commands/queries for reward operations
- **IMemoryCache**: Built-in ASP.NET Core caching (future Redis migration path)

---

## Open Items

None. All NEEDS CLARIFICATION markers from spec resolved:
- ✅ Timezone handling: UTC-based
- ✅ Duplicate prevention: Composite indexes + app checks
- ✅ Leaderboard refresh: 15-minute cache TTL
- ✅ Negative balance: Prevented via validation
- ✅ Retroactive achievements: Supported via batch jobs
- ✅ Race conditions: Database transactions + optimistic concurrency

---

**Status**: All research complete. Proceed to Phase 1 (Design & Contracts).
