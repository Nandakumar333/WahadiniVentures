# Feature Specification: Admin Dashboard

**Feature Branch**: `009-admin-dashboard`  
**Created**: December 16, 2025  
**Status**: Draft  
**Input**: A centralized, secure admin interface for managing the entire platform. This includes user management, course content creation, task reviewing, reward management, and deep analytics visualization. The dashboard serves as the control center for platform operations.

## Clarifications

### Session 2025-12-16

- Q: Does the system need a super-admin role distinct from regular admin, or should ALL admin-to-admin actions be blocked? → A: Implement super-admin role with elevated privileges to manage other admins and resolve lockouts
- Q: How should notifications be delivered to users when their tasks are reviewed? → A: Email notifications with in-app banner
- Q: How long should historical analytics data be retained and available for viewing? → A: 12 months rolling retention

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Platform Health Overview (Priority: P1)

As an Admin, I want to see a high-level dashboard with critical platform metrics upon login so I can quickly assess platform health and identify areas requiring immediate attention.

**Why this priority**: This is the foundation of the admin experience. Without a clear overview, admins cannot effectively prioritize their work or identify urgent issues. This is the first thing admins see and sets the context for all other administrative actions.

**Independent Test**: Can be fully tested by logging in with an admin account and verifying that key metrics (total users, active subscribers, monthly revenue, pending task queue) are displayed accurately and update when underlying data changes. Delivers immediate value by providing operational visibility.

**Acceptance Scenarios**:

1. **Given** I am logged in as an Admin, **When** I navigate to `/admin`, **Then** I see summary cards displaying Total Users, Active Subscribers, Monthly Recurring Revenue (MRR), and Pending Tasks count
2. **Given** the dashboard is displaying metrics, **When** underlying data changes (e.g., new user signup), **Then** the metrics update to reflect current state within a reasonable timeframe (page refresh acceptable for MVP)
3. **Given** I am viewing the dashboard, **When** I see revenue trends and user growth charts, **Then** they visualize data for the last 30 days by default with clear labels and accurate data points
4. **Given** I have no pending administrative actions, **When** I view the dashboard, **Then** all queue counters show zero without errors

---

### User Story 2 - Task Review Workflow (Priority: P2)

As an Admin, I need to efficiently review and grade pending user task submissions so users can progress through courses and earn their rewards without unnecessary delays.

**Why this priority**: This directly impacts user experience and platform engagement. Users blocked on pending reviews cannot progress, leading to frustration and potential churn. This is a critical operational workflow that must be efficient.

**Independent Test**: Can be fully tested by creating test task submissions and reviewing them through the admin interface. Delivers value by unblocking users and maintaining platform engagement.

**Acceptance Scenarios**:

1. **Given** there are pending task submissions, **When** I navigate to the Task Review section, **Then** I see a list of submissions with user name, task name, submission date, and submission content (text/image)
2. **Given** I am viewing a task submission, **When** I approve it with feedback comments, **Then** the task status changes to Approved, points are awarded to the user, and the user receives a notification
3. **Given** I am viewing a task submission, **When** I reject it with specific feedback, **Then** the task status changes to Rejected, the user receives the feedback via notification, and can resubmit
4. **Given** I am reviewing submissions, **When** I filter by date range, course, or submission status, **Then** only matching submissions are displayed
5. **Given** there are no pending submissions, **When** I view the Task Review section, **Then** I see an empty state message indicating all tasks are reviewed

---

### User Story 3 - User Account Management (Priority: P3)

As an Admin, I want to manage user accounts (search, view details, update roles, ban/unban) so I can handle support issues, enforce community guidelines, and manage platform access.

**Why this priority**: While important for platform moderation and support, this functionality is not required for daily operations unless specific incidents occur. It's reactive rather than proactive.

**Independent Test**: Can be fully tested by creating test user accounts and performing all CRUD operations through the admin interface. Delivers value by enabling customer support and community moderation.

**Acceptance Scenarios**:

1. **Given** I am in the User Management section, **When** I search for a user by email, username, or ID, **Then** matching users are displayed in a paginated table with key details (username, email, role, status, signup date)
2. **Given** I am viewing a user's profile, **When** I click to see full details, **Then** I see their complete profile including enrollment history, completed courses, current points balance, purchase history, and activity log
3. **Given** I am viewing a user's profile, **When** I change their role from "User" to "Admin", **Then** the role is updated and the user gains admin privileges on next login
4. **Given** a user has violated platform guidelines, **When** I ban their account, **Then** they cannot log in and see a message explaining their account status
5. **Given** a previously banned user, **When** I unban their account, **Then** they can log in normally and access all features according to their subscription level
6. **Given** I am managing users, **When** I apply filters (by role, subscription status, or account status), **Then** only users matching the criteria are displayed

---

### User Story 4 - Course Content Management (Priority: P4)

As an Admin, I want to create, edit, and publish courses and lessons through a visual interface so I can manage educational content without directly manipulating the database.

**Why this priority**: Content management is essential but typically done in batches or scheduled sessions rather than daily. It's a strategic activity that doesn't require immediate availability for platform operations.

**Independent Test**: Can be fully tested by creating a complete course with multiple lessons, editing existing content, and publishing/unpublishing courses. Delivers value by enabling content creators to work independently.

**Acceptance Scenarios**:

1. **Given** I am in the Course Management section, **When** I click "Create New Course", **Then** I see a form to enter course title, description, category, thumbnail URL, and difficulty level
2. **Given** I am creating/editing a course, **When** I use a rich text editor for the description, **Then** I can format text, add links, and create structured content
3. **Given** I have created a course, **When** I add lessons to it, **Then** I can specify lesson title, description, video URL (YouTube), duration, and point reward for completion
4. **Given** I have multiple lessons in a course, **When** I drag and drop to reorder them, **Then** the lesson sequence updates and is reflected in the user-facing course view
5. **Given** I have a draft course, **When** I toggle it to "Published", **Then** it becomes visible to users and appears in the course catalog
6. **Given** I have a published course, **When** I unpublish it, **Then** it is hidden from new enrollments but existing enrolled users can still access it
7. **Given** I am editing course content, **When** I save changes, **Then** the updates are reflected immediately in the user-facing interface

---

### User Story 5 - Reward System Management (Priority: P5)

As an Admin, I want to manage the platform reward economy (create discount codes, view redemption logs, manually adjust user points) so I can run marketing campaigns and resolve customer support issues.

**Why this priority**: This is a support and marketing function that is not required for core platform operations. It's used occasionally for campaigns or resolving specific user issues.

**Independent Test**: Can be fully tested by creating discount codes, simulating redemptions, and manually adjusting test user point balances. Delivers value for marketing initiatives and customer support resolution.

**Acceptance Scenarios**:

1. **Given** I am in the Rewards Management section, **When** I create a new discount code, **Then** I can specify code value, discount percentage or fixed amount, expiration date, and usage limits (single-use or multi-use)
2. **Given** I have created discount codes, **When** I view the discount code list, **Then** I see all codes with their status (active, expired, fully redeemed) and redemption count
3. **Given** I am viewing redemption logs, **When** I filter by date range or discount code, **Then** I see who redeemed codes and when
4. **Given** a user has a point discrepancy, **When** I manually adjust their point balance with a reason note, **Then** the change is applied immediately and logged in the audit trail
5. **Given** I am creating a discount code, **When** I set it to expire in the past or set usage limit to 0, **Then** I receive a validation error preventing the invalid configuration

---

### User Story 6 - Analytics and Insights (Priority: P6)

As an Admin, I want to analyze platform performance through visual charts and reports (revenue trends, user retention, course popularity, engagement metrics) so I can make data-driven decisions about content and marketing strategy.

**Why this priority**: Analytics are valuable for strategic planning but not required for day-to-day operations. This is used periodically for business reviews and planning sessions.

**Independent Test**: Can be fully tested by viewing various analytics dashboards with sample data and verifying chart accuracy. Delivers value for strategic decision-making and performance optimization.

**Acceptance Scenarios**:

1. **Given** I am viewing revenue analytics, **When** I select a date range, **Then** I see a line chart showing daily revenue and a total for the period
2. **Given** I am viewing user analytics, **When** I view the user growth chart, **Then** I see new signups per day/week/month and cumulative user count
3. **Given** I am viewing course analytics, **When** I see the course popularity report, **Then** courses are ranked by enrollment count, completion rate, and average rating
4. **Given** I am viewing engagement metrics, **When** I see the task completion funnel, **Then** I understand drop-off rates at each stage (enrolled → started → 50% complete → completed)
5. **Given** I am analyzing retention, **When** I view the cohort retention chart, **Then** I see what percentage of users from each signup cohort remain active over time

---

### User Story 7 - Audit Log and Accountability (Priority: P7)

As an Admin, I want to see a comprehensive log of all administrative actions taken on the platform so I can maintain accountability, investigate issues, and comply with security best practices.

**Why this priority**: While important for security and compliance, audit logging is a background function that is only accessed when investigating specific incidents or during audits.

**Independent Test**: Can be fully tested by performing various admin actions and verifying they appear in the audit log with correct timestamps, actor, and action details. Delivers value for security compliance and incident investigation.

**Acceptance Scenarios**:

1. **Given** any admin performs an action, **When** the action completes, **Then** it is logged with timestamp, admin user, action type, and affected resource
2. **Given** I am viewing the audit log, **When** I filter by admin user, action type, or date range, **Then** only matching log entries are displayed
3. **Given** I am investigating an issue, **When** I view a specific log entry, **Then** I see full details including before/after values for data changes
4. **Given** I am viewing audit logs, **When** I export the filtered results, **Then** I receive a downloadable CSV file with all log entries

---

### Edge Cases

- What happens when an admin reviews a task that was already reviewed by another admin (concurrent access)? System should prevent duplicate reviews or show a warning.
- How does the system handle course deletion when users are currently enrolled? Soft delete should be enforced, hiding from catalog but preserving enrolled user access.
- What happens when a regular admin tries to ban another admin account? System prevents the action and requires super-admin role.
- How does the dashboard handle missing or corrupted data (e.g., invalid YouTube URLs in lessons)? Display graceful error messages without breaking the interface.
- What happens when creating a discount code with a code that already exists? Validation error should prevent creation.
- How does the system handle very large datasets (e.g., 100,000+ users) in list views? Pagination and search must remain performant.
- What happens if an admin manually sets a user's points to a negative value? Validation should prevent negative point balances.
- How are timezone differences handled in analytics charts and audit logs? All timestamps should display in a consistent timezone with clear indication.
- What happens when an admin unpublishes a course that is part of an active subscription package? Course should remain accessible to existing subscribers.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST restrict all admin dashboard routes to users with the "Admin" role
- **FR-002**: System MUST display a dashboard homepage with summary cards showing Total Users count, Active Subscribers count, Monthly Recurring Revenue (MRR), and Pending Tasks count (updated on page load or manual refresh; real-time WebSocket updates not required for MVP)
- **FR-003**: System MUST visualize revenue trends over the last 30 days using a line chart (refreshed on page load)
- **FR-004**: System MUST visualize new user signups over the last 30 days using a line chart (refreshed on page load)
- **FR-005**: System MUST provide a paginated, searchable list of all platform users with filters for role, subscription status, and account status
- **FR-006**: System MUST display complete user profile details including enrollment history, completed courses, current points balance, purchase history, and recent activity
- **FR-007**: System MUST allow admins to update user roles between "User" and "Admin"
- **FR-008**: System MUST allow admins to ban user accounts, preventing login and displaying an appropriate message to the user
- **FR-009**: System MUST allow admins to unban previously banned accounts, restoring full access
- **FR-010**: System MUST provide a list view of pending task submissions with user name, task name, submission date, and preview of submission content
- **FR-011**: System MUST allow admins to approve task submissions, automatically awarding points and sending email notification to the user with in-app notification banner (via GET /api/notifications/unread polled on app load and every 60 seconds)
- **FR-012**: System MUST allow admins to reject task submissions with written feedback, sending email notification to the user with the feedback and in-app notification banner (via GET /api/notifications/unread polled on app load and every 60 seconds)
- **FR-013**: System MUST support filtering task submissions by date range, course, and submission status
- **FR-014**: System MUST provide a course creation form accepting title, description, category, thumbnail URL, and difficulty level
- **FR-015**: System MUST include a rich text editor for course and lesson descriptions supporting formatting, links, and structured content
- **FR-016**: System MUST allow admins to add lessons to courses, specifying title, description, YouTube video URL, duration, and point reward
- **FR-017**: System MUST support drag-and-drop reordering of lessons within a course
- **FR-018**: System MUST allow admins to toggle course publish status, controlling visibility in the user-facing catalog
- **FR-019**: System MUST enforce soft delete for courses (never hard delete) to preserve transaction and enrollment history
- **FR-020**: System MUST allow admins to create discount codes with configurable value, discount type (percentage or fixed amount), expiration date, and usage limits
- **FR-021**: System MUST display a list of all discount codes with status indicators (active, expired, fully redeemed) and redemption count
- **FR-022**: System MUST provide a redemption log showing which users redeemed which codes and when
- **FR-023**: System MUST allow admins to manually adjust user point balances with a required reason note
- **FR-024**: System MUST validate discount code creation preventing duplicate codes, past expiration dates, and invalid usage limits
- **FR-025**: System MUST display analytics charts for revenue trends, user growth, course popularity, and engagement metrics
- **FR-026**: System MUST support date range selection for all analytics views
- **FR-027**: System MUST retain historical analytics data for at least 12 months on a rolling basis (365-day sliding window from current date) for trend analysis and year-over-year comparisons
- **FR-028**: System MUST rank courses by enrollment count, completion rate, and average rating
- **FR-029**: System MUST log all administrative actions with timestamp, admin user, action type, and affected resource
- **FR-030**: System MUST provide an audit log view with filtering by admin user, action type, and date range
- **FR-031**: System MUST support exporting filtered audit logs to CSV format
- **FR-032**: System MUST prevent concurrent task review conflicts by showing a warning or locking mechanism
- **FR-033**: System MUST implement a super-admin role with elevated privileges to manage other admin accounts, preventing admin lockout scenarios
- **FR-034**: System MUST restrict admin-to-admin account modifications (role changes, banning) to users with super-admin role only
- **FR-035**: System MUST display all timestamps in a consistent timezone with clear indication
- **FR-036**: System MUST prevent setting user point balances to negative values
- **FR-037**: System MUST support pagination for all large dataset views (users, tasks, audit logs) with configurable page size (default: 20 items, maximum: 100 items per page)
- **FR-038**: System MUST display graceful error messages for corrupted or invalid data without breaking the interface

### Key Entities

- **AdminDashboardStats**: Aggregated metrics representing platform health (Total Users, Active Subscribers, MRR, Pending Tasks count, Revenue Trend data points, User Growth data points)
- **UserSummary**: Condensed user information for list views (ID, Username, Email, Role, Subscription Status, Account Status, Signup Date)
- **UserDetail**: Complete user information including enrollment history, completed courses, points balance, purchase records, and activity timeline
- **TaskSubmissionReview**: Task submission awaiting admin review (Submission ID, User, Task, Course, Submission Date, Content Type, Content Data, Current Status)
- **CourseEditor**: Editable course structure (Course ID, Title, Description, Category, Thumbnail URL, Difficulty Level, Publish Status, Lessons collection)
- **LessonEditor**: Editable lesson structure (Lesson ID, Title, Description, Video URL, Duration, Point Reward, Order/Sequence)
- **DiscountCode**: Promotional code configuration (Code, Discount Type, Discount Value, Expiration Date, Usage Limit, Usage Count, Status)
- **RedemptionLog**: Record of discount code usage (Redemption ID, User, Code, Redemption Date)
- **PointAdjustment**: Manual point balance modification (User, Previous Balance, New Balance, Adjustment Amount, Reason, Admin User, Timestamp)
- **AnalyticsMetric**: Time-series data point for charts (Date, Metric Name, Metric Value)
- **AuditLogEntry**: Record of administrative action (Timestamp, Admin User, Action Type, Resource Type, Resource ID, Before Value, After Value, IP Address)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Admins can access the dashboard and view all summary metrics within 3 seconds of login
- **SC-002**: Admins can review and approve/reject a task submission in under 30 seconds on average
- **SC-003**: Admins can create a complete course with 5 lessons in under 15 minutes
- **SC-004**: Admins can locate a specific user account within 10 seconds using search
- **SC-005**: Task review queue response time (from submission to admin review decision) is reduced by 50% compared to manual database operations
- **SC-006**: 95% of admin actions (create, update, delete) complete successfully without errors
- **SC-007**: Analytics charts render with accurate data and load within 2 seconds for 30-day date ranges
- **SC-008**: Zero security incidents of unauthorized access to admin features in first 3 months of deployment
- **SC-009**: All user list and task list views support pagination and remain performant with 100,000+ records
- **SC-010**: Admins successfully complete primary workflows (user management, task review, course creation) on first attempt 90% of the time
- **SC-011**: Audit log captures 100% of administrative actions with no data loss
- **SC-012**: System prevents invalid operations (negative points, duplicate discount codes, banning admins) with clear validation messages 100% of the time

## Assumptions

- The admin dashboard will be part of the main SPA (lazily loaded) rather than a separate application to share components and reduce maintenance overhead
- Image uploads for course thumbnails will be handled via external URLs for MVP; dedicated upload functionality can be added in a future iteration
- Real-time updates via WebSockets are not required for MVP; pull-to-refresh or page reload is acceptable for dashboard metrics
- All timestamps will be stored in UTC and displayed in the admin's local timezone with timezone indicator
- Soft delete pattern is already implemented in the database schema for users and courses
- The existing role-based authorization system supports the "Admin" role and can be extended to support a "SuperAdmin" role
- A super-admin role will be implemented to manage other admin accounts and prevent lockout scenarios; at least one super-admin account must exist
- Analytics queries will be optimized with appropriate database indexes and may include aggregated/materialized views for performance
- Historical analytics data will be retained for 12 months on a rolling basis; older data may be archived or aggregated for long-term storage
- The rich text editor will be a third-party component (e.g., Quill, TinyMCE, or similar) rather than custom-built
- CSV export functionality will be server-generated rather than client-side to handle large datasets
- Concurrent task review conflicts are rare enough that a simple warning message is sufficient; complex locking is not required for MVP
- The platform operates in a single timezone for business operations; multi-timezone support for international teams is out of scope

## Dependencies

- Existing user authentication and role-based authorization system (ASP.NET Identity with "Admin" role)
- Existing database schema with users, courses, lessons, tasks, submissions, discount codes, and transaction tables
- Existing soft delete implementation for entity management
- Chart visualization library (Recharts, Chart.js, or similar) for analytics dashboards
- Rich text editor component for course and lesson content editing
- Pagination utilities for handling large datasets efficiently
- Existing email service (MailKit) for sending task review notifications to users
- In-app notification system for displaying notification banners when users log in

## Out of Scope

- Mobile-specific admin interface (responsive design for tablet/laptop is in scope)
- Real-time collaborative editing of courses by multiple admins simultaneously
- Advanced analytics features like predictive modeling, cohort analysis beyond basic retention, or A/B testing
- Custom report builder allowing admins to create ad-hoc queries
- Automated task grading or AI-assisted review suggestions
- Bulk operations (e.g., bulk user role updates, bulk task approvals)
- Version history and rollback for course content changes
- Granular permission system beyond simple Admin role (e.g., Content Editor, User Manager, Support Admin roles)
- Integration with external analytics platforms (Google Analytics, Mixpanel, etc.)
- Scheduled/automated discount code campaigns
- Email template editor for customizing notification messages
- User impersonation/login-as feature for support purposes
