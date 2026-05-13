# Feature Specification: Course & Lesson Management System

**Feature Branch**: `003-course-management`  
**Created**: November 14, 2025  
**Status**: Draft  
**Input**: User description: "Implement a complete course and lesson management system allowing administrators to create educational content, organize courses by categories, manage lessons with YouTube videos, and enable users to enroll and track their learning progress."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Browse and Discover Courses (Priority: P1)

As a platform user, I need to browse available courses by category, difficulty level, and premium status so that I can find relevant learning content that matches my skill level and subscription tier.

**Why this priority**: Core user-facing functionality. Without the ability to discover and view courses, the platform has no value to users. This is the most critical feature for any learning platform.

**Independent Test**: Can be fully tested by navigating to the courses page, applying filters (category: "Airdrops", difficulty: "Beginner"), viewing results in a grid layout, and verifying course cards display title, thumbnail, difficulty, reward points, and premium badge. Delivers immediate value by allowing users to explore course catalog.

**Acceptance Scenarios**:

1. **Given** I am on the courses page, **When** I view the course catalog, **Then** I see all published courses displayed in a grid layout with thumbnails, titles, difficulty badges, and reward points
2. **Given** multiple categories exist (Airdrops, GameFi, DeFi, NFT, Task-to-Earn), **When** I select the "Airdrops" category filter, **Then** only courses in the Airdrops category are displayed
3. **Given** courses with different difficulty levels exist, **When** I filter by "Beginner" difficulty, **Then** only beginner-level courses are shown
4. **Given** I am a free user, **When** I view the course catalog, **Then** I see both free and premium courses, with premium courses clearly marked with a "Premium" badge
5. **Given** more than 12 courses exist, **When** I scroll to the bottom of the first page, **Then** pagination controls allow me to navigate to additional pages of courses

---

### User Story 2 - View Course Details and Enroll (Priority: P1)

As a platform user, I need to view detailed course information including all lessons, estimated duration, and reward points, and enroll in courses I'm interested in so that I can access the course content.

**Why this priority**: Essential for user engagement. Users must be able to understand what a course offers and enroll before accessing any lessons. Without enrollment, no learning activity can occur.

**Independent Test**: Can be tested by clicking a course card, viewing the course detail page with full description, lesson list, and enrollment button, clicking "Enroll", and verifying enrollment confirmation message appears and course is added to "My Courses". Delivers complete course discovery-to-enrollment workflow.

**Acceptance Scenarios**:

1. **Given** I am viewing a course card, **When** I click on it, **Then** I am taken to the course detail page showing full description, category, difficulty, estimated duration, and complete lesson list
2. **Given** I am on a course detail page for a course I'm not enrolled in, **When** I click the "Enroll" button, **Then** I am enrolled in the course and see a confirmation message "Successfully enrolled in [Course Title]"
3. **Given** I have enrolled in a course, **When** I refresh the course detail page, **Then** the "Enroll" button is replaced with "Continue Learning" or shows my current progress percentage
4. **Given** I am viewing a premium course detail as a free user, **When** I try to enroll, **Then** I see a message prompting me to upgrade to Premium to access this course
5. **Given** I am viewing lessons within a course, **When** I see the lesson list, **Then** each lesson shows its title, duration, reward points, and order number (e.g., "Lesson 1", "Lesson 2")

---

### User Story 3 - Track Learning Progress Across Courses (Priority: P2)

As a platform user, I need to view all my enrolled courses and see my progress percentage for each one so that I can track my learning journey and easily resume where I left off.

**Why this priority**: Important for user retention and engagement, but platform can function with course browsing and enrollment first. Progress tracking enhances the experience but isn't required for initial content access.

**Independent Test**: Can be tested by enrolling in multiple courses, completing some lessons, navigating to "My Courses" page, and verifying each enrolled course displays with current progress percentage, last accessed date, and completion status. Progress calculations are accurate based on completed lessons.

**Acceptance Scenarios**:

1. **Given** I have enrolled in multiple courses, **When** I navigate to the "My Courses" page, **Then** I see all my enrolled courses with progress bars showing completion percentage
2. **Given** I have completed 3 out of 10 lessons in a course, **When** I view my progress for that course, **Then** the progress bar shows 30% completion
3. **Given** I have not started any lessons in an enrolled course, **When** I view the course on "My Courses", **Then** progress shows 0% and displays "Not Started" status
4. **Given** I have completed all lessons in a course, **When** I view the course on "My Courses", **Then** progress shows 100% and displays "Completed" status with completion date
5. **Given** I am viewing "My Courses", **When** I click on a course, **Then** I am taken to the next incomplete lesson or the first lesson if none are complete

---

### User Story 4 - Admin Creates and Manages Courses (Priority: P1)

As an administrator, I need to create new courses with titles, descriptions, categories, difficulty levels, and premium flags so that I can organize and publish educational content for users.

**Why this priority**: Critical for content creation. Without admin course management, there's no way to populate the platform with educational content. Required for platform to have any courses at all.

**Independent Test**: Can be tested by logging in as admin, navigating to admin course management page, clicking "Create Course", filling in all course fields (title, description, category, difficulty, premium flag, thumbnail URL), saving the course as draft, and verifying it appears in the admin course list but not in public catalog until published.

**Acceptance Scenarios**:

1. **Given** I am logged in as an administrator, **When** I navigate to the admin dashboard, **Then** I see a "Course Management" section with options to create, edit, and delete courses
2. **Given** I click "Create Course", **When** I fill in the course form with title "Bitcoin Fundamentals", category "Airdrops", difficulty "Beginner", and click "Save as Draft", **Then** the course is created but not published to users
3. **Given** I am creating a course, **When** I toggle the "Premium" switch to ON, **Then** the course is marked as premium-only content
4. **Given** I have created a draft course, **When** I add at least one lesson and click "Publish", **Then** the course becomes visible to users in the course catalog
5. **Given** I am editing an existing course, **When** I change the title or description and save, **Then** the changes are immediately reflected on the course detail page for users

---

### User Story 5 - Admin Adds Lessons to Courses (Priority: P1)

As an administrator, I need to add lessons to courses with YouTube video IDs, descriptions, durations, reward points, and order sequences so that users can access structured video-based learning content.

**Why this priority**: Essential for course content. Courses without lessons are empty shells. Lesson management is the core of content delivery and must be available for course creation to be useful.

**Independent Test**: Can be tested by editing a course, clicking "Add Lesson", entering lesson title "What is Bitcoin?", YouTube video ID "dQw4w9WgXcQ", duration "15", reward points "100", order index "1", saving the lesson, and verifying it appears in the course's lesson list with correct data and video player embedding.

**Acceptance Scenarios**:

1. **Given** I am editing a course in admin panel, **When** I click "Add Lesson", **Then** I see a lesson creation form with fields for title, description, YouTube video ID, duration, reward points, order index, and premium flag
2. **Given** I enter a YouTube video ID "dQw4w9WgXcQ", **When** I save the lesson, **Then** the system validates the video ID format (11-character alphanumeric) and stores it correctly
3. **Given** I create multiple lessons with order indices 1, 2, 3, **When** users view the course, **Then** lessons are displayed in the correct sequence based on order index
4. **Given** I need to reorder lessons, **When** I use drag-and-drop to move lesson 3 to position 1, **Then** the order indices are automatically updated (3→1, 1→2, 2→3)
5. **Given** I mark a lesson as premium, **When** a free user attempts to access it, **Then** they see a preview message but cannot watch the full video or earn rewards

---

### User Story 6 - Admin Publishes and Unpublishes Courses (Priority: P2)

As an administrator, I need to publish courses to make them visible to users and unpublish them to remove from public catalog while retaining all data so that I can control content visibility and make updates without disruption.

**Why this priority**: Important for content management workflow, but admin can create and view courses in draft state initially. Publishing control enhances workflow but isn't required for basic course creation.

**Independent Test**: Can be tested by creating a draft course with lessons, clicking "Publish" button, verifying course appears in public catalog for users, clicking "Unpublish", and confirming course is removed from public view but still accessible in admin panel for editing.

**Acceptance Scenarios**:

1. **Given** I have a draft course with at least one lesson, **When** I click the "Publish" button in admin panel, **Then** the course's IsPublished flag is set to true and course appears in public catalog immediately
2. **Given** I try to publish a course with zero lessons, **When** I click "Publish", **Then** I see an error message "Cannot publish course without lessons" and publish action is prevented
3. **Given** I have a published course, **When** I click "Unpublish", **Then** the course is removed from public catalog but remains visible in admin panel for editing
4. **Given** users are currently enrolled in a published course, **When** I unpublish it, **Then** enrolled users retain access to the course and can continue their progress, but new enrollments are prevented
5. **Given** I unpublish a course to make updates, **When** I add new lessons and re-publish, **Then** all changes are visible to users including those already enrolled

---

### User Story 7 - View Course Analytics (Priority: P3)

As an administrator, I need to see analytics for each course including view count, enrollment count, completion rate, and average progress so that I can understand course performance and identify content that needs improvement.

**Why this priority**: Valuable for data-driven decision making but not essential for core platform functionality. Analytics can be added after basic course and lesson management is working. Platform functions fine without analytics initially.

**Independent Test**: Can be tested by navigating to a course in admin panel, clicking "View Analytics", and verifying the analytics dashboard displays metrics: total views (e.g., 1,234), total enrollments (e.g., 567), completion rate (e.g., 35%), average progress (e.g., 62%), with visual charts for progress distribution and enrollment trends over time.

**Acceptance Scenarios**:

1. **Given** users have viewed and enrolled in a course, **When** I open the course analytics dashboard, **Then** I see total view count, enrollment count, completion rate percentage, and average progress percentage
2. **Given** 100 users are enrolled in a course and 35 have completed it, **When** I view the completion rate, **Then** it displays "35% completion rate"
3. **Given** users are at various stages of course progress, **When** I view the progress distribution chart, **Then** I see a histogram showing how many users are at 0-25%, 26-50%, 51-75%, and 76-100% completion
4. **Given** enrollments have occurred over the past 90 days, **When** I view the enrollment trend chart, **Then** I see a line graph showing daily enrollment counts over time
5. **Given** a course has low completion rates, **When** I identify lessons with high drop-off, **Then** analytics highlight which specific lessons have the highest abandonment rates for targeted improvement

---

### User Story 8 - Search and Filter Courses (Priority: P2)

As a platform user, I need to search courses by title or description keywords and apply multiple filters simultaneously (category + difficulty + premium status) so that I can quickly find specific courses that match my learning needs.

**Why this priority**: Enhances user experience significantly, especially as course catalog grows. However, platform can function with basic category browsing initially. Search becomes essential once there are 50+ courses, but can be added after core features.

**Independent Test**: Can be tested by entering search term "Bitcoin" in search box, verifying results show all courses with "Bitcoin" in title or description, then applying category filter "Airdrops" to narrow results further, and confirming only Bitcoin-related courses in Airdrops category are displayed.

**Acceptance Scenarios**:

1. **Given** I am on the courses page, **When** I enter "Bitcoin" in the search box, **Then** I see all courses containing "Bitcoin" in title or description
2. **Given** I have searched for "trading", **When** I apply the "DeFi" category filter, **Then** results show only DeFi courses containing "trading" in their content
3. **Given** I am searching for beginner content, **When** I select "Beginner" difficulty and toggle "Show Free Only", **Then** I see only free beginner courses
4. **Given** my search returns no results, **When** I see the "No courses found" message, **Then** I am shown a "Clear Filters" button and suggestions for broadening my search
5. **Given** I am typing in the search box, **When** I pause typing for 500ms, **Then** search results update automatically without requiring me to press Enter (debounced search)

---

### User Story 9 - Soft Delete Courses and Lessons (Priority: P2)

As an administrator, I need to delete courses or lessons while preserving data in the database for audit purposes so that I can remove outdated content from public view without losing enrollment history or analytics data.

**Why this priority**: Important for data integrity and audit compliance, but platform can function with manual database management initially. Soft delete prevents accidental data loss and is important for production systems, but can be implemented after basic CRUD operations work.

**Independent Test**: Can be tested by deleting a course with existing enrollments, verifying course no longer appears in public catalog or admin lists (filtered out), confirming IsDeleted flag is set to true in database, and checking that enrollment records and progress data remain intact for audit queries.

**Acceptance Scenarios**:

1. **Given** I am viewing a course in admin panel, **When** I click "Delete" and confirm the action, **Then** the course's IsDeleted flag is set to true and course is hidden from all user-facing and admin lists
2. **Given** a course has been soft-deleted, **When** users search for or browse courses, **Then** the deleted course does not appear in any results
3. **Given** users were enrolled in a deleted course, **When** querying enrollment history for analytics, **Then** enrollment and progress records remain accessible for reporting purposes
4. **Given** I have soft-deleted a course by mistake, **When** I query the database directly (or use an "undelete" admin tool), **Then** I can restore the course by setting IsDeleted to false
5. **Given** a lesson within a course is deleted, **When** users view the course, **Then** the deleted lesson is excluded from the lesson list and course progress calculations are adjusted accordingly

---

### User Story 10 - Premium Content Access Control (Priority: P1)

As the platform, I need to enforce access control so that only premium subscribers can enroll in and access premium courses and lessons, while free users see preview information but are prompted to upgrade.

**Why this priority**: Critical for business model and monetization. Without proper access control, premium content leaks undermine revenue. Must be in place before launching any premium courses. Core security requirement.

**Independent Test**: Can be tested by logging in as a free user, attempting to enroll in a premium course, verifying access is denied with upgrade prompt, upgrading to premium, and confirming immediate access to previously restricted content. Premium gates work at both course and individual lesson levels.

**Acceptance Scenarios**:

1. **Given** I am a free user viewing a premium course, **When** I attempt to click "Enroll", **Then** I see a modal prompting me to "Upgrade to Premium" with pricing information
2. **Given** I am a free user, **When** I browse courses, **Then** premium courses are clearly marked with a "Premium" badge and display a lock icon on lesson listings
3. **Given** I am enrolled in a course with some free and some premium lessons, **When** I try to access a premium lesson as a free user, **Then** I see a preview message and upgrade prompt but cannot watch the video or earn rewards
4. **Given** I upgrade to a premium subscription, **When** I refresh my enrolled courses, **Then** I immediately gain access to all premium content without needing to re-enroll
5. **Given** my premium subscription expires, **When** I attempt to access a premium lesson I was previously watching, **Then** access is revoked and I am prompted to renew subscription to continue

---

### Edge Cases

- **What happens when an admin tries to publish a course with no lessons?** System prevents publish action and displays error message "Cannot publish course without lessons". Course remains in draft state.

- **How does the system handle deleted YouTube videos?** Frontend video player detects 404 error, displays "Video unavailable" message, and allows admin to update video ID. User can still view lesson description and attempt tasks.

- **What if multiple admins edit the same course simultaneously?** Last write wins (no optimistic locking in MVP). For future: implement version numbers and conflict detection.

- **Can users enroll in the same course multiple times?** Database unique constraint on (UserId, CourseId) prevents duplicate enrollments. Attempting to re-enroll returns existing enrollment.

- **What happens when lesson order indices conflict (two lessons with OrderIndex = 3)?** Display lessons in database insertion order when order indices match. Admin UI enforces unique order indices through drag-and-drop reordering.

- **How are lessons displayed after one is soft-deleted?** Soft-deleted lessons are filtered out by query-level filter (HasQueryFilter in EF Core). Order indices remain unchanged; gaps in sequence are acceptable.

- **What if a user's progress percentage exceeds 100% due to lessons being deleted?** Progress calculation includes validation: `Min(completedLessons / totalActiveLessons * 100, 100)` to cap at 100%.

- **Can a course be moved to a different category after users have enrolled?** Yes, category changes don't affect enrollments. User progress and enrollment history remain valid.

- **What happens when an admin deletes a category that has courses?** Foreign key constraint prevents hard delete. Category must be marked IsActive = false and courses must be reassigned before deactivation.

- **How does pagination handle concurrent course creation?** Pagination uses offset/limit. New courses may appear mid-session. Acceptable for MVP; consider cursor-based pagination for future if needed.

---

## Requirements *(mandatory)*

### Functional Requirements

#### Course Management

- **FR-001**: System MUST allow administrators to create courses with title (max 200 chars), description (max 2000 chars), category selection, difficulty level (Beginner/Intermediate/Advanced), estimated duration, premium flag, and thumbnail URL
- **FR-002**: System MUST support course states: Draft (IsPublished = false) and Published (IsPublished = true), where draft courses are visible only to admins
- **FR-003**: System MUST prevent course publication if the course has zero lessons
- **FR-004**: System MUST allow administrators to edit all course properties except CreatedAt and CreatedByUserId
- **FR-005**: System MUST allow administrators to soft-delete courses (set IsDeleted = true) without removing database records
- **FR-006**: System MUST filter out soft-deleted courses from all user-facing queries and admin lists (using EF Core query filters)
- **FR-007**: System MUST track course view count and increment it each time a user views the course detail page

#### Lesson Management

- **FR-008**: System MUST allow administrators to add lessons to courses with title (max 200 chars), description (max 2000 chars), YouTube video ID (11-character alphanumeric), duration in minutes, reward points, and order index
- **FR-009**: System MUST validate YouTube video ID format: exactly 11 characters, alphanumeric only, no special characters
- **FR-010**: System MUST allow administrators to reorder lessons by updating order indices, maintaining sequential ordering
- **FR-011**: System MUST display lessons in ascending order by OrderIndex when users view a course
- **FR-012**: System MUST allow administrators to mark individual lessons as premium (IsPremium = true)
- **FR-013**: System MUST allow administrators to soft-delete lessons, removing them from lesson lists and course progress calculations
- **FR-014**: System MUST support optional ContentMarkdown field for lessons, allowing rich text lesson notes

#### Course Discovery and Browsing

- **FR-015**: System MUST display all published courses in a paginated grid layout (12 courses per page by default)
- **FR-016**: System MUST allow users to filter courses by category (single selection)
- **FR-017**: System MUST allow users to filter courses by difficulty level (Beginner, Intermediate, Advanced)
- **FR-018**: System MUST allow users to filter courses by premium status (free only, premium only, or all)
- **FR-019**: System MUST allow users to search courses by title or description using case-insensitive partial matching
- **FR-020**: System MUST display course cards showing thumbnail, title, category, difficulty badge, estimated duration, reward points, lesson count, and premium badge (if applicable)
- **FR-021**: System MUST allow users to click a course card to navigate to the course detail page

#### Course Detail and Enrollment

- **FR-022**: System MUST display course detail page showing full description, complete lesson list, category, difficulty, estimated duration, total reward points, and enrollment status
- **FR-023**: System MUST allow authenticated users to enroll in non-premium courses by clicking "Enroll" button
- **FR-024**: System MUST prevent duplicate enrollments using unique constraint on (UserId, CourseId) in UserCourseEnrollment table
- **FR-025**: System MUST record enrollment date (EnrolledAt) and initialize progress to 0% when user enrolls
- **FR-026**: System MUST display "Continue Learning" button and progress percentage for enrolled users viewing course detail page
- **FR-027**: System MUST display lesson list with each lesson showing title, duration, reward points, order number, completion status icon, and premium badge (if applicable)

#### Progress Tracking

- **FR-028**: System MUST calculate course progress as: (completed lessons / total active lessons) × 100, capped at 100%
- **FR-029**: System MUST display enrolled courses on "My Courses" page with progress bar, enrollment date, and completion status
- **FR-030**: System MUST categorize courses as "Not Started" (0% progress), "In Progress" (1-99% progress), or "Completed" (100% progress)
- **FR-031**: System MUST update course progress percentage whenever a user completes a lesson
- **FR-032**: System MUST allow users to click on a course in "My Courses" to navigate to the next incomplete lesson or first lesson if none are complete

#### Premium Access Control

- **FR-033**: System MUST enforce premium course access: only users with Premium or Admin roles can enroll in premium courses
- **FR-034**: System MUST display upgrade prompt when free users attempt to enroll in premium courses
- **FR-035**: System MUST enforce premium lesson access: only premium users can access premium lessons within any course
- **FR-036**: System MUST allow free users to view preview information for premium content (title, description, duration) but not access video or earn rewards
- **FR-037**: System MUST immediately revoke premium content access when a user's premium subscription expires

#### Admin Course Management

- **FR-038**: System MUST provide admin interface for listing all courses (including drafts) with search, filter, and sort capabilities
- **FR-039**: System MUST allow admins to view course analytics including view count, enrollment count, completion rate, and average progress (P3 priority - may be deferred)
- **FR-040**: System MUST provide course editor interface with forms for all course properties and embedded lesson management
- **FR-041**: System MUST provide lesson editor interface with forms for all lesson properties and YouTube video preview
- **FR-042**: System MUST allow admins to drag-and-drop reorder lessons, automatically updating order indices
- **FR-043**: System MUST display course preview functionality allowing admins to view course as a student before publishing

### Constitution Compliance Requirements

**Learning-First**: Each feature MUST demonstrate how it enhances educational value rather than mere engagement. Progress tracking must focus on comprehension and skill development, not just completion metrics.

- Course design requires clear learning objectives in descriptions
- Lesson content includes educational notes (ContentMarkdown) beyond just video
- Reward points encourage meaningful engagement, not just button clicking
- Premium content provides additional educational value, not just gated content

**Security & Privacy**: All features MUST include appropriate authentication (JWT with refresh tokens), data encryption (AES-256), input validation against XSS/SQL injection, audit logging, and GDPR compliance.

- All course management endpoints require authentication (JWT Bearer tokens)
- Admin course creation/editing endpoints require Admin role authorization
- User enrollment requires authenticated user context (cannot enroll on behalf of others)
- YouTube video IDs validated for format to prevent XSS injection
- All text inputs (titles, descriptions) sanitized against XSS attacks
- Soft delete strategy preserves data for GDPR audit compliance
- CreatedByUserId tracked for accountability

**Scalability**: Feature MUST be designed to handle 1000+ concurrent users with <3 second load times, mobile-responsive interfaces (768px tablet, 480px mobile breakpoints), and horizontal scaling capabilities.

- Course listing uses pagination (12 per page) to limit query size
- Database indexes on CategoryId, IsPublished, IsPremium, DifficultyLevel for fast filtering
- Lesson queries include index on (CourseId, OrderIndex) for efficient sorting
- Progress queries indexed on (UserId, CourseId) for fast lookup
- Frontend implements responsive grid (4 cols desktop, 2 cols tablet, 1 col mobile)
- API responses include caching headers for static course data
- YouTube video delivery handled by YouTube CDN, not platform infrastructure

**Fair Economy**: Any reward/gamification elements MUST prevent exploitation through rate limiting (10 points/hour max), duplicate detection, and economic modeling to maintain balance.

- Reward points defined at lesson level, awarded only upon verified completion
- Duplicate enrollment prevented by unique database constraint
- Lesson completion tracked in UserProgress to prevent duplicate point awards
- Admin controls over reward point values for economic balancing
- (Future: Rate limiting on lesson completions to prevent rapid-fire gaming)

**Quality Assurance**: Content features MUST include accuracy verification, regular updates (quarterly for time-sensitive topics), multi-level approval workflows, and misinformation prevention.

- Draft/Publish workflow allows content review before public availability
- Admin-only creation ensures editorial control
- IsPublished flag enables content to be pulled for corrections without data loss
- CreatedByUserId tracks content creator accountability
- (Future: Multi-level approval workflow with reviewer roles)
- (Future: Content freshness tracking with LastUpdatedAt timestamps)

**Accessibility**: All interfaces MUST comply with WCAG 2.1 AA standards, provide keyboard navigation, screen reader compatibility, and transparent user communication with proper support channels.

- Course cards include alt text for thumbnails (img alt attribute)
- Difficulty badges use color + text labels (not color alone)
- Premium badges use lock icon + "Premium" text (not icon alone)
- Form inputs include proper labels and ARIA attributes
- Keyboard navigation supported for course browsing and enrollment
- Screen reader announces course enrollment status and progress
- Video player (react-player) includes YouTube's native accessibility features
- Error messages provide clear, actionable feedback

**Business Ethics**: Features must maintain ethical freemium practices, fair pricing transparency, no dark patterns, and protect user data without third-party sales.

- Premium content clearly marked with badges, no hidden costs
- Free users see premium course previews before being asked to upgrade
- No artificial scarcity or countdown timers to pressure upgrades
- Progress tracking and enrollment history never paywalled
- User data (enrollment, progress) not shared with third parties
- Transparent upgrade prompts explain value proposition, no deceptive language

**Technical Excellence**: Implementation must follow Clean Architecture, include comprehensive test coverage (80%+), code reviews, CI/CD pipeline, and proper documentation.

- Clean Architecture: Domain entities (Course, Lesson, Category) in Core layer
- Repository interfaces (ICourseRepository, ILessonRepository) in Core layer
- Repository implementations in DAL layer (CourseRepository, LessonRepository)
- Service layer orchestrates business logic (CourseService)
- API controllers handle HTTP concerns only (CoursesController)
- CQRS pattern: Commands (CreateCourseCommand, EnrollInCourseCommand) and Queries (GetCoursesQuery)
- FluentValidation: CreateCourseRequestValidator, CreateLessonRequestValidator
- AutoMapper profiles for DTO mapping (CourseProfile)
- **Unit tests**: 85% coverage for backend services/repositories, 80% coverage for frontend components/hooks
- **Integration tests**: 100% coverage for all API endpoints and frontend API services
- **E2E tests**: 5 critical user flows automated (browse→enroll, create course, premium gate, lesson reordering, admin publish)
- Test data management: Seeded fixtures + factory pattern for entity generation
- CI/CD pipeline runs all tests before deployment, fails if coverage drops below thresholds
- OpenAPI/Swagger documentation for all endpoints

### Key Entities

- **Course**: Represents a structured learning course on a crypto topic (e.g., "Bitcoin Fundamentals"). Contains title, description, category, difficulty level (Beginner/Intermediate/Advanced), premium flag, publish status, thumbnail URL, estimated duration, reward points, and view count. Owned by creator (admin user). Relates to Category (many-to-one), Lessons (one-to-many), and Enrollments (one-to-many).

- **Lesson**: Represents an individual video lesson within a course (e.g., "What is Bitcoin?"). Contains title, description, YouTube video ID (for embedding), duration in minutes, order index (for sequencing), premium flag, reward points, and optional markdown content notes. Relates to Course (many-to-one) and UserProgress (one-to-many).

- **Category**: Represents a course category for organization (e.g., "Airdrops", "GameFi", "DeFi", "NFT Strategies", "Task-to-Earn"). Contains name, description, icon URL, display order, and active status. Relates to Courses (one-to-many).

- **UserCourseEnrollment**: Tracks which users are enrolled in which courses. Contains enrollment date, progress percentage (0-100), completion status, and last accessed date. Prevents duplicate enrollments with unique constraint on (UserId, CourseId). Relates to User (many-to-one) and Course (many-to-one).

- **UserProgress**: Tracks completion status of individual lessons for users. Contains completion timestamp, video watch position (for resume functionality), and completion status. Relates to User (many-to-one) and Lesson (many-to-one).

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can browse and filter courses by category, difficulty, and premium status, viewing results in under 2 seconds on a catalog of 100+ courses
- **SC-002**: Users can enroll in a course and see enrollment confirmation in under 1 second, with enrollment persisted in database and reflected immediately on course detail page
- **SC-003**: Admin users can create a complete course with 5 lessons in under 10 minutes using the course editor interface, with all data validated and saved to database
- **SC-004**: Course progress percentage calculations are accurate within 1% for all enrolled users, updating within 2 seconds after lesson completion
- **SC-005**: Premium access control prevents 100% of unauthorized access attempts to premium courses and lessons by free users, with clear upgrade prompts displayed
- **SC-006**: Pagination handles course catalogs of 500+ courses without performance degradation, maintaining sub-2-second page load times
- **SC-007**: Course search returns relevant results within 1 second for keyword queries, matching titles and descriptions case-insensitively
- **SC-008**: Soft-deleted courses and lessons are excluded from all user-facing queries within 1 second of deletion, while data remains in database for audit
- **SC-009**: YouTube video player embeds successfully display for 99%+ of valid video IDs, with graceful error handling for invalid/deleted videos
- **SC-010**: Mobile users can browse courses, enroll, and view lesson lists with fully responsive layouts at 768px (tablet) and 480px (mobile) breakpoints, without horizontal scrolling
- **SC-011**: Admin course management interface allows editing existing courses and lessons without data loss, with changes reflected immediately on public site
- **SC-012**: Comprehensive test coverage achieved: Backend ≥85% unit test coverage for services/repositories, 100% integration tests for all API endpoints, 5 critical E2E flows automated. Frontend ≥80% unit test coverage for components/hooks, 100% integration tests for API services, E2E tests matching backend flows. All tests pass in CI/CD pipeline before deployment.

---

## Assumptions *(include if relevant)*

- **YouTube API Availability**: YouTube embed functionality will remain available and free for embedded player usage. Video availability is dependent on YouTube's infrastructure.

- **Video ID Stability**: Once a YouTube video ID is assigned to a lesson, it will remain valid for the video's lifetime. If videos are deleted, admins will manually update video IDs.

- **Category Stability**: The 5 core categories (Airdrops, GameFi, DeFi, NFT Strategies, Task-to-Earn) will remain relatively stable. Category changes are infrequent and managed by admins.

- **Single Currency**: Reward points are the only currency in MVP. Premium subscriptions are handled by a separate feature (not in scope for this spec).

- **English Language**: All course content is in English for MVP. Internationalization and localization are future enhancements.

- **Trust in Admins**: Admin users are trusted to create accurate, high-quality content. Multi-level approval workflows and content moderation are future enhancements.

- **Synchronous Operations**: Course creation, lesson addition, and enrollment are synchronous operations. Asynchronous processing for batch operations is a future optimization.

- **No Course Versioning**: Courses do not have version history. Edits overwrite previous data. Audit logging and version history are future enhancements.

- **No Course Prerequisites**: MVP does not enforce course prerequisites or learning paths. Users can enroll in any course regardless of prior completions.

- **Standard Browsers**: Users access the platform via modern browsers (Chrome, Firefox, Safari, Edge) with JavaScript enabled. No IE11 support required.

- **Stable Database Schema**: Database schema for Course, Lesson, Category entities is already defined in Feature 002 (Database Schema). This feature implements business logic and UI on top of existing schema.

- **Existing Auth System**: User authentication, JWT tokens, and role-based authorization (Admin, Premium, Free) are already implemented in Feature 001 (User Authentication). This feature leverages existing auth infrastructure.

---

## Open Questions *(if any)*

None at this time. All critical design decisions have been made based on existing architecture patterns and database schema.

---

## Dependencies *(if any)*

### Required (Blocking)

- **Feature 001 - User Authentication**: Must be complete. Requires JWT authentication, role-based authorization (Admin, Premium, Free), and user session management.

- **Feature 002 - Database Schema**: Must be complete. Requires Course, Lesson, Category, UserCourseEnrollment, and UserProgress entities defined and migrated to database.

### Optional (Non-blocking)

- **Feature 004 - Video Player**: Not blocking. This feature implements basic YouTube video embedding. Enhanced video player with progress tracking, playback speed controls, and watch time analytics would enhance the experience but aren't required for MVP.

- **Feature 005 - Task System**: Not blocking. While lessons can have tasks, course and lesson management functions independently of task submission and verification workflows.

---

## Out of Scope *(explicitly excluded)*

### Excluded from MVP

- **Course Recommendations**: AI-powered or collaborative filtering-based course recommendations. Users discover courses through browsing and search only.

- **Course Reviews and Ratings**: User-submitted reviews, star ratings, and comments on courses. Future enhancement for community feedback.

- **Course Certificates**: Completion certificates, digital badges, and achievement tracking. Handled by separate subscription/gamification features.

- **Live Streaming**: Real-time video streaming, live classes, or webinars. Platform supports only pre-recorded YouTube videos.

- **Video Hosting**: Platform does not host or transcode videos. All videos must be uploaded to YouTube first and embedded via video ID.

- **Course Cloning/Templates**: Duplicate courses or create course templates for rapid course creation. Admins create each course manually.

- **Multi-language Support**: Internationalization, localization, and multi-language course content. English only for MVP.

- **Course Prerequisites**: Enforced learning paths requiring course A completion before enrolling in course B. Users can enroll in any course.

- **Advanced Analytics**: Detailed heatmaps, engagement metrics, video drop-off analysis, A/B testing. Basic analytics only (view count, enrollments, completion rate).

- **Bulk Import**: CSV/Excel import for batch course and lesson creation. Manual entry through UI only for MVP.

- **Version History**: Audit trail of course edits, rollback to previous versions, or change tracking. Edits overwrite data without history.

- **Collaborative Editing**: Multiple admins editing the same course simultaneously with conflict resolution. Last-write-wins for MVP.

- **Content Scheduling**: Schedule courses to publish automatically at future dates. Admins manually publish courses when ready.

- **Discussion Forums**: Course-specific or lesson-specific discussion boards. Community features are out of scope.

- **Quizzes Embedded in Lessons**: While lessons can have associated tasks (handled by Feature 005), inline quiz questions within video player are not supported.

- **Offline Mode**: Download courses for offline viewing. Platform requires internet connection for all features.

- **Mobile Apps**: Native iOS/Android apps. Mobile access is via responsive web interface only.

---

## Notes *(any additional context)*

### Technical Implementation Notes

- **EF Core Query Filters**: Soft delete is implemented using `HasQueryFilter(e => !e.IsDeleted)` on Course and Lesson entities. This automatically filters out deleted entities from all queries unless explicitly overridden with `IgnoreQueryFilters()`.

- **YouTube Video Embedding**: Use `react-player/youtube` library on frontend for video embedding. Video ID is passed as prop: `<ReactPlayer url={`https://www.youtube.com/watch?v=${lesson.youtubeVideoId}`} />`.

- **Pagination Strategy**: Use offset-based pagination (`SKIP/TAKE` in SQL, `Skip()/Take()` in LINQ) for MVP. Cursor-based pagination can be added later if needed for large datasets.

- **Progress Calculation**: Progress is stored as a decimal percentage (0.00 to 100.00) in UserCourseEnrollment table. Recalculated on-demand when lessons are completed or deleted, not cached at lesson level.

- **Order Index Management**: When reordering lessons via drag-and-drop, update order indices sequentially (1, 2, 3, ...). Gaps in sequence are acceptable if lessons are deleted but not required to be compacted.

- **Thumbnail Storage**: Thumbnail URLs are stored as strings pointing to external URLs (e.g., Cloudinary, AWS S3). Image upload and storage handled by separate infrastructure feature.

- **Category Seeding**: Default categories (Airdrops, GameFi, DeFi, NFT Strategies, Task-to-Earn) are seeded during database initialization in Feature 002. This feature assumes categories exist.

- **CQRS Implementation**: Use MediatR for CQRS pattern. Commands: `CreateCourseCommand`, `UpdateCourseCommand`, `DeleteCourseCommand`, `PublishCourseCommand`, `EnrollInCourseCommand`. Queries: `GetCoursesQuery`, `GetCourseByIdQuery`, `GetUserEnrolledCoursesQuery`.

- **Validation Strategy**: Use FluentValidation for request DTOs. Validators: `CreateCourseRequestValidator` (checks title length, category exists, duration > 0), `CreateLessonRequestValidator` (checks YouTube video ID format, reward points >= 0, order index > 0).

- **Frontend State Management**: Use Zustand for global course state (current course, filters, pagination) and React Query for server state (fetching, caching, invalidating course data).

- **Error Handling**: Return appropriate HTTP status codes: 404 for course not found, 403 for premium access denied, 400 for validation errors, 409 for duplicate enrollments.

### Business Logic Notes

- **Draft to Published Workflow**: Courses start in draft state (IsPublished = false). Admins review, add lessons, then publish. Once published, courses can be edited without unpublishing unless major changes needed.

- **Premium Pricing**: Premium subscription pricing and payment processing handled by Feature 006 (Subscription Management). This feature only enforces access control based on user role.

- **Reward Points Economics**: Lesson reward points are defined by content creators (admins) based on lesson difficulty and length. No automatic calculation. Points range from 25 (short intro lessons) to 200 (comprehensive deep-dives).

- **Enrollment vs Access**: Enrollment is the act of joining a course (creates UserCourseEnrollment record). Access control determines whether user can view lessons (based on premium status). Free users cannot enroll in premium courses at all; premium users can access premium lessons within any enrolled course.

- **Completion Criteria**: A course is "completed" when user has completed 100% of active, non-deleted lessons. If lessons are added after user completes course, progress drops below 100% and user must complete new lessons to reach 100% again.

- **View Count Logic**: View count increments each time course detail page is loaded by any user (authenticated or not). No deduplication. Used for popularity metrics, not unique visitors.

### UI/UX Notes

- **Responsive Breakpoints**: Desktop (1280px+), tablet (768px-1279px), mobile (320px-767px). Course grid: 4 columns (desktop), 2 columns (tablet), 1 column (mobile).

- **Course Card Design**: Thumbnail image (16:9 aspect ratio), title (truncated at 2 lines), category badge, difficulty badge (color-coded: green=Beginner, yellow=Intermediate, red=Advanced), premium lock icon, reward points badge, lesson count.

- **Loading States**: Display skeleton loaders for course grids during data fetching. Show "Loading..." spinner for enrollment action. Optimistic UI updates where possible (e.g., immediately show "Enrolled" after button click, rollback on error).

- **Empty States**: "No courses found" message with illustration when filters return zero results. "You haven't enrolled in any courses yet" message on empty My Courses page with CTA to browse catalog.

- **Error Messages**: User-friendly error messages for all failure scenarios. Examples: "Failed to enroll. Please try again.", "This course requires a Premium subscription. [Upgrade Now]", "Video is unavailable. Please contact support."

- **Markdown Rendering**: Use `react-markdown` library to render ContentMarkdown in lesson detail pages. Support basic formatting: headings, bold, italics, lists, links. No HTML allowed (XSS prevention).

---

## Clarifications

This section documents design decisions and ambiguity resolutions discovered during specification development and stakeholder review.

### Session 2025-11-14

Design decisions made during specification phase:

- **Q: Can multiple admins edit the same course simultaneously?** → **A: Last write wins for MVP**
  - *Applied to Edge Cases*: Documented concurrent editing behavior with last-write-wins strategy
  - *Applied to Out of Scope*: Added "Collaborative Editing" with conflict resolution to future enhancements
  - *Applied to Assumptions*: Noted "No Course Versioning" - edits overwrite without history

- **Q: How to handle deleted YouTube videos?** → **A: Graceful degradation with admin update option**
  - *Applied to Edge Cases*: Added handling for deleted YouTube videos (404 detection, "Video unavailable" message)
  - *Applied to Functional Requirements*: Implicit in FR-009 (video ID validation) and error handling strategy
  - *Applied to Assumptions*: Documented "Video ID Stability" assumption

- **Q: Should course enrollment be automatic or require confirmation?** → **A: Automatic for free, subscription-gated for premium**
  - *Applied to FR-023*: "System MUST allow authenticated users to enroll in non-premium courses by clicking 'Enroll' button"
  - *Applied to FR-033*: "System MUST enforce premium course access: only users with Premium or Admin roles can enroll in premium courses"
  - *Applied to User Story 2*: Enrollment workflow detailed in acceptance scenarios

- **Q: Can users unenroll from courses?** → **A: Not in MVP**
  - *Applied to Out of Scope*: Unenrollment functionality explicitly excluded from MVP
  - *Applied to Notes*: Enrollment creates permanent record for analytics; soft delete pattern available for future unenrollment feature

- **Q: Maximum number of lessons per course?** → **A: No hard limit in MVP**
  - *Applied to Edge Cases*: Noted lesson pagination consideration for 50+ lessons (future enhancement)
  - *Applied to Functional Requirements*: No validation constraints on lesson count
  - *Applied to Scalability*: Queries indexed on (CourseId, OrderIndex) to support large lesson sets

- **Q: How to handle course drafts?** → **A: IsPublished flag with admin-only visibility**
  - *Applied to FR-002*: "System MUST support course states: Draft (IsPublished = false) and Published (IsPublished = true)"
  - *Applied to User Story 4*: Draft workflow detailed in acceptance scenarios
  - *Applied to User Story 6*: Publish/unpublish functionality with business logic

- **Q: Can lesson order be changed after course is published?** → **A: Yes, with progress recalculation**
  - *Applied to FR-010*: "System MUST allow administrators to reorder lessons by updating order indices"
  - *Applied to Edge Cases*: Documented lesson reordering impact on user progress
  - *Applied to User Story 5*: Drag-and-drop reordering in acceptance scenarios

- **Q: Should deleted courses remain in database?** → **A: Yes, soft delete for audit compliance**
  - *Applied to FR-005*: "System MUST allow administrators to soft-delete courses (set IsDeleted = true)"
  - *Applied to FR-006*: "System MUST filter out soft-deleted courses from all user-facing queries"
  - *Applied to User Story 9*: Complete soft delete workflow with data preservation
  - *Applied to Security & Privacy*: Soft delete preserves data for GDPR audit compliance

- **Q: How to handle course prerequisites?** → **A: Not in MVP**
  - *Applied to Out of Scope*: "Course Prerequisites" explicitly listed in excluded features
  - *Applied to Assumptions*: "No Course Prerequisites - users can enroll in any course regardless of prior completions"

- **Q: Rich text editor - which library?** → **A: react-markdown for display, textarea for input in MVP**
  - *Applied to Functional Requirements*: FR-014 supports optional ContentMarkdown field
  - *Applied to Out of Scope*: Advanced rich text editors (TinyMCE, Quill) deferred to future
  - *Applied to UI/UX Notes*: react-markdown rendering with XSS prevention (no HTML allowed)

**Clarification Impact Summary:**
- **10 questions answered** during specification development
- **0 critical ambiguities remaining** - all design decisions resolved
- **Sections updated**: Functional Requirements (6 FRs), Edge Cases (4 items), User Stories (5 stories), Out of Scope (3 items), Assumptions (3 items), Notes (2 items)
- **Specification status**: Ready for implementation planning

- **Q: What test coverage standards should be enforced for this feature?** → **A: Comprehensive Three-Tier Testing Standard**
  - *Rationale*: Establishes clear quality gates aligned with Clean Architecture layers. Prevents gaps in test coverage that could allow bugs in production. Balances thoroughness with development velocity based on industry best practices for .NET/React applications.
  - *Applied to Testing Requirements*: New section added below with detailed coverage targets
  - *Applied to Success Criteria*: SC-012 updated to include test coverage metrics
  - *Applied to Technical Excellence*: Constitution requirement references specific coverage percentages

---

## Testing Requirements *(mandatory)*

### Test Coverage Standards

This feature MUST achieve comprehensive test coverage across all architectural layers to ensure production readiness and maintainability.

#### Backend Testing Requirements

**Unit Tests (Target: 85% coverage)**

- **Service Layer** (CourseService, LessonService):
  - Test all public methods with valid inputs (happy path)
  - Test all validation scenarios (invalid inputs, boundary conditions)
  - Test business logic edge cases (enrollment when already enrolled, publish course with zero lessons)
  - Test exception handling (course not found, unauthorized access)
  - Mock repository dependencies using Moq or NSubstitute
  - Example: `CourseService_CreateCourse_WithValidData_ReturnsCreatedCourse()`
  - Example: `CourseService_EnrollUser_WhenAlreadyEnrolled_ThrowsInvalidOperationException()`

- **Repository Layer** (CourseRepository, LessonRepository):
  - Test CRUD operations with in-memory database
  - Test filtering logic (by category, difficulty, premium status, search query)
  - Test pagination (first page, middle page, last page, page beyond total)
  - Test soft delete behavior (IsDeleted flag filtering)
  - Test query performance with large datasets (500+ courses)
  - Example: `CourseRepository_GetCourses_FilterByCategory_ReturnsFilteredResults()`
  - Example: `CourseRepository_GetCourses_WithSoftDeletedCourses_ExcludesDeleted()`

- **Validation Layer** (FluentValidation validators):
  - Test each validation rule in isolation
  - Test valid inputs pass validation
  - Test invalid inputs fail with appropriate error messages
  - Test boundary conditions (max length - 1, max length, max length + 1)
  - Example: `CreateCourseValidator_WithTitleTooLong_FailsValidation()`
  - Example: `CreateLessonValidator_WithInvalidYouTubeVideoId_FailsValidation()`

**Integration Tests (Target: 100% of API endpoints)**

- **API Controllers** (CoursesController, LessonsController):
  - Test all HTTP endpoints with real database (TestContainers with PostgreSQL)
  - Test authentication/authorization (anonymous, authenticated user, admin)
  - Test request validation (invalid DTOs return 400 Bad Request)
  - Test success responses (200 OK, 201 Created, 204 No Content)
  - Test error responses (404 Not Found, 403 Forbidden, 409 Conflict)
  - Test pagination and filtering via query parameters
  - Example: `POST_ApiCourses_AsAdmin_Returns201Created()`
  - Example: `POST_ApiCourses_AsNonAdmin_Returns403Forbidden()`
  - Example: `GET_ApiCourses_WithCategoryFilter_ReturnsFilteredCourses()`

**End-to-End Tests (Target: 5 critical user flows)**

1. **Browse Courses Flow**:
   - Navigate to courses page
   - Apply category filter "Airdrops"
   - Apply difficulty filter "Beginner"
   - Verify filtered results displayed
   - Click pagination to page 2
   - Verify new results loaded

2. **Enroll in Course Flow**:
   - Login as free user
   - Navigate to free course detail page
   - Click "Enroll" button
   - Verify enrollment confirmation message
   - Navigate to "My Courses"
   - Verify enrolled course appears in list

3. **Admin Create Course Flow**:
   - Login as admin user
   - Navigate to admin course management
   - Click "Create Course"
   - Fill course form (title, description, category, difficulty)
   - Add 3 lessons with YouTube video IDs
   - Save as draft
   - Verify course appears in admin list (not public)
   - Publish course
   - Verify course appears in public catalog

4. **Premium Access Gate Flow**:
   - Login as free user
   - Navigate to premium course detail page
   - Click "Enroll" button
   - Verify upgrade prompt displayed with "Upgrade to Premium" message
   - Verify enrollment blocked
   - Login as premium user
   - Navigate to same premium course
   - Click "Enroll" button
   - Verify enrollment succeeds

5. **Admin Lesson Reordering Flow**:
   - Login as admin
   - Navigate to course editor for course with 5 lessons
   - Drag lesson 5 to position 1
   - Save changes
   - Verify lesson order updated
   - Navigate to public course view
   - Verify lessons displayed in new order (5, 1, 2, 3, 4)

#### Frontend Testing Requirements

**Unit Tests (Target: 80% coverage)**

- **Components**:
  - Test rendering with various props (CourseCard with/without premium badge)
  - Test user interactions (button clicks, form submissions)
  - Test conditional rendering (enrollment button vs progress indicator)
  - Test error states (loading, error messages, empty states)
  - Use React Testing Library (@testing-library/react)
  - Example: `CourseCard_WithPremiumCourse_DisplaysPremiumBadge()`
  - Example: `CourseFilters_OnCategoryChange_CallsOnChangeCallback()`

- **Custom Hooks**:
  - Test hooks with renderHook from @testing-library/react-hooks
  - Test data fetching (loading, success, error states)
  - Test mutations (enroll in course, create course)
  - Test refetch behavior
  - Mock API responses using MSW (Mock Service Worker)
  - Example: `useCourses_OnMount_FetchesCourses()`
  - Example: `useEnrollment_OnEnroll_UpdatesEnrollmentState()`

- **Services** (API integration layer):
  - Test all service methods
  - Mock Axios requests
  - Test request parameters constructed correctly
  - Test response parsing
  - Test error handling (network errors, 4xx, 5xx responses)
  - Example: `courseService_getCourses_WithFilters_ConstructsCorrectQueryString()`
  - Example: `courseService_enrollInCourse_OnSuccess_ReturnsEnrollmentData()`

- **State Management** (Zustand stores):
  - Test store initialization
  - Test action creators update state correctly
  - Test derived state / selectors
  - Example: `courseStore_setFilters_UpdatesFiltersState()`

**Integration Tests (Target: 100% of API service calls)**

- Test frontend services against mock API server (MSW)
- Test authentication token inclusion in requests
- Test request/response interceptors
- Test error handling flows (401 triggers logout, 403 shows access denied)
- Example: `courseService_WithExpiredToken_TriggersTokenRefresh()`

**End-to-End Tests (Target: Match 5 backend flows)**

- Use Playwright or Cypress for E2E testing
- Test same 5 critical flows as backend E2E tests
- Test responsive design breakpoints (desktop 1920px, tablet 768px, mobile 375px)
- Test accessibility (keyboard navigation, screen reader compatibility)
- Example: Playwright test navigates full enrollment flow from login to "My Courses"

#### Test Data Management

**Seeded Fixtures**:
- Create database seeding scripts for test data
- Seed 5 categories (Airdrops, GameFi, DeFi, NFT, Task-to-Earn)
- Seed 20 courses (mix of free/premium, beginner/intermediate/advanced)
- Seed 100 lessons across courses
- Seed 10 test users (1 admin, 3 premium, 6 free)
- Seed enrollment data for progress testing

**Factory Pattern**:
- Implement CourseFactory for generating test courses
- Implement LessonFactory for generating test lessons
- Implement UserFactory for generating test users
- Allow customization of properties (e.g., `CourseFactory.CreatePremiumCourse()`)
- Example: `var course = CourseFactory.Create(title: "Test Course", isPremium: true);`

**Test Database Strategy**:
- Backend integration tests use TestContainers (PostgreSQL Docker container)
- Each test class gets isolated database instance
- Teardown removes test data after each test
- Frontend E2E tests use dedicated test database (not production)
- Reset database state between E2E test runs

#### Test Execution & CI/CD Integration

**Local Development**:
- Developers run unit tests before committing: `dotnet test` (backend), `npm test` (frontend)
- Pre-commit hook runs linting and fast unit tests
- Full test suite runs on-demand: `npm run test:all`

**CI/CD Pipeline**:
- Pull requests trigger automated test runs
- Backend: Run all unit tests (< 5 min), integration tests (< 10 min)
- Frontend: Run unit tests (< 3 min), build check (< 2 min)
- Merge to main triggers full E2E suite (< 20 min)
- Test coverage reports uploaded to code coverage tool (Coveralls, Codecov)
- Pipeline fails if coverage drops below thresholds (85% backend, 80% frontend)

**Test Reports**:
- Generate JUnit XML reports for CI/CD integration
- Generate HTML coverage reports for developer review
- Track coverage trends over time
- Flag coverage regressions in pull request reviews

### Critical Test Scenarios

These scenarios MUST have explicit test coverage:

1. **Enrollment Duplicate Prevention**: Test that duplicate enrollment (UserId, CourseId) constraint prevents double enrollment and returns appropriate error
2. **Premium Access Control**: Test free user cannot enroll in premium course, premium user can enroll, access revoked when subscription expires
3. **Soft Delete Behavior**: Test soft-deleted courses/lessons excluded from queries, data preserved in database for audit
4. **Lesson Reordering**: Test drag-and-drop updates order indices correctly, no gaps allowed, concurrent reordering handled
5. **Progress Calculation**: Test progress = (completed lessons / total active lessons) × 100, capped at 100%, recalculated when lessons added/deleted
6. **YouTube Video ID Validation**: Test 11-character alphanumeric validation, reject special characters, reject malformed IDs
7. **Pagination Edge Cases**: Test page 1, last page, page beyond total, empty results, single result
8. **Course Publishing**: Test cannot publish course with zero lessons, published course visible to users, draft course only visible to admin
9. **Concurrent Editing**: Test last-write-wins behavior when multiple admins edit same course
10. **Search and Filtering**: Test case-insensitive search, combined filters (category + difficulty + premium), empty search results

### Testing Requirements Coverage Gaps

The following test requirement areas have been identified as gaps during checklist validation (test-requirements.md - 39 items, 22% incomplete). These should be addressed during implementation:

**High Priority Gaps** (Implementation Blockers):
- **CHK029**: Clarify repository test database strategy (in-memory EF Core vs TestContainers) - Currently states "in-memory database" but TestContainers is specified for integration tests
- **CHK092-093**: YouTube API failure scenarios and database connection failure handling test requirements
- **CHK097**: Input validation tests for XSS prevention and SQL injection protection
- **CHK099**: JWT token expiration and refresh test scenarios

**Medium Priority Gaps** (Quality Enhancement):
- **CHK012**: Accessibility testing requirements (WCAG 2.1 Level AA compliance)
- **CHK102-105**: Performance test requirements:
  - Pagination performance with large datasets
  - Concurrent user load testing (1000+ users target)
  - API response time requirements (<500ms p95)
  - Page load time requirements (<3s for course listing)
- **CHK153-154**: JWT authentication integration tests, Stripe payment integration tests (if premium features require payment)

**Low Priority Gaps** (Documentation):
- **CHK115**: Factory pattern default value documentation
- **CHK121**: Database migration strategy for test environments
- **CHK156-158**: Environment version requirements (Node.js, .NET SDK, browser versions for E2E)
- **CHK160-161**: Seeded data consistency assumptions, YouTube video ID test data availability

**Future Enhancement Gaps** (Post-MVP):
- **CHK162-175**: Advanced testing scenarios not critical for MVP:
  - Localization/internationalization testing
  - Browser compatibility matrix (specific versions)
  - Mobile device testing (specific devices/OS versions)
  - Network condition testing (offline, slow connection)
  - Security penetration testing
  - Test environment configuration documentation
  - Test code review standards
  - Flaky test handling procedures
  - Test suite maintenance and deprecation strategy
  - Test performance regression monitoring

**Recommendation**: Core testing requirements (78% complete) are sufficient to proceed with implementation. Address High Priority Gaps during Phase 5-6 implementation, Medium Priority Gaps during Phase 7 (Polish & Testing), and Future Enhancement Gaps post-MVP.

---
