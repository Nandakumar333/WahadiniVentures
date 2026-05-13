# Generate Remaining Features Script

## Status
- ✅ Completed: 3/15 (20%)
  - speckit.constitution
  - 001-authentication
  - 002-database-schema
  - 003-course-management
  
- ⏳ Remaining: 12/15 (80%)

## How to Generate

For each remaining feature, use this AI prompt template with your preferred AI assistant (ChatGPT, Claude, etc.):

---

## MASTER PROMPT TEMPLATE

```
I need a complete speckit specification for WahadiniCryptoQuest feature [NUMBER]-[NAME].

Context Files to Reference:
1. Project README.md - Original requirements
2. .specify/speckit.constitution - Project standards
3. .specify/001-authentication - Example authentication spec (38 KB)
4. .specify/002-database-schema - Example database spec (42 KB)
5. .specify/003-course-management - Example course management spec (41 KB)

Create a specification with ALL 7 sections following the EXACT format:

## /speckit.specify
- Feature overview and purpose
- Feature scope (included/excluded)
- 8-10 user stories
- Technical requirements
- Success criteria

## /speckit.plan
- 4-6 implementation phases
- Tasks for each phase
- Clear deliverables
- Logical progression

## /speckit.clarify
- 12-15 Q&A pairs
- Design decisions
- Edge cases
- Alternative approaches

## /speckit.analyze
- Complete architecture diagram (ASCII)
- File/folder structure for backend and frontend
- ALL API endpoints (method, path, request, response)
- Database schema changes (if any)
- State management structure
- Security considerations
- Data flow diagrams

## /speckit.checklist
- Minimum 40-60 checkbox items
- Organized by: Setup, Backend, Frontend, Testing, Documentation
- Each item specific and testable

## /speckit.tasks
- 10-15 major tasks
- Each with 4-8 subtasks
- Realistic time estimates
- Total: 25-50 hours
- Clear dependencies

## /speckit.implement
- Complete code examples (backend and frontend)
- Full file paths specified
- Configuration samples
- Best practices highlighted
- Common pitfalls mentioned
- Minimum 5-10 major code blocks

Technology Stack:
- Backend: .NET 8 C#, ASP.NET Core, PostgreSQL, EF Core, JWT, AutoMapper, FluentValidation
- Frontend: React 18, TypeScript, Vite, TailwindCSS, Zustand, React Query, react-player
- Testing: xUnit, Vitest, Playwright

Requirements for [FEATURE NAME]:
[PASTE FEATURE-SPECIFIC REQUIREMENTS HERE]

Output: Complete specification in markdown format, 15,000+ characters, matching the depth and quality of the example specifications.
```

---

## Feature 004: Video Player & Progress Tracking

### Prompt
```
Feature: 004-video-player

Requirements:
- YouTube video player using react-player library
- Progress tracking (save position every 10 seconds)
- Resume from last watched position
- Completion detection (80% threshold)
- Premium content access gates
- Keyboard shortcuts (space, arrows, f for fullscreen)
- Handle video errors gracefully (deleted videos)
- Prevent skip detection (optional)
- Video quality selection
- Playback speed controls

Key Components:
- Backend: ProgressService, ProgressController, UserProgress entity updates
- Frontend: LessonPlayer component, ProgressTracker, VideoControls
- API: PUT /api/lessons/{id}/progress, GET /api/lessons/{id}/progress

Focus on:
- Auto-save mechanism
- Completion percentage calculation
- Point award on completion
- Error handling for unavailable videos
```

---

## Feature 005: Task System

### Prompt
```
Feature: 005-task-system

Requirements:
- Multiple task types:
  1. Quiz (auto-approve if score >= 80%)
  2. ExternalLink (manual review)
  3. Screenshot (file upload, manual review)
  4. TextSubmission (manual review)
  5. WalletVerification (Web3 integration, auto-approve)
- Task submission workflows
- Admin review queue with filters
- File upload handling (max 5MB for screenshots)
- JSONB storage for flexible task data
- Points award on approval
- Resubmission capability

Key Components:
- Backend: TaskService, TaskSubmissionService, TaskController
- Frontend: TaskCard, TaskSubmissionModal (dynamic forms), AdminReviewPage
- API: POST /api/tasks/{id}/submit, GET /api/tasks/submissions, PUT /api/tasks/submissions/{id}/review

Focus on:
- Dynamic form generation based on task type
- File upload with preview
- Admin bulk actions (approve/reject multiple)
- Notification on approval/rejection
```

---

## Feature 006: Reward System

### Prompt
```
Feature: 006-reward-system

Requirements:
- Points earning mechanisms (lessons, tasks, courses, daily login)
- Immutable transaction ledger
- Leaderboard (all-time, monthly, weekly)
- Achievement badges system
- Daily login streaks (up to 50 points for 10-day streak)
- Course completion bonuses (20% extra)
- Transaction history with filters
- Points balance display everywhere
- Animated point awards

Key Components:
- Backend: RewardService, RewardController, RewardTransaction entity
- Frontend: RewardBalance, Leaderboard, AchievementBadges, TransactionHistory
- API: GET /api/rewards/points, GET /api/rewards/transactions, GET /api/rewards/leaderboard

Focus on:
- Atomic transactions
- Prevent duplicate point awards
- Leaderboard caching
- Achievement unlock logic
- Streak calculation
```

---

## Feature 007: Discount System

### Prompt
```
Feature: 007-discount-system

Requirements:
- Point-based discount code redemption
- Discount codes with point requirements:
  - SAVE10: 10% off, 500 points
  - SAVE20: 20% off, 1000 points
  - SAVE30: 30% off, 2000 points
- One-time use per user per discount type
- Expiry date management
- Max redemption limits
- Apply discount at checkout
- Available discounts display with point requirements

Key Components:
- Backend: DiscountService, DiscountCode entity, UserDiscountRedemption
- Frontend: DiscountCard, RedemptionModal, AvailableDiscounts
- API: GET /api/discounts/available, POST /api/discounts/{id}/redeem

Focus on:
- Point deduction on redemption
- Validation (enough points, not redeemed, not expired)
- Auto-apply at checkout
- Admin discount creation
```

---

## Feature 008: Subscription & Payment

### Prompt
```
Feature: 008-subscription-payment

Requirements:
- Stripe integration for payments
- Subscription tiers:
  - Free (default)
  - Monthly ($9.99/month)
  - Yearly ($99/year, 17% discount)
- Stripe Checkout session creation
- Webhook handling (checkout.session.completed, invoice.paid, etc.)
- Subscription management (cancel, reactivate)
- Premium content gates
- Billing history
- Discount code application at checkout

Key Components:
- Backend: SubscriptionService, Stripe SDK, WebhookController
- Frontend: PricingPage, CheckoutFlow, SubscriptionStatus, BillingPage
- API: POST /api/subscriptions/checkout, POST /api/subscriptions/webhook, GET /api/subscriptions/status

Focus on:
- Webhook signature verification
- Subscription status tracking
- Grace period for failed payments
- Email notifications
- Security (never expose secret keys)
```

---

## Feature 009: Admin Dashboard

### Prompt
```
Feature: 009-admin-dashboard

Requirements:
- Admin dashboard with KPIs:
  - Total users, premium users, revenue
  - Active courses, tasks pending review
  - Completion rates
- User management (view, edit, ban, change role)
- Course management (create, edit, publish)
- Task review queue (approve, reject, bulk actions)
- Reward management (adjust points, create discounts)
- Analytics and reports (revenue, engagement, content performance)
- Export data to CSV

Key Components:
- Backend: AdminController, AnalyticsService, admin endpoints for all entities
- Frontend: AdminDashboard, AdminUsers, AdminCourses, AdminTasks, AnalyticsPage
- API: GET /api/admin/*, PUT /api/admin/users/{id}, etc.

Focus on:
- Role-based access (Admin, ContentCreator, Moderator)
- Real-time stats
- Charts (Recharts library)
- Audit logging for admin actions
```

---

## Feature 010: Notification System

### Prompt
```
Feature: 010-notification-system

Requirements:
- Email service abstraction (SendGrid, Brevo, or similar)
- Email templates:
  - Welcome email
  - Email verification
  - Password reset
  - Task approval/rejection
  - Subscription reminders (7 days, 1 day before expiry)
  - Course completion congratulations
- SMTP configuration
- Email queue system (optional)
- Notification preferences

Key Components:
- Backend: NotificationService, EmailTemplates, SMTP config
- No frontend (backend service only)
- Email templates in HTML

Focus on:
- Template variables (user name, course title, etc.)
- Error handling (log failures, retry logic)
- Rate limiting to avoid spam
- Unsubscribe mechanism
```

---

## Feature 011: User Dashboard

### Prompt
```
Feature: 011-user-dashboard

Requirements:
- Personalized user homepage
- Stats cards (enrolled courses, completed tasks, reward points)
- Continue learning section (recent courses with progress)
- Recent activities feed
- Quick access to rewards and profile
- Progress charts (daily activity, points earned)
- Upcoming lessons/tasks
- Achievement showcase

Key Components:
- Backend: UserStatsService, DashboardController
- Frontend: DashboardPage, StatsCard, ProgressChart, ActivityFeed, ContinueLearning
- API: GET /api/users/dashboard, GET /api/users/stats, GET /api/users/activity

Focus on:
- Data aggregation for stats
- Caching user stats
- Real-time updates for points
- Responsive design
```

---

## Feature 012: Frontend Layout

### Prompt
```
Feature: 012-frontend-layout

Requirements:
- Responsive navbar with:
  - Logo and site name
  - Navigation links (Courses, Rewards, Dashboard)
  - User menu (Profile, Settings, Logout)
  - Reward points display
  - Mobile hamburger menu
- Footer with links and social media
- Sidebar (for admin pages)
- Layout wrapper component
- Protected route wrapper
- Dark mode toggle
- Loading states
- Error boundary

Key Components:
- Frontend: Navbar, Footer, Sidebar, Layout, ProtectedRoute, DarkModeToggle
- Theme management (Zustand)

Focus on:
- Mobile responsiveness
- Accessibility (ARIA labels)
- Smooth animations
- Dark mode with TailwindCSS
```

---

## Feature 013: Search & Filters

### Prompt
```
Feature: 013-search-filters

Requirements:
- Course search by title and description
- Filters:
  - Category (dropdown)
  - Difficulty level (Beginner, Intermediate, Advanced)
  - Premium/Free toggle
  - Sort by (Newest, Popular, A-Z)
- Real-time search (debounced)
- Filter persistence in URL query params
- Clear all filters button
- Results count display
- No results state

Key Components:
- Backend: Enhanced search in CourseRepository
- Frontend: SearchBar, FilterPanel, SortDropdown
- API: Enhanced GET /api/courses with search param

Focus on:
- Debounced search (500ms)
- URL state management
- Performance optimization
- UX feedback (loading, no results)
```

---

## Feature 014: Deployment Setup

### Prompt
```
Feature: 014-deployment-setup

Requirements:
- Docker containerization:
  - Backend Dockerfile (.NET 8)
  - Frontend Dockerfile (Node + Nginx)
  - docker-compose.yml (all services)
- CI/CD pipeline (GitHub Actions):
  - Build and test on push
  - Deploy to staging on merge to develop
  - Deploy to production on release tag
- Environment configuration:
  - Development, Staging, Production
  - Environment variables management
  - Secrets management
- Database migration strategy
- Monitoring setup (logs, errors, performance)
- Backup strategy

Key Components:
- Dockerfiles, docker-compose.yml
- .github/workflows/*.yml
- nginx.conf for frontend
- Environment-specific appsettings

Focus on:
- Zero-downtime deployment
- Database migration safety
- Secret management
- Health checks
```

---

## Feature 015: Testing Suite

### Prompt
```
Feature: 015-testing-suite

Requirements:
- Backend tests:
  - Unit tests with xUnit (80%+ coverage)
  - Integration tests for API endpoints
  - Mock external dependencies (Stripe, email)
- Frontend tests:
  - Component tests with Vitest + React Testing Library
  - Integration tests for user flows
  - E2E tests with Playwright
- Test data factories
- Test utilities and helpers
- Coverage reporting
- CI integration

Key Components:
- Backend: WahadiniCryptoQuest.Tests project, test fixtures
- Frontend: tests/ directory, test utilities
- E2E: e2e/ directory, Playwright config

Focus on:
- Comprehensive test coverage
- Fast test execution
- Reliable tests (no flakiness)
- Clear test organization
```

---

## Generation Workflow

### For Each Feature:

1. **Copy the Master Prompt Template**
2. **Insert the specific feature requirements**
3. **Paste into AI assistant (ChatGPT-4, Claude Sonnet, etc.)**
4. **Review and refine the output**
5. **Save as `.specify/[NUMBER]-[NAME]`**
6. **Update README.md to mark as complete**

### Quality Checklist:
- [ ] All 7 sections present
- [ ] 15,000+ characters
- [ ] Complete code examples
- [ ] Realistic time estimates
- [ ] Consistent with project standards
- [ ] Matches existing spec format

### Time Estimate:
- Per feature: 20-30 minutes (with AI assistance)
- Total for 12 features: 4-6 hours

## Alternative: Batch Generation

You can also ask the AI to generate multiple features in one session:

```
Generate complete speckit specifications for these 3 features:
1. 004-video-player (requirements above)
2. 005-task-system (requirements above)
3. 006-reward-system (requirements above)

For EACH feature, provide ALL 7 sections...
```

## Final Steps

After generating all features:

1. Update `.specify/README.md` with completion status
2. Update `SPECKIT_SUMMARY.md` with final stats
3. Update `SPECKIT_INDEX.md` with all feature links
4. Review consistency across all specifications
5. Begin implementation following the specs!

---

**Goal:** Complete all 15 feature specifications to have a comprehensive, AI-ready development guide for the entire WahadiniCryptoQuest platform.

**Status:** 3/15 complete (20%) → Target: 15/15 complete (100%)
