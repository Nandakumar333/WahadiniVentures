# Feature Specification: Reward Points, Leaderboard & Achievements

**Feature Branch**: `006-reward-system`  
**Created**: 2025-12-04  
**Status**: Draft  
**Input**: User description: "Implement a comprehensive gamification system to drive user engagement through reward points, competitive leaderboards, unlockable achievements, daily login streaks, and referral incentives."

## Clarifications

### Session 2025-12-04

- Q: How do we handle timezones for daily streaks? → A: Use UTC for all server-side calculations. A "day" is defined as a 24-hour UTC window (00:00 to 23:59 UTC).
- Q: What happens if a user completes a lesson multiple times? → A: Points awarded for first completion only.
- Q: How often is the leaderboard updated? → A: Cache 15 minutes for high-traffic leaderboards.
- Q: Can points be negative? → A: No, current balance cannot go below zero.
- Q: Are achievements retroactive? → A: Yes, scan existing data for new achievements.
- Q: How do we prevent race conditions on point balance? → A: Database transactions with optimistic concurrency.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Earn Points For Learning (Priority: P1)

Learners receive reward points the first time they complete eligible lessons, tasks, or courses so that progress feels rewarding and motivates continued study.

**Why this priority**: Without reliable point awards tied to core learning actions, the gamification system fails to deliver immediate value and loses credibility.

**Independent Test**: Trigger completion events for a single learner and verify that points post once per eligible activity with the correct reason and amount.

**Acceptance Scenarios**:

1. **Given** a learner has not completed Lesson A before, **When** they finish Lesson A, **Then** the system records a point transaction with the configured lesson reward and updates the learner's balance accordingly.
2. **Given** a learner already earned points for Task B, **When** they repeat Task B, **Then** the system logs the attempt without issuing additional points.

---

### User Story 2 - Track Balance & History (Priority: P2)

Learners can view their current point balance and a dated transaction history so they can understand how points were earned or adjusted.

**Why this priority**: Transparency builds trust in the reward system and reduces support overhead caused by balance disputes.

**Independent Test**: Access the reward summary screen for a learner with mixed transactions and verify balance math, chronological ordering, pagination, and filtering.

**Acceptance Scenarios**:

1. **Given** a learner has at least three point transactions, **When** they open the reward history view, **Then** the system shows each entry with amount, category, descriptive label, and resulting balance in chronological order.
2. **Given** a learner with zero transactions, **When** they open the reward history view, **Then** the system displays an informative empty state without errors.

---

### User Story 3 - Maintain Daily Streaks (Priority: P3)

Learners maintain a daily login streak that grants escalating bonuses for consecutive participation, encouraging consistent engagement.

**Why this priority**: Daily streaks create habitual platform usage and drive repeat visits, amplifying overall learning time.

**Independent Test**: Simulate logins across multiple UTC days for one learner and verify streak count, reset rules, and bonus transactions.

**Acceptance Scenarios**:

1. **Given** a learner logged in yesterday, **When** they log in today within the next UTC day window, **Then** the system increments their streak, awards the daily bonus, and records the updated longest streak if applicable.
2. **Given** a learner missed two consecutive UTC days, **When** they next log in, **Then** the system resets the streak to one and issues only the base daily reward.

---

### User Story 4 - Compete On Leaderboards (Priority: P4)

Learners compare their performance on weekly, monthly, and all-time leaderboards while seeing their personal rank even if outside the top list.

**Why this priority**: Competitive visibility reinforces motivation and social proof, increasing retention for high-performing learners.

**Independent Test**: Generate sample data across periods, refresh leaderboards, and verify rank ordering, ties, caching interval, and personal rank visibility.

**Acceptance Scenarios**:

1. **Given** leaderboards were refreshed within the defined cache window, **When** a learner opens the weekly leaderboard, **Then** they see the top ranked learners for the current week and their own rank with associated point totals.
2. **Given** two learners share the same point total, **When** leaderboards render, **Then** the system applies the documented tie-breaking rule and displays consistent rankings across all views.

---

### User Story 5 - Unlock Achievements & Referrals (Priority: P5)

Learners unlock milestone achievements and earn referral bonuses when invited friends complete qualifying courses, providing long-term goals and growth incentives.

**Why this priority**: Achievements and referrals extend engagement beyond individual lessons, supporting user advocacy and broader platform adoption.

**Independent Test**: Trigger milestone events and referral completions for controlled accounts and verify badge unlocks, celebration messaging, and bonus point attribution.

**Acceptance Scenarios**:

1. **Given** a learner crosses a defined achievement threshold (e.g., five courses completed), **When** the event is processed, **Then** the system awards the corresponding badge, records the unlock date, and optionally grants bonus points per configuration.
2. **Given** a referred learner completes their first course, **When** referral eligibility checks pass, **Then** the system awards the referrer the configured bonus and marks the referral as fulfilled to prevent duplicate awards.

### Edge Cases

- If a learner completes multiple eligible actions simultaneously (e.g., finishing a lesson and the final course requirement), the system prioritizes deduplication so each configured reward fires exactly once.
- If a learner's streak login occurs near the UTC day boundary, the system evaluates timestamps using UTC to avoid timezone-based exploits or accidental resets.
- When an admin applies a negative adjustment that would drive the balance below zero, the system rejects the operation with a validation error. The available balance acts as the maximum deductible amount.
- If a referral code is reused by the same invitee or already redeemed, the system records the attempt without issuing additional points and flags it for monitoring.
- When leaderboards refresh while a user is viewing them, the system gracefully updates or notifies the viewer without duplicate pagination or inconsistent ranks.
- If notification service is unavailable during point award or achievement unlock, the system completes the reward transaction successfully and queues notification retry up to 3 attempts with exponential backoff (1s, 2s, 4s).
- When calculating weekly/monthly leaderboards, if transaction volume exceeds query timeout (30 seconds), the system returns cached stale data with staleness indicator rather than failing completely.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST award points for a learner’s first completion of eligible lessons, tasks, course milestones, or referral milestones according to a configurable reward schedule.
- **FR-002**: The system MUST maintain an immutable transaction ledger that records every point change with timestamp, amount, category, descriptive context, and reference IDs.
- **FR-003**: The system MUST present each learner with an accurate current balance, total lifetime points earned, and an auditable transaction history that reconciles with the ledger.
- **FR-004**: The system MUST enforce business rules that prevent duplicate awards for the same learner-action pair. Points are awarded only for the *first* completion of each eligible lesson, task, or course to prevent farming.
- **FR-005**: The system MUST evaluate daily streaks using UTC calendar days (00:00 to 23:59 UTC), incrementing, resetting, and issuing escalating bonuses based on consecutive logins. This prevents timezone hopping exploits.
- **FR-006**: The system MUST expose weekly, monthly, and all-time leaderboards that rank learners by points, apply deterministic tie-breaking, and cache for 15 minutes. High-traffic leaderboards (All-Time) refresh every 15 minutes; "My Rank" may use real-time calculation.
- **FR-007**: The system MUST unlock and persist achievement badges when learners meet milestone criteria and surface badge status (locked/unlocked, unlock date, description) to the learner. When new achievements are added, the system scans existing user data to retroactively award applicable badges.
- **FR-008**: The system MUST attribute referral codes to inviters, verify completion criteria for invitees, and issue referral bonuses once per successful referral.
- **FR-009**: The system MUST provide administrators with the ability to view any learner's transaction history and apply manual adjustments (bonuses or penalties) with required justification notes. Justification must be minimum 10 characters and stored in the transaction description. Admin adjustments create audit trail with admin user ID, timestamp, and reason.
- **FR-010**: The system MUST notify learners through in-app toast notifications when significant reward events occur: awards ≥100 points, streak milestones (days 5/10/30/100), any achievement unlock, and referral success. Notifications display point amount, icon, and auto-dismiss after 5 seconds.
- **FR-011**: The system MUST offer APIs or equivalent interfaces so frontend experiences can retrieve balances, histories, leaderboards, achievements, streak status, and referral progress in real time.
- **FR-012**: The system MUST support configuration of point values, streak bonus levels, leaderboard cache durations, and achievement thresholds without redeploying code.
- **FR-013**: The system MUST enforce that current point balance never goes below zero. Redemptions or deductions check for sufficient funds. Individual `RewardTransaction` records may have negative amounts (Redemption, Penalty), but the aggregate balance is non-negative.
- **FR-014**: The system MUST ensure idempotency for point award operations using deduplication keys (UserId + ReferenceId + TransactionType). Duplicate requests within 24 hours return existing transaction without creating new records.
- **FR-015**: The system MUST rollback partial transactions if any step fails (transaction creation, balance update, achievement check). All database operations execute within a single atomic transaction scope with isolation level Read Committed.

### Non-Functional Requirements

**Performance**:
- **NFR-001**: Balance retrieval API responds within 200ms for 95th percentile under normal load (100 concurrent users)
- **NFR-002**: Transaction history pagination responds within 200ms per page for 95th percentile
- **NFR-003**: Leaderboard queries complete within 500ms for 95th percentile with 100K+ user base
- **NFR-004**: Point award operations complete end-to-end within 100ms for 95th percentile

**Scalability**:
- **NFR-005**: System supports minimum 100 concurrent point award operations without conflicts
- **NFR-006**: RewardTransaction table partitioned by month after reaching 1M records
- **NFR-007**: Cache layer supports horizontal scaling migration path to Redis without application code changes

**Security**:
- **NFR-008**: Admin adjustment endpoints require Admin role authorization
- **NFR-009**: Users can only view their own transaction history (non-admins)
- **NFR-010**: All point value configurations validated against min/max bounds (0-10,000 points)
- **NFR-011**: Rate limiting applied: 100 requests/minute per user for balance/history, 10 requests/minute for leaderboards

**Observability**:
- **NFR-012**: All point transactions logged with structured logging (UserId, Amount, Type, Timestamp)
- **NFR-013**: Leaderboard cache hit rate metrics tracked and exposed for monitoring
- **NFR-014**: Failed transaction rollbacks trigger alerts after 10 failures within 5 minutes
- **NFR-015**: Achievement unlock events tracked in analytics with attribution data

**Reliability**:
- **NFR-016**: Point award idempotency prevents duplicate awards for same action within 24 hours
- **NFR-017**: Database transaction isolation level Read Committed prevents dirty reads
- **NFR-018**: Optimistic concurrency on User balance updates prevents lost updates
- **NFR-019**: System maintains eventual consistency between transaction ledger and denormalized balance within 100ms

### Key Entities *(include if feature involves data)*

- **RewardTransaction**: Immutable record of point changes including user, amount, category, reference entity, resulting balance snapshot, admin user ID (for manual adjustments), and audit metadata.
- **User** (Extended): Denormalized point totals (CurrentPoints, TotalPointsEarned), ReferralCode for invites, and RowVersion for optimistic concurrency control.
- **UserStreak**: Current and longest consecutive login counts with last activity timestamp used to determine streak continuity and bonus eligibility.
- **AchievementDefinition**: Code-defined catalog of milestone criteria, descriptive copy, optional bonus rewards (0-250 points), and display ordering used to evaluate unlocks.
- **UserAchievement**: Junction between learner and achievement definition capturing unlock status, unlock date, and bonus points awarded.
- **ReferralAttribution**: Mapping between inviter and invitee including code used, qualifying actions completed, and reward fulfillment status.

## Configuration *(required values)*

### Point Values
- **Lesson Completion**: 50 points (first completion only)
- **Task Approval**: 100 points (first completion only)
- **Course Completion**: 500 points (awarded when final lesson completed)
- **Referral Bonus**: 200 points (awarded when invitee completes first course)
- **Admin Bonus**: Variable (requires justification)
- **Admin Penalty**: Variable negative (requires justification, cannot exceed current balance)

### Streak Bonuses
- **Base Daily Login**: 5 points
- **Day 5 Milestone**: +10 bonus (15 total)
- **Day 10 Milestone**: +25 bonus (30 total)
- **Day 30 Milestone**: +100 bonus (105 total)
- **Day 100 Milestone**: +500 bonus (505 total)

### Achievement Definitions (MVP)
1. **First Steps** - Complete first lesson (0 bonus points)
2. **Task Master** - Complete first task (0 bonus points)
3. **Course Champion** - Complete first course (50 bonus points)
4. **Triple Threat** - Complete 3 courses (100 bonus points)
5. **Point Hoarder** - Earn 5,000 total points (0 bonus points)
6. **Streak Warrior** - Maintain 7-day streak (25 bonus points)
7. **Social Butterfly** - Refer 3 successful users (150 bonus points)
8. **Century Club** - Complete 100 lessons (250 bonus points)

### Leaderboard Configuration
- **Cache Duration**: 15 minutes (900 seconds)
- **Top Ranks Displayed**: 100 users
- **Tie-Breaking Rule**: Earlier registration timestamp (User.CreatedAt ASC)
- **Rank Calculation**: Dense ranking (ties share same rank, next rank continues sequentially)

### Transaction History Pagination
- **Default Page Size**: 20 transactions
- **Maximum Page Size**: 100 transactions
- **Sorting**: CreatedAt DESC (newest first)
- **Cursor Format**: Base64-encoded JSON `{"timestamp":"ISO8601","id":"guid"}`

### Notification Thresholds
- **Large Point Award**: ≥100 points in single transaction
- **Streak Milestone**: Days 5, 10, 30, 100
- **Achievement Unlock**: All achievements trigger notification
- **Referral Success**: When invitee completes qualifying course

## Assumptions

- Referral bonuses trigger only after the invitee completes at least one full course, aligning with the provided user story.
- Celebration messaging (toasts, badges, emails) leverages existing notification channels; no new delivery channel is introduced within this feature.
- Anti-fraud controls beyond duplicate prevention and referral reuse checks remain out of scope for this release.
- Race conditions on point balance are prevented using database transactions when inserting `RewardTransaction` and updating `User.CurrentPoints`, combined with optimistic concurrency control on the User record (RowVersion/ETag pattern).
- Configuration values are stored in `appsettings.json` (or environment variables) and can be updated without code deployment using configuration hot-reload.
- LeaderboardSnapshot entity is future optimization; MVP calculates ranks on-demand from RewardTransaction aggregations.
- RewardBalance entity mentioned in requirements is implemented as denormalized fields (CurrentPoints, TotalPointsEarned) on User entity for performance.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 95% of eligible learning and referral events issue the correct point award within one minute of the triggering action during acceptance testing.
- **SC-002**: Learners can load their balance and the latest 20 transactions within three seconds for 95% of requests during peak hours.
- **SC-003**: At least 60% of active learners unlock one achievement or streak milestone within the first 30 days after launch, indicating engagement with the system.
- **SC-004**: Leaderboard views maintain under two-second perceived load time for 95% of page visits while showing the requester’s current rank even when outside the displayed top bracket.
