# Feature: User Authentication & Authorization System

## /speckit.specify

### Feature Overview
Implement a complete JWT-based authentication and authorization system with role-based access control, email verification, password reset, and refresh token functionality.

### Feature Scope
- User registration with email verification
- User login with JWT token generation
- Refresh token mechanism
- Password reset flow with time-limited tokens
- Role-based authorization (Admin, Premium, Free)
- Protected routes and endpoints
- Session management

### User Stories
1. As a new user, I want to register with email and password so I can access the platform
2. As a registered user, I want to verify my email to activate my account
3. As a user, I want to login with my credentials and receive a secure token
4. As a user, I want to reset my password if I forget it
5. As a user, I want my session to remain active without frequent re-logins (refresh tokens)
6. As an admin, I want role-based access to admin-only features
7. As a premium user, I want access to premium content

### Technical Requirements
- JWT tokens with 60-minute expiration
- Refresh tokens with 7-day expiration
- BCrypt password hashing
- Email verification with token-based confirmation
- Rate limiting on auth endpoints (5 attempts per minute)
- Secure token storage
- CORS configuration for frontend

---

## /speckit.plan

### Implementation Plan

#### Phase 1: Backend Authentication Infrastructure
**Tasks:**
1. Create User entity with ASP.NET Identity integration
2. Set up JWT configuration in appsettings.json
3. Implement JwtSettings class and token generation
4. Create AuthService with registration/login logic
5. Implement password hashing and verification
6. Create refresh token storage and management

**Deliverables:**
- User entity with PasswordHash, Email, Role fields
- JWT token generation service
- AuthService interface and implementation
- Refresh token mechanism

#### Phase 2: Authentication Endpoints
**Tasks:**
1. Create AuthController with endpoints:
   - POST /api/auth/register
   - POST /api/auth/login
   - POST /api/auth/refresh-token
   - POST /api/auth/forgot-password
   - POST /api/auth/reset-password
   - GET /api/auth/verify-email/{token}
2. Implement request/response DTOs
3. Add validation with FluentValidation
4. Configure JWT bearer authentication in Program.cs

**Deliverables:**
- Complete AuthController
- RegisterDto, LoginDto, ResetPasswordDto
- Validation rules
- JWT middleware configuration

#### Phase 3: Authorization Policies
**Tasks:**
1. Configure authorization policies (RequireAdmin, RequirePremium)
2. Create custom authorization handlers
3. Implement role-based [Authorize] attributes
4. Add subscription status checking middleware

**Deliverables:**
- Authorization policies in Program.cs
- Custom authorization handlers
- Middleware for subscription validation

#### Phase 4: Frontend Authentication
**Tasks:**
1. Create AuthStore (Zustand) for state management
2. Implement authService for API calls
3. Create LoginForm and RegisterForm components
4. Build ProtectedRoute component
5. Implement axios interceptors for token management
6. Create email verification page
7. Create password reset flow pages

**Deliverables:**
- AuthStore with login/logout/register methods
- Login and Register pages
- ProtectedRoute wrapper
- Axios interceptors for auth headers

#### Phase 5: Token Management
**Tasks:**
1. Implement token storage strategy (localStorage)
2. Create auto-refresh mechanism (before expiry)
3. Handle 401 errors with token refresh attempt
4. Implement logout (clear tokens, redirect)
5. Create "Remember Me" functionality

**Deliverables:**
- Token storage utilities
- Auto-refresh logic
- Error handling for expired tokens
- Logout functionality

---

## /speckit.clarify

### Questions & Answers

**Q: Should we use cookies or localStorage for token storage?**
A: Use localStorage for MVP. Consider httpOnly cookies for production for better XSS protection. Document both approaches.

**Q: What password strength requirements?**
A: Minimum 8 characters, at least one uppercase, one lowercase, one number, one special character.

**Q: Should we implement 2FA?**
A: Not in MVP. Add to future enhancements roadmap.

**Q: How should we handle concurrent sessions?**
A: Allow multiple sessions in MVP. Add session management in future versions.

**Q: Email service for verification?**
A: Use SendGrid or Brevo free tier (100-300 emails/day). Abstract email service for easy switching.

**Q: What happens to user data on password reset?**
A: Only password changes. All other data remains intact.

**Q: Should refresh tokens be revocable?**
A: Yes. Store refresh tokens in database with expiry and revocation flag for security.

---

## /speckit.analyze

### Technical Analysis

#### Backend Architecture
```
WahadiniCryptoQuest.Domain/
└── Entities/
    ├── User.cs (Id, Email, Username, PasswordHash, Role, SubscriptionTier, etc.)
    └── RefreshToken.cs (Token, UserId, ExpiryDate, IsRevoked)

WahadiniCryptoQuest.Application/
├── Interfaces/
│   ├── IAuthService.cs
│   └── ITokenService.cs
├── Services/
│   ├── AuthService.cs
│   └── TokenService.cs
└── DTOs/
    ├── RegisterDto.cs
    ├── LoginDto.cs
    ├── LoginResponseDto.cs
    ├── RefreshTokenDto.cs
    └── ResetPasswordDto.cs

WahadiniCryptoQuest.API/
└── Controllers/
    └── AuthController.cs
```

#### Frontend Architecture
```
src/
├── stores/
│   └── authStore.ts (Zustand store)
├── services/
│   ├── api.ts (Axios instance with interceptors)
│   └── authService.ts (Auth API calls)
├── components/
│   └── auth/
│       ├── LoginForm.tsx
│       ├── RegisterForm.tsx
│       ├── ProtectedRoute.tsx
│       └── PremiumGate.tsx
├── pages/
│   ├── LoginPage.tsx
│   ├── RegisterPage.tsx
│   ├── VerifyEmailPage.tsx
│   ├── ForgotPasswordPage.tsx
│   └── ResetPasswordPage.tsx
└── hooks/
    └── useAuth.ts
```

#### Database Schema
```sql
CREATE TABLE Users (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Email VARCHAR(255) UNIQUE NOT NULL,
    Username VARCHAR(100) UNIQUE NOT NULL,
    PasswordHash TEXT NOT NULL,
    Role VARCHAR(50) NOT NULL DEFAULT 'Free',
    SubscriptionTier VARCHAR(50) NOT NULL DEFAULT 'Free',
    SubscriptionExpiryDate TIMESTAMP NULL,
    EmailVerified BOOLEAN DEFAULT FALSE,
    EmailVerificationToken TEXT NULL,
    PasswordResetToken TEXT NULL,
    PasswordResetTokenExpiry TIMESTAMP NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    IsActive BOOLEAN DEFAULT TRUE
);

CREATE TABLE RefreshTokens (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Token TEXT UNIQUE NOT NULL,
    UserId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    ExpiryDate TIMESTAMP NOT NULL,
    IsRevoked BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_users_email ON Users(Email);
CREATE INDEX idx_refresh_tokens_token ON RefreshTokens(Token);
CREATE INDEX idx_refresh_tokens_userid ON RefreshTokens(UserId);
```

#### API Endpoints
```
POST   /api/auth/register
       Body: { email, username, password }
       Response: { message, userId }

POST   /api/auth/login
       Body: { email, password }
       Response: { token, refreshToken, user: { id, email, username, role, isPremium } }

POST   /api/auth/refresh-token
       Body: { refreshToken }
       Response: { token, refreshToken }

POST   /api/auth/forgot-password
       Body: { email }
       Response: { message }

POST   /api/auth/reset-password
       Body: { token, newPassword }
       Response: { message }

GET    /api/auth/verify-email?token={token}
       Response: { message }

GET    /api/auth/me
       Headers: Authorization: Bearer {token}
       Response: { id, email, username, role, subscriptionTier, rewardPoints }
```

#### Security Measures
1. Password hashing with BCrypt (cost factor 12)
2. JWT with HMAC-SHA256 signing
3. Token expiration validation
4. Refresh token rotation on use
5. Email verification before full access
6. Rate limiting: 5 login attempts per minute per IP
7. Secure random token generation for email/password reset
8. Time-limited reset tokens (1 hour expiry)
9. Protection against timing attacks in password verification

#### State Flow
1. **Registration:** User submits form → Backend validates → Creates user with unverified email → Sends verification email → Returns success
2. **Email Verification:** User clicks link → Frontend extracts token → Calls verify endpoint → Backend marks email verified → Redirects to login
3. **Login:** User submits credentials → Backend validates → Generates JWT + refresh token → Returns tokens → Frontend stores tokens → Redirects to dashboard
4. **Token Refresh:** Token near expiry → Frontend calls refresh endpoint → Backend validates refresh token → Issues new tokens → Frontend updates storage
5. **Logout:** User clicks logout → Frontend clears tokens → Backend revokes refresh token → Redirects to home

---

## /speckit.checklist

### Implementation Checklist

#### Backend Setup
- [ ] Install required NuGet packages (Identity, JWT, FluentValidation)
- [ ] Create User entity with all required fields
- [ ] Create RefreshToken entity
- [ ] Add User and RefreshToken DbSets to ApplicationDbContext
- [ ] Configure ASP.NET Identity in Program.cs
- [ ] Create and apply EF migrations

#### JWT Configuration
- [ ] Create JwtSettings class
- [ ] Add JWT configuration to appsettings.json
- [ ] Implement TokenService for generation and validation
- [ ] Configure JWT bearer authentication middleware
- [ ] Add authorization policies

#### Auth Service
- [ ] Create IAuthService interface
- [ ] Implement RegisterAsync method
- [ ] Implement LoginAsync method
- [ ] Implement RefreshTokenAsync method
- [ ] Implement SendPasswordResetAsync method
- [ ] Implement ResetPasswordAsync method
- [ ] Implement VerifyEmailAsync method
- [ ] Create password hashing utilities
- [ ] Create token generation utilities

#### Auth Controller
- [ ] Create AuthController
- [ ] Implement Register endpoint with validation
- [ ] Implement Login endpoint with validation
- [ ] Implement RefreshToken endpoint
- [ ] Implement ForgotPassword endpoint
- [ ] Implement ResetPassword endpoint
- [ ] Implement VerifyEmail endpoint
- [ ] Add rate limiting attributes
- [ ] Add proper error handling

#### DTOs and Validation
- [ ] Create RegisterDto with validation rules
- [ ] Create LoginDto with validation rules
- [ ] Create LoginResponseDto
- [ ] Create RefreshTokenDto
- [ ] Create ResetPasswordDto with validation rules
- [ ] Create UserDto for responses

#### Frontend Auth Store
- [ ] Create AuthStore with Zustand
- [ ] Add user state property
- [ ] Add token state property
- [ ] Add isAuthenticated computed property
- [ ] Implement login action
- [ ] Implement register action
- [ ] Implement logout action
- [ ] Implement refreshToken action
- [ ] Implement checkAuth initialization

#### Frontend Auth Service
- [ ] Create axios instance with base URL
- [ ] Implement login API call
- [ ] Implement register API call
- [ ] Implement refreshToken API call
- [ ] Implement forgotPassword API call
- [ ] Implement resetPassword API call
- [ ] Implement verifyEmail API call
- [ ] Implement getCurrentUser API call

#### Axios Interceptors
- [ ] Create request interceptor to add Authorization header
- [ ] Create response interceptor for 401 handling
- [ ] Implement token refresh on 401
- [ ] Implement redirect to login on refresh failure
- [ ] Handle network errors gracefully

#### Auth Components
- [ ] Create LoginForm with validation
- [ ] Create RegisterForm with validation
- [ ] Create ProtectedRoute component
- [ ] Create PremiumGate component
- [ ] Create EmailVerificationSuccess component
- [ ] Create ForgotPasswordForm
- [ ] Create ResetPasswordForm

#### Auth Pages
- [ ] Create LoginPage
- [ ] Create RegisterPage
- [ ] Create VerifyEmailPage
- [ ] Create ForgotPasswordPage
- [ ] Create ResetPasswordPage
- [ ] Add proper error displays
- [ ] Add loading states

#### Testing
- [ ] Unit tests for AuthService methods
- [ ] Unit tests for TokenService
- [ ] Integration tests for auth endpoints
- [ ] Component tests for LoginForm
- [ ] Component tests for RegisterForm
- [ ] E2E test for registration flow
- [ ] E2E test for login flow
- [ ] E2E test for password reset flow

#### Security
- [ ] Verify password hashing implementation
- [ ] Test JWT token validation
- [ ] Verify refresh token rotation
- [ ] Test rate limiting
- [ ] Check for SQL injection vulnerabilities
- [ ] Check for XSS vulnerabilities
- [ ] Verify CORS configuration
- [ ] Test email token expiration
- [ ] Test password reset token expiration

#### Documentation
- [ ] Document API endpoints in Swagger
- [ ] Create authentication flow diagrams
- [ ] Write user guide for registration/login
- [ ] Document token management strategy
- [ ] Document security measures

---

## /speckit.tasks

### Task Breakdown (Estimated 40-50 hours)

#### Task 1: Database & Entities (4 hours)
**Description:** Create database schema and entity classes for authentication
**Subtasks:**
1. Create User entity class with all properties and data annotations
2. Create RefreshToken entity class
3. Add DbSets to ApplicationDbContext
4. Configure Fluent API relationships
5. Create and apply migration
6. Seed initial admin user

#### Task 2: JWT Infrastructure (3 hours)
**Description:** Set up JWT token generation and validation
**Subtasks:**
1. Create JwtSettings configuration class
2. Add JWT settings to appsettings.json
3. Create ITokenService interface
4. Implement TokenService (GenerateJwtToken, GenerateRefreshToken, ValidateToken)
5. Configure JWT authentication middleware
6. Test token generation and validation

#### Task 3: Auth Service Implementation (6 hours)
**Description:** Build core authentication business logic
**Subtasks:**
1. Create IAuthService interface
2. Implement RegisterAsync with validation
3. Implement LoginAsync with password verification
4. Implement RefreshTokenAsync with rotation
5. Implement SendPasswordResetAsync with email
6. Implement ResetPasswordAsync with token validation
7. Implement VerifyEmailAsync
8. Add error handling and logging

#### Task 4: Auth Controller & Endpoints (4 hours)
**Description:** Create API endpoints for authentication
**Subtasks:**
1. Create AuthController with dependency injection
2. Implement Register endpoint
3. Implement Login endpoint
4. Implement RefreshToken endpoint
5. Implement ForgotPassword endpoint
6. Implement ResetPassword endpoint
7. Implement VerifyEmail endpoint
8. Add Swagger documentation

#### Task 5: DTOs & Validation (3 hours)
**Description:** Create data transfer objects and validation rules
**Subtasks:**
1. Create RegisterDto with FluentValidation rules
2. Create LoginDto with validation
3. Create LoginResponseDto
4. Create RefreshTokenDto
5. Create ResetPasswordDto with validation
6. Create UserDto for API responses
7. Test validation rules

#### Task 6: Authorization Policies (2 hours)
**Description:** Configure role-based authorization
**Subtasks:**
1. Add authorization policies in Program.cs
2. Create custom authorization handlers if needed
3. Apply [Authorize] attributes to controllers
4. Test role-based access control

#### Task 7: Frontend Auth Store (3 hours)
**Description:** Create Zustand store for authentication state
**Subtasks:**
1. Create AuthStore with initial state
2. Implement login action
3. Implement register action
4. Implement logout action
5. Implement refreshToken action
6. Add persistence to localStorage
7. Create useAuth custom hook

#### Task 8: Frontend Auth Service (3 hours)
**Description:** Build API integration layer
**Subtasks:**
1. Create axios instance with interceptors
2. Implement login API call
3. Implement register API call
4. Implement refreshToken API call
5. Implement forgotPassword API call
6. Implement resetPassword API call
7. Implement verifyEmail API call

#### Task 9: Auth Components (6 hours)
**Description:** Build authentication UI components
**Subtasks:**
1. Create LoginForm with React Hook Form
2. Create RegisterForm with validation
3. Style forms with TailwindCSS
4. Create ProtectedRoute wrapper
5. Create PremiumGate component
6. Add error and success messages
7. Add loading states

#### Task 10: Auth Pages (4 hours)
**Description:** Create authentication pages
**Subtasks:**
1. Create LoginPage layout
2. Create RegisterPage layout
3. Create VerifyEmailPage
4. Create ForgotPasswordPage
5. Create ResetPasswordPage
6. Add navigation and routing
7. Test user flows

#### Task 11: Token Management (3 hours)
**Description:** Implement token refresh and expiry handling
**Subtasks:**
1. Implement auto-refresh before token expiry
2. Handle 401 errors with token refresh attempt
3. Implement logout with token cleanup
4. Test token expiration scenarios
5. Add token validation on app init

#### Task 12: Testing (6 hours)
**Description:** Write comprehensive tests
**Subtasks:**
1. Unit tests for AuthService (80% coverage)
2. Unit tests for TokenService
3. Integration tests for auth endpoints
4. Component tests for LoginForm
5. Component tests for RegisterForm
6. E2E test for complete auth flow

#### Task 13: Security Hardening (3 hours)
**Description:** Implement security measures
**Subtasks:**
1. Add rate limiting middleware
2. Implement refresh token rotation
3. Add email verification checks
4. Test password reset token expiration
5. Security audit and vulnerability scan

---

## /speckit.implement

### Implementation Guide

#### Step 1: Create User Entity

**File:** `WahadiniCryptoQuest.Domain/Entities/User.cs`

```csharp
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WahadiniCryptoQuest.Domain.Entities;

public class User : IdentityUser<Guid>
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Role { get; set; } = "Free"; // Free, Premium, Admin
    
    [Required]
    public string SubscriptionTier { get; set; } = "Free"; // Free, Monthly, Yearly
    
    public DateTime? SubscriptionExpiryDate { get; set; }
    
    public int RewardPoints { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    public string? EmailVerificationToken { get; set; }
    
    public string? PasswordResetToken { get; set; }
    
    public DateTime? PasswordResetTokenExpiry { get; set; }
    
    // Navigation properties
    public virtual ICollection<UserCourseEnrollment> Enrollments { get; set; } = new List<UserCourseEnrollment>();
    public virtual ICollection<UserProgress> Progress { get; set; } = new List<UserProgress>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public class RefreshToken
{
    public Guid Id { get; set; }
    
    [Required]
    public string Token { get; set; } = string.Empty;
    
    public Guid UserId { get; set; }
    
    public DateTime ExpiryDate { get; set; }
    
    public bool IsRevoked { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual User User { get; set; } = null!;
}
```

#### Step 2: Configure ApplicationDbContext

**File:** `WahadiniCryptoQuest.Infrastructure/Data/ApplicationDbContext.cs`

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Domain.Entities;

namespace WahadiniCryptoQuest.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    // ... other DbSets
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
        
        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

#### Step 3: Implement TokenService

**File:** `WahadiniCryptoQuest.Application/Services/TokenService.cs`

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WahadiniCryptoQuest.Application.Interfaces;
using WahadiniCryptoQuest.Domain.Entities;

namespace WahadiniCryptoQuest.Application.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    
    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("SubscriptionTier", user.SubscriptionTier),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    
    public ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // Don't validate lifetime for refresh
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
        };
        
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
```

#### Step 4: Implement AuthService

**File:** `WahadiniCryptoQuest.Application/Services/AuthService.cs`

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Application.DTOs;
using WahadiniCryptoQuest.Application.Interfaces;
using WahadiniCryptoQuest.Domain.Entities;
using WahadiniCryptoQuest.Infrastructure.Data;

namespace WahadiniCryptoQuest.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    
    public AuthService(
        UserManager<User> userManager,
        ITokenService tokenService,
        ApplicationDbContext context,
        INotificationService notificationService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
        _notificationService = notificationService;
    }
    
    public async Task<(bool Success, string? UserId, string? Error)> RegisterAsync(RegisterDto dto)
    {
        // Check if user exists
        if (await _userManager.FindByEmailAsync(dto.Email) != null)
        {
            return (false, null, "User with this email already exists");
        }
        
        if (await _userManager.Users.AnyAsync(u => u.Username == dto.Username))
        {
            return (false, null, "Username already taken");
        }
        
        // Create user
        var user = new User
        {
            Email = dto.Email,
            Username = dto.Username,
            UserName = dto.Email, // Required by Identity
            EmailVerificationToken = Guid.NewGuid().ToString(),
            Role = "Free",
            SubscriptionTier = "Free"
        };
        
        var result = await _userManager.CreateAsync(user, dto.Password);
        
        if (!result.Succeeded)
        {
            return (false, null, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        
        // Send verification email
        await _notificationService.SendEmailVerificationAsync(user.Email!, user.EmailVerificationToken);
        
        return (true, user.Id.ToString(), null);
    }
    
    public async Task<(bool Success, LoginResponseDto? Response, string? Error)> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        
        if (user == null)
        {
            return (false, null, "Invalid credentials");
        }
        
        if (!user.IsActive)
        {
            return (false, null, "Account is deactivated");
        }
        
        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            return (false, null, "Invalid credentials");
        }
        
        // Generate tokens
        var jwtToken = _tokenService.GenerateJwtToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        
        // Store refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        };
        
        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();
        
        var response = new LoginResponseDto
        {
            Token = jwtToken,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                Username = user.Username,
                Role = user.Role,
                SubscriptionTier = user.SubscriptionTier,
                RewardPoints = user.RewardPoints,
                EmailVerified = user.EmailConfirmed
            }
        };
        
        return (true, response, null);
    }
    
    public async Task<(bool Success, string? Token, string? RefreshToken, string? Error)> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);
        
        if (storedToken == null)
        {
            return (false, null, null, "Invalid refresh token");
        }
        
        if (storedToken.ExpiryDate < DateTime.UtcNow)
        {
            return (false, null, null, "Refresh token expired");
        }
        
        // Revoke old refresh token (rotation)
        storedToken.IsRevoked = true;
        
        // Generate new tokens
        var newJwtToken = _tokenService.GenerateJwtToken(storedToken.User);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        
        var newRefreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = storedToken.UserId,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        };
        
        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync();
        
        return (true, newJwtToken, newRefreshToken, null);
    }
    
    public async Task<bool> VerifyEmailAsync(string token)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
        
        if (user == null)
        {
            return false;
        }
        
        user.EmailConfirmed = true;
        user.EmailVerificationToken = null;
        await _userManager.UpdateAsync(user);
        
        return true;
    }
    
    public async Task<bool> SendPasswordResetAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        
        if (user == null)
        {
            // Don't reveal if user exists
            return true;
        }
        
        var resetToken = Guid.NewGuid().ToString();
        user.PasswordResetToken = resetToken;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        
        await _userManager.UpdateAsync(user);
        await _notificationService.SendPasswordResetAsync(user.Email!, resetToken);
        
        return true;
    }
    
    public async Task<(bool Success, string? Error)> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(
            u => u.PasswordResetToken == dto.Token &&
                 u.PasswordResetTokenExpiry > DateTime.UtcNow);
        
        if (user == null)
        {
            return (false, "Invalid or expired reset token");
        }
        
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
        
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await _userManager.UpdateAsync(user);
        
        return (true, null);
    }
}
```

#### Step 5: Create AuthController

**File:** `WahadiniCryptoQuest.API/Controllers/AuthController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WahadiniCryptoQuest.Application.DTOs;
using WahadiniCryptoQuest.Application.Interfaces;

namespace WahadiniCryptoQuest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var (success, userId, error) = await _authService.RegisterAsync(dto);
        
        if (!success)
        {
            return BadRequest(new { message = error });
        }
        
        return Ok(new { message = "Registration successful. Please check your email to verify your account.", userId });
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var (success, response, error) = await _authService.LoginAsync(dto);
        
        if (!success)
        {
            return Unauthorized(new { message = error });
        }
        
        return Ok(response);
    }
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        var (success, token, refreshToken, error) = await _authService.RefreshTokenAsync(dto.RefreshToken);
        
        if (!success)
        {
            return Unauthorized(new { message = error });
        }
        
        return Ok(new { token, refreshToken });
    }
    
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        var success = await _authService.VerifyEmailAsync(token);
        
        if (!success)
        {
            return BadRequest(new { message = "Invalid verification token" });
        }
        
        return Ok(new { message = "Email verified successfully" });
    }
    
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        await _authService.SendPasswordResetAsync(dto.Email);
        return Ok(new { message = "If the email exists, a password reset link has been sent" });
    }
    
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var (success, error) = await _authService.ResetPasswordAsync(dto);
        
        if (!success)
        {
            return BadRequest(new { message = error });
        }
        
        return Ok(new { message = "Password reset successful" });
    }
    
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        // Implementation to get current user from claims
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        // Fetch and return user data
        return Ok();
    }
}
```

#### Step 6: Frontend AuthStore

**File:** `frontend/src/stores/authStore.ts`

```typescript
import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface User {
  id: string;
  email: string;
  username: string;
  role: string;
  subscriptionTier: string;
  rewardPoints: number;
  emailVerified: boolean;
}

interface AuthState {
  user: User | null;
  token: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isPremium: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, username: string, password: string) => Promise<void>;
  logout: () => void;
  refreshTokenFn: () => Promise<void>;
  setUser: (user: User | null) => void;
  setTokens: (token: string, refreshToken: string) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      token: null,
      refreshToken: null,
      isAuthenticated: false,
      isPremium: false,
      
      login: async (email: string, password: string) => {
        const response = await authService.login({ email, password });
        set({
          user: response.user,
          token: response.token,
          refreshToken: response.refreshToken,
          isAuthenticated: true,
          isPremium: ['Monthly', 'Yearly'].includes(response.user.subscriptionTier),
        });
      },
      
      register: async (email: string, username: string, password: string) => {
        await authService.register({ email, username, password });
      },
      
      logout: () => {
        set({
          user: null,
          token: null,
          refreshToken: null,
          isAuthenticated: false,
          isPremium: false,
        });
      },
      
      refreshTokenFn: async () => {
        const { refreshToken } = get();
        if (!refreshToken) throw new Error('No refresh token');
        
        const response = await authService.refreshToken(refreshToken);
        set({
          token: response.token,
          refreshToken: response.refreshToken,
        });
      },
      
      setUser: (user) => set({ user }),
      setTokens: (token, refreshToken) => set({ token, refreshToken }),
    }),
    {
      name: 'auth-storage',
    }
  )
);
```

### Notes
- Ensure all error messages are user-friendly
- Log all authentication attempts for security monitoring
- Test thoroughly with various edge cases
- Follow OWASP authentication guidelines
- Keep tokens secure and never expose in logs
