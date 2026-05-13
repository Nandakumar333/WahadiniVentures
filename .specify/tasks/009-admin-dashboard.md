# Feature: Comprehensive Admin Dashboard

## /speckit.specify

### Feature Overview
A centralized, secure admin interface for managing the entire platform. This includes user management, course content creation, task reviewing, reward management, and deep analytics visualization. The dashboard serves as the control center for platform operations.

### Feature Scope
- **Dashboard:** High-level KPIs (MRR, User Growth, Task Queue).
- **User Management:** CRUD operations, banning, role assignment.
- **Course Management:** Full CMS capabilities for courses and lessons.
- **Task Review:** Interface for admins to grade user submissions.
- **Rewards:** Manage point systems and generic discount codes.
- **Analytics:** Data visualization for revenue, retention, and engagement.
- **Security:** Role-based access (Admin only), Audit logs.

### User Stories
1. As an **Admin**, I want to see a birds-eye view of platform health (revenue, new users) upon login.
2. As an **Admin**, I need to review pending user tasks efficiently to unblock their progress.
3. As an **Admin**, I want to create and edit courses without touching the database directly.
4. As an **Admin**, I want to ban abusive users and manage user roles.
5. As an **Admin**, I want to analyze which courses are most popular to optimize content.
6. As an **Admin**, I need to create discount codes for marketing campaigns.
7. As an **Admin**, I want to see a log of administrative actions for accountability.

### Technical Requirements
- **Framework:** React Admin Dashboard template (e.g., using Material UI or Tailwind-based layout).
- **Charts:** Use `Recharts` or `Chart.js` for visualization.
- **API:** Admin-specific endpoints protected by `[Authorize(Roles = "Admin")]`.
- **Data:** Aggregation queries for analytics (SQL `GROUP BY`, `COUNT`).
- **Rich Text Editor:** For course description and lesson content.
- **Performance:** Pagination for large datasets (Users, Tasks).

---

## /speckit.plan

### Implementation Plan

#### Phase 1: Admin Shell & Security
**Tasks:**
1. create `AdminLayout` with sidebar navigation.
2. Implement `RequireAdmin` authorization policy.
3. Create `AdminController` skeleton.

**Deliverables:**
- Secure Admin route `/admin`.
- Sidebar navigation structure.

#### Phase 2: Dashboard & Analytics
**Tasks:**
1. Implement `AnalyticsService` to fetch KPI data.
2. Create `AdminDashboard` page with Summary Cards.
3. Integrate Chart library.
4. Visualize "Revenue Trend" and "User Signups".

**Deliverables:**
- Dashboard home with real-time stats.
- Visual charts for key metrics.

#### Phase 3: User Management
**Tasks:**
1. Create `UserList` table with search/filter.
2. Create `UserDetail` view (profile, progress, purchases).
3. Implement Ban/Unban logic.
4. Implement Role update logic.

**Deliverables:**
- Fully functional User Management section.

#### Phase 4: Course Management (CMS)
**Tasks:**
1. Create `CourseList` view.
2. Build `CourseEditor` (Form for metadata).
3. Build `LessonEditor` (Rich text, video URL).
4. Implement Reordering logic (Drag & Drop).

**Deliverables:**
- Ability to Create/Edit/Publish courses.

#### Phase 5: Task Review System
**Tasks:**
1. Create `TaskReviewQueue` page.
2. Build `SubmissionViewer` (Image/Text).
3. Implement Approve/Reject actions with feedback.
4. Notification trigger on review completion.

**Deliverables:**
- Workflow for grading user tasks.

#### Phase 6: Reward Management
**Tasks:**
1. Create `RewardsPage`.
2. Implement `CreateDiscountCode` form.
3. View redemption logs.
4. Manually adjust user points.

**Deliverables:**
- Tools to manage the economy system.

---

## /speckit.clarify

### Questions & Answers

**Q: Should the Admin Dashboard be a separate app or part of the main SPA?**
A: Part of the main SPA to share components and context, but lazily loaded to avoid bloating the user bundle. Route: `/admin/*`.

**Q: How do we handle image uploads for courses?**
A: We need an `UploadController` (S3/Azure Blob Storage). For MVP, we can potentially use external URLs or a simple local upload if configured.

**Q: Is "Soft Delete" required for users?**
A: Yes, we never hard delete users to preserve transaction history. Use an `IsDeleted` flag.

**Q: Do we need real-time updates for the dashboard?**
A: No, pull-to-refresh or page load is sufficient for MVP. WebSockets are overkill here.

---

## /speckit.analyze

### Technical Analysis

#### Backend Architecture
```
WahadiniCryptoQuest.Application/
├── Services/
│   ├── AnalyticsService.cs (Complex queries)
│   └── AdminService.cs (Orchestration)
└── DTOs/
    ├── Admin/
    │   ├── DashboardStatsDto.cs
    │   ├── UserSummaryDto.cs
    │   └── CourseEditDto.cs

WahadiniCryptoQuest.API/
└── Controllers/
    └── AdminController.cs
```

#### Frontend Architecture
```
src/
├── layouts/
│   └── AdminLayout.tsx
├── features/
│   └── admin/
│       ├── Dashboard/
│       │   ├── StatsCard.tsx
│       │   └── RevenueChart.tsx
│       ├── Users/
│       │   ├── UserTable.tsx
│       │   └── UserEditModal.tsx
│       ├── Courses/
│       │   ├── CourseForm.tsx
│       │   └── LessonList.tsx
│       └── Tasks/
│           └── ReviewCard.tsx
```

#### API Endpoints (Admin)
```
GET    /api/admin/stats (KPIs)
GET    /api/admin/users (Paginated, Search)
PUT    /api/admin/users/{id}/role
POST   /api/admin/users/{id}/ban
GET    /api/admin/tasks/pending
POST   /api/admin/tasks/{id}/review (Approve/Reject)
POST   /api/admin/courses (Create)
PUT    /api/admin/courses/{id} (Update)
```

---

## /speckit.checklist

### Implementation Checklist

#### Backend Prep
- [ ] Implement `RequireAdmin` policy in Program.cs.
- [ ] Create `AdminController` secured with Policy.
- [ ] Create `AnalyticsService`.

#### Feature: Dashboard
- [ ] Query Total Users count.
- [ ] Query Active Subscribers count.
- [ ] Calculate Monthly Recurring Revenue (MRR).
- [ ] Query Pending Tasks count.
- [ ] Build `DashboardStatsDto`.

#### Feature: User Management
- [ ] Implement `GetUsersAsync` (Search, Filter, Pagination).
- [ ] Implement `GetUserDetailAsync`.
- [ ] Implement `UpdateUserRoleAsync`.
- [ ] Implement `ToggleUserBanAsync`.

#### Feature: Content Management
- [ ] Implement Course CRUD endpoints.
- [ ] Implement Lesson CRUD endpoints.
- [ ] Add `IsPublished` toggle logic.

#### Feature: Task Review
- [ ] Query `TaskSubmission` where status is `Pending`.
- [ ] Implement `ReviewSubmissionAsync`.
- [ ] Send email/notification on review result.

#### Frontend Admin Area
- [ ] Create `/admin` Route Guard.
- [ ] Build Sidebar Navigation.
- [ ] Build Dashboard Page with Recharts.
- [ ] Build Data Tables (sorting, pagination).
- [ ] Build Forms (React Hook Form).

---

## /speckit.tasks

### Task Breakdown (Estimated 35-40 hours)

#### Task 1: Admin Infrastructure (4 hours)
**Description:** Setup routing, layout, and authorization.
**Subtasks:**
1. Create `AdminLayout` component.
2. Define children routes (Dashboard, Users, Courses, etc.).
3. Secure backend endpoints.

#### Task 2: Analytics & Dashboard (5 hours)
**Description:** Visualizing data.
**Subtasks:**
1. Implement `AnalyticsService` aggregation methods.
2. Create `StatsCards` component.
3. Integrate `recharts` for Line/Bar charts.
4. Fetch data on mount.

#### Task 3: User Management System (6 hours)
**Description:** Managing the user base.
**Subtasks:**
1. Build `UserTable` with columns (Name, Email, Role, Status, Joined).
2. Add Search bar and Filters (e.g., "Show Premium Only").
3. Create "Edit User" modal.
4. Connect Ban/Unban actions.

#### Task 4: CMS for Courses (10 hours)
**Description:** The core content creation tool.
**Subtasks:**
1. Create `CourseList` view.
2. Create `CourseEditor` (Title, Description, Price, Image).
3. Create nested `LessonManager`.
4. Implement "Add Lesson" functionality.
5. Rich text editor integration (e.g., `react-quill` or `tiptap`).

#### Task 5: Task Review Interface (6 hours)
**Description:** Queue for manual grading.
**Subtasks:**
1. Fetch pending submissions.
2. Create `SubmissionCard` displaying user proof (text/image).
3. Input field for "Feedback".
4. Buttons for "Approve" (Grant points) and "Reject".

#### Task 6: Reward Management (4 hours)
**Description:** Controlling the economy.
**Subtasks:**
1. List active discount codes.
2. Form to creat new codes (Type: %, Fixed).
3. View redemption usage stats.

---

## /speckit.implement

### Implementation Guide

#### Step 1: Admin Controller

**File:** `WahadiniCryptoQuest.API/Controllers/AdminController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WahadiniCryptoQuest.Application.Interfaces;
using WahadiniCryptoQuest.Application.DTOs.Admin;

[Route("api/admin")]
[ApiController]
[Authorize(Roles = "Admin")] // Critical Security
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IAnalyticsService _analyticsService;

    public AdminController(IAdminService adminService, IAnalyticsService analyticsService)
    {
        _adminService = adminService;
        _analyticsService = analyticsService;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        var stats = await _analyticsService.GetDashboardStatsAsync();
        return Ok(stats);
    }

    [HttpGet("users")]
    public async Task<ActionResult<PagedResult<UserSummaryDto>>> GetUsers([FromQuery] UserFilterDto filter)
    {
        var users = await _adminService.GetUsersAsync(filter);
        return Ok(users);
    }

    [HttpPost("users/{id}/ban")]
    public async Task<IActionResult> BanUser(Guid id)
    {
        await _adminService.BanUserAsync(id);
        return NoContent();
    }

    [HttpGet("tasks/pending")]
    public async Task<ActionResult<List<PendingTaskDto>>> GetPendingTasks()
    {
        var tasks = await _adminService.GetPendingTasksAsync();
        return Ok(tasks);
    }

    [HttpPost("tasks/{submissionId}/review")]
    public async Task<IActionResult> ReviewTask(Guid submissionId, [FromBody] TaskReviewDto review)
    {
        // review.Status = Approved | Rejected
        // review.Feedback = "..."
        await _adminService.ReviewTaskSubmissionAsync(submissionId, review);
        return Ok();
    }
    
    // Course Management Endpoints...
}
```

#### Step 2: Analytics Service

**File:** `WahadiniCryptoQuest.Application/Services/AnalyticsService.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Application.DTOs.Admin;
using WahadiniCryptoQuest.Infrastructure.Data;

public class AnalyticsService : IAnalyticsService
{
    private readonly ApplicationDbContext _context;

    public AnalyticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var totalUsers = await _context.Users.CountAsync();
        var premiumUsers = await _context.Users.CountAsync(u => u.SubscriptionTier != "Free");
        
        // Calculate MRR (Simplified: Sum of active subscription prices)
        // Ideally stored historically or fetched from Stripe
        var mrr = premiumUsers * 9.99m; 

        var pendingTasks = await _context.TaskSubmissions.CountAsync(t => t.Status == "Pending");

        var revenueTrend = new List<ChartPointDto>(); 
        // Logic to group payments by month for last 12 months...

        return new DashboardStatsDto
        {
            TotalUsers = totalUsers,
            PremiumUsers = premiumUsers,
            MonthlyRecurringRevenue = mrr,
            PendingTasks = pendingTasks,
            RevenueTrend = revenueTrend
        };
    }
}
```

#### Step 3: Admin Layout & Dashboard (Frontend)

**File:** `src/layouts/AdminLayout.tsx`

```tsx
import React from 'react';
import { Outlet, Link } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

const AdminLayout = () => {
  const { user } = useAuth();

  if (user?.role !== 'Admin') {
    return <div>Access Denied</div>;
  }

  return (
    <div className="flex h-screen bg-gray-100 dark:bg-gray-900">
      {/* Sidebar */}
      <aside className="w-64 bg-white dark:bg-gray-800 shadow-md">
        <div className="p-4 text-xl font-bold">Admin Panel</div>
        <nav className="mt-6 flex flex-col space-y-2 px-4">
          <Link to="/admin" className="p-2 hover:bg-gray-200 rounded">Dashboard</Link>
          <Link to="/admin/users" className="p-2 hover:bg-gray-200 rounded">Users</Link>
          <Link to="/admin/courses" className="p-2 hover:bg-gray-200 rounded">Courses</Link>
          <Link to="/admin/tasks" className="p-2 hover:bg-gray-200 rounded">Task Reviews</Link>
          <Link to="/admin/rewards" className="p-2 hover:bg-gray-200 rounded">Rewards</Link>
        </nav>
      </aside>

      {/* Main Content */}
      <main className="flex-1 overflow-y-auto p-8">
        <Outlet />
      </main>
    </div>
  );
};
```

**File:** `src/pages/admin/AdminDashboard.tsx`

```tsx
import React, { useEffect, useState } from 'react';
import { LineChart, Line, XAxis, YAxis, Tooltip, ResponsiveContainer } from 'recharts';
import api from '../../services/api';

const AdminDashboard = () => {
  const [stats, setStats] = useState<any>(null);

  useEffect(() => {
    const fetchStats = async () => {
      const res = await api.get('/admin/stats');
      setStats(res.data);
    };
    fetchStats();
  }, []);

  if (!stats) return <div>Loading...</div>;

  return (
    <div>
      <h1 className="text-3xl font-bold mb-6">Dashboard Overview</h1>
      
      {/* KPI Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
        <div className="bg-white p-6 rounded shadow">
          <h3 className="text-gray-500">Total Users</h3>
          <p className="text-3xl font-bold">{stats.totalUsers}</p>
        </div>
        <div className="bg-white p-6 rounded shadow">
          <h3 className="text-gray-500">Premium Users</h3>
          <p className="text-3xl font-bold text-indigo-600">{stats.premiumUsers}</p>
        </div>
        <div className="bg-white p-6 rounded shadow">
          <h3 className="text-gray-500">MRR (Est.)</h3>
          <p className="text-3xl font-bold text-green-600">${stats.monthlyRecurringRevenue}</p>
        </div>
        <div className="bg-white p-6 rounded shadow">
          <h3 className="text-gray-500">Pending Reviews</h3>
          <p className="text-3xl font-bold text-yellow-500">{stats.pendingTasks}</p>
        </div>
      </div>

      {/* Revenue Chart */}
      <div className="bg-white p-6 rounded shadow">
        <h3 className="text-xl font-bold mb-4">Revenue Trend</h3>
        <div className="h-64">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={stats.revenueTrend}>
              <XAxis dataKey="date" />
              <YAxis />
              <Tooltip />
              <Line type="monotone" dataKey="amount" stroke="#8884d8" strokeWidth={2} />
            </LineChart>
          </ResponsiveContainer>
        </div>
      </div>
    </div>
  );
};

export default AdminDashboard;
```

#### Step 4: Admin Task Review Page

**File:** `src/pages/admin/AdminTaskReview.tsx`

```tsx
import React, { useEffect, useState } from 'react';
import api from '../../services/api';

const AdminTaskReview = () => {
  const [tasks, setTasks] = useState([]);

  const loadTasks = async () => {
    const res = await api.get('/admin/tasks/pending');
    setTasks(res.data);
  };

  useEffect(() => { loadTasks(); }, []);

  const handleReview = async (id: string, status: 'Approved' | 'Rejected', feedback: string) => {
    await api.post(`/admin/tasks/${id}/review`, { status, feedback });
    loadTasks(); // Refresh list
  };

  return (
    <div>
      <h1 className="text-3xl font-bold mb-6">Pending Task Reviews</h1>
      <div className="space-y-4">
        {tasks.map((task: any) => (
          <div key={task.id} className="bg-white p-6 rounded shadow flex flex-col gap-4">
             <div className="flex justify-between">
                <h3 className="font-bold">{task.taskTitle}</h3>
                <span className="text-sm text-gray-500">User: {task.username}</span>
             </div>
             <div className="bg-gray-50 p-4 rounded border">
                <p><strong>Submission:</strong> {task.content}</p>
                {task.imageUrl && (
                  <img src={task.imageUrl} alt="Proof" className="mt-2 max-h-48 object-contain" />
                )}
             </div>
             
             <div className="flex gap-2">
                <button 
                  onClick={() => handleReview(task.id, 'Approved', 'Great work!')}
                  className="px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600"
                >
                  Approve
                </button>
                <button 
                   onClick={() => handleReview(task.id, 'Rejected', 'Please Fix X')}
                   className="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600"
                >
                   Reject
                </button>
             </div>
          </div>
        ))}
        {tasks.length === 0 && <p>No pending tasks.</p>}
      </div>
    </div>
  );
};

export default AdminTaskReview;
```
