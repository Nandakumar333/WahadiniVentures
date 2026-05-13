# Implementation Tasks: Reward Points, Leaderboard & Achievements

**Feature**: 006-reward-system  
**Branch**: `006-reward-system`  
**Created**: 2025-12-04  
**Total Estimated Time**: 42 hours

## Overview

This document provides a detailed, executable task breakdown for implementing the reward gamification system. Tasks are organized by user story priority to enable incremental delivery and independent testing. Each task follows Clean Architecture layers (Domain → Application → Infrastructure → Presentation) and includes specific file paths for LLM-guided implementation.

**Implementation Strategy**: MVP-first approach starting with core point awarding (US1), expanding to balance tracking (US2), then gamification features (US3-US5). Each phase delivers a testable, valuable increment.

---

## Task Summary

| Phase | User Story | Tasks | Est. Time | Deliverable |
|-------|-----------|-------|-----------|-------------|
| Phase 1 | Setup | 12 tasks | 4h | Project foundation |
| Phase 2 | Foundational | 8 tasks | 4h | Shared infrastructure |
| Phase 3 | US1 (P1) | 15 tasks | 8h | Point awarding |
| Phase 4 | US2 (P2) | 13 tasks | 7h | Balance & history |
| Phase 5 | US3 (P3) | 11 tasks | 5h | Daily streaks |
| Phase 6 | US4 (P4) | 15 tasks | 8h | Leaderboards |
| Phase 7 | US5 (P5) | 21 tasks | 9h | Achievements & referrals |
| Phase 8 | Polish | 18 tasks | 5h | Cross-cutting concerns |
| **Total** | | **113 tasks** | **50h** | Complete feature |

---

## Dependencies & Parallel Execution

### Story Completion Order
```
Setup (Phase 1)
  ↓
Foundational (Phase 2)
  ↓
US1: Earn Points (Phase 3) ← MVP
  ↓
US2: Track Balance & History (Phase 4)
  ↓
┌──────────────┬──────────────┬──────────────┐
│ US3: Streaks │ US4: Boards  │ US5: Achieve │
│ (Phase 5)    │ (Phase 6)    │ (Phase 7)    │
└──────────────┴──────────────┴──────────────┘
  ↓
Polish & Optimization (Phase 8)
```

**Independent After US2**: US3, US4, US5 can be implemented in parallel by different developers.

**MVP Scope**: Phase 1 + Phase 2 + Phase 3 (US1) = 16 hours (35 tasks)

---

## Phase 1: Setup & Project Foundation
**Goal**: Initialize backend and frontend project structure following Clean Architecture
**Time**: 4 hours

### Backend Setup

- [X] T001 [P] Verify .NET 8 SDK and PostgreSQL 15+ installed - **COMPLETED 2025-12-04**
- [X] T002 Create RewardTransaction entity in backend/src/WahadiniCryptoQuest.Core/Entities/RewardTransaction.cs (Domain Layer) - **COMPLETED 2025-12-04** - Entity updated with AdminUserId, BalanceAfter fields
- [X] T003 [P] Create UserStreak entity in backend/src/WahadiniCryptoQuest.Core/Entities/UserStreak.cs (Domain Layer) - **COMPLETED 2025-12-04** - Entity updated, LastActivityDate renamed to LastLoginDate
- [X] T004 [P] Create UserAchievement entity in backend/src/WahadiniCryptoQuest.Core/Entities/UserAchievement.cs (Domain Layer) - **COMPLETED 2025-12-04**
- [X] T005 [P] Create ReferralAttribution entity in backend/src/WahadiniCryptoQuest.Core/Entities/ReferralAttribution.cs (Domain Layer) - **COMPLETED 2025-12-04**
- [X] T006 Extend User entity with CurrentPoints, TotalPointsEarned, ReferralCode in backend/src/WahadiniCryptoQuest.Core/Entities/User.cs (Domain Layer) - **COMPLETED 2025-12-04** - Added RowVersion, AwardPoints(), DeductPoints(), GenerateReferralCode()
- [X] T007 Create AchievementCatalog static class in backend/src/WahadiniCryptoQuest.Core/Domain/AchievementCatalog.cs (Domain Layer) - **COMPLETED 2025-12-04** - 8 MVP achievements defined
- [X] T008 Create reward DTOs folder backend/src/WahadiniCryptoQuest.Core/DTOs/Reward/ with BalanceDto, TransactionDto, LeaderboardDto, AchievementDto (Domain Layer) - **COMPLETED 2025-12-04**
- [X] T009 Create EF Core entity configurations in backend/src/WahadiniCryptoQuest.DAL/Configurations/RewardTransactionConfiguration.cs (Infrastructure Layer) - **COMPLETED 2025-12-04** - All 4 reward entities configured
- [X] T010 Create EF Core migration AddRewardSystem in backend/src/WahadiniCryptoQuest.DAL/Migrations/ (Infrastructure Layer) - **COMPLETED 2025-12-04** - Migration exists and applied
- [X] T011 Add reward system connection string to backend/src/WahadiniCryptoQuest.API/appsettings.json (Presentation Layer) - **COMPLETED 2025-12-04** - Connection string configured
- [X] T012 Apply migration to database using dotnet ef database update (Infrastructure Layer) - **COMPLETED 2025-12-04** - AddRewardSystem + AddUserRowVersionForConcurrency applied

### Frontend Setup (if applicable)

_Note: Frontend tasks will be included in each user story phase for feature-specific UI_

---

## Phase 2: Foundational Infrastructure
**Goal**: Implement shared infrastructure needed by all user stories
**Time**: 4 hours

### Repository Interfaces & Base Implementation

- [X] T013 Create IRewardTransactionRepository interface in backend/src/WahadiniCryptoQuest.Core/Interfaces/Repositories/IRewardTransactionRepository.cs (Domain Layer) - **COMPLETED 2025-12-04** - Already existed
- [X] T014 [P] Create IUserStreakRepository interface in backend/src/WahadiniCryptoQuest.Core/Interfaces/Repositories/IUserStreakRepository.cs (Domain Layer) - **COMPLETED 2025-12-04** - Created with GetByUserIdAsync, GetActiveStreaksAsync, GetTopLongestStreaksAsync methods
- [X] T015 [P] Create IUserAchievementRepository interface in backend/src/WahadiniCryptoQuest.Core/Interfaces/Repositories/IUserAchievementRepository.cs (Domain Layer) - **COMPLETED 2025-12-04** - Created with GetUserAchievementsAsync, HasAchievementAsync, GetUnnotifiedAchievementsAsync, MarkAsNotifiedAsync methods
- [X] T016 [P] Create IReferralAttributionRepository interface in backend/src/WahadiniCryptoQuest.Core/Interfaces/Repositories/IReferralAttributionRepository.cs (Domain Layer) - **COMPLETED 2025-12-04** - Created with GetByInviteeUserIdAsync, GetReferralsByInviterAsync, GetSuccessfulReferralsAsync, GetSuccessfulReferralCountAsync methods

### Repository Implementations

- [X] T017 Implement RewardTransactionRepository with cursor pagination in backend/src/WahadiniCryptoQuest.DAL/Repositories/RewardTransactionRepository.cs (Infrastructure Layer) - **COMPLETED 2025-12-04** - Already existed
- [X] T018 [P] Implement UserStreakRepository in backend/src/WahadiniCryptoQuest.DAL/Repositories/UserStreakRepository.cs (Infrastructure Layer) - **COMPLETED 2025-12-04** - Implemented all interface methods
- [X] T019 [P] Implement UserAchievementRepository in backend/src/WahadiniCryptoQuest.DAL/Repositories/UserAchievementRepository.cs (Infrastructure Layer) - **COMPLETED 2025-12-04** - Implemented with MarkAsNotified domain method integration
- [X] T020 [P] Implement ReferralAttributionRepository in backend/src/WahadiniCryptoQuest.DAL/Repositories/ReferralAttributionRepository.cs (Infrastructure Layer) - **COMPLETED 2025-12-04** - Implemented using ReferrerId/ReferredUserId properties

---

## Phase 3: US1 - Earn Points For Learning (Priority: P1)
**Goal**: Implement core point awarding for lesson/task/course completion
**Time**: 8 hours
**Independent Test**: Trigger completion events and verify points post once per activity with correct amount

### Domain Layer

- [X] T021 [US1] Add AwardPoints domain method with validation to User entity in backend/src/WahadiniCryptoQuest.Core/Entities/User.cs (Domain Layer) - **COMPLETED 2025-12-04** - Method exists with validation for positive amounts (max 100000) and updates CurrentPoints + TotalPointsEarned
- [X] T022 [P] [US1] Add DeductPoints domain method with insufficient balance check to User entity in backend/src/WahadiniCryptoQuest.Core/Entities/User.cs (Domain Layer) - **COMPLETED 2025-12-04** - Method exists with validation for positive amounts and insufficient balance check before deduction
- [X] T023 [P] [US1] Add Create factory method to RewardTransaction entity in backend/src/WahadiniCryptoQuest.Core/Entities/RewardTransaction.cs (Domain Layer) - **COMPLETED 2025-12-04** - Factory method exists with validation for non-zero amounts and required description, supports optional referenceId/referenceType/adminUserId parameters
- [X] T024 [P] [US1] Create TransactionType enum in backend/src/WahadiniCryptoQuest.Core/Enums/TransactionType.cs (Domain Layer) - **COMPLETED 2025-12-04** - Enum exists with 9 values: LessonCompletion, TaskApproval, CourseCompletion, DailyStreak, ReferralBonus, AchievementBonus, AdminBonus, AdminPenalty, Redemption

### Application Layer

- [X] T025 [US1] Create IRewardService interface in backend/src/WahadiniCryptoQuest.Core/Interfaces/Services/IRewardService.cs (Domain Layer) - **COMPLETED 2025-12-04** - Interface exists with AwardPointsAsync, DeductPointsAsync, GetUserBalanceAsync methods
- [X] T026 [US1] Create AwardPointsCommand in backend/src/WahadiniCryptoQuest.Service/Commands/Rewards/AwardPointsCommand.cs (Application Layer) - **COMPLETED 2025-12-04** - MediatR IRequest<Guid> command exists with properties: UserId, Amount, Type, Description, ReferenceId, ReferenceType
- [X] T027 [P] [US1] Create AwardPointsCommandHandler with database transaction and idempotency in backend/src/WahadiniCryptoQuest.Service/Commands/Rewards/AwardPointsCommandHandler.cs (Application Layer) - **COMPLETED 2025-12-04** - Handler exists, enhanced with retry logic (exponential backoff, max 3 retries, 100ms * retryCount delay) for DbUpdateConcurrencyException handling
- [X] T028 [P] [US1] Create AwardPointsValidator using FluentValidation in backend/src/WahadiniCryptoQuest.API/Validators/Rewards/AwardPointsValidator.cs (Presentation Layer) - **COMPLETED 2025-12-04** - Validator exists with rules: Amount >0 and ≤100000, Description max 500 chars, ReferenceId max 100 chars, ReferenceType max 50 chars
- [X] T029 [US1] Create RewardService implementing IRewardService in backend/src/WahadiniCryptoQuest.Service/Rewards/RewardService.cs (Application Layer) - **COMPLETED 2025-12-04** - Service exists, enhanced with idempotency check using GetByReferenceAsync before creating transactions to prevent duplicate point awards for same lesson/task/course completion
- [X] T030 [P] [US1] Create RewardMappingProfile for AutoMapper in backend/src/WahadiniCryptoQuest.Service/Mappings/RewardMappingProfile.cs (Application Layer) - **COMPLETED 2025-12-04** - Profile created with mappings: RewardTransaction→TransactionDto (Id, Amount, Type, Description, CreatedAt, ReferenceId), User→BalanceDto (CurrentPoints, TotalEarned, Rank=0), UserAchievement→AchievementDto (Id, IsUnlocked=true, UnlockedAt, Progress=100)

### Infrastructure Layer

- [X] T031 [US1] Add deduplication unique index to RewardTransaction in migration backend/src/WahadiniCryptoQuest.DAL/Migrations/AddRewardSystemDeduplication.cs (Infrastructure Layer) - **COMPLETED 2025-12-04** - Composite unique index added to RewardTransactionConfiguration on (UserId, ReferenceId, ReferenceType) with HasFilter for NULL references, migration created: AddRewardSystemDeduplicationIndex
- [X] T032 [P] [US1] Add RowVersion to User entity for optimistic concurrency in backend/src/WahadiniCryptoQuest.Core/Entities/User.cs (Domain Layer) - **COMPLETED 2025-12-04** - RowVersion property exists (byte[] type) in User entity for concurrency control, added in prior migration AddUserRowVersionForConcurrency
- [X] T033 [P] [US1] Configure optimistic concurrency in EF Core User configuration backend/src/WahadiniCryptoQuest.DAL/Configurations/UserConfiguration.cs (Infrastructure Layer) - **COMPLETED 2025-12-04** - RowVersion configured with .IsRowVersion() in EntityConfigurations.cs, enables automatic concurrency token handling

### Presentation Layer

- [X] T034 [US1] Create RewardsController with AwardPoints endpoint in backend/src/WahadiniCryptoQuest.API/Controllers/RewardsController.cs (Presentation Layer) - **COMPLETED 2025-12-04** - Controller exists with POST /api/rewards/award endpoint accepting AwardPointsDto, sends AwardPointsCommand via IMediator
- [X] T035 [P] [US1] Add [Authorize] and role-based authorization attributes to RewardsController (Presentation Layer) - **COMPLETED 2025-12-04** - [Authorize(Roles = "Admin")] attribute applied to AwardPoints action, restricts point awarding to admin users only

---

## Phase 4: US2 - Track Balance & History (Priority: P2)
**Goal**: Implement balance retrieval and transaction history with pagination
**Time**: 6 hours
**Independent Test**: Access reward summary for learner with mixed transactions, verify balance math, chronological ordering, pagination

### Application Layer

- [X] T036 [US2] Create GetBalanceQuery in backend/src/WahadiniCryptoQuest.Service/Queries/Rewards/GetBalanceQuery.cs (Application Layer) **COMPLETED 2025-12-04** - MediatR query with UserId parameter
- [X] T037 [US2] Create GetBalanceQueryHandler in backend/src/WahadiniCryptoQuest.Service/Queries/Rewards/GetBalanceQueryHandler.cs (Application Layer) **COMPLETED 2025-12-04** - Uses IUserRepository and AutoMapper to map User→BalanceDto
- [X] T038 [US2] Create GetTransactionHistoryQuery with cursor pagination in backend/src/WahadiniCryptoQuest.Service/Queries/Rewards/GetTransactionHistoryQuery.cs (Application Layer) **COMPLETED 2025-12-04** - Query with UserId, PageSize (default 20, max 100), optional Cursor (Base64), optional TransactionType filter
- [X] T039 [US2] Create GetTransactionHistoryQueryHandler with cursor encoding/decoding and 1-hour expiration validation in backend/src/WahadiniCryptoQuest.Service/Queries/Rewards/GetTransactionHistoryQueryHandler.cs (Application Layer) **COMPLETED 2025-12-04** - Cursor format: timestamp|transactionId|cursorCreatedAt (Base64), 1-hour expiration, uses GetQueryable() for filtering
- [X] T040 [P] [US2] Create PaginatedResult<T> wrapper class in backend/src/WahadiniCryptoQuest.Core/DTOs/Common/PaginatedResult.cs (Domain Layer) **COMPLETED 2025-12-04** - Generic wrapper with Items, TotalCount, PageNumber, PageSize, TotalPages, HasNextPage/HasPreviousPage, NextCursor/PreviousCursor, includes Map<TResult> method

### Presentation Layer

- [X] T041 [US2] Add GET /api/rewards/balance endpoint to RewardsController (Presentation Layer) **COMPLETED 2025-12-04** - Returns BalanceDto for authenticated user, extracts UserId from JWT ClaimTypes.NameIdentifier
- [X] T042 [US2] Add GET /api/rewards/transactions endpoint with pagination to RewardsController (Presentation Layer) **COMPLETED 2025-12-04** - Query params: pageSize (default 20), cursor (optional), transactionType filter (optional), returns PaginatedResult<TransactionDto>
- [X] T043 [P] [US2] Add rate limiting middleware with token bucket algorithm in backend/src/WahadiniCryptoQuest.API/Middleware/RateLimitingMiddleware.cs (Presentation Layer) **COMPLETED 2025-12-04** - Token bucket middleware already exists; enhanced with ASP.NET Core rate limiter policies for endpoint-specific limits
- [X] T044 [US2] Configure rate limit policies in appsettings.json (100 req/min balance/history, 10 req/min leaderboards) and register in Program.cs (Presentation Layer) **COMPLETED 2025-12-04** - Added RateLimitPolicies section with balance-history (100/min), leaderboard (10/min), achievements (50/min), rewards-general (100/min); configured sliding window limiters in Program.cs; applied [EnableRateLimiting] attributes to RewardsController endpoints

### Frontend Layer

- [X] T045 [P] [US2] Create reward.types.ts in frontend/src/types/reward.types.ts (BalanceDto, TransactionDto, LeaderboardDto, AchievementDto, StreakDto, ReferralDto) **COMPLETED 2025-12-04** - TypeScript types with as const enums: TransactionTypeValues, LeaderboardPeriodValues, AchievementCategoryValues, ReferralStatusValues, PaginatedResult<T> generic
- [X] T046 [P] [US2] Create reward.service.ts API client in frontend/src/services/api/reward.service.ts **COMPLETED 2025-12-04** - Complete API client with balance, transactions, leaderboard, achievements, streaks, referrals methods, utility functions (formatPoints, getTransactionColor, getTransactionIcon, formatRelativeTime)
- [X] T047 [US2] Create useRewardBalance hook using React Query in frontend/src/hooks/reward/useRewardBalance.ts **COMPLETED 2025-12-04** - React Query hook with 2-minute stale time, auto-refetch every 5 minutes, useRefreshBalance and usePrefetchBalance helpers
- [X] T048 [US2] Create RewardBalance component for navbar in frontend/src/components/rewards/RewardBalance.tsx **COMPLETED 2025-12-04** - Three variants: RewardBalance (navbar), RewardBalanceCompact (mobile), RewardBalanceDetailed (dashboard), loading/error states, animated coin icon
- [X] T049 [US2] Create TransactionHistory page with pagination in frontend/src/pages/rewards/TransactionHistory.tsx **COMPLETED 2025-12-04** - Infinite scroll with cursor pagination, filter by transaction type, balance summary card, empty states, responsive design
- [X] T050 [P] [US2] Create TransactionRow component in frontend/src/components/rewards/TransactionRow.tsx **COMPLETED 2025-12-04** - Three variants: TransactionRow, TransactionRowCompact, TransactionCard, color-coded amounts, relative time display, type icons
- [X] T051 [P] [US2] Add empty state component for zero transactions in frontend/src/components/rewards/EmptyTransactionState.tsx **COMPLETED 2025-12-04** - EmptyTransactionState, EmptyFilteredTransactionState, EmptyTransactionStateCompact variants, helpful CTAs, ways to earn points display

---

## Phase 5: US3 - Maintain Daily Streaks (Priority: P3)
**Goal**: Implement daily login streak tracking with escalating bonuses
**Time**: 5 hours
**Independent Test**: Simulate logins across multiple UTC days, verify streak count, reset rules, bonus transactions

### Application Layer

- [X] T052 [US3] Create IStreakService interface in backend/src/WahadiniCryptoQuest.Core/Interfaces/Services/IStreakService.cs (Domain Layer) **COMPLETED 2025-12-04** - Interface with ProcessLoginAsync and GetUserStreakAsync methods
- [X] T053 [US3] Create StreakService with ProcessLogin method in backend/src/WahadiniCryptoQuest.Service/Services/StreakService.cs (Application Layer) **COMPLETED 2025-12-04** - Implemented with UTC date comparison, streak increment/reset logic, bonus point awarding
- [X] T054 [US3] Implement UTC date comparison logic in StreakService (Application Layer) **COMPLETED 2025-12-04** - DateTime.UtcNow.Date comparison with daysSinceLastLogin calculation
- [X] T055 [US3] Add streak bonus configuration to appsettings.json (base 5, milestones at 5/10/30/100) (Presentation Layer) **COMPLETED 2025-12-04** - RewardSettings:Streaks:BaseBonus: 5, Milestones: {5:25, 10:50, 30:100, 100:250}
- [X] T056 [US3] Create GetStreakQuery in backend/src/WahadiniCryptoQuest.Service/Queries/Reward/GetStreakQuery.cs (Application Layer) **COMPLETED 2025-12-04** - MediatR IRequest<StreakDto> with UserId parameter
- [X] T057 [US3] Create GetStreakQueryHandler in backend/src/WahadiniCryptoQuest.Service/Queries/Reward/GetStreakQueryHandler.cs (Application Layer) **COMPLETED 2025-12-04** - Handler delegates to IStreakService.GetUserStreakAsync
- [X] T052.1 [US3] Create ProcessStreakCommand and ProcessStreakCommandHandler for POST endpoint **COMPLETED 2025-12-04** - MediatR command pattern for streak processing
- [X] T052.2 [US3] Register IStreakService in DI container **COMPLETED 2025-12-04** - Added services.AddScoped<IStreakService, StreakService>() to ServiceCollectionExtensions
- [X] T052.3 [US3] Update StreakDto in backend/src/WahadiniCryptoQuest.Core/DTOs/Reward/RewardDtos.cs **COMPLETED 2025-12-04** - Added StreakDto(CurrentStreak, LongestStreak, LastLoginDate, BonusPointsAwarded, NextMilestoneAt)
- [X] T052.4 [US3] Update ReferralDto and ReferralCodeDto in RewardDtos.cs **COMPLETED 2025-12-04** - Added ReferralDto and ReferralCodeDto for future Phase 7 implementation

### Presentation Layer

- [X] T058 [US3] Add POST /api/rewards/streak/process endpoint to RewardsController (Presentation Layer) **COMPLETED 2025-12-04** - Endpoint processes user login streak, extracts UserId from JWT, returns StreakDto
- [X] T059 [US3] Add GET /api/rewards/streak endpoint to RewardsController (Presentation Layer) **COMPLETED 2025-12-04** - Endpoint retrieves current streak info without processing login

### Frontend Layer

- [X] T060 [P] [US3] Create useStreak hook in frontend/src/hooks/reward/useStreak.ts **COMPLETED 2025-12-04** - useStreak (query, 1-min stale), useProcessStreak (mutation), useRefreshStreak, usePrefetchStreak helpers
- [X] T061 [P] [US3] Create StreakTracker widget component in frontend/src/components/rewards/StreakTracker.tsx **COMPLETED 2025-12-04** - Three variants: StreakTracker (full with progress bar), StreakTrackerCompact (navbar badge), StreakTrackerDetailed (dashboard with milestones)
- [X] T062 [US3] Integrate streak processing into login flow in frontend/src/components/auth/LoginForm.tsx **COMPLETED 2025-12-04** - processStreak.mutate() called after successful login, invalidates balance and transactions cache
- [X] T060.1 [US3] Update StreakDto in frontend/src/types/reward.types.ts **COMPLETED 2025-12-04** - Updated to match backend: currentStreak, longestStreak, lastLoginDate, bonusPointsAwarded, nextMilestoneAt
- [X] T060.2 [US3] Add exports to frontend/src/hooks/reward/index.ts **COMPLETED 2025-12-04** - Exported useStreak, useProcessStreak, useRefreshStreak, usePrefetchStreak
- [X] T060.3 [US3] Add exports to frontend/src/components/rewards/index.ts **COMPLETED 2025-12-04** - Exported StreakTracker, StreakTrackerCompact, StreakTrackerDetailed

---

## Phase 6: US4 - Compete On Leaderboards (Priority: P4)
**Goal**: Implement cached leaderboards (weekly/monthly/all-time) with personal rank
**Time**: 7 hours
**Independent Test**: Generate sample data across periods, verify rank ordering, ties, caching, personal rank visibility

### Application Layer

- [x] T063 [US4] Create ILeaderboardService interface in backend/src/WahadiniCryptoQuest.Core/Interfaces/Services/ILeaderboardService.cs (Domain Layer) - COMPLETED 2025-12-04
- [x] T064 [US4] Create LeaderboardService with IMemoryCache in backend/src/WahadiniCryptoQuest.Service/Rewards/LeaderboardService.cs (Application Layer) - COMPLETED 2025-12-04
- [x] T065 [US4] Implement GetLeaderboard method with 15-minute cache TTL in LeaderboardService (Application Layer) - COMPLETED 2025-12-04
- [x] T066 [US4] Implement GetUserRank method with real-time calculation in LeaderboardService (Application Layer) - COMPLETED 2025-12-04
- [x] T067 [US4] Add tie-breaking logic (User.CreatedAt ASC) to leaderboard queries (Application Layer) - COMPLETED 2025-12-04
- [x] T068 [US4] Create GetLeaderboardQuery in backend/src/WahadiniCryptoQuest.Service/Queries/Rewards/GetLeaderboardQuery.cs (Application Layer) - COMPLETED 2025-12-04
- [x] T069 [US4] Create GetLeaderboardQueryHandler in backend/src/WahadiniCryptoQuest.Service/Queries/Rewards/GetLeaderboardQueryHandler.cs (Application Layer) - COMPLETED 2025-12-04

### Presentation Layer

- [x] T070 [US4] Extended RewardsController with leaderboard endpoints in backend/src/WahadiniCryptoQuest.API/Controllers/RewardsController.cs (Presentation Layer) - COMPLETED 2025-12-04
- [x] T071 [US4] Add GET /api/v1/rewards/leaderboard endpoint with type parameter (weekly/monthly/all-time) (Presentation Layer) - COMPLETED 2025-12-04
- [x] T072 [P] [US4] Add rate limiting (10 req/min) to leaderboard endpoints (Presentation Layer) - COMPLETED 2025-12-04

### Frontend Layer

- [x] T073 [P] [US4] Create useLeaderboard hook in frontend/src/hooks/reward/useLeaderboard.ts - COMPLETED 2025-12-04
- [x] T074 [US4] Create Leaderboard page with tabs in frontend/src/pages/rewards/Leaderboard.tsx - COMPLETED 2025-12-04
- [x] T075 [P] [US4] Create LeaderboardTable component in frontend/src/components/rewards/LeaderboardTable.tsx - COMPLETED 2025-12-04
- [x] T076 [P] [US4] Create UserRankCard component in frontend/src/components/rewards/UserRankCard.tsx - COMPLETED 2025-12-04
- [x] T077 [P] [US4] Create EmptyLeaderboardState component in frontend/src/components/rewards/EmptyLeaderboardState.tsx - COMPLETED 2025-12-04

---

## Phase 7: US5 - Unlock Achievements & Referrals (Priority: P5)
**Goal**: Implement achievement tracking and referral attribution system
**Time**: 6 hours
**Independent Test**: Trigger milestone events and referral completions, verify badge unlocks, celebration messaging, bonus attribution

### Application Layer - Achievements

- [x] T078 [US5] Create IAchievementService interface in backend/src/WahadiniCryptoQuest.Core/Interfaces/Services/IAchievementService.cs (Domain Layer) - **COMPLETED 2025-12-05**
- [x] T079 [US5] Create AchievementService with CheckAndUnlock method in backend/src/WahadiniCryptoQuest.Service/Services/AchievementService.cs (Application Layer) - **COMPLETED 2025-12-05**
- [x] T080 [US5] Implement achievement evaluation logic using AchievementCatalog in AchievementService (Application Layer) - **COMPLETED 2025-12-05**
- [x] T081 [US5] Add achievement bonus point awarding to CheckAndUnlock method (0-250 points per achievement definition) (Application Layer) - **COMPLETED 2025-12-05**
- [x] T082 [US5] Create GetAchievementsQuery in backend/src/WahadiniCryptoQuest.Service/Queries/Rewards/GetAchievementsQuery.cs (Application Layer) - **COMPLETED 2025-12-05**
- [x] T083 [US5] Create GetAchievementsQueryHandler with progress calculation in backend/src/WahadiniCryptoQuest.Service/Handlers/Rewards/GetAchievementsQueryHandler.cs (Application Layer) - **COMPLETED 2025-12-05**

### Application Layer - Referrals

- [x] T084 [P] [US5] Create IReferralService interface in backend/src/WahadiniCryptoQuest.Core/Interfaces/Services/IReferralService.cs (Domain Layer) - **COMPLETED 2025-12-05**
- [x] T085 [P] [US5] Create ReferralService in backend/src/WahadiniCryptoQuest.Service/Services/ReferralService.cs (Application Layer) - **COMPLETED 2025-12-05**
- [x] T086 [P] [US5] Implement ValidateReferralCode method in ReferralService (Application Layer) - **COMPLETED 2025-12-05**
- [x] T087 [P] [US5] Implement ProcessReferralCompletion method in ReferralService (Application Layer) - **COMPLETED 2025-12-05**

### Presentation Layer

- [x] T097 [US5] Add GET /api/v1/rewards/achievements endpoint to RewardsController (Presentation Layer) - **COMPLETED 2025-12-05**
- [x] T098 [US5] Add GET /api/v1/rewards/referrals endpoint to RewardsController (Presentation Layer) - **COMPLETED 2025-12-05**
- [x] T099 [P] [US5] Add GET /api/v1/rewards/referrals/validate/{code} public endpoint to RewardsController (Presentation Layer) - **COMPLETED 2025-12-05**
- [x] T100 [P] [US5] Add GET /api/v1/rewards/rank endpoint to RewardsController for personal rank display (Presentation Layer) - **COMPLETED 2025-12-05**

### Notification Infrastructure

- [x] T088 [US5] Create INotificationQueue interface in backend/src/WahadiniCryptoQuest.Core/Interfaces/Services/INotificationQueue.cs (Domain Layer) - **COMPLETED 2025-12-05**
- [x] T089 [US5] Create NotificationQueueService (stub implementation with logging) in backend/src/WahadiniCryptoQuest.Service/Services/NotificationQueueService.cs (Infrastructure Layer) - **COMPLETED 2025-12-05**
- [x] T090 [US5] Add notification queue to AwardPointsCommandHandler for ≥100 point awards in backend/src/WahadiniCryptoQuest.Service/Handlers/Reward/AwardPointsCommandHandler.cs (Application Layer) - **COMPLETED 2025-12-05**
- [x] T091 [US5] Add notification queue to AchievementService for unlock events in backend/src/WahadiniCryptoQuest.Service/Services/AchievementService.cs (Application Layer) - **COMPLETED 2025-12-05**

### Frontend Layer

- [x] T092 [P] [US5] Create useAchievements hook in frontend/src/hooks/reward/useAchievements.ts - **COMPLETED 2025-12-05**
- [x] T093 [P] [US5] Create AchievementGrid component in frontend/src/components/rewards/AchievementGrid.tsx - **COMPLETED 2025-12-05**
- [x] T094 [P] [US5] Create AchievementCard component with lock/unlock states in frontend/src/components/rewards/AchievementCard.tsx - **COMPLETED 2025-12-05**
- [x] T095 [P] [US5] Create PointsToast notification component in frontend/src/components/rewards/PointsToast.tsx - **COMPLETED 2025-12-05**
- [x] T096 [US5] Create ReferralLink component in frontend/src/components/rewards/ReferralLink.tsx - **COMPLETED 2025-12-05**

---

## Phase 8: Polish & Cross-Cutting Concerns
**Goal**: Add admin features, performance optimizations, error handling enhancements
**Time**: 2 hours

### Admin Features

- [x] T101 [P] Add AdminUserId nullable field to RewardTransaction entity in backend/src/WahadiniCryptoQuest.Core/Entities/RewardTransaction.cs (Domain Layer)
- [x] T102 [P] Create EF Core migration AddAdminAuditToTransactions in backend/src/WahadiniCryptoQuest.DAL/Migrations/ (Infrastructure Layer)
- [x] T103 [P] Create AdminRewardsController with [Authorize(Roles="Admin")] in backend/src/WahadiniCryptoQuest.API/Controllers/AdminRewardsController.cs (Presentation Layer)
- [x] T104 [P] Add POST /api/v1/admin/rewards/award endpoint with AdminUserId tracking (Presentation Layer)
- [x] T105 [P] Add POST /api/v1/admin/rewards/deduct endpoint with justification validation (min 10 chars) and AdminUserId tracking (Presentation Layer)
- [x] T106 [P] Add GET /api/v1/admin/rewards/users/{userId} endpoint for full transaction history with admin actions highlighted (Presentation Layer)

### Error Handling & Logging

- [x] T107 Add structured logging to RewardService for all point transactions in backend/src/WahadiniCryptoQuest.Service/Services/RewardService.cs (Application Layer)
- [x] T108 [P] Add global exception handler for RewardException types in backend/src/WahadiniCryptoQuest.API/Middleware/GlobalExceptionHandlerMiddleware.cs (Presentation Layer)
- [x] T109 Add alert configuration for failed transaction rollbacks (10 failures within 5 minutes) in backend/src/WahadiniCryptoQuest.API/appsettings.json (Presentation Layer)

### Background Jobs

- [x] T110 [P] Create DeduplicationCleanupJob for removing transactions older than 24 hours in backend/src/WahadiniCryptoQuest.Service/BackgroundJobs/DeduplicationCleanupJob.cs (Application Layer)
- [x] T111 [P] Register DeduplicationCleanupJob to run daily at 2 AM UTC in Program.cs (Presentation Layer)

### Performance Optimization

- [x] T112 [P] Add composite indexes for leaderboard queries in ApplicationDbContext (Infrastructure Layer)
- [x] T113 [P] Add table partitioning strategy documentation for RewardTransaction at 1M records in backend/src/WahadiniCryptoQuest.DAL/README.md (Infrastructure Layer)

---

## Validation Checklist

**Format Compliance**: ✅ All 113 tasks follow `- [ ] [ID] [Markers] Description with path` format

**Story Coverage**:
- ✅ US1 (P1): 15 tasks covering domain, application, infrastructure, presentation layers
- ✅ US2 (P2): 13 tasks covering backend API + frontend UI + rate limiting
- ✅ US3 (P3): 11 tasks covering streak logic + UI integration
- ✅ US4 (P4): 15 tasks covering leaderboard caching + UI + empty states
- ✅ US5 (P5): 21 tasks covering achievements + referrals + notifications + retry queue
- ✅ Phase 8: 18 tasks covering admin features, logging, background jobs, performance

**Gap Resolution**:
- ✅ H001: Notification retry queue added (T088-T091)
- ✅ H002: Rate limiting configuration added (T044)
- ✅ H003: Idempotency cleanup background job added (T110-T111)
- ✅ H004: Cursor expiration validation added (T039)
- ✅ M005: Achievement bonus points task added (T081)
- ✅ M006: AdminUserId audit trail added (T101-T102)
- ✅ L003: Empty state components added for all views

**Independent Testability**: Each user story phase includes:
- Domain entities with business logic
- Service layer with CQRS handlers
- API endpoints with authorization
- Frontend components (where applicable)
- Clear acceptance criteria from spec.md

**Parallel Opportunities**: 58 tasks marked [P] can be executed in parallel within their phase

**MVP Scope**: Phase 1 + Phase 2 + Phase 3 (35 tasks, 16 hours) delivers core point awarding capability

---

## Next Steps

1. **Start MVP**: Execute Phase 1 → Phase 2 → Phase 3 (US1) for first deliverable
2. **Incremental Delivery**: Complete US2, then US3/US4/US5 in any order
3. **Testing**: Run independent tests after each phase per spec.md acceptance scenarios
4. **Review**: Cross-reference completed tasks with requirements-quality.md checklist

**Status**: Task breakdown complete with all HIGH priority gaps resolved. Ready for implementation.  
**Estimated Total Time**: 50 hours  
**MVP Time**: 16 hours (Phases 1-3, 35 tasks)
