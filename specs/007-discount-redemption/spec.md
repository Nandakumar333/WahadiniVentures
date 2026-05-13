# Feature Specification: Point-Based Discount Redemption System

**Feature Branch**: `007-discount-redemption`  
**Created**: December 5, 2025  
**Status**: Draft  
**Input**: User description: "Implement a point-based discount redemption system where users can exchange their earned reward points for subscription discounts or free trials. This system integrates with the existing rewards infrastructure and Stripe checkout flow."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Browse Available Discounts (Priority: P1)

Users need to discover what rewards they can obtain with their accumulated points before making redemption decisions.

**Why this priority**: This is the entry point to the entire discount system. Without the ability to view available discounts, users cannot engage with the redemption feature.

**Independent Test**: Can be fully tested by viewing the discount gallery and verifying all active, non-expired discounts are displayed with accurate point costs. Delivers value by informing users of their redemption options.

**Acceptance Scenarios**:

1. **Given** a logged-in user with 1000 points, **When** they navigate to the discounts page, **Then** they see all active discount offers with point costs, descriptions, and discount percentages
2. **Given** a user viewing the discount gallery, **When** they see a discount requiring 500 points and they have 1200 points, **Then** the discount shows as "Available to Redeem"
3. **Given** a user viewing the discount gallery, **When** they see a discount requiring 2000 points and they have 1200 points, **Then** the discount shows as "Insufficient Points" with a clear indicator of points needed
4. **Given** a user viewing discounts, **When** a discount has expired or reached its redemption limit, **Then** it does not appear in the available discounts list

---

### User Story 2 - Redeem Points for Discount Code (Priority: P1)

Users must be able to exchange their earned points for a specific discount code to use during checkout.

**Why this priority**: This is the core transaction of the system. Without redemption capability, the feature provides no value.

**Independent Test**: Can be fully tested by selecting a discount, confirming redemption, and verifying points are deducted and a valid code is issued. Delivers immediate value by providing the user with a usable discount code.

**Acceptance Scenarios**:

1. **Given** a user with 1000 points, **When** they redeem a discount costing 500 points, **Then** their point balance reduces to 500 and they receive a valid discount code
2. **Given** a user attempting to redeem a 1000-point discount, **When** they only have 800 points, **Then** the redemption is blocked with a clear error message
3. **Given** a user redeeming a one-time-per-user discount, **When** they attempt to redeem the same discount again, **Then** they receive an error indicating they have already redeemed this offer
4. **Given** a successful redemption, **When** the transaction completes, **Then** a negative reward transaction record is created for audit purposes
5. **Given** two users simultaneously attempting to redeem the last available discount (max redemptions = 100, current = 99), **When** both submit redemption requests, **Then** only one succeeds and the other receives an "offer no longer available" message

---

### User Story 3 - View My Redeemed Discount Codes (Priority: P2)

Users need access to their previously redeemed discount codes so they can copy and use them during checkout.

**Why this priority**: Essential for user experience, but can be delivered after core redemption functionality. Users need this to actually use their codes.

**Independent Test**: Can be fully tested by redeeming multiple discounts and verifying all codes are displayed in the user's discount wallet with expiry dates and usage status. Delivers value by providing easy access to redeemed codes.

**Acceptance Scenarios**:

1. **Given** a user who has redeemed 3 discount codes, **When** they navigate to "My Discounts", **Then** they see all 3 codes with their expiry dates and usage status
2. **Given** a user viewing their discount codes, **When** they click on a code, **Then** it is copied to their clipboard with a success notification
3. **Given** a user viewing an expired code, **When** the code is past its expiry date, **Then** it is clearly marked as "Expired" and visually distinguished from active codes
4. **Given** a user with no redeemed codes, **When** they view "My Discounts", **Then** they see a helpful message with a link to browse available discounts

---

### User Story 4 - Admin Creates and Manages Discount Types (Priority: P2)

Administrators need to create new discount offerings and configure their parameters to run promotional campaigns.

**Why this priority**: Required for system operation but can be implemented after user-facing redemption. Initial discounts can be seeded via database scripts.

**Independent Test**: Can be fully tested by creating a new discount type through admin interface and verifying it appears in the user discount gallery. Delivers value by enabling campaign management.

**Acceptance Scenarios**:

1. **Given** an admin user, **When** they create a new discount type with name "50% Off Annual", point cost 2000, discount percentage 50%, **Then** the discount becomes available in the user gallery
2. **Given** an admin editing a discount, **When** they set an expiry date of December 31, 2025, **Then** the discount stops appearing in the gallery after that date
3. **Given** an admin creating a discount, **When** they set max redemptions to 100, **Then** the system prevents the 101st redemption
4. **Given** an admin, **When** they deactivate a discount, **Then** it immediately stops appearing in the user gallery but existing redemptions remain valid

---

### User Story 5 - Admin Views Redemption Analytics (Priority: P3)

Administrators need visibility into redemption patterns to measure campaign effectiveness and plan future offerings.

**Why this priority**: Valuable for business intelligence but not critical for initial launch. Can be added after core functionality is proven.

**Independent Test**: Can be fully tested by viewing the analytics dashboard after multiple redemptions and verifying accurate statistics. Delivers value through business insights.

**Acceptance Scenarios**:

1. **Given** an admin viewing the analytics dashboard, **When** 50 users have redeemed the "SAVE10" discount, **Then** the dashboard shows 50 total redemptions and 25,000 total points burned (if cost is 500)
2. **Given** an admin viewing discount performance, **When** they select a specific discount type, **Then** they see redemption trends over time and top redeeming users
3. **Given** multiple active discounts, **When** the admin views the analytics page, **Then** discounts are ranked by total redemptions and points burned

---

### Edge Cases

- **Simultaneous Point Transactions**: When a user has exactly enough points and simultaneous transactions occur (earning + spending), optimistic concurrency control via RowVersion will cause one transaction to fail with retry prompt. The failed transaction must be retried after the successful one commits.

- **Discount Expires During Redemption**: If a discount expires between page load and redemption submission, the availability check in the transaction will detect expiry and return "DISCOUNT_UNAVAILABLE" error. User must refresh to see updated gallery.

- **Unredeemed Codes**: If a user redeems a code but never uses it at checkout, the code remains valid until its expiry date. The system does not automatically reclaim points. Future webhook integration (out of scope) would track actual usage.

- **Stripe API Failures During Validation**: During admin discount creation, if Stripe API call fails or times out (>10 seconds), the system logs a warning but allows discount creation to proceed. The admin is responsible for ensuring the Stripe code exists. Real-time validation is optional and non-blocking.

- **Max Redemptions Race Condition**: When max redemptions is reached between page load and redemption attempt, the availability check within the database transaction will detect CurrentRedemptions >= MaxRedemptions and return "DISCOUNT_UNAVAILABLE" error with 400 status code.

- **Timezone Handling**: All timestamps are stored in UTC in the database. Expiry date comparisons are performed in UTC. Frontend converts timestamps to user's browser timezone for display using JavaScript Date APIs. Campaign expiry dates are compared at midnight UTC.

- **Admin Deletes Redeemed Discount**: Discount types use soft delete (IsDeleted flag) to preserve foreign key integrity. When an admin deletes a discount type, IsDeleted is set to true, it disappears from the gallery, but existing UserDiscountRedemption records remain intact with their codes valid until expiry.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display all active discount offers that are not expired and have not reached their maximum redemption limit
- **FR-002**: System MUST validate user has sufficient points before allowing redemption
- **FR-003**: System MUST validate user has not previously redeemed a one-time-per-user discount before allowing redemption
- **FR-004**: System MUST deduct points from user's account in the same atomic transaction as creating the redemption record
- **FR-005**: System MUST generate or provide a discount code upon successful redemption
- **FR-006**: System MUST create a negative reward transaction record for every point deduction during redemption
- **FR-007**: System MUST prevent race conditions during concurrent redemption attempts using optimistic concurrency control
- **FR-008**: System MUST calculate discount expiry date based on redemption date plus configured duration days
- **FR-009**: System MUST enforce global maximum redemption limits across all users
- **FR-010**: System MUST allow administrators to create discount types with configurable name, code, point cost, percentage, expiry date, and redemption limits
- **FR-011**: System MUST allow administrators to activate/deactivate discount types
- **FR-012**: System MUST provide users access to their redemption history showing code, redemption date, expiry date, and usage status
- **FR-013**: System MUST allow users to copy discount codes to clipboard from their redemption history
- **FR-014**: System MUST provide administrators visibility into total redemptions and points burned per discount type
- **FR-015**: System MUST integrate with Stripe by accepting discount codes that are pre-configured in Stripe as Promotion Codes
- **FR-016**: System MUST secure all administrative endpoints with admin role authorization
- **FR-017**: System MUST validate discount codes exist in Stripe before making them available for redemption
- **FR-018**: System MUST handle discount types as either static codes (same code for all users) or unique codes (generated with unique suffixes per user)
- **FR-019**: System MUST handle Stripe API failures with retry logic (3 attempts with exponential backoff) and graceful degradation
- **FR-020**: System MUST store and compare all timestamps in UTC and convert to user's local timezone for display
- **FR-021**: System MUST provide paginated responses for user redemption history with configurable page size (default 20, max 100)
- **FR-022**: System MUST timeout Stripe API validation calls after 10 seconds and log failures without blocking discount creation
- **FR-023**: System MUST display loading states for all asynchronous operations (discount list fetch, redemption submission, code copying)
- **FR-024**: System MUST support keyboard navigation for all interactive discount UI elements (WCAG 2.1 Level AA compliance)
- **FR-025**: System MUST implement retry logic for transient database failures (up to 3 attempts with 1-second intervals)
- **FR-026**: System MUST provide user notifications when redemption fails after all retry attempts with actionable error messages
- **FR-027**: System MUST validate all admin inputs with FluentValidation rules (name length, code format, point cost range, percentage bounds)
- **FR-028**: System MUST enforce soft delete for discount types to preserve foreign key integrity with existing redemptions
- **FR-029**: System MUST implement rate limiting of 10 redemption requests per user per minute to prevent abuse
- **FR-030**: System MUST display zero-state UI when no discounts are available or user has zero points balance
- **FR-031**: System MUST refresh discount availability in real-time when user navigates from redemption confirmation back to gallery
- **FR-032**: System MUST generate unique code suffixes using the last 8 characters of the redemption GUID to prevent collisions

### Key Entitiesal Requirements

#### Performance Requirements
- **NFR-001**: Discount gallery page MUST load within 2 seconds for catalogs up to 1000 discount types (95th percentile)
- **NFR-002**: Redemption transaction MUST complete within 5 seconds for 95% of requests under normal load
- **NFR-003**: System MUST handle 50+ concurrent redemption requests without double-spending (100% accuracy)
- **NFR-004**: Database queries for redemption history MUST use indexed columns (UserId, DiscountTypeId, RedeemedAt)
- **NFR-005**: Frontend discount list MUST use React Query caching with 5-minute stale time to reduce API calls
- **NFR-006**: Point balance updates MUST reflect within 500ms after successful redemption

#### Security Requirements
- **NFR-007**: All user endpoints MUST require valid JWT authentication token with [Authorize] attribute
- **NFR-008**: All admin endpoints MUST enforce [Authorize(Policy = "RequireAdmin")] role-based authorization
- **NFR-009**: All input validation MUST occur server-side using FluentValidation before processing
- **NFR-010**: SQL injection prevention MUST be ensured through Entity Framework parameterized queries
- **NFR-011**: Rate limiting MUST restrict users to 10 redemption attempts per minute (per user ID)
- **NFR-012**: Audit trail (RewardTransaction) records MUST be append-only with no delete operations permitted
- **NFR-013**: Discount codes MUST only be visible to users who have redeemed them (no public code exposure)

#### Scalability Requirements
- **NFR-014**: System MUST support 10,000+ active users with linear performance degradation
- **NFR-015**: System MUST handle 1,000+ redemptions per day with consistent sub-5s response times
- **NFR-016**: Database MUST support 100+ concurrent discount campaigns without performance impact
- **NFR-017**: Reward transaction audit trail MUST use time-based partitioning (monthly partitions) for performance

#### Accessibility Requirements
- **NFR-018**: All discount UI components MUST support keyboard navigation (Tab, Enter, Escape keys)
- **NFR-019**: All interactive elements MUST have visible focus indicators meeting WCAG 2.1 Level AA contrast ratios (4.5:1)
- **NFR-020**: Screen reader announcements MUST be provided for redemption success/failure states using ARIA live regions
- **NFR-021**: All discount images MUST have descriptive alt text for screen reader users

#### Usability Requirements
- **NFR-022**: Mobile UI MUST support all redemption functionality without horizontal scrolling (responsive down to 320px)
- **NFR-023**: Error messages MUST be user-friendly with actionable guidance (e.g., "You need 500 more points" not "Validation failed")
- **NFR-024**: Loading states MUST be displayed for all async operations >500ms with spinner/skeleton UI
- **NFR-025**: Discount code copying MUST provide immediate user feedback (success toast notification with checkmark)
- **NFR-026**: Redemption confirmation modal MUST clearly display point cost and final balance before submission

### Key Entities

- **Discount Type**: Represents a redeemable offer configuration including name, code pattern, point cost, discount percentage, global expiry date, maximum total redemptions, duration days (how long code is valid after redemption), and active status
- **User Discount Redemption**: Represents a single user's redemption of a discount type, including which user redeemed it, which discount type, the specific code issued, redemption timestamp, calculated expiry date for that code, and usage status
- **Reward Transaction**: Existing entity that will be extended to record point deductions for redemptions (negative transaction with reason "Redeemed [Discount Name]")

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can view all available discounts and complete a redemption in under 30 seconds
- **SC-002**: System prevents double-spending with 100% accuracy under concurrent load (tested with 50 simultaneous redemption attempts)
- **SC-003**: 95% of users successfully copy their discount code on first attempt
- **SC-004**: Redemption transaction failure rate is below 0.1% (excluding user errors like insufficient points)
- **SC-005**: Admin can create a new discount type and have it appear in user gallery within 5 seconds
- **SC-006**: Point deduction and discount issuance happen atomically with zero inconsistencies
- **SC-007**: Users attempting to redeem with insufficient points receive clear error message 100% of the time
- **SC-008**: System accurately enforces one-per-user limits with zero duplicate redemptions per user per discount type
- **SC-009**: Analytics dashboard loads redemption statistics for up to 1000 discount types within 2 seconds

## Assumptions *(mandatory)*

1. Discount codes (e.g., "SAVE10", "SAVE20") are pre-created in Stripe as Promotion Codes before they are configured in the system
2. The system manages access control to these codes (who can use them) but does not create the codes in Stripe programmatically
3. Users understand that points are non-refundable once redeemed for a discount code
4. A single discount code can be used only once per Stripe subscription checkout
5. The existing reward points system is already functional and accurate
6. Admin users have appropriate permissions configured in the existing authorization system
7. Users have email notifications enabled for successful redemptions (optional enhancement, not in scope)
8. Stripe webhook integration for tracking code usage is out of scope for this feature
9. Currency conversion for international users is handled by existing subscription logic
10. Mobile responsive design will follow existing platform standards

## Out of Scope *(mandatory)*

1. Automatic creation of Stripe Promotion Codes via API
2. Integration with payment gateway beyond accepting pre-configured Stripe codes
3. Refund mechanism for redeemed discount codes
4. Transferring discount codes between users
5. Secondary marketplace for trading discount codes
6. Machine learning recommendations for which discounts to redeem
7. Gamification features like "discount of the day" or "flash sales"
8. Email/SMS notifications for new discount availability
9. Social sharing of discount codes
10. Integration with external coupon/deal aggregation platforms
11. Multi-currency point conversion for international redemptions
12. Bulk redemption of multiple discount types in a single transaction
13. Partial point refunds for unused or expired codes
14. User-to-user gifting of discount codes

## Dependencies

### System Dependencies
- Existing reward points system must be operational and accurate
- User authentication and authorization system must support admin role checks
- Stripe account must have Promotion Codes feature enabled
- PostgreSQL database with transaction support for atomic operations

### External Dependencies
- Stripe API availability for validating promotion codes during discount configuration
- Stripe Promotion Codes must be pre-created by administrators in Stripe dashboard

### Feature Dependencies
- User profile system must expose current point balance
- Existing reward transaction system must support negative transactions for point deductions

## Constraints

### Technical Constraints
- Must maintain ACID properties for all point deduction and redemption transactions
- Must use optimistic concurrency control to prevent race conditions
- Must integrate with existing Entity Framework Core data layer
- Must follow Clean Architecture patterns established in the codebase
- Must use existing JWT authentication and authorization middleware

### Business Constraints
- Discount codes must be manually pre-created in Stripe before system configuration
- Point costs must be positive integers (no fractional points)
- Discount percentages must be between 0 and 100
- Maximum redemptions must be non-negative (0 = unlimited)
- Duration days must be positive (minimum 1 day)

### User Experience Constraints
- Redemption flow must complete in under 5 seconds for 95% of transactions
- UI must clearly distinguish between available, unavailable (insufficient points), and expired discounts
- Error messages must be actionable and user-friendly
- Mobile interface must support all redemption functionality without horizontal scrolling

## Security Considerations

1. **Authorization**: All admin endpoints must enforce [Authorize(Policy = "RequireAdmin")] with RequireAdmin policy defined in authentication configuration
2. **Concurrency**: Optimistic concurrency control via EF Core RowVersion attribute must prevent double-spending of points with 100% accuracy
3. **Transaction Integrity**: Point deduction and redemption must be atomic using database transactions with IsolationLevel.ReadCommitted
4. **Input Validation**: All user inputs must be validated server-side using FluentValidation with specific rules:
   - Discount name: 3-100 characters, alphanumeric with spaces
   - Stripe code: 3-50 characters, alphanumeric with hyphens
   - Point cost: 1-1,000,000 integer
   - Discount percentage: 0-100 decimal with 2 decimal places
   - Duration days: 1-365 integer
   - Max redemptions: 0-1,000,000 integer
5. **Audit Trail**: All point deductions must create permanent, append-only RewardTransaction records with no delete operations
6. **Rate Limiting**: Implement rate limiting of 10 redemption requests per user per minute using in-memory cache with sliding window algorithm
7. **Code Exposure**: Discount codes must only be returned in API responses for:
   - Users who have redeemed them (GET /discounts/my-redemptions)
   - Admin users viewing all redemptions (GET /admin/discounts/redemptions)
8. **Admin Access**: Admin analytics must not expose PII (email, full name) unless explicitly required; use anonymized user IDs in analytics dashboards
9. **CSRF Protection**: All state-changing endpoints (POST, PUT, DELETE) must include anti-forgery token validation
10. **API Timeout Protection**: All Stripe API calls must have 10-second timeout with circuit breaker pattern to prevent cascading failures

## Open Questions

None - all critical aspects have been clarified based on the feature description and reasonable industry standards.

## Notes

- Initial discount types should be seeded via database migration for testing purposes
- Consider implementing soft delete for discount types to preserve historical redemption data
- Future enhancement: Implement Stripe webhook to track when redeemed codes are actually used at checkout
- Future enhancement: Add email notifications for successful redemptions
- Future enhancement: Implement expiry reminder notifications (e.g., "Your code expires in 3 days")
