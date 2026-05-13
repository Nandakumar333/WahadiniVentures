# Implementation Plan: Point-Based Discount Redemption System

**Branch**: `007-discount-redemption` | **Date**: December 5, 2025 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/007-discount-redemption/spec.md`

## Summary

Implement a point-based discount redemption system where users can exchange earned reward points for subscription discount codes (Stripe Promotion Codes). The system integrates with the existing rewards infrastructure, requires atomic transactions for point deduction and code issuance, implements optimistic concurrency control to prevent double-spending, and provides both user-facing redemption interfaces and admin management capabilities. The technical approach follows Clean Architecture with domain-driven design, CQRS pattern via MediatR, Entity Framework Core with PostgreSQL for persistence, and React 18 with TypeScript for the frontend.

## Technical Context

**Backend**
- **Language/Version**: .NET 8 C# with ASP.NET Core Web API
- **Primary Dependencies**: Entity Framework Core 8.0, AutoMapper, FluentValidation, MediatR, Stripe SDK
- **Storage**: PostgreSQL 15+ with JSONB support, time-based partitioning for transactions
- **Testing**: xUnit, Moq, FluentAssertions, Integration tests with TestContainers
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, Presentation layers)

**Frontend**
- **Language/Version**: TypeScript 4.9+ with React 18
- **Primary Dependencies**: Vite, React Router 7, React Query 5, Zustand, React Hook Form with Zod, TailwindCSS 3.4
- **Testing**: Vitest, React Testing Library, Playwright for E2E
- **Architecture**: Component-based with feature folders, custom hooks for business logic

**Integration**
- **External Services**: Stripe API (Promotion Codes validation)
- **Authentication**: JWT Bearer Tokens with role-based authorization
- **Project Type**: Full-stack web application (separate backend/frontend)

**Performance Goals**
- Redemption transaction completion: <5 seconds for 95% of requests
- Concurrent redemption handling: 50+ simultaneous requests without double-spending
- Gallery page load: <2 seconds with 1000+ discount types
- Point balance update latency: <500ms after redemption

**Constraints**
- ACID transactions required for all point deductions
- Optimistic concurrency control mandatory (prevent race conditions)
- Admin endpoints require RequireAdmin authorization policy
- Stripe Promotion Codes must be pre-created (no programmatic creation)
- All redemptions must create audit trail (negative RewardTransaction)

**Scale/Scope**
- Expected users: 10,000+ active users
- Discount types: 100+ concurrent campaigns
- Redemption volume: 1,000+ redemptions per day
- Audit retention: Permanent storage of all transactions

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Design Evaluation

**✅ Clean Architecture Compliance**
- Feature follows established Clean Architecture layers
- Domain entities are rich with business logic
- Infrastructure dependencies are abstracted via interfaces
- No direct dependencies from domain to infrastructure

**✅ Security Requirements**
- Admin endpoints will use [Authorize(Policy = "RequireAdmin")]
- JWT authentication required for all redemption operations
- Input validation at both API and domain levels via FluentValidation
- Audit trail mandatory for all point deductions

**✅ Testing Standards**
- Unit tests required for domain logic and service layer
- Integration tests required for API endpoints and database operations
- Test coverage target: >80% for critical redemption logic
- Concurrency tests required for race condition scenarios

**✅ Performance Standards**
- Async/await pattern for all I/O operations
- AsNoTracking for read-only discount queries
- Indexed database columns for redemption lookups
- React Query caching for frontend discount list

**✅ SOLID Principles**
- Single Responsibility: Each service handles one domain concern
- Open/Closed: Extension via interfaces, not modification
- Liskov Substitution: Repository pattern abstractions
- Interface Segregation: Specific repository interfaces per aggregate
- Dependency Inversion: Depend on abstractions (IDiscountService, IRewardService)

### Post-Design Re-Check

**✅ All Pre-Design Gates Passed - Design Review Complete**

**Phase 1 Deliverables Completed**:
- ✅ `research.md`: All technical decisions documented with rationale
- ✅ `data-model.md`: Complete entity design with EF Core configurations
- ✅ `contracts/user-endpoints.yaml`: User API specification (OpenAPI 3.0)
- ✅ `contracts/admin-endpoints.yaml`: Admin API specification (OpenAPI 3.0)
- ✅ `quickstart.md`: Developer onboarding guide with setup instructions
- ✅ Agent context updated (GitHub Copilot instructions)

**Design Quality Assessment**:
- ✅ Domain entities contain business logic (not anemic)
- ✅ Optimistic concurrency strategy documented and validated
- ✅ Transaction boundaries properly defined
- ✅ API contracts follow RESTful conventions
- ✅ Security considerations addressed (admin authorization, audit trails)
- ✅ Performance optimizations planned (indexes, AsNoTracking queries)
- ✅ Error handling patterns defined

**Architecture Compliance**:
- ✅ Clean Architecture layers properly separated
- ✅ CQRS pattern applied via MediatR
- ✅ Repository pattern abstracts data access
- ✅ Domain events and factory methods implemented
- ✅ DTOs separate API contracts from domain entities

**Ready for Phase 2**: Implementation can now proceed with `/speckit.tasks` to break down implementation into actionable tasks.

## Project Structure

### Documentation (this feature)

```text
specs/007-discount-redemption/
├── plan.md              # This file
├── research.md          # Phase 0: Technical decisions and patterns
├── data-model.md        # Phase 1: Entity design and relationships
├── quickstart.md        # Phase 1: Developer onboarding guide
├── contracts/           # Phase 1: API contracts (OpenAPI)
│   ├── user-endpoints.yaml
│   └── admin-endpoints.yaml
├── checklists/
│   └── requirements.md  # Specification quality validation
└── spec.md              # Feature specification (already created)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── WahadiniCryptoQuest.Core/              # Domain Layer
│   │   ├── Entities/
│   │   │   ├── DiscountType.cs                # NEW: Discount campaign configuration
│   │   │   ├── UserDiscountRedemption.cs      # NEW: User redemption record
│   │   │   └── RewardTransaction.cs           # EXISTING: Updated for redemptions
│   │   ├── Interfaces/
│   │   │   ├── Repositories/
│   │   │   │   ├── IDiscountRepository.cs     # NEW
│   │   │   │   └── IRewardRepository.cs       # EXISTING
│   │   │   └── Services/
│   │   │       ├── IDiscountService.cs        # NEW
│   │   │       └── IRewardService.cs          # EXISTING: Extended
│   │   └── DTOs/
│   │       └── Discount/                      # NEW
│   │           ├── DiscountTypeDto.cs
│   │           ├── RedeemDiscountRequestDto.cs
│   │           ├── RedemptionResponseDto.cs
│   │           ├── CreateDiscountTypeDto.cs
│   │           └── DiscountAnalyticsDto.cs
│   │
│   ├── WahadiniCryptoQuest.Service/           # Application Layer
│   │   ├── Services/
│   │   │   ├── DiscountService.cs             # NEW: Redemption orchestration
│   │   │   └── RewardService.cs               # EXISTING: Updated for deductions
│   │   ├── Commands/
│   │   │   ├── RedeemDiscountCommand.cs       # NEW: CQRS command
│   │   │   └── CreateDiscountTypeCommand.cs   # NEW: Admin command
│   │   ├── Queries/
│   │   │   ├── GetAvailableDiscountsQuery.cs  # NEW
│   │   │   └── GetUserRedemptionsQuery.cs     # NEW
│   │   └── Validators/
│   │       └── Discount/                      # NEW
│   │           ├── RedeemDiscountValidator.cs
│   │           └── CreateDiscountTypeValidator.cs
│   │
│   ├── WahadiniCryptoQuest.DAL/               # Infrastructure Layer
│   │   ├── Data/
│   │   │   └── ApplicationDbContext.cs        # UPDATED: Add DbSets
│   │   ├── Repositories/
│   │   │   └── DiscountRepository.cs          # NEW
│   │   ├── Configurations/
│   │   │   ├── DiscountTypeConfiguration.cs   # NEW: EF Core configuration
│   │   │   └── UserDiscountRedemptionConfiguration.cs # NEW
│   │   └── Migrations/
│   │       └── [Timestamp]_AddDiscountSystem.cs # NEW
│   │
│   └── WahadiniCryptoQuest.API/               # Presentation Layer
│       ├── Controllers/
│       │   └── DiscountController.cs          # NEW: User + Admin endpoints
│       └── Extensions/
│           └── DependencyInjection/
│               └── DiscountServiceExtensions.cs # NEW: DI registration
│
└── tests/
    ├── WahadiniCryptoQuest.Core.Tests/
    │   └── Services/
    │       └── DiscountServiceTests.cs        # NEW: Unit tests
    ├── WahadiniCryptoQuest.Service.Tests/
    │   ├── Commands/
    │   │   └── RedeemDiscountCommandTests.cs  # NEW
    │   └── Validators/
    │       └── RedeemDiscountValidatorTests.cs # NEW
    └── WahadiniCryptoQuest.API.Tests/
        └── Controllers/
            └── DiscountControllerTests.cs     # NEW: Integration tests

frontend/
├── src/
│   ├── components/
│   │   ├── discount/                          # NEW: Discount components
│   │   │   ├── DiscountCard.tsx              # Displays single discount offer
│   │   │   ├── DiscountList.tsx              # Grid of available discounts
│   │   │   ├── RedemptionModal.tsx           # Confirmation and success modal
│   │   │   ├── MyDiscountsView.tsx           # User's redeemed codes
│   │   │   └── index.ts
│   │   ├── admin/
│   │   │   ├── DiscountManagement.tsx        # NEW: Admin CRUD interface
│   │   │   ├── DiscountForm.tsx              # NEW: Create/Edit form
│   │   │   ├── DiscountAnalytics.tsx         # NEW: Stats dashboard
│   │   │   └── index.ts (updated)
│   │   └── ui/                               # EXISTING: Reuse components
│   │       ├── button.tsx
│   │       ├── card.tsx
│   │       ├── dialog.tsx
│   │       └── badge.tsx
│   │
│   ├── pages/
│   │   ├── discount/                          # NEW
│   │   │   ├── DiscountsPage.tsx             # User-facing gallery
│   │   │   └── MyDiscountsPage.tsx           # User's wallet
│   │   └── admin/
│   │       └── AdminDiscountsPage.tsx        # NEW: Admin interface
│   │
│   ├── hooks/
│   │   └── discount/                          # NEW
│   │       ├── useDiscounts.ts               # Fetch available discounts
│   │       ├── useRedeemDiscount.ts          # Redemption mutation
│   │       ├── useMyRedemptions.ts           # User's redemptions
│   │       └── index.ts
│   │
│   ├── services/
│   │   └── api/
│   │       └── discount.service.ts           # NEW: API client
│   │
│   ├── types/
│   │   └── discount.types.ts                 # NEW: TypeScript types
│   │
│   └── store/
│       ├── reward.store.ts                   # UPDATED: Sync points after redemption
│       └── discount.store.ts                 # NEW: Discount state (optional)
│
└── tests/
    └── discount/                              # NEW
        ├── DiscountCard.test.tsx
        ├── RedemptionModal.test.tsx
        └── useRedeemDiscount.test.ts
```

**Structure Decision**: Full-stack web application following Clean Architecture on backend and component-based architecture on frontend. Backend uses layered approach (Core → Service → DAL → API) with CQRS via MediatR. Frontend uses feature-based component structure with custom hooks for business logic and React Query for server state.

## Complexity Tracking

> No constitutional violations requiring justification.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
