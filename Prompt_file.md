# WahadiniCryptoQuest Platform - Feature-by-Feature SpecKit Prompts

A comprehensive guide for implementing the WahadiniCryptoQuest crypto education platform using speckit methodology. This document breaks down the platform into manageable features for systematic implementation.

## Platform Overview

**WahadiniCryptoQuest** is a gamified crypto education platform featuring:
- Video-based learning with YouTube integration
- Multi-type task verification system (Quiz, ExternalLink, Screenshot, TextSubmission, WalletVerification)
- Reward points economy with achievements and leaderboards  
- Premium subscription with point-based discount redemption
- Comprehensive admin dashboard for content and user management

**Tech Stack**: .NET 8 (backend), React 18/TypeScript (frontend), PostgreSQL, Stripe, JWT Auth

---

## Project Constitution

### /speckit.constitution

```
Create a project constitution for WahadiniCryptoQuest, a gamified crypto education platform that will guide all development decisions and ensure alignment with core values and principles.

**Platform Context**: 
WahadiniCryptoQuest is a comprehensive crypto education platform combining video-based learning, multi-type task verification (Quiz, ExternalLink, Screenshot, TextSubmission, WalletVerification), reward points economy, premium subscriptions with discount redemption, and admin dashboard for content and user management.

**Core Principles to Define**:

### 1. Learning-First Experience
**Question**: How do we ensure educational value takes priority over engagement metrics?

**Constitution Requirements**:
- All gamification elements must enhance learning outcomes, not distract from them
- Progress tracking should focus on comprehension and skill development, not just completion metrics
- Task verification must validate actual understanding, not gaming the system
- Content quality standards must be maintained through review processes
- Reward points should incentivize learning behaviors, not addictive engagement patterns
- Course completion rates should reflect genuine skill acquisition
- Platform algorithms must prioritize educational effectiveness over retention hooks

**Validation**: Each feature must demonstrate how it enhances educational value rather than mere engagement.

### 2. Security & Privacy Standards
**Question**: What security measures are non-negotiable for protecting user data and financial transactions?

**Constitution Requirements**:
- All user data must be encrypted at rest and in transit (HTTPS only, AES-256 encryption)
- Authentication must use industry-standard JWT with refresh tokens
- Password storage must use BCrypt with minimum 12 rounds
- Payment processing must be PCI-DSS compliant (via Stripe)
- User data handling must comply with GDPR and CCPA regulations
- Audit logs required for all admin actions and financial transactions
- Session management must include timeout, device limits, and secure invalidation
- No sensitive data (passwords, payment info) logged or exposed in APIs
- Rate limiting on all authentication and high-value endpoints
- Input validation and sanitization to prevent XSS, SQL injection, CSRF attacks
- Regular security audits and penetration testing
- Clear privacy policy and transparent data usage communication

**Validation**: All features must include appropriate authentication, authorization, encryption, and audit logging.

### 3. Scalability & Performance
**Question**: How do we ensure the platform performs well from day one through growth to thousands of users?

**Constitution Requirements**:
- Target: Support 1000+ concurrent users with <3 second page load times
- Mobile-first responsive design (breakpoints: 768px tablet, 480px mobile)
- Database design must include proper indexing and partitioning strategy (time-based partitioning for user activity data)
- API endpoints must include pagination (max 100 items per page)
- Caching strategy for frequently accessed data (categories, subscription plans)
- CDN integration for video delivery with adaptive bitrate streaming
- Stateless architecture enabling horizontal scaling
- Rate limiting to prevent abuse and ensure fair resource usage
- Background job processing for heavy operations (email, reports)
- Performance monitoring with alerts for degradation
- Database query optimization with N+1 prevention
- Asset optimization (image compression, code minification, lazy loading)

**Validation**: Features must specify performance requirements and scaling considerations.

### 4. Fair Gamification Economy
**Question**: How do we create a sustainable reward system that motivates without enabling exploitation?

**Constitution Requirements**:
- Point earning rates must be balanced and sustainable long-term
- Anti-gaming protection mechanisms:
  - Rate limiting: 10 points per user per hour maximum
  - Duplicate detection via content hashing
  - IP-based velocity monitoring (50 submissions per IP per day)
  - Transaction auditing with immutable ledger
  - ML-based pattern detection for suspicious activity
- Discount redemption limits: 500 points per month with rollover
- One-time use per user per discount type to prevent abuse
- Free tier must provide meaningful value without pay-to-win scenarios
- Premium features must offer clear value, not create artificial scarcity
- Point inflation prevention through economic modeling
- Regular audits of point distribution patterns
- Transparent point award rules visible to users
- Fair competition in leaderboards (detect and ban cheaters)

**Validation**: Gamification elements must prevent exploitation and maintain economic balance.

### 5. Content Quality Assurance
**Question**: How do we maintain educational accuracy and content quality at scale?

**Constitution Requirements**:
- Instructor self-service portal with mandatory admin approval workflow
- Content accuracy verification before publication
- Regular content reviews and updates (at least quarterly for time-sensitive topics)
- Multi-level task verification:
  - Auto-approval for objective tasks (quizzes with 80%+ score, wallet verification)
  - Manual review for subjective tasks (screenshots, text submissions, external links)
  - Escalation system during review backlogs
- Plagiarism detection for text submissions
- Quality metrics tracking (completion rates, user ratings, feedback analysis)
- Content moderation capabilities for community-generated content
- Clear guidelines for instructors on content creation standards
- Misinformation prevention through fact-checking workflows
- Regular curriculum updates aligned with crypto industry changes
- Expert review for advanced courses

**Validation**: Content features must include accuracy verification and quality maintenance mechanisms.

### 6. Accessibility & Transparency
**Question**: How do we ensure the platform is accessible to all users and operates transparently?

**Constitution Requirements**:
- WCAG 2.1 AA compliance for all user interfaces
- Keyboard navigation support throughout the platform
- Screen reader compatibility with proper ARIA labels
- Color contrast ratios meeting accessibility standards
- Responsive design supporting assistive technologies
- Clear and simple language in all communications
- Transparent pricing with no hidden fees
- Clear terms of service and privacy policy (written at 8th-grade reading level)
- Algorithm transparency in recommendations and leaderboards
- Open communication about platform changes (30-day notice for pricing changes)
- Accessible customer support channels:
  - Response time: <24 hours for premium users, <48 hours for free users
  - Multiple contact methods (email, chat, knowledge base)
- Public roadmap showing planned features and improvements
- Regular transparency reports on platform metrics
- Clear refund and cancellation policies

**Validation**: All interfaces must comply with accessibility standards and provide transparent communication.

### 7. Business Model Ethics
**Question**: How do we build a sustainable business while maintaining ethical practices?

**Constitution Requirements**:
- Freemium model must provide genuine value in free tier (access to core learning features)
- Premium pricing must be fair and competitive ($9.99/month, $99/year)
- No dark patterns in subscription flows (clear cancel buttons, no forced continuation)
- Transparent discount system (reward points redemption is opt-in, not required)
- No selling user data to third parties
- Ad-free experience for premium users
- Clear value proposition for premium features
- Grace period for failed payments (3 days) before access revocation
- Prorated refunds within 7 days of subscription purchase
- No automatic renewal without user consent and reminder emails
- Priority support for paying customers without neglecting free users
- Sustainable instructor compensation model (if applicable)
- Reinvestment of revenue into platform improvement and content quality

**Validation**: Business decisions must prioritize user value and ethical practices over short-term revenue.

### 8. Technical Excellence
**Question**: What development practices ensure code quality and maintainability?

**Constitution Requirements**:
- Clean Architecture with clear separation of concerns (Domain, Application, Infrastructure, API)
- Comprehensive test coverage (target: 80% code coverage)
- Code review required for all changes
- Continuous integration/continuous deployment (CI/CD) pipeline
- Automated testing in deployment pipeline
- Version control with meaningful commit messages
- Documentation for all public APIs and complex business logic
- Error handling with graceful degradation
- Logging and monitoring for all critical paths
- Regular dependency updates and security patches
- Technical debt management (allocate 20% of sprint capacity)
- Performance benchmarking and regression testing
- Code style consistency through linting and formatting tools

**Validation**: All code changes must meet quality standards and include appropriate tests.

### Constitution Enforcement

**Implementation Rules**:
1. Every feature specification must include a "Constitution Compliance" section
2. Code reviews must verify constitutional adherence
3. Regular audits (monthly) to ensure ongoing compliance
4. User feedback mechanisms to report constitutional violations
5. Quarterly constitution review to adapt to platform evolution

**Decision Framework**:
When facing a trade-off between principles, prioritize in this order:
1. Security & Privacy (non-negotiable)
2. Learning-First Experience (core value)
3. Accessibility & Transparency (user trust)
4. Fair Gamification Economy (platform integrity)
5. Content Quality Assurance (educational credibility)
6. Scalability & Performance (user experience)
7. Business Model Ethics (sustainability)
8. Technical Excellence (long-term maintainability)

This constitution serves as the foundational contract between the platform, its users, and its stakeholders, ensuring WahadiniCryptoQuest remains true to its mission of providing quality crypto education through ethical, secure, and effective means.
```

---

## Feature 1: User Authentication & Profile Management

### /speckit.specify

```
Create a comprehensive user authentication and profile management system for WahadiniCryptoQuest platform.

**Feature Name**: User Authentication & Profile Management

**User Story**: A learner registers, verifies their email, logs in securely, and manages their profile with role-based access (Free, Premium, Admin).

**Core Requirements**:

1. **User Registration**
   - Email and password-based registration
   - Username uniqueness validation (3-30 alphanumeric characters)
   - Password requirements: minimum 8 characters with complexity
   - Email verification flow with time-limited tokens
   - Automated welcome email on successful registration
   - Initial role assignment as "Free" tier

2. **Authentication System**
   - JWT token-based authentication with 60-minute expiration
   - Refresh token mechanism with 7-day validity
   - Secure password hashing using BCrypt
   - Login attempt tracking and rate limiting (5 attempts per 15 minutes)
   - Session management with 24-hour token expiration
   - Concurrent session limits (3 devices maximum)
   - Automatic logout on inactivity (30 minutes)

3. **Password Management**
   - Forgot password flow with email-based reset
   - Time-limited reset tokens (60 minutes expiration)
   - Secure session invalidation on password change
   - Password change from user profile

4. **Profile Management**
   - View and edit profile information (username, email)
   - Profile privacy controls
   - Account statistics (courses enrolled, tasks completed, reward points)
   - GDPR-compliant account deletion with 30-day retention
   - Data export functionality

5. **Role-Based Access**
   - Free users: access to free courses, 5 task submissions per day
   - Premium users: unlimited access, all premium content
   - Admin users: full platform management capabilities

**Technical Requirements**:
- Backend: ASP.NET Core Identity, JWT middleware
- Frontend: Zustand/Redux for auth state, protected routes
- Database: User entity with authentication fields
- Security: HTTPS only, secure token storage, XSS prevention

**Success Criteria**:
- User completes registration and email verification within 3 minutes
- Login success rate >99% for valid credentials
- Password reset flow completion rate >90%
- Zero unauthorized access incidents
```

### /speckit.plan

```
Create an implementation plan for User Authentication & Profile Management feature.

**Phase Breakdown**:

**Phase 1: Database and Domain Models** (2-3 days)
- Create User entity with authentication fields
- Set up ASP.NET Identity integration
- Configure JWT settings in appsettings.json
- Create database migration for users table

**Phase 2: Backend API** (3-4 days)
- Implement AuthController with registration, login, refresh-token endpoints
- Create password reset endpoints
- Implement email verification endpoints
- Add JWT token generation and validation middleware
- Create UserService for business logic
- Implement rate limiting on auth endpoints

**Phase 3: Frontend Authentication** (3-4 days)
- Create auth store (Zustand) with login/logout/register actions
- Build registration form with validation
- Build login form with error handling
- Implement protected route component
- Add axios interceptors for token management
- Create password reset flow UI

**Phase 4: Profile Management** (2-3 days)
- Build profile page with view/edit functionality
- Implement account settings
- Add data export functionality
- Create account deletion flow

**Phase 5: Testing & Security** (2 days)
- Unit tests for authentication logic
- Integration tests for auth endpoints
- Security audit for token handling
- Test rate limiting and session management

**Dependencies**:
- Email service configuration (SendGrid/SMTP)
- SSL certificates for HTTPS
- Database setup

**Risks**:
- Token refresh complexity
- Email deliverability issues
- Session synchronization across devices
```

### /speckit.clarify

```
Identify and clarify ambiguous areas in the User Authentication & Profile Management feature before implementation.

**Areas Requiring Clarification**:

1. **Token Refresh Strategy**
   - Should refresh tokens be rotated on each use or remain static?
   - What happens to active sessions when user changes password?
   - Should we force logout all devices or allow graceful session migration?
   - How to handle refresh token expiry during active user session?

2. **Email Verification Flow**
   - Should unverified users have limited access or full access until verified?
   - How long should email verification tokens remain valid?
   - Should we send reminder emails for unverified accounts?
   - What happens to user data if they never verify email (auto-deletion after X days)?

3. **Multi-Device Session Management**
   - How should concurrent session limits be enforced (3 devices)?
   - What happens when user tries to login on 4th device (kick oldest session)?
   - Should users see list of active sessions and ability to revoke them?
   - How to handle session synchronization across devices (logout one, logout all)?

4. **Rate Limiting Implementation**
   - What rate limiting strategy: per IP, per user, or combination?
   - Should rate limits differ for authenticated vs unauthenticated requests?
   - How to handle shared IPs (corporate networks, VPNs)?
   - What error messages/response codes for rate-limited requests?

5. **Account Deletion & GDPR**
   - What data should be permanently deleted vs anonymized?
   - How long is the retention period before final deletion (30 days)?
   - Can users cancel deletion during retention period?
   - What happens to user-created content (courses, comments) after deletion?

6. **Password Policy Details**
   - Exact password requirements (min 8 chars, but what about special chars, numbers)?
   - Should we enforce password history (prevent reusing last N passwords)?
   - How often should users be prompted to change passwords (never, 90 days, etc.)?
   - Should we integrate with Have I Been Pwned API for compromised password checking?

7. **Profile Update Edge Cases**
   - Can users change their email address (require re-verification)?
   - Can users change their username (check for uniqueness)?
   - What profile fields are immutable after creation?
   - How to handle username changes in references (courses, submissions)?

**Clarification Needed Before Implementation**: Document answers to these questions in spec before proceeding to tasks phase.
```

### /speckit.analyze

```
Perform cross-artifact consistency and alignment analysis for User Authentication & Profile Management feature.

**Analysis Areas**:

1. **Specification-to-Plan Alignment**
   - ✅ Check: All specified auth features (registration, login, password reset, email verification) are in implementation plan
   - ✅ Check: JWT configuration with refresh tokens matches spec requirements (60-min access, 7-day refresh)
   - ✅ Check: Rate limiting (5 attempts per 15 minutes) is included in plan
   - ✅ Check: Session management requirements (24-hour expiration, 3-device limit) are addressed
   - ❓ Gap: Profile data export functionality needs more detail on export format (JSON, CSV?)
   - ❓ Gap: Account deletion 30-day retention period not explicitly in plan timeline

2. **Database Schema Alignment**
   - ✅ Check: User entity includes all auth fields (Email, PasswordHash, EmailVerified, tokens)
   - ✅ Check: Relationships support profile management (one-to-many with other entities)
   - ❓ Gap: Need indexes on Email and Username for performance
   - ❓ Gap: Session tracking table needed for concurrent device management

3. **API Design Consistency**
   - ✅ Check: RESTful endpoint naming follows convention (/api/auth/*, /api/users/*)
   - ✅ Check: HTTP methods appropriate (POST for mutations, GET for queries)
   - ❓ Gap: Need to define exact error response format for all auth endpoints
   - ❓ Gap: API versioning strategy not specified

4. **Frontend-Backend Contract**
   - ✅ Check: Auth store actions match backend endpoints
   - ✅ Check: Token storage strategy defined (localStorage with XSS considerations)
   - ❓ Gap: Need exact DTO structure for registration, login requests/responses
   - ❓ Gap: Error handling mapping between backend errors and frontend messages

5. **Security Consistency**
   - ✅ Check: Password hashing (BCrypt) specified in both plan and security requirements
   - ✅ Check: JWT signing algorithm and secret key management addressed
   - ⚠️ Warning: CORS configuration needs explicit allowed origins (not wildcard in production)
   - ⚠️ Warning: Token storage in localStorage has XSS vulnerability - consider httpOnly cookies

6. **Constitution Compliance**
   - ✅ Learning-First: Not applicable to auth feature
   - ✅ Security & Privacy: JWT, BCrypt, HTTPS, rate limiting all included
   - ✅ Scalability: Stateless JWT design supports horizontal scaling
   - ✅ Accessibility: Auth forms should be keyboard navigable and screen-reader friendly
   - ❓ Gap: Accessibility requirements not explicitly in plan

**Recommendations**:
1. Add Session entity to database schema for device tracking
2. Define explicit DTO contracts before frontend implementation
3. Consider httpOnly cookies for token storage instead of localStorage
4. Add accessibility checklist to Phase 3 (Frontend) tasks
5. Document GDPR data retention policy in detail
6. Create API error response format standard

**Alignment Score**: 85% - Good alignment with minor gaps to address before implementation.
```

### /speckit.checklist

```
Generate comprehensive quality checklists for User Authentication & Profile Management feature.

**Requirements Completeness Checklist**:
- [x] User registration flow fully defined
- [x] Email verification process specified
- [x] Login flow with credential validation
- [x] JWT token generation and refresh mechanism
- [x] Password reset flow documented
- [x] Rate limiting requirements stated
- [x] Session management rules defined
- [x] Profile view/edit functionality
- [ ] Data export format specification (missing)
- [x] Account deletion with retention period
- [ ] Multi-device session details (needs clarification)
- [ ] Password policy specifics (needs more detail)
- [x] Role-based access control (Free, Premium, Admin)
- [ ] Accessibility requirements for auth forms (missing)

**Clarity Checklist**:
- [x] Authentication flow diagrams clear
- [ ] Token refresh strategy ambiguous - needs clarification
- [x] API endpoint naming consistent
- [ ] Error message formats not standardized
- [x] Database schema relationships clear
- [ ] DTO structures not fully defined
- [x] Security requirements explicit
- [x] Performance expectations stated (login < 2 seconds)

**Consistency Checklist**:
- [x] Terminology consistent (JWT, tokens, sessions)
- [x] Data types standardized (Guid, DateTime, string)
- [x] Naming conventions uniform (PascalCase for C#, camelCase for TS)
- [ ] Error response format needs standardization
- [x] API versioning approach consistent (not yet implemented)
- [x] Security measures applied uniformly

**Implementation Readiness Checklist**:
- [x] Database schema ready for migration
- [x] Backend technology stack decided (.NET 8, ASP.NET Identity)
- [x] Frontend technology chosen (React 18, Zustand)
- [x] Third-party services identified (SendGrid for email)
- [ ] Environment configuration template created
- [ ] Local development setup documented
- [x] Testing strategy defined (unit, integration, security)
- [ ] Deployment checklist prepared

**Security Checklist**:
- [x] Password hashing algorithm specified (BCrypt)
- [x] Token signing algorithm defined (JWT with symmetric key)
- [x] HTTPS enforcement required
- [x] Input validation planned
- [ ] XSS prevention measures documented
- [ ] CSRF protection strategy defined
- [x] SQL injection prevention (Entity Framework parameterized queries)
- [x] Rate limiting configured
- [ ] Security headers configuration (CSP, HSTS, X-Frame-Options)
- [ ] Secrets management strategy (environment variables, Azure Key Vault?)

**Accessibility Checklist** (WCAG 2.1 AA):
- [ ] Keyboard navigation for all auth forms
- [ ] Screen reader labels for form fields
- [ ] Error messages announced to screen readers
- [ ] Color contrast ratios meet standards
- [ ] Focus indicators visible
- [ ] Form validation accessible
- [ ] Alternative text for any icons/images
- [ ] No timing-based restrictions without extensions

**Performance Checklist**:
- [x] Login response time target: < 2 seconds
- [x] Registration completion time: < 3 minutes
- [ ] Token refresh mechanism doesn't block UI
- [ ] Database queries optimized (indexed fields)
- [ ] API response size minimized (only necessary fields)
- [x] Rate limiting prevents abuse without impacting legitimate users

**Documentation Checklist**:
- [ ] API endpoint documentation (Swagger/OpenAPI)
- [ ] Authentication flow diagrams
- [ ] Setup instructions for developers
- [ ] Environment variables documented
- [ ] Error code reference guide
- [ ] Testing procedures documented
- [ ] Security best practices guide
- [ ] Troubleshooting guide

**Gaps Identified**: 9 items need attention before proceeding to implementation.
**Readiness Assessment**: 75% ready - address gaps in clarify phase.
```

### /speckit.tasks

```
Generate detailed, actionable tasks for implementing User Authentication & Profile Management feature.

**Prerequisites**:
- [ ] Project solution created (backend + frontend)
- [ ] Database server running (PostgreSQL)
- [ ] Development environment configured
- [ ] Git repository initialized

---

**PHASE 1: Database & Domain Models** (Tasks 1-8)

**Task 1.1: Create User Entity**
- File: `backend/WahadiniCryptoQuest.Domain/Entities/User.cs`
- Create User class with all auth properties (Id, Email, Username, PasswordHash, EmailVerified, etc.)
- Add data annotations for validation
- Add navigation properties
- Acceptance: Entity compiles without errors, includes all 20+ required fields

**Task 1.2: Configure ASP.NET Identity**
- File: `backend/WahadiniCryptoQuest.Infrastructure/Data/ApplicationDbContext.cs`
- Create ApplicationDbContext inheriting IdentityDbContext<User>
- Configure DbSets for all entities
- Add OnModelCreating for custom configurations
- Acceptance: DbContext compiles, Identity tables will be created

**Task 1.3: Create Database Migration**
- Command: `dotnet ef migrations add InitialAuthSetup`
- Review generated migration for correctness
- Acceptance: Migration creates Users table with all fields, indexes on Email and Username

**Task 1.4: Configure JWT Settings**
- File: `backend/WahadiniCryptoQuest.API/appsettings.json`
- Add JwtSettings section (SecretKey, Issuer, Audience, ExpirationMinutes)
- Document environment variables needed
- Acceptance: Configuration structure ready for Program.cs

**Task 1.5: Create Auth DTOs**
- Files: `backend/WahadiniCryptoQuest.Application/DTOs/Auth/`
  - RegisterRequestDto.cs (Email, Username, Password)
  - LoginRequestDto.cs (Email, Password)
  - AuthResponseDto.cs (Token, RefreshToken, User)
  - PasswordResetRequestDto.cs
  - PasswordResetDto.cs
- Add validation attributes
- Acceptance: All DTOs compile with proper validation

**Task 1.6: Create IAuthService Interface**
- File: `backend/WahadiniCryptoQuest.Application/Interfaces/IAuthService.cs`
- Define method signatures for all auth operations
- Acceptance: Interface defines 8+ authentication methods

**Task 1.7: Apply Database Migration**
- Command: `dotnet ef database update`
- Verify tables created in PostgreSQL
- Acceptance: Database contains Users, Roles, and Identity tables

**Task 1.8: Create User Repository**
- Files:
  - `backend/WahadiniCryptoQuest.Application/Interfaces/IUserRepository.cs`
  - `backend/WahadiniCryptoQuest.Infrastructure/Repositories/UserRepository.cs`
- Implement GetByEmailAsync, GetByUsernameAsync, etc.
- Acceptance: Repository compiles, ready for service layer

---

**PHASE 2: Backend API Implementation** (Tasks 2.1-2.12)

**Task 2.1: Configure JWT in Program.cs**
- File: `backend/WahadiniCryptoQuest.API/Program.cs`
- Add Authentication service with JWT Bearer
- Configure token validation parameters
- Add Authorization service
- Acceptance: App starts without errors, auth middleware configured

**Task 2.2: Implement AuthService - User Registration**
- File: `backend/WahadiniCryptoQuest.Application/Services/AuthService.cs`
- Implement RegisterAsync method
- Validate username/email uniqueness
- Hash password with BCrypt
- Generate email verification token
- Acceptance: User can be created in database with hashed password

**Task 2.3: Implement AuthService - Login**
- Method: LoginAsync in AuthService
- Verify credentials against database
- Generate JWT access token
- Generate refresh token
- Acceptance: Returns valid JWT token for correct credentials

**Task 2.4: Implement JWT Token Generation**
- Method: GenerateJwtToken in AuthService
- Add claims (UserId, Email, Role, SubscriptionTier)
- Sign with secret key
- Set expiration
- Acceptance: Generated tokens validate correctly

**Task 2.5: Implement Refresh Token Logic**
- Method: RefreshTokenAsync in AuthService
- Validate refresh token
- Generate new access token
- Optionally rotate refresh token
- Acceptance: Expired access tokens can be refreshed

**Task 2.6: Implement Email Verification**
- Methods: GenerateEmailVerificationToken, VerifyEmailAsync
- Create time-limited verification token
- Update EmailVerified status
- Acceptance: Email verification flow works end-to-end

**Task 2.7: Implement Password Reset - Request**
- Method: SendPasswordResetAsync in AuthService
- Generate password reset token (60-minute expiry)
- Send email with reset link
- Acceptance: Reset email sent to valid user email

**Task 2.8: Implement Password Reset - Confirm**
- Method: ResetPasswordAsync in AuthService
- Validate reset token
- Update password hash
- Invalidate all sessions
- Acceptance: User can login with new password

**Task 2.9: Create AuthController - Registration Endpoint**
- File: `backend/WahadiniCryptoQuest.API/Controllers/AuthController.cs`
- POST /api/auth/register endpoint
- Validate request body
- Call AuthService.RegisterAsync
- Return appropriate response
- Acceptance: API returns 201 Created with user data

**Task 2.10: Create AuthController - Login Endpoint**
- Endpoint: POST /api/auth/login
- Validate credentials
- Return JWT tokens
- Set refresh token in httpOnly cookie (optional)
- Acceptance: API returns 200 OK with tokens

**Task 2.11: Create AuthController - Other Endpoints**
- POST /api/auth/refresh-token
- POST /api/auth/forgot-password
- POST /api/auth/reset-password
- GET /api/auth/verify-email/{token}
- Acceptance: All endpoints return appropriate responses

**Task 2.12: Implement Rate Limiting**
- Package: AspNetCoreRateLimit
- Configure rate limiting (5 attempts per 15 minutes for login)
- Add middleware to Program.cs
- Acceptance: Excessive requests return 429 Too Many Requests

---

**PHASE 3: Frontend Authentication** (Tasks 3.1-3.10)

**Task 3.1: Create Auth Store**
- File: `frontend/src/stores/authStore.ts`
- Define AuthState interface
- Implement login, register, logout actions
- Add token storage logic
- Acceptance: Store compiles with TypeScript

**Task 3.2: Create API Service**
- File: `frontend/src/services/api.ts`
- Create axios instance with base URL
- Add request interceptor for auth token
- Add response interceptor for token refresh
- Acceptance: API client configured correctly

**Task 3.3: Create Auth Service**
- File: `frontend/src/services/authService.ts`
- Implement login, register, logout functions
- Handle token storage (localStorage or cookies)
- Decode JWT to get user info
- Acceptance: Service methods call backend correctly

**Task 3.4: Create Registration Form**
- File: `frontend/src/components/auth/RegisterForm.tsx`
- Build form with email, username, password fields
- Add validation (zod schema)
- Handle form submission
- Show success/error messages
- Acceptance: Form submits to backend, shows validation errors

**Task 3.5: Create Login Form**
- File: `frontend/src/components/auth/LoginForm.tsx`
- Build form with email, password fields
- Add "Remember me" checkbox
- Handle form submission
- Redirect to dashboard on success
- Acceptance: User can login and is redirected

**Task 3.6: Create Registration Page**
- File: `frontend/src/pages/RegisterPage.tsx`
- Include RegisterForm component
- Add link to login page
- Style with TailwindCSS
- Acceptance: Page renders, navigation works

**Task 3.7: Create Login Page**
- File: `frontend/src/pages/LoginPage.tsx`
- Include LoginForm component
- Add "Forgot password?" link
- Add link to registration
- Acceptance: Page renders, all links work

**Task 3.8: Create Protected Route Component**
- File: `frontend/src/components/auth/ProtectedRoute.tsx`
- Check authentication state
- Redirect to login if not authenticated
- Show loading spinner while checking
- Acceptance: Routes protected correctly

**Task 3.9: Implement Axios Interceptors**
- Update: `frontend/src/services/api.ts`
- Add Authorization header in request interceptor
- Handle 401 errors in response interceptor
- Attempt token refresh before redirecting to login
- Acceptance: Expired tokens refreshed automatically

**Task 3.10: Create Forgot Password Flow**
- Files:
  - `frontend/src/pages/ForgotPasswordPage.tsx`
  - `frontend/src/pages/ResetPasswordPage.tsx`
- Request reset email form
- Reset password form with token
- Acceptance: Complete password reset flow works

---

**PHASE 4: Profile Management** (Tasks 4.1-4.6)

**Task 4.1: Create UsersController**
- File: `backend/WahadiniCryptoQuest.API/Controllers/UsersController.cs`
- GET /api/users/profile - get current user
- PUT /api/users/profile - update profile
- GET /api/users/stats - get user statistics
- Acceptance: Endpoints return user data correctly

**Task 4.2: Implement Profile Update Logic**
- Update AuthService or create UserService
- Handle username/email changes (uniqueness check)
- Update profile fields
- Acceptance: Profile updates persist to database

**Task 4.3: Create Profile Page**
- File: `frontend/src/pages/ProfilePage.tsx`
- Display user information
- Show edit form with pre-filled data
- Save button to update profile
- Acceptance: Users can view and edit profile

**Task 4.4: Implement Data Export**
- Backend: GET /api/users/export-data
- Generate JSON export of user data
- Include GDPR-required data
- Acceptance: Returns downloadable JSON file

**Task 4.5: Implement Account Deletion**
- Backend: DELETE /api/users/account
- Mark account for deletion (30-day retention)
- Scheduled job for final deletion (future task)
- Frontend: Account deletion page with confirmation
- Acceptance: Account marked for deletion

**Task 4.6: Create Account Settings Page**
- File: `frontend/src/pages/AccountSettingsPage.tsx`
- Change password section
- Privacy settings
- Data export button
- Delete account button
- Acceptance: All account management features accessible

---

**PHASE 5: Testing & Security** (Tasks 5.1-5.8)

**Task 5.1: Write Auth Service Unit Tests**
- File: `backend/WahadiniCryptoQuest.Tests/Services/AuthServiceTests.cs`
- Test registration logic
- Test login validation
- Test token generation
- Test password reset flow
- Acceptance: 80%+ code coverage for AuthService

**Task 5.2: Write Auth Controller Integration Tests**
- File: `backend/WahadiniCryptoQuest.Tests/Integration/AuthControllerTests.cs`
- Test all auth endpoints
- Test rate limiting
- Test validation errors
- Acceptance: All endpoints tested with various inputs

**Task 5.3: Frontend Auth Tests**
- Files: `frontend/src/__tests__/auth/`
- Test auth store actions
- Test form validation
- Test protected route behavior
- Acceptance: Jest tests pass

**Task 5.4: Security Audit - Token Handling**
- Review token storage mechanism
- Check token expiration handling
- Verify secure token transmission (HTTPS)
- Test logout clears all tokens
- Acceptance: No security vulnerabilities found

**Task 5.5: Security Audit - Password Handling**
- Verify BCrypt implementation
- Check password complexity requirements
- Test password reset token expiry
- Verify password not logged anywhere
- Acceptance: Password handling secure

**Task 5.6: Test Rate Limiting**
- Make 10 rapid login requests
- Verify 429 response after limit
- Check rate limit reset
- Acceptance: Rate limiting works correctly

**Task 5.7: Test Session Management**
- Login from 3 devices
- Attempt login from 4th device
- Verify oldest session terminated
- Test logout from one device
- Acceptance: Session limits enforced

**Task 5.8: E2E Authentication Flow Test**
- Use Playwright or Cypress
- Test complete registration → email verification → login flow
- Test password reset flow
- Test profile update
- Acceptance: E2E tests pass

---

**Task Summary**:
- Total Tasks: 46
- Estimated Effort: 12-15 days
- Dependencies: Must complete phases in order
- Testing: Integrated throughout, formal testing in Phase 5

**Definition of Done** (for all tasks):
- Code written and reviewed
- Unit tests passing
- Integration tests passing (where applicable)
- Documentation updated
- Security checklist verified
- Accessibility verified (for frontend)
- Merged to development branch
```

### /speckit.implement

```
Execute the implementation of User Authentication & Profile Management feature using the specified tasks and plan.

**Implementation Strategy**: Follow the phase-by-phase approach with continuous testing and security verification at each step.

**Current Phase**: Phase 1 - Database & Domain Models

**Step-by-Step Execution Guide**:

### Step 1: Environment Setup
```bash
# Backend setup
cd backend
dotnet new sln -n WahadiniCryptoQuest
dotnet new webapi -n WahadiniCryptoQuest.API
dotnet new classlib -n WahadiniCryptoQuest.Domain
dotnet new classlib -n WahadiniCryptoQuest.Application
dotnet new classlib -n WahadiniCryptoQuest.Infrastructure
dotnet new xunit -n WahadiniCryptoQuest.Tests

# Add projects to solution
dotnet sln add **/*.csproj

# Add project references
cd WahadiniCryptoQuest.API
dotnet add reference ../WahadiniCryptoQuest.Application
dotnet add reference ../WahadiniCryptoQuest.Infrastructure

cd ../WahadiniCryptoQuest.Application
dotnet add reference ../WahadiniCryptoQuest.Domain

cd ../WahadiniCryptoQuest.Infrastructure
dotnet add reference ../WahadiniCryptoQuest.Domain
dotnet add reference ../WahadiniCryptoQuest.Application

# Install NuGet packages
cd ../WahadiniCryptoQuest.API
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Swashbuckle.AspNetCore
dotnet add package Serilog.AspNetCore

cd ../WahadiniCryptoQuest.Infrastructure
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore

cd ../WahadiniCryptoQuest.Application
dotnet add package FluentValidation
dotnet add package BCrypt.Net-Next

# Frontend setup
cd ../../frontend
npm create vite@latest . -- --template react-ts
npm install react-router-dom axios zustand
npm install react-hook-form zod @hookform/resolvers
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p
```

### Step 2: Implement Phase 1 Tasks (Database & Domain)

**Task 1.1: Create User Entity**
- Location: `backend/WahadiniCryptoQuest.Domain/Entities/User.cs`
- Follow the User entity specification from data-model.md
- Include all properties: Id, Email, Username, PasswordHash, Role, etc.
- Add validation attributes

**Task 1.2-1.3: Configure DbContext and Create Migration**
- Create ApplicationDbContext with ASP.NET Identity
- Configure entity relationships
- Run: `dotnet ef migrations add InitialAuthSetup`
- Review and apply migration

**Task 1.4-1.6: DTOs and Interfaces**
- Create all auth DTOs in Application layer
- Define IAuthService interface
- Define IUserRepository interface

### Step 3: Implement Phase 2 Tasks (Backend API)

**Configure JWT Authentication**
- Update Program.cs with JWT configuration
- Set up authentication middleware
- Configure authorization policies

**Implement AuthService**
- Create AuthService class implementing IAuthService
- Implement each method according to task specifications
- Add proper error handling and validation

**Create AuthController**
- Implement all endpoints as specified
- Add proper HTTP status codes
- Include API documentation attributes

**Add Rate Limiting**
- Configure rate limiting middleware
- Test with multiple rapid requests

### Step 4: Implement Phase 3 Tasks (Frontend)

**Create Auth Store**
- Use Zustand for state management
- Implement login, register, logout actions
- Handle token storage securely

**Build Auth Forms**
- Create registration and login forms
- Add form validation with zod
- Implement error handling and user feedback

**Create Protected Routes**
- Implement route guards
- Add loading states
- Handle redirects properly

### Step 5: Implement Phase 4 Tasks (Profile Management)

**Backend Profile Endpoints**
- Create UsersController
- Implement profile CRUD operations
- Add data export functionality

**Frontend Profile Pages**
- Build profile view and edit UI
- Create account settings page
- Implement account deletion flow

### Step 6: Execute Phase 5 Tasks (Testing & Security)

**Write Unit Tests**
- Test AuthService methods
- Mock dependencies
- Achieve 80%+ coverage

**Integration Tests**
- Test API endpoints
- Test authentication flows
- Verify rate limiting

**Security Audit**
- Review token handling
- Check password security
- Verify HTTPS enforcement
- Test session management

**E2E Tests**
- Complete user registration flow
- Test login/logout
- Verify profile management

### Quality Gates (must pass before moving to next phase):

**After Phase 1**:
- [ ] Database migration applies successfully
- [ ] All entities compile without errors
- [ ] Entity relationships correctly defined

**After Phase 2**:
- [ ] All API endpoints return correct status codes
- [ ] JWT tokens generated and validated correctly
- [ ] Rate limiting works as expected
- [ ] Postman/Swagger tests pass

**After Phase 3**:
- [ ] User can register and login via UI
- [ ] Protected routes block unauthenticated access
- [ ] Token refresh works automatically
- [ ] Forms validate correctly

**After Phase 4**:
- [ ] User can view and edit profile
- [ ] Account settings accessible
- [ ] Data export generates correct file
- [ ] Account deletion marks for deletion

**After Phase 5**:
- [ ] All unit tests pass (80%+ coverage)
- [ ] All integration tests pass
- [ ] Security audit completed with no critical issues
- [ ] E2E tests pass
- [ ] Performance benchmarks met

### Continuous Practices Throughout Implementation:

1. **Code Review**: Every PR reviewed before merging
2. **Testing**: Write tests alongside implementation
3. **Documentation**: Update API docs as endpoints created
4. **Security**: Run security checks after each phase
5. **Constitution Check**: Verify compliance with project constitution

### Rollback Plan:
- If critical issue found, revert to last stable phase
- Each phase should be independently deployable
- Keep database migrations reversible

### Success Criteria:
- All 46 tasks completed
- All tests passing
- Security audit passed
- Feature deployed to staging
- Ready for user acceptance testing

**Next Steps After Implementation**:
1. Deploy to staging environment
2. Conduct user acceptance testing
3. Performance testing under load
4. Security penetration testing
5. Deploy to production
6. Monitor for issues

Execute tasks in order, ensuring each phase is complete and tested before proceeding to the next.
```

---

## Feature 2: Course Catalog & Content Discovery

### /speckit.specify

```
Create a comprehensive course catalog and content discovery system with categories, filtering, and search capabilities.

**Feature Name**: Course Catalog & Content Discovery

**User Story**: A learner browses available crypto courses by category, filters by difficulty and type (free/premium), searches for specific topics, and enrolls in courses to begin learning.

**Core Requirements**:

1. **Category Management**
   - Multiple course categories (DeFi, NFTs, GameFi, Trading, Airdrops, etc.)
   - Category icons and descriptions
   - Display order configuration
   - Active/inactive status control

2. **Course Browsing**
   - Grid/list view of available courses
   - Course cards displaying: thumbnail, title, description, difficulty, duration, reward points, premium badge
   - Category-based filtering
   - Difficulty level filtering (Beginner, Intermediate, Advanced)
   - Free vs Premium filtering
   - Search by title and description
   - Sort options (newest, popular, highest points, difficulty)

3. **Course Detail Page**
   - Comprehensive course information
   - Lesson list with duration and points
   - Total reward points display
   - Instructor information
   - Enrollment count and completion rate
   - Course prerequisites
   - Enrollment button with premium access check
   - Progress indicator for enrolled users

4. **Course Enrollment**
   - One-click enrollment for free courses
   - Premium access verification for premium courses
   - Enrollment tracking and history
   - "My Courses" dashboard for enrolled courses
   - Continue learning from last position

5. **Course Discovery Features**
   - Featured courses section
   - Trending courses based on enrollment
   - Recommended courses based on user progress
   - Recently added courses

**Technical Requirements**:
- Backend: CoursesController, CategoriesController with pagination
- Frontend: Course catalog page, course detail page, search/filter components
- Database: Course, Category, Lesson, UserCourseEnrollment entities
- Caching: Category and featured course lists

**Success Criteria**:
- Users find relevant courses within 60 seconds
- Search results load in <1 second
- 85% of users successfully enroll in first course
- Course detail pages load in <2 seconds
```

### /speckit.plan

```
Create an implementation plan for Course Catalog & Content Discovery feature.

**Phase Breakdown**:

**Phase 1: Database Schema** (2 days)
- Create Category entity with relationships
- Create Course entity with all metadata
- Create Lesson entity linked to courses
- Create UserCourseEnrollment entity
- Set up database indexes for performance

**Phase 2: Backend API** (4-5 days)
- Implement CategoriesController (GET categories, admin CRUD)
- Implement CoursesController (GET courses with filters, search, enrollment)
- Create CourseService with business logic
- Add pagination helpers
- Implement caching for categories
- Create enrollment logic with access checks

**Phase 3: Admin Course Management** (3-4 days)
- Create admin endpoints for course CRUD operations
- Build course creation/edit forms
- Add course publishing workflow
- Implement course analytics endpoints

**Phase 4: Frontend Catalog** (4-5 days)
- Build course catalog page with grid/list views
- Create course card components
- Implement filter and search UI
- Add pagination/infinite scroll
- Build course detail page
- Create enrollment flow

**Phase 5: Discovery Features** (2-3 days)
- Build featured courses section
- Implement recommendation algorithm
- Create "My Courses" dashboard
- Add continue learning functionality

**Dependencies**:
- User authentication system complete
- File upload system for thumbnails
- Image optimization/CDN

**Risks**:
- Search performance with large course catalogs
- Recommendation algorithm accuracy
- Image loading performance
```

### /speckit.clarify

```
Identify and clarify ambiguous areas in the Course Catalog & Content Discovery feature before implementation.

**Areas Requiring Clarification**:

1. **Course Visibility and Access Control**
   - Can unpublished courses be visible to admins for preview?
   - Should draft courses show in instructor's personal view?
   - What happens to enrolled users when a course is unpublished?
   - How to handle course visibility during updates (temporary unavailability)?

2. **Search and Filtering Strategy**
   - Should search index course descriptions, lesson titles, or both?
   - What's the search ranking algorithm (relevance, popularity, recency)?
   - How to handle typos and fuzzy matching?
   - Should search include instructor names and tags?

3. **Image Management**
   - Where to store course thumbnails (local, AWS S3, Cloudinary)?
   - What image sizes and formats are required (thumbnails, full-size)?
   - Should images be optimized/compressed automatically?
   - What's the maximum file size limit for uploads?

4. **Course Recommendation Algorithm**
   - What factors determine recommendations (category, difficulty, user progress)?
   - How many courses to recommend (3, 5, 10)?
   - Should recommendations include completed courses?
   - How frequently should recommendations refresh?

5. **Enrollment Business Rules**
   - Can users unenroll from courses?
   - What happens to progress if user unenrolls then re-enrolls?
   - Should there be enrollment limits per user?
   - Can instructors limit course enrollment numbers?

6. **Course Updates After Enrollment**
   - How to handle lesson additions to courses with enrolled users?
   - Should users be notified of course content updates?
   - What if a lesson is removed that user already completed?
   - How to handle course restructuring (lesson reordering)?

7. **Performance and Caching**
   - Which data should be cached (categories, featured courses)?
   - How long should cache remain valid?
   - How to invalidate cache when admin updates categories?
   - Should course listings be paginated or infinite scroll?

**Clarification Needed Before Implementation**: Document answers in spec before proceeding to tasks phase.
```

### /speckit.analyze

```
Perform cross-artifact consistency and alignment analysis for Course Catalog & Content Discovery feature.

**Analysis Areas**:

1. **Specification-to-Plan Alignment**
   - ✅ Check: All core requirements (5 areas) mapped to implementation phases
   - ✅ Check: Category management included in Phase 1 (database) and Phase 2 (API)
   - ✅ Check: Course browsing, filtering, search covered in Phase 4 (frontend)
   - ✅ Check: Discovery features (recommended, featured) in Phase 5
   - ❓ Gap: Course thumbnail upload not explicitly in timeline
   - ❓ Gap: Search indexing strategy not detailed in backend phase

2. **Database Schema Alignment**
   - ✅ Check: Category entity matches data-model.md specification
   - ✅ Check: Course entity includes all metadata (title, description, difficulty, etc.)
   - ✅ Check: Lesson entity linked to Course with OrderIndex
   - ✅ Check: UserCourseEnrollment tracks enrollment and progress
   - ❓ Gap: Need index on Course.Title and Course.Description for search performance
   - ❓ Gap: CourseView entity needed for tracking view counts?

3. **API Design Consistency**
   - ✅ Check: GET /api/categories follows RESTful convention
   - ✅ Check: GET /api/courses supports query parameters for filtering
   - ✅ Check: POST /api/courses/{id}/enroll follows enrollment pattern
   - ❓ Gap: Need to define exact query parameter names (category, difficulty, isPremium, search, sort, page, limit)
   - ❓ Gap: Response format for paginated lists not standardized

4. **Frontend-Backend Contract**
   - ✅ Check: Course catalog page expects paginated course list
   - ✅ Check: Filter components need category list API
   - ✅ Check: Enrollment button calls enrollment endpoint
   - ❓ Gap: CourseDto structure not defined (what fields to include?)
   - ❓ Gap: Enrollment response format not specified

5. **Feature Dependencies**
   - ✅ Check: Depends on Feature 1 (auth system) for enrollment
   - ✅ Check: Provides data for Feature 3 (video player) - lesson information
   - ⚠️ Warning: Feature 6 (subscriptions) needed for premium content gating
   - ❓ Gap: How does this feature integrate with Feature 5 (reward points) for point display?

6. **Constitution Compliance**
   - ✅ Learning-First: Course catalog supports educational discovery
   - ✅ Security: Enrollment checks user authentication
   - ✅ Scalability: Pagination and caching included
   - ✅ Accessibility: Need to ensure catalog is keyboard navigable
   - ❓ Gap: Accessibility requirements not in acceptance criteria

**Recommendations**:
1. Define CourseDto, CategoryDto contracts before frontend work
2. Add database indexes for search performance (Course.Title, Course.Description)
3. Document query parameter naming convention
4. Create image upload service before Phase 3
5. Add accessibility checklist to Phase 4 tasks
6. Define cache invalidation strategy

**Alignment Score**: 80% - Good foundation, needs detail refinement before tasks.
```

### /speckit.checklist

```
Generate comprehensive quality checklists for Course Catalog & Content Discovery feature.

**Requirements Completeness Checklist**:
- [x] Category CRUD operations defined
- [x] Course browsing with filters specified
- [x] Search functionality described
- [x] Course detail page requirements
- [x] Enrollment flow documented
- [x] Discovery features (featured, recommended) included
- [ ] Image upload specifications missing
- [ ] Search algorithm not detailed
- [ ] Cache strategy needs documentation
- [ ] Pagination vs infinite scroll not decided
- [ ] Course update notifications not specified
- [ ] Unenroll functionality not addressed

**Clarity Checklist**:
- [x] User stories clear and testable
- [x] UI components described
- [ ] Search ranking algorithm ambiguous
- [ ] Image storage location unclear
- [ ] Recommendation algorithm not detailed
- [ ] Cache duration not specified
- [x] Enrollment success/error flows defined
- [ ] DTO structures not documented

**Consistency Checklist**:
- [x] Naming follows convention (Course, Category, Enrollment)
- [x] RESTful API design consistent
- [ ] Query parameter names need standardization
- [ ] Response format for lists needs template
- [x] Database entities align with API
- [ ] Error response format needs definition

**Implementation Readiness Checklist**:
- [x] Database schema ready
- [x] API endpoints listed
- [ ] DTOs need full definition
- [ ] Image upload service selection needed
- [ ] Cache implementation library not chosen
- [x] Frontend component structure clear
- [ ] Search library (ElasticSearch, built-in PostgreSQL) not decided
- [x] Testing strategy implied

**Performance Checklist**:
- [x] Pagination included for scalability
- [x] Caching strategy mentioned
- [ ] Database query optimization plan needed
- [ ] Image CDN integration not specified
- [ ] Search index optimization not detailed
- [x] Response time targets defined (<1s search, <2s detail page)

**Accessibility Checklist** (WCAG 2.1 AA):
- [ ] Keyboard navigation for course catalog
- [ ] Screen reader support for course cards
- [ ] Focus indicators for interactive elements
- [ ] Alt text for course thumbnails
- [ ] ARIA labels for filters and search
- [ ] High contrast mode support
- [ ] Responsive design for screen magnification

**Security Checklist**:
- [x] Enrollment requires authentication
- [x] Premium content gating mentioned
- [ ] File upload validation needed
- [ ] XSS prevention for course descriptions
- [ ] SQL injection prevention (parameterized queries)
- [ ] Rate limiting on search endpoint

**Feature Integration Checklist**:
- [x] Authentication integration for enrollment
- [x] Premium subscription check for gated content
- [ ] Reward points display integration
- [ ] Video player integration (lesson links)
- [ ] Admin dashboard integration (course management)

**Gaps Identified**: 15 items need attention before implementation.
**Readiness Assessment**: 70% ready - address gaps in clarify and plan refinement.
```

### /speckit.tasks

```
Generate detailed, actionable tasks for implementing Course Catalog & Content Discovery feature.

**Prerequisites**:
- [x] User authentication system complete (Feature 1)
- [ ] Image upload service selected and configured
- [ ] Database migrations from Feature 1 applied

---

**PHASE 1: Database Schema** (Tasks 1.1-1.8, 2 days)

**Task 1.1: Create Category Entity**
- File: `backend/WahadiniCryptoQuest.Domain/Entities/Category.cs`
- Create Category class with properties: Id, Name, Description, IconUrl, DisplayOrder, IsActive
- Add navigation property to Courses
- Add validation attributes
- Acceptance: Entity compiles, includes all 7 required properties

**Task 1.2: Create Course Entity**
- File: `backend/WahadiniCryptoQuest.Domain/Entities/Course.cs`
- Create Course class with 15+ properties (see data-model.md)
- Include: Id, CategoryId, Title, Description, ThumbnailUrl, DifficultyLevel, EstimatedDuration, IsPremium, RewardPoints, etc.
- Add navigation properties (Category, Lessons, Enrollments)
- Add validation attributes (Title length, positive duration, etc.)
- Acceptance: Entity compiles with all relationships

**Task 1.3: Create Lesson Entity**
- File: `backend/WahadiniCryptoQuest.Domain/Entities/Lesson.cs`
- Create Lesson class with properties as specified in data-model.md
- Link to Course via CourseId foreign key
- Add OrderIndex for lesson sequencing
- Acceptance: Entity compiles with Course relationship

**Task 1.4: Create UserCourseEnrollment Entity**
- File: `backend/WahadiniCryptoQuest.Domain/Entities/UserCourseEnrollment.cs`
- Track user enrollments with enrollment date, progress, completion
- Link to User and Course
- Add unique constraint on UserId + CourseId
- Acceptance: Entity compiles with relationships

**Task 1.5: Configure Entity Relationships in DbContext**
- File: `backend/WahadiniCryptoQuest.Infrastructure/Data/ApplicationDbContext.cs`
- Configure one-to-many: Category → Courses
- Configure one-to-many: Course → Lessons
- Configure many-to-many: Users ↔ Courses (via UserCourseEnrollment)
- Use Fluent API for cascade delete rules
- Acceptance: Relationships configured correctly

**Task 1.6: Create Database Indexes**
- Add indexes for performance:
  - Category: Name (unique)
  - Course: Title, CategoryId, IsPublished, IsPremium
  - Course: (Title, Description) for full-text search
  - Lesson: CourseId, OrderIndex
  - UserCourseEnrollment: UserId, CourseId, EnrolledAt
- Acceptance: Indexes added to migration

**Task 1.7: Create Database Migration**
- Command: `dotnet ef migrations add AddCourseCatalog`
- Review migration for all tables and indexes
- Acceptance: Migration creates 4 tables with proper relationships

**Task 1.8: Seed Initial Categories**
- File: `backend/WahadiniCryptoQuest.Infrastructure/Data/DbInitializer.cs`
- Seed 8-10 categories: DeFi, NFTs, GameFi, Trading, Airdrops, Staking, DAOs, Web3
- Set DisplayOrder and icons
- Acceptance: Categories appear in database after seed

---

**PHASE 2: Backend API** (Tasks 2.1-2.12, 4-5 days)

**Task 2.1: Create Category DTOs**
- Files: `backend/WahadiniCryptoQuest.Application/DTOs/Category/`
  - CategoryDto.cs (Id, Name, Description, IconUrl, DisplayOrder)
  - CreateCategoryDto.cs (Name, Description, IconUrl, DisplayOrder)
  - UpdateCategoryDto.cs (same as Create)
- Acceptance: DTOs compile with validation attributes

**Task 2.2: Create Course DTOs**
- Files: `backend/WahadiniCryptoQuest.Application/DTOs/Course/`
  - CourseDto.cs (all course fields + category name)
  - CourseListDto.cs (simplified for catalog listing)
  - CreateCourseDto.cs (for admin creation)
  - UpdateCourseDto.cs
  - CourseDetailDto.cs (includes lessons list)
- Acceptance: 5 DTOs created with appropriate fields

**Task 2.3: Create ICategoryRepository and Implementation**
- Interface: `backend/WahadiniCryptoQuest.Application/Interfaces/ICategoryRepository.cs`
- Implementation: `backend/WahadiniCryptoQuest.Infrastructure/Repositories/CategoryRepository.cs`
- Methods: GetAllAsync, GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync, GetActiveAsync
- Acceptance: Repository implements CRUD operations

**Task 2.4: Create ICourseRepository and Implementation**
- Interface and implementation files
- Methods: GetAllAsync(filters, pagination), GetByIdAsync, GetByCategoryAsync, SearchAsync, AddAsync, UpdateAsync, DeleteAsync, GetEnrolledCoursesAsync
- Include filtering and sorting logic
- Acceptance: Repository handles complex queries

**Task 2.5: Create CategoryService**
- File: `backend/WahadiniCryptoQuest.Application/Services/CategoryService.cs`
- Implement GetAllCategoriesAsync, GetCategoryByIdAsync
- Admin methods: CreateCategoryAsync, UpdateCategoryAsync, DeleteCategoryAsync
- Add caching for GetAllCategoriesAsync (5-minute cache)
- Acceptance: Service implements business logic with caching

**Task 2.6: Create CourseService**
- File: `backend/WahadiniCryptoQuest.Application/Services/CourseService.cs`
- Implement GetCoursesAsync(filters, pagination, sort)
- Implement SearchCoursesAsync(query, pagination)
- Implement GetCourseDetailAsync(courseId, userId)
- Implement EnrollUserAsync(userId, courseId)
- Implement GetUserCoursesAsync(userId)
- Check premium access in EnrollUserAsync
- Acceptance: All methods implemented with proper validation

**Task 2.7: Create CategoriesController**
- File: `backend/WahadiniCryptoQuest.API/Controllers/CategoriesController.cs`
- GET /api/categories - get all active categories
- GET /api/categories/{id} - get category details
- GET /api/categories/{id}/courses - get courses in category
- POST /api/categories - admin only, create category
- PUT /api/categories/{id} - admin only, update
- DELETE /api/categories/{id} - admin only, soft delete
- Acceptance: 6 endpoints with proper HTTP methods

**Task 2.8: Create CoursesController**
- File: `backend/WahadiniCryptoQuest.API/Controllers/CoursesController.cs`
- GET /api/courses - get courses with filters (category, difficulty, isPremium, search, sort, page, limit)
- GET /api/courses/{id} - get course details
- POST /api/courses/{id}/enroll - enroll user (authenticated)
- GET /api/courses/my-courses - get user's enrolled courses
- POST /api/courses - admin only, create course
- PUT /api/courses/{id} - admin only, update
- DELETE /api/courses/{id} - admin only, soft delete
- Acceptance: 7 endpoints, returns appropriate status codes

**Task 2.9: Implement Pagination Helper**
- File: `backend/WahadiniCryptoQuest.Application/Helpers/PaginationHelper.cs`
- Create PaginatedResult<T> class with Items, TotalCount, Page, PageSize, TotalPages
- Create extension method for IQueryable.ToPaginatedListAsync
- Acceptance: Helper works with any entity type

**Task 2.10: Implement Category Caching**
- Use IMemoryCache for categories
- Cache GetAllCategories for 5 minutes
- Invalidate cache on category CRUD operations
- Acceptance: Categories cached, admin updates clear cache

**Task 2.11: Add Authorization to Admin Endpoints**
- Add [Authorize(Policy = "RequireAdmin")] to admin methods
- Acceptance: Non-admin users get 403 Forbidden

**Task 2.12: Test API Endpoints with Postman/Swagger**
- Create test collection for all endpoints
- Test success scenarios
- Test error scenarios (not found, unauthorized, validation)
- Acceptance: All endpoints return expected responses

---

**PHASE 3: Admin Course Management** (Tasks 3.1-3.6, 3-4 days)

**Task 3.1: Create Course Creation Endpoint**
- Enhance POST /api/courses
- Accept course metadata and lesson list in single request
- Validate all fields
- Set initial status as Draft
- Acceptance: Admin can create complete course

**Task 3.2: Create Course Update Endpoint**
- Enhance PUT /api/courses/{id}
- Allow updating all course fields
- Handle lesson additions/updates/deletions
- Preserve enrollments during updates
- Acceptance: Course can be fully edited

**Task 3.3: Create Course Publishing Workflow**
- POST /api/admin/courses/{id}/publish
- Validate course has at least 1 lesson
- Change IsPublished to true
- Send notification to followers (future enhancement)
- Acceptance: Course becomes visible after publishing

**Task 3.4: Add Course Analytics Endpoint**
- GET /api/admin/courses/{id}/analytics
- Return: view count, enrollment count, completion rate, average progress
- Acceptance: Returns meaningful metrics

**Task 3.5: Implement Course Duplication**
- POST /api/admin/courses/{id}/duplicate
- Copy course with all lessons
- Set new course as Draft
- Append "(Copy)" to title
- Acceptance: Duplicate created successfully

**Task 3.6: Add File Upload for Course Thumbnails**
- POST /api/courses/upload-thumbnail
- Accept image file (PNG, JPG, max 2MB)
- Validate file type and size
- Store in configured storage (local/cloud)
- Return URL
- Acceptance: Image uploaded and URL returned

---

**PHASE 4: Frontend Catalog** (Tasks 4.1-4.12, 4-5 days)

**Task 4.1: Create Course Types**
- File: `frontend/src/types/course.types.ts`
- Define Course, Category, Enrollment, Lesson interfaces
- Match backend DTOs
- Acceptance: TypeScript interfaces complete

**Task 4.2: Create Course API Service**
- File: `frontend/src/services/courseService.ts`
- Implement getCourses(filters, pagination)
- Implement searchCourses(query)
- Implement getCourseDetail(id)
- Implement enrollInCourse(id)
- Implement getMyEnrollments()
- Acceptance: All API calls implemented

**Task 4.3: Create Category API Service**
- File: `frontend/src/services/categoryService.ts`
- Implement getCategories()
- Implement getCategoryById(id)
- Acceptance: Category API calls work

**Task 4.4: Create Course Store (Zustand)**
- File: `frontend/src/stores/courseStore.ts`
- State: courses, selectedCourse, categories, loading, error
- Actions: fetchCourses, fetchCourseDetail, enrollCourse, fetchMyEnrollments
- Acceptance: Store manages course state

**Task 4.5: Create CourseCard Component**
- File: `frontend/src/components/course/CourseCard.tsx`
- Display: thumbnail, title, short description, difficulty badge, duration, reward points, premium badge
- Clickable to course detail
- Acceptance: Card renders with all info

**Task 4.6: Create CourseFilters Component**
- File: `frontend/src/components/course/CourseFilters.tsx`
- Category dropdown (multi-select or tabs)
- Difficulty select
- Free/Premium toggle
- Sort dropdown
- Acceptance: All filters functional

**Task 4.7: Create Search Component**
- File: `frontend/src/components/course/SearchBar.tsx`
- Search input with debounce (300ms)
- Clear button
- Search on Enter or after debounce
- Acceptance: Search calls API after debounce

**Task 4.8: Create CoursesPage**
- File: `frontend/src/pages/CoursesPage.tsx`
- Header with title and search
- Filters sidebar/top bar
- Course grid with CourseCards
- Pagination or infinite scroll
- Loading skeleton
- Empty state for no results
- Acceptance: Full catalog browsing works

**Task 4.9: Create CourseDetailPage**
- File: `frontend/src/pages/CourseDetailPage.tsx`
- Course header (title, description, instructor, difficulty, duration)
- Lesson list with progress indicators
- Total reward points display
- Enroll button (check premium access)
- Prerequisites section
- Acceptance: Detail page shows all course info

**Task 4.10: Implement Enrollment Flow**
- Click enroll button → check authentication
- If not logged in, redirect to login
- If premium course and user not premium, show upgrade modal
- If eligible, call enroll API
- On success, update UI and redirect to first lesson
- Acceptance: Enrollment works with all checks

**Task 4.11: Create EnrollmentModal Component**
- Show course title and enrollment confirmation
- Display next steps after enrollment
- Link to first lesson
- Acceptance: Modal shows after successful enrollment

**Task 4.12: Add Responsive Design**
- Mobile: single column, simplified filters
- Tablet: 2-column grid
- Desktop: 3-4 column grid, sidebar filters
- Acceptance: Works on all screen sizes

---

**PHASE 5: Discovery Features** (Tasks 5.1-5.6, 2-3 days)

**Task 5.1: Implement Featured Courses Endpoint**
- GET /api/courses/featured
- Return admin-selected featured courses
- Add IsFeatured flag to Course entity
- Acceptance: Endpoint returns featured courses

**Task 5.2: Implement Trending Courses Logic**
- GET /api/courses/trending
- Calculate based on recent enrollments (last 7 days)
- Acceptance: Returns courses with high recent enrollment

**Task 5.3: Implement Course Recommendations**
- GET /api/courses/recommended
- Based on user's enrolled course categories
- Based on user's difficulty progress
- Exclude already enrolled courses
- Acceptance: Returns relevant recommendations

**Task 5.4: Create FeaturedCourses Component**
- File: `frontend/src/components/course/FeaturedCourses.tsx`
- Display 3-5 featured courses prominently
- Use on home page and catalog top
- Acceptance: Featured section displays

**Task 5.5: Create MyCourses Dashboard**
- File: `frontend/src/pages/MyCoursesPage.tsx`
- Show enrolled courses with progress
- "Continue Learning" section with recent courses
- Filter: In Progress, Completed
- Acceptance: User sees enrolled courses

**Task 5.6: Implement Continue Learning Feature**
- Track last accessed lesson per course
- Display "Continue" button with last lesson
- Clicking continues from that lesson
- Acceptance: User can resume learning easily

---

**PHASE 6: Testing** (Tasks 6.1-6.5, 2 days)

**Task 6.1: Backend Unit Tests**
- File: `backend/WahadiniCryptoQuest.Tests/Services/CourseServiceTests.cs`
- Test GetCoursesAsync with various filters
- Test EnrollUserAsync (success, premium check, duplicate enrollment)
- Test SearchCoursesAsync
- Acceptance: 80%+ coverage for CourseService

**Task 6.2: Backend Integration Tests**
- File: `backend/WahadiniCryptoQuest.Tests/Integration/CoursesControllerTests.cs`
- Test all API endpoints
- Test pagination
- Test filtering and search
- Test enrollment flow
- Acceptance: All endpoints tested

**Task 6.3: Frontend Component Tests**
- Test CourseCard rendering
- Test CourseFilters functionality
- Test Search debounce
- Acceptance: Components render correctly

**Task 6.4: Frontend Integration Tests**
- Test complete catalog browsing flow
- Test course detail and enrollment
- Test filter combinations
- Acceptance: User flows work end-to-end

**Task 6.5: E2E Tests**
- Use Playwright
- Test: Browse → Filter → Search → View Detail → Enroll → Start Learning
- Test premium content gating
- Acceptance: E2E scenarios pass

---

**Task Summary**:
- Total Tasks: 51
- Estimated Effort: 15-18 days
- Dependencies: Feature 1 (auth) must be complete
- Critical Path: Database → Backend API → Frontend Catalog

**Definition of Done**:
- All tasks completed and tested
- Code reviewed and merged
- API documented in Swagger
- Accessibility verified
- Performance benchmarks met
```

### /speckit.implement

```
Execute the implementation of Course Catalog & Content Discovery feature using the specified tasks.

**Implementation Strategy**: Build foundation first (database, backend API), then user-facing features (catalog UI), finally polish (discovery features).

**Current Phase**: Phase 1 - Database Schema

---

### Step 1: Environment Setup (if not done)

```bash
# Ensure Feature 1 complete and database accessible
cd backend
dotnet build

# Verify connection to PostgreSQL
dotnet ef database update
```

### Step 2: Execute Phase 1 Tasks (Database Schema)

**Task 1.1-1.4: Create Entities**

Create Category entity:
```csharp
// backend/WahadiniCryptoQuest.Domain/Entities/Category.cs
public class Category
{
    public Guid Id { get; set; }
    
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public string? IconUrl { get; set; }
    
    public int DisplayOrder { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
```

Create Course and Lesson entities following data-model.md specifications.

**Task 1.5-1.6: Configure Relationships and Indexes**

In ApplicationDbContext.cs:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // Category configuration
    modelBuilder.Entity<Category>(entity =>
    {
        entity.HasIndex(e => e.Name).IsUnique();
        entity.HasIndex(e => e.DisplayOrder);
    });
    
    // Course configuration
    modelBuilder.Entity<Course>(entity =>
    {
        entity.HasIndex(e => e.Title);
        entity.HasIndex(e => new { e.CategoryId, e.IsPublished, e.IsPremium });
        
        entity.HasOne(c => c.Category)
              .WithMany(cat => cat.Courses)
              .HasForeignKey(c => c.CategoryId)
              .OnDelete(DeleteBehavior.Restrict);
    });
    
    // ... more configurations
}
```

**Task 1.7-1.8: Migration and Seed**

```bash
dotnet ef migrations add AddCourseCatalog
dotnet ef database update
```

### Step 3: Execute Phase 2 Tasks (Backend API)

**Create DTOs** (Tasks 2.1-2.2)
**Create Repositories** (Tasks 2.3-2.4)
**Create Services** (Tasks 2.5-2.6)

Example CourseService.GetCoursesAsync:
```csharp
public async Task<PaginatedResult<CourseListDto>> GetCoursesAsync(
    CourseFilters filters,
    int page = 1,
    int pageSize = 12)
{
    var query = _context.Courses
        .Include(c => c.Category)
        .Where(c => c.IsPublished);
    
    // Apply filters
    if (filters.CategoryId.HasValue)
        query = query.Where(c => c.CategoryId == filters.CategoryId.Value);
    
    if (filters.DifficultyLevel.HasValue)
        query = query.Where(c => c.DifficultyLevel == filters.DifficultyLevel.Value);
    
    if (filters.IsPremium.HasValue)
        query = query.Where(c => c.IsPremium == filters.IsPremium.Value);
    
    if (!string.IsNullOrWhiteSpace(filters.SearchQuery))
    {
        var search = filters.SearchQuery.ToLower();
        query = query.Where(c => 
            c.Title.ToLower().Contains(search) ||
            c.Description.ToLower().Contains(search));
    }
    
    // Apply sorting
    query = filters.SortBy switch
    {
        "popular" => query.OrderByDescending(c => c.ViewCount),
        "points" => query.OrderByDescending(c => c.RewardPoints),
        "difficulty" => query.OrderBy(c => c.DifficultyLevel),
        _ => query.OrderByDescending(c => c.CreatedAt) // newest
    };
    
    // Paginate
    return await query.ToPaginatedListAsync(page, pageSize, _mapper);
}
```

**Create Controllers** (Tasks 2.7-2.8)

CoursesController example:
```csharp
[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<CourseListDto>>> GetCourses(
        [FromQuery] CourseFilters filters,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var result = await _courseService.GetCoursesAsync(filters, page, pageSize);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<CourseDetailDto>> GetCourse(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var course = await _courseService.GetCourseDetailAsync(id, userId);
        
        if (course == null)
            return NotFound();
        
        return Ok(course);
    }
    
    [Authorize]
    [HttpPost("{id}/enroll")]
    public async Task<IActionResult> EnrollInCourse(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        try
        {
            await _courseService.EnrollUserAsync(userId, id);
            return Ok(new { message = "Successfully enrolled in course" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message); // Premium required
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

### Step 4: Execute Phase 3 Tasks (Admin Management)

Implement admin endpoints for course creation, editing, publishing.

### Step 5: Execute Phase 4 Tasks (Frontend Catalog)

**Create Types and Services** (Tasks 4.1-4.3)

Example course service:
```typescript
// frontend/src/services/courseService.ts
export const courseService = {
  async getCourses(filters: CourseFilters, page: number = 1) {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: '12',
      ...(filters.categoryId && { categoryId: filters.categoryId }),
      ...(filters.difficulty && { difficulty: filters.difficulty }),
      ...(filters.isPremium !== undefined && { isPremium: filters.isPremium.toString() }),
      ...(filters.search && { search: filters.search }),
      ...(filters.sort && { sort: filters.sort }),
    });
    
    const response = await api.get(`/courses?${params}`);
    return response.data;
  },
  
  async getCourseDetail(id: string) {
    const response = await api.get(`/courses/${id}`);
    return response.data;
  },
  
  async enrollInCourse(id: string) {
    const response = await api.post(`/courses/${id}/enroll`);
    return response.data;
  },
  
  async getMyEnrollments() {
    const response = await api.get('/courses/my-courses');
    return response.data;
  },
};
```

**Create Components** (Tasks 4.5-4.12)

Build CourseCard, CourseFilters, SearchBar, CoursesPage, CourseDetailPage following the designs.

### Step 6: Execute Phase 5 Tasks (Discovery Features)

Add featured courses, trending, recommendations, and "My Courses" dashboard.

### Step 7: Execute Phase 6 (Testing)

Run all tests and fix issues.

### Quality Gates:

**After Phase 1**:
- [ ] Database migration successful
- [ ] All entities created with relationships
- [ ] Indexes created for performance

**After Phase 2**:
- [ ] API endpoints return correct data
- [ ] Filtering and search work
- [ ] Pagination returns correct page sizes
- [ ] Swagger documentation complete

**After Phase 3**:
- [ ] Admin can create courses via API
- [ ] Course publishing workflow works
- [ ] File upload for thumbnails works

**After Phase 4**:
- [ ] Catalog page displays courses
- [ ] Filters and search functional
- [ ] Course detail page shows all info
- [ ] Enrollment flow works end-to-end

**After Phase 5**:
- [ ] Featured courses display
- [ ] Recommendations relevant
- [ ] "My Courses" shows enrolled courses

**After Phase 6**:
- [ ] All unit tests pass (80%+ coverage)
- [ ] All integration tests pass
- [ ] E2E tests pass
- [ ] Performance benchmarks met

### Success Criteria:
- Users can browse and search courses within 60 seconds
- Search results load in <1 second
- Course detail pages load in <2 seconds
- Enrollment success rate >95%
- All accessibility requirements met

**Next Steps After Implementation**:
1. Deploy to staging
2. User acceptance testing
3. Load testing with 100+ courses
4. Move to Feature 3 (Video Player)
```

---

## Feature 3: Video Player & Progress Tracking

### /speckit.specify

```
Create a robust YouTube video player with progress tracking, resume functionality, and completion detection.

**Feature Name**: Video Player & Progress Tracking

**User Story**: A learner watches video lessons with automatic progress saving, resumes from last position when returning, and earns reward points upon completing 80% of the video.

**Core Requirements**:

1. **YouTube Video Integration**
   - Embed YouTube videos using react-player library
   - Video playback with YouTube's native controls
   - Playback speed control
   - Quality selection
   - Fullscreen support
   - Keyboard shortcuts (Space, F, Arrow keys)
   - Handle video unavailability with fallback to cached description

2. **Progress Tracking**
   - Track video position every 10 seconds
   - Save progress to backend automatically
   - Calculate completion percentage (watched time / total duration)
   - Resume from last saved position on return
   - Show progress bar overlay on video
   - Handle page refresh/navigation gracefully
   - Sync progress across devices

3. **Completion Detection**
   - Mark lesson as completed when 80% watched (configurable)
   - Award reward points on completion
   - Prevent duplicate point awards
   - Show completion badge on completed lessons
   - Update course progress percentage

4. **Access Control**
   - Verify user enrollment before loading video
   - Check premium status for premium lessons
   - Show upgrade prompt for non-premium users
   - Validate YouTube video ID exists

5. **Resume Experience**
   - Show "Resume from X:XX" prompt when returning
   - Auto-seek to last position or start from beginning
   - Clear resume data after completion

**Technical Requirements**:
- Frontend: react-player, progress tracking hooks
- Backend: LessonController with progress endpoints
- Database: UserProgress entity with watch position
- CDN: Adaptive bitrate streaming for video delivery

**Success Criteria**:
- Progress saves within 10 seconds of watching
- Resume functionality works 100% of the time
- Completion detection accuracy >99%
- Video loads within 3 seconds on average
- Zero duplicate point awards
```

### /speckit.plan

```
Create an implementation plan for Video Player & Progress Tracking feature.

**Phase Breakdown**:

**Phase 1: Database & Backend** (2-3 days)
- Create UserProgress entity
- Implement LessonService with progress methods
- Create progress update endpoint (PUT /api/lessons/{id}/progress)
- Create lesson completion endpoint
- Implement point award logic
- Add course completion check

**Phase 2: Video Player Component** (3-4 days)
- Integrate react-player library
- Build LessonPlayer component
- Implement progress tracking logic
- Add auto-save every 10 seconds
- Create resume functionality
- Add completion detection

**Phase 3: Lesson Page** (3 days)
- Build lesson page layout
- Add video player integration
- Show lesson content below video
- Add previous/next lesson navigation
- Implement breadcrumb navigation
- Create premium content gate

**Phase 4: Progress Indicators** (2 days)
- Add progress bar overlay on video
- Create completion badge
- Build course progress tracking
- Add progress percentage to course detail

**Phase 5: Edge Cases & Testing** (2 days)
- Handle video unavailability
- Test concurrent playback prevention
- Verify skip detection (optional)
- Test progress sync across devices
- Ensure point award idempotency

**Dependencies**:
- Course catalog feature complete
- Reward system for point awards
- User authentication system

**Risks**:
- YouTube API rate limits
- Network issues during progress saves
- Browser storage limitations
```

---

## Feature 4: Multi-Type Task Verification System

### /speckit.specify

```
Create a comprehensive task verification system supporting five task types with automated and manual approval workflows.

**Feature Name**: Multi-Type Task Verification System

**User Story**: A learner completes various task types (quizzes, wallet verifications, screenshots, text submissions, external links) to verify understanding, receives automated approval for simple tasks or manual review for complex submissions, and earns reward points upon approval.

**Core Requirements**:

1. **Task Types**

**Quiz Tasks**:
- Multiple choice questions with 4 options
- Auto-grading with 80% passing threshold (configurable)
- Immediate feedback on submission
- Show correct answers after completion
- Allow retakes (configurable per task)
- Track attempt history

**Wallet Verification Tasks**:
- Connect MetaMask or wallet via WalletConnect
- Verify wallet ownership through message signing
- Check minimum balance requirements
- Verify specific token holdings
- Auto-approve when conditions met
- Support multiple networks (Ethereum, BSC, Polygon)

**Screenshot Tasks**:
- File upload with drag-and-drop
- Image validation (PNG, JPG, max 5MB)
- Preview uploaded image
- Manual admin review required
- Admin can approve/reject with feedback
- Store files securely (local or cloud storage)

**Text Submission Tasks**:
- Textarea with word counter
- Minimum/maximum word count validation
- Plagiarism check (optional integration)
- Manual admin review required
- Admin provides written feedback
- Allow resubmission after rejection

**External Link Tasks**:
- Input field for transaction hash or wallet address
- URL format validation
- Manual admin review initially
- Future: blockchain verification integration
- Admin verifies action completion

2. **Submission Workflow**:
- One pending submission per user-task combination
- Submit button with loading state
- Show validation errors clearly
- Display submission status (Not Started, Pending, Approved, Rejected)
- Allow resubmission for rejected tasks
- Track attempt history

3. **Admin Review Interface**:
- Review queue with filters (task type, date range, course)
- Tabs for Pending, Approved, Rejected, All
- Submission preview (full details, files, text)
- Approve/Reject actions with optional feedback
- Bulk approve/reject functionality
- Request resubmission option
- Review time tracking

4. **Reward System Integration**:
- Award points automatically on approval
- Prevent duplicate point awards
- Different point values per task type
- Bonus points for streak completion
- Track points awarded per task

5. **Notification System**:
- Email notification on task approval
- Email notification on task rejection with feedback
- In-app notification badges
- Admin notifications for pending reviews

**Technical Requirements**:
- Backend: TaskController, TaskSubmissionService
- Frontend: Dynamic task submission forms, admin review interface
- Database: Task, UserTaskSubmission, FileUpload entities
- File Storage: Local storage or Cloudinary free tier

**Success Criteria**:
- Quiz auto-approval within 1 second
- Manual review completed within 24 hours average
- Task submission success rate >95%
- Zero duplicate point awards
- File upload success rate >98%
```

### /speckit.plan

```
Create an implementation plan for Multi-Type Task Verification System.

**Phase Breakdown**:

**Phase 1: Database & Domain** (2-3 days)
- Create Task entity with TaskData JSONB structure
- Create UserTaskSubmission entity
- Create FileUpload entity for screenshot tasks
- Define task type schemas
- Set up database indexes

**Phase 2: Backend Core** (4-5 days)
- Implement TaskController with submission endpoint
- Create TaskSubmissionService with validation logic
- Implement auto-approval for Quiz tasks
- Implement auto-approval for WalletVerification tasks
- Create manual review queue endpoint
- Add approval/rejection endpoints

**Phase 3: Quiz Task Implementation** (2-3 days)
- Build quiz submission validation
- Implement grading logic
- Create quiz UI component
- Add immediate feedback display
- Implement retake logic

**Phase 4: File Upload Tasks** (3-4 days)
- Implement file upload endpoint
- Add file validation (type, size)
- Set up file storage (local/cloud)
- Create screenshot submission UI
- Add image preview functionality
- Implement virus scanning (optional)

**Phase 5: Other Task Types** (3-4 days)
- Build wallet verification integration (MetaMask)
- Implement text submission with word counter
- Create external link submission form
- Add validation for each type

**Phase 6: Admin Review Interface** (4-5 days)
- Build review queue page
- Create submission detail modal
- Implement approve/reject actions
- Add feedback text input
- Build bulk action functionality
- Add filtering and search

**Phase 7: Notifications & Points** (2-3 days)
- Integrate reward point awards
- Implement email notifications
- Add in-app notifications
- Ensure idempotency for point awards

**Dependencies**:
- Reward system for point awards
- Email service configuration
- File storage setup
- Web3 library for wallet integration

**Risks**:
- File upload reliability
- Wallet integration complexity
- Review queue scalability
- Point award race conditions
```

---

## Feature 5: Reward Points & Gamification System

### /speckit.specify

```
Create a comprehensive reward points economy with earning mechanisms, leaderboards, achievements, and discount redemption.

**Feature Name**: Reward Points & Gamification System

**User Story**: A learner earns points through various activities (completing lessons, tasks, daily streaks, referrals), competes on leaderboards, unlocks achievements, and redeems points for subscription discounts to enhance motivation and engagement.

**Core Requirements**:

1. **Point Earning Mechanisms**:
- Lesson completion: 10 points (configurable per lesson)
- Quiz task: 5 points
- External link task: 3 points
- Screenshot task: 8 points
- Text submission: 12 points
- Wallet verification: 15 points
- Course completion bonus: 20% of total lesson/task points
- Daily login streak: 2x multiplier after 7 days
- Referral completion: 50 points per referred user who completes first course

2. **Anti-Gaming Protection**:
- Rate limiting: 10 points per user per hour maximum
- Duplicate submission detection via content hashing
- IP-based velocity monitoring (50 submissions per IP per day)
- Transaction auditing with immutable ledger
- ML-based pattern detection for suspicious activity

3. **Points Ledger**:
- Immutable RewardTransaction records
- Track transaction type (Earned, Redeemed, Bonus, Penalty, Expired)
- Reference entity (TaskId, CourseId, DiscountCodeId)
- Description field for transparency
- Current balance = SUM of all transactions

4. **Leaderboard System**:
- All-time leaderboard
- Monthly leaderboard
- Weekly leaderboard
- Top 100 users displayed
- Show current user rank even if not in top 100
- Cache and refresh hourly

5. **Achievement System**:
- First Steps: Complete first lesson (10 bonus points)
- Task Master: Complete 10 tasks (25 bonus points)
- Course Conqueror: Complete first course (50 bonus points)
- Knowledge Seeker: Complete 5 courses (100 bonus points)
- Crypto Pro: Complete an advanced course (75 bonus points)
- Point Hoarder: Reach 5000 points (200 bonus points)
- Generous: Refer 3 friends (150 bonus points)

6. **Discount Code Redemption**:
- SAVE10: 10% off monthly (costs 500 points)
- SAVE20: 20% off monthly (costs 1000 points)
- SAVE30: 30% off yearly (costs 2000 points)
- FREE7DAYS: 7-day trial (costs 200 points)
- Monthly redemption cap: 500 points with rollover
- One-time use per user per discount type
- Expiry dates on codes

7. **Rewards Page**:
- Large points balance display
- Transaction history table with pagination
- Available discount codes grid
- Redeem buttons with point requirements
- Leaderboard section
- Achievement showcase

**Technical Requirements**:
- Backend: RewardsController, RewardService
- Frontend: Rewards page, leaderboard component, achievement badges
- Database: RewardTransaction, Achievement, UserAchievement, DiscountCode entities
- Caching: Leaderboard caching with hourly refresh

**Success Criteria**:
- Zero duplicate point awards
- Point balance accuracy 100%
- Leaderboard updates within 1 hour
- Redemption success rate >99%
- No point economy exploits
```

---

## Feature 6: Premium Subscription & Payment Processing

### /speckit.specify

```
Create a secure subscription and payment system with Stripe integration, premium content gating, and discount code application.

**Feature Name**: Premium Subscription & Payment Processing

**User Story**: A user upgrades to premium subscription to access exclusive content and unlimited task submissions, applies earned reward points as discounts, completes secure payment through Stripe, and immediately gains access to premium features.

**Core Requirements**:

1. **Subscription Tiers**:

**Free Tier**:
- Access to free courses only
- Limited task submissions (5 per day)
- Basic community access
- Ads displayed (optional)

**Monthly Premium ($9.99/month)**:
- Access to all courses (free + premium)
- Unlimited task submissions
- Priority support
- Ad-free experience
- Early access to new courses

**Yearly Premium ($99/year - 17% discount)**:
- All monthly benefits
- Exclusive advanced courses
- 1-on-1 consultation call (quarterly)
- Certificate of completion

2. **Stripe Integration**:
- Stripe Checkout Session creation
- Secure payment processing (PCI compliant)
- Webhook handling for payment events
- Customer and subscription ID storage
- Invoice management
- Payment method management

3. **Subscription Lifecycle**:
- Upgrade from free to premium
- Downgrade from premium to free (keep access until period end)
- Subscription renewal automation
- Payment failure handling (3-day grace period)
- Cancellation processing (access until period end)
- Subscription expiry management

4. **Discount Application**:
- Apply redeemed discount codes at checkout
- Validate discount code availability
- Calculate discounted price
- Mark discount as used after payment
- Support multiple discount types (percentage off)

5. **Premium Content Gating**:
- Check user subscription status before content access
- Show upgrade prompt for premium content
- Immediate access upon successful payment
- Preserve progress during subscription changes
- Handle expired subscriptions gracefully

6. **Billing Management**:
- View current subscription status
- See next renewal date
- Access invoice history
- Download invoice PDFs
- Update payment method via Stripe portal
- Cancel subscription with confirmation

**Technical Requirements**:
- Backend: SubscriptionsController, Stripe.NET SDK
- Frontend: Pricing page, checkout flow, billing management
- Stripe: Webhook endpoint, test mode initially
- Database: User subscription fields, payment tracking

**Success Criteria**:
- Payment processing success rate >99%
- Immediate premium access after payment
- Webhook processing within 30 seconds
- Zero unauthorized premium access
- Refund processing within 24 hours
```

---

## Feature 7: Admin Dashboard & Content Management

### /speckit.specify

```
Create a comprehensive admin dashboard for platform management, content creation, user administration, and analytics.

**Feature Name**: Admin Dashboard & Content Management

**User Story**: An administrator creates and manages courses with lessons and tasks, reviews user task submissions with approval/rejection workflows, monitors user activity and platform analytics, and maintains content quality and platform integrity.

**Core Requirements**:

1. **Admin Dashboard Overview**:
- Key metrics cards (total users, premium users, revenue, active courses)
- Pending task reviews count (clickable)
- Revenue trend chart (last 12 months)
- User signup chart (last 30 days)
- Course enrollment pie chart (by category)
- Recent activities feed

2. **Course Management**:
- Create new courses with rich text editor
- Add/edit/delete lessons within courses
- Drag-and-drop lesson reordering
- Add tasks to lessons with type selection
- Publish/unpublish courses
- Duplicate existing courses
- Course analytics (views, enrollments, completion rate)

3. **Lesson Management**:
- YouTube video URL input with ID extraction
- Video duration auto-fetch or manual input
- Lesson content with markdown support
- Premium/free designation
- Reward points configuration
- Task attachment

4. **Task Management**:
- Task type selector (5 types)
- Dynamic form based on task type
- Quiz: add questions with options and correct answers
- ExternalLink: instructions and expected action
- Screenshot: upload requirements
- TextSubmission: prompt and word limits
- WalletVerification: token and balance requirements
- Reward points per task

5. **Task Review Queue**:
- Tabs for Pending, Approved, Rejected, All
- Filters by task type, date range, course
- Submission table with user, task, preview, actions
- Screenshot preview (thumbnail with fullscreen modal)
- Text preview (truncated with expand)
- Quiz score display
- Approve/Reject buttons
- Feedback textarea
- Bulk approve/reject
- Request resubmission option

6. **User Management**:
- User search (email, username)
- Filters (role, subscription, status)
- User table (avatar, username, email, role, subscription, points, joined date)
- View user details modal
- Grant/revoke premium manually
- Adjust user reward points
- Ban/unban users
- Delete users (soft delete)
- Export users to CSV

7. **Analytics & Reporting**:
- Revenue section (MRR, ARR, revenue by plan)
- User section (total, premium, free, conversion rate, churn rate)
- Engagement section (DAU, MAU, avg session duration)
- Content section (top courses, completion rates)
- CSV export for all reports

8. **Discount Code Management**:
- Create discount codes
- Set point requirements
- Configure discount percentage
- Set max redemptions
- Set expiry dates
- Deactivate codes
- View redemption statistics

9. **Platform Settings**:
- Site name and logo
- Default reward points per lesson
- Max free tasks per day
- Email notification settings
- Stripe keys configuration (masked)
- Maintenance mode toggle

**Technical Requirements**:
- Backend: Admin controllers with role authorization
- Frontend: Admin layout, dashboard components, data tables
- Charts: Recharts or Chart.js
- Authorization: "RequireAdmin" policy

**Success Criteria**:
- Admin can create complete course in <15 minutes
- Review queue processing <2 minutes per submission
- Analytics load in <3 seconds
- Single admin can manage 100+ daily reviews
```

---

## Implementation Sequence

Use the following sequence for systematic implementation:

### Phase 1: Foundation (Weeks 1-2)
1. Feature 1: User Authentication & Profile Management
2. Set up project structure (backend + frontend)
3. Configure database and migrations

### Phase 2: Core Learning (Weeks 3-5)
4. Feature 2: Course Catalog & Content Discovery
5. Feature 3: Video Player & Progress Tracking
6. Basic admin course creation

### Phase 3: Engagement (Weeks 6-7)
7. Feature 4: Multi-Type Task Verification System (start with Quiz only)
8. Feature 5: Reward Points & Gamification System (basic points only)

### Phase 4: Monetization (Week 8-9)
9. Feature 6: Premium Subscription & Payment Processing
10. Expand Feature 4: Add remaining task types

### Phase 5: Management (Week 10-11)
11. Feature 7: Admin Dashboard & Content Management
12. Expand Feature 5: Add leaderboards and achievements

### Phase 6: Polish (Week 12)
13. Testing, bug fixes, performance optimization
14. Documentation and deployment

---

## Getting Started

1. **Initialize speckit for Feature 1**:
```bash
/speckit.specify # Use the Feature 1 specification above
/speckit.plan    # Generate implementation plan
/speckit.tasks   # Break down into actionable tasks
/speckit.implement # Execute the implementation
```

2. **Move to Feature 2 after Feature 1 is complete and tested**

3. **Follow the implementation sequence** to build the platform systematically

4. **Use clarification commands when needed**:
```bash
/speckit.clarify # Ask structured questions for ambiguous areas
/speckit.analyze # Check consistency across features
/speckit.checklist # Validate requirements completeness
```

---

## Additional Resources

- **Database Schema**: See `001-platform-baseline-spec/data-model.md`
- **API Contracts**: Will be generated in `001-platform-baseline-spec/contracts/`
- **Original Specification**: See `001-platform-baseline-spec/spec.md`
- **Implementation Plan**: See `001-platform-baseline-spec/plan.md`

---

## Notes

- Each feature is designed to be independently testable
- Features build upon each other but maintain loose coupling
- Use test-driven development for critical business logic
- Implement security measures from day one
- Focus on MVP first, enhance iteratively
- Prioritize user experience and educational value over engagement metrics

---

## SpecKit Command Pattern for Remaining Features

**Note**: Feature 1 (User Authentication & Profile Management) includes complete examples of all speckit commands. Apply the same command structure to Features 2-7:

### Commands Included for Feature 1:
1. `/speckit.specify` - Full feature specification with requirements
2. `/speckit.plan` - Phase-by-phase implementation breakdown
3. `/speckit.clarify` - Questions to resolve ambiguities
4. `/speckit.analyze` - Cross-artifact consistency checks
5. `/speckit.checklist` - Quality validation checklists
6. `/speckit.tasks` - Detailed actionable tasks (46 tasks for Feature 1)
7. `/speckit.implement` - Step-by-step execution guide

### How to Apply to Features 2-7:

For each remaining feature (Course Catalog, Video Player, Task System, Rewards, Subscriptions, Admin Dashboard), follow this pattern:

1. **Start with `/speckit.specify`** - Adapt the specification template from Feature 1
2. **Run `/speckit.clarify`** first if you have ambiguous requirements
3. **Generate `/speckit.plan`** - Break down into 5-6 phases like Feature 1
4. **Check with `/speckit.analyze`** - Verify alignment with other features and database schema
5. **Validate with `/speckit.checklist`** - Ensure completeness before tasks
6. **Create `/speckit.tasks`** - Generate 30-50 detailed tasks per feature
7. **Execute with `/speckit.implement`** - Follow the implementation guide

### Feature 2-7 Quick Templates:

Each feature should include:
- **User Story**: Who, what, why
- **Core Requirements**: 5-8 major requirement areas
- **Technical Requirements**: Backend, frontend, database, third-party services
- **Success Criteria**: Measurable outcomes
- **Phase Breakdown**: 4-6 implementation phases (2-5 days each)
- **Dependencies**: What must be complete first
- **Risks**: Potential blockers
- **Clarification Questions**: 5-10 areas needing more detail
- **Analysis Checks**: Alignment with other features
- **Quality Checklists**: Completeness, clarity, consistency, security, accessibility
- **Tasks**: 30-50 actionable tasks with acceptance criteria
- **Implementation Guide**: Step-by-step execution with quality gates

### Example Structure for Feature 2 (Course Catalog):

```markdown
## Feature 2: Course Catalog & Content Discovery

### /speckit.specify
[Adapt from Feature 1 template, focus on catalog-specific requirements]

### /speckit.plan
**Phase 1**: Database Schema (Category, Course, Lesson entities)
**Phase 2**: Backend API (CoursesController, filtering, search)
**Phase 3**: Admin Course Management (CRUD operations)
**Phase 4**: Frontend Catalog (course cards, filters, search)
**Phase 5**: Discovery Features (recommendations, featured courses)

### /speckit.clarify
- Course visibility rules?
- Search ranking algorithm?
- Image upload and storage strategy?
- Course approval workflow for instructors?
- How to handle course updates after enrollment?

### /speckit.analyze
- Check: Course entity aligns with data-model.md
- Check: API endpoints match frontend needs
- Gap: Need course image optimization strategy
- Gap: Search performance for 1000+ courses

### /speckit.checklist
- [ ] All course metadata fields defined
- [ ] Search functionality specified
- [ ] Image upload limits documented
- [ ] Caching strategy defined
... [30+ items]

### /speckit.tasks
**Task 2.1**: Create Category Entity
**Task 2.2**: Create Course Entity with relationships
**Task 2.3**: Implement CategoriesController
... [40+ tasks total]

### /speckit.implement
**Step 1**: Database setup
**Step 2**: Backend API
**Step 3**: Admin CRUD
**Step 4**: Frontend catalog
**Quality Gates**: [After each phase]
```

### Time Estimates Per Feature:

- **Feature 1** (Auth): 12-15 days (✓ Complete documentation)
- **Feature 2** (Catalog): 10-12 days
- **Feature 3** (Video Player): 8-10 days
- **Feature 4** (Tasks): 15-18 days (most complex)
- **Feature 5** (Rewards): 10-12 days
- **Feature 6** (Subscriptions): 8-10 days
- **Feature 7** (Admin): 12-15 days

**Total Development Time**: 75-92 days (approximately 12-15 weeks for 2 developers)

### Best Practices:

1. **Always run commands in order**: specify → clarify (optional) → plan → analyze (optional) → checklist (optional) → tasks → implement
2. **Document clarification answers** before moving to tasks
3. **Address all gaps** found in analyze before implementation
4. **Check all checklist items** before declaring phase complete
5. **Test after each phase** in the implementation guide
6. **Review constitution compliance** for each feature

### Integration Points Between Features:

- **Feature 1 → Feature 2**: Auth system protects course enrollment endpoints
- **Feature 2 → Feature 3**: Course catalog provides data for video player
- **Feature 3 → Feature 4**: Video completion triggers task availability
- **Feature 4 → Feature 5**: Task approval awards points
- **Feature 5 → Feature 6**: Points unlock discount codes for subscriptions
- **Feature 6 → Features 2-4**: Premium status gates content access
- **Feature 7 → All**: Admin manages all platform content and users

### Command Reference Summary:

| Command | Purpose | When to Use | Output |
|---------|---------|-------------|---------|
| `/speckit.constitution` | Define principles | Project start | Guiding values |
| `/speckit.specify` | Detail requirements | Start of feature | Feature spec |
| `/speckit.clarify` | Resolve ambiguities | When uncertain | Questions list |
| `/speckit.plan` | Create timeline | After specify | Implementation plan |
| `/speckit.analyze` | Check consistency | Before tasks | Gap analysis |
| `/speckit.checklist` | Validate quality | Before implementation | Quality checks |
| `/speckit.tasks` | Break into actions | After plan | Task list |
| `/speckit.implement` | Execute build | After tasks | Implementation guide |

---

## Quick Start Guide

### For Solo Developer:
1. Week 1-2: Complete Feature 1 (Auth)
2. Week 3-4: Complete Feature 2 (Catalog) + Feature 3 (Video)
3. Week 5-7: Complete Feature 4 (Tasks) - most complex
4. Week 8-9: Complete Feature 5 (Rewards) + Feature 6 (Subscriptions)
5. Week 10-11: Complete Feature 7 (Admin)
6. Week 12: Integration testing, bug fixes, deployment

### For 2-Developer Team:
- **Developer A**: Backend (API, database, services)
- **Developer B**: Frontend (UI, state management, integration)
- **Both**: Review each other's code, integration work together

### Daily Workflow:
1. Morning: Review tasks for the day
2. Implement 2-3 tasks
3. Write tests for completed code
4. Afternoon: Code review or pair programming
5. Evening: Update task status, plan next day

### Weekly Milestones:
- **Week 1**: Auth backend complete
- **Week 2**: Auth frontend complete, user can register/login
- **Week 3**: Course catalog browsing works
- **Week 4**: Video player with progress tracking works
- **Week 5**: Quiz tasks work end-to-end
- **Week 6-7**: All task types implemented
- **Week 8**: Points system works
- **Week 9**: Stripe payment test mode works
- **Week 10**: Admin can create courses
- **Week 11**: Admin dashboard complete
- **Week 12**: Production ready

---

## Appendix: Complete Command Examples

Refer to **Feature 1: User Authentication & Profile Management** (above) for complete examples of all speckit commands. The same structure and level of detail should be applied to Features 2-7.

### Key Takeaways:

- **Specification**: Be detailed about requirements, include acceptance criteria
- **Planning**: Break into 5-6 phases, estimate days per phase
- **Clarification**: Ask 5-10 specific questions about ambiguities
- **Analysis**: Check 5 alignment areas, identify gaps
- **Checklist**: Create 20-40 validation items across 4-6 categories
- **Tasks**: Generate 30-50 actionable tasks with clear acceptance criteria
- **Implementation**: Provide step-by-step guide with quality gates

This systematic approach ensures nothing is missed and implementation proceeds smoothly with minimal rework.

---

### 2. /speckit.specify

```
Create a comprehensive baseline specification for WahadiniCryptoQuest platform based on the provided documentation.

**Platform Overview**: 
Gamified crypto education platform with YouTube video integration, multi-type task verification system, reward points economy, premium subscriptions with discount redemption, and comprehensive admin dashboard.

**Core Features to Specify**:

1. **Authentication & User Management**
   - JWT-based auth with refresh tokens
   - Role-based access (Free, Premium, Admin)
   - User profiles with progress tracking

2. **Course & Lesson System**
   - Category-based course organization
   - YouTube video integration with progress tracking
   - Resume functionality
   - Premium content gating

3. **Task Verification System**
   - 5 task types: Quiz, ExternalLink, Screenshot, TextSubmission, WalletVerification
   - Auto-approval vs manual review workflows
   - Admin review interface

4. **Reward & Gamification System**
   - Point earning mechanisms (lessons, tasks, streaks, referrals)
   - Discount code redemption system
   - Leaderboards and achievements
   - Point transaction ledger

5. **Subscription & Payment System**
   - Stripe integration (Free/Monthly/Yearly tiers)
   - Discount application from reward points
   - Subscription management

6. **Admin Dashboard**
   - Content management (courses, lessons, tasks)
   - User management and analytics
   - Task review system
   - Revenue and engagement analytics

**Technical Stack**:
- Frontend: React 18 + TypeScript + Vite + TailwindCSS
- Backend: .NET 8 Web API + Clean Architecture
- Database: PostgreSQL
- Authentication: JWT + ASP.NET Identity
- Payments: Stripe
- Video: YouTube API via react-player

**Success Criteria**:
- Support 1000+ concurrent users
- 99.9% uptime
- Mobile-responsive design
- Secure payment processing
- Real-time progress tracking
```

### 3. /speckit.plan

```
Create a detailed implementation plan for WahadiniCryptoQuest platform, breaking down the specification into phases and implementation streams.

**Planning Constraints**:
- Target: MVP in 8-12 weeks
- Team: 1-2 developers (full-stack capability)
- Budget: Free/low-cost services initially
- Quality: Production-ready, scalable architecture

**Phase Structure**:

**Phase 1: Foundation (Weeks 1-3)**
- Database schema and migrations
- Backend API structure with authentication
- Basic frontend setup with routing
- User registration/login flows

**Phase 2: Core Learning System (Weeks 4-6)**
- Course and lesson management
- YouTube video player integration
- Progress tracking system
- Basic task system (Quiz type first)

**Phase 3: Gamification & Rewards (Weeks 7-8)**
- Reward points system
- Task submission and review
- Basic admin dashboard
- Leaderboards

**Phase 4: Premium Features (Weeks 9-10)**
- Stripe payment integration
- Subscription management
- Premium content gating
- Discount redemption system

**Phase 5: Advanced Features & Polish (Weeks 11-12)**
- Advanced task types
- Comprehensive admin features
- Analytics dashboard
- Performance optimization

**Implementation Streams**:
1. Backend API Development
2. Frontend Component Development
3. Database Design & Migration
4. Integration & Testing
5. DevOps & Deployment

**Dependencies**:
- Stripe account setup
- YouTube API quotas
- Database hosting (PostgreSQL)
- Email service configuration
- Domain and SSL certificates

**Risk Mitigation**:
- Start with free tiers of all services
- Implement core learning loop first
- Progressive enhancement approach
- Comprehensive error handling
```

### 4. /speckit.tasks

```
Generate detailed, actionable tasks for implementing WahadiniCryptoQuest platform based on the implementation plan.

**Task Categories**:

**A. Backend Development Tasks**
- Set up .NET 8 solution structure (API, Domain, Application, Infrastructure)
- Implement Entity Framework models and DbContext
- Create repository pattern implementation
- Build JWT authentication system
- Develop course and lesson management APIs
- Implement task submission and review system
- Create reward points calculation engine
- Integrate Stripe payment webhooks
- Build admin analytics endpoints

**B. Frontend Development Tasks**
- Set up React + Vite + TypeScript project
- Create authentication store and components
- Build course browsing and enrollment UI
- Implement YouTube video player with progress tracking
- Create task submission forms (dynamic based on type)
- Build reward points and leaderboard components
- Develop admin dashboard with content management
- Implement subscription and payment flows

**C. Database Tasks**
- Design and create PostgreSQL schema
- Set up database migrations
- Create seed data for categories and initial content
- Implement database indexes for performance
- Set up backup and recovery procedures

**D. Integration Tasks**
- Connect frontend to backend APIs
- Implement YouTube video URL validation
- Set up Stripe webhook handling
- Configure email notification system
- Create file upload handling for screenshots

**E. Testing & Quality Assurance Tasks**
- Write unit tests for critical business logic
- Implement integration tests for API endpoints
- Create end-to-end tests for user journeys
- Perform security testing (authentication, authorization)
- Conduct performance testing with sample data

**F. DevOps & Deployment Tasks**
- Set up CI/CD pipeline
- Configure production environment variables
- Deploy to cloud hosting (Railway/Render/Vercel)
- Set up monitoring and logging
- Configure SSL and domain

**Task Prioritization**:
1. High Priority: Core learning functionality
2. Medium Priority: Gamification and rewards
3. Lower Priority: Advanced admin features and analytics

**Acceptance Criteria for Each Task**:
- Clear definition of done
- Testing requirements
- Performance benchmarks
- Security considerations
- Documentation needs
```

### 5. /speckit.implement

```
Execute the implementation of WahadiniCryptoQuest platform using the specified tasks and plan.

**Implementation Focus**:
Start with Phase 1 tasks and progress through the implementation plan systematically.

**Current Implementation Priority**:

**1. Backend Foundation**
- Create .NET 8 solution with Clean Architecture
- Implement database schema with Entity Framework
- Set up JWT authentication with ASP.NET Identity
- Create basic CRUD operations for Users, Courses, Lessons

**2. Frontend Foundation**
- Set up React project with TypeScript and Vite
- Implement authentication store and protected routes
- Create basic layout components (Header, Navigation, Footer)
- Build login and registration forms

**3. Core Learning Loop**
- Implement course browsing and enrollment
- Create lesson viewing with YouTube integration
- Add progress tracking and lesson completion
- Build basic task submission (Quiz type first)

**Implementation Guidelines**:
- Follow the prompts provided in the original documentation
- Use the specific GitHub Copilot Edits prompts (Prompts 1-12)
- Implement error handling and loading states
- Ensure responsive design from the start
- Add comprehensive logging
- Include input validation and sanitization

**Quality Gates**:
- Each feature must be tested before moving to next
- Database migrations must be reversible
- API endpoints must include proper error responses
- Frontend components must handle loading and error states
- Security measures must be implemented from day one

**Deployment Strategy**:
- Use free tiers initially (Supabase, Vercel, Railway)
- Implement environment-based configuration
- Set up automated deployments
- Configure monitoring and error tracking

**Success Metrics**:
- User can register, login, and browse courses
- Video playback works with progress saving
- Task submission and approval workflow functions
- Reward points are correctly calculated and displayed
- Admin can create and manage content
- Payment flow works end-to-end (test mode)
```

## Enhancement Commands (Optional)

### /speckit.clarify

```
Identify and clarify ambiguous areas in the WahadiniCryptoQuest specification before implementation.

**Areas to Clarify**:

1. **Task Verification Workflows**
   - How should auto-approval thresholds be configurable?
   - What's the escalation process for disputed task reviews?
   - How to handle bulk task approvals efficiently?

2. **Reward Point Economy**
   - What prevents point inflation or gaming the system?
   - How are bonus calculations handled for edge cases?
   - What's the redemption rate limits to prevent abuse?

3. **Content Scaling Strategy**
   - How will course creation scale beyond manual admin input?
   - What's the content moderation process for user-generated elements?
   - How to handle multiple instructors or content creators?

4. **Premium Feature Boundaries**
   - Exactly which features require premium vs free access?
   - How is the trial experience structured?
   - What happens to progress when subscriptions expire?

5. **Technical Scalability Questions**
   - What are the expected concurrent user limits?
   - How should video streaming be optimized?
   - What's the plan for database partitioning as data grows?

Ask structured questions to de-risk these areas before detailed planning.
```

### /speckit.analyze

```
Perform cross-artifact consistency and alignment analysis for WahadiniCryptoQuest specification.

**Analysis Areas**:

1. **Specification-to-Plan Alignment**
   - Verify all specified features are included in implementation plan
   - Check that technical stack choices support all requirements
   - Ensure timeline is realistic for scope

2. **Architecture Consistency**
   - Validate that frontend and backend designs align
   - Check database schema supports all user stories
   - Verify API design matches frontend needs

3. **Security Alignment**
   - Ensure authentication flows are consistent across components
   - Verify payment security measures are comprehensive
   - Check that admin access controls are properly designed

4. **User Experience Coherence**
   - Validate that gamification elements enhance rather than distract
   - Check that premium upgrade flows are clear and compelling
   - Ensure mobile experience is consistently designed

5. **Business Logic Consistency**
   - Verify reward calculations are consistently defined
   - Check that subscription tiers have clear value propositions
   - Ensure task types support the learning objectives

Generate a report highlighting any inconsistencies or gaps that need resolution.
```

### /speckit.checklist

```
Generate comprehensive quality checklists for WahadiniCryptoQuest platform requirements.

**Requirements Completeness Checklist**:
- [ ] All user roles and permissions clearly defined
- [ ] Complete user journey flows documented
- [ ] All API endpoints specified with request/response formats
- [ ] Database relationships and constraints documented
- [ ] Error handling scenarios covered
- [ ] Performance requirements quantified
- [ ] Security requirements detailed
- [ ] Integration points clearly defined

**Clarity Checklist**:
- [ ] Technical terms consistently used throughout
- [ ] User interface behaviors explicitly described
- [ ] Business rules unambiguously stated
- [ ] Edge cases and error conditions covered
- [ ] Acceptance criteria measurable and testable
- [ ] Dependencies clearly identified
- [ ] Assumptions explicitly stated

**Consistency Checklist**:
- [ ] Naming conventions consistent across documents
- [ ] Data types and formats standardized
- [ ] User interface patterns consistent
- [ ] API design patterns consistent
- [ ] Security approaches uniform
- [ ] Error handling patterns consistent
- [ ] Documentation style uniform

**Implementation Readiness Checklist**:
- [ ] All technical decisions documented
- [ ] Development environment requirements specified
- [ ] Testing strategy defined
- [ ] Deployment process outlined
- [ ] Monitoring and maintenance plans included
- [ ] Team responsibilities clarified
- [ ] Timeline and milestones realistic

Use these checklists to validate specification quality before implementation begins.
```

## Usage Instructions

1. **Go to the project folder**: `cd WahadiniCryptoQuest`
2. **Start using slash commands with your AI agent**:
   - **2.1** `/speckit.constitution` - Establish project principles
   - **2.2** `/speckit.specify` - Create baseline specification
   - **2.3** `/speckit.plan` - Create implementation plan
   - **2.4** `/speckit.tasks` - Generate actionable tasks
   - **2.5** `/speckit.implement` - Execute implementation

### Enhancement Commands (Optional)
- **○** `/speckit.clarify` (optional) - Ask structured questions to de-risk ambiguous areas before planning (run before `/speckit.plan` if used)
- **○** `/speckit.analyze` (optional) - Cross-artifact consistency & alignment report (after `/speckit.tasks`, before `/speckit.implement`)
- **○** `/speckit.checklist` (optional) - Generate quality checklists to validate requirements completeness, clarity, and consistency (after `/speckit.plan`)

## Implementation Notes

These prompts will systematically transform your comprehensive crypto learning platform documentation into a well-structured, implementable project using SpecKit's methodology. Each command builds upon the previous one to ensure a cohesive development approach.

**Key Benefits**:
- Structured approach to complex platform development
- Clear phase-based implementation strategy
- Built-in quality assurance checkpoints
- Risk mitigation through systematic planning
- Actionable tasks with defined acceptance criteria