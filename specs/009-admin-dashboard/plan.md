# Implementation Plan: Admin Dashboard

**Branch**: `009-admin-dashboard` | **Date**: December 16, 2025 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/009-admin-dashboard/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

The Admin Dashboard feature provides comprehensive platform management capabilities for administrators and super-administrators of WahadiniCryptoQuest. This feature enables efficient oversight of user management, task review workflows, course content management, discount code creation, and detailed audit logging. The implementation follows clean architecture principles with role-based authorization (SuperAdmin > Admin > User) and dual-channel notifications (email + in-app). Key capabilities include real-time analytics with 12-month retention, task submission review with 30-second target turnaround, course CMS with rich text editing, manual point adjustments, and comprehensive audit trails for compliance.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: .NET 8 C# (backend), TypeScript 4.9+ with React 18 (frontend)  
**Primary Dependencies**: ASP.NET Core Web API 8.0, Entity Framework Core 8.0, React Router 7, React Query 5, MediatR, FluentValidation  
**Storage**: PostgreSQL 15+ with JSONB, time-based partitioning, EF Core migrations  
**Testing**: xUnit + Moq + FluentAssertions (backend), Vitest + React Testing Library (frontend), Playwright (E2E)  
**Target Platform**: Web application (SPA), Docker containerized backend, cross-browser frontend (Chrome, Firefox, Safari, Edge)
**Project Type**: Full-stack web application with RESTful API and React SPA  
**Performance Goals**: <3s dashboard load, <30s task review, <15min course creation, <2s analytics query response, 100 req/min rate limit per admin  
**Constraints**: 80%+ test coverage, WCAG 2.1 AA compliance, mobile-responsive design (Tailwind breakpoints), <200ms API p95 latency  
**Scale/Scope**: 5-10 concurrent admins, ~15,000 platform users, 50+ courses, 42+ pending tasks at peak, 1-year audit log retention (365 days)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Note**: No constitution.md file currently exists for WahadiniCryptoQuest (template found at `.specify/memory/constitution.md`). Proceeding with general clean architecture and testing best practices.

### Architectural Compliance

✅ **Clean Architecture Adherence**: Feature follows established 4-layer separation (Domain → Application → Infrastructure → Presentation)

✅ **CQRS Pattern**: Admin operations use MediatR commands/queries consistent with existing codebase

✅ **Repository Pattern**: AdminRepository follows existing IRepository<T> and Unit of Work patterns

✅ **Domain-Driven Design**: New entities (AuditLogEntry, UserNotification, PointAdjustment) use factory methods, private setters, and domain validation

### Testing Requirements

✅ **Test-First Development**: Comprehensive test coverage planned for all admin use cases

✅ **Unit Tests**: Service layer, domain validation, authorization policies

✅ **Integration Tests**: API endpoints, database interactions, email delivery

✅ **E2E Tests**: Critical admin workflows (user ban, task review, course creation)

### Security & Authorization

✅ **Role-Based Access Control**: SuperAdmin and Admin roles with policy-based authorization

✅ **Audit Logging**: All admin actions logged with before/after state, IP tracking

✅ **Input Validation**: FluentValidation for all admin commands with custom validators

### No Constitution Violations

No violations detected. Feature aligns with existing architectural patterns and does not introduce new paradigms requiring constitution amendments.

## Project Structure

### Documentation (this feature)

```text
specs/009-admin-dashboard/
├── spec.md                           # Feature specification (7 user stories, 38 FRs)
├── plan.md                           # This implementation plan
├── research.md                       # Technology decisions (8 decisions documented)
├── data-model.md                     # Domain entities and database schema
├── quickstart.md                     # Developer setup guide
├── checklists/
│   └── requirements.md               # Quality validation checklist
└── contracts/
    └── api-spec.md                   # REST API contracts (OpenAPI format)
```

### Source Code Structure

**Structure Decision**: Full-stack web application with separate backend and frontend directories following established WahadiniCryptoQuest architecture.

#### Backend (.NET 8 - Clean Architecture)

**New Files**:
```text
backend/src/
├── WahadiniCryptoQuest.Core/                     # Domain Layer
│   ├── Entities/
│   │   ├── AuditLogEntry.cs                      # NEW: Immutable audit trail
│   │   ├── UserNotification.cs                   # NEW: In-app notifications
│   │   └── PointAdjustment.cs                    # NEW: Manual point adjustments
│   ├── DTOs/Admin/
│   │   ├── AdminStatsDto.cs                      # NEW: Dashboard KPIs
│   │   ├── PaginatedUsersDto.cs                  # NEW: User management list
│   │   ├── UserDetailDto.cs                      # NEW: User detail view
│   │   ├── PendingTaskDto.cs                     # NEW: Task review queue
│   │   ├── TaskReviewRequestDto.cs               # NEW: Review submission
│   │   ├── CourseFormDto.cs                      # NEW: Course create/update
│   │   ├── DiscountCodeDto.cs                    # NEW: Discount management
│   │   └── AuditLogDto.cs                        # NEW: Audit log entries
│   └── Interfaces/
│       ├── IAuditLogService.cs                   # NEW: Audit logging abstraction
│       └── INotificationService.cs               # NEW: Notification delivery
│
├── WahadiniCryptoQuest.Service/                  # Application Layer
│   ├── Admin/
│   │   ├── Commands/
│   │   │   ├── UpdateUserRoleCommand.cs          # NEW: SuperAdmin role change
│   │   │   ├── BanUserCommand.cs                 # NEW: Ban user account
│   │   │   ├── UnbanUserCommand.cs               # NEW: Unban user
│   │   │   ├── ReviewTaskCommand.cs              # NEW: Approve/reject task
│   │   │   ├── CreateCourseCommand.cs            # NEW: Course creation
│   │   │   ├── UpdateCourseCommand.cs            # NEW: Course editing
│   │   │   ├── CreateDiscountCommand.cs          # NEW: Discount code
│   │   │   └── AdjustPointsCommand.cs            # NEW: Manual point adjustment
│   │   ├── Queries/
│   │   │   ├── GetAdminStatsQuery.cs             # NEW: Dashboard stats
│   │   │   ├── GetUsersQuery.cs                  # NEW: Paginated user list
│   │   │   ├── GetUserDetailQuery.cs             # NEW: User detail
│   │   │   ├── GetPendingTasksQuery.cs           # NEW: Task review queue
│   │   │   ├── GetCoursesQuery.cs                # NEW: Course management list
│   │   │   ├── GetDiscountCodesQuery.cs          # NEW: Discount list
│   │   │   └── GetAuditLogsQuery.cs              # NEW: Audit log with filters
│   │   └── AdminService.cs                       # NEW: Admin business logic orchestrator
│   └── Notifications/
│       ├── EmailNotificationService.cs           # NEW: MailKit email sender
│       └── InAppNotificationService.cs           # NEW: UserNotification persistence
│
├── WahadiniCryptoQuest.DAL/                      # Infrastructure Layer
│   ├── Repositories/
│   │   ├── AuditLogRepository.cs                 # NEW: Audit log data access
│   │   └── UserNotificationRepository.cs         # NEW: Notification data access
│   └── Migrations/
│       ├── 20250101_AddAuditLogEntities.cs       # NEW: Audit log table
│       ├── 20250101_AddUserNotifications.cs      # NEW: Notifications table
│       ├── 20250101_AddPointAdjustments.cs       # NEW: Point adjustments table
│       └── 20250101_ExtendDiscountCode.cs        # NEW: Discount extensions
│
└── WahadiniCryptoQuest.API/                      # Presentation Layer
    ├── Controllers/
    │   └── AdminController.cs                    # NEW: 20+ admin endpoints
    ├── Middleware/
    │   └── AuditLoggingMiddleware.cs             # NEW: Automatic audit capture
    ├── Validators/Admin/
    │   ├── UpdateUserRoleValidator.cs            # NEW: Role change validation
    │   ├── BanUserValidator.cs                   # NEW: Ban reason validation
    │   ├── ReviewTaskValidator.cs                # NEW: Task review validation
    │   ├── CreateCourseValidator.cs              # NEW: Course form validation
    │   └── CreateDiscountValidator.cs            # NEW: Discount code validation
    └── Policies/
        └── AdminPolicies.cs                      # NEW: SuperAdmin policy handler
```

**Modified Files**:
```text
backend/src/
├── WahadiniCryptoQuest.Core/
│   └── Entities/
│       ├── User.cs                               # MODIFIED: Add IsBanned, BanReason
│       └── DiscountCode.cs                       # MODIFIED: Add UsageLimit, UsageCount
│
├── WahadiniCryptoQuest.DAL/
│   └── Data/
│       └── ApplicationDbContext.cs               # MODIFIED: Add new DbSets
│
└── WahadiniCryptoQuest.API/
    ├── Program.cs                                # MODIFIED: Register admin services
    └── Extensions/DependencyInjection/
        └── ServiceExtensions.cs                  # MODIFIED: AddAdminServices()
```

#### Frontend (React 18 + TypeScript)

**New Files**:
```text
frontend/src/
├── pages/admin/
│   ├── Dashboard.tsx                             # NEW: Admin dashboard overview
│   ├── UserManagement.tsx                        # NEW: User list & management
│   ├── UserDetail.tsx                            # NEW: User detail view
│   ├── TaskReview.tsx                            # NEW: Task review queue
│   ├── CourseManagement.tsx                      # NEW: Course CMS
│   ├── CourseEditor.tsx                          # NEW: Course create/edit form
│   ├── DiscountCodes.tsx                         # NEW: Discount code management
│   ├── Analytics.tsx                             # NEW: Platform analytics
│   └── AuditLog.tsx                              # NEW: Audit log viewer
│
├── components/admin/
│   ├── AdminLayout.tsx                           # NEW: Admin page wrapper with sidebar
│   ├── AdminSidebar.tsx                          # NEW: Admin navigation sidebar
│   ├── KPICard.tsx                               # NEW: Dashboard metric card
│   ├── TrendChart.tsx                            # NEW: Recharts wrapper
│   ├── UserTable.tsx                             # NEW: Paginated user table
│   ├── TaskCard.tsx                              # NEW: Task submission card
│   ├── RichTextEditor.tsx                        # NEW: React Quill wrapper
│   └── DiscountCodeForm.tsx                      # NEW: Discount creation form
│
├── services/
│   ├── adminService.ts                           # NEW: Admin API client (20+ methods)
│   └── notificationService.ts                    # NEW: In-app notification polling
│
├── hooks/
│   ├── useAdminStats.ts                          # NEW: React Query hook for stats
│   ├── useUserManagement.ts                      # NEW: User CRUD operations
│   ├── useTaskReview.ts                          # NEW: Task approval logic
│   └── useAuditLog.ts                            # NEW: Audit log filtering
│
├── types/
│   └── admin.types.ts                            # NEW: TypeScript interfaces for DTOs
│
└── routes/
    └── admin.routes.tsx                          # NEW: Admin route definitions
```

**Modified Files**:
```text
frontend/src/
├── App.tsx                                       # MODIFIED: Add admin routes
└── routes/
    └── index.tsx                                 # MODIFIED: Import admin.routes
```

## Complexity Tracking

**No violations to justify** - This feature aligns with existing architecture patterns and introduces no additional complexity requiring constitution amendments.

**Complexity Category**: **VERY_COMPLEX**

**Justification**:
- **38 functional requirements** spanning 7 user stories
- **20+ API endpoints** with role-based authorization
- **4 new domain entities** with complex validation rules
- **Database migrations** affecting 5+ tables
- **Dual notification channels** (email + in-app)
- **12-month analytics** with materialized views
- **Comprehensive audit logging** with before/after state tracking
- **Rich text editing** with image management
- **Full CRUD** for courses, users, discounts
- **Frontend integration**: 9 new admin pages, 8 reusable components

**Estimated Effort**: 6-8 weeks (2 developers)
