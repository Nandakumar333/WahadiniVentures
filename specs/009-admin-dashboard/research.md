# Research & Decision Log: Admin Dashboard

**Feature**: 009-admin-dashboard  
**Date**: December 16, 2025  
**Status**: Complete

## Overview

This document consolidates research findings and architectural decisions for the WahadiniCryptoQuest Admin Dashboard feature. All clarifications from the specification have been resolved through established platform patterns.

## Technology Decisions

### 1. Admin Dashboard Integration

**Decision**: Integrate admin dashboard into main SPA with lazy loading

**Rationale**:
- Shares authentication context, components, and state management with main app
- Reduces code duplication and maintenance overhead
- Lazy loading prevents bloating the user bundle size
- Single deployment pipeline simplifies CI/CD

**Implementation Approach**:
- React.lazy() for route-level code splitting
- Separate chunk for admin routes (`/admin/*`)
- Conditional loading based on user role
- Estimated bundle impact: ~150KB (compressed) for admin features

**Alternatives Considered**:
- Separate admin SPA: Rejected due to code duplication and auth complexity
- Server-side rendering: Rejected due to unnecessary complexity for admin-only features

### 2. Super-Admin Role Implementation

**Decision**: Implement hierarchical role system with SuperAdmin > Admin > User

**Rationale**:
- Prevents admin lockout scenarios (critical operational requirement)
- Enables proper administrative hierarchy
- Supports future role expansion (ContentEditor, Support, etc.)
- Industry standard pattern for multi-admin systems

**Implementation Approach**:
- Extend ASP.NET Identity with custom roles table
- Role hierarchy: `SuperAdmin` (full privileges) > `Admin` (limited) > `User`
- Policy-based authorization: `[Authorize(Policy = "RequireSuperAdmin")]`
- At least one super-admin must exist (database seed requirement)

**Database Changes**:
```sql
-- UserRoles table (already exists in ASP.NET Identity)
-- Add custom claim for role hierarchy
ALTER TABLE AspNetRoles ADD COLUMN Hierarchy INTEGER DEFAULT 0;
-- SuperAdmin = 100, Admin = 50, User = 0
```

**Alternatives Considered**:
- Flat admin role: Rejected due to lockout risk
- Permission-based system: Deferred to future iteration for simplicity

### 3. Notification Delivery Mechanism

**Decision**: Dual-channel notifications (Email + In-App)

**Rationale**:
- Email ensures users receive notifications even when offline
- In-app banner provides immediate feedback when user is active
- Leverages existing MailKit integration
- Complies with user engagement best practices

**Implementation Approach**:
- Email: MailKit service with task review templates
- In-app: UserNotifications table with unread count badge
- Notification model: `{ UserId, Type, Message, IsRead, CreatedAt, RelatedEntityId }`
- React Query polling for unread count (30-second interval)

**Templates**:
- Task Approved: "✅ Your task '[TaskName]' has been approved! You earned [Points] points."
- Task Rejected: "❌ Your task '[TaskName]' needs revision. Feedback: [FeedbackText]"

**Alternatives Considered**:
- Push notifications: Deferred to mobile app phase
- WebSockets for real-time: Rejected as unnecessary for MVP (clarification confirmed)

### 4. Analytics Data Retention

**Decision**: 12-month rolling retention with monthly aggregation

**Rationale**:
- Supports year-over-year comparisons for business decision-making
- Balances storage cost with analytical value
- Aligns with typical SaaS retention policies
- PostgreSQL partitioning optimizes query performance

**Implementation Approach**:
- Time-based partitioning on `AnalyticsEvents` table (monthly partitions)
- Automatic partition creation via PostgreSQL extension
- Aggregated views for common queries (daily/weekly summaries)
- Archive strategy: Export to cold storage after 12 months (future enhancement)

**Query Optimization**:
```sql
-- Materialized view for dashboard stats (refresh every 5 minutes)
CREATE MATERIALIZED VIEW admin_dashboard_stats AS
SELECT 
    (SELECT COUNT(*) FROM Users WHERE IsDeleted = false) as total_users,
    (SELECT COUNT(*) FROM Users WHERE SubscriptionTier != 'Free') as active_subscribers,
    (SELECT SUM(Amount) FROM Transactions WHERE Date >= CURRENT_DATE - INTERVAL '30 days') as mrr,
    (SELECT COUNT(*) FROM TaskSubmissions WHERE Status = 'Pending') as pending_tasks;
```

**Alternatives Considered**:
- 24-month retention: Rejected due to storage costs exceeding value
- Real-time aggregation only: Rejected due to performance impact

### 5. Chart Visualization Library

**Decision**: Recharts for React dashboard

**Rationale**:
- Native React components (composable, declarative)
- Responsive and accessible out-of-the-box
- Smaller bundle size than Chart.js (45KB vs 200KB)
- Strong TypeScript support
- Active maintenance and community

**Chart Types**:
- Line Chart: Revenue trends, user growth
- Bar Chart: Course popularity, completion rates
- Pie Chart: Subscription distribution
- Area Chart: Engagement metrics over time

**Alternatives Considered**:
- Chart.js: Rejected due to larger bundle size
- D3.js: Rejected due to complexity overhead for simple charts
- Victory: Similar quality but less popular

### 6. Rich Text Editor

**Decision**: React Quill (Quill.js wrapper)

**Rationale**:
- Production-ready with comprehensive formatting features
- Lightweight (~75KB gzipped)
- Excellent mobile support
- Customizable toolbar for course descriptions
- Strong security (XSS protection)

**Configuration**:
```typescript
const modules = {
  toolbar: [
    [{ 'header': [1, 2, 3, false] }],
    ['bold', 'italic', 'underline', 'strike'],
    ['link', 'image', 'video'],
    [{ 'list': 'ordered'}, { 'list': 'bullet' }],
    ['clean']
  ]
};
```

**Alternatives Considered**:
- TinyMCE: Rejected due to licensing complexity and larger footprint
- TipTap: Considered but Quill has better React integration
- Draft.js: Rejected due to deprecation by Meta

### 7. Image Upload Strategy (MVP)

**Decision**: External URL input for MVP, S3 upload for future

**Rationale**:
- Speeds up MVP delivery (no upload infrastructure needed)
- Course creators can use existing CDNs (Imgur, Cloudinary)
- Reduces storage costs during early phase
- Clear upgrade path defined

**MVP Implementation**:
- `<input type="url">` for thumbnail URLs
- Client-side URL validation
- Server-side URL accessibility check
- Fallback placeholder image for broken URLs

**Future Enhancement Path**:
```csharp
// Phase 2: Direct upload
public class UploadController : ControllerBase
{
    [HttpPost("images")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        // S3 upload logic
        // Return CDN URL
    }
}
```

**Alternatives Considered**:
- Immediate S3 integration: Deferred to avoid scope creep
- Base64 encoding: Rejected due to database bloat

### 8. Pagination Strategy

**Decision**: Server-side pagination with configurable page size

**Rationale**:
- Prevents performance degradation with large datasets
- Reduces bandwidth and memory usage
- Supports 100,000+ records (spec requirement)
- Industry standard for admin interfaces

**Implementation**:
```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

**Default Page Sizes**:
- Users: 25 per page
- Tasks: 50 per page
- Audit Logs: 100 per page
- Discount Codes: 50 per page

**Alternatives Considered**:
- Infinite scroll: Rejected for admin context (harder to navigate)
- Client-side pagination: Rejected due to performance requirements

## Architecture Decisions

### Clean Architecture Layers

**Decision**: Follow established WahadiniCryptoQuest architecture

**Layer Breakdown**:

1. **Domain Layer (Core)**:
   - No new domain entities (reuses existing: User, Course, Lesson, Task, etc.)
   - New value objects: `AdminStats`, `AnalyticsDataPoint`

2. **Application Layer (Service)**:
   - `AdminService`: Orchestrates admin operations
   - `AnalyticsService`: Aggregates platform metrics
   - `AuditLogService`: Records admin actions
   - Commands: `BanUserCommand`, `ReviewTaskCommand`, `UpdateCourseCommand`
   - Queries: `GetDashboardStatsQuery`, `GetPendingTasksQuery`, `GetUsersQuery`

3. **Infrastructure Layer**:
   - Repository extensions for admin queries
   - Email templates for notifications
   - Analytics data aggregation queries

4. **Presentation Layer (API)**:
   - `AdminController`: RESTful endpoints
   - DTOs: Request/response models specific to admin operations
   - Validators: FluentValidation rules for admin inputs

### Frontend Architecture

**Component Structure**:
```
src/
├── components/
│   └── admin/
│       ├── Dashboard/
│       │   ├── StatsCard.tsx
│       │   ├── RevenueChart.tsx
│       │   └── UserGrowthChart.tsx
│       ├── Users/
│       │   ├── UserTable.tsx
│       │   ├── UserDetailModal.tsx
│       │   ├── UserFilters.tsx
│       │   └── RoleUpdateDialog.tsx
│       ├── Courses/
│       │   ├── CourseForm.tsx
│       │   ├── LessonEditor.tsx
│       │   └── LessonReorder.tsx
│       ├── Tasks/
│       │   ├── TaskReviewQueue.tsx
│       │   ├── SubmissionViewer.tsx
│       │   └── ReviewActions.tsx
│       └── Rewards/
│           ├── DiscountCodeForm.tsx
│           ├── DiscountCodeList.tsx
│           └── PointAdjustment.tsx
├── pages/
│   └── admin/
│       ├── AdminDashboard.tsx
│       ├── UserManagement.tsx
│       ├── CourseManagement.tsx
│       ├── TaskReview.tsx
│       └── RewardManagement.tsx
├── hooks/
│   └── admin/
│       ├── useAdminStats.ts
│       ├── useUserManagement.ts
│       ├── useTaskReview.ts
│       └── useCourseManagement.ts
└── services/
    └── admin/
        └── adminService.ts
```

### Security Considerations

**Authorization**:
- All admin endpoints: `[Authorize(Roles = "Admin,SuperAdmin")]`
- Super-admin only: `[Authorize(Policy = "RequireSuperAdmin")]`
- Admin-to-admin actions: Super-admin policy check

**Audit Logging**:
- Every admin action logged with: Timestamp, AdminUserId, Action, ResourceType, ResourceId, BeforeValue, AfterValue
- Immutable audit log (no updates/deletes)
- Retention: Indefinite (compliance requirement)

**Input Validation**:
- FluentValidation on all DTOs
- SQL injection protection (EF Core parameterization)
- XSS prevention (React automatic escaping + Quill sanitization)
- CSRF tokens for state-changing operations

### Performance Optimizations

**Database**:
- Indexes on: Users.Email, Users.Username, TaskSubmissions.Status, AuditLog.AdminUserId
- Materialized views for dashboard stats (5-minute refresh)
- Read replicas for analytics queries (future)

**Frontend**:
- React.lazy() for admin routes (~150KB lazy chunk)
- React Query caching (5-minute stale time for stats)
- Debounced search inputs (300ms delay)
- Virtual scrolling for large tables (react-window)

**API**:
- AsNoTracking() for read-only queries
- Pagination for all list endpoints
- Response compression (Gzip)
- API response caching headers

## Integration Points

### Existing Systems

1. **Authentication**: ASP.NET Identity (extends with SuperAdmin role)
2. **Email Service**: MailKit (adds task review templates)
3. **Database**: PostgreSQL (adds audit log table, analytics views)
4. **Authorization**: Policy-based (adds RequireSuperAdmin policy)

### New Dependencies

**Backend**:
- None (uses existing stack)

**Frontend**:
- `recharts`: ^2.10.0 (charts)
- `react-quill`: ^2.0.0 (rich text editor)
- `@tanstack/react-table`: ^8.11.0 (data tables)
- `react-beautiful-dnd`: ^13.1.1 (drag and drop)

## Risk Mitigation

### Identified Risks

1. **Admin Lockout**: Mitigated by SuperAdmin role requirement
2. **Performance Degradation**: Mitigated by pagination, indexes, materialized views
3. **Concurrent Task Reviews**: Mitigated by optimistic locking warning
4. **Data Leakage**: Mitigated by role-based authorization and audit logging

### Testing Strategy

**Unit Tests**:
- Service layer business logic (AdminService, AnalyticsService)
- Validators (FluentValidation rules)
- Component logic (React hooks)

**Integration Tests**:
- API endpoints (authorized and unauthorized access)
- Database queries (pagination, filtering)
- Email notifications (template rendering)

**E2E Tests**:
- Critical workflows (ban user, review task, create course)
- Role-based access control
- Analytics dashboard rendering

## Summary

All technical decisions align with WahadiniCryptoQuest's established architecture:
- ✅ Clean Architecture with CQRS
- ✅ .NET 8 + EF Core + PostgreSQL backend
- ✅ React 18 + TypeScript + TailwindCSS frontend
- ✅ Role-based authorization with JWT
- ✅ Email notifications with MailKit
- ✅ Production-ready patterns (pagination, caching, validation)

No blockers identified. Ready to proceed to Phase 1 (Data Model & Contracts).
