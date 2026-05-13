# Implementation Plan: Stripe Subscription Management

**Branch**: `008-stripe-subscription` | **Date**: 2025-12-09 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/008-stripe-subscription/spec.md`

**Note**: This plan follows Clean Architecture principles with multi-currency support, Stripe integration, and comprehensive webhook handling.

## Summary

Implement a comprehensive subscription system with three tiers (Free, Monthly Premium, Yearly Premium) supporting multiple currencies (USD, INR, EUR, JPY, GBP) with admin-configurable pricing. Users can subscribe via Stripe Checkout, apply reward-based discount codes, and manage subscriptions through Stripe's hosted billing portal. The system handles automatic subscription lifecycle management through webhooks including payment failures, renewals, and cancellations while maintaining synchronization between Stripe and the local database.

## Technical Context

**Language/Version**: .NET 8 C# (backend), TypeScript 4.9+ with React 18 (frontend)

**Primary Dependencies**: 
- Backend: ASP.NET Core Web API, Entity Framework Core 8.0, Stripe.net 43.0.0, AutoMapper, FluentValidation, MediatR, Serilog
- Frontend: React 18, Vite, TailwindCSS 3.4, @stripe/stripe-js, React Query 5, Zustand, React Hook Form 7, Zod

**Storage**: PostgreSQL 15+ with JSONB support, Entity Framework Core with code-first migrations, time-based partitioning for webhook events

**Testing**: xUnit (backend), Vitest + React Testing Library (frontend), Stripe CLI for webhook testing

**Target Platform**: Web application (ASP.NET Core API + React SPA), Dockerized containers

**Project Type**: Web (backend + frontend architecture)

**Performance Goals**: 
- Checkout session creation < 2 seconds for 95% of requests
- Webhook processing latency < 5 seconds for 99% of events
- Currency pricing lookup < 100ms with caching
- Support 1000+ concurrent checkout sessions

**Constraints**: 
- PCI compliance: Never store credit card data locally
- Webhook signature verification mandatory (HMAC-SHA256)
- Idempotency required for all webhook processing
- 3-day grace period for payment failures before downgrade
- Multi-currency support with admin-configurable pricing
- Stripe API rate limits (100 req/s read, 25 req/s write)

**Scale/Scope**: 
- Expected 10,000+ active subscriptions at launch
- Support 5 currencies initially (USD, INR, EUR, JPY, GBP), expandable to 20+
- ~500 webhook events per day initially
- Admin interface for 5-10 administrators
- Integration with existing Rewards system (discount codes)

**Currency Detection Strategy**: 
- Primary: Browser locale detection (navigator.language)
- Optional future enhancement: IP geolocation service integration
- Manual override always available via currency selector

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Status**: ✅ PASS (No constitution violations - template is placeholder only)

**Notes**: 
- Feature follows Clean Architecture principles as required
- All external dependencies (Stripe) properly isolated in Infrastructure layer
- Test coverage will be maintained above 80% threshold
- Security best practices enforced (webhook signature verification, no PCI data storage)
- Performance requirements clearly defined and achievable

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── WahadiniCryptoQuest.Core/              # Domain Layer
│   │   ├── Entities/
│   │   │   ├── Subscription.cs                # Aggregate root for subscription
│   │   │   ├── CurrencyPricing.cs            # Currency configuration entity
│   │   │   ├── WebhookEvent.cs               # Webhook audit entity
│   │   │   └── SubscriptionHistory.cs         # Subscription lifecycle tracking
│   │   ├── Enums/
│   │   │   ├── SubscriptionTier.cs
│   │   │   ├── SubscriptionStatus.cs
│   │   │   └── WebhookProcessingStatus.cs
│   │   ├── Events/
│   │   │   ├── SubscriptionCreatedEvent.cs
│   │   │   ├── SubscriptionCancelledEvent.cs
│   │   │   └── CurrencyPricingUpdatedEvent.cs
│   │   └── DTOs/
│   │       ├── Subscription/
│   │       └── Currency/
│   │
│   ├── WahadiniCryptoQuest.Service/            # Application Layer
│   │   ├── Services/
│   │   │   ├── SubscriptionService.cs          # Business logic orchestration
│   │   │   ├── CurrencyService.cs              # Currency management
│   │   │   └── WebhookProcessingService.cs     # Webhook event processing
│   │   ├── Interfaces/
│   │   │   ├── ISubscriptionService.cs
│   │   │   ├── IPaymentGateway.cs              # Abstraction for Stripe
│   │   │   └── ICurrencyService.cs
│   │   ├── Commands/
│   │   │   ├── CreateCheckoutSessionCommand.cs
│   │   │   ├── CancelSubscriptionCommand.cs
│   │   │   └── UpdateCurrencyPricingCommand.cs
│   │   ├── Queries/
│   │   │   ├── GetSubscriptionStatusQuery.cs
│   │   │   └── GetCurrencyPricingQuery.cs
│   │   ├── Handlers/
│   │   │   ├── CommandHandlers/
│   │   │   └── QueryHandlers/
│   │   ├── Validators/
│   │   │   ├── CreateCheckoutSessionValidator.cs
│   │   │   └── CurrencyPricingValidator.cs
│   │   └── Mappings/
│   │       └── SubscriptionMappingProfile.cs
│   │
│   ├── WahadiniCryptoQuest.DAL/                # Infrastructure Layer
│   │   ├── Data/
│   │   │   └── ApplicationDbContext.cs
│   │   ├── Repositories/
│   │   │   ├── SubscriptionRepository.cs
│   │   │   ├── CurrencyPricingRepository.cs
│   │   │   └── WebhookEventRepository.cs
│   │   ├── Configurations/
│   │   │   ├── SubscriptionConfiguration.cs
│   │   │   ├── CurrencyPricingConfiguration.cs
│   │   │   └── WebhookEventConfiguration.cs
│   │   ├── Migrations/
│   │   │   └── {timestamp}_AddStripeSubscription.cs
│   │   └── Infrastructure/
│   │       ├── StripePaymentGateway.cs         # Stripe SDK wrapper
│   │       └── CurrencyDetectionService.cs     # IP/locale detection
│   │
│   └── WahadiniCryptoQuest.API/                # Presentation Layer
│       ├── Controllers/
│       │   ├── SubscriptionsController.cs      # User-facing endpoints
│       │   ├── WebhooksController.cs           # Stripe webhook receiver
│       │   └── Admin/
│       │       └── CurrencyPricingController.cs # Admin endpoints
│       ├── Middleware/
│       │   └── StripeWebhookMiddleware.cs      # Signature verification
│       ├── Validators/
│       │   └── CheckoutRequestValidator.cs
│       └── Extensions/
│           └── SubscriptionServicesExtensions.cs

frontend/
├── src/
│   ├── components/
│   │   └── subscription/                       # Subscription feature components
│   │       ├── PricingCard/
│   │       │   ├── PricingCard.tsx
│   │       │   ├── PricingCard.test.tsx
│   │       │   └── index.ts
│   │       ├── PlanComparison/
│   │       │   ├── PlanComparison.tsx
│   │       │   └── index.ts
│   │       ├── SubscriptionStatus/
│   │       │   ├── SubscriptionStatus.tsx
│   │       │   └── index.ts
│   │       ├── CurrencySelector/
│   │       │   ├── CurrencySelector.tsx
│   │       │   └── index.ts
│   │       └── DiscountCodeInput/
│   │           ├── DiscountCodeInput.tsx
│   │           └── index.ts
│   │
│   ├── pages/
│   │   └── subscription/
│   │       ├── PricingPage.tsx                 # Public pricing page
│   │       ├── CheckoutSuccessPage.tsx
│   │       ├── CheckoutCancelPage.tsx
│   │       └── ManageSubscriptionPage.tsx      # Portal redirect
│   │
│   ├── hooks/
│   │   └── subscription/
│   │       ├── useSubscription.ts              # Subscription status
│   │       ├── useCreateCheckout.ts           # Checkout session
│   │       ├── useCurrencyPricing.ts          # Currency data
│   │       └── useDiscountValidation.ts        # Discount codes
│   │
│   ├── services/
│   │   └── api/
│   │       ├── subscriptionService.ts          # API calls
│   │       └── currencyService.ts
│   │
│   ├── types/
│   │   └── subscription.types.ts               # TypeScript interfaces
│   │
│   └── utils/
│       └── currency/
│           ├── formatCurrency.ts               # Currency formatting
│           └── detectCurrency.ts               # Client-side detection

tests/
├── backend/
│   ├── WahadiniCryptoQuest.Core.Tests/
│   │   └── Entities/
│   │       ├── SubscriptionTests.cs            # Domain logic tests
│   │       └── CurrencyPricingTests.cs
│   ├── WahadiniCryptoQuest.Service.Tests/
│   │   ├── Services/
│   │   │   └── SubscriptionServiceTests.cs
│   │   └── Handlers/
│   │       └── CreateCheckoutCommandHandlerTests.cs
│   └── WahadiniCryptoQuest.API.Tests/
│       ├── Controllers/
│       │   ├── SubscriptionsControllerTests.cs
│       │   └── WebhooksControllerTests.cs
│       └── Integration/
│           └── StripeIntegrationTests.cs
│
└── frontend/
    ├── components/
    │   └── subscription/
    │       └── PricingCard.test.tsx
    └── e2e/
        └── checkout-flow.spec.ts               # Playwright E2E tests
```

**Structure Decision**: Web application structure with backend (.NET 8) and frontend (React 18) following Clean Architecture principles. Backend organized into four layers (Domain, Application, Infrastructure, Presentation) with clear separation of concerns. Stripe-specific code isolated in Infrastructure layer for easy testing and potential provider swapping.

## Complexity Tracking

**No violations** - Feature follows existing architecture patterns without introducing complexity violations.

---

## Clarifications Applied from Specification

The following clarifications from `/speckit.clarify` session (2025-12-09) have been integrated into this implementation plan:

1. **Webhook Processing Failures**: When webhook processing fails internally (database timeout, network issue), system returns 500 error to trigger Stripe's automatic retry mechanism (up to 3 days), with failures logged for manual review if retries exhausted.

2. **Concurrent Checkout Prevention**: Before creating checkout session, system checks for existing active subscription and returns error if one exists to prevent duplicate subscriptions.

3. **Admin Currency Pricing Validation**: Currency pricing must be non-negative, cannot exceed $9999 per currency, with warnings displayed when prices differ by >50% from USD baseline.

4. **Discount Code Abandonment**: When user applies discount code but abandons checkout, code is temporarily reserved for 24 hours then automatically released to unused status if no payment succeeds.

5. **Grace Period Status Display**: During 3-day grace period after payment failure, system displays "Active (Payment Issue)" status with warning banner prompting payment update while maintaining full premium access.

6. **Grace Period Duration**: 3-day grace period before downgrade to Free tier (aligns with Stripe Smart Retries).

7. **Mid-Cycle Plan Changes**: Allow immediate plan upgrades/downgrades with proration handled by Stripe default settings.

---

## ✅ Pre-Implementation Phase
### 🎯 Phase 0: Research & Discovery
**Goal**: Resolve all NEEDS CLARIFICATION markers and establish technical foundation
**Duration**: ~4-6 hours
**Effort Type**: Discovery • Must complete before Phase 1

#### 🎯 Objectives
- [x] Resolve FR-021: Payment failure grace period (Decision: 3-day grace period)
- [x] Resolve FR-022: Mid-cycle plan change policy (Decision: Allow immediate changes with proration)
- [x] Research Stripe multi-currency pricing architecture
- [x] Research discount code integration patterns
- [x] Research webhook security and idempotency
- [x] Document Clean Architecture approach for payment gateway

#### 📋 Tasks
- [x] **Research**: Grace period best practices → **Deliverable**: `research.md` section with decision
- [x] **Research**: Proration strategies → **Deliverable**: `research.md` section with decision
- [x] **Research**: Stripe Prices API for multi-currency → **Deliverable**: Implementation pattern
- [x] **Research**: Stripe Promotion Codes vs. custom discount logic → **Deliverable**: Implementation pattern
- [x] **Research**: Webhook signature verification → **Deliverable**: Security implementation guide
- [x] **Document**: Clean Architecture layers for subscription domain → **Deliverable**: `research.md` architecture section

#### 📦 Deliverables
- [x] `research.md` with all technical decisions documented (8,186 lines)

#### ✅ Validation Criteria
- [x] All NEEDS CLARIFICATION markers resolved with documented decisions
- [x] Technical approach validated against Stripe best practices
- [x] Architecture aligns with WahadiniCryptoQuest Clean Architecture standards

---

### 🎯 Phase 1: Architecture & Contracts
**Goal**: Define domain model, API contracts, and developer setup guide
**Duration**: ~8-10 hours
**Effort Type**: Design • Prepares for implementation

#### 🎯 Objectives
- [x] Design domain entities following Clean Architecture and DDD principles
- [x] Define RESTful API contracts with comprehensive DTOs
- [x] Create developer quickstart guide for Stripe integration
- [x] Update implementation plan with project structure and detailed phases

#### 📋 Tasks
- [x] **Design**: Domain entities (Subscription, CurrencyPricing, WebhookEvent, SubscriptionHistory) → **Deliverable**: `data-model.md`
- [x] **Design**: Factory methods, business logic, domain events → **Deliverable**: Domain methods in `data-model.md`
- [x] **Design**: PostgreSQL schema with indexes and constraints → **Deliverable**: DDL scripts in `data-model.md`
- [x] **Design**: RESTful API endpoints (10 endpoints) → **Deliverable**: `contracts/api-endpoints.md`
- [x] **Design**: Request/response DTOs with validation rules → **Deliverable**: DTOs in `contracts/api-endpoints.md`
- [x] **Design**: Error handling patterns → **Deliverable**: Error responses in `contracts/api-endpoints.md`
- [x] **Document**: Stripe Dashboard setup steps → **Deliverable**: `quickstart.md` Step 1
- [x] **Document**: Backend environment configuration → **Deliverable**: `quickstart.md` Step 2
- [x] **Document**: Frontend Stripe.js setup → **Deliverable**: `quickstart.md` Step 3
- [x] **Document**: Webhook CLI testing → **Deliverable**: `quickstart.md` Step 4
- [x] **Document**: Verification tests → **Deliverable**: `quickstart.md` Step 5
- [x] **Plan**: Update implementation plan with project structure → **Deliverable**: `plan.md` Project Structure section
- [x] **Plan**: Define detailed implementation phases → **Deliverable**: `plan.md` Phase 2-8 sections

#### 📦 Deliverables
- [x] `data-model.md` with domain entities (6,164 lines)
- [x] `contracts/api-endpoints.md` with API specifications (4,962 lines)
- [x] `quickstart.md` with setup guide (2,934 lines)
- [x] `plan.md` with complete implementation plan

#### ✅ Validation Criteria
- [x] Domain model follows Clean Architecture (private setters, factory methods, domain events)
- [x] API contracts include all HTTP methods, authentication, error handling
- [x] Quickstart guide executable by developer (30-45 minute setup time)
- [x] Implementation plan includes project structure, 8 detailed phases, tasks, deliverables

---

## 🚀 Implementation Phases
### 🎯 Phase 2: Stripe Infrastructure Setup
**Goal**: Configure Stripe SDK, database schema, and payment gateway abstraction
**Duration**: ~12-16 hours
**Effort Type**: Backend Infrastructure

#### 🎯 Objectives
- Configure Stripe Products and Prices in Stripe Dashboard for all currencies
- Implement database migrations for subscription tables
- Build `IPaymentGateway` abstraction and `StripePaymentGateway` implementation
- Set up environment configuration for Stripe API keys

#### 📋 Tasks
1. **Stripe Dashboard Configuration** (~2 hours)
   - Create Products: "Premium Monthly" and "Premium Yearly"
   - Create Prices for each currency (USD, INR, EUR, JPY, GBP) per product
   - Configure webhook endpoints (development + production URLs)
   - Generate API keys (publishable + secret) and webhook signing secret
   - **Deliverable**: Stripe Dashboard configured with Price IDs documented

2. **Database Migration** (~3 hours)
   - Create migration: `AddStripeSubscription`
   - Implement `Subscriptions` table with all columns from `data-model.md`
   - Implement `CurrencyPricings` table with admin-configurable pricing
   - Implement `WebhookEvents` table for idempotency tracking
   - Implement `SubscriptionHistory` table for audit trail
   - Add indexes: `idx_subscriptions_userid`, `idx_subscriptions_stripecustomerid`, `idx_webhookevents_stripeeventid`
   - **Deliverable**: Migration applied successfully to dev database

3. **EF Core Configuration** (~2 hours)
   - Implement `SubscriptionConfiguration.cs` with entity mapping
   - Implement `CurrencyPricingConfiguration.cs` with precision settings
   - Implement `WebhookEventConfiguration.cs` with JSONB column
   - Configure relationships and foreign keys
   - **Deliverable**: EF Core configurations in `WahadiniCryptoQuest.DAL/Configurations/`

4. **Payment Gateway Abstraction** (~4 hours)
   - Define `IPaymentGateway` interface in `WahadiniCryptoQuest.Service/Interfaces/`
   - Methods: `CreateCheckoutSessionAsync()`, `CreatePortalSessionAsync()`, `GetSubscriptionAsync()`, `CancelSubscriptionAsync()`
   - Implement `StripePaymentGateway.cs` in `WahadiniCryptoQuest.DAL/Infrastructure/`
   - Install `Stripe.net` NuGet package (v43.0.0+)
   - Implement Stripe SDK calls with error handling
   - **Deliverable**: Payment gateway abstraction ready for DI registration

5. **Environment Configuration** (~1 hour)
   - Add Stripe configuration section in `appsettings.json`
   - Properties: `PublishableKey`, `SecretKey`, `WebhookSecret`, `SuccessUrl`, `CancelUrl`
   - Add environment variables for production secrets
   - **Deliverable**: Configuration ready for local development + production

6. **Dependency Injection Setup** (~1 hour)
   - Register `IPaymentGateway` → `StripePaymentGateway` in `Program.cs`
   - Configure Stripe client with secret key
   - **Deliverable**: Services registered and ready for injection

7. **Unit Tests** (~3 hours)
   - Test `StripePaymentGateway` with mocked Stripe SDK
   - Test error handling (network failures, API errors)
   - **Deliverable**: 80%+ code coverage for payment gateway

#### 📦 Deliverables
- Stripe Dashboard configured with multi-currency pricing
- Database migration applied with 4 new tables
- `IPaymentGateway` interface and `StripePaymentGateway` implementation
- Unit tests for payment gateway

#### ✅ Validation Criteria
- Migration runs successfully without errors
- Stripe API calls succeed with test keys
- Payment gateway tests pass with 80%+ coverage
- Environment configuration validated in local dev environment

---

### 🎯 Phase 3: Subscription Business Logic & Checkout
**Goal**: Implement subscription service, checkout flow, and discount validation
**Duration**: ~16-20 hours
**Effort Type**: Backend Application Layer

#### 🎯 Objectives
- Implement `SubscriptionService` with business logic
- Build checkout session creation with discount code support
- Implement subscription status queries
- Add FluentValidation for request DTOs

#### 📋 Tasks
1. **Subscription Service Core** (~5 hours)
   - Implement `SubscriptionService.cs` in `WahadiniCryptoQuest.Service/Services/`
   - Methods: `GetSubscriptionStatusAsync()`, `CreateCheckoutSessionAsync()`, `CancelSubscriptionAsync()`, `ReactivateSubscriptionAsync()`
   - Inject `IPaymentGateway`, `IRepository<Subscription>`, `IRepository<CurrencyPricing>`
   - **Deliverable**: Business logic layer complete

2. **Checkout Session Logic** (~4 hours)
   - Implement `CreateCheckoutSessionCommand` and handler
   - **Concurrent Prevention**: Check for existing active subscription before session creation, return error if exists
   - Logic: Validate currency, fetch pricing, apply discount code (if provided), create Stripe session
   - Return: `CheckoutSessionDto` with `sessionId` and `checkoutUrl`
   - Error handling: Invalid currency, inactive pricing, invalid discount code, existing subscription
   - **Deliverable**: Checkout session creation working with duplicate prevention

3. **Discount Code Validation** (~3 hours)
   - Implement `ValidateDiscountCodeAsync()` in `SubscriptionService`
   - Check Stripe Promotion Code validity via Stripe API
   - **Code Reservation**: Implement 24-hour temporary reservation when code applied to checkout
   - **Auto-Release**: Background job to release reserved codes after 24 hours if no successful payment
   - Return: `DiscountValidationDto` with percentage/amount off
   - **Deliverable**: Discount validation endpoint working with reservation logic

4. **Subscription Status Query** (~2 hours)
   - Implement `GetSubscriptionStatusQuery` and handler
   - Return: Current subscription tier, status, renewal date, cancellation date
   - Handle edge cases: No subscription (Free tier), past due status, canceled but active
   - **Deliverable**: Status query working for all user states

5. **FluentValidation Rules** (~2 hours)
   - Implement `CreateCheckoutSessionValidator` with rules: tier is valid, currency is valid
   - Implement `CurrencyPricingValidator` for admin updates: amount > 0, currency code valid
   - **Deliverable**: Validation rules enforced

6. **AutoMapper Profiles** (~1 hour)
   - Implement `SubscriptionMappingProfile.cs`
   - Map: `Subscription` → `SubscriptionStatusDto`, `CurrencyPricing` → `PricingDto`
   - **Deliverable**: DTOs mapped correctly

7. **Unit Tests** (~3 hours)
   - Test `SubscriptionService` with mocked repositories and payment gateway
   - Test discount code validation (valid/invalid/expired scenarios)
   - Test checkout session creation (with/without discount)
   - **Deliverable**: 80%+ code coverage for application layer

#### 📦 Deliverables
- `SubscriptionService` with all business logic methods
- Checkout session creation with discount support
- FluentValidation rules for request DTOs
- Unit tests for subscription service

#### ✅ Validation Criteria
- Checkout session returns valid `sessionId` from Stripe
- Discount codes validated correctly via Stripe API
- Subscription status query handles all user states (Free, Active, PastDue, Canceled)
- Unit tests pass with 80%+ coverage

---

### 🎯 Phase 4: Webhook Processing
**Goal**: Implement webhook receiver with signature verification and event processing
**Duration**: ~10-14 hours
**Effort Type**: Backend Infrastructure + Application Layer

#### 🎯 Objectives
- Build webhook endpoint with Stripe signature verification
- Process key events: `checkout.session.completed`, `invoice.payment_succeeded`, `invoice.payment_failed`, `customer.subscription.deleted`
- Implement idempotency to prevent duplicate processing
- Update subscription status based on webhook events

#### 📋 Tasks
1. **Webhook Controller** (~2 hours)
   - Implement `WebhooksController.cs` in `WahadiniCryptoQuest.API/Controllers/`
   - Endpoint: `POST /api/webhooks/stripe` with raw body access
   - **Deliverable**: Webhook endpoint configured

2. **Signature Verification** (~3 hours)
   - Implement `StripeWebhookMiddleware.cs` for signature verification
   - Use `EventUtility.ConstructEvent()` with webhook secret
   - Return 401 if signature invalid
   - **Deliverable**: Webhook security enforced

3. **Idempotency Check** (~2 hours)
   - Check if `StripeEventId` exists in `WebhookEvents` table
   - If exists: Return 200 OK (already processed)
   - If not: Insert event record with `Processing` status
   - **Deliverable**: Duplicate events prevented

4. **Event Processing Service** (~4 hours)
   - Implement `WebhookProcessingService.cs` in `WahadiniCryptoQuest.Service/Services/`
   - Handle `checkout.session.completed`: Create `Subscription` record with `Active` status, mark discount code as used
   - Handle `invoice.payment_succeeded`: Renew subscription, update `CurrentPeriodEnd`
   - Handle `invoice.payment_failed`: Mark subscription as `PastDue`, start 3-day grace period
   - Handle `customer.subscription.deleted`: Mark subscription as `Expired`
   - Update `WebhookEvent` status to `Completed` after processing
   - **Deliverable**: All webhook events processed correctly

5. **Error Handling** (~1 hour)
   - **Return 500 on Processing Failures**: When database errors, network issues, or exceptions occur during event processing
   - **Stripe Auto-Retry**: Triggers Stripe's automatic retry mechanism (up to 3 days)
   - Catch exceptions during event processing
   - Update `WebhookEvent` status to `Failed` with error message
   - Log errors with full context for manual investigation
   - Alert administrators if retries exhausted
   - **Deliverable**: Robust error handling with automatic retry support

6. **Integration Tests** (~2 hours)
   - Test webhook endpoint with Stripe CLI fixtures
   - Test signature verification (valid/invalid signatures)
   - Test idempotency (duplicate events)
   - Test event processing (all 4 event types)
   - **Deliverable**: Integration tests passing

#### 📦 Deliverables
- `WebhooksController` with signature verification
- `WebhookProcessingService` handling 4 key events
- Idempotency tracking with `WebhookEvents` table
- Integration tests for webhook processing

#### ✅ Validation Criteria
- Webhook signature verification works (test with Stripe CLI)
- Duplicate events are ignored (200 OK returned)
- Subscription status updated correctly for all event types
- Integration tests pass with real Stripe event fixtures

---

### 🎯 Phase 5: Frontend Pricing & Checkout Flow
**Goal**: Build pricing page with currency selection and checkout redirection
**Duration**: ~14-18 hours
**Effort Type**: Frontend Implementation

#### 🎯 Objectives
- Build responsive pricing page with `PricingCard` components
- Implement currency selector with automatic detection
- Build checkout flow (redirect to Stripe Checkout)
- Build success/cancel pages for post-checkout redirect
- Implement discount code input with validation

#### 📋 Tasks
1. **Pricing Page Layout** (~4 hours)
   - Implement `PricingPage.tsx` in `src/pages/subscription/`
   - Layout: 2 pricing cards side-by-side (Monthly, Yearly) with Free plan comparison table
   - Responsive: Stack vertically on mobile (<768px)
   - **Deliverable**: Pricing page layout complete

2. **Pricing Card Component** (~3 hours)
   - Implement `PricingCard.tsx` in `src/components/subscription/PricingCard/`
   - Props: `tier`, `price`, `currency`, `features`, `onSubscribe`
   - Display: Price with currency symbol, billing period, feature list, CTA button
   - Highlight: Most popular tier (Yearly) with badge
   - **Deliverable**: Reusable pricing card component

3. **Currency Selector** (~3 hours)
   - Implement `CurrencySelector.tsx` in `src/components/subscription/CurrencySelector/`
   - Fetch currencies: Use `useCurrencyPricing` hook calling `GET /api/subscriptions/plans?currency={code}`
   - Auto-detect: Use `navigator.language` to detect default currency
   - Dropdown: Show currency codes with flags (use emoji flags)
   - **Deliverable**: Currency selector working with auto-detection

4. **Checkout Flow** (~3 hours)
   - Implement `useCreateCheckout` hook in `src/hooks/subscription/`
   - Call `POST /api/subscriptions/checkout` with `tier`, `currency`, `discountCode`
   - Redirect to `checkoutUrl` from response using `window.location.href`
   - Loading state: Show spinner during session creation
   - Error handling: Display toast notification if checkout fails
   - **Deliverable**: Checkout flow redirecting to Stripe Checkout

5. **Discount Code Input** (~2 hours)
   - Implement `DiscountCodeInput.tsx` in `src/components/subscription/DiscountCodeInput/`
   - Input field with "Apply" button
   - Validation: Call `POST /api/subscriptions/validate-discount` on blur
   - Display: Show discount percentage if valid, error message if invalid
   - Apply: Pass `discountCode` to checkout session creation
   - **Deliverable**: Discount code input working with validation

6. **Success/Cancel Pages** (~2 hours)
   - Implement `CheckoutSuccessPage.tsx` with success message and "Go to Dashboard" button
   - Implement `CheckoutCancelPage.tsx` with cancellation message and "Try Again" button
   - Configure URLs in Stripe Dashboard and `appsettings.json`
   - **Deliverable**: Post-checkout redirect pages

7. **Component Tests** (~1 hour)
   - Test `PricingCard` rendering with Vitest + React Testing Library
   - Test `CurrencySelector` with mocked API responses
   - Test `DiscountCodeInput` validation flow
   - **Deliverable**: Component tests passing

#### 📦 Deliverables
- `PricingPage` with responsive layout
- `PricingCard` component with currency formatting
- `CurrencySelector` with auto-detection
- Checkout flow redirecting to Stripe
- Discount code input with validation
- Success/cancel pages

#### ✅ Validation Criteria
- Pricing page displays correctly on desktop and mobile
- Currency selector changes pricing dynamically
- Checkout redirects to Stripe Checkout with correct pricing
- Discount codes validated before checkout
- Success/cancel pages displayed after checkout

---

### 🎯 Phase 6: Subscription Management UI
**Goal**: Build subscription status display, billing portal access, and cancellation flow
**Duration**: ~12-16 hours
**Effort Type**: Frontend Implementation

#### 🎯 Objectives
- Display current subscription status on user dashboard
- Implement billing portal redirect for subscription management
- Build cancellation flow with confirmation modal
- Add premium badges for UI features requiring subscription

#### 📋 Tasks
1. **Subscription Status Component** (~4 hours)
   - Implement `SubscriptionStatus.tsx` in `src/components/subscription/SubscriptionStatus/`
   - Fetch status: Use `useSubscription` hook calling `GET /api/subscriptions/status`
   - Display: Current tier, renewal date, cancellation date (if scheduled)
   - **Grace Period Display**: Show "Active (Payment Issue)" status with prominent warning banner when subscription in grace period
   - **Warning Banner**: "Payment failed. Please update your payment method to avoid losing access."
   - Conditional rendering: Show "Upgrade" button for Free tier, "Manage Subscription" for paid tiers
   - Maintain premium access during grace period
   - **Deliverable**: Subscription status display complete with grace period warnings

2. **Billing Portal Integration** (~2 hours)
   - Implement `useCreatePortal` hook in `src/hooks/subscription/`
   - Call `POST /api/subscriptions/portal` to get `portalUrl`
   - Redirect to Stripe Customer Portal using `window.location.href`
   - **Deliverable**: Billing portal redirect working

3. **Cancellation Flow** (~3 hours)
   - Implement cancellation button in `SubscriptionStatus` component
   - Show confirmation modal: "Are you sure? Your subscription will remain active until [date]"
   - Call `POST /api/subscriptions/cancel` on confirmation
   - Optimistic update: Show "Cancellation scheduled" message immediately
   - **Deliverable**: Cancellation flow with confirmation

4. **Reactivation Flow** (~2 hours)
   - Display "Reactivate" button if subscription is canceled but still active
   - Call `POST /api/subscriptions/reactivate` on click
   - Remove cancellation date from display
   - **Deliverable**: Reactivation flow working

5. **Premium Badges** (~2 hours)
   - Create `PremiumBadge.tsx` component with crown icon
   - Add badges to features requiring subscription (e.g., exclusive videos, advanced tasks)
   - Use `useSubscription` hook to check if user has active subscription
   - Disable features for Free tier with tooltip explaining upgrade
   - **Deliverable**: Premium badges displayed on protected features

6. **ManageSubscription Page** (~1 hour)
   - Implement `ManageSubscriptionPage.tsx` in `src/pages/subscription/`
   - Display: `SubscriptionStatus` component with "Manage Billing" and "Cancel Subscription" buttons
   - **Deliverable**: Dedicated subscription management page

7. **E2E Tests** (~2 hours)
   - Test full checkout flow with Playwright
   - Test cancellation flow with confirmation
   - Test reactivation flow
   - **Deliverable**: E2E tests passing

#### 📦 Deliverables
- `SubscriptionStatus` component displaying current subscription
- Billing portal redirect working
- Cancellation flow with confirmation modal
- Reactivation flow
- Premium badges on protected features
- `ManageSubscriptionPage` for subscription management
- E2E tests for subscription flows

#### ✅ Validation Criteria
- Subscription status displays correctly for all tiers (Free, Monthly, Yearly)
- Billing portal redirect works (test in Stripe test mode)
- Cancellation schedules subscription end without immediate termination
- Reactivation removes cancellation date
- Premium badges displayed on protected features
- E2E tests pass in CI/CD pipeline

---

### 🎯 Phase 7: Admin Currency Management
**Goal**: Build admin interface for managing multi-currency pricing
**Duration**: ~8-12 hours
**Effort Type**: Backend + Frontend Admin Features

#### 🎯 Objectives
- Implement admin API endpoints for currency CRUD operations
- Build admin UI for viewing and editing currency pricing
- Add currency activation/deactivation toggle
- Validate admin role authorization

#### 📋 Tasks
1. **Admin API Endpoints** (~4 hours)
   - Implement `CurrencyPricingController.cs` in `WahadiniCryptoQuest.API/Controllers/Admin/`
   - Endpoint: `GET /api/admin/subscriptions/currencies` (list all currencies)
   - Endpoint: `POST /api/admin/subscriptions/currencies` (create or update currency pricing)
   - Authorize: Require `Admin` role
   - **Deliverable**: Admin API endpoints working

2. **Currency Service Logic** (~2 hours)
   - Implement `CurrencyService.cs` in `WahadiniCryptoQuest.Service/Services/`
   - Methods: `GetAllCurrenciesAsync()`, `UpdateCurrencyPricingAsync()`, `ActivateCurrencyAsync()`, `DeactivateCurrencyAsync()`
   - **Validation Rules**: 
     * Amount must be non-negative (≥$0)
     * Maximum price: $9999 per currency
     * Currency code must be valid ISO 4217
     * **Warning System**: Flag prices differing >50% from USD baseline for admin review
   - **Deliverable**: Currency management business logic with validation

3. **Admin Currency Management Page** (~4 hours)
   - Implement `CurrencyManagementPage.tsx` in `src/pages/admin/subscription/`
   - Display: Table with currency code, monthly price, yearly price, active status
   - **Warning Indicators**: Show warning icon for currencies with >50% price deviation from USD baseline
   - Actions: Edit button (opens modal), toggle active status
   - Modal: Form with currency code, monthly price, yearly price, Stripe Price IDs
   - **Validation Messages**: Display warnings when prices exceed validation thresholds
   - **Deliverable**: Admin UI for currency management with validation feedback

4. **Authorization Tests** (~2 hours)
   - Test admin endpoints return 403 Forbidden for non-admin users
   - Test admin endpoints work for admin users
   - **Deliverable**: Authorization tests passing

#### 📦 Deliverables
- Admin API endpoints for currency management
- `CurrencyService` with business logic
- Admin UI for viewing and editing currencies
- Authorization tests for admin endpoints

#### ✅ Validation Criteria
- Admin API endpoints accessible only by admin users
- Currency pricing updates reflected immediately on pricing page
- Currency activation/deactivation works correctly
- Admin UI displays all currencies with correct pricing

---

### 🎯 Phase 8: Testing, Documentation & Deployment
**Goal**: Comprehensive testing, documentation updates, and production deployment
**Duration**: ~10-14 hours
**Effort Type**: Testing, Documentation, DevOps

#### 🎯 Objectives
- Achieve 80%+ code coverage for subscription feature
- Update user documentation with subscription management guide
- Configure production environment variables
- Deploy to production with Stripe live mode

#### 📋 Tasks
1. **Integration Tests** (~4 hours)
   - Test full subscription lifecycle: checkout → webhook → status query → cancellation → expiration
   - Test discount code application end-to-end
   - Test multi-currency pricing switching
   - Test grace period handling (payment failure → past due → recovery)
   - **Deliverable**: Integration tests covering all user journeys

2. **Performance Tests** (~2 hours)
   - Test webhook processing under load (100 events/second)
   - Test pricing page load time (<2s)
   - Test checkout session creation latency (<500ms)
   - **Deliverable**: Performance benchmarks documented

3. **Security Audit** (~2 hours)
   - Verify webhook signature verification
   - Check for PCI compliance (no card data stored)
   - Validate admin authorization on currency endpoints
   - Test discount code enumeration prevention
   - **Deliverable**: Security checklist completed

4. **User Documentation** (~2 hours)
   - Update `docs/subscription-management-guide.md` with:
     * How to subscribe (checkout flow)
     * How to manage subscription (billing portal)
     * How to cancel subscription
     * How to reactivate subscription
     * FAQ (payment methods, refunds, proration)
   - **Deliverable**: User guide published

5. **Production Configuration** (~2 hours)
   - Add production Stripe keys to environment variables (secure secrets management)
   - Configure production webhook endpoint URL in Stripe Dashboard
   - Set up production success/cancel URLs
   - Create production Stripe Products and Prices for all currencies
   - **Deliverable**: Production environment configured

6. **Deployment** (~2 hours)
   - Run database migrations in production
   - Deploy backend API with new endpoints
   - Deploy frontend with subscription pages
   - Verify webhook endpoint reachable from Stripe
   - Test production checkout flow with real payment methods
   - **Deliverable**: Feature live in production

#### 📦 Deliverables
- 80%+ code coverage for subscription feature
- User documentation published
- Production environment configured
- Feature deployed to production

#### ✅ Validation Criteria
- All tests pass (unit + integration + E2E)
- Code coverage ≥80% for subscription domain
- Production checkout completes successfully
- Webhook processing verified in production
- User documentation reviewed by QA team

---

## 📊 Effort Summary
| Phase | Duration | Effort Type | Status |
|-------|----------|-------------|--------|
| Phase 0: Research & Discovery | ~4-6 hours | Discovery | ✅ Complete |
| Phase 1: Architecture & Contracts | ~8-10 hours | Design | ✅ Complete |
| Phase 2: Stripe Infrastructure Setup | ~12-16 hours | Backend Infrastructure | ⏳ Pending |
| Phase 3: Subscription Business Logic & Checkout | ~16-20 hours | Backend Application Layer | ⏳ Pending |
| Phase 4: Webhook Processing | ~10-14 hours | Backend Infrastructure + Application | ⏳ Pending |
| Phase 5: Frontend Pricing & Checkout Flow | ~14-18 hours | Frontend Implementation | ⏳ Pending |
| Phase 6: Subscription Management UI | ~12-16 hours | Frontend Implementation | ⏳ Pending |
| Phase 7: Admin Currency Management | ~8-12 hours | Backend + Frontend Admin | ⏳ Pending |
| Phase 8: Testing, Documentation & Deployment | ~10-14 hours | Testing, Documentation, DevOps | ⏳ Pending |
| **Total Estimated Effort** | **94-126 hours** | **~12-16 days** | **16% Complete** |
