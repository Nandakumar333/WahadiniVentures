# Feature: Reward Points, Leaderboard & Achievements

## /speckit.specify

### Feature Overview
Implement a comprehensive gamification system to drive user engagement through reward points, competitive leaderboards, unlockable achievements, and daily login streaks. This system incentivizes learning activities, consistent participation, and platform growth through referrals.

### Feature Scope
**Included:**
- **Points Engine:** Flexible point awarding for lessons, tasks, courses, and referrals.
- **Transaction Ledger:** Immutable record of all point changes (earnings, redemptions).
- **Leaderboards:** Global, Monthly, and Weekly rankings with caching.
- **Achievements:** System to track and award badges for milestones.
- **Streak Tracking:** Daily login tracking with escalating bonuses.
- **Frontend Components:** Balance display, history, leaderboard UI, achievement showcase.
- **Referral Tracking:** Basic referral code generation and attribution.

**Excluded:**
- **Point Redemption/Store:** (Handled in 007-discount-system).
- **Complex Anti-Fraud:** (Basic rate limiting only for MVP).
- **Social Sharing:** (Sharing achievements to social media is future scope).
- **Custom Avatar/Profile Customization:** (Future scope).

### User Stories
1. **As a user**, I want to earn points for completing lessons and tasks so that I feel rewarded for my progress.
2. **As a user**, I want to see my current point balance and transaction history to track my earnings.
3. **As a user**, I want to see where I rank on the leaderboard (weekly/monthly/all-time) to compete with others.
4. **As a user**, I want to earn badges/achievements for reaching milestones to showcase my expertise.
5. **As a user**, I want to maintain a daily login streak to earn bonus points.
6. **As a user**, I want to earn points for referring friends who complete a course.
7. **As an admin**, I want to view a user's point history to resolve disputes.
8. **As an admin**, I want to manually adjust points (bonus/penalty) if necessary.
9. **As a system**, I want to cache leaderboard data to ensure fast loading times.

### Technical Requirements
- **Backend:** .NET 8 C#, ASP.NET Core Web API.
- **Database:** PostgreSQL (Transactions, Achievements, Streaks).
- **Caching:** IMemoryCache (or Redis in future) for leaderboards.
- **Frontend:** React 18, TypeScript, TailwindCSS.
- **State Management:** React Query for data fetching.
- **Concurrency:** Optimistic concurrency control for point balances.

---

## /speckit.plan

### Implementation Plan

#### Phase 1: Database Schema & Entities (6 hours)
**Tasks:**
1. Create `RewardTransaction` entity (Immutable ledger).
2. Create `UserAchievement` and `Achievement` entities.
3. Create `UserStreak` entity.
4. Update `User` entity with `CurrentPoints` and `TotalPointsEarned`.
5. Create EF Core migrations and apply.

**Deliverables:**
- Entity classes and EF Core configurations.
- Database migration script.
- Updated database schema.

#### Phase 2: Core Reward Services (10 hours)
**Tasks:**
1. Implement `RewardService` (Award, Deduct, GetBalance).
2. Implement `TransactionService` (Log transactions).
3. Implement `StreakService` (Check/Update streak on login).
4. Implement `AchievementService` (Check rules, Unlock).
5. Implement `LeaderboardService` with caching strategies.

**Deliverables:**
- `IRewardService`, `IAchievementService`, `IStreakService`.
- Unit tests for point calculations and streak logic.

#### Phase 3: API Endpoints (8 hours)
**Tasks:**
1. Create `RewardsController` (Balance, Transactions).
2. Create `LeaderboardController` (Top users, My rank).
3. Create `AchievementsController` (List, My achievements).
4. Integrate Streak check into Auth/Login flow (or separate endpoint).

**Deliverables:**
- REST API endpoints with Swagger documentation.
- Integration tests for endpoints.

#### Phase 4: Frontend Components (12 hours)
**Tasks:**
1. Create `RewardBalance` component (Header widget).
2. Create `TransactionHistory` page/component.
3. Create `Leaderboard` page with tabs (Weekly, Monthly, All-Time).
4. Create `AchievementShowcase` component.
5. Create `StreakTracker` widget.
6. Implement "Level Up" or "Points Awarded" toast/animation.

**Deliverables:**
- React components for all reward features.
- Responsive UI implementation.

#### Phase 5: Integration & Events (6 hours)
**Tasks:**
1. Hook into `LessonCompleted` event to award points.
2. Hook into `TaskApproved` event to award points.
3. Hook into `CourseCompleted` event for bonuses.
4. Implement Referral completion hook.

**Deliverables:**
- Event handlers or service calls triggering rewards.
- End-to-end verification of point flows.

---

## /speckit.clarify

### Questions & Answers

**Q: How do we handle timezones for daily streaks?**
A: Use UTC for all server-side calculations. A "day" is defined as a 24-hour UTC window (00:00 to 23:59 UTC). This simplifies logic and prevents timezone hopping cheats.

**Q: What happens if a user completes a lesson multiple times?**
A: Points are only awarded for the *first* completion of a lesson or task. Subsequent completions do not award points to prevent farming.

**Q: How often is the leaderboard updated?**
A: High-traffic leaderboards (like All-Time) should be cached and updated every 15 minutes. Weekly/Monthly can be cached similarly. "My Rank" might need real-time or slightly delayed calculation.

**Q: Can points be negative?**
A: No, the current balance cannot go below zero. Redemptions should check for sufficient funds. However, `RewardTransaction` can have negative values (Redemption, Penalty).

**Q: Are achievements retroactive?**
A: Yes, if we add new achievements later (e.g., "Completed 5 courses"), the system should ideally scan existing data to award them. For MVP, we check achievements upon relevant actions (e.g., finishing a course).

**Q: How do we prevent race conditions on point balance?**
A: Use database transactions when inserting `RewardTransaction` and updating `User.CurrentPoints`. Alternatively, calculate balance on the fly from transactions (slower) or use optimistic concurrency on the User record. We will use the "Update User + Insert Transaction" approach within a transaction scope.

---

## /speckit.analyze

### Technical Architecture

#### Database Schema

```sql
-- Immutable ledger for all point changes
CREATE TABLE RewardTransactions (
    Id UUID PRIMARY KEY,
    UserId UUID NOT NULL,
    Amount INT NOT NULL, -- Positive for earn, negative for spend
    TransactionType VARCHAR(50) NOT NULL, -- 'LessonCompletion', 'TaskApproval', 'DailyStreak', 'Referral', 'Redemption', 'Bonus'
    ReferenceId UUID NULL, -- ID of Lesson, Task, or Referral
    Description TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- User Achievements
CREATE TABLE UserAchievements (
    UserId UUID NOT NULL,
    AchievementId VARCHAR(50) NOT NULL, -- e.g., 'FIRST_STEPS'
    UnlockedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    PRIMARY KEY (UserId, AchievementId)
);

-- User Streaks
CREATE TABLE UserStreaks (
    UserId UUID PRIMARY KEY,
    CurrentStreak INT DEFAULT 0,
    LastLoginDate DATE, -- Store as date only (UTC)
    LongestStreak INT DEFAULT 0,
    UpdatedAt TIMESTAMP WITH TIME ZONE
);
```

#### Service Layer Logic

**RewardService.AwardPoints:**
1. Validate input (User exists, Amount > 0).
2. Begin DB Transaction.
3. Create `RewardTransaction`.
4. Update `User.CurrentPoints` and `User.TotalPoints`.
5. Check for "Point Hoarder" achievement.
6. Commit Transaction.
7. Return new balance.

**LeaderboardService.GetLeaderboard(type):**
1. Check Cache (`Leaderboard_{type}`).
2. If miss:
   - Query Users table.
   - Filter by date range if Weekly/Monthly (requires joining Transactions or having separate counters). *Optimization: For MVP, All-Time is based on TotalPoints. Weekly/Monthly might require summing Transactions for that period.*
   - Order by Points DESC.
   - Take Top 100.
   - Set Cache (15 mins).
3. Return list.

**StreakService.ProcessLogin:**
1. Get `UserStreak`.
2. Compare `LastLoginDate` (UTC) with `Today` (UTC).
3. If `Today == LastLoginDate + 1 day`: Increment Streak.
4. If `Today > LastLoginDate + 1 day`: Reset Streak to 1.
5. If `Today == LastLoginDate`: Do nothing (already logged in today).
6. Save.
7. If Streak increased, award daily points (5 + bonus milestones).

#### API Endpoints

- `GET /api/rewards/balance` -> `{ currentPoints: 1250, totalEarned: 5000 }`
- `GET /api/rewards/history` -> `[ { amount: 50, type: 'Lesson', date: '...' }, ... ]`
- `GET /api/rewards/leaderboard?type=monthly` -> `[ { rank: 1, username: 'Alice', points: 500 }, ... ]`
- `GET /api/rewards/achievements` -> `[ { id: 'FIRST_STEPS', name: 'First Steps', unlocked: true, date: '...' }, ... ]`

---

## /speckit.checklist

### Implementation Checklist

#### Database & Entities
- [ ] Create `RewardTransaction` entity
- [ ] Create `UserAchievement` entity
- [ ] Create `UserStreak` entity
- [ ] Add `CurrentPoints` and `TotalPoints` to `User` entity
- [ ] Create migration `AddRewardSystem`
- [ ] Apply migration

#### Backend Services
- [ ] Implement `RewardService` (Award/Deduct)
- [ ] Implement `RewardService.GetHistory`
- [ ] Implement `StreakService` logic
- [ ] Implement `AchievementService` (Definitions & Unlock)
- [ ] Implement `LeaderboardService` with caching
- [ ] Add Unit Tests for Streak calculation
- [ ] Add Unit Tests for Point transactions

#### API Controllers
- [ ] Create `RewardsController`
- [ ] Implement `GET /balance`
- [ ] Implement `GET /transactions`
- [ ] Implement `GET /leaderboard`
- [ ] Implement `GET /achievements`
- [ ] Implement `GET /streak`

#### Frontend Components
- [ ] Create `RewardContext` or Hook for global point state
- [ ] Create `RewardBalance` badge for Navbar
- [ ] Create `TransactionHistory` table
- [ ] Create `Leaderboard` page with tabs
- [ ] Create `AchievementCard` and Grid
- [ ] Create `StreakWidget`
- [ ] Add "Points Earned" animation/toast

#### Integration
- [ ] Integrate `RewardService` into `LessonService` (Completion)
- [ ] Integrate `RewardService` into `TaskService` (Approval)
- [ ] Integrate `StreakService` into `AuthService` (Login)
- [ ] Verify Referral logic (if Referral system exists)

---

## /speckit.tasks

### Task Breakdown (Estimated 36 hours)

#### Task 1: Backend Core (8 hours)
**Description:** Entities, Migrations, and Basic Reward Service.
**Subtasks:**
1. Define Entities (Transaction, Streak, Achievement).
2. DB Migration.
3. Implement `RewardService` (Award, Deduct).
4. Implement `TransactionRepository`.

#### Task 2: Streak & Achievement Logic (8 hours)
**Description:** Complex logic for daily streaks and achievement unlocking.
**Subtasks:**
1. Implement `StreakService` with UTC date logic.
2. Define hardcoded Achievements list/enum.
3. Implement `AchievementService` to check conditions.
4. Hook into Login flow for streaks.

#### Task 3: Leaderboard System (6 hours)
**Description:** High-performance leaderboard queries and caching.
**Subtasks:**
1. Implement `LeaderboardService`.
2. Write optimized SQL/LINQ queries for rankings.
3. Integrate `IMemoryCache`.
4. Create `LeaderboardController`.

#### Task 4: Frontend Base & History (6 hours)
**Description:** UI for balance and history.
**Subtasks:**
1. Create `RewardBalance` component.
2. Create `TransactionHistory` page.
3. Connect to API.

#### Task 5: Frontend Gamification UI (8 hours)
**Description:** Leaderboard and Achievements UI.
**Subtasks:**
1. Build `Leaderboard` page with switching tabs.
2. Build `Achievement` showcase.
3. Add animations for point awards.

---

## /speckit.implement

### Code Examples

#### RewardTransaction Entity
```csharp
public class RewardTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Amount { get; set; }
    public string TransactionType { get; set; } // Earned, Redeemed, Bonus
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public User User { get; set; }
}
```

#### RewardService (AwardPoints)
```csharp
public async Task<int> AwardPointsAsync(Guid userId, int amount, string type, string description)
{
    if (amount <= 0) throw new ArgumentException("Amount must be positive");

    using var transaction = _context.Database.BeginTransaction();
    try
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new NotFoundException("User not found");

        // 1. Create Transaction Record
        var rewardTx = new RewardTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            TransactionType = type,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
        _context.RewardTransactions.Add(rewardTx);

        // 2. Update User Balance
        user.CurrentPoints += amount;
        user.TotalPointsEarned += amount;

        // 3. Check Point-based Achievements
        if (user.TotalPointsEarned >= 5000)
        {
            await _achievementService.UnlockAsync(userId, "POINT_HOARDER");
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return user.CurrentPoints;
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

#### Leaderboard Caching
```csharp
public async Task<List<LeaderboardEntryDto>> GetWeeklyLeaderboardAsync()
{
    string cacheKey = "Leaderboard_Weekly";
    if (!_cache.TryGetValue(cacheKey, out List<LeaderboardEntryDto> leaderboard))
    {
        // Calculate start of week (Sunday)
        var today = DateTime.UtcNow.Date;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);

        leaderboard = await _context.RewardTransactions
            .Where(t => t.CreatedAt >= startOfWeek && t.Amount > 0)
            .GroupBy(t => t.UserId)
            .Select(g => new LeaderboardEntryDto
            {
                UserId = g.Key,
                Points = g.Sum(t => t.Amount),
                Username = g.First().User.Username // Simplified, usually requires Join
            })
            .OrderByDescending(e => e.Points)
            .Take(100)
            .ToListAsync();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

        _cache.Set(cacheKey, leaderboard, cacheOptions);
    }
    return leaderboard;
}
```

#### Streak Calculation
```csharp
public async Task CheckStreakAsync(Guid userId)
{
    var streak = await _context.UserStreaks.FindAsync(userId);
    var today = DateTime.UtcNow.Date;

    if (streak == null)
    {
        streak = new UserStreak { UserId = userId, CurrentStreak = 1, LastLoginDate = today };
        _context.UserStreaks.Add(streak);
        await AwardPointsAsync(userId, 5, "DailyLogin", "Daily Login Bonus");
    }
    else
    {
        if (streak.LastLoginDate == today) return; // Already logged in

        if (streak.LastLoginDate == today.AddDays(-1))
        {
            // Consecutive day
            streak.CurrentStreak++;
            int bonus = 0;
            if (streak.CurrentStreak == 5) bonus = 10;
            if (streak.CurrentStreak == 10) bonus = 25;
            if (streak.CurrentStreak == 30) bonus = 100;

            await AwardPointsAsync(userId, 5 + bonus, "DailyLogin", $"Daily Login (Streak {streak.CurrentStreak})");
        }
        else
        {
            // Broken streak
            streak.CurrentStreak = 1;
            await AwardPointsAsync(userId, 5, "DailyLogin", "Daily Login (Streak Reset)");
        }
        streak.LastLoginDate = today;
    }
    await _context.SaveChangesAsync();
}
```
