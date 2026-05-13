Crypto Learning Platform - Comprehensive Project Documentation
I'll create a detailed documentation and GitHub Copilot Edits prompts for your crypto education platform with gamified task-to-earn features.

## 🎓 Feature Status: Course & Lesson Management System

**Status**: ✅ Fully Implemented | **Completion**: 150/208 tasks (72%) | [View Full Feature Overview](docs/COURSE_MANAGEMENT_FEATURE.md)

Core capabilities include course browsing, enrollment, YouTube video lessons with progress tracking, premium access control, and admin management interface. See [Feature Documentation](docs/COURSE_MANAGEMENT_FEATURE.md) for technical details, API reference, and setup instructions.

---

📋 Project Overview
Platform Name: WahadiniCryptoQuest (or your preferred name)
Core Features:

Video-based crypto education (YouTube embedded)
Real-time task verification system
Reward points system
Premium subscription with discounts
Gamification elements
Multi-category learning paths (Airdrops, GameFi, Task-to-Earn, etc.)

Tech Stack:

Frontend: React (with Vite recommended for speed)
Backend: .NET 8 Web API
Database: PostgreSQL (recommended for relational data + JSONB support)
Video: YouTube embedded (free)
Authentication: JWT + ASP.NET Identity
Payment: Stripe/PayPal (for premium subscriptions)


🎯 GitHub Copilot Edits Prompts
I'll provide you with detailed prompts for each component. Use these in GitHub Copilot Edits (formerly Speckit) by opening the relevant files and pasting these prompts.
Prompt 1: Database Schema & Models
Create a complete database schema for a crypto learning platform with the following requirements:

DATABASE: PostgreSQL

ENTITIES NEEDED:
1. Users
   - Id, Email, Username, PasswordHash, Role (Admin/Premium/Free)
   - SubscriptionTier (Free, Monthly, Yearly)
   - SubscriptionExpiryDate
   - RewardPoints (integer)
   - CreatedAt, UpdatedAt
   - IsActive, EmailVerified

2. Categories
   - Id, Name (e.g., "Airdrops", "GameFi", "Task-to-Earn", "DeFi", "NFT Strategies")
   - Description, IconUrl, DisplayOrder
   - IsActive

3. Courses
   - Id, CategoryId (FK), Title, Description, ThumbnailUrl
   - DifficultyLevel (Beginner, Intermediate, Advanced)
   - EstimatedDuration (minutes)
   - IsPremium (boolean)
   - RewardPoints (points earned on completion)
   - CreatedAt, UpdatedAt, CreatedByUserId
   - IsPublished, ViewCount

4. Lessons
   - Id, CourseId (FK), Title, Description
   - YoutubeVideoId (extract from URL)
   - Duration (minutes), OrderIndex
   - IsPremium, RewardPoints
   - ContentMarkdown (additional text content)

5. Tasks
   - Id, LessonId (FK), Title, Description
   - TaskType (Quiz, ExternalLink, WalletVerification, Screenshot, TextSubmission)
   - TaskData (JSONB - stores type-specific data like quiz questions, wallet address to interact with, etc.)
   - RewardPoints, TimeLimit (minutes, nullable)
   - OrderIndex, IsRequired

6. UserTaskSubmissions
   - Id, UserId (FK), TaskId (FK)
   - SubmissionData (JSONB - stores answers, screenshots, proof)
   - Status (Pending, Approved, Rejected)
   - SubmittedAt, ReviewedAt, ReviewedByUserId
   - FeedbackText, RewardPointsAwarded

7. UserProgress
   - Id, UserId (FK), LessonId (FK)
   - CompletionPercentage, LastWatchedPosition (seconds)
   - IsCompleted, CompletedAt
   - RewardPointsClaimed (boolean)

8. UserCourseEnrollments
   - Id, UserId (FK), CourseId (FK)
   - EnrolledAt, LastAccessedAt
   - CompletionPercentage, IsCompleted, CompletedAt

9. RewardTransactions
   - Id, UserId (FK), Amount (can be negative for redemptions)
   - TransactionType (Earned, Redeemed, Bonus, Penalty)
   - ReferenceId (TaskId, CourseId, etc.), ReferenceType
   - Description, CreatedAt

10. DiscountCodes
    - Id, Code (unique), DiscountPercentage
    - RequiredPoints (points needed to unlock)
    - MaxRedemptions, CurrentRedemptions
    - ExpiryDate, IsActive, CreatedAt

11. UserDiscountRedemptions
    - Id, UserId (FK), DiscountCodeId (FK)
    - RedeemedAt, UsedInSubscription (boolean)

CREATE:
- C# Entity classes with data annotations
- DbContext class with all DbSets
- Proper relationships (navigation properties)
- Indexes for performance (UserId, CourseId, Status fields)
- Fluent API configurations for complex relationships
- Seed data method for initial categories

FOLLOW .NET best practices with nullable reference types enabled.
```

---

### **Prompt 2: Backend API Structure**
```
Create a complete .NET 8 Web API project structure for a crypto learning platform:

ARCHITECTURE: Clean Architecture with the following layers
- API (Controllers, Middleware, Filters)
- Application (Services, DTOs, Interfaces, Validators)
- Domain (Entities, Enums, Exceptions)
- Infrastructure (Data, Repositories, External Services)

REQUIRED CONTROLLERS & ENDPOINTS:

1. AuthController
   - POST /api/auth/register (email, username, password)
   - POST /api/auth/login (returns JWT token)
   - POST /api/auth/refresh-token
   - POST /api/auth/forgot-password
   - POST /api/auth/reset-password
   - GET /api/auth/verify-email/{token}

2. UsersController
   - GET /api/users/profile (authenticated)
   - PUT /api/users/profile
   - GET /api/users/reward-points
   - GET /api/users/stats (courses enrolled, tasks completed, etc.)

3. CategoriesController
   - GET /api/categories (public, returns all active categories)
   - GET /api/categories/{id}/courses
   - POST /api/categories (admin only)
   - PUT /api/categories/{id} (admin only)

4. CoursesController
   - GET /api/courses (with filters: categoryId, difficulty, isPremium, search)
   - GET /api/courses/{id} (detailed info with lessons)
   - POST /api/courses/{id}/enroll (authenticated)
   - GET /api/courses/my-courses (user's enrolled courses)
   - POST /api/courses (admin only)
   - PUT /api/courses/{id} (admin only)
   - DELETE /api/courses/{id} (admin only)

5. LessonsController
   - GET /api/lessons/{id} (with video and tasks)
   - POST /api/lessons/{id}/complete
   - PUT /api/lessons/{id}/progress (update watch position)
   - POST /api/lessons (admin only)
   - PUT /api/lessons/{id} (admin only)

6. TasksController
   - GET /api/tasks/{id}
   - POST /api/tasks/{id}/submit (with submission data)
   - GET /api/tasks/my-submissions (user's task history)
   - GET /api/tasks/pending-reviews (admin only)
   - PUT /api/tasks/submissions/{id}/review (admin only - approve/reject)
   - POST /api/tasks (admin only)

7. RewardsController
   - GET /api/rewards/transactions (user's reward history)
   - GET /api/rewards/available-discounts
   - POST /api/rewards/redeem-discount (requires points)
   - GET /api/rewards/leaderboard (top users by points)

8. SubscriptionsController
   - GET /api/subscriptions/plans
   - POST /api/subscriptions/checkout (with optional discount code)
   - POST /api/subscriptions/webhook (payment provider webhook)
   - GET /api/subscriptions/status
   - POST /api/subscriptions/cancel

IMPLEMENT:
- JWT authentication middleware
- Role-based authorization (Free, Premium, Admin)
- Global exception handling middleware
- Request/Response logging
- CORS configuration
- Rate limiting for API endpoints
- Model validation with FluentValidation
- AutoMapper for DTO mappings
- Pagination helpers for list endpoints
- Swagger/OpenAPI documentation
- appsettings.json structure (ConnectionStrings, JWT settings, etc.)

Create the Program.cs with all service registrations and middleware configuration.
```

---

### **Prompt 3: Repository Pattern & Services**
```
Create a complete Repository pattern implementation and Application services for the crypto learning platform:

REQUIRED REPOSITORIES (Interface + Implementation):
- IUserRepository / UserRepository
- ICourseRepository / CourseRepository
- ILessonRepository / LessonRepository
- ITaskRepository / TaskRepository
- IRewardRepository / RewardRepository
- ISubscriptionRepository / SubscriptionRepository

EACH REPOSITORY SHOULD HAVE:
- GetByIdAsync
- GetAllAsync (with pagination)
- AddAsync
- UpdateAsync
- DeleteAsync
- SaveChangesAsync
- Specialized query methods (e.g., GetCoursesByCategory, GetUserActiveTasks)

APPLICATION SERVICES (Interface + Implementation):

1. IAuthService / AuthService
   - RegisterAsync(RegisterDto)
   - LoginAsync(LoginDto) - returns JWT token
   - RefreshTokenAsync(string refreshToken)
   - SendPasswordResetAsync(string email)
   - ResetPasswordAsync(ResetPasswordDto)
   - GenerateJwtToken(User user)
   - HashPassword / VerifyPassword

2. ICourseService / CourseService
   - GetAllCoursesAsync(filters, pagination)
   - GetCourseDetailAsync(courseId, userId) - check enrollment & access
   - EnrollUserAsync(userId, courseId) - check premium access
   - GetUserCoursesAsync(userId)
   - CreateCourseAsync(CreateCourseDto) - admin only
   - UpdateCourseAsync(UpdateCourseDto)
   - CalculateCourseProgress(userId, courseId)

3. ILessonService / LessonService
   - GetLessonDetailAsync(lessonId, userId) - check access
   - UpdateProgressAsync(userId, lessonId, watchPosition)
   - CompleteLessonAsync(userId, lessonId) - award points
   - ValidateUserAccess(userId, lessonId) - check premium/enrollment

4. ITaskService / TaskService
   - GetTaskDetailAsync(taskId, userId)
   - SubmitTaskAsync(userId, taskId, submissionData)
   - GetUserSubmissionsAsync(userId, filters)
   - GetPendingReviewsAsync(pagination) - admin
   - ReviewSubmissionAsync(submissionId, approved, feedback, reviewerId)
   - ValidateTaskSubmission(taskType, submissionData) - type-specific validation

5. IRewardService / RewardService
   - GetUserPointsAsync(userId)
   - AwardPointsAsync(userId, points, transactionType, referenceId, description)
   - DeductPointsAsync(userId, points, reason)
   - GetTransactionHistoryAsync(userId, pagination)
   - GetAvailableDiscountsAsync(userPoints)
   - RedeemDiscountAsync(userId, discountCodeId)
   - GetLeaderboardAsync(topN, timeRange)

6. ISubscriptionService / SubscriptionService
   - GetPlansAsync()
   - CreateCheckoutSessionAsync(userId, planId, discountCode)
   - ProcessPaymentWebhookAsync(webhookData)
   - GetUserSubscriptionAsync(userId)
   - CancelSubscriptionAsync(userId)
   - IsUserPremiumAsync(userId)

7. INotificationService / NotificationService
   - SendEmailAsync(to, subject, body)
   - SendTaskApprovalNotificationAsync(userId, taskTitle, pointsAwarded)
   - SendWelcomeEmailAsync(userId)

IMPLEMENT:
- Proper error handling with custom exceptions (NotFoundException, UnauthorizedException, ValidationException)
- Transaction management where needed
- Caching for frequently accessed data (categories, subscription plans)
- Logging with ILogger
- Use dependency injection throughout
- DTOs for all service inputs/outputs
- Validation logic in services
- Unit of Work pattern if needed

Include common DTOs like:
- CourseDto, LessonDto, TaskDto
- UserProfileDto, UserStatsDto
- RewardTransactionDto, LeaderboardEntryDto
- PaginatedResponseDto<T>
```

---

### **Prompt 4: React Frontend Structure**
```
Create a complete React application structure for a crypto learning platform using Vite:

PROJECT SETUP:
- Use Vite + React + TypeScript
- Install: react-router-dom, axios, zustand (state management), react-query, tailwindcss, lucide-react (icons), react-player (for YouTube), react-hook-form, zod (validation)

FOLDER STRUCTURE:
src/
├── components/
│   ├── common/ (Button, Card, Modal, Input, etc.)
│   ├── layout/ (Navbar, Sidebar, Footer)
│   ├── auth/ (LoginForm, RegisterForm)
│   ├── course/ (CourseCard, CourseDetail, LessonPlayer)
│   ├── task/ (TaskCard, TaskSubmissionForm)
│   └── reward/ (RewardBalance, LeaderboardTable)
├── pages/
│   ├── HomePage
│   ├── CoursesPage
│   ├── CourseDetailPage
│   ├── LessonPage
│   ├── DashboardPage
│   ├── ProfilePage
│   ├── RewardsPage
│   ├── AdminPage
│   ├── LoginPage
│   └── RegisterPage
├── services/ (API calls)
│   ├── api.ts (axios instance with interceptors)
│   ├── authService.ts
│   ├── courseService.ts
│   ├── taskService.ts
│   └── rewardService.ts
├── stores/ (Zustand stores)
│   ├── authStore.ts
│   ├── courseStore.ts
│   └── uiStore.ts
├── hooks/
│   ├── useAuth.ts
│   ├── useCourses.ts
│   └── useRewards.ts
├── types/
│   ├── user.types.ts
│   ├── course.types.ts
│   └── task.types.ts
├── utils/
│   ├── constants.ts
│   ├── formatters.ts
│   └── validators.ts
└── App.tsx, main.tsx, routes.tsx

REQUIRED PAGES & COMPONENTS:

1. HomePage
   - Hero section with value proposition
   - Featured courses
   - Categories grid
   - How it works section
   - Stats (total users, points awarded, courses)

2. CoursesPage
   - Filters (category, difficulty, premium/free)
   - Search bar
   - Course cards with: thumbnail, title, difficulty, reward points, premium badge
   - Pagination

3. CourseDetailPage
   - Course header (title, description, instructor, difficulty)
   - Enrollment button (check premium access)
   - Lessons list with progress indicators
   - Total reward points display
   - Requirements section

4. LessonPage (CRITICAL)
   - YouTube video player (react-player)
   - Video progress tracking (save position every 10 seconds)
   - Lesson content/transcript below video
   - Tasks section (show locked/unlocked tasks)
   - Navigation (previous/next lesson)
   - Complete lesson button (after watching 80% or more)

5. TaskSubmissionForm (varies by task type)
   - Quiz type: Multiple choice questions
   - ExternalLink type: Input field for wallet address or transaction hash
   - Screenshot type: File upload
   - TextSubmission type: Textarea
   - Submit button
   - Show reward points on completion

6. DashboardPage (authenticated users)
   - User stats cards (enrolled courses, completed tasks, reward points)
   - Continue learning section (resume courses)
   - Recent activities
   - Available discounts
   - Progress charts

7. RewardsPage
   - Current points balance (prominent display)
   - Transaction history table
   - Available discount codes (with point requirements)
   - Redeem discount button
   - Leaderboard

8. ProfilePage
   - Edit profile form
   - Subscription status
   - Change password
   - Account settings

9. AdminPage (admin only)
   - Dashboard with stats
   - Create/Edit courses
   - Manage tasks
   - Review task submissions (approve/reject)
   - User management

IMPLEMENT:
- Protected routes (check authentication)
- Premium content guards (check subscription)
- JWT token management (store in httpOnly cookie or localStorage)
- Axios interceptors for auth headers and token refresh
- Loading states for all async operations
- Error handling with toast notifications
- Responsive design (mobile-first)
- Dark mode toggle
- Form validation with zod
- Infinite scroll or pagination
- Video progress persistence
- Optimistic UI updates

Create a modern, clean UI with Tailwind CSS. Use a color scheme appropriate for crypto/finance (blues, purples, greens).

Include TypeScript interfaces for all API responses and component props.
```

---

### **Prompt 5: Authentication & Authorization System**
```
Create a complete authentication and authorization system for both backend (.NET) and frontend (React):

BACKEND (.NET):

1. JWT Configuration
   - JwtSettings class (Issuer, Audience, SecretKey, ExpirationMinutes)
   - Configure in Program.cs with AddAuthentication & AddJwtBearer
   - Token generation method with claims (UserId, Email, Role, SubscriptionTier)
   - Refresh token generation and storage

2. ASP.NET Identity Integration
   - Configure Identity with User entity
   - Custom UserManager and RoleManager setup
   - Email confirmation flow
   - Password requirements configuration

3. Authorization Policies
   - [Authorize] attribute usage
   - Custom policy: "RequirePremium" - checks subscription expiry
   - Custom policy: "RequireAdmin"
   - Custom authorization handler for premium content access

4. Middleware
   - JWT validation middleware
   - Custom middleware to check subscription status on each request
   - Exception handling middleware

FRONTEND (React):

1. Auth Store (Zustand)
```typescript
   interface AuthState {
     user: User | null;
     token: string | null;
     isAuthenticated: boolean;
     isPremium: boolean;
     login: (email: string, password: string) => Promise<void>;
     register: (userData: RegisterDto) => Promise<void>;
     logout: () => void;
     refreshToken: () => Promise<void>;
     checkAuth: () => void;
   }
```

2. Auth Service
   - login(email, password) - save token, set axios default header
   - register(userData)
   - logout() - clear token, redirect to home
   - refreshToken() - call refresh endpoint before token expires
   - getCurrentUser() - decode JWT to get user info

3. Axios Interceptor
   - Request interceptor: Add Authorization header
   - Response interceptor: Handle 401 errors, attempt token refresh, redirect to login if refresh fails

4. Protected Route Component
   - Check authentication state
   - Redirect to login if not authenticated
   - Show loading spinner while checking auth

5. Premium Content Guard Component
   - Check isPremium or subscription status
   - Show upgrade prompt for non-premium users
   - Allow access for premium users

6. Auth Context Provider (wrap App)
   - Initialize auth state on mount
   - Check token validity
   - Auto-refresh token before expiry

IMPLEMENT:
- Password hashing with BCrypt (backend)
- Token storage strategy (localStorage with XSS considerations or httpOnly cookies)
- CSRF protection if using cookies
- Persistent login (remember me)
- Auto-logout on token expiry
- Login/Register form validation
- Email verification flow
- Password reset flow with time-limited tokens
- Rate limiting on auth endpoints

Create login and register forms with proper error handling and UX feedback.
```

---

### **Prompt 6: Task System Implementation**
```
Create a comprehensive task verification and submission system:

TASK TYPES TO SUPPORT:

1. Quiz Type
   - Multiple choice questions stored in TaskData JSONB
   - Structure: { questions: [{ question: string, options: string[], correctAnswer: number }] }
   - Frontend: Display questions with radio buttons
   - Backend: Validate answers, calculate score
   - Auto-approve if score >= 80%

2. ExternalLink Type (Wallet Interaction)
   - TaskData: { walletAddress: string, action: string (e.g., "swap", "stake"), minAmount: number }
   - Frontend: Input field for transaction hash or wallet address
   - Backend: Store submission for manual review or integrate blockchain API for verification
   - Manual approval initially, API integration later

3. Screenshot Type
   - TaskData: { instructions: string, requiredElements: string[] }
   - Frontend: File upload (image only, max 5MB)
   - Backend: Store file (use local storage or free tier Cloudinary)
   - Manual approval by admin

4. TextSubmission Type
   - TaskData: { prompt: string, minWords: number }
   - Frontend: Textarea with word counter
   - Backend: Validate word count, store submission
   - Manual approval

5. WalletVerification Type
   - TaskData: { requiredToken: string, minBalance: number }
   - Frontend: Connect wallet button (MetaMask), display detected balance
   - Backend: Verify signature and balance (integrate Web3 library)
   - Auto-approve if conditions met

BACKEND IMPLEMENTATION:

1. TaskSubmissionService
   - ValidateSubmission(taskType, submissionData) - type-specific validation
   - ProcessSubmission(userId, taskId, submissionData)
     - Create UserTaskSubmission record with Pending status
     - If auto-approvable (Quiz, WalletVerification), validate and approve immediately
     - Else, queue for manual review
   - ApproveSubmission(submissionId, reviewerId, feedback)
     - Update status to Approved
     - Award reward points via RewardService
     - Send notification to user
   - RejectSubmission(submissionId, reviewerId, feedback)
     - Update status to Rejected
     - Send notification with feedback

2. Admin Review Interface Endpoint
   - GET /api/tasks/pending-reviews?type=Screenshot&page=1
     - Return submissions with user info, task details, submission data
     - Include preview for screenshots
   - PUT /api/tasks/submissions/{id}/review
     - Body: { approved: boolean, feedback: string }

FRONTEND IMPLEMENTATION:

1. TaskCard Component
   - Display task title, description, reward points
   - Show task type badge
   - Display user's submission status (Not Started, Pending, Approved, Rejected)
   - Lock tasks until previous required tasks are completed
   - "Start Task" button → opens TaskSubmissionModal

2. TaskSubmissionModal Component
   - Dynamic form based on task type
   - Quiz: Display questions one at a time or all at once
   - ExternalLink: Input field with validation (address format, hash format)
   - Screenshot: Drag-and-drop file upload with preview
   - TextSubmission: Textarea with real-time word count
   - WalletVerification: Integrate MetaMask connection
   - Submit button with loading state
   - Show validation errors
   - On success: Show success message with points earned, close modal, update task status

3. AdminReviewPage Component
   - Tabs for each task type
   - Table with columns: User, Task, Submitted At, Preview, Actions
   - Screenshot preview: Clickable thumbnail → fullscreen modal
   - Approve/Reject buttons with optional feedback textarea
   - Bulk actions (approve/reject multiple)

4. TaskProgressIndicator Component
   - Show completion status for each task in lesson
   - Visual progress bar for lesson tasks
   - Display total points earned vs available in lesson

IMPLEMENT:
- File upload handling (use multipart/form-data)
- Image compression before upload
- File type and size validation
- Prevent duplicate submissions (check existing Pending submission)
- Rate limiting on task submissions (prevent spam)
- Notification system for approval/rejection
- Points ledger integrity (prevent duplicate point awards)
- Transaction rollback if point award fails

CREATE SQL QUERIES:
- Get user's completed tasks for a lesson
- Get user's pending submissions
- Get admin review queue with filters
- Get leaderboard (top users by reward points)

Include loading states, error handling, and success feedback for all user actions.
```

---

### **Prompt 7: Reward & Discount System**
```
Create a complete reward points and discount redemption system:

REWARD SYSTEM FEATURES:

1. Point Earning Mechanisms
   - Complete lesson: Variable points based on lesson difficulty
   - Complete task: Points defined per task
   - Complete full course: Bonus points (sum of lesson points + 20% bonus)
   - Daily login streak: 5 points per day, up to 50 points (10-day streak)
   - Referral bonus: 100 points when referred user completes first course
   - Achievement badges: Unlock badges for milestones, earn bonus points

2. Point Redemption
   - Discount codes with point requirements
   - Example codes:
     - SAVE10: 10% off monthly, costs 500 points
     - SAVE20: 20% off monthly, costs 1000 points
     - SAVE30: 30% off yearly, costs 2000 points
     - FREE7DAYS: 7-day free premium trial, costs 200 points
   - One-time use per user per discount type
   - Expiry date on discount codes

3. Points Ledger (Immutable Transaction Log)
   - Every point change creates a RewardTransaction record
   - Types: Earned, Redeemed, Bonus, Penalty, Expired
   - Track reference (TaskId, CourseId, DiscountCodeId)
   - Current balance = SUM of all transaction amounts

BACKEND IMPLEMENTATION:

1. RewardService Methods
```csharp
   Task<int> GetUserPointsAsync(Guid userId);
   Task<RewardTransaction> AwardPointsAsync(Guid userId, int points, TransactionType type, string referenceId, string description);
   Task<RewardTransaction> DeductPointsAsync(Guid userId, int points, string reason);
   Task<List<RewardTransaction>> GetTransactionHistoryAsync(Guid userId, int page, int pageSize);
   Task<List<DiscountCode>> GetAvailableDiscountsAsync(int userPoints);
   Task<UserDiscountRedemption> RedeemDiscountAsync(Guid userId, Guid discountCodeId);
   Task<bool> HasRedeemedDiscountAsync(Guid userId, Guid discountCodeId);
   Task<List<LeaderboardEntry>> GetLeaderboardAsync(int topN, DateTime? startDate = null);
```

2. Point Award Triggers
   - On lesson completion: Award points after validation (watched 80%+)
   - On task approval: Award points in ReviewSubmissionAsync
   - On course completion: Check all lessons completed, award bonus
   - Daily login: Track last login date, compare to today
   - Referral: Award when referred user's first course completion

3. Discount Redemption Flow
   - Check user has enough points
   - Check discount hasn't been redeemed by user before
   - Check discount is active and not expired
   - Check max redemptions not exceeded
   - Create UserDiscountRedemption record
   - Deduct points (create negative RewardTransaction)
   - Return discount code to user
   - User applies code during checkout

4. Leaderboard Calculation
   - Cache leaderboard (refresh every hour)
   - Filter by time range (all-time, this month, this week)
   - Top 100 users by points
   - Include user rank even if not in top 100

FRONTEND IMPLEMENTATION:

1. RewardBalance Component (persistent header/sidebar)
   - Display current points with icon
   - Animated number on point changes
   - "+X points" toast notification on earning
   - Click to open rewards page

2. RewardsPage
   - Hero section with large points display
   - "Earn More Points" section (ways to earn)
   - Available discounts grid
     - Card for each discount: discount amount, required points, "Redeem" button
     - Disabled state if insufficient points or already redeemed
     - Tooltip showing points needed
   - Transaction history table (paginated)
     - Columns: Date, Description, Points, Balance After
     - Color-coded: Green for earned, Red for redeemed
   - Leaderboard section
     - Top 10 users with rank, username, points
     - "View Full Leaderboard" button → modal with full list
     - Highlight current user's rank

3. DiscountRedemptionModal
   - Confirm redemption dialog
   - Show discount code after redemption (copy button)
   - Instructions on how to apply during checkout
   - Automatically apply discount if user redeems during active checkout

4. PointAnimation Component
   - Floating "+X points" animation on point award
   - Confetti effect on major milestones (1000 points, 5000 points)

5. StreakTracker Component (dashboard)
   - Display current login streak
   - Calendar view with checkmarks
   - Next milestone progress bar

GAMIFICATION FEATURES:

1. Achievement Badges
   - Define badge types:
     - First Steps: Complete first lesson
     - Task Master: Complete 10 tasks
     - Course Conqueror: Complete first course
     - Knowledge Seeker: Complete 5 courses
     - Crypto Pro: Complete an advanced course
     - Point Hoarder: Reach 5000 points
     - Generous: Refer 3 friends
   - Store in UserAchievements table
   - Display on profile page
   - Award bonus points on unlock

2. Progress Milestones
   - Visual progress bars for achievements
   - Celebrate milestones with animations
   - Share achievements on social media (optional)

3. Daily Challenges (future enhancement)
   - Daily task: Watch 1 lesson, complete 1 task
   - Reward: 25 bonus points
   - Reset at midnight

IMPLEMENT:
- Atomic transactions for point operations
- Prevent negative balances
- Idempotency for point awards (prevent duplicate awards on API retry)
- Audit log for all point transactions
- Admin panel to manually adjust points (with reason)
- Email notifications for milestones
- Real-time points update with WebSockets (optional)

CREATE ENDPOINTS:
- GET /api/rewards/points
- GET /api/rewards/transactions
- GET /api/rewards/discounts
- POST /api/rewards/redeem/{discountCodeId}
- GET /api/rewards/leaderboard?range=month
- GET /api/rewards/achievements

Include analytics tracking for point earning patterns.
```

---

### **Prompt 8: Subscription & Payment Integration**
```
Create a subscription and payment system with Stripe integration (free tier):

SUBSCRIPTION TIERS:

1. Free Tier
   - Access to free courses only
   - Limited task submissions (5 per day)
   - Basic community access
   - Ads displayed (optional)

2. Monthly Premium ($9.99/month or your preferred price)
   - Access to all courses (free + premium)
   - Unlimited task submissions
   - Priority support
   - Ad-free experience
   - Early access to new courses

3. Yearly Premium ($99/year - 17% discount)
   - All monthly benefits
   - Exclusive advanced courses
   - 1-on-1 consultation call (1 per quarter)
   - Certificate of completion

BACKEND IMPLEMENTATION:

1. Stripe Setup
   - Install Stripe.NET NuGet package
   - Configure Stripe API keys in appsettings.json (use test keys initially)
   - Create Stripe webhook endpoint

2. SubscriptionService Methods
```csharp
   Task<List<SubscriptionPlan>> GetPlansAsync();
   Task<string> CreateCheckoutSessionAsync(Guid userId, string planId, string? discountCode = null);
   Task ProcessStripeWebhookAsync(string jsonPayload, string stripeSignature);
   Task<UserSubscription> GetUserSubscriptionAsync(Guid userId);
   Task<bool> IsUserPremiumAsync(Guid userId);
   Task CancelSubscriptionAsync(Guid userId);
   Task<bool> ApplyDiscountCodeAsync(CheckoutSession session, string discountCode);
```

3. Stripe Checkout Flow
   - User selects plan on frontend
   - Frontend calls POST /api/subscriptions/checkout
   - Backend creates Stripe Checkout Session
     - If discount code provided, validate and apply
     - Set success_url and cancel_url
     - Include metadata (userId, planId, discountCodeId)
   - Return checkout session URL to frontend
   - Frontend redirects to Stripe hosted checkout
   - User completes payment on Stripe
   - Stripe redirects back to success_url

4. Webhook Handling (CRITICAL)
   - POST /api/subscriptions/webhook
   - Verify webhook signature (prevent fake webhooks)
   - Handle events:
     - checkout.session.completed: Create/update subscription record, mark user as premium, send welcome email
     - invoice.paid: Extend subscription expiry date
     - invoice.payment_failed: Send payment failure notification
     - customer.subscription.deleted: Cancel subscription, revert to free tier
   - Return 200 OK

Prompt 8: Subscription & Payment Integration
5. Subscription Management
   - Store Stripe customer ID and subscription ID in User table
   - Track subscription status (Active, Cancelled, Expired, PaymentFailed)
   - Track subscription expiry date
   - Background job to check expired subscriptions daily
   - Downgrade expired premium users to free tier
   - Send expiry reminder emails (7 days, 1 day before expiry)

6. Discount Code Application
   - Validate discount code exists and is active
   - Check user has redeemed the discount (from rewards)
   - Check discount hasn't been used in subscription before
   - Apply discount percentage to Stripe Checkout Session
   - Mark discount as used in UserDiscountRedemptions

7. Subscription Cancellation
   - User cancels via dashboard
   - Cancel Stripe subscription (set cancel_at_period_end = true)
   - User retains premium until expiry date
   - Send cancellation confirmation email

FRONTEND IMPLEMENTATION:

1. PricingPage Component
   - Three-column layout (Free, Monthly, Yearly)
   - Feature comparison table
   - Highlight "Most Popular" or "Best Value"
   - "Current Plan" badge for authenticated users
   - "Upgrade" buttons → trigger checkout
   - Display discount if user has redeemed one

2. CheckoutFlow
```typescript
   const handleCheckout = async (planId: string) => {
     try {
       setLoading(true);
       const discountCode = userHasRedeemedDiscount ? discountCode : null;
       const response = await subscriptionService.createCheckout(planId, discountCode);
       // Redirect to Stripe Checkout
       window.location.href = response.checkoutUrl;
     } catch (error) {
       showToast('Checkout failed', 'error');
     } finally {
       setLoading(false);
     }
   };
```

3. SuccessPage Component (success_url)
   - Thank you message
   - Confirmation of subscription activation
   - "Start Learning" button → redirect to courses
   - Confetti animation

4. SubscriptionStatusCard (Dashboard)
   - Current plan name
   - Renewal date or "Cancelled" status
   - "Upgrade", "Cancel", or "Reactivate" button
   - Billing history link

5. BillingPage
   - Invoice history table (fetch from Stripe API)
   - Download invoice PDFs
   - Payment method management (Stripe Customer Portal)
   - Update billing details

6. PremiumContentGate Component
   - Wrap premium lessons/courses
   - Check user.isPremium
   - If not premium: Show upgrade prompt with benefits
   - "Unlock with Premium" button → redirect to pricing

7. DiscountApplicationFlow
   - On rewards page, after redeeming discount
   - Show modal: "Discount redeemed! Use code [CODE] at checkout"
   - Store discount code in session/state
   - Auto-apply during checkout
   - Display savings amount prominently

STRIPE TEST MODE SETUP:

1. Create Stripe account (free)
2. Use test API keys (pk_test_..., sk_test_...)
3. Test card numbers:
   - Success: 4242 4242 4242 4242
   - Declined: 4000 0000 0000 0002
   - 3D Secure: 4000 0025 0000 3155
4. Set up test webhook endpoint using Stripe CLI or ngrok for local testing

SECURITY CONSIDERATIONS:

- NEVER expose Stripe secret key in frontend
- Always verify webhook signatures
- Use HTTPS in production
- Store subscription status in database (don't rely solely on Stripe)
- Implement rate limiting on checkout endpoint
- Validate plan IDs server-side
- Log all payment events for audit

OPTIONAL FEATURES (Future):

- Promo codes (admin-created, not point-based)
- Gift subscriptions
- Team/group subscriptions
- Lifetime access option
- Cryptocurrency payment option (CoinBase Commerce, later phase)

IMPLEMENT:
- Subscription status middleware (check on premium content access)
- Grace period for failed payments (3 days)
- Email templates (welcome, payment success, payment failed, expiry reminder)
- Analytics tracking (conversion rates, churn rate)
- A/B testing for pricing

CREATE DATABASE MIGRATIONS:
- Add StripeCustomerId, StripeSubscriptionId to Users table
- Add SubscriptionPlans table (if storing in DB instead of hardcoding)
- Add Invoices table (optional, can fetch from Stripe)

Include comprehensive error messages and user guidance throughout the payment flow.
```

---

### **Prompt 9: Admin Dashboard & Content Management**
```
Create a comprehensive admin dashboard for managing the platform:

ADMIN ROLES & PERMISSIONS:

1. SuperAdmin
   - Full access to all features
   - User management (ban, delete, modify)
   - Financial reports
   - System settings

2. ContentCreator
   - Create/edit courses, lessons, tasks
   - Cannot manage users or view financial data

3. Moderator
   - Review task submissions
   - Respond to user reports
   - Cannot create content

BACKEND IMPLEMENTATION:

1. Admin Controllers

AdminUsersController:
- GET /api/admin/users (with filters: role, subscription, searchQuery, pagination)
- GET /api/admin/users/{id} (detailed user info with activity)
- PUT /api/admin/users/{id}/role (change user role)
- PUT /api/admin/users/{id}/subscription (manually grant/revoke premium)
- PUT /api/admin/users/{id}/ban (ban/unban user)
- DELETE /api/admin/users/{id} (soft delete)
- GET /api/admin/users/stats (total users, premium users, growth metrics)

AdminCoursesController:
- POST /api/admin/courses (create course with lessons and tasks in one request)
- PUT /api/admin/courses/{id}
- DELETE /api/admin/courses/{id}
- PUT /api/admin/courses/{id}/publish (publish/unpublish)
- POST /api/admin/courses/{id}/duplicate (clone course)
- GET /api/admin/courses/analytics (views, enrollments, completion rates)

AdminTasksController:
- GET /api/admin/tasks/submissions/pending (pending reviews queue)
- GET /api/admin/tasks/submissions/all (all submissions with filters)
- PUT /api/admin/tasks/submissions/{id}/review
- POST /api/admin/tasks/submissions/{id}/request-resubmit (ask for clarification)
- GET /api/admin/tasks/analytics (approval rates, avg review time)

AdminRewardsController:
- POST /api/admin/rewards/adjust (manually add/deduct points with reason)
- POST /api/admin/rewards/discount-codes (create new discount codes)
- GET /api/admin/rewards/analytics (points distribution, redemption rates)
- PUT /api/admin/rewards/discount-codes/{id} (edit discount)

AdminAnalyticsController:
- GET /api/admin/analytics/dashboard (KPIs: revenue, active users, course completions)
- GET /api/admin/analytics/revenue (revenue over time, MRR, ARR)
- GET /api/admin/analytics/engagement (DAU, MAU, retention cohorts)
- GET /api/admin/analytics/content-performance (top courses, completion rates)
- GET /api/admin/analytics/export (CSV export for reports)

2. Admin Authorization
   - [Authorize(Policy = "RequireAdmin")] on all admin controllers
   - Additional role-based checks in controller actions
   - Audit logging for all admin actions (who did what, when)

FRONTEND IMPLEMENTATION:

1. Admin Layout Component
   - Sidebar navigation:
     - Dashboard
     - Users
     - Courses
     - Tasks Review
     - Rewards & Discounts
     - Analytics
     - Settings
   - Top bar with admin name, logout
   - Breadcrumb navigation

2. AdminDashboard Page
   - KPI Cards:
     - Total Users (with growth %)
     - Premium Users (with conversion rate)
     - Monthly Revenue (with MRR growth)
     - Active Courses
     - Tasks Pending Review (clickable → tasks page)
     - Avg Course Completion Rate
   - Charts:
     - Revenue trend (line chart, last 12 months)
     - User signups (bar chart, last 30 days)
     - Course enrollments (pie chart, by category)
     - Task submission vs approval rate
   - Recent Activities feed (new users, course completions, reviews)
   - Quick Actions: Create Course, Review Tasks, Create Discount

3. AdminUsersPage
   - Search bar (by email, username)
   - Filters: Role, Subscription Tier, Status (Active/Banned)
   - Users table:
     - Columns: Avatar, Username, Email, Role, Subscription, Points, Joined Date, Actions
     - Actions dropdown: View Profile, Edit, Change Role, Ban/Unban, Delete
   - Pagination
   - Export to CSV button
   - "Add Admin User" button (if SuperAdmin)

4. UserDetailModal
   - User info (email, username, join date)
   - Subscription details
   - Reward points with transaction history
   - Enrolled courses with progress
   - Task submissions history
   - Admin notes textarea (internal notes)
   - Action buttons: Grant Premium, Adjust Points, Ban

5. AdminCoursesPage
   - "Create Course" button → CourseEditorModal
   - Courses table:
     - Columns: Thumbnail, Title, Category, Lessons, Enrollments, Completion %, Status, Actions
     - Actions: Edit, Duplicate, Publish/Unpublish, Analytics, Delete
   - Filters: Category, Status (Published/Draft), isPremium
   - Search by title

6. CourseEditor Component (Complex!)
   - Course basic info form:
     - Title, Description (rich text editor)
     - Category dropdown
     - Difficulty select
     - Thumbnail upload
     - isPremium toggle
     - Total reward points (auto-calculated)
   - Lessons section:
     - Drag-and-drop to reorder lessons
     - "Add Lesson" button → LessonForm
     - Each lesson card shows: Title, YouTube URL, Tasks count
     - Edit/Delete buttons
   - LessonForm:
     - Title, Description
     - YouTube video URL input (extract video ID)
     - Video duration (auto-fetch or manual input)
     - Lesson content (rich text editor)
     - Tasks section within lesson
       - "Add Task" button → TaskForm
       - Task list with type badges
   - TaskForm (Dynamic based on type):
     - Task type selector
     - Title, Description
     - Reward points input
     - Time limit (optional)
     - Type-specific fields:
       - Quiz: Add questions (question text, 4 options, correct answer)
       - ExternalLink: Instructions, required action
       - Screenshot: Instructions, what to capture
       - TextSubmission: Prompt, minimum words
       - WalletVerification: Token to check, minimum balance
   - Preview button (show course as student would see)
   - Save as Draft / Publish buttons

7. TasksReviewPage
   - Tabs: Pending, Approved, Rejected, All
   - Filters: Task Type, Date Range, Course/Lesson
   - Submissions table:
     - Columns: User, Task, Type, Submitted, Preview, Actions
     - Preview column:
       - Quiz: Show score
       - Screenshot: Thumbnail (click for fullscreen)
       - Text: Truncated text (click to expand)
       - ExternalLink: Link/hash with copy button
   - Bulk selection (checkboxes)
   - Bulk actions: Approve Selected, Reject Selected
   - Individual actions: Approve, Reject, Request Resubmit
   - ApprovalModal:
     - Show full submission details
     - Feedback textarea (optional)
     - Points to award (pre-filled, editable)
     - Approve / Reject buttons

8. RewardsManagementPage
   - Current Discount Codes table:
     - Columns: Code, Discount %, Required Points, Redeemed / Max, Expiry, Status, Actions
     - Actions: Edit, Deactivate
   - "Create Discount Code" button → DiscountCodeForm
   - DiscountCodeForm:
     - Code input (auto-generate option)
     - Discount percentage slider
     - Required points input
     - Max redemptions (0 = unlimited)
     - Expiry date picker
     - Applicable plans (Monthly, Yearly, Both)
   - Manual Point Adjustment section:
     - User search
     - Amount input (positive or negative)
     - Reason textarea
     - "Adjust Points" button
     - Confirmation dialog

9. AnalyticsPage
   - Date range picker (default: last 30 days)
   - Revenue Section:
     - MRR, ARR cards
     - Revenue by plan (pie chart)
     - Revenue trend (line chart)
     - Average order value
   - Users Section:
     - Total, Premium, Free counts
     - Conversion rate
     - Churn rate
     - User growth chart
     - Retention cohort table
   - Engagement Section:
     - DAU, MAU
     - Avg session duration
     - Most viewed courses (bar chart)
     - Course completion funnel
   - Content Section:
     - Top performing courses (table)
     - Category popularity (bar chart)
     - Task approval rate by type
     - Avg time to complete course
   - Export buttons for each section

10. SettingsPage (SuperAdmin only)
    - Platform settings:
      - Site name, logo upload
      - Default reward points per lesson
      - Max free tasks per day
      - Email notification settings
    - Payment settings:
      - Stripe keys (masked)
      - Subscription plan prices
      - Currency settings
    - Email templates editor
    - Maintenance mode toggle

IMPLEMENTATION DETAILS:

- Use React Query for data fetching and caching
- Implement optimistic updates (e.g., approve task → update UI immediately)
- Real-time updates for pending tasks count (WebSocket or polling)
- Confirmation dialogs for destructive actions
- Toast notifications for all actions
- Loading skeletons for tables
- Error boundaries for each admin section
- Responsive admin layout (mobile-friendly)
- Dark mode support

SECURITY:

- Check admin role on every request (backend)
- Hide admin routes from non-admin users (frontend)
- Rate limiting on admin actions
- Audit log all admin actions in database
- Two-factor authentication for admin accounts (future)

CREATE ENDPOINTS for analytics data:
- GET /api/admin/analytics/kpis
- GET /api/admin/analytics/revenue?startDate=...&endDate=...
- GET /api/admin/analytics/users/growth
- GET /api/admin/analytics/courses/performance

Include data visualization library (Recharts or Chart.js) for all charts.
```

---

### **Prompt 10: YouTube Video Integration & Progress Tracking**
```
Create a robust YouTube video player integration with progress tracking and resume functionality:

REQUIREMENTS:

1. Video Player Features
   - Embed YouTube videos using react-player library
   - Custom controls overlay (optional, can use YouTube's default)
   - Playback speed control
   - Quality selection
   - Fullscreen support
   - Keyboard shortcuts (Space: play/pause, F: fullscreen, Arrow keys: seek)
   - Prevent video URL manipulation (validate access before rendering)

2. Progress Tracking
   - Track watch position every 10 seconds
   - Save to backend automatically
   - Resume from last position on return
   - Mark lesson as completed when watched 80%+ (configurable threshold)
   - Prevent gaming (skip detection)
   - Handle page refresh/navigation gracefully

3. Access Control
   - Check user enrollment before loading video
   - Check premium status for premium lessons
   - Show upgrade prompt if not premium
   - Validate video ID exists and is accessible

BACKEND IMPLEMENTATION:

1. UserProgress Entity (already defined, additional methods)
```csharp
   public class UserProgress
   {
       public Guid Id { get; set; }
       public Guid UserId { get; set; }
       public Guid LessonId { get; set; }
       public int LastWatchedPosition { get; set; } // in seconds
       public decimal CompletionPercentage { get; set; }
       public bool IsCompleted { get; set; }
       public DateTime? CompletedAt { get; set; }
       public bool RewardPointsClaimed { get; set; }
       public DateTime LastUpdatedAt { get; set; }
       
       // Navigation properties
       public User User { get; set; }
       public Lesson Lesson { get; set; }
   }
```

2. LessonService Methods
```csharp
   Task<UserProgress> GetOrCreateProgressAsync(Guid userId, Guid lessonId);
   Task UpdateProgressAsync(Guid userId, Guid lessonId, int watchPosition);
   Task<bool> CompleteLessonAsync(Guid userId, Guid lessonId);
   Task<bool> ValidateLessonAccessAsync(Guid userId, Guid lessonId);
   Task<int> GetLastWatchedPositionAsync(Guid userId, Guid lessonId);
```

3. Progress Update Endpoint
   - PUT /api/lessons/{lessonId}/progress
   - Body: { watchPosition: number (seconds) }
   - Calculate completion percentage: (watchPosition / totalDuration) * 100
   - Update UserProgress record
   - If completion >= 80% and not completed: Mark as completed, award points
   - Return: { success: bool, completionPercentage: number, pointsAwarded: number }

4. Lesson Completion Logic
```csharp
   public async Task<bool> CompleteLessonAsync(Guid userId, Guid lessonId)
   {
       var progress = await GetOrCreateProgressAsync(userId, lessonId);
       var lesson = await _lessonRepository.GetByIdAsync(lessonId);
       
       // Check completion threshold (80%)
       if (progress.CompletionPercentage < 80)
           return false;
       
       if (progress.IsCompleted && progress.RewardPointsClaimed)
           return true; // Already completed
       
       progress.IsCompleted = true;
       progress.CompletedAt = DateTime.UtcNow;
       
       // Award points if not claimed
       if (!progress.RewardPointsClaimed)
       {
           await _rewardService.AwardPointsAsync(
               userId,
               lesson.RewardPoints,
               TransactionType.Earned,
               lessonId.ToString(),
               $"Completed lesson: {lesson.Title}"
           );
           progress.RewardPointsClaimed = true;
       }
       
       await _progressRepository.UpdateAsync(progress);
       
       // Check if course is completed
       await CheckCourseCompletionAsync(userId, lesson.CourseId);
       
       return true;
   }
   
   private async Task CheckCourseCompletionAsync(Guid userId, Guid courseId)
   {
       var course = await _courseRepository.GetWithLessonsAsync(courseId);
       var userProgress = await _progressRepository.GetByCourseAsync(userId, courseId);
       
       var totalLessons = course.Lessons.Count;
       var completedLessons = userProgress.Count(p => p.IsCompleted);
       
       if (completedLessons == totalLessons)
       {
           // Award course completion bonus
           var bonusPoints = (int)(course.Lessons.Sum(l => l.RewardPoints) * 0.2); // 20% bonus
           await _rewardService.AwardPointsAsync(
               userId,
               bonusPoints,
               TransactionType.Bonus,
               courseId.ToString(),
               $"Completed course: {course.Title}"
           );
           
           // Update enrollment
           var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(userId, courseId);
           enrollment.IsCompleted = true;
           enrollment.CompletedAt = DateTime.UtcNow;
           await _enrollmentRepository.UpdateAsync(enrollment);
       }
   }
```

FRONTEND IMPLEMENTATION:

1. LessonPlayer Component
```typescript
   import ReactPlayer from 'react-player/youtube';
   import { useState, useEffect, useRef } from 'react';
   
   interface LessonPlayerProps {
     lesson: Lesson;
     userId: string;
   }
   
   export const LessonPlayer: React.FC<LessonPlayerProps> = ({ lesson, userId }) => {
     const playerRef = useRef<ReactPlayer>(null);
     const [playing, setPlaying] = useState(false);
     const [progress, setProgress] = useState(0);
     const [lastSavedPosition, setLastSavedPosition] = useState(0);
     const [isCompleted, setIsCompleted] = useState(false);
     const saveIntervalRef = useRef<NodeJS.Timeout | null>(null);
     
     // Load saved progress on mount
     useEffect(() => {
       const loadProgress = async () => {
         try {
           const savedProgress = await lessonService.getProgress(lesson.id);
           if (savedProgress) {
             setLastSavedPosition(savedProgress.lastWatchedPosition);
             setProgress(savedProgress.completionPercentage);
             setIsCompleted(savedProgress.isCompleted);
             
             // Seek to last position after player is ready
             if (playerRef.current && savedProgress.lastWatchedPosition > 0) {
               playerRef.current.seekTo(savedProgress.lastWatchedPosition, 'seconds');
             }
           }
         } catch (error) {
           console.error('Failed to load progress:', error);
         }
       };
       
       loadProgress();
     }, [lesson.id]);
     
     // Auto-save progress every 10 seconds
     useEffect(() => {
       if (playing) {
         saveIntervalRef.current = setInterval(() => {
           saveProgress();
         }, 10000);
       } else {
         if (saveIntervalRef.current) {
           clearInterval(saveIntervalRef.current);
         }
       }
       
       return () => {
         if (saveIntervalRef.current) {
           clearInterval(saveIntervalRef.current);
         }
       };
     }, [playing]);
     
     // Save progress on unmount
     useEffect(() => {
       return () => {
         saveProgress();
       };
     }, []);
     
     const saveProgress = async () => {
       if (!playerRef.current) return;
       
       const currentTime = playerRef.current.getCurrentTime();
       const duration = playerRef.current.getDuration();
       
       if (!currentTime || !duration) return;
       
       try {
         const response = await lessonService.updateProgress(lesson.id, {
           watchPosition: Math.floor(currentTime)
         });
         
         setLastSavedPosition(Math.floor(currentTime));
         setProgress(response.completionPercentage);
         
         // Check if just completed
         if (response.pointsAwarded > 0 && !isCompleted) {
           setIsCompleted(true);
           showToast(`Lesson completed! +${response.pointsAwarded} points`, 'success');
           // Trigger confetti or celebration animation
         }
       } catch (error) {
         console.error('Failed to save progress:', error);
       }
     };
     
     const handleProgress = (state: { playedSeconds: number; played: number }) => {
       const newProgress = state.played * 100;
       setProgress(newProgress);
     };
     
     const handleEnded = () => {
       setPlaying(false);
       saveProgress();
     };
     
     return (
       <div className="lesson-player-container">
         <div className="relative aspect-video bg-black rounded-lg overflow-hidden">
           <ReactPlayer
             ref={playerRef}
             url={`https://www.youtube.com/watch?v=${lesson.youtubeVideoId}`}
             width="100%"
             height="100%"
             playing={playing}
             controls
             onPlay={() => setPlaying(true)}
             onPause={() => {
               setPlaying(false);
               saveProgress();
             }}
             onProgress={handleProgress}
             onEnded={handleEnded}
             config={{
               youtube: {
                 playerVars: {
                   modestbranding: 1,
                   rel: 0
                 }
               }
             }}
           />
           
           {/* Progress indicator overlay */}
           <div className="absolute bottom-0 left-0 right-0 h-1 bg-gray-700">
             <div 
               className="h-full bg-green-500 transition-all"
               style={{ width: `${progress}%` }}
             />
           </div>
           
           {/* Completion badge */}
           {isCompleted && (
             <div className="absolute top-4 right-4 bg-green-500 text-white px-3 py-1 rounded-full text-sm font-semibold flex items-center gap-2">
               <CheckCircle size={16} />
               Completed
             </div>
           )}
         </div>
         
         {/* Resume prompt */}
         {lastSavedPosition > 30 && !playing && (
           <div className="mt-4 p-4 bg-blue-50 dark:bg-blue-900 rounded-lg">
             <p className="text-sm text-blue-800 dark:text-blue-200">
               Resume from {formatTime(lastSavedPosition)}?
             </p>
             <button
               onClick={() => {
                 playerRef.current?.seekTo(lastSavedPosition, 'seconds');
                 setPlaying(true);
               }}
               className="mt-2 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
             >
               Resume
             </button>
           </div>
         )}
       </div>
     );
   };
```

2. LessonPage Component
```typescript
   export const LessonPage: React.FC = () => {
     const { lessonId } = useParams();
     const [lesson, setLesson] = useState<Lesson | null>(null);
     const [hasAccess, setHasAccess] = useState(false);
     const [loading, setLoading] = useState(true);
     const { user } = useAuthStore();
     
     useEffect(() => {
       const loadLesson = async () => {
         try {
           const lessonData = await lessonService.getLesson(lessonId!);
           setLesson(lessonData);
           
           // Check access
           if (lessonData.isPremium && !user.isPremium) {
             setHasAccess(false);
           } else {
             // Check enrollment
             const access = await courseService.checkEnrollment(lessonData.courseId);
             setHasAccess(access);
           }
         } catch (error) {
           showToast('Failed to load lesson', 'error');
         } finally {
           setLoading(false);
         }
       };
       
       loadLesson();
     }, [lessonId]);
     
     if (loading) return <LoadingSpinner />;
     if (!lesson) return <NotFound />;
     
     if (!hasAccess) {
       return (
         <PremiumGate
           message="This is a premium lesson"
           ctaText="Upgrade to Premium"
           ctaLink="/pricing"
         />
       );
     }
     
     return (
       <div className="max-w-6xl mx-auto p-6">
         {/* Breadcrumb */}
         <Breadcrumb
           items={[
             { label: 'Courses', link: '/courses' },
             { label: lesson.course.title, link: `/courses/${lesson.courseId}` },
             { label: lesson.title }
           ]}
         />
         
         {/* Lesson Header */}
         <div className="mt-6">
           <h1 className="text-3xl font-bold">{lesson.title}</h1>
           <p className="mt-2 text-gray-600">{lesson.description}</p>
           <div className="mt-4 flex items-center gap-4">
             <span className="text-sm text-gray-500">
               {lesson.duration} minutes
             </span>
             <span className="text-sm font-semibold text-green-600">
               +{lesson.rewardPoints} points
             </span>
           </div>
         </div>
         
         {/* Video Player */}
         <div className="mt-8">
           <LessonPlayer lesson={lesson} userId={user.id} />
         </div>
         
         {/* Lesson Content */}
         {lesson.contentMarkdown && (
           <div className="mt-8 prose dark:prose-invert max-w-none">
             <h2>Lesson Notes</h2>
             <ReactMarkdown>{lesson.contentMarkdown}</ReactMarkdown>
           </div>
         )}
         
         {/* Tasks Section */}
         <div className="mt-12">
           <h2 className="text-2xl font-bold mb-6">Practice Tasks</h2>
           <TasksList lessonId={lesson.id} />
         </div>
         
         {/* Navigation */}
         <div className="mt-12 flex justify-between">
           {lesson.previousLessonId && (
             <Link to={`/lessons/${lesson.previousLessonId}`}>
               <button className="flex items-center gap-2 px-6 py-3 border rounded-lg hover:bg-gray-50">
                 <ChevronLeft /> Previous Lesson
               </button>
             </Link>
           )}
           {lesson.nextLessonId && (
             <Link to={`/lessons/${lesson.nextLessonId}`} className="ml-auto">
               <button className="flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700">
                 Next Lesson <ChevronRight />
               </button>
             </Link>
           )}
         </div>
       </div>
     );
   };
```

3. Skip Detection (Optional Advanced Feature)
```typescript
   // Track playback events to detect skipping
   const [playbackEvents, setPlaybackEvents] = useState<Array<{ time: number, action: string }>>([]);
   
   const handleSeek = (seconds: number) => {
     const currentTime = playerRef.current?.getCurrentTime() || 0;
     const skipAmount = Math.abs(seconds - currentTime);
     
     if (skipAmount > 30) {
       // Large skip detected
       setPlaybackEvents(prev => [...prev, { time: Date.now(), action: 'skip' }]);
       
       // Optional: Reduce completion percentage or show warning
       showToast('Please watch the full lesson to earn points', 'warning');
     }
   };
```

IMPLEMENTATION NOTES:

- Use React Player's onReady callback to seek to saved position
- Debounce progress saves to avoid excessive API calls
- Store progress in IndexedDB/localStorage as backup (sync on connection)
- Handle network errors gracefully (queue progress updates)
- Show visual feedback for auto-save (small indicator)
- Prevent simultaneous playback in multiple tabs (optional)
- Add analytics: Track watch time, completion rate, drop-off points

SECURITY:

- Validate lesson access on every API call
- Don't expose complete lesson data if user doesn't have access
- Rate limit progress update endpoint
- Verify YouTube video IDs are valid before storing
- Prevent XSS in lesson content (sanitize markdown)

Include comprehensive error handling and loading states throughout the video player experience.
```

---

## 📚 Complete Project Setup Instructions

### **Prompt 11: Project Initialization & Setup**
```
CREATE STEP-BY-STEP SETUP INSTRUCTIONS:

BACKEND SETUP (.NET):

1. Create Solution Structure
```bash
   dotnet new sln -n WahadiniCryptoQuestPlatform
   dotnet new webapi -n WahadiniCryptoQuest.API
   dotnet new classlib -n WahadiniCryptoQuest.Domain
   dotnet new classlib -n WahadiniCryptoQuest.Application
   dotnet new classlib -n WahadiniCryptoQuest.Infrastructure
   
   dotnet sln add WahadiniCryptoQuest.API
   dotnet sln add WahadiniCryptoQuest.Domain
   dotnet sln add WahadiniCryptoQuest.Application
   dotnet sln add WahadiniCryptoQuest.Infrastructure
   
   # Add project references
   cd WahadiniCryptoQuest.API
   dotnet add reference ../WahadiniCryptoQuest.Application
   dotnet add reference ../WahadiniCryptoQuest.Infrastructure
   
   cd ../WahadiniCryptoQuest.Application
   dotnet add reference ../WahadiniCryptoQuest.Domain

   cd ../WahadiniCryptoQuest.Infrastructure
   dotnet add reference ../WahadiniCryptoQuest.Domain
   dotnet add reference ../WahadiniCryptoQuest.Application

Install Required NuGet Packages

bash   # In WahadiniCryptoQuest.API
   dotnet add package Microsoft.EntityFrameworkCore.Design
   dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
   dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
   dotnet add package Swashbuckle.AspNetCore
   dotnet add package Serilog.AspNetCore
   dotnet add package Stripe.net
   dotnet add package FluentValidation.AspNetCore
   dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
   
   # In WahadiniCryptoQuest.Infrastructure
   dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
   dotnet add package Microsoft.EntityFrameworkCore.Tools
   dotnet add package Dapper
   dotnet add package MailKit
   
   # In WahadiniCryptoQuest.Application
   dotnet add package FluentValidation
   dotnet add package AutoMapper

appsettings.json Configuration

json   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=WahadiniCryptoQuest;Username=postgres;Password=yourpassword"
     },
     "JwtSettings": {
       "SecretKey": "your-super-secret-key-min-32-chars-long-change-in-production",
       "Issuer": "WahadiniCryptoQuestAPI",
       "Audience": "WahadiniCryptoQuestClient",
       "ExpirationMinutes": 60,
       "RefreshTokenExpirationDays": 7
     },
     "StripeSettings": {
       "SecretKey": "sk_test_your_stripe_secret_key",
       "PublishableKey": "pk_test_your_stripe_publishable_key",
       "WebhookSecret": "whsec_your_webhook_secret",
       "MonthlyPriceId": "price_monthly_id",
       "YearlyPriceId": "price_yearly_id"
     },
     "EmailSettings": {
       "SmtpHost": "smtp.gmail.com",
       "SmtpPort": 587,
       "SenderEmail": "noreply@WahadiniCryptoQuest.com",
       "SenderName": "WahadiniCryptoQuest",
       "Username": "your-email@gmail.com",
       "Password": "your-app-password"
     },
     "AppSettings": {
       "FrontendUrl": "http://localhost:5173",
       "MaxFreeTasksPerDay": 5,
       "CompletionThresholdPercentage": 80,
       "CourseCompletionBonusPercentage": 20
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*"
   }

Database Migration Commands

bash   # From WahadiniCryptoQuest.API directory
   dotnet ef migrations add InitialCreate --project ../WahadiniCryptoQuest.Infrastructure --startup-project .
   dotnet ef database update --project ../WahadiniCryptoQuest.Infrastructure --startup-project .
   
   # Add seed data migration
   dotnet ef migrations add SeedInitialData --project ../WahadiniCryptoQuest.Infrastructure --startup-project .
   dotnet ef database update --project ../WahadiniCryptoQuest.Infrastructure --startup-project .

Program.cs Complete Configuration

csharp   using WahadiniCryptoQuest.Infrastructure.Data;
   using Microsoft.EntityFrameworkCore;
   using Microsoft.AspNetCore.Authentication.JwtBearer;
   using Microsoft.IdentityModel.Tokens;
   using System.Text;
   using Microsoft.OpenApi.Models;
   using WahadiniCryptoQuest.Application.Interfaces;
   using WahadiniCryptoQuest.Application.Services;
   using WahadiniCryptoQuest.Infrastructure.Repositories;
   using Serilog;
   
   var builder = WebApplication.CreateBuilder(args);
   
   // Configure Serilog
   Log.Logger = new LoggerConfiguration()
       .ReadFrom.Configuration(builder.Configuration)
       .CreateLogger();
   
   builder.Host.UseSerilog();
   
   // Database
   builder.Services.AddDbContext<ApplicationDbContext>(options =>
       options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
   
   // JWT Authentication
   var jwtSettings = builder.Configuration.GetSection("JwtSettings");
   var secretKey = jwtSettings["SecretKey"];
   
   builder.Services.AddAuthentication(options =>
   {
       options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
       options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
   })
   .AddJwtBearer(options =>
   {
       options.TokenValidationParameters = new TokenValidationParameters
       {
           ValidateIssuer = true,
           ValidateAudience = true,
           ValidateLifetime = true,
           ValidateIssuerSigningKey = true,
           ValidIssuer = jwtSettings["Issuer"],
           ValidAudience = jwtSettings["Audience"],
           IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
       };
   });
   
   // Authorization Policies
   builder.Services.AddAuthorization(options =>
   {
       options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
       options.AddPolicy("RequirePremium", policy => 
           policy.RequireAssertion(context => 
               context.User.HasClaim(c => c.Type == "SubscriptionTier" && 
                   (c.Value == "Monthly" || c.Value == "Yearly"))));
   });
   
   // CORS
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("AllowFrontend", policy =>
       {
           policy.WithOrigins(builder.Configuration["AppSettings:FrontendUrl"])
                 .AllowAnyMethod()
                 .AllowAnyHeader()
                 .AllowCredentials();
       });
   });
   
   // Repositories
   builder.Services.AddScoped<IUserRepository, UserRepository>();
   builder.Services.AddScoped<ICourseRepository, CourseRepository>();
   builder.Services.AddScoped<ILessonRepository, LessonRepository>();
   builder.Services.AddScoped<ITaskRepository, TaskRepository>();
   builder.Services.AddScoped<IRewardRepository, RewardRepository>();
   
   // Services
   builder.Services.AddScoped<IAuthService, AuthService>();
   builder.Services.AddScoped<ICourseService, CourseService>();
   builder.Services.AddScoped<ILessonService, LessonService>();
   builder.Services.AddScoped<ITaskService, TaskService>();
   builder.Services.AddScoped<IRewardService, RewardService>();
   builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
   builder.Services.AddScoped<INotificationService, NotificationService>();
   
   // AutoMapper
   builder.Services.AddAutoMapper(typeof(Program));
   
   // Controllers
   builder.Services.AddControllers();
   
   // Swagger
   builder.Services.AddEndpointsApiExplorer();
   builder.Services.AddSwaggerGen(c =>
   {
       c.SwaggerDoc("v1", new OpenApiInfo { Title = "WahadiniCryptoQuest API", Version = "v1" });
       c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
       {
           Description = "JWT Authorization header using the Bearer scheme.",
           Name = "Authorization",
           In = ParameterLocation.Header,
           Type = SecuritySchemeType.ApiKey,
           Scheme = "Bearer"
       });
       c.AddSecurityRequirement(new OpenApiSecurityRequirement
       {
           {
               new OpenApiSecurityScheme
               {
                   Reference = new OpenApiReference
                   {
                       Type = ReferenceType.SecurityScheme,
                       Id = "Bearer"
                   }
               },
               Array.Empty<string>()
           }
       });
   });
   
   var app = builder.Build();
   
   // Middleware
   if (app.Environment.IsDevelopment())
   {
       app.UseSwagger();
       app.UseSwaggerUI();
   }
   
   app.UseHttpsRedirection();
   app.UseCors("AllowFrontend");
   app.UseAuthentication();
   app.UseAuthorization();
   app.MapControllers();
   
   // Seed database
   using (var scope = app.Services.CreateScope())
   {
       var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
       await DbInitializer.SeedAsync(context);
   }
   
   app.Run();
FRONTEND SETUP (React + Vite):

Create React Project

bash   npm create vite@latest WahadiniCryptoQuest-frontend -- --template react-ts
   cd WahadiniCryptoQuest-frontend

Install Dependencies

bash   npm install react-router-dom axios zustand @tanstack/react-query
   npm install react-player react-hook-form zod @hookform/resolvers
   npm install lucide-react recharts
   npm install -D tailwindcss postcss autoprefixer
   npx tailwindcss init -p

Tailwind Configuration (tailwind.config.js)

javascript   /** @type {import('tailwindcss').Config} */
   export default {
     content: [
       "./index.html",
       "./src/**/*.{js,ts,jsx,tsx}",
     ],
     theme: {
       extend: {
         colors: {
           primary: {
             50: '#eff6ff',
             100: '#dbeafe',
             200: '#bfdbfe',
             300: '#93c5fd',
             400: '#60a5fa',
             500: '#3b82f6',
             600: '#2563eb',
             700: '#1d4ed8',
             800: '#1e40af',
             900: '#1e3a8a',
           },
           success: {
             500: '#10b981',
             600: '#059669',
           },
           warning: {
             500: '#f59e0b',
             600: '#d97706',
           },
           danger: {
             500: '#ef4444',
             600: '#dc2626',
           }
         }
       },
     },
     plugins: [],
     darkMode: 'class',
   }
```

4. Project Structure
```
   src/
   ├── assets/
   ├── components/
   │   ├── common/
   │   │   ├── Button.tsx
   │   │   ├── Card.tsx
   │   │   ├── Input.tsx
   │   │   ├── Modal.tsx
   │   │   ├── LoadingSpinner.tsx
   │   │   └── Toast.tsx
   │   ├── layout/
   │   │   ├── Navbar.tsx
   │   │   ├── Footer.tsx
   │   │   ├── Sidebar.tsx
   │   │   └── Layout.tsx
   │   ├── auth/
   │   │   ├── LoginForm.tsx
   │   │   ├── RegisterForm.tsx
   │   │   └── ProtectedRoute.tsx
   │   ├── course/
   │   │   ├── CourseCard.tsx
   │   │   ├── CourseDetail.tsx
   │   │   ├── CourseFilters.tsx
   │   │   └── LessonList.tsx
   │   ├── lesson/
   │   │   ├── LessonPlayer.tsx
   │   │   └── ProgressBar.tsx
   │   ├── task/
   │   │   ├── TaskCard.tsx
   │   │   ├── TaskSubmissionModal.tsx
   │   │   ├── QuizTask.tsx
   │   │   ├── ScreenshotTask.tsx
   │   │   └── TextSubmissionTask.tsx
   │   └── reward/
   │       ├── RewardBalance.tsx
   │       ├── LeaderboardTable.tsx
   │       └── DiscountCard.tsx
   ├── pages/
   │   ├── HomePage.tsx
   │   ├── CoursesPage.tsx
   │   ├── CourseDetailPage.tsx
   │   ├── LessonPage.tsx
   │   ├── DashboardPage.tsx
   │   ├── ProfilePage.tsx
   │   ├── RewardsPage.tsx
   │   ├── PricingPage.tsx
   │   ├── LoginPage.tsx
   │   ├── RegisterPage.tsx
   │   └── admin/
   │       ├── AdminDashboard.tsx
   │       ├── AdminUsers.tsx
   │       ├── AdminCourses.tsx
   │       └── AdminTasks.tsx
   ├── services/
   │   ├── api.ts
   │   ├── authService.ts
   │   ├── courseService.ts
   │   ├── lessonService.ts
   │   ├── taskService.ts
   │   ├── rewardService.ts
   │   └── subscriptionService.ts
   ├── stores/
   │   ├── authStore.ts
   │   ├── courseStore.ts
   │   ├── rewardStore.ts
   │   └── uiStore.ts
   ├── hooks/
   │   ├── useAuth.ts
   │   ├── useCourses.ts
   │   ├── useRewards.ts
   │   └── useToast.ts
   ├── types/
   │   ├── user.types.ts
   │   ├── course.types.ts
   │   ├── lesson.types.ts
   │   ├── task.types.ts
   │   └── reward.types.ts
   ├── utils/
   │   ├── constants.ts
   │   ├── formatters.ts
   │   ├── validators.ts
   │   └── helpers.ts
   ├── App.tsx
   ├── main.tsx
   ├── routes.tsx
   └── index.css

Environment Variables (.env)

env   VITE_API_URL=http://localhost:5000/api
   VITE_STRIPE_PUBLISHABLE_KEY=pk_test_your_stripe_key
   VITE_APP_NAME=WahadiniCryptoQuest

API Service Setup (src/services/api.ts)

typescript   import axios from 'axios';
   
   const api = axios.create({
     baseURL: import.meta.env.VITE_API_URL,
     headers: {
       'Content-Type': 'application/json',
     },
   });
   
   // Request interceptor
   api.interceptors.request.use(
     (config) => {
       const token = localStorage.getItem('token');
       if (token) {
         config.headers.Authorization = `Bearer ${token}`;
       }
       return config;
     },
     (error) => Promise.reject(error)
   );
   
   // Response interceptor
   api.interceptors.response.use(
     (response) => response,
     async (error) => {
       const originalRequest = error.config;
       
       if (error.response?.status === 401 && !originalRequest._retry) {
         originalRequest._retry = true;
         
         try {
           // Attempt token refresh
           const refreshToken = localStorage.getItem('refreshToken');
           const response = await axios.post(
             `${import.meta.env.VITE_API_URL}/auth/refresh-token`,
             { refreshToken }
           );
           
           const { token } = response.data;
           localStorage.setItem('token', token);
           originalRequest.headers.Authorization = `Bearer ${token}`;
           
           return api(originalRequest);
         } catch (refreshError) {
           // Refresh failed, redirect to login
           localStorage.removeItem('token');
           localStorage.removeItem('refreshToken');
           window.location.href = '/login';
           return Promise.reject(refreshError);
         }
       }
       
       return Promise.reject(error);
     }
   );
   
   export default api;

React Query Setup (src/main.tsx)

typescript   import React from 'react';
   import ReactDOM from 'react-dom/client';
   import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
   import App from './App';
   import './index.css';
   
   const queryClient = new QueryClient({
     defaultOptions: {
       queries: {
         refetchOnWindowFocus: false,
         retry: 1,
         staleTime: 5 * 60 * 1000, // 5 minutes
       },
     },
   });
   
   ReactDOM.createRoot(document.getElementById('root')!).render(
     <React.StrictMode>
       <QueryClientProvider client={queryClient}>
         <App />
       </QueryClientProvider>
     </React.StrictMode>
   );

Routing Setup (src/App.tsx)

typescript   import { BrowserRouter, Routes, Route } from 'react-router-dom';
   import Layout from './components/layout/Layout';
   import ProtectedRoute from './components/auth/ProtectedRoute';
   import HomePage from './pages/HomePage';
   import CoursesPage from './pages/CoursesPage';
   import CourseDetailPage from './pages/CourseDetailPage';
   import LessonPage from './pages/LessonPage';
   import DashboardPage from './pages/DashboardPage';
   import ProfilePage from './pages/ProfilePage';
   import RewardsPage from './pages/RewardsPage';
   import PricingPage from './pages/PricingPage';
   import LoginPage from './pages/LoginPage';
   import RegisterPage from './pages/RegisterPage';
   import AdminDashboard from './pages/admin/AdminDashboard';
   
   function App() {
     return (
       <BrowserRouter>
         <Routes>
           <Route path="/" element={<Layout />}>
             <Route index element={<HomePage />} />
             <Route path="courses" element={<CoursesPage />} />
             <Route path="courses/:id" element={<CourseDetailPage />} />
             <Route path="lessons/:id" element={
               <ProtectedRoute>
                 <LessonPage />
               </ProtectedRoute>
             } />
             <Route path="dashboard" element={
               <ProtectedRoute>
                 <DashboardPage />
               </ProtectedRoute>
             } />
             <Route path="profile" element={
               <ProtectedRoute>
                 <ProfilePage />
               </ProtectedRoute>
             } />
             <Route path="rewards" element={
               <ProtectedRoute>
                 <RewardsPage />
               </ProtectedRoute>
             } />
             <Route path="pricing" element={<PricingPage />} />
             <Route path="login" element={<LoginPage />} />
             <Route path="register" element={<RegisterPage />} />
             <Route path="admin/*" element={
               <ProtectedRoute requireAdmin>
                 <AdminDashboard />
               </ProtectedRoute>
             } />
           </Route>
         </Routes>
       </BrowserRouter>
     );
   }
   
   export default App;
DOCKER SETUP (Optional but recommended):

Backend Dockerfile (WahadiniCryptoQuest.API/)

dockerfile   FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
   WORKDIR /src
   
   COPY ["WahadiniCryptoQuest.API/WahadiniCryptoQuest.API.csproj", "WahadiniCryptoQuest.API/"]
   COPY ["WahadiniCryptoQuest.Application/WahadiniCryptoQuest.Application.csproj", "WahadiniCryptoQuest.Application/"]
   COPY ["WahadiniCryptoQuest.Domain/WahadiniCryptoQuest.Domain.csproj", "WahadiniCryptoQuest.Domain/"]
   COPY ["WahadiniCryptoQuest.Infrastructure/WahadiniCryptoQuest.Infrastructure.csproj", "WahadiniCryptoQuest.Infrastructure/"]
   
   RUN dotnet restore "WahadiniCryptoQuest.API/WahadiniCryptoQuest.API.csproj"
   
   COPY . .
   WORKDIR "/src/WahadiniCryptoQuest.API"
   RUN dotnet build "WahadiniCryptoQuest.API.csproj" -c Release -o /app/build
   
   FROM build AS publish
   RUN dotnet publish "WahadiniCryptoQuest.API.csproj" -c Release -o /app/publish
   
   FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
   WORKDIR /app
   COPY --from=publish /app/publish .
   ENTRYPOINT ["dotnet", "WahadiniCryptoQuest.API.dll"]

Frontend Dockerfile (WahadiniCryptoQuest-frontend/)

dockerfile   FROM node:18-alpine AS build
   WORKDIR /app
   
   COPY package*.json ./
   RUN npm ci
   
   COPY . .
   RUN npm run build
   
   FROM nginx:alpine
   COPY --from=build /app/dist /usr/share/nginx/html
   COPY nginx.conf /etc/nginx/conf.d/default.conf
   EXPOSE 80
   CMD ["nginx", "-g", "daemon off;"]

docker-compose.yml (root directory)

yaml   version: '3.8'
   
   services:
     postgres:
       image: postgres:15-alpine
       environment:
         POSTGRES_DB: WahadiniCryptoQuest
         POSTGRES_USER: postgres
         POSTGRES_PASSWORD: yourpassword
       ports:
         - "5432:5432"
       volumes:
         - postgres_data:/var/lib/postgresql/data
   
     backend:
       build:
         context: .
         dockerfile: WahadiniCryptoQuest.API/Dockerfile
       ports:
         - "5000:80"
       environment:
         - ASPNETCORE_ENVIRONMENT=Development
         - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=WahadiniCryptoQuest;Username=postgres;Password=yourpassword
       depends_on:
         - postgres
   
     frontend:
       build:
         context: ./WahadiniCryptoQuest-frontend
         dockerfile: Dockerfile
       ports:
         - "3000:80"
       depends_on:
         - backend
   
   volumes:
     postgres_data:
DEVELOPMENT WORKFLOW:

Backend Development

bash   # Run backend
   cd WahadiniCryptoQuest.API
   dotnet watch run
   
   # Run migrations
   dotnet ef migrations add MigrationName --project ../WahadiniCryptoQuest.Infrastructure
   dotnet ef database update --project ../WahadiniCryptoQuest.Infrastructure

Frontend Development

bash   # Run frontend
   cd WahadiniCryptoQuest-frontend
   npm run dev
   
   # Build for production
   npm run build
   npm run preview

Docker Development

bash   # Start all services
   docker-compose up -d
   
   # View logs
   docker-compose logs -f backend
   
   # Stop services
   docker-compose down
TESTING SETUP:

Backend Tests (Create WahadiniCryptoQuest.Tests project)

bash   dotnet new xunit -n WahadiniCryptoQuest.Tests
   dotnet add package Moq
   dotnet add package FluentAssertions
   dotnet add package Microsoft.EntityFrameworkCore.InMemory

Frontend Tests (Install testing libraries)

bash   npm install -D vitest @testing-library/react @testing-library/jest-dom @testing-library/user-event
```

DEPLOYMENT CHECKLIST:

Backend:
- [ ] Update appsettings.Production.json with production values
- [ ] Enable HTTPS
- [ ] Set up proper CORS origins
- [ ] Configure production database (consider managed PostgreSQL)
- [ ] Set up email service (SendGrid, AWS SES)
- [ ] Configure Stripe production keys
- [ ] Set up logging/monitoring (Serilog + Seq/ELK)
- [ ] Enable rate limiting
- [ ] Set up health checks
- [ ] Configure CI/CD pipeline

Frontend:
- [ ] Update environment variables for production
- [ ] Enable production build optimizations
- [ ] Set up CDN for static assets
- [ ] Configure error tracking (Sentry)
- [ ] Enable analytics (Google Analytics, Mixpanel)
- [ ] Set up monitoring
- [ ] Configure SEO meta tags
- [ ] Test responsive design on all devices

Database:
- [ ] Set up automated backups
- [ ] Configure connection pooling
- [ ] Add database indexes for performance
- [ ] Set up read replicas (if needed)
- [ ] Enable SSL connections
- [ ] Configure proper user permissions

RECOMMENDED FREE/BUDGET HOSTING:

Backend:
- Railway.app (free tier with PostgreSQL)
- Render.com (free tier)
- Fly.io (free allowance)

Frontend:
- Vercel (free tier, best for React)
- Netlify (free tier)
- Cloudflare Pages (free tier)

Database:
- Supabase (free PostgreSQL with 500MB)
- ElephantSQL (free tier 20MB)
- Railway PostgreSQL (free tier)

Email:
- SendGrid (100 emails/day free)
- Brevo (300 emails/day free)

Include comprehensive README.md files for both backend and frontend with setup instructions, environment variables, and contribution guidelines.
```

---

## 📖 Complete Documentation Structure

### **Prompt 12: Generate Complete Project Documentation**
CREATE COMPREHENSIVE PROJECT DOCUMENTATION:

README.md (Root)

markdown   # WahadiniCryptoQuest Platform
   
   A gamified crypto education platform with task-based learning and reward systems.
   
   ## Features
   - 🎓 Video-based courses (YouTube embedded)
   - ✅ Interactive tasks with multiple types
   - 🏆 Reward points system
   - 💎 Premium subscriptions with discounts
   - 📊 Progress tracking
   - 🎮 Gamification & leaderboards
   - 👨‍💼 Admin dashboard
   
   ## Tech Stack
   - **Frontend**: React 18, TypeScript, Vite, TailwindCSS
   - **Backend**: .NET 8 Web API
   - **Database**: PostgreSQL
   - **Authentication**: JWT
   - **Payments**: Stripe
   - **Video**: YouTube API
   
   ## Quick Start
   [Installation instructions]
   
   ## Project Structure
   [Directory structure]
   
   ## Documentation
   - [API Documentation](./docs/API.md)
   - [Database Schema](./docs/DATABASE.md)
   - [Frontend Guide](./docs/FRONTEND.md)
   - [Deployment Guide](./docs/DEPLOYMENT.md)
   
   ## License
   MIT

API Documentation (docs/API.md)

Complete endpoint reference
Request/response examples
Authentication flow
Error codes
Rate limiting
Webhook documentation


Database Documentation (docs/DATABASE.md)

ERD diagram
Table descriptions
Relationships
Indexes
Sample queries
Migration guide


Frontend Guide (docs/FRONTEND.md)

Component architecture
State management
Routing
API integration
Styling guide
Best practices


Deployment Guide (docs/DEPLOYMENT.md)

Environment setup
CI/CD pipeline
Docker deployment
Kubernetes configuration
Monitoring setup
Backup strategy


User Guide (docs/USER_GUIDE.md)

How to use the platform
Earning rewards
Task submission
Premium features
FAQ


Admin Guide (docs/ADMIN_GUIDE.md)

Creating courses
Managing users
Reviewing tasks
Analytics interpretation
Platform settings


Development Guide (docs/DEVELOPMENT.md)

Setup instructions
Coding standards
Git workflow
Testing guide
Contribution guidelines



Generate all documentation files with detailed content, code examples, screenshots (placeholders), and step-by-step instructions.

---

This is your **complete, production-ready project specification**! 🎉

## Next Steps:

1. **Start with Backend Setup** (Prompt 1-3)
   - Create database schema
   - Set up API structure
   - Implement repositories and services

2. **Build Frontend Foundation** (Prompt 4-5)
   - Set up React project
   - Create authentication system
   - Build basic layout components

3. **Implement Core Features** (Prompt 6-10)
   - Task system
   - Reward system
   - Subscription integration
   - Video player
   - Admin dashboard

4. **Deploy & Test** (Prompt 11-12)
   - Set up hosting
   - Configure production environment
   - Test all features
   - Write documentation

Each prompt is designed to be **copied directly into GitHub Copilot Edits** for the relevant files/sections. The prompts are detailed enough that Copilot will generate production-quality code.

**Pro Tips:**
- Work through prompts sequentially
- Test each feature before moving to the next
- Use the free tiers mentioned for hosting
- Start simple, add complexity gradually
- Focus on MVP first, then enhance