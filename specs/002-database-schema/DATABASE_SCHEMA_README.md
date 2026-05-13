# Database Schema Implementation - WahadiniCryptoQuest

## Overview
This document provides a comprehensive overview of the PostgreSQL database schema implementation for the WahadiniCryptoQuest crypto learning platform.

## Technology Stack
- **PostgreSQL 15+** with JSONB support
- **Entity Framework Core 8.0** - Code-first approach
- **ASP.NET Identity** - User authentication
- **Time-based Partitioning** - For user activity data

## Entity Relationship Diagram

```
┌──────────────────┐
│      User        │
├──────────────────┤
│ Id (PK)          │──┐
│ Email            │  │
│ FirstName        │  │
│ LastName         │  │
│ PasswordHash     │  │
│ EmailConfirmed   │  │
│ Role             │  │
│ IsActive         │  │
│ CreatedAt        │  │
└──────────────────┘  │
         │            │
         │            │
┌────────▼───────────┐│
│   RefreshToken     ││
├────────────────────┤│
│ Id (PK)            ││
│ Token              ││
│ UserId (FK)        ││◄┘
│ ExpiresAt          │
│ IsRevoked          │
└────────────────────┘

┌──────────────────┐
│   Category       │
├──────────────────┤
│ Id (PK)          │──┐
│ Name             │  │
│ Description      │  │
│ IconUrl          │  │
│ DisplayOrder     │  │
│ IsActive         │  │
└──────────────────┘  │
                      │
┌─────────────────────▼┐
│      Course          │
├──────────────────────┤
│ Id (PK)              │──┐
│ CategoryId (FK)      │◄─┘
│ Title                │
│ Description          │
│ ThumbnailUrl         │
│ DifficultyLevel      │
│ EstimatedDuration    │
│ IsPremium            │
│ RewardPoints         │
│ IsPublished          │
│ ViewCount            │
│ CreatedByUserId (FK) │
│ CreatedAt            │
│ UpdatedAt            │
│ IsDeleted            │
└──────────────────────┘
         │
         │
┌────────▼───────────┐
│      Lesson        │
├────────────────────┤
│ Id (PK)            │──┐
│ CourseId (FK)      │◄─┘
│ Title              │
│ Description        │
│ YoutubeVideoId     │
│ Duration           │
│ OrderIndex         │
│ IsPremium          │
│ RewardPoints       │
│ ContentMarkdown    │
│ CreatedAt          │
│ UpdatedAt          │
│ IsDeleted          │
└────────────────────┘
         │
         │
┌────────▼───────────┐
│   LearningTask     │
├────────────────────┤
│ Id (PK)            │
│ LessonId (FK)      │◄─┘
│ Title              │
│ Description        │
│ TaskType           │
│ TaskData (JSONB)   │
│ RewardPoints       │
│ TimeLimit          │
│ OrderIndex         │
│ IsRequired         │
│ CreatedAt          │
└────────────────────┘

┌──────────────────────┐      ┌──────────────────────┐
│  UserProgress        │      │ UserCourseEnrollment │
├──────────────────────┤      ├──────────────────────┤
│ Id (PK)              │      │ Id (PK)              │
│ UserId (FK)          │      │ UserId (FK)          │
│ LessonId (FK)        │      │ CourseId (FK)        │
│ LastWatchedPosition  │      │ EnrolledAt           │
│ CompletionPercent    │      │ LastAccessedAt       │
│ IsCompleted          │      │ CompletionPercent    │
│ CompletedAt          │      │ IsCompleted          │
│ RewardPointsClaimed  │      │ CompletedAt          │
│ LastUpdatedAt        │      └──────────────────────┘
└──────────────────────┘

┌────────────────────────┐
│ UserTaskSubmission     │
├────────────────────────┤
│ Id (PK)                │
│ UserId (FK)            │
│ TaskId (FK)            │
│ SubmissionData (JSONB) │
│ Status                 │
│ SubmittedAt            │
│ ReviewedAt             │
│ ReviewedByUserId (FK)  │
│ FeedbackText           │
│ RewardPointsAwarded    │
└────────────────────────┘

┌────────────────────────┐
│  RewardTransaction     │
├────────────────────────┤
│ Id (PK)                │
│ UserId (FK)            │
│ Amount                 │
│ TransactionType        │
│ ReferenceId            │
│ ReferenceType          │
│ Description            │
│ CreatedAt              │
└────────────────────────┘

┌──────────────────────┐
│   DiscountCode       │
├──────────────────────┤
│ Id (PK)              │
│ Code                 │
│ DiscountPercentage   │
│ RequiredPoints       │
│ MaxRedemptions       │
│ CurrentRedemptions   │
│ ExpiryDate           │
│ IsActive             │
│ CreatedAt            │
└──────────────────────┘
         │
         │
┌────────▼─────────────────┐
│ UserDiscountRedemption   │
├──────────────────────────┤
│ Id (PK)                  │
│ UserId (FK)              │
│ DiscountCodeId (FK)      │◄─┘
│ RedeemedAt               │
│ UsedInSubscription       │
└──────────────────────────┘
```

## Core Entities

### 1. User
Rich domain model with authentication data and business logic.

**Properties:**
- `Id` (Guid): Primary key
- `Email` (string, 320): Unique user email
- `FirstName` (string, 100): User's first name
- `LastName` (string, 100): User's last name
- `PasswordHash` (string, 255): Hashed password
- `EmailConfirmed` (bool): Email verification status
- `Role` (UserRoleEnum): User role (Free, Premium, Admin, etc.)
- `IsActive` (bool): Account active status
- `CreatedAt` (DateTime): Account creation timestamp

**Domain Methods:**
- `Create()`: Factory method for creating new users
- `ConfirmEmail()`: Mark email as confirmed
- `RecordLogin()`: Track successful login
- `IncrementFailedLoginAttempts()`: Track failed logins
- `LockAccount()`: Lock user account
- `UpdatePassword()`: Change user password

### 2. Category
Course categorization system.

**Properties:**
- `Id` (Guid): Primary key
- `Name` (string, 100): Category name
- `Description` (string, 500): Category description
- `IconUrl` (string, 500): Icon URL
- `DisplayOrder` (int): Display order
- `IsActive` (bool): Active status

**Default Categories:**
1. Airdrops
2. GameFi
3. Task-to-Earn
4. DeFi
5. NFT Strategies

### 3. Course
Educational course entity with metadata.

**Properties:**
- `Id` (Guid): Primary key
- `CategoryId` (Guid): Foreign key to Category
- `Title` (string, 200): Course title
- `Description` (string, 2000): Course description
- `ThumbnailUrl` (string, 500): Thumbnail image URL
- `DifficultyLevel` (enum): Beginner, Intermediate, Advanced, Expert
- `EstimatedDuration` (int): Duration in minutes
- `IsPremium` (bool): Premium course flag
- `RewardPoints` (int): Points earned on completion
- `IsPublished` (bool): Publication status
- `ViewCount` (int): Number of views
- `IsDeleted` (bool): Soft delete flag

**Domain Methods:**
- `Publish()`: Publish the course
- `IncrementViewCount()`: Track course views

### 4. Lesson
Individual lesson with YouTube video integration.

**Properties:**
- `Id` (Guid): Primary key
- `CourseId` (Guid): Foreign key to Course
- `Title` (string, 200): Lesson title
- `Description` (string, 2000): Lesson description
- `YoutubeVideoId` (string, 50): YouTube video ID
- `Duration` (int): Video duration in minutes
- `OrderIndex` (int): Lesson order in course
- `IsPremium` (bool): Premium lesson flag
- `RewardPoints` (int): Points for completion
- `ContentMarkdown` (text): Additional lesson content

### 5. LearningTask
Interactive tasks for verification with flexible JSONB data.

**Properties:**
- `Id` (Guid): Primary key
- `LessonId` (Guid): Foreign key to Lesson
- `Title` (string, 200): Task title
- `Description` (string, 2000): Task description
- `TaskType` (enum): Quiz, ExternalLink, Screenshot, TextSubmission, WalletVerification
- `TaskData` (jsonb): Flexible task configuration
- `RewardPoints` (int): Points for completion
- `TimeLimit` (int?): Time limit in minutes (optional)
- `OrderIndex` (int): Task order in lesson
- `IsRequired` (bool): Required task flag

### 6. UserProgress
Tracks user progress through lessons.

**Properties:**
- `Id` (Guid): Primary key
- `UserId` (Guid): Foreign key to User
- `LessonId` (Guid): Foreign key to Lesson
- `LastWatchedPosition` (int): Video position in seconds
- `CompletionPercentage` (decimal): Progress percentage
- `IsCompleted` (bool): Completion flag
- `CompletedAt` (DateTime?): Completion timestamp
- `RewardPointsClaimed` (bool): Rewards claimed flag

**Domain Methods:**
- `UpdateProgress()`: Update lesson progress
- `ClaimRewards()`: Claim reward points

### 7. UserCourseEnrollment
Tracks user enrollment in courses.

**Properties:**
- `Id` (Guid): Primary key
- `UserId` (Guid): Foreign key to User
- `CourseId` (Guid): Foreign key to Course
- `EnrolledAt` (DateTime): Enrollment timestamp
- `LastAccessedAt` (DateTime): Last access timestamp
- `CompletionPercentage` (decimal): Course progress
- `IsCompleted` (bool): Completion flag
- `CompletedAt` (DateTime?): Completion timestamp

### 8. UserTaskSubmission
Task submission tracking with JSONB submission data.

**Properties:**
- `Id` (Guid): Primary key
- `UserId` (Guid): Foreign key to User
- `TaskId` (Guid): Foreign key to LearningTask
- `SubmissionData` (jsonb): Flexible submission data
- `Status` (enum): Pending, Approved, Rejected, UnderReview
- `SubmittedAt` (DateTime): Submission timestamp
- `ReviewedAt` (DateTime?): Review timestamp
- `ReviewedByUserId` (Guid?): Reviewer user ID
- `FeedbackText` (string): Review feedback
- `RewardPointsAwarded` (int): Points awarded

**Domain Methods:**
- `Approve()`: Approve submission
- `Reject()`: Reject submission

### 9. RewardTransaction
Immutable ledger for reward points.

**Properties:**
- `Id` (Guid): Primary key
- `UserId` (Guid): Foreign key to User
- `Amount` (int): Transaction amount (can be negative)
- `TransactionType` (enum): Earned, Redeemed, Bonus, Penalty, Expired
- `ReferenceId` (string, 100): Reference ID
- `ReferenceType` (string, 50): Reference type
- `Description` (string, 500): Transaction description
- `CreatedAt` (DateTime): Transaction timestamp

### 10. DiscountCode
Point-based discount system.

**Properties:**
- `Id` (Guid): Primary key
- `Code` (string, 50): Unique discount code
- `DiscountPercentage` (int): Discount percentage
- `RequiredPoints` (int): Points required to redeem
- `MaxRedemptions` (int): Max redemptions (0 = unlimited)
- `CurrentRedemptions` (int): Current redemption count
- `ExpiryDate` (DateTime?): Expiry date (optional)
- `IsActive` (bool): Active status

**Default Codes:**
- `SAVE10`: 10% discount for 500 points
- `SAVE20`: 20% discount for 1000 points
- `SAVE30`: 30% discount for 2000 points

**Domain Methods:**
- `CanRedeem()`: Check if code can be redeemed
- `IncrementRedemptions()`: Increment redemption count

### 11. UserDiscountRedemption
Tracks discount code redemptions.

**Properties:**
- `Id` (Guid): Primary key
- `UserId` (Guid): Foreign key to User
- `DiscountCodeId` (Guid): Foreign key to DiscountCode
- `RedeemedAt` (DateTime): Redemption timestamp
- `UsedInSubscription` (bool): Usage flag

## Enums

### TaskType
```csharp
public enum TaskType
{
    Quiz,
    ExternalLink,
    Screenshot,
    TextSubmission,
    WalletVerification
}
```

### SubmissionStatus
```csharp
public enum SubmissionStatus
{
    Pending,
    Approved,
    Rejected,
    UnderReview
}
```

### TransactionType
```csharp
public enum TransactionType
{
    Earned,     // Points earned from completing tasks/lessons
    Redeemed,   // Points redeemed for discount codes or rewards
    Bonus,      // Bonus points from promotions/referrals
    Penalty,    // Points deducted as penalty
    Expired     // Points expired due to inactivity
}
```

### DifficultyLevel
```csharp
public enum DifficultyLevel
{
    Beginner,      // Introductory content
    Intermediate,  // Basic knowledge required
    Advanced,      // Complex topics
    Expert         // Advanced technical content
}
```

## Indexing Strategy

### User Indexes
```sql
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_role ON users(role);
CREATE INDEX idx_users_created_at ON users(created_at);
```

### Course Indexes
```sql
CREATE INDEX idx_courses_category_id ON courses(category_id);
CREATE INDEX idx_courses_is_published ON courses(is_published);
CREATE INDEX idx_courses_is_premium ON courses(is_premium);
CREATE INDEX idx_courses_difficulty_level ON courses(difficulty_level);
```

### Lesson Indexes
```sql
CREATE INDEX idx_lessons_course_id ON lessons(course_id);
CREATE INDEX idx_lessons_order_index ON lessons(order_index);
```

### LearningTask Indexes
```sql
CREATE INDEX idx_learning_tasks_lesson_id ON learning_tasks(lesson_id);
CREATE INDEX idx_learning_tasks_task_type ON learning_tasks(task_type);
```

### UserProgress Indexes
```sql
CREATE INDEX idx_user_progress_user_id ON user_progress(user_id);
CREATE INDEX idx_user_progress_lesson_id ON user_progress(lesson_id);
CREATE INDEX idx_user_progress_is_completed ON user_progress(is_completed);
CREATE UNIQUE INDEX idx_user_progress_user_lesson ON user_progress(user_id, lesson_id);
```

### UserTaskSubmission Indexes
```sql
CREATE INDEX idx_user_task_submissions_user_id ON user_task_submissions(user_id);
CREATE INDEX idx_user_task_submissions_task_id ON user_task_submissions(task_id);
CREATE INDEX idx_user_task_submissions_status ON user_task_submissions(status);
CREATE INDEX idx_user_task_submissions_submitted_at ON user_task_submissions(submitted_at);
```

### RewardTransaction Indexes
```sql
CREATE INDEX idx_reward_transactions_user_id ON reward_transactions(user_id);
CREATE INDEX idx_reward_transactions_created_at ON reward_transactions(created_at);
CREATE INDEX idx_reward_transactions_type ON reward_transactions(transaction_type);
```

### UserCourseEnrollment Indexes
```sql
CREATE INDEX idx_user_enrollments_user_id ON user_course_enrollments(user_id);
CREATE INDEX idx_user_enrollments_course_id ON user_course_enrollments(course_id);
CREATE UNIQUE INDEX idx_user_enrollments_user_course ON user_course_enrollments(user_id, course_id);
```

### DiscountCode Indexes
```sql
CREATE UNIQUE INDEX idx_discount_codes_code ON discount_codes(code);
CREATE INDEX idx_discount_codes_is_active ON discount_codes(is_active);
```

## Relationships and Cascade Rules

### Category → Course
- Relationship: One-to-Many
- Cascade: Restrict (cannot delete category with courses)

### Course → Lesson
- Relationship: One-to-Many
- Cascade: Cascade (delete lessons when course deleted)

### Lesson → LearningTask
- Relationship: One-to-Many
- Cascade: Cascade (delete tasks when lesson deleted)

### User → UserProgress
- Relationship: One-to-Many
- Cascade: Cascade (delete progress when user deleted)

### User → UserCourseEnrollment
- Relationship: One-to-Many
- Cascade: Cascade (delete enrollments when user deleted)

### User → UserTaskSubmission
- Relationship: One-to-Many
- Cascade: Cascade (delete submissions when user deleted)

### User → RewardTransaction
- Relationship: One-to-Many
- Cascade: Cascade (delete transactions when user deleted)

### DiscountCode → UserDiscountRedemption
- Relationship: One-to-Many
- Cascade: Cascade (delete redemptions when code deleted)

## JSONB Columns

### TaskData in LearningTask
Flexible structure for different task types:

**Quiz Task:**
```json
{
  "questions": [
    {
      "id": "q1",
      "text": "What is a blockchain?",
      "options": ["A", "B", "C", "D"],
      "correctAnswer": "A"
    }
  ]
}
```

**Screenshot Task:**
```json
{
  "instructions": "Take a screenshot showing...",
  "acceptedFormats": ["png", "jpg"],
  "maxSizeKB": 5000
}
```

**Wallet Verification:**
```json
{
  "walletType": "Ethereum",
  "verificationMethod": "signature",
  "requiredMinimumBalance": 0
}
```

### SubmissionData in UserTaskSubmission
Flexible structure for different submission types:

**Quiz Submission:**
```json
{
  "answers": {
    "q1": "A",
    "q2": "C"
  },
  "score": 80
}
```

**Screenshot Submission:**
```json
{
  "imageUrl": "https://storage.../screenshot.png",
  "uploadedAt": "2025-01-15T10:30:00Z"
}
```

## Time-Based Partitioning

### UserProgress Partitioning
Partition by month based on `LastUpdatedAt` for optimal performance:

```sql
-- Parent table
CREATE TABLE user_progress (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL,
    lesson_id UUID NOT NULL,
    last_watched_position INT DEFAULT 0,
    completion_percentage DECIMAL(5,2) DEFAULT 0,
    is_completed BOOLEAN DEFAULT false,
    completed_at TIMESTAMPTZ,
    reward_points_claimed BOOLEAN DEFAULT false,
    last_updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
) PARTITION BY RANGE (last_updated_at);

-- Monthly partitions
CREATE TABLE user_progress_2025_01 PARTITION OF user_progress
    FOR VALUES FROM ('2025-01-01') TO ('2025-02-01');

CREATE TABLE user_progress_2025_02 PARTITION OF user_progress
    FOR VALUES FROM ('2025-02-01') TO ('2025-03-01');
```

### RewardTransaction Partitioning
Partition by month based on `CreatedAt` for transaction history:

```sql
-- Parent table
CREATE TABLE reward_transactions (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL,
    amount INT NOT NULL,
    transaction_type VARCHAR(50) NOT NULL,
    reference_id VARCHAR(100),
    reference_type VARCHAR(50),
    description VARCHAR(500) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
) PARTITION BY RANGE (created_at);

-- Monthly partitions
CREATE TABLE reward_transactions_2025_01 PARTITION OF reward_transactions
    FOR VALUES FROM ('2025-01-01') TO ('2025-02-01');

CREATE TABLE reward_transactions_2025_02 PARTITION OF reward_transactions
    FOR VALUES FROM ('2025-02-01') TO ('2025-03-01');
```

## Repository Pattern

### Generic Repository Interface
```csharp
public interface IRepository<T> where T : class, IEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task SoftDeleteAsync(Guid id);
}
```

### Unit of Work Pattern
```csharp
public interface IUnitOfWork : IDisposable
{
    ICategoryRepository Categories { get; }
    ICourseRepository Courses { get; }
    ILessonRepository Lessons { get; }
    ILearningTaskRepository LearningTasks { get; }
    IUserProgressRepository UserProgress { get; }
    IUserCourseEnrollmentRepository Enrollments { get; }
    IUserTaskSubmissionRepository TaskSubmissions { get; }
    IRewardTransactionRepository RewardTransactions { get; }
    IDiscountCodeRepository DiscountCodes { get; }
    IUserRepository Users { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

## Seed Data

### Default Categories
1. **Airdrops** - Learn about crypto airdrops
2. **GameFi** - Gaming and blockchain
3. **Task-to-Earn** - Earn by completing tasks
4. **DeFi** - Decentralized Finance basics
5. **NFT Strategies** - NFT investment strategies

### Admin User
- **Email**: admin@wahadinicryptoquest.com
- **Password**: Admin@123 (should be changed in production)
- **Role**: Admin
- **Email Confirmed**: Yes

### Discount Codes
1. **SAVE10** - 10% discount for 500 points
2. **SAVE20** - 20% discount for 1000 points
3. **SAVE30** - 30% discount for 2000 points

## Migration Guide

### Creating a New Migration
```bash
cd backend/src/WahadiniCryptoQuest.DAL
dotnet ef migrations add MigrationName --startup-project ../WahadiniCryptoQuest.API
```

### Applying Migrations
```bash
cd backend/src/WahadiniCryptoQuest.DAL
dotnet ef database update --startup-project ../WahadiniCryptoQuest.API
```

### Reverting Migrations
```bash
cd backend/src/WahadiniCryptoQuest.DAL
dotnet ef database update PreviousMigrationName --startup-project ../WahadiniCryptoQuest.API
```

### Removing Last Migration
```bash
cd backend/src/WahadiniCryptoQuest.DAL
dotnet ef migrations remove --startup-project ../WahadiniCryptoQuest.API
```

## Best Practices

### 1. Entity Design
- Use factory methods for entity creation
- Implement domain logic in entity methods
- Use private setters for encapsulation
- Validate business rules in domain methods

### 2. Repository Usage
- Use Unit of Work for transactional operations
- Implement specific repositories for complex queries
- Use AsNoTracking for read-only operations
- Leverage eager loading for navigation properties

### 3. Performance Optimization
- Index frequently queried columns
- Use pagination for large result sets
- Implement caching for frequently accessed data
- Use partitioning for large tables

### 4. Data Integrity
- Use foreign key constraints
- Implement soft delete for important data
- Use transactions for multi-table operations
- Validate data at both domain and database level

### 5. Migrations
- Create descriptive migration names
- Review generated migration code
- Test migrations on development database first
- Document schema changes

## Troubleshooting

### Common Issues

#### 1. Migration Conflicts
**Problem**: Multiple migrations created simultaneously
**Solution**: 
```bash
dotnet ef migrations remove
git pull
dotnet ef migrations add YourMigration
```

#### 2. Connection String Issues
**Problem**: Cannot connect to PostgreSQL
**Solution**: Check `appsettings.json` connection string and PostgreSQL service status

#### 3. JSONB Column Errors
**Problem**: JSONB operations fail
**Solution**: Ensure Npgsql.EntityFrameworkCore.PostgreSQL package is installed and PostgreSQL version is 15+

#### 4. Partitioning Issues
**Problem**: Partition not found
**Solution**: Ensure partitions are created for current date range

## Performance Metrics

### Expected Performance
- **Course listing**: < 100ms (with pagination)
- **Lesson loading**: < 50ms (with caching)
- **Progress update**: < 20ms
- **Task submission**: < 50ms
- **Reward transaction**: < 30ms

### Monitoring Queries
```sql
-- Check table sizes
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;

-- Check index usage
SELECT 
    schemaname,
    tablename,
    indexname,
    idx_scan,
    idx_tup_read,
    idx_tup_fetch
FROM pg_stat_user_indexes
WHERE schemaname = 'public'
ORDER BY idx_scan DESC;
```

## Security Considerations

### 1. Sensitive Data
- Hash all passwords using BCrypt
- Never log sensitive data
- Use secure connection strings
- Implement proper access controls

### 2. SQL Injection Prevention
- Use parameterized queries (handled by EF Core)
- Validate all input data
- Use stored procedures for complex operations

### 3. Data Privacy
- Implement soft delete for user data
- Provide data export functionality
- Follow GDPR compliance guidelines
- Encrypt sensitive fields

## Future Enhancements

### Planned Features
1. **Full-text search** - Implement PostgreSQL full-text search for courses
2. **Analytics tables** - Add denormalized tables for reporting
3. **Audit logging** - Track all data modifications
4. **Data archiving** - Archive old partitions
5. **Replication** - Set up read replicas for scaling

### Schema Evolution
- Version control all schema changes
- Document breaking changes
- Provide migration paths for major updates
- Maintain backward compatibility when possible

## Support

For issues or questions:
- Review this documentation
- Check Entity Framework Core logs
- Review PostgreSQL logs
- Contact the development team

---

**Last Updated**: November 14, 2025
**Schema Version**: 1.0.0
**EF Core Version**: 8.0
**PostgreSQL Version**: 15+
