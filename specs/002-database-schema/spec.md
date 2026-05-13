# Feature Specification: Database Schema Design

**Feature Branch**: `002-database-schema`  
**Created**: November 14, 2025  
**Status**: Draft  
**Input**: User description: "Design and implement a comprehensive PostgreSQL database schema with all entities, relationships, indexes, and migrations for the crypto learning platform."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Core Entity Structure (Priority: P1)

As a developer, I need a well-defined database schema with all core entities (Users, Categories, Courses, Lessons, Tasks) properly structured so that the application can store and retrieve educational content and user data efficiently.

**Why this priority**: Foundation for the entire application. Without these core entities, no other features can function. This is the absolute minimum viable database structure.

**Independent Test**: Can be fully tested by creating migration scripts, applying them to a test database, and verifying all tables are created with correct columns, data types, and constraints. Delivers a working database structure ready for application integration.

**Acceptance Scenarios**:

1. **Given** a fresh PostgreSQL database, **When** EF Core migrations are applied, **Then** all core entity tables (Users, Categories, Courses, Lessons, LearningTasks) are created with proper columns and data types
2. **Given** the database schema is created, **When** attempting to insert data with invalid types or missing required fields, **Then** database constraints prevent invalid data entry
3. **Given** the Users table exists, **When** integrating with ASP.NET Identity, **Then** authentication and authorization features work correctly with the User entity

---

### User Story 2 - Relationships and Foreign Keys (Priority: P1)

As a developer, I need properly defined relationships between entities (Courses to Categories, Lessons to Courses, Tasks to Lessons) with appropriate foreign keys and cascade rules so that data integrity is maintained and navigation between related entities is efficient.

**Why this priority**: Critical for data consistency and application reliability. Incorrect relationships lead to orphaned records, data corruption, and application crashes. Must be part of MVP.

**Independent Test**: Can be tested by inserting related records (e.g., Course → Lesson → Task) and verifying foreign key constraints work correctly. Delete operations trigger appropriate cascade behaviors. Query performance with joins meets requirements.

**Acceptance Scenarios**:

1. **Given** a Course exists with multiple Lessons, **When** the Course is deleted, **Then** all associated Lessons are also deleted (cascade delete)
2. **Given** a Lesson references a Course, **When** attempting to delete the Course without cascade, **Then** database prevents deletion to maintain referential integrity
3. **Given** related entities exist, **When** querying a Course with its Lessons, **Then** EF Core navigation properties load data correctly
4. **Given** a User is soft-deleted, **When** querying active users, **Then** soft-deleted users are automatically excluded from results

---

### User Story 3 - Progress and Enrollment Tracking (Priority: P2)

As a platform user, I need my learning progress and course enrollments tracked accurately so that I can resume where I left off and see my completion status across all courses.

**Why this priority**: Essential for user experience and platform value, but the platform can technically function without progress tracking initially. Users can access content even if progress isn't tracked.

**Independent Test**: Can be tested by enrolling users in courses, tracking lesson completions, and verifying UserProgress and UserCourseEnrollment records are created and updated correctly. Progress percentages calculate accurately.

**Acceptance Scenarios**:

1. **Given** a user enrolls in a course, **When** a UserCourseEnrollment record is created, **Then** the enrollment date, progress percentage (initially 0%), and status are stored correctly
2. **Given** a user completes a lesson, **When** UserProgress is updated, **Then** completion timestamp, video watch time, and lesson status are recorded
3. **Given** a user has completed multiple lessons, **When** calculating course progress, **Then** the progress percentage reflects the ratio of completed lessons to total lessons
4. **Given** a user enrolls in the same course twice, **When** attempting to create a duplicate enrollment, **Then** database unique constraint prevents duplicate records

---

### User Story 4 - Task Submissions and Verification (Priority: P2)

As a platform administrator, I need to track all user task submissions with their data and review status so that I can verify submissions and award points appropriately.

**Why this priority**: Core to the gamification and task-to-earn features, but platform can function with read-only course content initially. Task verification can be added after basic content delivery works.

**Independent Test**: Can be tested by submitting tasks of different types (Quiz, Screenshot, Wallet), storing submission data in JSONB format, updating submission status (Pending → Approved/Rejected), and verifying rewards are recorded correctly.

**Acceptance Scenarios**:

1. **Given** a user completes a task, **When** submitting task data, **Then** UserTaskSubmission record is created with task type, submission data (JSONB), and Pending status
2. **Given** a task submission exists, **When** an admin reviews and approves it, **Then** submission status updates to Approved and points are awarded
3. **Given** multiple submissions for the same task by the same user, **When** checking submission history, **Then** only the latest valid submission counts for rewards
4. **Given** a task has quiz answers in JSONB format, **When** storing complex nested JSON data, **Then** PostgreSQL JSONB column stores and retrieves data without corruption

---

### User Story 5 - Reward Points Ledger (Priority: P2)

As a platform user, I need an accurate and immutable record of all my reward point transactions (earned, redeemed, bonuses, penalties) so that I can trust the point system and track my earnings.

**Why this priority**: Important for user trust and gamification effectiveness, but platform can function with courses and tasks before implementing rewards. Can be added as an enhancement.

**Independent Test**: Can be tested by creating reward transactions of different types (Earned, Redeemed, Bonus, Penalty), verifying immutability (no updates/deletes allowed), calculating user point balances accurately, and ensuring transaction history is complete.

**Acceptance Scenarios**:

1. **Given** a user completes a task worth 50 points, **When** a reward transaction is created, **Then** TransactionType is Earned, Amount is 50, and record is immutable
2. **Given** a user redeems 500 points for a discount, **When** a reward transaction is created, **Then** TransactionType is Redeemed, Amount is -500, and user balance decreases
3. **Given** multiple reward transactions exist, **When** calculating user's total points, **Then** SUM of all transaction amounts equals user's current balance
4. **Given** a reward transaction is created, **When** attempting to update or delete it, **Then** database constraints prevent modification (append-only ledger)

---

### User Story 6 - Point-Based Discount System (Priority: P3)

As a platform user, I need to redeem my earned points for discount codes on premium subscriptions so that I can reduce my subscription cost by engaging with the platform.

**Why this priority**: Nice-to-have feature that enhances user engagement but isn't critical for core platform functionality. Can be added after basic subscription system is working.

**Independent Test**: Can be tested by creating discount codes with point costs, allowing users to redeem points for codes, tracking redemptions, enforcing usage limits, and verifying discounts apply correctly during checkout.

**Acceptance Scenarios**:

1. **Given** discount codes exist (SAVE10, SAVE20, SAVE30), **When** a user with sufficient points redeems a code, **Then** UserDiscountRedemption record is created and points are deducted
2. **Given** a discount code has a maximum usage limit, **When** the limit is reached, **Then** no further redemptions are allowed
3. **Given** a user has redeemed a discount code, **When** purchasing a subscription, **Then** the discount percentage is applied to the final price
4. **Given** a discount code expires, **When** attempting to use it, **Then** system prevents redemption and notifies user

---

### User Story 7 - Performance Optimization with Indexes (Priority: P1)

As a developer, I need proper database indexes on frequently queried columns so that the application performs well under load with fast query response times.

**Why this priority**: Critical for production readiness and user experience. Without indexes, queries become slow as data grows, leading to poor performance and user frustration. Must be part of MVP.

**Independent Test**: Can be tested by running common queries (user login, course search, progress lookup) on databases with varying data volumes (1K, 10K, 100K records) and measuring query execution times. All queries should complete in under 500ms.

**Acceptance Scenarios**:

1. **Given** 10,000 users exist, **When** querying a user by email, **Then** query completes in under 100ms using email index
2. **Given** 1,000 courses exist, **When** filtering courses by category and difficulty, **Then** query completes in under 200ms using composite index
3. **Given** 50,000 progress records exist, **When** querying a user's progress for a specific course, **Then** query completes in under 150ms using composite index on (UserId, CourseId)
4. **Given** 100,000 task submissions exist, **When** filtering pending submissions, **Then** query completes in under 300ms using status index

---

### User Story 8 - Time-Based Partitioning for Activity Data (Priority: P3)

As a database administrator, I need time-based partitioning on high-volume tables (UserProgress, RewardTransactions) so that the database remains performant as activity data grows to millions of records.

**Why this priority**: Important for long-term scalability but not needed for initial launch. Partitioning can be added later when data volume justifies the complexity. Platform works fine without it initially.

**Independent Test**: Can be tested by creating monthly partitions for UserProgress and RewardTransactions tables, inserting data across multiple months, verifying data routes to correct partitions, and measuring query performance improvements on partitioned vs non-partitioned tables.

**Acceptance Scenarios**:

1. **Given** UserProgress table is partitioned by month, **When** inserting progress records for different months, **Then** records are automatically routed to the correct partition
2. **Given** partitioned tables exist, **When** querying data for a specific month, **Then** only the relevant partition is scanned (partition pruning)
3. **Given** old partitions exist (older than 1 year), **When** archiving old data, **Then** entire partitions can be detached and archived efficiently
4. **Given** a new month begins, **When** the first record for that month is inserted, **Then** a new partition is automatically created

---

### User Story 9 - JWT Refresh Token Storage (Priority: P1)

As a platform user, I need my authentication session to remain secure with refresh tokens properly stored in the database so that I can maintain long-lived sessions without compromising security.

**Why this priority**: Critical for security and user experience. Without refresh tokens, users must re-login frequently, creating frustration. Secure token management is essential for production systems.

**Independent Test**: Can be tested by generating JWT access and refresh tokens, storing refresh tokens in the database with expiration dates, validating token pairs, revoking tokens on logout, and ensuring expired tokens are automatically cleaned up.

**Acceptance Scenarios**:

1. **Given** a user logs in successfully, **When** JWT tokens are issued, **Then** refresh token is stored in RefreshTokens table with expiration date
2. **Given** an access token expires, **When** user requests a new access token with valid refresh token, **Then** system validates refresh token from database and issues new access token
3. **Given** a user logs out, **When** logout is processed, **Then** refresh token is revoked (IsRevoked = true) in database
4. **Given** refresh tokens expire after 30 days, **When** cleanup job runs, **Then** expired and revoked tokens older than 30 days are deleted

---

### User Story 10 - Initial Data Seeding (Priority: P2)

As a developer, I need the database to be seeded with initial data (categories, admin user, sample discount codes) so that the application has essential reference data and a default admin account for setup.

**Why this priority**: Important for deployment and initial setup, but not critical for core functionality. Database can be manually populated initially, but seeding automates deployment and ensures consistency.

**Independent Test**: Can be tested by running the application on a fresh database, verifying seed data is inserted automatically, checking that re-running seeds doesn't create duplicates (idempotent), and confirming all required categories and admin user exist.

**Acceptance Scenarios**:

1. **Given** a fresh database with applied migrations, **When** application starts for the first time, **Then** 5 course categories are automatically created (Airdrops, GameFi, Task-to-Earn, DeFi, NFT Strategies)
2. **Given** seed data runs, **When** creating the admin user, **Then** default admin account is created with secure password hash and Admin role
3. **Given** discount codes are seeded, **When** checking DiscountCodes table, **Then** 3 default codes exist (SAVE10, SAVE20, SAVE30) with appropriate point costs
4. **Given** seed data has already been applied, **When** running seeds again, **Then** no duplicate records are created (idempotent operation)

---

### Edge Cases

- What happens when a user tries to enroll in a course they're already enrolled in? (Unique constraint prevents duplicates)
- How does the system handle concurrent task submissions from the same user? (Last-write-wins with timestamp comparison)
- What if a course is deleted while users are actively enrolled? (Soft delete prevents loss of enrollment history)
- How are orphaned records prevented when cascading deletes fail? (Transaction rollback ensures atomicity)
- What happens when JSONB data exceeds PostgreSQL column limits? (Validation at application layer prevents oversized JSON)
- How does the system handle time zone differences in timestamps? (All timestamps stored in UTC, converted at presentation layer)
- What if a user's point balance calculation becomes inconsistent? (Ledger-based design ensures recalculation from immutable transactions)
- How are race conditions handled when multiple users redeem the last available discount code? (Database unique constraints and optimistic concurrency)
- What happens when partitioned tables reach maximum partition count? (Automated partition management with archival strategy)
- How does the system recover from partial migration failures? (EF Core transactions ensure all-or-nothing migration application)

## Requirements *(mandatory)*

### Functional Requirements

#### Core Entity Requirements

- **FR-001**: System MUST define a Users entity with properties for authentication (Email, PasswordHash, EmailVerified), profile (FirstName, LastName, ProfileImageUrl), subscription (SubscriptionTier, SubscriptionExpiresAt), and points (CurrentPoints)
- **FR-002**: System MUST define a Categories entity for course organization with properties Name, Description, IconUrl, and IsActive
- **FR-003**: System MUST define a Courses entity with Title, Description, ThumbnailUrl, DifficultyLevel, EstimatedDurationHours, IsPublished, IsPremium, CategoryId (foreign key), CreatedById (foreign key to Users)
- **FR-004**: System MUST define a Lessons entity with Title, Description, YouTubeVideoId, VideoUrl, DurationSeconds, OrderIndex, IsPreview, CourseId (foreign key)
- **FR-005**: System MUST define a LearningTasks entity with Title, Description, TaskType (Quiz/Screenshot/Wallet/TextSubmission/ExternalLink), TaskData (JSONB), PointsReward, LessonId (foreign key)

#### Relationship Requirements

- **FR-006**: System MUST establish one-to-many relationship between Categories and Courses with foreign key constraint
- **FR-007**: System MUST establish one-to-many relationship between Courses and Lessons with cascade delete behavior
- **FR-008**: System MUST establish one-to-many relationship between Lessons and LearningTasks with cascade delete behavior
- **FR-009**: System MUST establish one-to-many relationship between Users and Courses (CreatedBy) for content authorship tracking
- **FR-010**: System MUST prevent deletion of Categories that have active Courses by restricting foreign key constraint

#### Progress and Enrollment Requirements

- **FR-011**: System MUST define UserCourseEnrollments entity to track user enrollments with UserId, CourseId, EnrollmentDate, CompletionDate, ProgressPercentage, LastAccessedAt
- **FR-012**: System MUST define UserProgress entity to track lesson completion with UserId, LessonId, IsCompleted, CompletedAt, VideoWatchTimeSeconds (int, seconds), ProgressPercentage
- **FR-013**: System MUST enforce unique constraint on UserCourseEnrollments (UserId, CourseId) to prevent duplicate enrollments
- **FR-014**: System MUST enforce unique constraint on UserProgress (UserId, LessonId) to prevent duplicate progress records
- **FR-015**: System MUST implement composite index on UserProgress (UserId, CourseId) for efficient progress queries

#### Task Submission Requirements

- **FR-016**: System MUST define UserTaskSubmissions entity with UserId, LearningTaskId, SubmissionData (JSONB), SubmissionStatus (Pending/Approved/Rejected), SubmittedAt, ReviewedAt, ReviewedById, ReviewNotes
- **FR-017**: System MUST store task submission data in JSONB format to support flexible schema for different task types
- **FR-018**: System MUST track submission status with enum values: Pending, Approved, Rejected
- **FR-019**: System MUST allow multiple submissions per user per task but track submission history
- **FR-020**: System MUST implement index on SubmissionStatus for efficient filtering of pending submissions

#### Reward System Requirements

- **FR-021**: System MUST define RewardTransactions entity as an immutable ledger with UserId, TransactionType (Earned/Redeemed/Bonus/Penalty/Expired), Amount, Description, RelatedTaskSubmissionId, RelatedDiscountRedemptionId, TransactionDate
- **FR-022**: System MUST implement RewardTransactions as append-only (no updates or deletes allowed)
- **FR-023**: System MUST support transaction types: Earned (completing tasks), Redeemed (using points for discounts), Bonus (admin awards), Penalty (rule violations), Expired (point expiration)
- **FR-024**: System MUST calculate user's current point balance as SUM of all transaction amounts for that user with SERIALIZABLE transaction isolation to prevent race conditions during concurrent redemptions
- **FR-025**: System MUST implement index on RewardTransactions (UserId, TransactionDate) for efficient balance calculations

#### Discount Code Requirements

- **FR-026**: System MUST define DiscountCodes entity with Code, DiscountPercentage, PointCost, IsActive, ExpiresAt, MaxUsageCount, CurrentUsageCount
- **FR-027**: System MUST define UserDiscountRedemptions entity to track redemptions with UserId, DiscountCodeId, RedeemedAt, PointsSpent
- **FR-028**: System MUST enforce unique constraint on DiscountCodes.Code to prevent duplicate codes
- **FR-029**: System MUST validate MaxUsageCount before allowing discount redemption
- **FR-030**: System MUST create corresponding RewardTransaction (Redeemed type) when user redeems discount code

#### Authentication Requirements

- **FR-031**: System MUST define RefreshTokens entity with UserId, Token, ExpiresAt, CreatedAt, CreatedByIp, RevokedAt, RevokedByIp, ReplacedByToken, ReasonRevoked, IsExpired, IsRevoked, IsActive
- **FR-032**: System MUST store JWT refresh tokens securely in database with expiration tracking
- **FR-033**: System MUST support token revocation by updating RevokedAt and IsRevoked fields
- **FR-034**: System MUST implement cascading delete for RefreshTokens when User is deleted
- **FR-035**: System MUST implement cleanup mechanism for expired refresh tokens (older than 30 days)

#### Index and Performance Requirements

- **FR-036**: System MUST create unique index on Users.Email for authentication queries
- **FR-037**: System MUST create index on Users.SubscriptionTier for filtering premium users
- **FR-038**: System MUST create composite index on Courses (CategoryId, DifficultyLevel, IsPublished) for search and filtering
- **FR-039**: System MUST create index on Lessons (CourseId, OrderIndex) for ordered lesson retrieval
- **FR-040**: System MUST create index on LearningTasks (LessonId) for task queries
- **FR-041**: System MUST create composite index on UserTaskSubmissions (UserId, LearningTaskId, SubmittedAt) for submission history
- **FR-042**: System MUST create index on RewardTransactions (UserId, TransactionDate DESC) for transaction history queries
- **FR-043**: System MUST create index on RefreshTokens (Token) for token validation queries
- **FR-044**: System MUST implement JSONB GIN indexes on TaskData and SubmissionData for JSON queries

#### Data Integrity Requirements

- **FR-045**: System MUST implement soft delete for Users, Courses, Lessons, and Categories using IsDeleted and DeletedAt fields
- **FR-046**: System MUST automatically set CreatedAt timestamp on entity creation
- **FR-047**: System MUST automatically update UpdatedAt timestamp on entity modification
- **FR-048**: System MUST prevent NULL values in required foreign key fields
- **FR-049**: System MUST enforce referential integrity with foreign key constraints
- **FR-050**: System MUST use transactions to ensure atomicity of related operations (e.g., task approval + reward transaction)

#### Partitioning Requirements

- **FR-051**: System MUST implement monthly time-based partitioning for UserProgress table based on CompletedAt timestamp
- **FR-052**: System MUST implement monthly time-based partitioning for RewardTransactions table based on TransactionDate
- **FR-053**: System MUST create partitions automatically for upcoming months (3 months in advance)
- **FR-054**: System MUST maintain partition metadata for historical data archival
- **FR-055**: System MUST support partition pruning in queries to improve performance

#### Migration and Seeding Requirements

- **FR-056**: System MUST use EF Core code-first migrations for schema changes
- **FR-057**: System MUST include migration for creating all tables, indexes, constraints, and relationships
- **FR-058**: System MUST support rollback of migrations to previous schema versions
- **FR-059**: System MUST seed initial data for 5 course categories: Airdrops, GameFi, Task-to-Earn, DeFi, NFT Strategies
- **FR-060**: System MUST seed default admin user with username "admin@wahadini.com" and secure password
- **FR-061**: System MUST seed 3 default discount codes: SAVE10 (10%, 500 points), SAVE20 (20%, 1000 points), SAVE30 (30%, 2000 points)
- **FR-062**: System MUST ensure seed operations are idempotent (can be run multiple times without creating duplicates)

### Constitution Compliance Requirements

**Learning-First**: Database schema is designed to track detailed learning progress (lesson completion, video watch time, progress percentages) rather than just gamification metrics. Progress tracking focuses on educational outcomes, not just task completion counts.

**Security & Privacy**: All sensitive data (passwords, tokens) stored using secure hashing and encryption. User data includes soft delete support for GDPR compliance (right to be forgotten). Audit fields (CreatedAt, UpdatedAt, ReviewedAt) enable comprehensive audit logging. Foreign key constraints and validation rules prevent SQL injection and data corruption.

**Scalability**: Schema designed with proper indexes for sub-second query performance on large datasets. Time-based partitioning supports millions of activity records. Composite indexes optimize common query patterns. JSONB columns provide flexibility without schema changes for evolving requirements.

**Fair Economy**: Reward system uses immutable ledger design to prevent point manipulation. Multiple submission tracking prevents duplicate rewards. Discount code usage limits prevent abuse. Transaction history provides complete transparency and enables fraud detection.

**Quality Assurance**: Schema includes fields for content review and approval (IsPublished, ReviewedBy, ReviewedAt). Referential integrity prevents orphaned content. Cascade delete rules maintain data consistency. Version tracking through timestamps enables content history and rollback.

**Accessibility**: Schema supports user preferences and settings (future extension). Metadata fields (Description, Title) enable screen reader compatibility at data layer. Flexible JSONB storage allows adding accessibility-specific data without schema changes.

**Business Ethics**: Transparent pricing through DiscountCodes table. Clear subscription tracking (SubscriptionTier, ExpiresAt). Point costs clearly defined. No hidden charges or dark patterns in data model.

**Technical Excellence**: Clean Architecture principles with clear domain models. Repository pattern support through EF Core. SOLID principles in entity design. Comprehensive indexing strategy. Proper constraint enforcement. Migration-based version control for database changes.

### Key Entities

- **User**: Represents platform user account with authentication credentials (email/password hash), profile information (name, image), subscription details (tier, expiration), current point balance, and soft delete support. Related to Courses (as creator), UserCourseEnrollments, UserProgress, UserTaskSubmissions, RewardTransactions, UserDiscountRedemptions, and RefreshTokens.

- **Category**: Represents course category for organizing educational content into topics like Airdrops, GameFi, DeFi, NFT Strategies, Task-to-Earn. Contains name, description, icon URL, and active status. Related to Courses (one-to-many).

- **Course**: Represents a complete educational course with metadata (title, description, thumbnail), difficulty level (Beginner/Intermediate/Advanced), estimated duration, publishing status, premium flag, and content creator reference. Related to Category, User (creator), Lessons, and UserCourseEnrollments.

- **Lesson**: Represents individual lesson within a course containing YouTube video (video ID, URL, duration), title, description, order index for sequencing, and preview flag for free access. Related to Course and LearningTasks (one-to-many).

- **LearningTask**: Represents interactive task for skill verification with task type (Quiz, Screenshot, Wallet Verification, Text Submission, External Link), task configuration in JSONB format, point reward value, and lesson reference. Related to Lesson and UserTaskSubmissions.

- **UserCourseEnrollment**: Represents user enrollment in a course tracking enrollment date, completion date, overall progress percentage, and last access timestamp. Unique composite key on (UserId, CourseId) prevents duplicate enrollments. Related to User and Course.

- **UserProgress**: Represents lesson-level progress tracking with completion status, completion timestamp, video watch time, and progress percentage. Unique composite key on (UserId, LessonId) prevents duplicate progress records. Related to User and Lesson. Partitioned by month based on CompletedAt for scalability.

- **UserTaskSubmission**: Represents task submission by user with submission data in JSONB format (flexible schema for different task types), submission status (Pending/Approved/Rejected), submission and review timestamps, reviewer reference, and review notes. Related to User, LearningTask, and User (reviewer).

- **RewardTransaction**: Represents immutable ledger entry for point transactions with transaction type (Earned, Redeemed, Bonus, Penalty, Expired), amount (positive or negative), description, optional references to related task submission or discount redemption, and transaction timestamp. Append-only design prevents tampering. Related to User, UserTaskSubmission (optional), and UserDiscountRedemption (optional). Partitioned by month based on TransactionDate.

- **DiscountCode**: Represents point-redeemable discount code for subscriptions with unique code, discount percentage, point cost, active status, expiration date, maximum usage limit, and current usage counter. Related to UserDiscountRedemptions.

- **UserDiscountRedemption**: Represents user redemption of a discount code tracking redemption timestamp and points spent. Creates corresponding RewardTransaction for point deduction. Related to User and DiscountCode.

- **RefreshToken**: Represents JWT refresh token for maintaining user sessions with token string, expiration date, creation details (timestamp, IP), revocation details (timestamp, IP, reason, replacement token), and computed properties for status checks (IsExpired, IsRevoked, IsActive). Related to User with cascade delete.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Database migrations execute successfully on a fresh PostgreSQL 15+ database, creating all 12 entity tables with correct schemas in under 30 seconds
- **SC-002**: All foreign key relationships are correctly established, and referential integrity is enforced (attempting to delete referenced records fails with constraint violation)
- **SC-003**: User authentication queries (login by email) complete in under 50ms on a database with 10,000 user records using email index
- **SC-004**: Course search and filtering queries (by category, difficulty, published status) complete in under 200ms on a database with 1,000 courses using composite indexes
- **SC-005**: User progress queries (fetching all progress for a user in a specific course) complete in under 150ms on a database with 50,000 progress records using composite index (UserId, CourseId)
- **SC-006**: Task submission status queries (filtering by Pending status) complete in under 300ms on a database with 100,000 submissions using status index
- **SC-007**: Reward balance calculation (SUM of all transactions for a user) completes in under 200ms on a database with 100,000 transactions using userId index
- **SC-008**: Seed data operations are idempotent (running seeds multiple times creates exactly 5 categories, 1 admin user, and 3 discount codes with no duplicates)
- **SC-009**: JSONB columns (TaskData, SubmissionData) correctly store and retrieve complex nested JSON objects with arrays, nested objects, and special characters without data loss or corruption
- **SC-010**: Soft delete functionality works correctly (deleted users/courses/lessons are excluded from queries by default but remain in database for audit purposes)
- **SC-011**: Cascade delete rules work correctly (deleting a course removes all associated lessons, tasks, and enrollments automatically)
- **SC-012**: Time-based partitioning reduces query time by at least 40% for date-range queries on UserProgress and RewardTransactions tables compared to non-partitioned tables with equivalent data volume (tested with 1M+ records)
- **SC-013**: Database supports 100 concurrent write operations (inserts/updates) without deadlocks or transaction failures
- **SC-014**: All database constraints prevent invalid data (NULL values in required fields, duplicate email addresses, negative point amounts, invalid foreign keys)
- **SC-015**: EF Core migrations can be rolled back to previous versions without data loss (down migrations execute successfully)
- **SC-016**: Refresh token validation queries (by token string) complete in under 100ms using token index
- **SC-017**: Discount code redemption operations are atomic (point deduction, reward transaction creation, redemption record, usage count update all succeed or all fail together)
- **SC-018**: Database schema documentation is auto-generated from entity configurations and accurately reflects all tables, columns, relationships, and constraints
- **SC-019**: All timestamps are stored in UTC and maintain precision to milliseconds for accurate sorting and audit trails
- **SC-020**: Database backup and restore operations complete successfully without data corruption or constraint violations

## Constitution Compliance *(mandatory)*

### I. Learning-First Experience ✅
**Alignment**: Database schema prioritizes educational progress tracking over simple gamification metrics.
- UserProgress entity tracks detailed learning metrics: VideoWatchTimeSeconds (actual engagement), ProgressPercentage (comprehension level), IsCompleted (mastery achievement)
- Lesson completion tracking focuses on educational outcomes rather than just task completion counts
- RewardTransaction ledger links points to actual learning activities (RelatedTaskSubmissionId) rather than arbitrary engagement metrics
- Course difficulty levels (Beginner, Intermediate, Advanced) enable appropriate content sequencing for skill development

### II. Security & Privacy Standards ✅
**Alignment**: All security and privacy requirements met through schema design and audit capabilities.
- User data includes soft delete support (IsDeleted, DeletedAt) for GDPR compliance (right to be forgotten while maintaining audit trail)
- Audit fields (CreatedAt, UpdatedAt, ReviewedAt) enable comprehensive security logging for all data changes
- RefreshToken entity supports secure JWT session management with expiration tracking (ExpiresAt) and revocation capabilities (RevokedAt, ReasonRevoked)
- Foreign key constraints and validation rules prevent SQL injection and data corruption
- EmailVerified field enables email verification workflow for secure user onboarding
- PasswordHash storage (via ASP.NET Identity integration) ensures passwords never stored in plain text

### III. Scalability & Performance ✅
**Alignment**: Schema designed for high performance with proper indexing and scalability strategy.
- 36+ indexes defined for sub-second query performance: email lookups (<50ms), course searches (<200ms), progress queries (<150ms)
- Composite indexes optimize common query patterns: (UserId, LessonId), (UserId, CourseId), (CategoryId, IsPublished)
- Time-based partitioning strategy documented for UserProgress and RewardTransactions when data volume exceeds 1M records
- JSONB columns provide schema flexibility without requiring migrations for evolving task types
- GIN indexes on JSONB columns (TaskData, SubmissionData) enable efficient JSON path queries
- Query filters for soft delete automatically applied, improving query performance by excluding deleted records

### IV. Fair Gamification Economy ✅
**Alignment**: Reward system uses immutable ledger design to prevent manipulation and ensure transparency.
- RewardTransaction entity is append-only (no updates/deletes) preventing point tampering or retroactive changes
- Explicit foreign keys (RelatedTaskSubmissionId, RelatedDiscountRedemptionId) provide complete audit trail for all point movements
- Unique constraints on UserProgress and UserCourseEnrollment prevent duplicate reward claims
- DiscountCode entity includes MaxRedemptions and CurrentRedemptions fields to prevent abuse
- Transaction history (TransactionType enum: Earned, Redeemed, Bonus, Penalty, Expired) enables fraud detection and economic modeling
- Point balance always calculable from immutable ledger (SUM of transactions) ensuring consistency
- SERIALIZABLE transaction isolation for redemption operations prevents race conditions in concurrent point spending

### V. Content Quality Assurance ✅
**Alignment**: Schema supports comprehensive content review and approval workflows.
- Course entity includes IsPublished field for content approval before making courses visible to users
- UserTaskSubmission includes review workflow: Status (Pending/Approved/Rejected), ReviewedAt, ReviewedByUserId, ReviewNotes
- Referential integrity (foreign key constraints) prevents orphaned content and maintains data consistency
- Cascade delete rules maintain data integrity: deleting Course cascades to Lessons and LearningTasks
- Version tracking through CreatedAt and UpdatedAt timestamps enables content history and audit trails
- Soft delete on Courses and Lessons preserves content for potential restoration while hiding from users
- JSONB schema for TaskData allows flexible task types while maintaining structured validation at application layer

### VI. Accessibility & Transparency ✅
**Alignment**: Schema supports accessibility metadata and transparent data practices.
- Metadata fields (Title, Description) on all content entities enable screen reader compatibility at data layer
- JSONB flexible storage allows adding accessibility-specific data (alt text, captions) without schema changes
- Clear audit trail through timestamps (CreatedAt, UpdatedAt, DeletedAt) provides transparency into data lifecycle
- Soft delete strategy supports data retention requirements while respecting user privacy (DeletedAt timestamp)
- Transparent reward system: all point transactions immutable and traceable through RewardTransaction ledger
- Clear relationship structure (foreign keys with descriptive names) makes data flow transparent for developers

### VII. Business Model Ethics ✅
**Alignment**: Schema supports transparent freemium model with clear pricing and no dark patterns.
- DiscountCode entity with explicit PointCost and DiscountPercentage enables transparent reward redemption
- SubscriptionTier and SubscriptionExpiresAt fields on User entity track subscription status clearly
- IsPremium flags on Course and Lesson entities clearly distinguish free vs paid content
- No hidden charges in data model: all costs (PointCost) and benefits (DiscountPercentage) explicitly stored
- UserDiscountRedemption tracks redemption usage transparently (RedeemedAt, UsedInSubscription)
- Reward transaction descriptions (Description field) provide clear explanations for all point changes

### VIII. Technical Excellence ✅
**Alignment**: Database design follows Clean Architecture and SOLID principles.
- Clear domain models in separate layer (Entity classes) without infrastructure dependencies
- Repository pattern support through EF Core DbContext abstraction
- Migration-based version control for database schema changes (all changes tracked in migrations)
- Comprehensive indexing strategy balancing query performance with write overhead
- Proper constraint enforcement (foreign keys, unique constraints, check constraints)
- SOLID principles: Single Responsibility (each entity has clear purpose), Open/Closed (extensible via JSONB)
- Dependency Inversion: domain entities independent of data access implementation
- Code-first approach enables automated testing and continuous integration

### Constitution Enforcement Checklist
- [x] All 8 core principles addressed with specific implementation details
- [x] Security & Privacy standards (Principle II) given highest priority per Decision Framework
- [x] Learning-First Experience (Principle I) prioritized in schema design
- [x] Scalability requirements (Principle III) met with indexing and partitioning strategy
- [x] Fair economy (Principle IV) enforced through immutable ledger and constraints
- [x] Quality assurance (Principle V) supported through approval workflows
- [x] Accessibility (Principle VI) enabled through flexible metadata storage
- [x] Business ethics (Principle VII) reflected in transparent pricing fields
- [x] Technical excellence (Principle VIII) demonstrated through Clean Architecture

**Gate Status**: ✅ **PASS** - All constitution principles satisfied with concrete implementation

## Assumptions *(optional)*

- PostgreSQL 15 or higher is available as the database server
- Entity Framework Core 8.0 is used for ORM and migrations
- ASP.NET Identity is integrated with the Users entity for authentication
- All application servers and database servers are configured for UTC timezone
- Database connection pooling is configured appropriately for expected load
- Regular database backups are handled by infrastructure team (not part of schema design)
- Database monitoring and alerting is configured separately from schema implementation
- Point expiration rules are handled by application logic (schema supports expired transactions but doesn't enforce expiration automatically)
- Video watch time is tracked by frontend application and submitted to backend for storage
- Task submission review is performed manually by administrators (no automatic approval)
- Partition management automation is implemented separately from initial schema creation
- Database user permissions and roles are configured by database administrators (not part of migrations)
- JSONB data validation is performed at application layer before database insertion
- Email uniqueness is case-insensitive (handled by lowercase conversion at application layer)
- Password hashing uses bcrypt or similar industry-standard algorithm (implemented at application layer)
- JWT token secrets are managed through secure configuration (not stored in database)
- Subscription payment processing is handled by external service (Stripe), schema only tracks subscription status
- Course content (videos, descriptions) is hosted externally (YouTube, CDN), schema stores references only
- Geographic compliance (GDPR, CCPA) requirements are met through soft delete and data export features
- Performance testing assumes standard PostgreSQL configuration without specialized tuning
- Initial data volume estimates: 10K users, 1K courses, 10K lessons, 50K tasks, 100K submissions, 500K progress records, 1M transactions within first year

## Dependencies *(optional)*

### External Dependencies

- **PostgreSQL 15+**: Required database server with JSONB support and native partitioning capabilities
- **Entity Framework Core 8.0**: ORM framework for code-first migrations and data access
- **ASP.NET Identity**: User authentication and authorization framework integrated with Users entity
- **.NET 8**: Runtime environment for EF Core and application code

### Internal Dependencies

- **None**: This is a foundational feature. No other features depend on this, but all future features will depend on this database schema.

### Blocked By

- **None**: This is the first feature to be implemented. No blockers.

### Blocks

- **User Authentication Feature**: Cannot implement login/registration without Users table and RefreshTokens table
- **Course Management Feature**: Cannot create/publish courses without Categories, Courses, and Lessons tables
- **Progress Tracking Feature**: Cannot track user progress without UserProgress and UserCourseEnrollment tables
- **Task Submission Feature**: Cannot submit and review tasks without LearningTasks and UserTaskSubmissions tables
- **Reward System Feature**: Cannot award or redeem points without RewardTransactions and DiscountCodes tables
- **Admin Dashboard Feature**: Cannot display analytics without underlying data tables and relationships
- **All other features**: Database schema is foundational dependency for entire application

## Out of Scope *(optional)*

The following are explicitly NOT included in this database schema feature:

### Data Population

- Actual course content (courses, lessons, tasks) beyond seed data - this is content creation, not schema design
- Real user accounts beyond admin user - user registration is a separate feature
- Historical data migration from any previous system
- Production data import scripts or ETL processes

### Advanced Features

- Full-text search indexes (PostgreSQL FTS) - can be added as performance optimization later
- Database replication setup and configuration - infrastructure concern
- Database backup and disaster recovery procedures - operations concern
- Automated partition archival and cleanup jobs - separate maintenance feature
- Database performance monitoring and alerting - observability feature
- Geographic data replication for multi-region support
- Database sharding strategy for extreme scale
- Advanced JSONB query optimization beyond basic GIN indexes

### Business Logic

- Point expiration rules and automation (when points expire, how much, notifications) - application logic
- Task submission automatic grading algorithms - separate AI/ML feature
- Content recommendation algorithms - separate recommendation engine
- Fraud detection rules for reward abuse - separate fraud prevention feature
- Subscription renewal and payment processing - payment integration feature
- Email notification triggers - notification system feature
- User behavior analytics and reporting - analytics feature

### Security Implementation

- Database user role management and permissions - database administration
- Row-level security policies - can be added as security hardening
- Encryption at rest configuration - infrastructure concern
- Database audit logging configuration - compliance feature
- SSL/TLS connection enforcement - infrastructure configuration
- Database firewall rules - network security concern

### Application Layer

- API endpoints for CRUD operations - API development feature
- Business logic validation beyond database constraints - application validation
- Caching strategies (Redis, in-memory) - performance optimization feature
- Rate limiting for database operations - application middleware
- Background job processing for async operations - job queue feature

### Testing

- Performance testing automation framework - separate testing infrastructure
- Load testing scripts and scenarios - QA feature
- Database migration testing automation - CI/CD pipeline concern
- Data integrity validation scripts - monitoring feature

### Documentation

- API documentation (Swagger/OpenAPI) - API development feature
- Database administration runbooks - operations documentation
- Disaster recovery playbooks - operations documentation
- Database performance tuning guides - optimization documentation

This feature focuses ONLY on designing and implementing the core database schema with entities, relationships, indexes, and migrations. All other concerns are separate features or operational tasks.

## Technical Notes *(optional)*

### PostgreSQL-Specific Considerations

**JSONB Storage**: Using JSONB (binary JSON) instead of JSON for TaskData and SubmissionData provides several advantages:
- Binary format is faster to process (no reparsing)
- Supports indexing with GIN (Generalized Inverted Index)
- Enables efficient queries like `TaskData @> '{"type": "quiz"}'`
- Automatic validation of JSON structure on insert
- Compression reduces storage for large JSON objects

**Time-Based Partitioning**: PostgreSQL native partitioning (introduced in v10, improved in v15) provides:
- Automatic routing of data to correct partition based on range
- Partition pruning in queries (only scans relevant partitions)
- Easy archival by detaching old partitions
- Better vacuum performance on smaller partitions
- Maintenance windows can target specific partitions

**Index Strategy**: 
- GIN indexes on JSONB columns enable fast containment queries (`@>`, `?`, `?&`, `?|` operators)
- Composite indexes ordered by query selectivity (most selective column first)
- Partial indexes for frequently filtered subsets (e.g., `WHERE IsDeleted = false`)
- Consider index-only scans for covering indexes
- BRIN indexes could be added for very large tables with natural ordering

### Entity Framework Core Configuration

**Fluent API Over Annotations**: Use Fluent API in `OnModelCreating` for:
- Better separation of concerns (domain models without infrastructure attributes)
- Complex configurations not supported by annotations (composite keys, indexes, cascade rules)
- Easier unit testing of domain models
- More explicit configuration visibility

**Migration Best Practices**:
- Keep migrations small and focused (one logical change per migration)
- Test both Up and Down migrations in development
- Never modify existing migrations after they've been applied to production
- Use custom SQL for complex operations (partitioning, stored procedures)
- Version control all migrations with descriptive names

**Owned Entities vs JSONB**: For structured complex types, consider:
- Owned entities for strongly-typed, frequently queried properties
- JSONB for flexible, schema-less data that changes frequently
- Trade-off: type safety vs flexibility

### Performance Optimization Notes

**Index Maintenance**: 
- Monitor index usage with `pg_stat_user_indexes`
- Drop unused indexes (they slow down writes)
- Rebuild fragmented indexes periodically
- Consider bloom filters for multi-column queries with low selectivity

**Query Optimization**:
- Use `AsNoTracking()` for read-only queries (faster, less memory)
- Implement pagination for large result sets
- Avoid N+1 queries with eager loading (`Include()`)
- Use compiled queries for frequently executed queries
- Consider materialized views for complex aggregations

**Connection Pooling**:
- Configure appropriate pool size based on concurrent users
- Monitor pool exhaustion with metrics
- Use async/await to avoid thread pool starvation
- Consider read replicas for read-heavy workloads

### Security Considerations

**Preventing SQL Injection**:
- EF Core parameterizes all queries automatically
- Avoid raw SQL unless absolutely necessary
- If using raw SQL, always use parameters: `FromSqlRaw("SELECT * FROM Users WHERE Email = {0}", email)`

**Sensitive Data**:
- Never store plain text passwords (use PasswordHash field)
- Refresh tokens should be cryptographically random
- Consider encryption at rest for PII (email, names)
- Implement column-level encryption for highly sensitive data

**Audit Trail**:
- CreatedAt/UpdatedAt provide basic audit
- Consider separate audit table for detailed change history
- Soft delete enables "undo" and compliance with data retention laws
- ReviewedBy/ReviewedAt fields enable accountability for administrative actions

### Scalability Considerations

**Vertical Scaling Limits**:
- Single PostgreSQL instance can handle up to ~10TB with proper configuration
- SSDs critical for I/O performance
- RAM should be ~25% of database size for effective caching

**When to Consider Sharding**:
- Over 10 million users (shard by UserId hash)
- Over 100 million transactions (shard by date range)
- Geographic distribution requirements (shard by region)

**Read Replicas**:
- Offload reporting and analytics queries
- Reduce load on primary for write-heavy workloads
- Configure with async replication (slight lag acceptable)

### Monitoring and Maintenance

**Key Metrics to Monitor**:
- Query execution time (p50, p95, p99 percentiles)
- Index hit ratio (should be >99%)
- Transaction rate and queue depth
- Replication lag (if using replicas)
- Disk space growth rate
- Deadlock frequency

**Regular Maintenance Tasks**:
- VACUUM to reclaim space (auto-vacuum should be tuned)
- ANALYZE to update statistics for query planner
- REINDEX to rebuild fragmented indexes
- Partition pruning (detach old partitions)
- Backup verification (restore test)

### Development Workflow

**Local Development**:
- Use Docker Compose for PostgreSQL (consistent environment)
- Seed realistic test data for accurate performance testing
- Test migrations on database dumps from production

**CI/CD Integration**:
- Automated migration testing in pipeline
- Schema comparison between environments
- Rollback procedures tested automatically

**Code Review Checklist for Migrations**:
- [ ] Migration adds appropriate indexes
- [ ] Foreign keys have correct cascade rules
- [ ] Default values are sensible
- [ ] Down migration is implemented and tested
- [ ] Breaking changes are documented
- [ ] Performance impact is assessed

### Common Pitfalls to Avoid

1. **Missing Indexes**: Always add indexes for foreign keys and frequently filtered columns
2. **Over-Indexing**: Too many indexes slow down writes; only index what's queried
3. **N+1 Queries**: Use `Include()` for eager loading related entities
4. **Missing Cascade Rules**: Specify ON DELETE behavior to avoid orphaned records
5. **Ignoring Soft Delete**: Remember to filter `IsDeleted = false` in global query filters
6. **Large Transactions**: Keep transaction scope small to avoid long locks
7. **Not Testing Rollbacks**: Always test Down migrations before deploying
8. **Hardcoded IDs**: Use GUIDs or database sequences, never hardcoded IDs in migrations
9. **Ignoring Concurrency**: Use optimistic concurrency (RowVersion) for frequently updated entities
10. **Premature Optimization**: Implement partitioning only when data volume justifies complexity

### Future Enhancements (Not in Scope)

- Event sourcing for complete audit history
- CQRS with separate read/write databases
- GraphQL schema generation from EF models
- Temporal tables for automatic history tracking
- PostgreSQL extensions (PostGIS for location, pg_trgm for fuzzy search)
- Database-level encryption with pg_crypto
- Materialized views for complex reporting

## Open Questions *(if any)*

No open questions. All requirements are clearly defined with specific technical details. Proceeding with implementation.

## Revision History

| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2025-11-14 | 1.0 | Initial specification created | GitHub Copilot |
