# Complete Feature Generation Guide

## 🎯 Mission: Generate 12 Remaining Features

**Current Status:** 3/15 Complete (20%)  
**Target:** 15/15 Complete (100%)  
**Time Required:** 3-4 hours with AI assistance

---

## 🤖 Automated Generation Process

### Method 1: One-by-One Generation (Recommended for Quality)

For each feature below, copy the COMPLETE PROMPT into ChatGPT-4 or Claude:

---

## 004: VIDEO PLAYER & PROGRESS TRACKING

```
Create a complete speckit specification for WahadiniCryptoQuest feature 004-video-player.

Reference these for format:
- Project: WahadiniCryptoQuest crypto learning platform
- Standards: .NET 8, React 18, PostgreSQL, JWT auth
- Example: Follow format from 001-authentication (38 KB) and 003-course-management (41 KB)

FEATURE: YouTube Video Player with Progress Tracking

REQUIREMENTS:
- YouTube video player using react-player library
- Auto-save watch position every 10 seconds to backend
- Resume from last watched position on return
- Completion detection at 80% threshold
- Award points automatically on completion
- Premium content access gates
- Keyboard shortcuts (Space: play/pause, F: fullscreen, arrows: seek ±10s)
- Handle deleted/unavailable videos gracefully
- Playback speed controls (0.5x, 1x, 1.5x, 2x)
- Video quality selection
- Progress bar with hover preview
- Prevent excessive skipping (optional fraud detection)

TECHNICAL:
- Backend: LessonProgressService, ProgressController
- Frontend: LessonPlayer component (main video player), ProgressTracker, VideoControls
- Database: UserProgress entity (already exists, enhance if needed)
- API Endpoints:
  - PUT /api/lessons/{id}/progress (body: { watchPosition: number })
  - GET /api/lessons/{id}/progress (returns: { lastPosition, completionPercentage, isCompleted })
  - POST /api/lessons/{id}/complete (triggers point award)

CODE COMPONENTS NEEDED:
Backend:
- ProgressService.cs (UpdateProgressAsync, GetProgressAsync, CompleteLessonAsync)
- ProgressController.cs (all endpoints)
- Progress validation logic

Frontend:
- LessonPlayer.tsx (main video component with react-player)
- useVideoProgress.ts (custom hook for auto-save)
- VideoControls.tsx (custom controls overlay)
- ProgressBar.tsx (progress indicator)

Create specification with ALL 7 sections:
1. /speckit.specify - Overview, scope, 8-10 user stories, technical requirements
2. /speckit.plan - 5-6 phases with tasks and deliverables
3. /speckit.clarify - 12-15 Q&A pairs covering edge cases
4. /speckit.analyze - Complete architecture, API specs, component structure, state flows
5. /speckit.checklist - 40-50 implementation tasks
6. /speckit.tasks - 10-12 major tasks with subtasks and time estimates (total: 25-35 hours)
7. /speckit.implement - Complete code examples for ProgressService, LessonPlayer component, API endpoints

Target: 18,000+ characters with production-ready code examples.
```

---

## 005: TASK SYSTEM

```
Create complete speckit specification for 005-task-system.

FEATURE: Task Submission & Verification System

REQUIREMENTS:
Five task types with different workflows:

1. QUIZ (auto-approve)
   - Multiple choice questions stored in JSONB
   - Auto-approve if score >= 80%
   - Immediate feedback

2. EXTERNAL LINK (manual review)
   - User submits wallet address or transaction hash
   - Admin reviews and approves/rejects
   - Instructions for what to do

3. SCREENSHOT (manual review with file upload)
   - File upload (max 5MB, jpg/png)
   - Image preview in admin review
   - Manual approval

4. TEXT SUBMISSION (manual review)
   - Long-form text input
   - Minimum word count validation
   - Manual approval

5. WALLET VERIFICATION (auto-approve with Web3)
   - Connect MetaMask wallet
   - Verify token balance or NFT ownership
   - Auto-approve if conditions met
   - (Web3 integration for future, manual for MVP)

WORKFLOWS:
- User sees available tasks for lesson
- User clicks task → opens submission modal
- Dynamic form based on task type
- User submits → status changes to "Pending"
- Admin reviews in queue → Approve/Reject
- On approval: Award points, notify user
- On rejection: Send feedback, allow resubmit

ADMIN FEATURES:
- Review queue with filters (task type, date, status)
- Bulk actions (approve/reject multiple)
- Preview submissions (especially screenshots)
- Add feedback/notes
- View user's previous submissions

TECHNICAL:
Backend:
- TaskService, TaskSubmissionService
- TaskController (CRUD + submit + review endpoints)
- File upload handling with validation
- JSONB for flexible TaskData and SubmissionData

Frontend:
- TaskCard (displays task with status)
- TaskSubmissionModal (dynamic form generator)
- QuizTaskForm, ScreenshotTaskForm, TextTaskForm, ExternalLinkTaskForm, WalletTaskForm
- AdminReviewPage with DataTable
- SubmissionPreviewModal

API Endpoints:
- GET /api/tasks/{id}
- POST /api/tasks/{id}/submit (multipart for files)
- GET /api/tasks/my-submissions?status=Pending
- GET /api/admin/tasks/submissions?type=Screenshot&status=Pending&page=1
- PUT /api/admin/tasks/submissions/{id}/review (body: { approved: bool, feedback: string })

Create ALL 7 sections with complete code for:
- Task entity with JSONB
- TaskSubmissionService with type-specific validation
- Dynamic React form generator
- Admin review interface
- File upload handling

Target: 25,000+ characters (largest feature).
```

---

## 006: REWARD SYSTEM

```
Create complete speckit specification for 006-reward-system.

FEATURE: Reward Points, Leaderboard & Achievements

REQUIREMENTS:
Points Earning:
- Complete lesson: Variable points (10-50 based on difficulty)
- Complete task: Points defined per task (5-100)
- Complete course: Bonus 20% of total course points
- Daily login: 5 points (streak bonus up to 50 for 10 days)
- First course completion: 100 bonus points
- Referral: 100 points when referred user completes first course

Points System:
- Immutable transaction ledger (RewardTransaction table)
- Every point change creates a transaction record
- Current balance = SUM(all transactions)
- Transaction types: Earned, Redeemed, Bonus, Penalty, Expired

Leaderboard:
- All-time leaderboard (top 100)
- Monthly leaderboard (resets each month)
- Weekly leaderboard
- Show user's rank even if not in top 100
- Cached for performance (update every 15 minutes)

Achievements:
- First Steps: Complete first lesson (10 bonus points)
- Task Master: Complete 10 tasks (50 points)
- Course Conqueror: Complete first course (100 points)
- Knowledge Seeker: Complete 5 courses (250 points)
- Crypto Pro: Complete advanced course (200 points)
- Point Hoarder: Reach 5000 points (500 bonus points)
- Generous: Refer 3 friends (300 points)

Daily Streaks:
- Track consecutive login days
- Award 5 points per day
- Streak bonuses: 5 days (+10), 10 days (+25), 30 days (+100)
- Reset on missed day

TECHNICAL:
Backend:
- RewardService (AwardPoints, DeductPoints, GetBalance, GetTransactions, GetLeaderboard)
- AchievementService (CheckAchievements, UnlockAchievement)
- RewardController, LeaderboardController

Frontend:
- RewardBalance component (persistent display)
- TransactionHistory page
- Leaderboard page with tabs (all-time, monthly, weekly)
- AchievementBadges showcase
- StreakTracker component
- Point animation on award (floating +X with confetti)

API Endpoints:
- GET /api/rewards/balance
- GET /api/rewards/transactions?page=1&type=Earned
- GET /api/rewards/leaderboard?range=monthly&page=1
- GET /api/rewards/achievements
- GET /api/rewards/streak

Create ALL 7 sections with code for:
- RewardTransaction immutable ledger
- Leaderboard caching and calculation
- Achievement unlock logic
- Streak calculation
- Point award integration with lesson/task completion

Target: 20,000+ characters.
```

---

## 007: DISCOUNT SYSTEM

```
Create complete speckit specification for 007-discount-system.

FEATURE: Point-Based Discount Redemption

REQUIREMENTS:
Discount Codes:
- SAVE10: 10% off monthly subscription, costs 500 points
- SAVE20: 20% off monthly subscription, costs 1000 points
- SAVE30: 30% off yearly subscription, costs 2000 points
- FREE7DAYS: 7-day free trial, costs 200 points

Rules:
- User must have enough points to redeem
- One redemption per discount type per user
- Discounts have expiry dates
- Max redemptions limit (optional, 0 = unlimited)
- Points are deducted immediately on redemption
- Discount code generated and displayed to user
- User applies code during Stripe checkout

Admin Features:
- Create new discount codes
- Set point requirements
- Set discount percentage
- Set expiry date
- Set max redemptions
- View redemption statistics

WORKFLOWS:
1. User browses available discounts
2. User clicks "Redeem for X points"
3. System validates (enough points, not redeemed, not expired)
4. System deducts points (creates negative RewardTransaction)
5. System creates UserDiscountRedemption record
6. System displays discount code to user
7. User copies code for checkout

TECHNICAL:
Backend:
- DiscountService
- DiscountController

Frontend:
- AvailableDiscounts page (grid of discount cards)
- DiscountCard component
- RedemptionModal (shows code after redemption)
- MyDiscounts page (shows redeemed codes)

API Endpoints:
- GET /api/discounts/available (filters by user points)
- POST /api/discounts/{id}/redeem
- GET /api/discounts/my-redeemed
- POST /api/admin/discounts (create new)
- PUT /api/admin/discounts/{id}
- GET /api/admin/discounts/analytics

Create ALL 7 sections with complete code.
Target: 15,000+ characters.
```

---

## 008: SUBSCRIPTION & PAYMENT

```
Create complete speckit specification for 008-subscription-payment.

FEATURE: Stripe Subscription Integration

REQUIREMENTS:
Subscription Tiers:
1. Free (default)
   - Access to free courses only
   - Limited task submissions (5 per day)
   - Ads displayed

2. Monthly Premium ($9.99/month)
   - Access to all courses
   - Unlimited tasks
   - No ads
   - Priority support

3. Yearly Premium ($99/year, 17% discount)
   - All monthly benefits
   - Exclusive advanced courses
   - Certificate of completion
   - 1-on-1 consultation (1 per quarter)

Stripe Integration:
- Checkout session creation
- Redirect to Stripe hosted checkout
- Webhook handling (checkout.session.completed, invoice.paid, invoice.payment_failed, customer.subscription.deleted)
- Webhook signature verification
- Subscription status tracking
- Grace period for failed payments (3 days)
- Automatic downgrade on expiry

Discount Application:
- Apply discount code from rewards at checkout
- Validate code hasn't been used in subscription
- Apply percentage discount to Stripe checkout session
- Mark code as used after successful payment

User Features:
- View subscription status
- Cancel subscription (retains premium until expiry)
- View billing history
- Update payment method (Stripe portal)
- Reactivate cancelled subscription

TECHNICAL:
Backend:
- SubscriptionService
- Stripe SDK integration
- WebhookController (separate endpoint)
- SubscriptionController

Frontend:
- PricingPage (3-tier comparison)
- CheckoutFlow
- SubscriptionStatusCard
- BillingHistoryPage
- Stripe Customer Portal integration

API Endpoints:
- GET /api/subscriptions/plans
- POST /api/subscriptions/checkout (body: { planId, discountCode? })
- POST /api/subscriptions/webhook (Stripe webhook)
- GET /api/subscriptions/status
- POST /api/subscriptions/cancel
- POST /api/subscriptions/reactivate
- GET /api/subscriptions/portal-link

SECURITY:
- Never expose Stripe secret key in frontend
- Verify webhook signatures
- Use environment variables for keys
- HTTPS required in production

Create ALL 7 sections with complete Stripe integration code.
Target: 22,000+ characters.
```

---

## 009: ADMIN DASHBOARD

```
Create complete speckit specification for 009-admin-dashboard.

FEATURE: Comprehensive Admin Interface

REQUIREMENTS:

Dashboard KPIs:
- Total users (with growth %)
- Premium users (with conversion rate)
- Monthly recurring revenue (MRR)
- Active courses
- Tasks pending review (clickable)
- Average course completion rate
- Revenue trend chart (last 12 months)
- User signup chart (last 30 days)
- Top performing courses

User Management:
- User list table (searchable, filterable)
- View user details (courses, progress, points, submissions)
- Edit user (role, subscription, points)
- Ban/unban users
- Delete users (soft delete)
- Send email to user
- View user activity log

Course Management:
- Course list with status (published/draft)
- Create new course (comprehensive form)
- Edit course (all fields)
- Add/edit/reorder lessons
- Publish/unpublish toggle
- Duplicate course
- View course analytics
- Delete course

Task Review:
- Pending submissions queue
- Filter by task type, course, date
- Preview submissions (especially screenshots)
- Approve with optional feedback
- Reject with required feedback
- Bulk approve/reject
- View submission history
- Reassign for review

Reward Management:
- Manually adjust user points
- Create discount codes
- View redemption analytics
- Points distribution chart
- Top earners list

Analytics:
- Revenue by plan (pie chart)
- User retention cohorts
- Course completion funnel
- Task approval rates
- Export data to CSV

TECHNICAL:
Backend:
- AdminController (all admin endpoints)
- AnalyticsService (data aggregation)
- Role-based authorization policies
- Audit logging

Frontend:
- AdminLayout (sidebar navigation)
- AdminDashboard (KPIs and charts)
- AdminUsers page
- AdminCourses page
- AdminTasks page
- AdminRewards page
- AnalyticsPage
- Recharts for data visualization

Create ALL 7 sections with complete admin interface code.
Target: 25,000+ characters (comprehensive).
```

---

## 010: NOTIFICATION SYSTEM

```
Create complete speckit specification for 010-notification-system.

FEATURE: Email Notification Service

REQUIREMENTS:
Email Templates:
1. Welcome Email (on registration)
2. Email Verification (with clickable link)
3. Password Reset (with token link)
4. Task Approved (with points earned)
5. Task Rejected (with feedback)
6. Lesson Completed (congratulations)
7. Course Completed (with certificate link)
8. Subscription Activated
9. Subscription Expiring (7 days notice)
10. Subscription Expired
11. Payment Failed
12. Achievement Unlocked
13. Referral Earned Points

Email Service:
- Abstracted interface (IEmailService)
- SendGrid implementation (free tier 100/day)
- SMTP fallback option
- Template engine (HTML with variables)
- Queue system for batch emails
- Retry logic for failures
- Unsubscribe mechanism

Configuration:
- SMTP settings in appsettings.json
- Template storage (embedded resources or files)
- Email preferences per user

TECHNICAL:
Backend:
- INotificationService interface
- EmailService implementation
- SendGrid SDK integration
- MailKit for SMTP
- Template engine (Razor or Scriban)
- Background job for email queue

No Frontend (backend service only)

Email Template Structure:
- HTML layout with inline CSS
- Responsive design
- Variable placeholders: {{userName}}, {{courseName}}, {{points}}, etc.
- Unsubscribe link in footer
- Company branding

API Endpoints:
- POST /api/notifications/test-email (admin testing)
- GET /api/notifications/preferences
- PUT /api/notifications/preferences

Create ALL 7 sections with complete email service code.
Target: 16,000+ characters.
```

---

## 011: USER DASHBOARD

```
Create complete speckit specification for 011-user-dashboard.

FEATURE: Personalized User Dashboard

REQUIREMENTS:

Dashboard Sections:

1. Header Stats (cards):
   - Total reward points (prominent)
   - Courses enrolled
   - Courses completed
   - Tasks completed
   - Current streak

2. Continue Learning:
   - Recently accessed courses
   - Progress bars for each
   - "Resume" buttons
   - Time since last access

3. Upcoming Tasks:
   - Next 5 incomplete tasks
   - From enrolled courses
   - Estimated time to complete
   - Point rewards

4. Recent Activities:
   - Last 10 activities (enrolled, completed lesson, earned points)
   - Timestamps
   - Activity icons

5. Progress Charts:
   - Weekly activity chart (lessons/tasks completed)
   - Points earned over time (line chart)
   - Course completion pie chart

6. Quick Actions:
   - Browse courses
   - View rewards
   - Check leaderboard
   - Edit profile

7. Achievements Showcase:
   - Recently unlocked badges
   - Progress to next achievement
   - "View all" link

8. Subscription Status:
   - Current plan (Free/Premium)
   - Expiry date if premium
   - "Upgrade" button if free
   - Manage subscription link

TECHNICAL:
Backend:
- DashboardService (aggregates data)
- UserStatsService
- DashboardController

Frontend:
- DashboardPage (main layout)
- StatsCard component
- ContinueLearningCard
- RecentActivities list
- ProgressCharts (Recharts)
- QuickActions grid
- AchievementShowcase

API Endpoints:
- GET /api/users/dashboard (all dashboard data in one call)
- GET /api/users/stats
- GET /api/users/recent-activity?limit=10
- GET /api/users/continue-learning

Optimization:
- Cache dashboard data for 5 minutes
- Lazy load charts
- Pagination for activities

Create ALL 7 sections with complete dashboard code.
Target: 18,000+ characters.
```

---

## 012: FRONTEND LAYOUT

```
Create complete speckit specification for 012-frontend-layout.

FEATURE: Core Layout Components & Navigation

REQUIREMENTS:

Navbar:
- Logo and site name (clickable → home)
- Navigation links: Home, Courses, Rewards, Dashboard, Admin (if admin)
- Search icon (opens search modal)
- Reward points badge (always visible for logged-in users)
- Notifications bell icon (future)
- User menu dropdown:
  - Profile
  - Settings
  - My Courses
  - Billing (if premium)
  - Logout
- Mobile hamburger menu
- Responsive design (mobile-first)
- Sticky on scroll
- Dark mode toggle

Footer:
- Company info and tagline
- Navigation columns:
  - Learn (Courses, Categories)
  - Company (About, Blog, Contact)
  - Legal (Privacy, Terms, Cookies)
  - Support (FAQ, Help Center)
- Social media icons
- Newsletter signup
- Copyright notice
- Language selector (future)

Sidebar (Admin Pages):
- Admin navigation menu
- Collapsible sections
- Active link highlighting
- Icons for each section
- Collapse/expand button

Layout Wrapper:
- Main Layout component
- AdminLayout component
- Consistent spacing and padding
- Max-width containers
- Breadcrumb navigation

Protected Routes:
- ProtectedRoute component (checks auth)
- PremiumRoute component (checks subscription)
- AdminRoute component (checks role)
- Redirect to login if unauthorized
- Show loading spinner while checking

Theme System:
- Dark mode toggle (localStorage persistence)
- TailwindCSS dark: classes
- Smooth transitions
- System preference detection

Loading States:
- Full-page loader
- Skeleton loaders for content
- Button loading states
- Inline spinners

Error Handling:
- Error boundary component
- 404 Not Found page
- 403 Forbidden page
- 500 Error page
- Network error display

TECHNICAL:
Frontend only:
- Navbar.tsx
- Footer.tsx
- Sidebar.tsx
- Layout.tsx, AdminLayout.tsx
- ProtectedRoute.tsx, PremiumRoute.tsx, AdminRoute.tsx
- DarkModeToggle.tsx
- ErrorBoundary.tsx
- LoadingSpinner.tsx

Theme store (Zustand):
- Dark mode state
- Toggle function
- Persistence

Create ALL 7 sections with complete layout code.
Target: 20,000+ characters.
```

---

## 013: SEARCH & FILTERS

```
Create complete speckit specification for 013-search-filters.

FEATURE: Course Search & Advanced Filtering

REQUIREMENTS:

Search Features:
- Full-text search in course titles and descriptions
- Real-time search (debounced 500ms)
- Search suggestions dropdown
- Recent searches (stored locally)
- Clear search button
- Search results count

Filter Options:
1. Category filter (dropdown with all categories)
2. Difficulty filter (checkboxes: Beginner, Intermediate, Advanced)
3. Content type filter (Free/Premium toggle)
4. Sort by:
   - Newest first
   - Most popular (by enrollment)
   - Highest rated (future)
   - A-Z title
   - Most reward points

Advanced Filters (expandable):
- Duration range (slider: 0-120 minutes)
- Points range (slider: 0-500)
- Created date range (date picker)

Filter Persistence:
- Store filters in URL query params
- Deep linking support (shareable URLs)
- Persist filters in sessionStorage
- "Clear all filters" button

UI Features:
- Filter panel (sidebar on desktop, modal on mobile)
- Active filters display (chips with remove button)
- Results count: "Showing 24 of 156 courses"
- No results state with suggestions
- Loading skeleton while searching

TECHNICAL:
Backend:
- Enhanced CourseRepository search methods
- Full-text search with PostgreSQL
- Optimized queries with indexes

Frontend:
- SearchBar component
- FilterPanel component
- FilterChips component
- SortDropdown component
- useSearchFilters custom hook
- URL state management (react-router searchParams)

API Enhancements:
- GET /api/courses?search=crypto&category=guid&difficulty=Beginner&isPremium=false&sortBy=popular&minDuration=10&maxDuration=60&page=1

Optimizations:
- Debounce search input
- Cache search results
- Virtualized list for large result sets

Create ALL 7 sections with complete search/filter code.
Target: 17,000+ characters.
```

---

## 014: DEPLOYMENT SETUP

```
Create complete speckit specification for 014-deployment-setup.

FEATURE: Docker, CI/CD & Production Deployment

REQUIREMENTS:

Docker Setup:
- Backend Dockerfile (.NET 8 multi-stage build)
- Frontend Dockerfile (Node build + Nginx serve)
- docker-compose.yml (all services: backend, frontend, postgres)
- docker-compose.prod.yml (production overrides)
- nginx.conf for frontend (SPA routing, gzip, caching)
- .dockerignore files

CI/CD Pipeline (GitHub Actions):
- Build workflow (.github/workflows/build.yml):
  - Run on push to any branch
  - Build backend (.NET restore, build)
  - Build frontend (npm install, build)
  - Run tests
  - Lint code

- Test workflow:
  - Run unit tests (backend + frontend)
  - Run integration tests
  - Generate coverage report
  - Comment coverage on PR

- Deploy to staging:
  - Trigger on merge to 'develop' branch
  - Build Docker images
  - Push to container registry
  - Deploy to staging server
  - Run smoke tests

- Deploy to production:
  - Trigger on release tag (v*)
  - Build production images
  - Push to registry with version tag
  - Deploy to production with zero-downtime
  - Database migration
  - Health check
  - Rollback on failure

Environment Configuration:
- .env.example files
- Environment-specific appsettings (Development, Staging, Production)
- Secrets management (GitHub Secrets, Azure Key Vault)
- Database connection strings
- API keys (Stripe, SendGrid, etc.)

Database Migrations:
- Migration strategy in CI/CD
- Backup before migration
- Rollback plan
- Test migrations in staging first

Health Checks:
- /health endpoint (backend)
- Database connectivity check
- External service checks (Stripe, email)

Monitoring & Logging:
- Structured logging (Serilog)
- Log aggregation (consider free options)
- Error tracking (Sentry free tier)
- Application insights

Backup Strategy:
- Automated daily database backups
- Backup retention (30 days)
- Backup restoration testing

Recommended Hosting (Free/Budget):
- Backend: Railway.app, Render.com, Fly.io
- Frontend: Vercel, Netlify, Cloudflare Pages
- Database: Supabase, Railway PostgreSQL
- Container Registry: GitHub Container Registry

TECHNICAL:
Infrastructure:
- Dockerfile (backend)
- Dockerfile (frontend)
- docker-compose.yml
- nginx.conf
- .github/workflows/*.yml

Scripts:
- deploy.sh
- migrate.sh
- backup.sh
- health-check.sh

Documentation:
- DEPLOYMENT.md
- CONTRIBUTING.md
- Environment setup guide

Create ALL 7 sections with complete deployment configuration.
Target: 20,000+ characters.
```

---

## 015: TESTING SUITE

```
Create complete speckit specification for 015-testing-suite.

FEATURE: Comprehensive Testing Infrastructure

REQUIREMENTS:

Backend Tests (xUnit):
- Unit tests (80%+ coverage):
  - Services (AuthService, CourseService, RewardService, etc.)
  - Validators (FluentValidation)
  - Utilities and helpers
  - Domain logic

- Integration tests:
  - API endpoints (all controllers)
  - Database operations (repositories)
  - Authentication flow
  - External service integration (mocked)

- Test structure:
  - Arrange-Act-Assert pattern
  - Test fixtures for setup/teardown
  - In-memory database for tests
  - Moq for mocking dependencies
  - FluentAssertions for readable assertions

Frontend Tests:
- Component tests (Vitest + React Testing Library):
  - All components (user-facing and reusable)
  - User interactions (clicks, inputs, form submissions)
  - Conditional rendering
  - Props and state changes

- Integration tests:
  - Page components
  - User flows (registration, course enrollment, task submission)
  - API integration (mocked with MSW)
  - Routing

- E2E tests (Playwright):
  - Critical user journeys:
    1. User registration → email verification → login
    2. Browse courses → enroll → watch lesson → complete task
    3. Earn points → redeem discount → subscribe
    4. Admin creates course → publishes → user enrolls
  - Cross-browser testing (Chrome, Firefox, Safari)
  - Mobile viewport testing

Test Utilities:
- Test data factories (generate mock users, courses, etc.)
- Test helpers (login helper, seed data, cleanup)
- Custom matchers
- Mock API responses

Coverage Reporting:
- Backend: Coverlet + ReportGenerator
- Frontend: Vitest coverage with v8
- Coverage thresholds: 80% minimum
- Display in CI/CD pipeline
- Generate HTML reports

CI Integration:
- Run tests on every push
- Fail pipeline if tests fail
- Parallel test execution
- Fast feedback (< 5 minutes)

Performance Tests:
- Load testing with k6 (optional)
- API response time benchmarks
- Database query performance

TECHNICAL:
Backend:
- WahadiniCryptoQuest.Tests project
- Test fixtures and base classes
- Mock configurations

Frontend:
- vitest.config.ts
- playwright.config.ts
- test/ directory structure
- Mock service worker setup

Scripts:
- test.sh (run all tests)
- test-watch.sh (watch mode)
- coverage.sh (generate reports)

Create ALL 7 sections with complete test examples.
Target: 22,000+ characters.
```

---

## 📝 Usage Instructions

1. **Copy entire prompt** for desired feature (including the triple backticks)
2. **Paste into AI** (ChatGPT-4, Claude Sonnet 3.5, or similar)
3. **Wait for generation** (2-3 minutes per feature)
4. **Review output** - ensure all 7 sections are present
5. **Save as file** - `.specify/00X-feature-name`
6. **Repeat** for next feature

## ⚡ Batch Processing

You can also ask AI to generate 2-3 features at once:

```
Generate complete specifications for these features:
1. 004-video-player (prompt above)
2. 005-task-system (prompt above)
3. 006-reward-system (prompt above)

For EACH feature, provide ALL 7 sections with complete code examples...
```

## ✅ Quality Checklist

After generation, verify:
- [ ] All 7 sections present
- [ ] 15,000+ characters
- [ ] Complete code examples (3-5+ major blocks)
- [ ] API endpoints fully specified
- [ ] Time estimates realistic
- [ ] Matches format of 001-003

## 🎯 Target Timeline

- **Per feature:** 15-20 minutes
- **All 12 features:** 3-4 hours
- **With batch processing:** 2-3 hours

---

**Ready to complete the framework!** 🚀
