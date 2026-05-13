# Implementation Tasks: Stripe Subscription Management

**Feature**: Stripe Subscription Management  
**Branch**: `008-stripe-subscription`  
**Generated**: 2025-12-14  
**Total Estimated Hours**: 94-126 hours  

## Overview

This task breakdown follows Clean Architecture principles, organizing tasks by user story priority to enable independent implementation and testing. Each user story represents a complete, testable feature increment that can be deployed independently.

**Architecture**: .NET 8 backend (Domain → Application → Infrastructure → Presentation) + React 18 frontend  
**Payment Provider**: Stripe with multi-currency support  
**Database**: PostgreSQL 15+ with EF Core migrations  
**Testing**: xUnit (backend), Vitest + React Testing Library (frontend), Stripe CLI (webhooks)

## Task Format

All tasks follow this format:
- `- [ ]` Checkbox for tracking completion
- `[T###]` Unique task identifier in execution order
- `[P]` Parallel execution marker (optional) - task can run concurrently with others
- `[US#]` User story mapping (for story-specific tasks)
- Description with specific file paths

**Example**: `- [ ] T012 [P] [US1] Create Subscription entity in backend/src/WahadiniCryptoQuest.Core/Entities/Subscription.cs`

---

## Phase 1: Setup & Infrastructure (12-16 hours)

**Goal**: Initialize Stripe integration, configure database schema, and set up payment gateway abstraction

**Tasks**:

- [x] T001 Create Stripe Dashboard products: "Premium Monthly" and "Premium Yearly" with pricing for USD, INR, EUR, JPY, GBP
- [x] T002 Generate Stripe API keys (publishable, secret) and webhook signing secret from Stripe Dashboard
- [x] T003 Configure webhook endpoint URLs in Stripe Dashboard for development and production environments
- [x] T004 Install Stripe.net NuGet package (v43.0.0+) in backend/src/WahadiniCryptoQuest.DAL/WahadiniCryptoQuest.DAL.csproj
- [x] T005 Add Stripe configuration section to backend/src/WahadiniCryptoQuest.API/appsettings.json with PublishableKey, SecretKey, WebhookSecret
- [x] T006 Create database migration AddStripeSubscription in backend/src/WahadiniCryptoQuest.DAL/Migrations/
- [x] T007 [P] Define Subscription entity configuration in backend/src/WahadiniCryptoQuest.DAL/Configurations/SubscriptionConfiguration.cs
- [x] T008 [P] Define CurrencyPricing entity configuration in backend/src/WahadiniCryptoQuest.DAL/Configurations/CurrencyPricingConfiguration.cs
- [x] T009 [P] Define WebhookEvent entity configuration in backend/src/WahadiniCryptoQuest.DAL/Configurations/WebhookEventConfiguration.cs
- [x] T010 [P] Define SubscriptionHistory entity configuration in backend/src/WahadiniCryptoQuest.DAL/Configurations/SubscriptionHistoryConfiguration.cs
- [x] T011 Apply migration to development database and verify schema creation
- [x] T012 [P] Create IPaymentGateway interface in backend/src/WahadiniCryptoQuest.Service/Interfaces/IPaymentGateway.cs
- [x] T013 Implement StripePaymentGateway in backend/src/WahadiniCryptoQuest.DAL/Infrastructure/StripePaymentGateway.cs with CreateCheckoutSessionAsync method
- [x] T014 Implement CreatePortalSessionAsync method in backend/src/WahadiniCryptoQuest.DAL/Infrastructure/StripePaymentGateway.cs
- [x] T015 Implement GetSubscriptionAsync method in backend/src/WahadiniCryptoQuest.DAL/Infrastructure/StripePaymentGateway.cs
- [x] T016 Implement CancelSubscriptionAsync method in backend/src/WahadiniCryptoQuest.DAL/Infrastructure/StripePaymentGateway.cs
- [x] T017 Register IPaymentGateway service in backend/src/WahadiniCryptoQuest.API/Program.cs dependency injection
- [x] T018 Seed initial currency pricing data for USD, INR, EUR, JPY, GBP in database

**Deliverables**:
- Stripe Dashboard configured with multi-currency pricing
- Database schema with 4 new tables (Subscriptions, CurrencyPricings, WebhookEvents, SubscriptionHistory)
- Payment gateway abstraction layer ready for business logic

---

## Phase 2: Foundational Components (8-12 hours)

**Goal**: Build shared infrastructure needed by all user stories (domain entities, repositories, DTOs)

**Tasks**:

- [x] T019 [P] Create Subscription domain entity in backend/src/WahadiniCryptoQuest.Core/Entities/Subscription.cs with factory methods
- [x] T020 [P] Create CurrencyPricing domain entity in backend/src/WahadiniCryptoQuest.Core/Entities/CurrencyPricing.cs
- [x] T021 [P] Create WebhookEvent domain entity in backend/src/WahadiniCryptoQuest.Core/Entities/WebhookEvent.cs
- [x] T022 [P] Create SubscriptionHistory domain entity in backend/src/WahadiniCryptoQuest.Core/Entities/SubscriptionHistory.cs
- [x] T023 [P] Create SubscriptionTier enum in backend/src/WahadiniCryptoQuest.Core/Enums/SubscriptionTier.cs (Free, Monthly, Yearly)
- [x] T024 [P] Create SubscriptionStatus enum in backend/src/WahadiniCryptoQuest.Core/Enums/SubscriptionStatus.cs (Active, PastDue, Canceled, Incomplete)
- [x] T025 [P] Create WebhookProcessingStatus enum in backend/src/WahadiniCryptoQuest.Core/Enums/WebhookProcessingStatus.cs
- [x] T026 [P] Create SubscriptionRepository in backend/src/WahadiniCryptoQuest.DAL/Repositories/SubscriptionRepository.cs
- [x] T027 [P] Create CurrencyPricingRepository in backend/src/WahadiniCryptoQuest.DAL/Repositories/CurrencyPricingRepository.cs
- [x] T028 [P] Create WebhookEventRepository in backend/src/WahadiniCryptoQuest.DAL/Repositories/WebhookEventRepository.cs
- [x] T029 [P] Create SubscriptionStatusDto in backend/src/WahadiniCryptoQuest.Core/DTOs/Subscription/SubscriptionStatusDto.cs
- [x] T030 [P] Create CheckoutSessionDto in backend/src/WahadiniCryptoQuest.Core/DTOs/Subscription/CheckoutSessionDto.cs
- [x] T031 [P] Create PricingDto in backend/src/WahadiniCryptoQuest.Core/DTOs/Currency/PricingDto.cs
- [x] T032 [P] Create SubscriptionMappingProfile in backend/src/WahadiniCryptoQuest.Service/Mappings/SubscriptionMappingProfile.cs for AutoMapper
- [x] T033 Register repositories in dependency injection in backend/src/WahadiniCryptoQuest.API/Program.cs

**Deliverables**:
- Domain entities with business logic encapsulation
- Repository pattern implementation for data access
- DTOs for API communication
- AutoMapper configuration for entity-DTO mapping

---

## Phase 3: User Story 1 - Subscribe to Premium Plan (16-20 hours)

**Goal**: Free users can view pricing, select plan, complete checkout, and gain immediate premium access

**User Story**: US1 - Subscribe to Premium Plan (Priority: P1)  
**Independent Test**: Create test user, navigate to pricing page, verify currency-specific pricing, select plan, complete checkout with test card, verify premium access granted

**Tasks**:

### Backend - Domain & Application Layer

- [x] T034 [P] [US1] Create CreateCheckoutSessionCommand in backend/src/WahadiniCryptoQuest.Service/Commands/CreateCheckoutSessionCommand.cs
- [x] T035 [P] [US1] Create GetSubscriptionStatusQuery in backend/src/WahadiniCryptoQuest.Service/Queries/GetSubscriptionStatusQuery.cs
- [x] T036 [US1] Implement CreateCheckoutSessionCommandHandler in backend/src/WahadiniCryptoQuest.Service/Handlers/CommandHandlers/CreateCheckoutSessionCommandHandler.cs
- [x] T037 [US1] Add concurrent checkout prevention: check for existing active subscription before creating session, return error if exists
- [x] T038 [US1] Implement GetSubscriptionStatusQueryHandler in backend/src/WahadiniCryptoQuest.Service/Handlers/QueryHandlers/GetSubscriptionStatusQueryHandler.cs
- [x] T039 [P] [US1] Create CreateCheckoutSessionValidator in backend/src/WahadiniCryptoQuest.Service/Validators/CreateCheckoutSessionValidator.cs with FluentValidation
- [x] T040 [US1] Implement SubscriptionService.GetSubscriptionStatusAsync in backend/src/WahadiniCryptoQuest.Service/Services/SubscriptionService.cs
- [x] T041 [US1] Implement SubscriptionService.CreateCheckoutSessionAsync with currency pricing lookup

### Backend - Infrastructure & Presentation Layer

- [x] T042 [US1] Create SubscriptionsController in backend/src/WahadiniCryptoQuest.API/Controllers/SubscriptionsController.cs
- [x] T043 [US1] Implement POST /api/subscriptions/checkout endpoint with [Authorize] attribute
- [x] T044 [US1] Implement GET /api/subscriptions/status endpoint to return current subscription details
- [x] T045 [P] [US1] Create CreateCheckoutRequest model in backend/src/WahadiniCryptoQuest.API/Models/Requests/CreateCheckoutRequest.cs
- [x] T046 [P] [US1] Create SubscriptionStatusResponse model in backend/src/WahadiniCryptoQuest.API/Models/Responses/SubscriptionStatusResponse.cs

### Frontend - Components & Hooks

- [x] T047 [P] [US1] Create PricingCard component in frontend/src/components/subscription/PricingCard/PricingCard.tsx
- [x] T048 [P] [US1] Create PlanComparison component in frontend/src/components/subscription/PlanComparison/PlanComparison.tsx
- [x] T049 [P] [US1] Create SubscriptionStatus component in frontend/src/components/subscription/SubscriptionStatus/SubscriptionStatus.tsx
- [x] T050 [US1] Create PricingPage in frontend/src/pages/subscription/PricingPage.tsx with responsive 3-column layout
- [x] T051 [US1] Create CheckoutSuccessPage in frontend/src/pages/subscription/CheckoutSuccessPage.tsx
- [x] T052 [US1] Create CheckoutCancelPage in frontend/src/pages/subscription/CheckoutCancelPage.tsx
- [x] T053 [P] [US1] Create useCreateCheckout hook in frontend/src/hooks/subscription/useCreateCheckout.ts with React Query
- [x] T054 [P] [US1] Create useSubscription hook in frontend/src/hooks/subscription/useSubscription.ts to fetch status
- [x] T055 [US1] Implement subscriptionService.createCheckout in frontend/src/services/api/subscriptionService.ts
- [x] T056 [US1] Implement subscriptionService.getStatus in frontend/src/services/api/subscriptionService.ts
- [x] T057 [P] [US1] Create subscription TypeScript types in frontend/src/types/subscription.types.ts
- [x] T058 [US1] Install @stripe/stripe-js npm package in frontend/package.json
- [x] T059 [US1] Configure Stripe publishable key in frontend/.env
- [x] T060 [US1] Implement checkout redirection logic on "Subscribe" button click with loading state
- [x] T061 [US1] Handle URL parameters (?success=true, ?canceled=true) on return from Stripe
- [x] T062 [US1] Display error messages if checkout creation fails

### Testing

- [x] T063 [P] [US1] Write unit tests for CreateCheckoutSessionCommandHandler in backend/tests/WahadiniCryptoQuest.Service.Tests/
- [x] T064 [P] [US1] Write unit tests for SubscriptionsController in backend/tests/WahadiniCryptoQuest.API.Tests/
- [x] T065 [P] [US1] Write component tests for PricingCard in frontend/src/components/subscription/PricingCard/PricingCard.test.tsx
- [x] T066 [US1] Write E2E test for checkout flow in frontend/tests/e2e/checkout-flow.spec.ts using Playwright

**Acceptance Criteria**:
- ✅ Pricing page displays all three tiers with currency-specific pricing
- ✅ "Subscribe" button creates checkout session and redirects to Stripe
- ✅ Success page shows confirmation after successful payment
- ✅ Cancel page shows appropriate message when user abandons checkout
- ✅ Premium access granted immediately after webhook processes payment

---

## Phase 4: User Story 4 - Automatic Subscription Lifecycle (10-14 hours)

**Goal**: System automatically processes subscription events via webhooks to maintain synchronized state

**User Story**: US4 - Automatic Subscription Lifecycle Handling (Priority: P4)  
**Independent Test**: Simulate webhook events for payment success, payment failure, subscription deletion in test environment, verify database updates and access level changes

**Tasks**:

### Backend - Webhook Infrastructure

- [x] T067 [P] [US4] Create WebhooksController in backend/src/WahadiniCryptoQuest.API/Controllers/WebhooksController.cs
- [x] T068 [US4] Implement POST /api/webhooks/stripe endpoint with raw body access and no [Authorize]
- [x] T069 [US4] Implement StripeWebhookMiddleware in backend/src/WahadiniCryptoQuest.API/Middleware/StripeWebhookMiddleware.cs for signature verification
- [x] T070 [US4] Use EventUtility.ConstructEvent() to verify webhook signature with HMAC-SHA256
- [x] T071 [US4] Return 401 Unauthorized if signature verification fails
- [x] T072 [US4] Implement idempotency check: verify StripeEventId not in WebhookEvents table before processing
- [x] T073 [US4] Insert WebhookEvent record with Processing status before event handling

### Backend - Event Handlers

- [x] T074 [P] [US4] Create WebhookProcessingService in backend/src/WahadiniCryptoQuest.Service/Services/WebhookProcessingService.cs
- [x] T075 [US4] Implement HandleCheckoutSessionCompletedAsync to create Subscription record and mark discount code as used
- [x] T076 [US4] Implement HandleInvoicePaymentSucceededAsync to extend subscription period end date
- [x] T077 [US4] Implement HandleInvoicePaymentFailedAsync to set status to PastDue (grace period starts)
- [x] T078 [US4] Implement HandleCustomerSubscriptionDeletedAsync to downgrade user to Free tier
- [x] T079 [US4] Update WebhookEvent status to Processed or Failed after handling
- [x] T080 [US4] Return HTTP 500 on processing failures (database errors, network issues) to trigger Stripe auto-retry
- [x] T081 [US4] Log failures for manual review if Stripe retries exhausted after 3 days
- [x] T082 [US4] Implement transaction scope for webhook processing to ensure atomicity

### Backend - Domain Events

- [x] T083 [P] [US4] Create SubscriptionCreatedEvent in backend/src/WahadiniCryptoQuest.Core/Events/SubscriptionCreatedEvent.cs
- [x] T084 [P] [US4] Create SubscriptionCancelledEvent in backend/src/WahadiniCryptoQuest.Core/Events/SubscriptionCancelledEvent.cs
- [x] T085 [P] [US4] Create SubscriptionRenewedEvent in backend/src/WahadiniCryptoQuest.Core/Events/SubscriptionRenewedEvent.cs

### Testing

- [x] T086 [P] [US4] Write unit tests for WebhookProcessingService in backend/tests/WahadiniCryptoQuest.Service.Tests/
- [x] T087 [P] [US4] Write integration tests for WebhooksController in backend/tests/WahadiniCryptoQuest.API.Tests/
- [x] T088 [US4] Test webhook signature verification with valid and invalid signatures
- [x] T089 [US4] Test idempotency: send same webhook event twice, verify processed only once
- [x] T090 [US4] Test all event types: checkout.session.completed, invoice.payment_succeeded, invoice.payment_failed, customer.subscription.deleted
- [x] T091 [US4] Use Stripe CLI (stripe listen --forward-to localhost:5000/api/webhooks/stripe) for local webhook testing

**Acceptance Criteria**:
- ✅ Webhook signature verification prevents unauthorized requests
- ✅ Idempotency prevents duplicate processing of same event
- ✅ Subscription created after checkout.session.completed
- ✅ Subscription period extended after invoice.payment_succeeded
- ✅ Status set to PastDue after invoice.payment_failed
- ✅ User downgraded to Free tier after customer.subscription.deleted
- ✅ Failed processing returns 500 to trigger Stripe retry

---

## Phase 5: User Story 2 - Apply Discount Code (8-12 hours)

**Goal**: Users can apply reward-earned discount codes during checkout to receive reduced pricing

**User Story**: US2 - Apply Discount Code at Checkout (Priority: P2)  
**Independent Test**: Generate valid discount code, initiate checkout, apply code, verify discounted price in session, complete payment, confirm code marked as used

**Tasks**:

### Backend - Discount Validation

- [x] T092 [P] [US2] Create DiscountValidationDto in backend/src/WahadiniCryptoQuest.Core/DTOs/Subscription/DiscountValidationDto.cs
- [x] T093 [US2] Update CreateCheckoutSessionCommand to accept optional discountCode parameter
- [x] T094 [US2] Implement ValidateDiscountCodeAsync in SubscriptionService to check Stripe Promotion Code validity
- [x] T095 [US2] Implement 24-hour temporary reservation when discount code applied: mark code as reserved with timestamp
- [x] T096 [US2] Create background job to auto-release reserved codes after 24 hours if no successful payment
- [x] T097 [US2] Apply discount code to Stripe checkout session creation (coupon parameter)
- [x] T098 [US2] Update HandleCheckoutSessionCompletedAsync to mark discount code as permanently used after successful payment
- [x] T099 [US2] Return error if discount code is invalid, expired, or already used

### Backend - API Endpoints

- [x] T100 [US2] Update POST /api/subscriptions/checkout to accept discountCode in request body
- [x] T101 [P] [US2] Create POST /api/subscriptions/validate-discount endpoint to validate code before checkout

### Frontend - Discount UI

- [x] T102 [P] [US2] Create DiscountCodeInput component in frontend/src/components/subscription/DiscountCodeInput/DiscountCodeInput.tsx
- [x] T103 [US2] Add discount code input field to checkout flow with validation feedback
- [x] T104 [US2] Implement useDiscountValidation hook in frontend/src/hooks/subscription/useDiscountValidation.ts
- [x] T105 [US2] Display applied discount amount in checkout summary
- [x] T106 [US2] Show error message for invalid/used discount codes
- [x] T107 [US2] Update checkout button to include discount code parameter

### Testing

- [x] T108 [P] [US2] Write unit tests for discount code validation in backend/tests/WahadiniCryptoQuest.Service.Tests/
- [x] T109 [P] [US2] Write component tests for DiscountCodeInput in frontend/src/components/subscription/DiscountCodeInput/DiscountCodeInput.test.tsx
- [x] T110 [US2] Test discount code scenarios: valid, invalid, expired, already used, checkout abandoned

**Acceptance Criteria**:
- ✅ User can enter discount code during checkout
- ✅ Valid codes display discounted pricing
- ✅ Invalid codes show error message
- ✅ Code marked as used after successful payment
- ✅ Abandoned codes released after 24 hours

---

## Phase 6: User Story 3 - Manage Subscription and Billing (8-10 hours)

**Goal**: Subscribers can access billing portal to manage subscription settings and payment methods

**User Story**: US3 - Manage Subscription and Billing (Priority: P3)  
**Independent Test**: From premium account, access billing management, verify portal redirect, update payment method, initiate cancellation, verify access continues until period end

**Tasks**:

### Backend - Portal & Cancellation

- [x] T111 [P] [US3] Create CreatePortalSessionCommand in backend/src/WahadiniCryptoQuest.Service/Commands/CreatePortalSessionCommand.cs
- [x] T112 [P] [US3] Create CancelSubscriptionCommand in backend/src/WahadiniCryptoQuest.Service/Commands/CancelSubscriptionCommand.cs
- [x] T113 [US3] Implement CreatePortalSessionCommandHandler to generate Stripe billing portal session
- [x] T114 [US3] Implement CancelSubscriptionCommandHandler to cancel subscription at period end (set CancelAtPeriodEnd = true)
- [x] T115 [US3] Implement ReactivateSubscriptionAsync to remove cancellation flag if user changes mind
- [x] T116 [US3] Update SubscriptionStatusDto to include next billing date, cancellation date, payment method last 4 digits

### Backend - API Endpoints

- [x] T117 [US3] Implement POST /api/subscriptions/portal endpoint to create billing portal session
- [x] T117a [US3] Verify Stripe billing portal configuration displays currency-specific invoice formatting
- [x] T118 [US3] Implement POST /api/subscriptions/cancel endpoint with [Authorize] attribute
- [x] T119 [US3] Implement POST /api/subscriptions/reactivate endpoint

### Frontend - Subscription Management UI

- [x] T120 [P] [US3] Create ManageSubscriptionPage in frontend/src/pages/subscription/ManageSubscriptionPage.tsx
- [x] T121 [US3] Add "Manage Billing" button to user profile that redirects to Stripe portal
- [x] T122 [US3] Display subscription status card showing current plan, next billing date, cancellation status
- [x] T123 [US3] Display "Active (Payment Issue)" status with warning banner during grace period
- [x] T124 [US3] Add "Cancel Subscription" button with confirmation modal
- [x] T125 [US3] Show cancellation confirmation: "Access until [period end date]"
- [x] T126 [US3] Add "Reactivate" button for cancelled-but-active subscriptions
- [x] T127 [US3] Implement usePortalSession hook in frontend/src/hooks/subscription/usePortalSession.ts

### Testing

- [x] T128 [P] [US3] Write unit tests for portal session creation in backend/tests/WahadiniCryptoQuest.Service.Tests/
- [x] T129 [P] [US3] Write unit tests for cancellation logic in backend/tests/WahadiniCryptoQuest.Service.Tests/
- [x] T130 [US3] Test portal redirect flow with test Stripe account
- [x] T131 [US3] Test cancellation preserves access until period end

**Acceptance Criteria**:
- ✅ "Manage Billing" button opens Stripe billing portal
- ✅ Portal displays subscription details, invoices, payment history
- ✅ Cancel button sets subscription to cancel at period end
- ✅ Premium access continues until period end date
- ✅ Reactivate button removes cancellation flag

---

## Phase 7: User Story 5 - Admin Configure Currency Pricing (6-8 hours)

**Goal**: Administrators can configure subscription pricing for multiple currencies

**User Story**: US5 - Admin Configure Currency Pricing (Priority: P5)  
**Independent Test**: Login as admin, access currency configuration, add/update INR pricing, save changes, verify pricing page shows updated amounts

**Tasks**:

### Backend - Admin Commands

- [x] T132 [P] [US5] Create UpdateCurrencyPricingCommand in backend/src/WahadiniCryptoQuest.Service/Commands/UpdateCurrencyPricingCommand.cs
- [x] T133 [P] [US5] Create GetCurrencyPricingQuery in backend/src/WahadiniCryptoQuest.Service/Queries/GetCurrencyPricingQuery.cs
- [x] T134 [US5] Implement UpdateCurrencyPricingCommandHandler with validation rules: amount >= 0, amount <= $9999 equivalent
- [x] T135 [US5] Add warning system: flag prices differing >50% from USD baseline for admin review
- [x] T136 [US5] Implement GetCurrencyPricingQueryHandler to return all configured currencies
- [x] T137 [P] [US5] Create CurrencyPricingValidator with FluentValidation rules
- [x] T138 [US5] Implement CurrencyService.GetAllPricingAsync in backend/src/WahadiniCryptoQuest.Service/Services/CurrencyService.cs
- [x] T139 [US5] Implement CurrencyService.UpdatePricingAsync with admin authorization check

### Backend - Admin API

- [x] T140 [US5] Create CurrencyPricingController in backend/src/WahadiniCryptoQuest.API/Controllers/Admin/CurrencyPricingController.cs
- [x] T141 [US5] Implement GET /api/admin/currency-pricing endpoint with [Authorize(Roles = "Admin")]
- [x] T142 [US5] Implement PUT /api/admin/currency-pricing endpoint to update pricing
- [x] T143 [US5] Add warning indicators in API response when prices deviate >50% from baseline

### Frontend - Admin Currency Management

- [x] T144 [P] [US5] Create CurrencyPricingTable component in frontend/src/components/admin/CurrencyPricingTable/CurrencyPricingTable.tsx
- [x] T145 [US5] Create admin currency management page in frontend/src/pages/admin/CurrencyPricingPage.tsx
- [x] T146 [US5] Display table with all currencies: Code, Monthly Price, Yearly Price, Status
- [x] T147 [US5] Add edit functionality with inline editing or modal form
- [x] T148 [US5] Display warning icons for prices deviating >50% from USD
- [x] T149 [US5] Implement useCurrencyPricing hook in frontend/src/hooks/subscription/useCurrencyPricing.ts
- [x] T150 [US5] Add form validation: prices must be non-negative, max $9999

### Testing

- [x] T151 [P] [US5] Write unit tests for UpdateCurrencyPricingCommandHandler in backend/tests/WahadiniCryptoQuest.Service.Tests/
- [x] T152 [P] [US5] Write integration tests for CurrencyPricingController in backend/tests/WahadiniCryptoQuest.API.Tests/
- [x] T153 [US5] Test validation rules: negative price rejection, max price enforcement, deviation warnings

**Acceptance Criteria**:
- ✅ Admin can view all configured currency pricing
- ✅ Admin can update monthly and yearly prices per currency
- ✅ System validates prices are non-negative and below $9999
- ✅ Warning displayed for >50% deviation from USD baseline
- ✅ Pricing changes reflected on public pricing page immediately

---

## Phase 8: User Story 6 - View Plan Comparison (4-6 hours)

**Goal**: All users can view clear comparison of subscription tiers with feature lists

**User Story**: US6 - View Plan Comparison and Features (Priority: P6)  
**Independent Test**: Navigate to pricing page as authenticated and unauthenticated user from different regions, verify tier display, feature lists, pricing format

**Tasks**:

### Frontend - Pricing Display

- [x] T154 [US6] Create PricingPage with responsive 3-column layout (Free, Monthly Premium, Yearly Premium)
- [x] T155 [US6] Display feature list for each tier with checkmarks/crosses
- [x] T156 [US6] Add "Most Popular" badge to Yearly Premium plan
- [x] T157 [US6] Calculate and display savings percentage for yearly plan (e.g., "Save 17%")
- [x] T158 [US6] Show "Current Plan" badge for authenticated user's active subscription
- [x] T159 [US6] Disable subscribe button for current plan, change to "Manage" button
- [x] T160 [US6] Implement responsive layout: 3 columns on desktop, stacked on mobile
- [x] T161 [US6] Add currency-specific formatting with proper symbols and decimal places

### Frontend - Currency Detection & Selection

- [x] T162 [P] [US6] Create CurrencySelector component in frontend/src/components/subscription/CurrencySelector/CurrencySelector.tsx
- [x] T163 [US6] Implement client-side currency detection based on browser locale
- [x] T164 [US6] Allow manual currency selection via dropdown
- [x] T164a [US6] Implement currency persistence in sessionStorage when user manually selects currency
- [x] T164b [US6] Pass selected currency from sessionStorage to CreateCheckoutSessionCommand
- [x] T165 [US6] Create formatCurrency utility in frontend/src/utils/currency/formatCurrency.ts
- [x] T166 [US6] Create detectCurrency utility in frontend/src/utils/currency/detectCurrency.ts
- [x] T167 [US6] Implement currency symbol positioning (₹ prefix for INR, $ prefix for USD, € prefix for EUR, ¥ prefix for JPY)
- [x] T168 [US6] Implement decimal place formatting (0 decimals for JPY, 2 decimals for USD/EUR/INR)

### Testing

- [x] T169 [P] [US6] Write component tests for PricingPage in frontend/src/pages/subscription/PricingPage.test.tsx
- [x] T170 [P] [US6] Write component tests for CurrencySelector in frontend/src/components/subscription/CurrencySelector/CurrencySelector.test.tsx
- [x] T171 [US6] Test currency formatting for all supported currencies
- [x] T172 [US6] Test responsive layout on mobile, tablet, desktop breakpoints

**Acceptance Criteria**:
- ✅ Three pricing tiers display side-by-side on desktop
- ✅ Feature comparison clearly shows differences between tiers
- ✅ Currency detection works for major regions
- ✅ Manual currency selection updates pricing immediately
- ✅ Savings badge shows percentage saved with yearly plan
- ✅ Current plan badge displays for authenticated users

---

## Phase 9: Polish & Cross-Cutting Concerns (10-12 hours)

**Goal**: Enhance user experience, security, performance, and monitoring

**Tasks**:

### Security Hardening

- [x] T173 [P] Implement rate limiting on webhook endpoint to prevent abuse
- [x] T174 [P] Add input sanitization for all user-provided data (discount codes, currency selection)
- [x] T175 [P] Implement SQL injection prevention audit for all queries
- [x] T176 Add CSRF protection for subscription endpoints
- [x] T177 Implement audit logging for all subscription lifecycle events
- [x] T178 Add security headers (CSP, HSTS) to API responses

### Performance Optimization

- [x] T179 [P] Implement caching for currency pricing (Redis or in-memory with 5-minute TTL)
- [x] T180 [P] Add database indexes on frequently queried columns (UserId, StripeCustomerId, StripeEventId)
- [x] T181 Optimize webhook processing with background queue (Hangfire or Azure Queue)
- [x] T182 Implement pagination for admin currency listing
- [x] T183 Add query optimization: use AsNoTracking for read-only queries

### Error Handling & Logging

- [x] T184 [P] Standardize error responses across all subscription endpoints
- [x] T185 [P] Implement structured logging with Serilog for all Stripe API calls
- [x] T186 Add correlation IDs to trace requests through webhook processing
- [x] T187 Implement graceful degradation when Stripe service unavailable
- [x] T188 Add retry logic for transient Stripe API failures (exponential backoff)

### Monitoring & Observability

- [x] T189 [P] Add health check endpoint for Stripe API connectivity
- [x] T190 [P] Implement metrics collection for checkout success/failure rates
- [x] T191 Add monitoring dashboard for webhook processing delays
- [x] T192 Implement alerting for webhook signature verification failures
- [x] T193 Add subscription lifecycle event tracking (analytics)

### Documentation

- [x] T194 [P] Generate OpenAPI/Swagger documentation for all subscription endpoints
- [x] T195 [P] Create admin user guide for currency configuration
- [x] T196 Create developer documentation for webhook testing with Stripe CLI
- [x] T197 Document rollback procedures for failed deployments
- [x] T198 Create troubleshooting guide for common Stripe integration issues

### Accessibility & UX

- [x] T199 [P] Add ARIA labels to pricing cards and subscription status
- [x] T200 [P] Implement keyboard navigation for pricing page
- [x] T201 Ensure color contrast meets WCAG 2.1 AA standards for subscription UI
- [x] T202 Add loading skeletons for pricing page data fetch
- [x] T203 Implement toast notifications for subscription state changes
- [x] T204 Add confirmation modals for destructive actions (cancellation)

**Deliverables**:
- Enhanced security posture with rate limiting and input sanitization
- Optimized performance with caching and query optimization
- Comprehensive logging and monitoring
- Complete API documentation
- Accessible and user-friendly interface

---

## Dependency Graph

This diagram shows the required completion order for user stories. Stories within the same level can be implemented in parallel.

```
Level 0: Setup & Foundational
├── Phase 1: Setup & Infrastructure (T001-T018)
└── Phase 2: Foundational Components (T019-T033)

Level 1: Core Revenue Flow (BLOCKING)
└── Phase 3: US1 - Subscribe to Premium (T034-T066)
    └── Phase 4: US4 - Webhook Lifecycle (T067-T091) [depends on US1]

Level 2: Enhancement Features (PARALLEL)
├── Phase 5: US2 - Discount Codes (T092-T110) [depends on US1]
├── Phase 6: US3 - Subscription Management (T111-T131) [depends on US1]
└── Phase 7: US5 - Admin Currency Config (T132-T153) [independent]

Level 3: Marketing & UX (PARALLEL)
└── Phase 8: US6 - Plan Comparison (T154-T172) [independent]

Level 4: Production Readiness
└── Phase 9: Polish & Cross-Cutting (T173-T204) [depends on all features]
```

**Critical Path**: Setup → Foundational → US1 → US4 → Polish (minimum 50-66 hours)

---

## Parallel Execution Opportunities

### Phase 1 (Setup)
- **Group A** (T007-T010): EF Core configurations - 4 tasks can run in parallel (different files)
- **Group B** (T012-T016): Payment gateway methods - 5 tasks sequential (same class)

### Phase 2 (Foundational)
- **Group A** (T019-T022): Domain entities - 4 tasks in parallel
- **Group B** (T023-T025): Enums - 3 tasks in parallel
- **Group C** (T026-T028): Repositories - 3 tasks in parallel
- **Group D** (T029-T031): DTOs - 3 tasks in parallel

### Phase 3 (US1)
- **Group A** (T034-T035, T039, T045-T046): Commands, queries, validators, models - 5 tasks in parallel
- **Group B** (T047-T049): React components - 3 tasks in parallel
- **Group C** (T053-T054, T057): React hooks and types - 3 tasks in parallel
- **Group D** (T063-T065): Unit tests - 3 tasks in parallel

### Phase 4 (US4)
- **Group A** (T067, T083-T085): Controller and domain events - 4 tasks in parallel
- **Group B** (T086-T087): Unit and integration tests - 2 tasks in parallel

### Phase 5 (US2)
- **Group A** (T092, T101): DTO and API endpoint - 2 tasks in parallel
- **Group B** (T108-T109): Backend and frontend tests - 2 tasks in parallel

### Phase 6 (US3)
- **Group A** (T111-T112, T120): Commands and page component - 3 tasks in parallel
- **Group B** (T128-T129): Unit tests - 2 tasks in parallel

### Phase 7 (US5)
- **Group A** (T132-T133, T137, T144): Commands, queries, validator, component - 4 tasks in parallel
- **Group B** (T151-T152): Unit and integration tests - 2 tasks in parallel

### Phase 9 (Polish)
- **Group A** (T173-T175, T179-T180, T184-T185, T189-T191, T194-T196, T199-T200): Security, performance, logging, monitoring, docs, accessibility - 16 tasks in parallel (different concerns)

**Estimated Time Savings with Parallelization**: 30-40% reduction from 126 hours to ~85-90 hours with 3-4 developers

---

## Implementation Strategy

### MVP Scope (Weeks 1-2: 50-66 hours)
Focus on minimum viable product for revenue generation:
- ✅ Phase 1: Setup & Infrastructure
- ✅ Phase 2: Foundational Components
- ✅ Phase 3: US1 - Subscribe to Premium
- ✅ Phase 4: US4 - Webhook Lifecycle
- ✅ Selected Phase 9 tasks: Security hardening (T173-T178), Error handling (T184-T188)

**MVP Deliverable**: Users can subscribe to premium plans with automatic lifecycle management

### Enhancement Phase (Weeks 3-4: 30-44 hours)
Add user-facing features and admin capabilities:
- ✅ Phase 5: US2 - Discount Codes
- ✅ Phase 6: US3 - Subscription Management
- ✅ Phase 7: US5 - Admin Currency Config
- ✅ Phase 8: US6 - Plan Comparison

**Enhancement Deliverable**: Full-featured subscription system with admin controls

### Polish Phase (Week 5: 10-12 hours)
Production readiness and optimization:
- ✅ Remaining Phase 9 tasks: Performance optimization, monitoring, documentation, accessibility

**Polish Deliverable**: Production-ready subscription system with monitoring and documentation

---

## Testing Strategy

### Unit Testing (Backend)
- **Domain Layer**: Business logic in entities (factory methods, status transitions)
- **Application Layer**: Service methods, command/query handlers
- **Infrastructure Layer**: Payment gateway with mocked Stripe SDK
- **Coverage Target**: 80%+ for all layers

### Integration Testing (Backend)
- **API Controllers**: Full request/response cycle with test database
- **Webhook Processing**: End-to-end webhook handling with test events
- **Repository Layer**: Database operations with real PostgreSQL test instance

### Component Testing (Frontend)
- **React Components**: Rendering, user interactions, state changes
- **Custom Hooks**: Data fetching, state management
- **Coverage Target**: 70%+ for all components

### E2E Testing (Frontend)
- **Playwright Tests**: Full checkout flow from pricing page to success
- **Scenarios**: Subscribe with/without discount, cancel subscription, access billing portal

### Webhook Testing
- **Stripe CLI**: `stripe listen --forward-to localhost:5000/api/webhooks/stripe`
- **Event Simulation**: Trigger all event types (checkout completed, payment succeeded, payment failed, subscription deleted)
- **Idempotency Testing**: Send duplicate events, verify single processing

---

## Summary

**Total Tasks**: 207 tasks  
**Estimated Hours**: 95-128 hours (varies with parallelization)  
**User Stories**: 6 stories (US1-US6) organized by priority  
**Critical Dependencies**: US1 → US4 (blocking), others can be parallel  
**Parallel Opportunities**: ~40% of tasks can run concurrently with proper team coordination  
**MVP Timeline**: 2 weeks (50-66 hours) for core subscription functionality  
**Full Feature Timeline**: 4-5 weeks (94-126 hours) for complete implementation  

**Next Steps**:
1. Review and approve task breakdown
2. Assign tasks to development team
3. Set up Stripe test environment following quickstart.md
4. Begin Phase 1: Setup & Infrastructure
5. Track progress using checkbox format for transparency
