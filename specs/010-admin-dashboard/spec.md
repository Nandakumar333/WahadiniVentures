# Feature Specification: Comprehensive Admin Dashboard

**Feature Branch**: `010-admin-dashboard`  
**Created**: 2026-03-04  
**Status**: Draft  
**Input**: User description: "Comprehensive Admin Dashboard for managing users, courses, tasks, rewards, and analytics"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Platform Health Overview (Priority: P1)

An admin navigates to the admin area and immediately sees a high-level summary of the platform's health: total users, premium subscribers, estimated monthly revenue, and the number of task submissions awaiting review. This birds-eye view lets the admin assess the platform state without querying any data manually.

**Why this priority**: Without visibility into critical KPIs, no informed administrative decision can be made. This is the entry point for all admin activity and the highest-value feature for day-to-day operations.

**Independent Test**: Can be fully tested by logging in as an admin and visiting the dashboard home; the page must display at least four KPI values populated from real platform data.

**Acceptance Scenarios**:

1. **Given** a logged-in admin user, **When** they navigate to `/admin`, **Then** they see summary cards showing Total Users, Premium Users, estimated MRR, and Pending Task Reviews — all reflecting current platform state.
2. **Given** the dashboard is loaded, **When** new users register or tasks are submitted between page loads, **Then** refreshing the page updates the displayed values.
3. **Given** a non-admin user, **When** they attempt to access `/admin`, **Then** they are redirected and shown an "Access Denied" message.

---

### User Story 2 - Task Review Queue (Priority: P2)

An admin visits the Task Review section and sees a queue of pending user submissions. For each submission, the admin can view the user's proof (text or image), provide optional written feedback, and either approve or reject the submission. Approval triggers a point reward for the user.

**Why this priority**: Pending task reviews directly block user progress and earnings. A backlog in reviews stalls the core task-to-earn loop that drives retention.

**Independent Test**: Can be fully tested by creating a test user submission, then approving/rejecting it via the admin interface and confirming the user's status and points reflect the outcome.

**Acceptance Scenarios**:

1. **Given** a pending task submission exists, **When** an admin visits the task review queue, **Then** the submission appears with the task name, submitting user's name, submission content, and any attached proof image.
2. **Given** an admin approves a submission with optional feedback, **When** the action is confirmed, **Then** the submission is marked Approved, points are credited to the user, and the item is removed from the pending queue.
3. **Given** an admin rejects a submission with feedback, **When** the action is confirmed, **Then** the submission is marked Rejected and the user can see the feedback message.
4. **Given** there are no pending submissions, **When** an admin visits the queue, **Then** an empty state message is displayed.

---

### User Story 3 - User Management (Priority: P3)

An admin can search, filter, and paginate through all platform users. For each user, the admin can view their profile details (email, role, subscription status, join date), modify their role, or ban/unban their account. Banned users cannot log in.

**Why this priority**: User management is critical for platform safety and moderation. Banning abusive accounts protects the community.

**Independent Test**: Can be tested by searching for a specific user, changing their role to "Admin", verifying the change persists, then banning the account and confirming login is denied for that user.

**Acceptance Scenarios**:

1. **Given** users exist in the system, **When** an admin searches by name or email, **Then** matching users are returned in a paginated table.
2. **Given** an admin changes a user's role, **When** the change is saved, **Then** the user's access level updates on their next authenticated action.
3. **Given** an admin bans a user, **When** that user attempts to log in, **Then** they are rejected with a message indicating the account is suspended.
4. **Given** an admin unbans a previously banned user, **When** that user attempts to log in, **Then** access is restored.
5. **Given** the user list has more than the page size, **When** the admin navigates pages, **Then** a different subset of users is shown each time without duplication.

---

### User Story 4 - Course & Lesson Management (Priority: P4)

An admin can create, edit, and publish courses and their nested lessons without database access. Each course has metadata (title, description, price, image URL), and each lesson has a title, rich-text content, and an optional video reference. Admins can reorder lessons and toggle course publish status.

**Why this priority**: Content creation is the foundation of the learning platform. Without this CMS capability, new courses require database-level access, creating a bottleneck for content updates.

**Independent Test**: Can be tested by creating a new course with two lessons via the admin CMS, publishing the course, and verifying it appears in the learner-facing catalog.

**Acceptance Scenarios**:

1. **Given** an admin creates a course with required metadata and publishes it, **When** a learner browses the course catalog, **Then** the new course appears with the correct details.
2. **Given** an admin edits an existing lesson's content, **When** the changes are saved, **Then** learners viewing that lesson see the updated content.
3. **Given** an admin unpublishes a course, **When** a learner attempts to access it, **Then** the course is no longer visible or accessible.
4. **Given** an admin reorders lessons within a course, **When** a learner views the course outline, **Then** lessons appear in the newly defined order.

---

### User Story 5 - Analytics Visualization (Priority: P5)

An admin can view charts showing revenue trends over the last 12 months, user signup trends, and course engagement metrics (which courses are most popular by enrollment or completion). This helps identify content gaps and revenue patterns.

**Why this priority**: Analytics drive strategic decisions but do not block operations. They add high value but are discoverable through other means in the short term.

**Independent Test**: Can be tested by generating sample data, visiting the analytics section, and verifying charts render with correct data points matching the source records.

**Acceptance Scenarios**:

1. **Given** payment records exist, **When** an admin views the revenue chart, **Then** a time-series chart displays monthly revenue for the past 12 months.
2. **Given** users have enrolled in courses, **When** an admin views course analytics, **Then** courses are ranked by enrollment count.
3. **Given** no data exists for a period, **When** an admin views the chart, **Then** the empty period is shown as zero rather than an error or blank gap.

---

### User Story 6 - Discount Code Management (Priority: P6)

An admin can create new discount codes (percentage-based or fixed-amount), set optional expiry dates and usage limits, view all existing codes with their redemption counts, and deactivate codes no longer needed.

**Why this priority**: Discount codes support marketing campaigns but do not block core platform functionality.

**Independent Test**: Can be tested by creating a percentage discount code, applying it during a simulated checkout, and confirming the redemption count increments and the correct price reduction is applied.

**Acceptance Scenarios**:

1. **Given** an admin creates a 20% discount code with a usage limit of 100, **When** users apply the code at checkout, **Then** 20% is deducted from the price until 100 uses are reached.
2. **Given** an admin deactivates a discount code, **When** a user attempts to apply it, **Then** they receive an error that the code is no longer valid.
3. **Given** discount codes exist, **When** an admin views the rewards management page, **Then** each code shows its type, value, usage count, and status.

---

### User Story 7 - Audit Log (Priority: P7)

Every significant administrative action (user ban, role change, task approval/rejection, course publish/unpublish, discount code creation/deactivation) is recorded in an audit log with the acting admin's identity, the action taken, the affected resource, and a timestamp. Admins can browse and filter this log.

**Why this priority**: Audit logs provide accountability and are important for compliance, but do not block day-to-day operations — they can be delivered after core features stabilize.

**Independent Test**: Can be tested by performing three distinct admin actions and confirming all three appear in the audit log with correct actor, action type, resource reference, and timestamp.

**Acceptance Scenarios**:

1. **Given** an admin bans a user, **When** any admin views the audit log, **Then** an entry appears with the acting admin's name, "Ban User" action, the affected user's ID, and the exact timestamp.
2. **Given** the audit log contains many entries, **When** an admin filters by action type, **Then** only entries matching that type are displayed.
3. **Given** an admin views audit details, **When** they click an entry, **Then** metadata about the action (before/after state where applicable) is shown.

---

### Edge Cases

- What happens when an admin attempts to ban themselves? The system must prevent self-ban and return an error.
- What happens when an admin unpublishes a course that has active enrolled users? The course is unpublished but user progress records are preserved; enrolled users see a "content unavailable" message.
- How does the system handle concurrent admin actions on the same submission (e.g., two admins approving the same task simultaneously)? The first review to complete is accepted; subsequent attempts return a conflict error showing the current status.
- What happens when a discount code's usage limit is reached mid-request? The final redemption within the limit is honored; subsequent requests are rejected.
- What happens when analytics data for a month is unavailable? Missing periods display as zero, not as gaps or errors.
- What happens when a non-admin authenticated user directly accesses an admin endpoint? The system returns a 403 Forbidden response.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST restrict all admin area access to users with the Admin role; users with any other role MUST receive an "Access Denied" response.
- **FR-002**: The admin dashboard home MUST display: total registered users, total premium/paid users, estimated MRR, and count of pending task submissions — all reflecting data at time of page load.
- **FR-003**: The admin MUST be able to search users by name or email and filter by role or subscription tier, with paginated results.
- **FR-004**: The admin MUST be able to view a user's full profile including registration date, role, subscription status, points balance, and ban status.
- **FR-005**: The admin MUST be able to change any user's role; admins MUST NOT be able to change their own role to a lower-privilege level.
- **FR-006**: The admin MUST be able to ban and unban user accounts; banned users MUST be denied authentication on the next login attempt.
- **FR-007**: The admin MUST be able to create, edit, and soft-delete courses; each course has a title, description, price, cover image URL, and category.
- **FR-008**: The admin MUST be able to add, edit, reorder, and soft-delete lessons within a course; each lesson has a title, rich-text body, and optional video reference.
- **FR-009**: The admin MUST be able to toggle a course's published state; unpublished courses MUST NOT be visible or accessible to learners.
- **FR-010**: The task review queue MUST display all submissions with "Pending" status, showing: submitting user, task name, submission content, and any attached image proof URL.
- **FR-011**: The admin MUST be able to approve or reject each pending task submission with an optional feedback message; approval MUST trigger the configured point reward for the submitting user.
- **FR-012**: The analytics section MUST display a revenue trend broken down by month for the trailing 12 months.
- **FR-013**: The analytics section MUST display user signup counts by month for the trailing 12 months and a course enrollment/view count ranking.
- **FR-014**: The admin MUST be able to create discount codes specifying: code string, type (percentage or fixed amount), value, optional expiry date, and optional maximum redemption count.
- **FR-015**: The admin MUST be able to deactivate any active discount code; deactivated codes MUST NOT be redeemable from that point forward.
- **FR-016**: The system MUST produce an immutable audit log entry for each of the following actions: role change, user ban/unban, task approval/rejection, course publish/unpublish, discount code creation/deactivation. Each entry MUST include acting admin ID, action type, affected resource ID, and UTC timestamp.
- **FR-017**: The admin MUST be able to browse the audit log filtered by action type and date range, with paginated results.
- **FR-018**: The system MUST prevent an admin from banning their own account.

### Key Entities

- **AdminDashboardStats**: Aggregated snapshot of platform KPIs (total users, premium users, MRR estimate, pending tasks count, signup trend data, revenue trend data).
- **UserSummary**: A read projection of a platform user as seen by an admin — includes ID, display name, email, role, subscription tier, join date, ban status, and points balance.
- **TaskSubmission** *(existing, extended)*: A user's submitted proof for a course task; lifecycle states: Pending → Approved | Rejected. Extended with reviewer ID, reviewer feedback, and reviewed-at timestamp.
- **Course** *(existing, extended)*: A learning course containing metadata and a published/unpublished state flag.
- **Lesson** *(existing, extended)*: An ordered content unit within a course; extended with rich-text body content and optional video reference.
- **DiscountCode** *(existing)*: A redeemable promotional code with a type (percentage | fixed), value, usage limit, expiry date, and active/inactive status.
- **AuditLogEntry**: An immutable record of an admin action — acting admin ID, action type (enumerated), resource type, resource ID, optional before/after payload snapshot, and UTC timestamp.
- **ChartDataPoint**: A label-value pair used to populate analytics charts (period label, numeric value).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An admin can reach the dashboard overview and see all KPI cards populated with accurate data within 3 seconds of navigating to `/admin` on a standard connection.
- **SC-002**: An admin can review (approve or reject with feedback) a pending task submission in under 60 seconds from arriving at the review queue.
- **SC-003**: An admin can create and publish a new course with at least two lessons in under 10 minutes without any direct database or code access.
- **SC-004**: User search and filter operations return results in under 2 seconds for a dataset of up to 100,000 users.
- **SC-005**: 100% of admin actions covered by FR-016 produce a corresponding audit log entry with no omissions, including under concurrent admin sessions.
- **SC-006**: Analytics charts load and render within 3 seconds when querying up to 12 months of historical data.
- **SC-007**: Banned users are denied access on the very next login attempt after the ban is applied — no grace window or caching delay.
- **SC-008**: Deactivated discount codes become invalid for new redemptions within the same request cycle as deactivation; no redemption can slip through after the deactivation is confirmed.

## Assumptions

- Users are never hard-deleted; a soft-delete (IsDeleted flag) approach is used across the platform to preserve transaction and progress history.
- MRR on the dashboard is estimated as premium user count multiplied by the standard plan price; it is not a live query to the payment processor. Exact financial reporting is a future enhancement.
- Course images are referenced by URL (externally hosted or previously uploaded via a separate flow); a built-in file upload mechanism is out of scope for this feature.
- "Admin" is a single privileged role; no sub-roles (e.g., content-admin vs. super-admin) are required in this iteration.
- The audit log is append-only; admins can read entries but cannot modify or delete them.
- Real-time push updates are out of scope; dashboard data refreshes on page load or explicit manual refresh.
- Rich-text lesson content sanitization against injection attacks is handled consistently at the platform level and is not specific to this feature.
