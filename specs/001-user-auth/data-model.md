# Data Model: User Authentication & Authorization System

**Feature**: User Authentication & Authorization System  
**Date**: 2025-11-03  
**Branch**: `001-user-auth`

## Domain Entities

### User Entity

**Purpose**: Core entity representing platform users with authentication and authorization data.

**Properties**:
- `Id` (Guid) - Primary key, unique identifier
- `Email` (string, 256 chars) - User email address, unique, required for login
- `Username` (string, 50 chars) - Display name, unique, required
- `PasswordHash` (string, 255 chars) - BCrypt hashed password with 12 salt rounds
- `Role` (UserRole enum) - Access control level (Free, Premium, Admin)
- `IsEmailVerified` (bool) - Email verification status, required for login
- `FailedLoginAttempts` (int) - Failed login counter for progressive lockout
- `LockoutEnd` (DateTime?) - Account lockout expiration, null if not locked
- `LastLoginAt` (DateTime?) - Last successful login timestamp
- `CreatedAt` (DateTime) - Account creation timestamp
- `UpdatedAt` (DateTime) - Last modification timestamp
- `IsDeleted` (bool) - Soft delete flag for GDPR compliance

**Business Rules**:
- Email must be unique across all users (active and soft-deleted)
- Username must be unique across all active users
- Password must meet complexity requirements before hashing
- Email verification required before first login
- Progressive lockout: 5 attempts = 15 min, escalating for repeat offenses
- Soft delete maintains audit trail while enabling GDPR compliance

**Entity Methods**:
- `VerifyPassword(string password) : bool` - Verify password against hash
- `UpdatePassword(string newPassword)` - Hash and update password
- `VerifyEmail()` - Mark email as verified
- `IncrementFailedLoginAttempts()` - Track failed login for lockout
- `ResetFailedLoginAttempts()` - Clear failed login counter on success
- `ApplyLockout(TimeSpan duration)` - Lock account for specified duration
- `IsLockedOut() : bool` - Check if account is currently locked

**Factory Methods**:
```csharp
public static User CreateNew(string email, string username, string password, UserRole role = UserRole.Free)
public static User CreatePremiumUser(string email, string username, string password)
public static User CreateAdminUser(string email, string username, string password)
```

### RefreshToken Entity

**Purpose**: Manages secure session persistence with token rotation for enhanced security.

**Properties**:
- `Id` (Guid) - Primary key
- `UserId` (Guid) - Foreign key to User entity
- `Token` (string, 255 chars) - Cryptographically secure random token
- `ExpiresAt` (DateTime) - Token expiration timestamp (7 days from creation)
- `CreatedAt` (DateTime) - Token creation timestamp
- `RevokedAt` (DateTime?) - Token revocation timestamp
- `IsRevoked` (bool) - Computed property based on RevokedAt
- `IsExpired` (bool) - Computed property based on ExpiresAt
- `ReplacedByToken` (string?) - Reference to replacement token for rotation

**Business Rules**:
- One active refresh token per user session
- Automatic revocation when new token issued (rotation)
- 7-day expiration with no extension capability
- Immediate revocation on logout or security concern
- Token rotation on each access token refresh

**Entity Methods**:
- `IsActive() : bool` - Check if token is not expired or revoked
- `Revoke(string? replacedByToken = null)` - Mark token as revoked
- `IsValidForRefresh() : bool` - Validate token for access token refresh

**Factory Methods**:
```csharp
public static RefreshToken CreateForUser(Guid userId)
public static RefreshToken CreateReplacement(RefreshToken oldToken, Guid userId)
```

### EmailVerificationToken Entity

**Purpose**: Temporary security token for email address verification during registration.

**Properties**:
- `Id` (Guid) - Primary key
- `UserId` (Guid) - Foreign key to User entity
- `Token` (string, 255 chars) - Secure random verification token
- `ExpiresAt` (DateTime) - Token expiration (24 hours from creation)
- `CreatedAt` (DateTime) - Token creation timestamp
- `UsedAt` (DateTime?) - Token usage timestamp
- `IsUsed` (bool) - Computed property based on UsedAt
- `IsExpired` (bool) - Computed property based on ExpiresAt

**Business Rules**:
- Single-use token that becomes invalid after verification
- 24-hour expiration for security
- New token invalidates previous unused tokens for same user
- Token cleanup job removes expired tokens daily
- Email resend limited to 3 attempts per hour per user

**Entity Methods**:
- `IsValidForVerification() : bool` - Check token validity for use
- `MarkAsUsed()` - Mark token as consumed
- `IsExpiredOrUsed() : bool` - Combined validity check

**Factory Methods**:
```csharp
public static EmailVerificationToken CreateForUser(Guid userId)
```

### PasswordResetToken Entity

**Purpose**: Temporary security token for secure password reset functionality.

**Properties**:
- `Id` (Guid) - Primary key
- `UserId` (Guid) - Foreign key to User entity
- `Token` (string, 255 chars) - Secure random reset token
- `ExpiresAt` (DateTime) - Token expiration (1 hour from creation)
- `CreatedAt` (DateTime) - Token creation timestamp
- `UsedAt` (DateTime?) - Token usage timestamp
- `IsUsed` (bool) - Computed property based on UsedAt
- `IsExpired` (bool) - Computed property based on ExpiresAt

**Business Rules**:
- Single-use token with 1-hour expiration for security
- New token request invalidates previous unused tokens
- Token usage automatically updates user password
- Rate limiting: 3 reset requests per hour per email
- Token cleanup job removes expired tokens daily

**Entity Methods**:
- `IsValidForReset() : bool` - Check token validity for password reset
- `MarkAsUsed()` - Mark token as consumed
- `IsExpiredOrUsed() : bool` - Combined validity check

**Factory Methods**:
```csharp
public static PasswordResetToken CreateForUser(Guid userId)
```

### AuthAuditLog Entity

**Purpose**: Immutable security audit trail for authentication events and compliance.

**Properties**:
- `Id` (Guid) - Primary key
- `UserId` (Guid?) - User reference (nullable for failed attempts)
- `EventType` (AuthEventType enum) - Type of authentication event
- `IPAddress` (string, 45 chars) - Client IP address (IPv6 compatible)
- `UserAgent` (string, 500 chars) - Browser/client user agent
- `Success` (bool) - Event success status
- `ErrorMessage` (string?) - Error details for failed events
- `AdditionalData` (string?) - JSON serialized additional context
- `Timestamp` (DateTime) - Event timestamp (UTC)

**Business Rules**:
- Immutable records for audit integrity
- All authentication events must be logged
- Retention period: 2 years for compliance
- Time-based partitioning for performance
- No personal data in logs (GDPR compliance)

**Event Types** (AuthEventType enum):
- Login, LoginFailed, Logout
- Registration, EmailVerification
- PasswordReset, PasswordChanged
- TokenRefresh, TokenRevoked
- AccountLocked, AccountUnlocked

## Entity Relationships

### User Relationships
- `User` → `RefreshToken` (One-to-Many)
- `User` → `EmailVerificationToken` (One-to-Many)
- `User` → `PasswordResetToken` (One-to-Many)
- `User` → `AuthAuditLog` (One-to-Many)

### Relationship Rules
- Cascade delete: User deletion removes all related tokens and audit logs
- Referential integrity: All foreign keys must reference valid users
- Soft delete preservation: Deleted users maintain audit trail
- Token cleanup: Expired tokens removed by background service

## Database Schema

### Table Specifications

**Users Table**:
```sql
CREATE TABLE Users (
    Id UUID PRIMARY KEY,
    Email VARCHAR(256) NOT NULL UNIQUE,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role INTEGER NOT NULL DEFAULT 0,
    IsEmailVerified BOOLEAN NOT NULL DEFAULT FALSE,
    FailedLoginAttempts INTEGER NOT NULL DEFAULT 0,
    LockoutEnd TIMESTAMP NULL,
    LastLoginAt TIMESTAMP NULL,
    CreatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX IX_Users_Email ON Users(Email) WHERE IsDeleted = FALSE;
CREATE INDEX IX_Users_Username ON Users(Username) WHERE IsDeleted = FALSE;
CREATE INDEX IX_Users_Role ON Users(Role);
```

**RefreshTokens Table**:
```sql
CREATE TABLE RefreshTokens (
    Id UUID PRIMARY KEY,
    UserId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    Token VARCHAR(255) NOT NULL UNIQUE,
    ExpiresAt TIMESTAMP NOT NULL,
    CreatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    RevokedAt TIMESTAMP NULL,
    ReplacedByToken VARCHAR(255) NULL
);

CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
CREATE INDEX IX_RefreshTokens_ExpiresAt ON RefreshTokens(ExpiresAt);
```

**EmailVerificationTokens Table**:
```sql
CREATE TABLE EmailVerificationTokens (
    Id UUID PRIMARY KEY,
    UserId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    Token VARCHAR(255) NOT NULL UNIQUE,
    ExpiresAt TIMESTAMP NOT NULL,
    CreatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    UsedAt TIMESTAMP NULL
);

CREATE INDEX IX_EmailVerificationTokens_Token ON EmailVerificationTokens(Token);
CREATE INDEX IX_EmailVerificationTokens_UserId ON EmailVerificationTokens(UserId);
```

**PasswordResetTokens Table**:
```sql
CREATE TABLE PasswordResetTokens (
    Id UUID PRIMARY KEY,
    UserId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    Token VARCHAR(255) NOT NULL UNIQUE,
    ExpiresAt TIMESTAMP NOT NULL,
    CreatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    UsedAt TIMESTAMP NULL
);

CREATE INDEX IX_PasswordResetTokens_Token ON PasswordResetTokens(Token);
CREATE INDEX IX_PasswordResetTokens_UserId ON PasswordResetTokens(UserId);
```

**AuthAuditLogs Table** (Partitioned by Month):
```sql
CREATE TABLE AuthAuditLogs (
    Id UUID PRIMARY KEY,
    UserId UUID NULL REFERENCES Users(Id),
    EventType INTEGER NOT NULL,
    IPAddress VARCHAR(45) NOT NULL,
    UserAgent VARCHAR(500) NULL,
    Success BOOLEAN NOT NULL,
    ErrorMessage TEXT NULL,
    AdditionalData JSONB NULL,
    Timestamp TIMESTAMP NOT NULL DEFAULT NOW()
) PARTITION BY RANGE (Timestamp);

CREATE INDEX IX_AuthAuditLogs_UserId ON AuthAuditLogs(UserId);
CREATE INDEX IX_AuthAuditLogs_Timestamp ON AuthAuditLogs(Timestamp);
CREATE INDEX IX_AuthAuditLogs_EventType ON AuthAuditLogs(EventType);
```

### Performance Considerations

**Indexing Strategy**:
- Unique indexes on email/username for fast user lookup
- Composite indexes for common query patterns
- Partial indexes excluding soft-deleted records
- Time-based partitioning for audit logs

**Query Optimization**:
- Use async methods for all database operations
- Implement proper connection pooling
- Add database query logging for performance monitoring
- Use compiled queries for frequently executed operations

## Data Transfer Objects (DTOs)

### Request DTOs
- `RegisterRequest` - User registration data
- `LoginRequest` - Login credentials
- `RefreshTokenRequest` - Token refresh data
- `ForgotPasswordRequest` - Password reset initiation
- `ResetPasswordRequest` - Password reset completion
- `ChangePasswordRequest` - Authenticated password change

### Response DTOs
- `AuthResponse` - Authentication result with tokens
- `UserProfileResponse` - User profile information
- `RefreshTokenResponse` - Token refresh result

### Validation Rules
- Email: Valid format, max 256 characters
- Username: 3-50 characters, alphanumeric + underscore
- Password: 8+ characters, uppercase, lowercase, number, special character
- All required fields validated with meaningful error messages

## Migration Strategy

### Initial Migration
1. Create Users table with basic authentication fields
2. Create RefreshTokens table with proper relationships
3. Create verification and reset token tables
4. Create audit log table with partitioning
5. Add required indexes and constraints

### Future Migrations
- Add two-factor authentication fields
- Extend user profile information
- Add social login provider integration
- Implement role hierarchy for complex permissions

## Security Considerations

### Data Protection
- All passwords hashed with BCrypt (12 rounds)
- Sensitive tokens stored securely with expiration
- No plaintext passwords in database or logs
- GDPR-compliant soft delete with data anonymization

### Access Control
- Role-based authorization at entity level
- Row-level security for user data access
- Audit trail for all security-relevant operations
- Rate limiting implementation at database level

### Backup and Recovery
- Regular encrypted database backups
- Point-in-time recovery capability
- Audit log archival strategy
- Disaster recovery procedures documented