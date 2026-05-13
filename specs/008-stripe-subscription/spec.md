# Feature Specification: Stripe Subscription Management

**Feature Branch**: `008-stripe-subscription`  
**Created**: 2025-12-09  
**Updated**: 2025-12-15 (Added FR-023 to FR-035 for security, UX, and API requirements)  
**Status**: Ready for Implementation  
**Input**: User description: "Implement a comprehensive subscription system using Stripe, supporting Free, Monthly Premium, and Yearly Premium tiers. The system includes checkout flows, webhook handling, subscription management, and discount code application."

## Assumptions

- Users must be authenticated to subscribe to premium plans
- Yearly Premium saves approximately 17% compared to monthly billing (2 months free)
- Stripe handles payment retries automatically with Smart Retries feature
- Failed payment grace period is 3 days before downgrade to Free tier
- Only one discount code can be applied per subscription transaction
- Billing portal functionality is provided by Stripe's hosted solution
- System supports multiple currencies (USD, INR, EUR, JPY, etc.) with admin-configurable pricing for each currency
- User's currency is detected automatically based on browser locale/IP or can be manually selected
- Invoice history is maintained by Stripe and accessed via Customer Portal
- Credit card information is never stored locally; only Stripe Customer ID and Subscription ID are persisted
- Subscription changes (upgrades/downgrades) are handled through new checkout sessions with prorated billing managed by Stripe

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Subscribe to Premium Plan (Priority: P1)

Free users can view pricing comparison in their local currency and complete checkout to subscribe to Monthly Premium or Yearly Premium, gaining immediate access to premium content upon successful payment.

**Why this priority**: Core revenue-generating functionality. Without working subscription checkout, the entire premium business model cannot function. This is the primary path for monetization.

**Independent Test**: Create a test user account, navigate to pricing page, verify currency-specific pricing displays (e.g., USD $9.99/mo or INR ₹750/mo), select a plan, complete checkout with test payment method, and verify premium access is granted immediately after successful payment webhook processing.

**Acceptance Scenarios**:

1. **Given** a free user is logged in and viewing the pricing page in USD region, **When** they click "Subscribe Monthly" and complete Stripe checkout with valid payment, **Then** they are charged $9.99, their account is upgraded to Monthly Premium status and premium content becomes accessible immediately.
2. **Given** a free user from India views pricing in INR, **When** they select Yearly Premium plan and complete checkout successfully, **Then** they are charged ₹7500, their subscription status shows "Yearly Premium" with next billing date one year from today, and premium features are unlocked.
3. **Given** a user starts checkout but clicks "Cancel" on the Stripe payment page, **When** they return to the platform, **Then** they remain on Free tier and see appropriate messaging explaining checkout was cancelled.

---

### User Story 2 - Apply Discount Code at Checkout (Priority: P2)

Users with earned reward discount codes can apply them during checkout to receive reduced subscription pricing in their selected currency, with the code marked as used after successful payment.

**Why this priority**: Reward integration drives engagement and provides incentive for users to complete learning tasks. Links the gamification system to revenue conversion.

**Independent Test**: Generate a valid discount code for a test user, initiate checkout in specific currency, apply the code, verify discounted pricing shows in checkout session with correct currency formatting, complete payment, and confirm code is marked as used and cannot be reused.

**Acceptance Scenarios**:

1. **Given** a user has an unused 20% discount code in their rewards, **When** they apply it during Monthly Premium checkout in USD, **Then** the checkout session displays $7.99 instead of $9.99 and the discount code is validated.
2. **Given** a user applies a valid discount code and completes payment successfully in INR, **When** the payment confirmation webhook is processed, **Then** the discount code is permanently marked as "used" and cannot be applied to future subscriptions.
3. **Given** a user enters an invalid or already-used discount code, **When** they attempt checkout, **Then** an error message displays explaining the code is invalid, and checkout proceeds at full price without the discount.

---

### User Story 3 - Manage Subscription and Billing (Priority: P3)

Subscribers access billing portal to view payment history, download invoices, update payment methods, and manage subscription settings (cancel/reactivate) with changes taking effect according to billing cycle rules.

**Why this priority**: Essential for user autonomy and reducing support burden. Users need self-service capabilities to manage their subscriptions without admin intervention.

**Independent Test**: From a premium subscriber account, access billing management, verify portal link redirects to Stripe hosted page, attempt payment method update, initiate cancellation, verify access continues until period end, and test reactivation before period expires.

**Acceptance Scenarios**:

1. **Given** a premium subscriber clicks "Manage Billing" from their profile, **When** the billing portal opens, **Then** they see their current subscription details, next billing date, payment history with downloadable invoices, and options to update payment method.
2. **Given** a subscriber chooses to cancel their subscription, **When** they confirm cancellation in the portal, **Then** their account is marked for cancellation at period end, they retain premium access until that date, and a cancellation confirmation is displayed.
3. **Given** a subscriber updates their credit card information in the portal, **When** the next billing cycle occurs, **Then** the new payment method is charged successfully without service interruption.

---

### User Story 4 - Automatic Subscription Lifecycle Handling (Priority: P4)

The system automatically processes subscription lifecycle events including successful renewals, failed payments with grace periods, expirations, and downgrades to Free tier when subscriptions end, ensuring database state remains synchronized with payment provider.

**Why this priority**: Automation reduces manual intervention and ensures consistent access control. Critical for business operations but lower priority than user-facing checkout flows.

**Independent Test**: Simulate webhook events for payment success, payment failure, subscription deletion, and invoice payment in test environment, then verify database records update correctly and user access levels adjust automatically.

**Acceptance Scenarios**:

1. **Given** a subscription renewal payment succeeds, **When** the payment confirmation webhook is received, **Then** the subscription period end date extends by one billing cycle and the user maintains premium access without interruption.
2. **Given** a subscription payment fails, **When** retry attempts fail for 3 days, **Then** the subscription cancelled webhook triggers downgrade to Free tier and the user receives notification about payment failure.
3. **Given** a user's subscription expires naturally at period end after cancellation, **When** the period end date passes, **Then** the system automatically downgrades account to Free tier and restricts access to premium-only content.

---

### User Story 5 - Admin Configure Currency Pricing (Priority: P5)

Administrators can configure subscription pricing for multiple currencies, setting custom amounts for Monthly and Yearly Premium tiers per currency to account for regional purchasing power and market conditions.

**Why this priority**: Essential for global expansion but can start with default USD pricing. Enables market-specific pricing strategies without code changes.

**Independent Test**: Login as admin, access currency configuration interface, add/update pricing for INR currency (Monthly: ₹750, Yearly: ₹7500), save changes, then verify pricing page shows new INR amounts for users in that region.

**Acceptance Scenarios**:

1. **Given** an administrator accesses currency management settings, **When** they add a new currency (e.g., EUR) with Monthly Premium €8.99 and Yearly Premium €89, **Then** the system saves the pricing configuration and displays EUR options to users in European regions.
2. **Given** an administrator updates existing INR pricing from ₹750/mo to ₹699/mo, **When** they save the changes, **Then** all new checkout sessions for INR users reflect the updated pricing immediately.
3. **Given** a currency has no configured pricing, **When** a user from that region views the pricing page, **Then** the system falls back to USD pricing with appropriate currency conversion or prompts the user to select an available currency.

---

### User Story 6 - View Plan Comparison and Features (Priority: P6)

All users can view a clear comparison of Free, Monthly Premium, and Yearly Premium tiers with feature lists, pricing in their local currency, and savings calculations to make informed upgrade decisions.

**Why this priority**: Important for conversion but doesn't require backend integration. Can be implemented with static content. Lowest technical priority but high marketing value.

**Independent Test**: Navigate to pricing page as both authenticated and unauthenticated user from different regions, verify all three tiers display with correct currency-specific pricing, feature lists are accurate and readable, and CTA buttons behave appropriately based on authentication state.

**Acceptance Scenarios**:

1. **Given** a user from the US (authenticated or not) visits the pricing page, **When** the page loads, **Then** three pricing cards display side-by-side showing Free ($0), Monthly Premium ($9.99/mo), and Yearly Premium ($99/year) with feature comparison.
2. **Given** a user from India views the pricing page, **When** the page loads, **Then** pricing displays in INR format (Free: ₹0, Monthly Premium: ₹750/mo, Yearly Premium: ₹7500/year) with proper currency symbol and formatting.
3. **Given** a free user views the pricing comparison, **When** they review the Yearly Premium card, **Then** a "Save 17%" badge or similar indicator clearly shows the annual savings compared to monthly billing in their selected currency.
4. **Given** an already-subscribed user views pricing, **When** the page loads, **Then** their current plan shows "Current Plan" badge and the button is disabled or shows "Manage" instead of "Subscribe".

### Edge Cases

- When a user attempts to subscribe while already having an active subscription, the system should detect the existing subscription and redirect to upgrade/downgrade flow instead of creating duplicate subscriptions.
- If a webhook arrives out of order (e.g., payment success before checkout completion), the system must use idempotency keys and transaction IDs to prevent duplicate processing and maintain data consistency.
- When a discount code is applied but payment fails, the code should be temporarily reserved for 24 hours allowing the user to retry checkout with the same code, then automatically released back to unused status if no successful payment occurs within that window.
- If a user cancels their subscription but then immediately attempts to reactivate before the period ends, the system should allow reactivation by removing the cancellation flag rather than creating a new subscription.
- When webhook signature verification fails, the system must reject the webhook entirely, log the security event, and alert administrators without applying any state changes.
- If Stripe service is temporarily unavailable during checkout initiation, the system should display user-friendly error message and allow retry without corrupting user data or creating partial checkout sessions.
- When a subscription is downgraded from Yearly to Monthly mid-cycle, Stripe handles proration calculations and the system must accurately reflect the new billing amount and next charge date.
- If a user's detected currency has no configured pricing, the system should offer available currency options or default to USD pricing with clear indication of the currency being charged.
- When an administrator updates currency pricing while users have active checkouts in that currency, existing checkout sessions should honor the original pricing shown when the session was created to prevent user confusion.
- If a user manually switches currency on the pricing page and then completes checkout, the system must persist the selected currency in session storage, pass it to the checkout session creation, charge in the selected currency, and store the currency code with the subscription for accurate display in billing history.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide three distinct subscription tiers: Free (default, no charge), Monthly Premium, and Yearly Premium with pricing configurable per currency
- **FR-002**: System MUST support multiple currencies including USD, INR, EUR, JPY, GBP, and other major currencies with admin-configurable pricing for each
- **FR-003**: System MUST detect user's currency preference based on browser locale or manual selection and display pricing accordingly (IP geolocation detection is optional for future enhancement)
- **FR-004**: System MUST provide administrative interface for configuring subscription pricing per currency (e.g., USD: $9.99/mo, INR: ₹750/mo, EUR: €8.99/mo) with validation rules: prices must be non-negative, cannot exceed $9999 equivalent per currency, and system displays warnings when prices differ by more than 50% from USD baseline for review
- **FR-005**: System MUST create secure checkout sessions that redirect users to hosted payment page with pricing in the user's selected currency. Before creating a checkout session, system MUST verify the user does not already have an active subscription and return an error if one exists to prevent duplicate subscriptions.
- **FR-006**: System MUST validate and apply discount codes during checkout, reducing the subscription price according to the reward code's percentage or fixed amount in the checkout currency
- **FR-007**: System MUST process webhook events from payment provider to synchronize subscription status, including checkout completion, payment success, payment failure, and subscription cancellation. On internal processing failures (database errors, network issues), system MUST return 500 status to trigger payment provider's automatic retry mechanism (up to 3 days) and log failures for manual review if retries are exhausted.
- **FR-008**: System MUST verify webhook authenticity using cryptographic signatures before processing any webhook data
- **FR-009**: System MUST store customer identifier, subscription identifier, and currency code from payment provider, never storing sensitive payment information locally
- **FR-010**: System MUST provide access to Stripe's hosted billing portal where subscribers can view invoices with currency-specific amounts (verified during portal configuration), update payment methods, and manage subscription settings
- **FR-011**: System MUST maintain subscription status values including active, past_due, cancelled, and incomplete with corresponding access control rules. During the grace period after payment failure, system displays "Active (Payment Issue)" status to users with a warning banner prompting payment method update while maintaining full premium access.
- **FR-012**: System MUST preserve premium access for cancelled subscriptions until the current billing period ends
- **FR-013**: System MUST automatically downgrade accounts to Free tier when subscriptions expire or payments fail beyond grace period
- **FR-014**: System MUST mark discount codes as permanently used after successful payment webhook confirms subscription creation
- **FR-015**: System MUST prevent multiple concurrent subscriptions for the same user account
- **FR-016**: System MUST gate premium content and features based on current subscription status, blocking access for Free tier users
- **FR-017**: System MUST display current subscription status, next billing date with currency, current plan pricing, and cancellation state in user profile area. When subscription has payment issues during grace period, display "Active (Payment Issue)" status with prominent warning banner encouraging payment method update.
- **FR-018**: System MUST handle subscription changes through new checkout sessions with proration calculations managed by payment provider
- **FR-019**: System MUST log all subscription lifecycle events including creation, renewal, cancellation, failure, and currency with amounts for audit purposes
- **FR-020**: System MUST format currency displays using appropriate symbols (₹ for INR, $ for USD, € for EUR, ¥ for JPY) and decimal conventions per locale
- **FR-021**: System MUST retry failed payment processing according to payment provider's Smart Retries configuration with a 3-day grace period before forcing downgrade to Free tier
- **FR-022**: System MUST support immediate plan upgrades and downgrades mid-cycle through new checkout sessions, with proration calculations managed automatically by the payment provider
- **FR-023**: System MUST enforce rate limiting on subscription API endpoints (checkout creation, discount code validation) to prevent abuse, limiting requests to 10 per minute per user
- **FR-024**: System MUST sanitize and validate all webhook payload data before processing to prevent injection attacks and ensure data integrity
- **FR-025**: System MUST enforce HTTPS-only access for all webhook endpoints with TLS 1.2 minimum to secure webhook communications
- **FR-026**: System MUST specify allowed event types for webhook processing (checkout.session.completed, invoice.paid, invoice.payment_failed, customer.subscription.deleted, customer.subscription.updated) and reject unknown event types
- **FR-027**: System MUST implement transactional processing for webhook events, ensuring all database changes commit atomically or rollback completely on errors
- **FR-028**: System MUST validate discount code format (alphanumeric only, 6-20 characters) and prevent brute force attacks by limiting validation attempts to 5 per user per 10-minute window
- **FR-029**: System MUST handle Stripe API errors gracefully, distinguishing between card errors (user-facing message), rate limit errors (retry with exponential backoff), and API errors (log and alert administrators)
- **FR-030**: System MUST never log or display sensitive payment information (card numbers, CVV) in any system logs, error messages, or debug output
- **FR-031**: System MUST implement checkout session timeout of 30 minutes, after which expired sessions cannot be completed and users must initiate new checkout
- **FR-032**: System MUST display user-friendly error messages for all checkout failure scenarios, including payment declined, network errors, expired sessions, and invalid discount codes
- **FR-033**: System MUST show loading indicators during checkout session creation, discount code validation, and subscription status updates with minimum 300ms display to prevent UI flashing
- **FR-034**: System MUST format subscription pricing displays with appropriate decimal places per currency (0 for JPY, 2 for USD/EUR/INR/GBP) according to ISO 4217 standards
- **FR-035**: System MUST provide responsive pricing page layout adapting to mobile (stacked cards), tablet (2-column grid), and desktop (3-column grid) viewports with touch-friendly buttons (minimum 44px height) on mobile devices

### Key Entities

- **Subscription**: Represents a user's premium membership, including tier (Monthly/Yearly), status (active, past_due, cancelled, incomplete), Stripe identifiers (Customer ID, Subscription ID), currency code (USD, INR, EUR, JPY, GBP), current period end date, and cancellation flag. Links to User entity.
- **Currency Pricing**: Configuration entity defining subscription prices per currency, including currency code, Monthly Premium price, Yearly Premium price, currency symbol, and active status. Managed by administrators.
- **Stripe Customer Reference**: External reference stored on User entity, uniquely identifying the user in Stripe's system for creating checkout sessions and accessing billing portal.
- **Discount Code**: Reward-based promotional code with redemption status (unused/used/reserved), discount percentage or fixed amount, validity dates, reservation timestamp (for 24-hour temporary holds), and associated user. Linked through existing Rewards system.
- **Webhook Event**: Record of lifecycle events received from Stripe, including event type, processing status, timestamp, currency, amount, and idempotency key to prevent duplicate processing.

## Clarifications

### Session 2025-12-09

- Q: When a webhook is received from Stripe but internal processing fails (e.g., database timeout, network issue), what should the system do? → A: Return 500 error to trigger Stripe's automatic retry mechanism (up to 3 days), log failure for manual review if retries exhausted
- Q: What should the grace period duration be before forcing downgrade after payment failure? → A: 3 days (aligns with Stripe Smart Retries and industry standards)
- Q: Should users be able to upgrade/downgrade plans mid-cycle, or must they wait until period end? → A: Allow immediate changes with proration handled by Stripe default settings
- Q: If a user opens multiple browser tabs and clicks "Subscribe" in both simultaneously, creating two checkout sessions at the same time, what should happen? → A: Check for existing active subscription before creating checkout session, return error if subscription already exists
- Q: When administrators configure subscription pricing for different currencies, should the system enforce any validation rules on the price amounts? → A: Allow $0+ only, enforce reasonable maximum ($9999 per currency), warn if prices differ by >50% from USD baseline
- Q: When a user applies a valid discount code at checkout but then abandons the checkout (closes browser without completing payment), what should happen to the discount code? → A: Code is temporarily reserved for 24 hours, then automatically released back to unused status if no payment succeeds
- Q: When a user's subscription payment fails and enters the 3-day grace period, what subscription status should be displayed to the user in their profile/dashboard? → A: Show "Active (Payment Issue)" status with warning banner prompting payment update, maintain premium access

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can complete subscription checkout from pricing page to premium access confirmation in under 3 minutes for 95% of successful transactions across all supported currencies
- **SC-002**: System processes webhook events and updates subscription status within 5 seconds of receipt for 99% of events
- **SC-003**: Zero instances of duplicate charges or subscription state inconsistencies over 30-day monitoring period
- **SC-004**: At least 95% of users who apply valid discount codes successfully complete checkout with correct reduced pricing in their selected currency
- **SC-005**: Cancelled subscriptions correctly maintain premium access until period end in 100% of test cases
- **SC-006**: Failed payment processing triggers downgrade to Free tier within the defined grace period (default 3 days) with 100% accuracy
- **SC-007**: Billing portal access successfully redirects and loads for 99% of subscriber requests within 2 seconds
- **SC-008**: Premium content access controls correctly gate features with less than 0.1% false positive or false negative rate across all content
- **SC-009**: Currency detection accurately identifies user's preferred currency based on browser locale with 90% accuracy (measured as percentage of users who do not manually change the auto-detected currency within 30 seconds of page load), with manual override available for all users
- **SC-010**: Administrators can configure new currency pricing and see changes reflected on pricing page within 30 seconds for 100% of updates
- **SC-011**: All currency amounts display with correct formatting (symbols, decimals, grouping) according to locale standards with 100% accuracy across supported currencies
- **SC-012**: Rate limiting prevents abuse with less than 0.01% false positive rate (legitimate users blocked) while blocking 100% of automated attack patterns
- **SC-013**: Checkout sessions expire after 30 minutes with 100% enforcement, requiring users to restart checkout flow for expired sessions
- **SC-014**: All webhook payload validation catches 100% of malformed or malicious data before database processing
- **SC-015**: Loading indicators display for all async operations with zero instances of UI flashing or abrupt state changes in user testing
- **SC-016**: Pricing page renders correctly on mobile, tablet, and desktop with 100% of touch targets meeting 44px minimum size requirement on mobile devices
- **SC-017**: User-friendly error messages display for all failure scenarios with 95%+ user comprehension rate in usability testing (measured by ability to take corrective action without support contact)
