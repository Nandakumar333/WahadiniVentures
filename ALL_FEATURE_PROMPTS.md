# WahadiniCryptoQuest - Complete Feature Prompts Collection

**Project:** WahadiniCryptoQuest  
**Date:** December 8, 2025  
**Purpose:** Comprehensive prompts for generating SpecKit specifications  

---

## Table of Contents
1. [001 - User Authentication & Authorization](#001-authentication)
2. [002 - Course Management System](#002-course-management)
3. [003 - Lesson Content & Video Player](#003-lesson-content)
4. [004 - Reward Points System](#004-reward-system)
5. [005 - Task Submission & Review](#005-task-submission)
6. [006 - User Profile & Dashboard](#006-user-profile)
7. [007 - Discount Redemption System](#007-discount-system)
8. [008 - Subscription & Payment (Stripe)](#008-subscription-payment)
9. [009 - Admin Dashboard & Analytics](#009-admin-dashboard)
10. [010 - Leaderboard & Gamification](#010-leaderboard)
11. [011 - Certificate Generation](#011-certificates)
12. [012 - Notification System](#012-notifications)

---

## 001-authentication

### Feature: User Authentication & Authorization System

```
Create complete speckit specification for 001-authentication.

FEATURE: User Authentication & Authorization System

REQUIREMENTS:
User Registration:
- Email and password registration
- Username uniqueness validation
- Email verification with token
- Password strength requirements (min 8 chars, 1 upper, 1 lower, 1 number, 1 special)
- Send verification email with link

User Login:
- Email/password authentication
- JWT token generation (60-minute expiry)
- Refresh token mechanism (7-day expiry)
- Return user profile with token
- "Remember Me" functionality

Password Management:
- Forgot password flow
- Email with reset link (1-hour expiry)
- Token-based password reset
- Password change for logged-in users

Role-Based Authorization:
- Roles: Admin, Premium, Free
- Role-based route protection
- Subscription tier validation
- Admin-only endpoints

Token Management:
- JWT with secure signing
- Refresh token rotation
- Token revocation on logout
- Auto-refresh before expiry
- Secure token storage

TECHNICAL:
Backend:
- ASP.NET Identity integration
- BCrypt password hashing
- JWT token service
- AuthService (register, login, refresh, reset)
- AuthController with all endpoints
- RefreshToken entity with database storage

Frontend:
- AuthStore (Zustand) with persistence
- LoginForm, RegisterForm components
- ProtectedRoute wrapper
- Axios interceptors for tokens
- Email verification page
- Password reset flow
- Auto token refresh logic

API Endpoints:
- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/refresh-token
- POST /api/auth/forgot-password
- POST /api/auth/reset-password
- GET /api/auth/verify-email?token={token}
- GET /api/auth/me

Database:
- Users table (Id, Email, Username, PasswordHash, Role, SubscriptionTier, EmailVerified, etc.)
- RefreshTokens table (Id, Token, UserId, ExpiryDate, IsRevoked)

SECURITY:
- Rate limiting (5 attempts per minute)
- Secure password hashing (BCrypt cost 12)
- HTTPS required
- Token expiration validation
- SQL injection prevention
- XSS protection
- CORS configuration

Create ALL 7 sections with complete authentication flow.
Target: 20,000+ characters.
```

---

## 002-course-management

### Feature: Course Management System

```
Create complete speckit specification for 002-course-management.

FEATURE: Course Management System

REQUIREMENTS:
Course Structure:
- Course: Title, Description, Thumbnail, Category, Difficulty, IsPremium
- Lessons: Ordered list within course
- Prerequisites: Required courses before enrollment
- Estimated duration
- Instructor information

Admin Features:
- Create/Edit/Delete courses
- Upload course thumbnails
- Add lessons to courses
- Reorder lessons (drag & drop)
- Set course as premium/free
- Publish/unpublish courses
- Course analytics (enrollments, completions)

Student Features:
- Browse all courses (grid/list view)
- Filter by category, difficulty, premium status
- Search courses by title/description
- Enroll in courses
- View enrolled courses
- Track course progress
- View course details with lesson list
- Mark course as favorite
- Rate and review courses

Course Categories:
- Blockchain Basics
- Cryptocurrency Trading
- Smart Contracts
- DeFi (Decentralized Finance)
- NFTs
- Web3 Development
- Security & Privacy

Difficulty Levels:
- Beginner
- Intermediate
- Advanced
- Expert

TECHNICAL:
Backend:
- Course entity with relationships
- Lesson entity
- CourseEnrollment entity
- CourseService (CRUD operations)
- CourseController (admin + student endpoints)
- Image upload service (Azure/AWS/local)

Frontend:
- CoursesPage (browse & filter)
- CourseDetailPage
- CourseCard component
- CoursePlayer (lesson viewer)
- EnrollButton component
- ProgressBar component
- Admin CourseEditor

API Endpoints:
- GET /api/courses (public, with filters)
- GET /api/courses/{id}
- POST /api/courses (admin)
- PUT /api/courses/{id} (admin)
- DELETE /api/courses/{id} (admin)
- POST /api/courses/{id}/enroll
- GET /api/courses/enrolled
- POST /api/courses/{id}/favorite
- GET /api/courses/categories

Database:
- Courses table
- Lessons table (with courseId)
- CourseEnrollments (userId, courseId, enrolledDate, progress)
- CourseFavorites

Create ALL 7 sections with complete course management.
Target: 22,000+ characters.
```

---

## 003-lesson-content

### Feature: Lesson Content & Video Player

```
Create complete speckit specification for 003-lesson-content.

FEATURE: Lesson Content & YouTube Video Player

REQUIREMENTS:
Lesson Structure:
- Title, Description, Content (Markdown)
- YouTube video URL
- Duration estimate
- Order within course
- IsRequired flag
- Reward points on completion

Video Player Features:
- Embedded YouTube player
- Play/pause controls
- Progress tracking (80% = completed)
- Resume from last position
- Speed controls (0.5x, 1x, 1.25x, 1.5x, 2x)
- Fullscreen mode
- Quality selection
- Captions/subtitles
- Auto-play next lesson

Progress Tracking:
- Track watch time per user
- Mark as completed at 80% watched
- Save current timestamp
- Award points on first completion
- Display progress bar
- Prevent skipping required lessons

Content Display:
- Markdown rendering for text content
- Code syntax highlighting
- Embedded images
- Links to resources
- Downloadable materials

TECHNICAL:
Backend:
- Lesson entity
- UserProgress entity (lessonId, userId, watchedSeconds, completed)
- LessonService (get, track progress, complete)
- LessonController

Frontend:
- LessonPlayer component (react-player)
- ProgressTracker hook
- MarkdownRenderer component
- ResumePrompt modal
- CompletionCelebration component
- NextLessonButton

API Endpoints:
- GET /api/lessons/{id}
- GET /api/lessons/{id}/progress
- POST /api/lessons/{id}/track-progress (body: { watchedSeconds })
- POST /api/lessons/{id}/complete
- GET /api/courses/{courseId}/lessons

Database:
- Lessons table (Id, CourseId, Title, Description, Content, YouTubeVideoId, Duration, OrderIndex, RewardPoints)
- UserProgress (UserId, LessonId, WatchedSeconds, CompletedAt, PointsAwarded)

Integration:
- YouTube IFrame API
- react-player library
- localStorage for resume position
- Offline progress queue

Create ALL 7 sections with complete lesson/video implementation.
Target: 20,000+ characters.
```

---

## 004-reward-system

### Feature: Reward Points System

```
Create complete speckit specification for 004-reward-system.

FEATURE: Reward Points System

REQUIREMENTS:
Point Earning Events:
- Complete lesson: 50-200 points (based on difficulty)
- Complete course: 500-1000 points
- Submit task: 25-100 points
- Task approved: Bonus 50-200 points
- Daily login: 10 points
- Weekly streak: Bonus 50 points
- Referral signup: 100 points
- Watch video 80%: Auto points

Point Usage:
- Redeem discount codes
- Unlock premium content (future)
- Leaderboard rankings
- Achievement badges

Point Transactions:
- Earned (positive)
- Redeemed (negative)
- Bonus (positive)
- Adjusted (admin)
- Expired (negative, after 1 year)

Display Features:
- Current point balance (header)
- Point transaction history
- Point breakdown by source
- Animated point gain notifications
- Progress to next milestone

TECHNICAL:
Backend:
- RewardTransaction entity (UserId, Points, Type, Description, RelatedEntityType, RelatedEntityId)
- RewardService (award, deduct, getBalance, getTransactions)
- RewardController

Frontend:
- PointsDisplay component (animated counter)
- PointsNotification component (toast/modal)
- TransactionHistory page
- PointsBreakdown component
- usePoints hook

API Endpoints:
- GET /api/rewards/balance
- GET /api/rewards/transactions
- POST /api/rewards/award (internal/admin)
- POST /api/rewards/deduct (internal)
- GET /api/rewards/leaderboard

Database:
- RewardTransactions table
- Indexes on UserId, CreatedAt

Features:
- Real-time point updates
- Transaction immutability
- Audit logging
- Point expiration job (background)

Create ALL 7 sections with complete reward system.
Target: 18,000+ characters.
```

---

## 005-task-submission

### Feature: Task Submission & Review System

```
Create complete speckit specification for 005-task-submission.

FEATURE: Task Submission & Review System

REQUIREMENTS:
Task Types:
1. Quiz (multiple choice, auto-graded)
2. Text Submission (essay, requires review)
3. Screenshot Upload (with validation)
4. External Link (URL validation)
5. Wallet Verification (crypto address)

Task Structure:
- Title, Description
- Task type
- Reward points
- Time limit (optional)
- IsRequired flag
- Task data (JSON for quiz questions, etc.)

Student Workflow:
- View available tasks in lesson
- Submit task based on type
- Track submission status (Pending, Approved, Rejected)
- Receive feedback on rejected tasks
- Resubmit rejected tasks
- View submission history

Quiz Task Features:
- Multiple choice questions
- Single correct answer per question
- Immediate auto-grading
- Pass threshold (e.g., 70%)
- Randomize question order
- Show correct answers after submission

Admin Review Features:
- Queue of pending submissions
- View submission details
- Approve/Reject with feedback
- Bulk actions
- Submission statistics
- Filter by status, date, user

TECHNICAL:
Backend:
- LearningTask entity (linked to lesson)
- TaskSubmission entity (userId, taskId, data, status, feedback)
- TaskService (submit, review, getStatus)
- AdminTaskController (review queue)

Frontend:
- TaskCard component (shows task details)
- TaskSubmissionModal (different forms per type)
- QuizForm component
- TextSubmissionForm
- ScreenshotUploadForm
- SubmissionStatusBadge
- AdminReviewQueue page

API Endpoints:
- GET /api/tasks/{lessonId}
- POST /api/tasks/{taskId}/submit
- GET /api/tasks/my-submissions
- GET /api/tasks/{taskId}/status
- POST /api/admin/tasks/{submissionId}/review (body: { status, feedback })
- GET /api/admin/tasks/pending

Database:
- LearningTasks table
- TaskSubmissions table
- Indexes on status, userId, taskId

Validation:
- File size limits (5MB)
- File type validation (images only)
- URL format validation
- Quiz answer validation
- Plagiarism detection (future)

Create ALL 7 sections with complete task system.
Target: 24,000+ characters.
```

---

## 006-user-profile

### Feature: User Profile & Dashboard

```
Create complete speckit specification for 006-user-profile.

FEATURE: User Profile & Dashboard System

REQUIREMENTS:
Profile Information:
- Avatar (upload or default)
- Username (editable)
- Email (verified badge)
- Bio/About section
- Location (optional)
- Social links (Twitter, LinkedIn, GitHub)
- Member since date
- Subscription tier badge

Dashboard Sections:
1. Overview Stats:
   - Total points
   - Courses enrolled
   - Courses completed
   - Current streak
   - Rank on leaderboard

2. Recent Activity:
   - Last 10 activities (completed lesson, earned points, etc.)
   - Timeline view

3. Enrolled Courses:
   - Grid of course cards
   - Progress bars
   - Quick access to continue learning

4. Achievements:
   - Badge collection
   - Locked/unlocked badges
   - Progress to next achievement

5. Subscription Status:
   - Current tier
   - Expiry date
   - Upgrade button
   - Billing history link

Settings Page:
- Update profile information
- Change password
- Email preferences
- Notification settings
- Privacy settings
- Account deletion

Public Profile:
- View other users' profiles
- See their achievements
- View completed courses
- Leaderboard ranking
- Privacy controls (hide profile)

TECHNICAL:
Backend:
- UserProfile entity
- ProfileService (update, getPublic)
- ProfileController
- Avatar upload service

Frontend:
- DashboardPage
- ProfilePage
- SettingsPage
- AvatarUpload component
- StatCard component
- ActivityTimeline component
- CourseProgress component

API Endpoints:
- GET /api/profile
- PUT /api/profile
- POST /api/profile/avatar
- GET /api/profile/{userId} (public)
- GET /api/dashboard/stats
- GET /api/dashboard/activity

Database:
- UserProfiles table (userId, avatar, bio, location, socialLinks)
- ActivityLog table

Create ALL 7 sections with complete profile system.
Target: 20,000+ characters.
```

---

## 007-discount-system

### Feature: Point-Based Discount Redemption System

```
Create complete speckit specification for 007-discount-system.

FEATURE: Point-Based Discount Redemption System

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
- Discount entity
- UserDiscountRedemption entity
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

Database:
- Discounts table (Id, Name, Description, PointCost, DiscountValue, DiscountType, ApplicablePlans, ExpiryDate, MaxRedemptions, CurrentRedemptions)
- UserDiscountRedemptions (Id, UserId, DiscountId, GeneratedCode, RedeemedAt, ExpiresAt, IsUsed)

Integration:
- Stripe promo code creation
- Email notification on redemption
- Code validation at checkout

Create ALL 7 sections with complete discount system.
Target: 22,000+ characters.
```

---

## 008-subscription-payment

### Feature: Stripe Subscription Integration

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
- Subscription entity (userId, stripeSubscriptionId, tier, status, expiryDate)
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

Database:
- Subscriptions table
- PaymentHistory table
- StripeCustomer (userId, stripeCustomerId)

SECURITY:
- Never expose Stripe secret key in frontend
- Verify webhook signatures
- Use environment variables for keys
- HTTPS required in production
- Idempotency keys for payments

Create ALL 7 sections with complete Stripe integration code.
Target: 25,000+ characters.
```

---

## 009-admin-dashboard

### Feature: Admin Dashboard & Analytics

```
Create complete speckit specification for 009-admin-dashboard.

FEATURE: Admin Dashboard & Analytics System

REQUIREMENTS:
Dashboard Overview:
- Total users (active, inactive)
- Total revenue (monthly, yearly)
- Active subscriptions by tier
- Course enrollments (last 30 days)
- Task submissions pending review
- Top courses by enrollment
- Recent user signups
- System health metrics

User Management:
- List all users (paginated, searchable)
- Filter by role, subscription, status
- View user details (profile, activity, subscriptions)
- Edit user roles
- Deactivate/activate accounts
- View user's reward points
- Manually adjust points
- Send notifications to users

Course Management:
- List all courses
- Create/edit/delete courses
- Publish/unpublish courses
- View course analytics (enrollments, completions, avg. rating)
- Add/remove lessons
- Reorder lessons

Task Review Queue:
- Pending submissions (priority queue)
- Filter by task type, date, user
- Review submission details
- Approve/reject with feedback
- Bulk approval/rejection
- View submission history

Analytics & Reports:
- Revenue reports (daily, weekly, monthly)
- User growth charts
- Course completion rates
- Popular courses/lessons
- Point distribution
- Subscription churn rate
- Task submission statistics
- Geographic distribution of users

Discount Management:
- Create/edit discount codes
- View redemption statistics
- Active/inactive discounts
- Usage analytics

TECHNICAL:
Backend:
- AdminService (aggregates data)
- AdminController (all admin endpoints)
- AnalyticsService (generates reports)
- Authorization (Admin role required)

Frontend:
- AdminDashboard page
- UserManagement page
- CourseManagement page
- TaskReviewQueue page
- AnalyticsReports page
- Charts (recharts/chart.js)

API Endpoints:
- GET /api/admin/dashboard/stats
- GET /api/admin/users
- GET /api/admin/users/{id}
- PUT /api/admin/users/{id}
- GET /api/admin/courses
- GET /api/admin/tasks/pending
- POST /api/admin/tasks/{id}/review
- GET /api/admin/analytics/revenue
- GET /api/admin/analytics/users

Database:
- Analytics views/stored procedures
- Indexed queries for performance

Features:
- Real-time dashboard updates
- Export reports (CSV, PDF)
- Date range filters
- Role-based access (super admin vs admin)

Create ALL 7 sections with complete admin dashboard.
Target: 24,000+ characters.
```

---

## 010-leaderboard

### Feature: Leaderboard & Gamification System

```
Create complete speckit specification for 010-leaderboard.

FEATURE: Leaderboard & Gamification System

REQUIREMENTS:
Leaderboard Types:
1. Global Leaderboard (all-time points)
2. Monthly Leaderboard (resets each month)
3. Weekly Leaderboard (resets each week)
4. Course-specific Leaderboard

Rankings:
- Top 100 users displayed
- Current user's rank always visible
- Rank movement indicator (up/down arrows)
- Points required to next rank
- Profile pictures and usernames
- Subscription tier badges

Gamification Elements:
1. Achievements/Badges:
   - First Lesson: Complete first lesson
   - Course Master: Complete 5 courses
   - Point Collector: Earn 1000 points
   - Early Bird: 7-day login streak
   - Quiz Master: Pass 50 quizzes
   - Social Butterfly: Refer 10 friends
   - Premium Learner: Maintain premium for 3 months

2. Levels:
   - Level 1: 0-500 points (Newbie)
   - Level 2: 501-1500 points (Learner)
   - Level 3: 1501-3000 points (Scholar)
   - Level 4: 3001-5000 points (Expert)
   - Level 5: 5000+ points (Master)

3. Streaks:
   - Daily login streak counter
   - Longest streak record
   - Streak rewards (bonus points)
   - Streak freeze (use points to maintain)

Display Features:
- Leaderboard page with filters
- User profile shows achievements
- Progress bars for each achievement
- Notifications for new achievements
- Share achievements on social media

TECHNICAL:
Backend:
- Leaderboard entity (cached rankings)
- Achievement entity
- UserAchievement entity (unlock history)
- LeaderboardService (calculate rankings)
- AchievementService (check and award)
- Background job for leaderboard updates

Frontend:
- LeaderboardPage
- AchievementGallery component
- RankCard component
- ProgressToNextRank component
- AchievementNotification component
- LevelBadge component

API Endpoints:
- GET /api/leaderboard/global
- GET /api/leaderboard/monthly
- GET /api/leaderboard/weekly
- GET /api/leaderboard/course/{courseId}
- GET /api/achievements
- GET /api/achievements/user/{userId}
- GET /api/user/level
- GET /api/user/streak

Database:
- LeaderboardCache table (updated hourly)
- Achievements table
- UserAchievements table
- UserStreaks table

Features:
- Caching for performance
- Real-time rank updates
- Achievement unlock animations
- Social sharing

Create ALL 7 sections with complete leaderboard system.
Target: 22,000+ characters.
```

---

## 011-certificates

### Feature: Certificate Generation System

```
Create complete speckit specification for 011-certificates.

FEATURE: Certificate of Completion Generation

REQUIREMENTS:
Certificate Eligibility:
- Complete all required lessons in course
- Complete all required tasks
- Pass all quizzes (minimum 70%)
- Minimum course progress: 100%
- Premium subscription required (for paid courses)

Certificate Design:
- Professional template with logo
- Course title and description
- Student name
- Completion date
- Unique certificate ID
- Instructor signature (digital)
- QR code for verification
- Watermark for authenticity

Certificate Features:
- Auto-generate on course completion
- Download as PDF
- Share on LinkedIn
- Share on social media
- Email certificate to user
- Print-friendly format

Verification System:
- Public verification page
- Verify by certificate ID
- Shows: Student name, course, date, validity
- Cannot be forged

Certificate Management:
- View all earned certificates
- Download anytime
- Re-download if lost
- Certificate history

Admin Features:
- Revoke certificates (plagiarism, etc.)
- Customize certificate templates
- Add instructor signatures
- Analytics: Certificates issued

TECHNICAL:
Backend:
- Certificate entity (userId, courseId, certificateId, generatedAt, isRevoked)
- CertificateService (generate, verify, revoke)
- PDF generation library (iTextSharp/DinkToPdf)
- CertificateController

Frontend:
- CertificatePage (display certificate)
- MyCertificates page
- DownloadButton component
- ShareButtons component
- VerificationPage (public)

API Endpoints:
- POST /api/certificates/generate/{courseId}
- GET /api/certificates
- GET /api/certificates/{certificateId}/download
- GET /api/certificates/verify/{certificateId} (public)
- POST /api/admin/certificates/{id}/revoke

Database:
- Certificates table
- CertificateTemplates table

Features:
- PDF generation with custom fonts
- QR code generation
- Watermark overlay
- Email with attachment
- LinkedIn API integration

Create ALL 7 sections with complete certificate system.
Target: 20,000+ characters.
```

---

## 012-notifications

### Feature: Notification System

```
Create complete speckit specification for 012-notifications.

FEATURE: Multi-Channel Notification System

REQUIREMENTS:
Notification Types:
1. In-App Notifications:
   - Real-time bell icon with count
   - Dropdown list of recent notifications
   - Mark as read/unread
   - Delete notifications

2. Email Notifications:
   - Welcome email on signup
   - Email verification
   - Password reset
   - Course enrollment confirmation
   - Task submission received
   - Task review (approved/rejected)
   - Points earned
   - Achievement unlocked
   - Certificate generated
   - Subscription expiring soon
   - Payment receipt

3. Push Notifications (future):
   - Browser push
   - Mobile app push

Notification Events:
- User signs up → Welcome email
- Email verification → Verification link
- Course enrolled → Confirmation
- Lesson completed → Points earned
- Task submitted → Receipt confirmation
- Task reviewed → Approval/rejection + feedback
- Achievement unlocked → Celebration
- Subscription expiring → Reminder (7 days, 1 day)
- Payment failed → Action required
- New course available → Announcement
- Admin message → Custom notification

User Preferences:
- Enable/disable email notifications per type
- Enable/disable in-app notifications
- Notification frequency (instant, daily digest)
- Quiet hours (no notifications)

Notification Management:
- View all notifications
- Filter by type, read/unread
- Mark all as read
- Delete notifications
- Notification settings page

Admin Features:
- Send broadcast notifications to all users
- Send targeted notifications (by role, subscription)
- Schedule notifications
- Notification templates
- Delivery analytics

TECHNICAL:
Backend:
- Notification entity (userId, type, title, message, isRead, createdAt)
- NotificationService (create, send, markRead)
- EmailService (SendGrid/Brevo integration)
- NotificationHub (SignalR for real-time)
- NotificationController

Frontend:
- NotificationBell component (header)
- NotificationDropdown component
- NotificationsPage (full list)
- NotificationSettings page
- Toast notifications for real-time

API Endpoints:
- GET /api/notifications (paginated)
- GET /api/notifications/unread-count
- POST /api/notifications/{id}/read
- POST /api/notifications/mark-all-read
- DELETE /api/notifications/{id}
- PUT /api/notifications/preferences
- POST /api/admin/notifications/broadcast

Database:
- Notifications table
- NotificationPreferences table
- EmailQueue table (for retry logic)

Integration:
- SignalR for real-time updates
- SendGrid/Brevo API
- Email templates (Handlebars/Razor)
- Background jobs for email sending

Features:
- Real-time notification delivery
- Email retry logic
- Unsubscribe links in emails
- Notification grouping
- Rich text formatting

Create ALL 7 sections with complete notification system.
Target: 23,000+ characters.
```

---

## Usage Instructions

### How to Use These Prompts:

1. **Copy the entire prompt** for the feature you want to implement
2. **Paste into your AI assistant** (Claude, ChatGPT, etc.)
3. The AI will generate a **complete SpecKit specification** with all 7 sections:
   - `/speckit.specify` - Feature overview
   - `/speckit.plan` - Implementation plan
   - `/speckit.clarify` - Q&A
   - `/speckit.analyze` - Technical analysis
   - `/speckit.checklist` - Implementation checklist
   - `/speckit.tasks` - Task breakdown
   - `/speckit.implement` - Code implementation guide

4. **Save the output** to `specs/{feature-id}.md`
5. **Use as reference** for development

### Target Character Counts:
- Minimum: 18,000 characters per spec
- Recommended: 20,000-25,000 characters
- All sections must be comprehensive with code examples

### Dependencies:
Some features depend on others. Implement in this order:
1. Authentication (001) - Foundation
2. Course Management (002) - Core content
3. Lesson Content (003) - Delivery mechanism
4. Reward System (004) - Gamification base
5. Task Submission (005) - Assessment
6. User Profile (006) - User experience
7. Discount System (007) - Monetization bridge
8. Subscription (008) - Revenue
9. Admin Dashboard (009) - Management
10. Leaderboard (010) - Social engagement
11. Certificates (011) - Achievement proof
12. Notifications (012) - Communication

---

**Project:** WahadiniCryptoQuest  
**Total Features:** 12  
**Estimated Total Development Time:** 400-500 hours  
**Tech Stack:** .NET 8, PostgreSQL, React 18, TypeScript, Stripe  

---

*This document serves as a comprehensive prompt collection for generating detailed technical specifications for the entire WahadiniCryptoQuest platform.*
