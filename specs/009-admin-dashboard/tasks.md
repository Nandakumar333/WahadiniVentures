# Implementation Tasks: Admin Dashboard

**Feature**: 009-admin-dashboard  
**Branch**: `009-admin-dashboard`  
**Created**: December 16, 2025  
**Total Estimated Effort**: 120-140 hours per developer (240-280 total dev hours, 6-7 weeks elapsed with 2 developers)

---

## Overview

This document breaks down the admin dashboard feature into granular, executable tasks organized by user story priority. Each phase represents an independently testable increment that delivers value.

**Implementation Strategy**: 
- MVP Scope: Phase 3 (User Story 1 - Platform Health Overview)
- Incremental Delivery: Each user story phase can be deployed independently
- Parallel Execution: Tasks marked `[P]` can be executed in parallel within the same phase

**User Story Priorities** (from spec.md):
1. **P1**: Platform Health Overview (Dashboard KPIs & Analytics) - **MVP**
2. **P2**: Task Review Workflow (Approve/Reject Submissions)
3. **P3**: User Account Management (Search, Ban, Role Management)
4. **P4**: Course Content Management (CMS with Rich Text)
5. **P5**: Reward System Management (Discount Codes, Points)
6. **P6**: Analytics and Insights (Advanced Charts)
7. **P7**: Audit Log and Accountability (Compliance Logging)

---

## Phase 1: Setup & Infrastructure

**Goal**: Establish project foundation with authentication, authorization, and admin-specific infrastructure.

**Estimated Effort**: 12-16 hours

**Deliverables**:
- SuperAdmin policy configured
- Admin middleware and audit logging infrastructure
- Database migrations for new entities
- Admin route structure (frontend)

### Backend Infrastructure

- [X] T001 [P] Create AuditLogEntry entity in `backend/src/WahadiniCryptoQuest.Core/Entities/AuditLogEntry.cs` with factory method, immutable properties (Timestamp, AdminUserId, ActionType, ResourceType, ResourceId, BeforeValue, AfterValue, IPAddress)
- [X] T002 [P] Create UserNotification entity in `backend/src/WahadiniCryptoQuest.Core/Entities/UserNotification.cs` with factory method (UserId, Type, Message, IsRead, CreatedAt)
- [X] T003 [P] Create PointAdjustment entity in `backend/src/WahadiniCryptoQuest.Core/Entities/PointAdjustment.cs` with validation (UserId, PreviousBalance, NewBalance, AdjustmentAmount, Reason, AdminUserId, Timestamp)
- [X] T004 Extend User entity in `backend/src/WahadiniCryptoQuest.Core/Entities/User.cs` to add IsBanned (bool), BanReason (string), BannedAt (DateTime?), BannedBy (Guid?) properties
- [X] T005 Extend DiscountCode entity in `backend/src/WahadiniCryptoQuest.Core/Entities/DiscountCode.cs` to add UsageLimit (int), UsageCount (int), CreatedBy (Guid) properties
- [X] T006 [P] Add DbSet properties to ApplicationDbContext in `backend/src/WahadiniCryptoQuest.DAL/Context/ApplicationDbContext.cs` for AuditLogEntries, UserNotifications, PointAdjustments
- [X] T007 Create EF Core migration `20250101_AddAdminDashboardEntities` in `backend/src/WahadiniCryptoQuest.DAL/Migrations/` for AuditLogEntry, UserNotification, PointAdjustment tables with indexes
- [X] T008 Configure SuperAdmin authorization policy in `backend/src/WahadiniCryptoQuest.API/Program.cs` with RequireSuperAdmin requirement
- [X] T009 Create AuditLoggingMiddleware in `backend/src/WahadiniCryptoQuest.API/Middleware/AuditLoggingMiddleware.cs` to capture admin actions (POST, PUT, DELETE requests from /api/admin/*) with before/after values
- [X] T010 [P] Create IAuditLogService interface in `backend/src/WahadiniCryptoQuest.Core/Interfaces/Services/IAuditLogService.cs` with LogActionAsync, GetAuditLogsAsync methods
- [X] T011 [P] Create INotificationService interface in `backend/src/WahadiniCryptoQuest.Core/Interfaces/Services/INotificationService.cs` with SendEmailAsync, CreateInAppNotificationAsync methods
- [X] T012 Implement AuditLogService in `backend/src/WahadiniCryptoQuest.Service/Admin/AuditLogService.cs` with repository pattern and JSONB before/after serialization
- [X] T013 [P] Implement EmailNotificationService in `backend/src/WahadiniCryptoQuest.Service/Notifications/EmailNotificationService.cs` using MailKit with task review templates
- [X] T014 [P] Implement InAppNotificationService in `backend/src/WahadiniCryptoQuest.Service/Notifications/InAppNotificationService.cs` with UserNotification creation and retrieval
- [X] T015 Create AdminController skeleton in `backend/src/WahadiniCryptoQuest.API/Controllers/AdminController.cs` with [Authorize(Roles = "Admin,SuperAdmin")] attribute and dependency injection for services
- [X] T016 Register admin services in `backend/src/WahadiniCryptoQuest.API/Extensions/DependencyInjection/ServiceExtensions.cs` AddAdminServices method (IAuditLogService, INotificationService, repositories)
- [X] T017 Apply database migration and seed initial SuperAdmin account with email `admin@wahadini.com` per quickstart.md

### Frontend Infrastructure

- [X] T018 [P] Create AdminLayout component in `frontend/src/layouts/AdminLayout.tsx` with sidebar navigation, role guard checking user.role === 'Admin' or 'SuperAdmin'
- [X] T019 [P] Create AdminSidebar component in `frontend/src/components/admin/AdminSidebar.tsx` with navigation links (Dashboard, Users, Tasks, Courses, Rewards, Analytics, Audit Log)
- [X] T020 [P] Create admin route configuration in `frontend/src/routes/admin.routes.tsx` with lazy-loaded pages (Dashboard, UserManagement, TaskReview, CourseManagement, DiscountCodes, Analytics, AuditLog)
- [X] T021 Update App.tsx in `frontend/src/App.tsx` to import and register admin routes under /admin/* path with RequireAdmin route guard
- [X] T022 [P] Create adminService.ts in `frontend/src/services/adminService.ts` with Axios client configured with JWT interceptors for /api/admin/* endpoints
- [X] T023 [P] Create admin TypeScript types in `frontend/src/types/admin.types.ts` for all DTOs (AdminStatsDto, UserSummaryDto, TaskSubmissionDto, etc.)
- [X] T024 Install frontend dependencies: `npm install recharts react-quill @tanstack/react-query zustand` in frontend directory

---

## Phase 2: User Story 1 - Platform Health Overview (P1) 🎯 MVP

**Story**: As an Admin, I want to see a high-level dashboard with critical platform metrics upon login so I can quickly assess platform health and identify areas requiring immediate attention.

**Goal**: Deliver a functional admin dashboard displaying Total Users, Active Subscribers, MRR, Pending Tasks, and trend charts.

**Estimated Effort**: 24-28 hours

**Independent Test**: Admin can login, navigate to /admin, see all KPI cards with accurate counts, view 30-day revenue/user growth charts, and refresh to see updated metrics.

### Backend - Domain Layer

- [X] T025 [P] [US1] Create AdminStatsDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Admin/AdminStatsDto.cs` with properties (TotalUsers, ActiveSubscribers, MonthlyRecurringRevenue, PendingTasks, RevenueTrend List<ChartPointDto>, UserGrowthTrend List<ChartPointDto>)
- [X] T026 [P] [US1] Create ChartPointDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Admin/ChartPointDto.cs` with Date and Value properties

### Backend - Application Layer

- [X] T027 [P] [US1] Create IAnalyticsService interface in `backend/src/WahadiniCryptoQuest.Core/Interfaces/Services/IAnalyticsService.cs` with GetDashboardStatsAsync method
- [X] T028 [US1] Create GetAdminStatsQuery in `backend/src/WahadiniCryptoQuest.Service/Queries/Admin/GetAdminStatsQuery.cs` as MediatR IRequest<AdminStatsDto>
- [X] T029 [US1] Create GetAdminStatsQueryHandler in `backend/src/WahadiniCryptoQuest.Service/Handlers/Admin/GetAdminStatsQueryHandler.cs` implementing IRequestHandler with EF Core queries for aggregations (COUNT, SUM, GROUP BY date for trends over 30 days)
- [X] T030 [US1] Implement AnalyticsService in `backend/src/WahadiniCryptoQuest.Service/Services/AnalyticsService.cs` orchestrating MediatR calls, registered in DI container
- [X] T031 [P] [US1] Create GetAdminStatsValidator in `backend/src/WahadiniCryptoQuest.Service/Validators/Admin/GetAdminStatsValidator.cs` using FluentValidation (placeholder for consistency)

### Backend - Infrastructure Layer

- [X] T032 [P] [US1] Add database indexes in migration for optimized analytics queries: Users(CreatedAt), Subscriptions(Status, CreatedAt), TaskSubmissions(Status)

### Backend - Presentation Layer

- [X] T033 [US1] Implement GET /api/admin/stats endpoint in AdminController in `backend/src/WahadiniCryptoQuest.API/Controllers/Admin/AdminController.cs` with [Authorize(Roles = "Admin,SuperAdmin")], calling AnalyticsService.GetDashboardStatsAsync(), returns AdminStatsDto

### Frontend - Components

- [X] T034 [P] [US1] Create KPICard component in `frontend/src/components/admin/KPICard.tsx` accepting title, value, icon, trend (positive/negative/neutral) props with Tailwind styling and loading states
- [X] T035 [P] [US1] Create TrendChart component in `frontend/src/components/admin/TrendChart.tsx` wrapping Recharts LineChart with responsive container, custom tooltips, date formatting on X-axis
- [X] T036 [US1] Update Dashboard page in `frontend/src/pages/admin/AdminDashboard.tsx` with grid layout (4 KPI cards: Total Users, Active Subscribers, MRR, Pending Tasks), 2 trend charts (Revenue, User Growth) fetching data on mount
- [X] T037 [P] [US1] Create useAdminStats hook in `frontend/src/hooks/useAdminStats.ts` using React Query with staleTime 5min, gcTime 10min, automatic refetch on window focus

### Frontend - Services

- [X] T038 [US1] Implement getDashboardStats method in `frontend/src/services/adminService.ts` calling GET /api/admin/stats with error handling and type safety

### Testing (Optional - if TDD requested)

- [ ] T039 [P] [US1] Unit test for GetAdminStatsQueryHandler in `backend/tests/WahadiniCryptoQuest.Service.Tests/Admin/GetAdminStatsQueryHandlerTests.cs` mocking repository, asserting correct aggregation logic
- [ ] T040 [P] [US1] Integration test for GET /api/admin/stats endpoint in `backend/tests/WahadiniCryptoQuest.API.Tests/Controllers/AdminControllerTests.cs` using WebApplicationFactory, seeding test data, asserting response structure
- [ ] T041 [P] [US1] Component test for Dashboard page in `frontend/tests/pages/admin/Dashboard.test.tsx` using React Testing Library, mocking API, asserting KPI cards render

---

## Phase 3: User Story 2 - Task Review Workflow (P2)

**Story**: As an Admin, I need to efficiently review and grade pending user task submissions so users can progress through courses and earn their rewards without unnecessary delays.

**Goal**: Deliver task review interface with approve/reject actions, feedback input, and dual-channel notifications (email + in-app).

**Estimated Effort**: 20-24 hours

**Independent Test**: Admin can view pending submissions, approve with feedback (points awarded, notification sent), reject with feedback (user can resubmit), filter by date/course/status.

### Backend - Domain Layer

- [ ] T042 [P] [US2] Create PendingTaskDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Admin/PendingTaskDto.cs` with SubmissionId, UserId, Username, TaskId, TaskTitle, CourseName, SubmittedAt, ContentType, SubmissionData properties
- [ ] T043 [P] [US2] Create TaskReviewRequestDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Admin/TaskReviewRequestDto.cs` with Status (Approved/Rejected enum), Feedback (string) properties

### Backend - Application Layer

- [ ] T044 [US2] Create GetPendingTasksQuery in `backend/src/WahadiniCryptoQuest.Service/Admin/Queries/GetPendingTasksQuery.cs` with optional filters (DateFrom, DateTo, CourseId, Status) as MediatR IRequest<List<PendingTaskDto>>
- [ ] T045 [US2] Create GetPendingTasksQueryHandler in `backend/src/WahadiniCryptoQuest.Service/Admin/Queries/GetPendingTasksQueryHandler.cs` querying TaskSubmissions with Status="Pending", joining User and Task entities, applying filters, ordering by SubmittedAt DESC
- [ ] T046 [US2] Create ReviewTaskCommand in `backend/src/WahadiniCryptoQuest.Service/Admin/Commands/ReviewTaskCommand.cs` with SubmissionId, Status, Feedback, AdminUserId properties as MediatR IRequest<Unit>
- [ ] T047 [US2] Create ReviewTaskCommandHandler in `backend/src/WahadiniCryptoQuest.Service/Admin/Commands/ReviewTaskCommandHandler.cs` implementing logic: update TaskSubmission.Status, award points if Approved (User.PointsBalance += Task.PointReward), create notification (INotificationService), send email, log audit entry
- [ ] T048 [P] [US2] Create ReviewTaskValidator in `backend/src/WahadiniCryptoQuest.API/Validators/Admin/ReviewTaskValidator.cs` with FluentValidation rules: Status required (Approved/Rejected), Feedback required if Rejected (max 1000 chars)

### Backend - Presentation Layer

- [ ] T049 [US2] Implement GET /api/admin/tasks/pending endpoint in AdminController with query parameters (dateFrom, dateTo, courseId, status), calling IMediator.Send(GetPendingTasksQuery), returns List<PendingTaskDto>
- [ ] T050 [US2] Implement POST /api/admin/tasks/{submissionId}/review endpoint in AdminController with [FromBody] TaskReviewRequestDto, calling IMediator.Send(ReviewTaskCommand), returns 200 OK or validation errors- [ ] T050A [US2] Add RowVersion (timestamp) column to TaskSubmissions table in migration for optimistic concurrency control, implement version check in ReviewTaskCommandHandler returning 409 Conflict if TaskSubmission was modified since read (prevents concurrent review by multiple admins)
### Frontend - Components

- [ ] T051 [P] [US2] Create TaskCard component in `frontend/src/components/admin/TaskCard.tsx` displaying submission details (user, task, date, content preview), approve/reject buttons, feedback textarea with character counter
- [ ] T052 [US2] Create TaskReview page in `frontend/src/pages/admin/TaskReview.tsx` with filter bar (date range picker, course dropdown, status select), paginated task card list, empty state when no pending tasks
- [ ] T053 [P] [US2] Create useTaskReview hook in `frontend/src/hooks/useTaskReview.ts` with React Query mutations for approve/reject actions, optimistic updates, refetch pending tasks after review

### Frontend - Services

- [ ] T054 [US2] Implement getPendingTasks method in `frontend/src/services/adminService.ts` with query parameters (dateFrom, dateTo, courseId, status)
- [ ] T055 [US2] Implement reviewTask method in `frontend/src/services/adminService.ts` calling POST /api/admin/tasks/{id}/review with status and feedback

### Testing (Optional)

- [ ] T056 [P] [US2] Unit test for ReviewTaskCommandHandler verifying points awarded on approval, notification sent, audit logged
- [ ] T057 [P] [US2] Integration test for task review workflow (create submission → review → verify points updated and notification created)

---

## Phase 4: User Story 3 - User Account Management (P3)

**Story**: As an Admin, I want to manage user accounts (search, view details, update roles, ban/unban) so I can handle support issues, enforce community guidelines, and manage platform access.

**Goal**: Deliver user management interface with search, filtering, pagination, role updates (SuperAdmin only), and ban/unban actions.

**Estimated Effort**: 22-26 hours

**Independent Test**: Admin can search users by email/username, view paginated results, see user details, ban/unban users (with reason), SuperAdmin can update roles.

### Backend - Domain Layer

- [ ] T058 [P] [US3] Create PaginatedUsersDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Admin/PaginatedUsersDto.cs` with Items (List<UserSummaryDto>), TotalCount, PageNumber, PageSize, TotalPages properties
- [ ] T059 [P] [US3] Create UserSummaryDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Admin/UserSummaryDto.cs` with Id, Username, Email, Role, SubscriptionTier, AccountStatus, SignupDate, PointsBalance properties
- [ ] T060 [P] [US3] Create UserDetailDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Admin/UserDetailDto.cs` extending UserSummaryDto with EnrolledCourses, CompletedCourses, PurchaseHistory, RecentActivity lists
- [ ] T061 [P] [US3] Create UpdateUserRoleRequestDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Admin/UpdateUserRoleRequestDto.cs` with Role (string) property
- [ ] T062 [P] [US3] Create BanUserRequestDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Admin/BanUserRequestDto.cs` with Reason (string, max 500 chars) property

### Backend - Application Layer

- [ ] T063 [US3] Create GetUsersQuery in `backend/src/WahadiniCryptoQuest.Service/Admin/Queries/GetUsersQuery.cs` with PageNumber, PageSize, Search, Role, SubscriptionTier, AccountStatus, SortBy, SortOrder parameters
- [ ] T064 [US3] Create GetUsersQueryHandler in `backend/src/WahadiniCryptoQuest.Service/Admin/Queries/GetUsersQueryHandler.cs` implementing pagination, search (on Username/Email with Contains), filtering, sorting with EF Core IQueryable
- [ ] T065 [US3] Create GetUserDetailQuery in `backend/src/WahadiniCryptoQuest.Service/Admin/Queries/GetUserDetailQuery.cs` with UserId parameter
- [ ] T066 [US3] Create GetUserDetailQueryHandler in `backend/src/WahadiniCryptoQuest.Service/Admin/Queries/GetUserDetailQueryHandler.cs` loading user with Include for Enrollments, CompletedCourses, Transactions, mapping to UserDetailDto
- [ ] T067 [US3] Create UpdateUserRoleCommand in `backend/src/WahadiniCryptoQuest.Service/Admin/Commands/UpdateUserRoleCommand.cs` with UserId, NewRole, AdminUserId properties (SuperAdmin only)
- [ ] T068 [US3] Create UpdateUserRoleCommandHandler implementing logic: validate role (User/Admin/SuperAdmin), check requester is SuperAdmin, prevent self-demotion (requester cannot reduce their own role - UserId != AdminUserId when downgrading), prevent creating multiple SuperAdmins without approval, update User.Role, log audit entry
- [ ] T069 [US3] Create BanUserCommand in `backend/src/WahadiniCryptoQuest.Service/Admin/Commands/BanUserCommand.cs` with UserId, Reason, AdminUserId properties
- [ ] T070 [US3] Create BanUserCommandHandler implementing logic: prevent banning Admin (unless requester is SuperAdmin), set User.IsBanned=true, BanReason, BannedAt, BannedBy, log audit entry
- [ ] T071 [US3] Create UnbanUserCommand in `backend/src/WahadiniCryptoQuest.Service/Admin/Commands/UnbanUserCommand.cs` with UserId, AdminUserId
- [ ] T072 [US3] Create UnbanUserCommandHandler implementing logic: set User.IsBanned=false, clear BanReason/BannedAt/BannedBy, log audit entry
- [ ] T073 [P] [US3] Create UpdateUserRoleValidator with rule: Role must be in ["User", "Admin", "SuperAdmin"]
- [ ] T074 [P] [US3] Create BanUserValidator with rule: Reason required (max 500 chars)

### Backend - Presentation Layer

- [ ] T075 [US3] Implement GET /api/admin/users endpoint in AdminController with query parameters (pageNumber, pageSize, search, role, subscriptionTier, accountStatus, sortBy, sortOrder), returns PaginatedUsersDto
- [ ] T076 [US3] Implement GET /api/admin/users/{id} endpoint returning UserDetailDto
- [ ] T077 [US3] Implement PUT /api/admin/users/{id}/role endpoint with [Authorize(Policy = "RequireSuperAdmin")], [FromBody] UpdateUserRoleRequestDto, returns 200 OK or 403 Forbidden
- [ ] T078 [US3] Implement POST /api/admin/users/{id}/ban endpoint with [FromBody] BanUserRequestDto, returns 200 OK or 409 Conflict if already banned
- [ ] T079 [US3] Implement POST /api/admin/users/{id}/unban endpoint, returns 200 OK

### Frontend - Components

- [ ] T080 [P] [US3] Create UserTable component in `frontend/src/components/admin/UserTable.tsx` with columns (Username, Email, Role, Subscription, Status, Signup Date, Points), sortable headers, row actions (View Details, Ban/Unban)
- [ ] T081 [P] [US3] Create UserSearchBar component in `frontend/src/components/admin/UserSearchBar.tsx` with search input (debounced), filter dropdowns (Role, Subscription, Status), clear filters button
- [ ] T082 [P] [US3] Create UserDetailModal component in `frontend/src/components/admin/UserDetailModal.tsx` displaying full profile, enrollment history, purchase records, recent activity, actions (Update Role - SuperAdmin only, Ban/Unban)
- [ ] T083 [US3] Create UserManagement page in `frontend/src/pages/admin/UserManagement.tsx` with UserSearchBar, UserTable, pagination controls, UserDetailModal trigger
- [ ] T084 [P] [US3] Create useUserManagement hook in `frontend/src/hooks/useUserManagement.ts` with React Query for users list, user detail, mutations (updateRole, ban, unban) with optimistic updates

### Frontend - Services

- [ ] T085 [US3] Implement getUsers method in adminService.ts with pagination and filter parameters
- [ ] T086 [US3] Implement getUserDetail method in adminService.ts
- [ ] T087 [US3] Implement updateUserRole method in adminService.ts (PUT /api/admin/users/{id}/role)
- [ ] T088 [US3] Implement banUser method in adminService.ts (POST /api/admin/users/{id}/ban)
- [ ] T089 [US3] Implement unbanUser method in adminService.ts (POST /api/admin/users/{id}/unban)

### Testing (Optional)

- [ ] T090 [P] [US3] Integration test verifying SuperAdmin can update roles but Admin cannot
- [ ] T091 [P] [US3] Integration test verifying Admin cannot ban another Admin, but SuperAdmin can

---

## Phase 5: User Story 4 - Course Content Management (P4)

**Story**: As an Admin, I want to create, edit, and publish courses and lessons through a visual interface so I can manage educational content without directly manipulating the database.

**Goal**: Deliver course CMS with rich text editor, lesson management, drag-and-drop reordering, and publish/unpublish toggle.

**Estimated Effort**: 28-32 hours

**Independent Test**: Admin can create course with metadata, add lessons with YouTube URLs and rich text descriptions, reorder lessons via drag-and-drop, publish course to make it visible to users.

### Backend - Domain Layer

- [ ] T092 [P] [US4] Create CourseFormDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Admin/CourseFormDto.cs` with Title, Description (HTML), Category, ThumbnailUrl, Difficulty, IsPublished properties
- [ ] T093 [P] [US4] Create LessonFormDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Admin/LessonFormDto.cs` with Title, Description (HTML), VideoUrl, Duration, PointReward, Order properties
- [ ] T094 [P] [US4] Create CourseListDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Admin/CourseListDto.cs` with Id, Title, Category, Difficulty, IsPublished, CreatedAt, TotalLessons, EnrollmentCount

### Backend - Application Layer

- [ ] T095 [US4] Create GetCoursesQuery in `backend/src/WahadiniCryptoQuest.Service/Admin/Queries/GetCoursesQuery.cs` with PageNumber, PageSize, Category, IsPublished filters
- [ ] T096 [US4] Create GetCoursesQueryHandler querying Courses with Include(Lessons) for lesson count, Include(Enrollments) for enrollment count, returning paginated CourseListDto
- [ ] T097 [US4] Create CreateCourseCommand in `backend/src/WahadiniCryptoQuest.Service/Admin/Commands/CreateCourseCommand.cs` with CourseFormDto properties, AdminUserId
- [ ] T098 [US4] Create CreateCourseCommandHandler validating thumbnail URL format, creating Course entity with factory method, saving to repository, logging audit entry, returning CourseId
- [ ] T099 [US4] Create UpdateCourseCommand in `backend/src/WahadiniCryptoQuest.Service/Admin/Commands/UpdateCourseCommand.cs` with CourseId, CourseFormDto, AdminUserId
- [ ] T100 [US4] Create UpdateCourseCommandHandler loading existing course, updating properties, logging audit entry with before/after values
- [ ] T101 [US4] Create DeleteCourseCommand (soft delete) in `backend/src/WahadiniCryptoQuest.Service/Admin/Commands/DeleteCourseCommand.cs` with CourseId, AdminUserId
- [ ] T102 [US4] Create DeleteCourseCommandHandler setting Course.IsDeleted=true, preserving data for enrolled users, logging audit entry
- [ ] T103 [US4] Create AddLessonCommand in `backend/src/WahadiniCryptoQuest.Service/Admin/Commands/AddLessonCommand.cs` with CourseId, LessonFormDto, AdminUserId
- [ ] T104 [US4] Create AddLessonCommandHandler validating YouTube URL format (regex: youtube.com/watch or youtu.be), creating Lesson entity, setting Order to max+1, saving, logging audit
- [ ] T104A [P] [US4] Install HtmlSanitizer NuGet package and integrate sanitization in CourseFormValidator and LessonFormValidator to prevent XSS attacks in rich text Description fields (whitelist: p, br, strong, em, ul, ol, li, a[href])
- [ ] T105 [US4] Create UpdateLessonCommand in `backend/src/WahadiniCryptoQuest.Service/Admin/Commands/UpdateLessonCommand.cs` with LessonId, LessonFormDto, AdminUserId
- [ ] T106 [US4] Create UpdateLessonCommandHandler updating lesson properties, logging audit
- [ ] T107 [US4] Create ReorderLessonsCommand in `backend/src/WahadiniCryptoQuest.Service/Admin/Commands/ReorderLessonsCommand.cs` with CourseId, LessonIds (ordered array), AdminUserId
- [ ] T108 [US4] Create ReorderLessonsCommandHandler updating Lesson.Order for each lesson based on array index, logging audit
- [ ] T109 [P] [US4] Create CourseFormValidator with rules: Title required (max 200), Description required, Category required, ThumbnailUrl format validation, Difficulty in [Beginner, Intermediate, Advanced]
- [ ] T110 [P] [US4] Create LessonFormValidator with rules: Title required (max 200), VideoUrl required and YouTube URL format, Duration > 0, PointReward >= 0

### Backend - Presentation Layer

- [ ] T111 [US4] Implement GET /api/admin/courses endpoint with pagination/filters, returns List<CourseListDto>
- [ ] T112 [US4] Implement POST /api/admin/courses endpoint with [FromBody] CourseFormDto, returns CreatedAtRoute with CourseId
- [ ] T113 [US4] Implement PUT /api/admin/courses/{id} endpoint with [FromBody] CourseFormDto, returns 200 OK or 404 Not Found
- [ ] T114 [US4] Implement DELETE /api/admin/courses/{id} endpoint (soft delete), returns 204 No Content
- [ ] T115 [US4] Implement POST /api/admin/courses/{courseId}/lessons endpoint with [FromBody] LessonFormDto, returns CreatedAtRoute with LessonId
- [ ] T116 [US4] Implement PUT /api/admin/courses/{courseId}/lessons/{lessonId} endpoint with [FromBody] LessonFormDto
- [ ] T117 [US4] Implement PUT /api/admin/courses/{courseId}/lessons/reorder endpoint with [FromBody] List<Guid> lessonIds, returns 200 OK

### Frontend - Components

- [ ] T118 [P] [US4] Create RichTextEditor component in `frontend/src/components/admin/RichTextEditor.tsx` wrapping React Quill with custom toolbar (bold, italic, lists, links), HTML sanitization on save
- [ ] T119 [P] [US4] Create CourseTable component in `frontend/src/components/admin/CourseTable.tsx` with columns (Title, Category, Difficulty, Published, Lessons, Enrollments), row actions (Edit, Delete, Publish/Unpublish)
- [ ] T120 [US4] Create CourseEditor page in `frontend/src/pages/admin/CourseEditor.tsx` with form fields (Title input, RichTextEditor for description, Category/Difficulty dropdowns, Thumbnail URL input, IsPublished toggle), lesson list with drag-and-drop (react-beautiful-dnd or @dnd-kit), Add Lesson button
- [ ] T121 [P] [US4] Create LessonForm component in `frontend/src/components/admin/LessonForm.tsx` with Title, Description (RichTextEditor), YouTube URL input with validation preview, Duration (number), PointReward (number)
- [ ] T122 [US4] Create CourseManagement page in `frontend/src/pages/admin/CourseManagement.tsx` with CourseTable, Create New Course button, search/filter bar
- [ ] T123 [P] [US4] Create useCourseManagement hook in `frontend/src/hooks/useCourseManagement.ts` with React Query for courses list, mutations (create, update, delete, addLesson, updateLesson, reorderLessons)

### Frontend - Services

- [ ] T124 [US4] Implement getCourses, createCourse, updateCourse, deleteCourse methods in adminService.ts
- [ ] T125 [US4] Implement addLesson, updateLesson, reorderLessons methods in adminService.ts

### Frontend - Dependencies

- [ ] T126 Install react-quill and @dnd-kit/core @dnd-kit/sortable for rich text and drag-and-drop: `npm install react-quill @dnd-kit/core @dnd-kit/sortable`

### Testing (Optional)

- [ ] T127 [P] [US4] Integration test verifying published course appears in user-facing catalog, unpublished does not
- [ ] T128 [P] [US4] Component test for CourseEditor verifying lesson reordering updates Order correctly

---

## Phase 6: User Story 5 - Reward System Management (P5)

**Story**: As an Admin, I want to manage the platform reward economy (create discount codes, view redemption logs, manually adjust user points) so I can run marketing campaigns and resolve customer support issues.

**Goal**: Deliver discount code management and manual point adjustment tools.

**Estimated Effort**: 16-20 hours

**Independent Test**: Admin can create discount codes with expiration/usage limits, view redemption history, manually adjust user points with reason logging.

### Backend - Domain Layer

- [ ] T129 [P] [US5] Create DiscountCodeDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Admin/DiscountCodeDto.cs` with Id, Code, DiscountType, DiscountValue, ExpirationDate, UsageLimit, UsageCount, Status, CreatedAt properties
- [ ] T130 [P] [US5] Create CreateDiscountCodeDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Admin/CreateDiscountCodeDto.cs` with Code, DiscountType (Percentage/FixedAmount enum), DiscountValue, ExpirationDate, UsageLimit
- [ ] T131 [P] [US5] Create RedemptionLogDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Admin/RedemptionLogDto.cs` with UserId, Username, Code, RedeemedAt, DiscountAmount
- [ ] T132 [P] [US5] Create AdjustPointsRequestDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Admin/AdjustPointsRequestDto.cs` with AdjustmentAmount (int, can be positive/negative), Reason (string, required)

### Backend - Application Layer

- [ ] T133 [US5] Create GetDiscountCodesQuery in `backend/src/WahadiniCryptoQuest.Service/Admin/Queries/GetDiscountCodesQuery.cs` with Status filter (Active, Expired, FullyRedeemed)
- [ ] T134 [US5] Create GetDiscountCodesQueryHandler querying DiscountCodes, calculating Status based on ExpirationDate and UsageCount vs UsageLimit, returning List<DiscountCodeDto>
- [ ] T135 [US5] Create CreateDiscountCodeCommand in `backend/src/WahadiniCryptoQuest.Service/Admin/Commands/CreateDiscountCodeCommand.cs` with CreateDiscountCodeDto properties, AdminUserId
- [ ] T136 [US5] Create CreateDiscountCodeCommandHandler validating Code uniqueness, ExpirationDate > Now, DiscountValue rules (0-100 for Percentage, >0 for FixedAmount), creating DiscountCode entity, logging audit
- [ ] T137 [US5] Create GetRedemptionsQuery in `backend/src/WahadiniCryptoQuest.Service/Admin/Queries/GetRedemptionsQuery.cs` with Code, DateFrom, DateTo filters
- [ ] T138 [US5] Create GetRedemptionsQueryHandler querying Redemptions table with joins to Users, returning List<RedemptionLogDto>
- [ ] T139 [US5] Create AdjustPointsCommand in `backend/src/WahadiniCryptoQuest.Service/Admin/Commands/AdjustPointsCommand.cs` with UserId, AdjustmentAmount, Reason, AdminUserId
- [ ] T140 [US5] Create AdjustPointsCommandHandler loading User, validating final balance >= 0, updating PointsBalance, creating PointAdjustment entity with PreviousBalance/NewBalance, logging audit
- [ ] T141 [P] [US5] Create CreateDiscountCodeValidator with rules: Code 6-20 alphanumeric, DiscountType required, DiscountValue 0-100 if Percentage or >0 if FixedAmount, ExpirationDate > Now, UsageLimit >= 0
- [ ] T142 [P] [US5] Create AdjustPointsValidator with rules: Reason required (max 500 chars), AdjustmentAmount != 0

### Backend - Presentation Layer

- [ ] T143 [US5] Implement GET /api/admin/discounts endpoint with status filter, returns List<DiscountCodeDto>
- [ ] T144 [US5] Implement POST /api/admin/discounts endpoint with [FromBody] CreateDiscountCodeDto, returns 201 Created or 409 Conflict if code exists
- [ ] T145 [US5] Implement GET /api/admin/discounts/{code}/redemptions endpoint with date range filters, returns List<RedemptionLogDto>
- [ ] T146 [US5] Implement POST /api/admin/users/{id}/points/adjust endpoint with [FromBody] AdjustPointsRequestDto, returns 200 OK with updated balance or 400 if resulting balance would be negative

### Frontend - Components

- [ ] T147 [P] [US5] Create DiscountCodeTable component in `frontend/src/components/admin/DiscountCodeTable.tsx` with columns (Code, Type, Value, Expiration, Usage, Status), row actions (View Redemptions)
- [ ] T148 [P] [US5] Create DiscountCodeForm component in `frontend/src/components/admin/DiscountCodeForm.tsx` with Code input, DiscountType radio (Percentage/FixedAmount), Value number input with dynamic validation, Expiration date picker, UsageLimit number input (0 = unlimited)
- [ ] T149 [P] [US5] Create PointAdjustmentModal component in `frontend/src/components/admin/PointAdjustmentModal.tsx` with UserId prop, current balance display, AdjustmentAmount input (supports negative), Reason textarea, preview of new balance
- [ ] T150 [US5] Create DiscountCodes page in `frontend/src/pages/admin/DiscountCodes.tsx` with DiscountCodeTable, Create New Code button triggering DiscountCodeForm modal
- [ ] T151 [P] [US5] Create useDiscountCodes hook in `frontend/src/hooks/useDiscountCodes.ts` with React Query for discount list, redemptions, create mutation

### Frontend - Services

- [ ] T152 [US5] Implement getDiscountCodes, createDiscountCode, getRedemptions methods in adminService.ts
- [ ] T153 [US5] Implement adjustUserPoints method in adminService.ts (POST /api/admin/users/{id}/points/adjust)

### Frontend - Integration

- [ ] T154 [US5] Add Point Adjustment button to UserDetailModal (from Phase 4) triggering PointAdjustmentModal

### Testing (Optional)

- [ ] T155 [P] [US5] Integration test verifying discount code cannot be created with duplicate code (409 Conflict)
- [ ] T156 [P] [US5] Integration test verifying point adjustment prevents negative balance

---

## Phase 7: User Story 6 - Analytics and Insights (P6)

**Story**: As an Admin, I want to analyze platform performance through visual charts and reports (revenue trends, user retention, course popularity, engagement metrics) so I can make data-driven decisions.

**Goal**: Deliver advanced analytics dashboard with date range selection, multiple chart types, and course popularity rankings.

**Estimated Effort**: 18-22 hours

**Independent Test**: Admin can select date ranges, view revenue/user/engagement charts, see course rankings by enrollment/completion/rating.

### Backend - Application Layer

- [ ] T157 [P] [US6] Create GetAnalyticsQuery in `backend/src/WahadiniCryptoQuest.Service/Admin/Queries/GetAnalyticsQuery.cs` with DateFrom, DateTo, MetricType (Revenue, UserGrowth, Engagement) parameters
- [ ] T158 [US6] Create GetAnalyticsQueryHandler implementing time-series queries with GROUP BY date, calculating daily/weekly/monthly aggregations based on date range
- [ ] T159 [P] [US6] Create GetCoursePopularityQuery in `backend/src/WahadiniCryptoQuest.Service/Admin/Queries/GetCoursePopularityQuery.cs` returning courses ranked by enrollments, completion rate, average rating
- [ ] T160 [US6] Create GetCoursePopularityQueryHandler with complex query joining Courses, Enrollments, UserProgress, Reviews, calculating metrics

### Backend - Infrastructure Layer

- [ ] T161 [US6] Create materialized view `mv_daily_analytics` in migration for pre-aggregated daily metrics (revenue, new users, active users) to optimize query performance for 12-month retention

### Backend - Presentation Layer

- [ ] T162 [US6] Implement GET /api/admin/analytics endpoint with dateFrom, dateTo, metricType query params, returns List<ChartPointDto>
- [ ] T163 [US6] Implement GET /api/admin/analytics/courses/popularity endpoint, returns List<CoursePopularityDto>

### Frontend - Components

- [ ] T164 [P] [US6] Create DateRangePicker component in `frontend/src/components/admin/DateRangePicker.tsx` with preset ranges (Last 7 days, Last 30 days, Last 90 days, Last 12 months, Custom)
- [ ] T165 [P] [US6] Create MultiMetricChart component in `frontend/src/components/admin/MultiMetricChart.tsx` supporting line, bar, area chart types with Recharts, togglable metrics
- [ ] T166 [P] [US6] Create CoursePopularityTable component in `frontend/src/components/admin/CoursePopularityTable.tsx` with columns (Course, Enrollments, Completion Rate %, Avg Rating), sortable
- [ ] T167 [US6] Create Analytics page in `frontend/src/pages/admin/Analytics.tsx` with DateRangePicker, tab navigation (Revenue, Users, Engagement, Courses), corresponding charts, export CSV button
- [ ] T168 [P] [US6] Create useAnalytics hook in `frontend/src/hooks/useAnalytics.ts` with React Query for analytics data, course popularity, CSV export function

### Frontend - Services

- [ ] T169 [US6] Implement getAnalytics, getCoursePopularity, exportAnalyticsCSV methods in adminService.ts

### Testing (Optional)

- [ ] T170 [P] [US6] Integration test verifying analytics data aggregates correctly for different date ranges

---

## Phase 8: User Story 7 - Audit Log and Accountability (P7)

**Story**: As an Admin, I want to see a comprehensive log of all administrative actions taken on the platform so I can maintain accountability, investigate issues, and comply with security best practices.

**Goal**: Deliver audit log viewer with filtering, search, detail view, and CSV export.

**Estimated Effort**: 14-18 hours

**Independent Test**: Admin performs actions (user ban, course edit, etc.), sees entries in audit log with timestamp/admin/action/resource, filters by admin/action/date, exports to CSV.

### Backend - Application Layer

- [ ] T171 [US7] Create GetAuditLogsQuery in `backend/src/WahadiniCryptoQuest.Service/Admin/Queries/GetAuditLogsQuery.cs` with PageNumber, PageSize, AdminUserId, ActionType, ResourceType, DateFrom, DateTo filters
- [ ] T172 [US7] Create GetAuditLogsQueryHandler querying AuditLogEntries with filters, joining Users for AdminUsername, ordering by Timestamp DESC, returning paginated results
- [ ] T173 [US7] Create ExportAuditLogQuery in `backend/src/WahadiniCryptoQuest.Service/Admin/Queries/ExportAuditLogQuery.cs` with same filters as GetAuditLogsQuery
- [ ] T174 [US7] Create ExportAuditLogQueryHandler generating CSV string with headers (Timestamp, Admin, Action, Resource Type, Resource ID, Before, After, IP), streaming large datasets

### Backend - Presentation Layer

- [ ] T175 [US7] Implement GET /api/admin/audit-log endpoint with pagination and all filter parameters, returns PaginatedAuditLogDto
- [ ] T176 [US7] Implement GET /api/admin/audit-log/export endpoint with filter parameters, returns CSV file with Content-Disposition attachment header

### Frontend - Components

- [ ] T177 [P] [US7] Create AuditLogTable component in `frontend/src/components/admin/AuditLogTable.tsx` with columns (Timestamp, Admin, Action, Resource, Details expandable), filter dropdowns (Admin, Action Type, Resource Type)
- [ ] T178 [P] [US7] Create AuditLogDetailModal component in `frontend/src/components/admin/AuditLogDetailModal.tsx` displaying full entry with JSON-formatted Before/After values with diff highlighting
- [ ] T179 [US7] Create AuditLog page in `frontend/src/pages/admin/AuditLog.tsx` with filter bar, DateRangePicker, AuditLogTable, pagination, Export CSV button
- [ ] T180 [P] [US7] Create useAuditLog hook in `frontend/src/hooks/useAuditLog.ts` with React Query for audit logs, filter state management, CSV export function

### Frontend - Services

- [ ] T181 [US7] Implement getAuditLogs, exportAuditLog methods in adminService.ts

### Testing (Optional)

- [ ] T182 [P] [US7] Integration test verifying all admin actions trigger audit log entries
- [ ] T183 [P] [US7] Integration test verifying CSV export contains expected columns and data

---

## Phase 9: Polish & Cross-Cutting Concerns

**Goal**: Finalize production-ready implementation with performance optimization, security hardening, error handling, and documentation.

**Estimated Effort**: 16-20 hours

### Performance Optimization

- [ ] T184 [P] Add response caching middleware in `backend/src/WahadiniCryptoQuest.API/Middleware/CachingMiddleware.cs` for GET /api/admin/stats with 5-minute cache duration
- [ ] T185 [P] Implement query pagination optimization with keyset pagination for large datasets (users, audit logs) to replace offset pagination
- [ ] T186 [P] Add database query logging in development mode to identify N+1 queries and missing indexes
- [ ] T187 Create composite indexes in migration for common filter combinations: (AdminUserId, ActionType, Timestamp) on AuditLogEntries, (IsBanned, Role, CreatedAt) on Users

### Security Hardening

- [ ] T188 Implement rate limiting on admin endpoints using AspNetCoreRateLimit package: 100 requests/minute per admin user in `backend/src/WahadiniCryptoQuest.API/Extensions/RateLimitingExtensions.cs`
- [ ] T189 [P] Implement CSRF protection for state-changing admin endpoints using AntiForgery tokens (XSS sanitization moved to T104A in Phase 5)
- [ ] T190 [P] Implement CSRF protection for state-changing admin endpoints using AntiForgery tokens
- [ ] T191 Add IP address validation and geo-blocking configuration option in appsettings.json for admin access restriction

### Error Handling & Logging

- [ ] T192 [P] Create GlobalExceptionHandler in `backend/src/WahadiniCryptoQuest.API/Middleware/GlobalExceptionHandler.cs` catching unhandled exceptions, logging with Serilog, returning structured error responses
- [ ] T193 [P] Implement structured logging for all admin actions using Serilog with context enrichment (AdminUserId, IPAddress, ActionType) in AuditLoggingMiddleware
- [ ] T194 [P] Create ErrorBoundary component in `frontend/src/components/ErrorBoundary.tsx` catching React errors, displaying user-friendly message, logging to error tracking service
- [ ] T195 Add toast notification system in `frontend/src/components/admin/Toast.tsx` for success/error feedback on all admin actions

### Documentation

- [ ] T196 [P] Generate Swagger/OpenAPI documentation for all admin endpoints with XML comments in controllers, example request/response bodies
- [ ] T197 [P] Create admin user guide in `docs/user-guides/admin-dashboard-guide.md` with screenshots, common workflows, troubleshooting
- [ ] T198 Update API documentation in `docs/api/admin-endpoints.md` with authentication requirements, rate limits, error codes

### Accessibility

- [ ] T199 [P] Add ARIA labels and roles to all interactive admin components (tables, modals, forms) for screen reader compatibility
- [ ] T200 [P] Implement keyboard navigation with focus management for all admin workflows (Tab, Enter, Escape shortcuts)
- [ ] T201 Add skip navigation link in AdminLayout for keyboard users to jump to main content

### Testing & QA

- [ ] T202 Run Lighthouse accessibility audit on all admin pages, fix issues to achieve WCAG 2.1 AA compliance
- [ ] T203 [P] Run security scan with OWASP ZAP on admin endpoints, remediate identified vulnerabilities
- [ ] T204 [P] Performance test admin dashboard under load (10 concurrent admins) using k6 or Artillery, ensure <200ms p95 latency
- [ ] T205 [P] Cross-browser testing on Chrome, Firefox, Safari, Edge for all admin features
- [ ] T206 Execute end-to-end test suite covering all 7 user stories with Playwright

### Deployment Preparation

- [ ] T207 Create database migration rollback scripts for all new migrations in `backend/src/WahadiniCryptoQuest.DAL/Migrations/Rollback/`
- [ ] T208 Configure environment-specific settings in appsettings.Production.json (cache durations, rate limits, email templates)
- [ ] T209 Create Docker Compose configuration update for admin dashboard dependencies (Redis for caching, if needed)
- [ ] T210 Update CI/CD pipeline in `.github/workflows/deploy.yml` to run admin integration tests before deployment

---

## Dependencies & Execution Order

### Critical Path (Must Complete in Order)

1. **Phase 1** (Setup) → **Phase 2** (US1 - Dashboard) → **MVP Release**
2. Phase 2 → Phase 3 (US2 - Task Review)
3. Phase 2 → Phase 4 (US3 - User Management)
4. Phase 2 → Phase 5 (US4 - Course Management)
5. Phase 2 → Phase 6 (US5 - Rewards)
6. Phase 2 → Phase 7 (US6 - Analytics)
7. Phase 2 → Phase 8 (US7 - Audit Log)
8. Phases 3-8 → Phase 9 (Polish)

### User Story Independence

After Phase 2 (US1) is complete, **User Stories 2-7 are independent** and can be developed in parallel by different developers:

- **Developer 1**: US2 (Task Review) → US5 (Rewards)
- **Developer 2**: US3 (User Management) → US4 (Course Management)
- **Developer 3**: US6 (Analytics) → US7 (Audit Log)

### Parallel Execution Opportunities Within Phases

Tasks marked with `[P]` can be executed in parallel within the same phase. Examples:

**Phase 2 (US1 - Dashboard)** Parallel Groups:
- Group A: T025, T026 (DTOs)
- Group B: T027, T031 (Interfaces and validators)
- Group C: T034, T035, T037 (Frontend components)
- Group D: T039, T040, T041 (Tests)

**Phase 4 (US3 - User Management)** Parallel Groups:
- Group A: T058-T062 (All DTOs)
- Group B: T073, T074 (Validators)
- Group C: T080-T082 (UI Components)

---

## Suggested MVP Scope

**MVP = Phase 1 + Phase 2 (User Story 1: Platform Health Overview)**

**Rationale**: Delivers immediate value by providing admins with operational visibility into platform health. This is the foundation that all other admin features build upon.

**MVP Deliverables**:
- Functional admin login with role-based access
- Dashboard displaying 4 KPI cards (Total Users, Active Subscribers, MRR, Pending Tasks)
- 2 trend charts (Revenue and User Growth over 30 days)
- Audit logging infrastructure (actions logged, but no UI viewer yet)
- ~40-44 hours of development effort
- Can be deployed to production for immediate admin usage

**Post-MVP Releases** (Incremental):
- **Release 2**: Add User Story 2 (Task Review) - Most critical for user experience
- **Release 3**: Add User Story 3 (User Management) - Essential for support operations
- **Release 4**: Add User Story 4 (Course Management) - Enables content team independence
- **Release 5**: Add User Stories 5, 6, 7 (Rewards, Analytics, Audit Log) - Value-add features

---

## Format Validation Summary

✅ **All tasks follow strict checklist format**:
- Checkbox: `- [ ]` ✓
- Task ID: T001-T210 sequential ✓
- [P] marker: 82 parallelizable tasks identified ✓
- [Story] label: All user story tasks labeled (US1-US7) ✓
- Description: Clear action with file path ✓

✅ **Task Organization**:
- Phase 1: Setup (17 tasks)
- Phase 2: US1 - P1 (17 tasks) - **MVP**
- Phase 3: US2 - P2 (16 tasks)
- Phase 4: US3 - P3 (32 tasks)
- Phase 5: US4 - P4 (37 tasks)
- Phase 6: US5 - P5 (27 tasks)
- Phase 7: US6 - P6 (14 tasks)
- Phase 8: US7 - P7 (13 tasks)
- Phase 9: Polish (27 tasks)
- **Total**: 210 tasks

✅ **Testing**: Tests marked as optional per SpecKit guidelines (only included if TDD requested)

✅ **Dependencies**: Clear execution order defined with parallelization opportunities

---

**Ready for Execution**: Each task is specific enough for an LLM or developer to implement without additional context. All necessary file paths, validation rules, and implementation details are included.
