# Implementation Plan: Reward Points, Leaderboard & Achievements

**Branch**: `006-reward-system` | **Date**: 2025-12-04 | **Spec**: [spec.md](./spec.md)  
**Input**: Feature specification from `/specs/006-reward-system/spec.md`

## Summary

Implement a comprehensive gamification system to drive user engagement through reward points, competitive leaderboards, unlockable achievements, and daily login streaks. The system provides flexible point awarding for learning activities, maintains an immutable transaction ledger, implements cached leaderboards (weekly/monthly/all-time), tracks achievement milestones, manages daily streaks with escalating bonuses, and supports referral tracking with attribution.

**Technical Approach**: Event-sourced transaction ledger with denormalized balances, UTC-based streak calculation, code-defined achievements with database state tracking, multi-tier caching for leaderboards, and cursor-based pagination for transaction history. All components follow Clean Architecture with CQRS patterns.

## Technical Context

**Language/Version**: .NET 8 C# (backend), TypeScript 4.9+ with React 18 (frontend)

**Primary Dependencies**:
- Backend: ASP.NET Core Web API, Entity Framework Core 8.0, PostgreSQL 15+, AutoMapper, FluentValidation, MediatR, JWT Bearer tokens, IMemoryCache
- Frontend: React 18, Vite, TailwindCSS 3.4, React Query 5, Zustand, React Hook Form 7, Zod, Axios

**Storage**: PostgreSQL 15+ with JSONB support, time-based partitioning for user activity data

**Testing**: 
- Backend: xUnit, Moq, FluentAssertions, EF Core In-Memory provider
- Frontend: Vitest, React Testing Library, MSW (Mock Service Worker)

**Target Platform**: Web application (responsive design for desktop/tablet/mobile)

**Project Type**: Full-stack web application (backend + frontend)

**Performance Goals**:
- Leaderboard queries: <500ms for top 100 with 100K+ users
- Transaction history pagination: <200ms per page
- Point award operations: <100ms end-to-end
- Cache hit rate: >90% for leaderboard requests
- Concurrent point operations: Support 100 simultaneous awards without conflicts

**Constraints**:
- Balance must never go negative (enforced at domain level)
- Transactions are immutable (append-only ledger)
- Streak calculations use UTC only (no timezone conversions)
- Leaderboard cache TTL: 15 minutes maximum
- Personal rank always calculated fresh (no caching)

**Scale/Scope**:
- Expected user base: 10K-100K users
- Transaction volume: 1M+ entries within first year
- Achievement catalog: 10-20 milestone achievements

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Constitution File Status**: Template present but not populated with project-specific rules.

**Default Checks Applied**:
- ✅ **Clean Architecture**: Domain entities separated from infrastructure, repository pattern applied
- ✅ **SOLID Principles**: Single responsibility, dependency inversion via interfaces
- ✅ **Testability**: All business logic in service layer with unit test coverage
- ✅ **Security**: JWT authentication required, role-based admin endpoints, input validation
- ✅ **Performance**: Caching strategy defined, indexed queries, pagination implemented

**No Constitution Violations Detected**

## Project Structure

### Documentation (this feature)

```text
specs/006-reward-system/
├── spec.md              # Feature specification (completed)
├── plan.md              # This file (implementation plan)
├── research.md          # Phase 0 output (technical decisions)
├── data-model.md        # Phase 1 output (entity designs)
├── quickstart.md        # Phase 1 output (developer guide)
├── contracts/           # Phase 1 output (API documentation)
│   └── api-contracts.md
└── checklists/
    └── requirements.md  # Spec validation checklist
```

### Source Code (repository root)

```text
backend/src/WahadiniCryptoQuest.Core/              # Domain Layer
  ├── Entities/
  │   ├── RewardTransaction.cs           [NEW]
  │   ├── UserStreak.cs                  [NEW]
  │   ├── UserAchievement.cs             [NEW]
  │   ├── ReferralAttribution.cs         [NEW]
  │   └── User.cs                        [MODIFY]
  ├── DTOs/Reward/                       [NEW]
  ├── Interfaces/Repositories/           [NEW]
  ├── Interfaces/Services/               [NEW]
  └── Domain/AchievementCatalog.cs       [NEW]

backend/src/WahadiniCryptoQuest.Service/           # Application Layer
  ├── Rewards/                           [NEW]
  ├── Commands/                          [NEW]
  ├── Queries/                           [NEW]
  └── Mappings/RewardMappingProfile.cs   [NEW]

backend/src/WahadiniCryptoQuest.DAL/               # Infrastructure Layer
  ├── Repositories/                      [NEW]
  ├── Configurations/                    [NEW]
  └── Migrations/AddRewardSystem.cs      [NEW]

backend/src/WahadiniCryptoQuest.API/               # Presentation Layer
  ├── Controllers/                       [NEW]
  └── Validators/                        [NEW]

frontend/src/
  ├── types/reward.types.ts              [NEW]
  ├── services/api/reward.service.ts     [NEW]
  ├── hooks/reward/                      [NEW]
  ├── components/rewards/                [NEW]
  └── pages/rewards/                     [NEW]
```

**Structure Decision**: Full-stack web application following Clean Architecture. Backend uses layered structure (Core/Service/DAL/API). Frontend uses component-based architecture with feature folders.

## Complexity Tracking

**No Constitution Violations** - No complexity justification required.

---

## Phase 0: Research & Design Decisions ✅ COMPLETE

**Status**: All technical unknowns resolved. See [research.md](./research.md)

**Key Decisions**:
1. ✅ Event-sourced transaction ledger with denormalized balances
2. ✅ Database transactions + optimistic concurrency
3. ✅ IMemoryCache with 15-minute TTL for leaderboards
4. ✅ UTC-based streak calculation
5. ✅ Code-defined achievements with database state tracking
6. ✅ Cursor-based pagination for transaction history
7. ✅ Idempotency using deduplication keys (UserId + ReferenceId + TransactionType)
8. ✅ Configuration stored in appsettings.json with hot-reload support
9. ✅ Notification retry queue with exponential backoff (3 attempts: 1s, 2s, 4s)
10. ✅ Rate limiting: 100 req/min (balance/history), 10 req/min (leaderboards)

---

## Phase 1: Data Model & Contracts ✅ COMPLETE

**Status**: All design artifacts generated and gaps filled.

**Deliverables**:
- ✅ [data-model.md](./data-model.md) - Complete entity designs
- ✅ [contracts/api-contracts.md](./contracts/api-contracts.md) - RESTful API specification
- ✅ [quickstart.md](./quickstart.md) - Developer implementation guide
- ✅ [spec.md](./spec.md) - Updated with configuration values, NFRs, and edge cases
- ✅ [checklists/requirements-quality.md](./checklists/requirements-quality.md) - 100-item quality checklist

**Entities**: 7 total (RewardTransaction, User, UserStreak, AchievementDefinition, UserAchievement, ReferralAttribution, LeaderboardSnapshot)

**API Endpoints**: 12 total (balance, transactions, leaderboard, rank, achievements, streak, referrals, admin)

**Configuration Defined**:
- Point values: Lesson (50), Task (100), Course (500), Referral (200)
- Streak bonuses: Base (5), Day 5 (+10), Day 10 (+25), Day 30 (+100), Day 100 (+500)
- 8 MVP achievements with unlock criteria and point bonuses
- Tie-breaking rule: Earlier registration timestamp (User.CreatedAt ASC)
- Pagination: Default 20, Max 100, Cursor-based with Base64 JSON format
- Notification thresholds: ≥100 points, streak milestones, all achievements

**Gap Resolution Summary** (from requirements-quality.md checklist):
- ✅ CHK001-CHK003: Point values and achievement definitions fully specified
- ✅ CHK008, CHK015: Tie-breaking rule explicitly defined (CreatedAt ASC)
- ✅ CHK011: "Significant reward event" quantified (≥100 points, milestones)
- ✅ CHK017: Admin justification format specified (min 10 chars, stored in description)
- ✅ CHK034: Rate limiting requirements added (NFR-011)
- ✅ CHK038: Response time SLAs defined (NFR-001 through NFR-004)
- ✅ CHK047: Achievement unlock failure rollback specified (FR-015)
- ✅ CHK051-CHK055: Performance requirements fully quantified (NFR-001 to NFR-004)
- ✅ CHK060: Admin adjustment audit trail specified (FR-009)
- ✅ CHK063-CHK066: Observability requirements added (NFR-012 to NFR-015)
- ✅ CHK067-CHK068: Idempotency and rollback requirements added (FR-014, FR-015)
- ✅ CHK069: External service failure handling specified (notification retry edge case)
- ✅ CHK095: Cache vs real-time conflict resolved (leaderboard cached, My Rank real-time)
- ✅ CHK097: LeaderboardSnapshot scope clarified (future optimization, not MVP)
- ✅ CHK098: RewardBalance vs User.CurrentPoints relationship documented (denormalized fields)
- ✅ CHK099: Configuration storage defined (appsettings.json with hot-reload)

**Remaining Low-Priority Gaps** (deferred to implementation phase):
- CHK029: Data retention/archival policy (define after MVP launch based on actual volume)
- CHK032: LeaderboardSnapshot persistence strategy (future optimization)
- CHK056: Data volume projections (track post-MVP, partition at 1M records per NFR-006)
- CHK079-CHK083: Integration event payload specs (document during Phase 2 implementation)

---

## Phase 2: Implementation (Ready to Start)

Use `/speckit.tasks` command to break down into developer tasks.

**Estimated Timeline**: 36-42 hours

**Key Integration Points**:
1. Lesson completion → Award points
2. Task approval → Award points
3. Course completion → Award bonus
4. User login → Process streak
5. Referral completion → Award referrer

---

## Next Steps

1. ✅ **Gap Resolution**: COMPLETE - See [GAP_RESOLUTION_SUMMARY.md](./GAP_RESOLUTION_SUMMARY.md) for details (85% resolved, 15% deferred)
2. ⏭️ **Run `/speckit.tasks`**: Generate detailed implementation task breakdown
3. ⏭️ **Phase 2 Implementation**: Start with Database & Entities following [quickstart.md](./quickstart.md)
4. ⏭️ **Document Integration Events**: During Phase 2 (Integration & Events)
5. ⏭️ **Monitor Performance**: Post-MVP to determine need for LeaderboardSnapshot optimization

---

**Status**: Planning complete with all critical gaps resolved. Ready for implementation.  
**Branch**: `006-reward-system`  
**Last Updated**: 2025-12-04
