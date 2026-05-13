# Feature: Database Schema & Data Models

## Change Log
**Updated: November 14, 2025**
- ✅ Fixed entity naming: `Tasks` → `LearningTask` (singular, aligned with .NET conventions)
- ✅ Fixed property naming: `LastWatchedPosition` → `VideoWatchTimeSeconds`, `CompletionPercent` → `ProgressPercentage`, `RequiredPoints` → `PointCost`, `FeedbackText` → `ReviewNotes`, `YoutubeVideoId` → `YouTubeVideoId`
- ✅ Fixed RewardTransaction schema: Changed from generic `ReferenceId/ReferenceType` to explicit FKs (`RelatedTaskSubmissionId`, `RelatedDiscountRedemptionId`)
- ✅ Added missing audit fields: `UpdatedAt`, `IsDeleted`, `DeletedAt` to User, Category, Course, Lesson entities
- ✅ Updated table naming to singular form per .NET/EF Core conventions
- ✅ Added JSONB GIN indexes for `TaskData` and `SubmissionData` columns
- ✅ Clarified RefreshToken computed properties: `IsExpired`, `IsRevoked`, `IsActive` are computed from `ExpiresAt` and `RevokedAt`
- ✅ Added query filters for soft delete on User, Category, Course, Lesson entities

## /speckit.specify

### Feature Overview
Design and implement a comprehensive PostgreSQL database schema with all entities, relationships, indexes, and migrations for the crypto learning platform.

### Feature Scope
- Define all core entities (Users, Courses, Lessons, Tasks, Rewards, Subscriptions)
- Establish relationships and foreign keys
- Create performance indexes
- Implement time-based partitioning for user activity data
- Set up Entity Framework Core with code-first migrations
- Seed initial data (categories, admin user)

### Key Entities
1. Users - User accounts with authentication data
2. Categories - Course categories (Airdrops, GameFi, DeFi, NFT, Task-to-Earn)
3. Courses - Educational courses with metadata
4. Lessons - Individual lessons with YouTube videos
5. Tasks - Interactive tasks for verification
6. UserTaskSubmissions - Task submission tracking
7. UserProgress - Lesson completion tracking
8. UserCourseEnrollments - Course enrollment records
9. RewardTransactions - Immutable reward points ledger
10. DiscountCodes - Point-based discount system
11. RefreshTokens - JWT refresh token storage

### Technical Requirements
- PostgreSQL 15+ with JSONB support
- Entity Framework Core 8.0
- Code-first migrations
- Proper indexing for performance
- Cascade delete rules
- Timestamp tracking (CreatedAt, UpdatedAt)
- Soft delete support where needed
- Data validation at database level

---

## /speckit.plan

### Implementation Plan

#### Phase 1: Core Entity Models (Domain Layer)
**Tasks:**
1. Create User entity (extends IdentityUser)
2. Create Category entity
3. Create Course entity with relationships
4. Create Lesson entity
5. Create Task entity with JSONB TaskData
6. Add enums (TaskType, SubscriptionTier, Role, etc.)

**Deliverables:**
- All entity classes in Domain layer
- Proper navigation properties
- Data annotations
- Domain enums

#### Phase 2: Progress & Engagement Entities
**Tasks:**
1. Create UserProgress entity
2. Create UserCourseEnrollment entity
3. Create UserTaskSubmission entity with JSONB
4. Add relationships to User, Course, Lesson, Task

**Deliverables:**
- Progress tracking entities
- Enrollment tracking
- Submission tracking with flexible data storage

#### Phase 3: Rewards & Economy Entities
**Tasks:**
1. Create RewardTransaction entity (immutable ledger)
2. Create DiscountCode entity
3. Create UserDiscountRedemption entity
4. Add transaction types enum

**Deliverables:**
- Reward system entities
- Discount system entities
- Proper constraints and indexes

#### Phase 4: DbContext Configuration
**Tasks:**
1. Create ApplicationDbContext with all DbSets
2. Configure Fluent API for relationships
3. Add indexes for performance
4. Configure cascade delete rules
5. Set up value conversions (enums, JSONB)
6. Add timestamp default values

**Deliverables:**
- Complete ApplicationDbContext
- Fluent API configurations
- Proper constraints and indexes

#### Phase 5: Migrations & Seed Data
**Tasks:**
1. Create initial migration
2. Create seed data method
3. Seed categories
4. Seed admin user
5. Seed sample courses (optional)

**Deliverables:**
- Initial migration files
- DbInitializer class
- Seed data scripts

#### Phase 6: Repository Interfaces
**Tasks:**
1. Create generic IRepository<T> interface
2. Create specific repository interfaces (IUserRepository, ICourseRepository, etc.)
3. Define common query methods
4. Add pagination support

**Deliverables:**
- Repository interfaces in Application layer
- Standard CRUD methods
- Specialized query methods

---

## /speckit.clarify

### Questions & Answers

**Q: Should we use soft delete or hard delete?**
A: Use soft delete for Users, Courses, and Lessons (IsActive flag). Hard delete for progress and submissions if needed.

**Q: How to handle JSONB in Entity Framework?**
A: Use owned entity types or value converters. For TaskData and SubmissionData, use JSON column type with EF Core 7+ native JSON support.

**Q: What indexing strategy?**
A: Index all foreign keys, email fields, and frequently queried fields (Status, IsActive, dates). Composite indexes for common filter combinations.

**Q: Time-based partitioning implementation?**
A: Implement partitioning for UserProgress and RewardTransactions tables partitioned by CreatedAt month for performance.

**Q: How to handle enum storage?**
A: Store enums as strings in database for readability and migration safety. Use value converters in EF Core.

**Q: Cascade delete rules?**
A: Cascade delete for child entities (lessons -> tasks, user -> progress). Restrict delete for referenced entities (category -> courses).

**Q: Maximum lengths for text fields?**
A: Email: 255, Username: 100, Title: 200, Description: 2000, ContentMarkdown: unlimited (TEXT), URLs: 500

---

## /speckit.analyze

### Database Schema Design

#### Complete ERD

```
┌──────────────────┐
│       User       │
├──────────────────┤
│ Id (PK)          │──┐
│ Email            │  │
│ Username         │  │
│ FirstName        │  │
│ LastName         │  │
│ PasswordHash     │  │
│ Role             │  │
│ SubscriptionTier │  │
│ SubscriptionExp  │  │
│ RewardPoints     │  │
│ EmailVerified    │  │
│ CreatedAt        │  │
│ UpdatedAt        │  │
│ IsDeleted        │  │
│ DeletedAt        │  │
│ IsActive         │  │
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
│ RevokedAt          │
│ CreatedAt          │
└────────────────────┘
Note: IsExpired, IsRevoked, IsActive are computed properties

┌──────────────────┐
│     Category     │
├──────────────────┤
│ Id (PK)          │──┐
│ Name             │  │
│ Description      │  │
│ IconUrl          │  │
│ DisplayOrder     │  │
│ CreatedAt        │  │
│ UpdatedAt        │  │
│ IsDeleted        │  │
│ DeletedAt        │  │
│ IsActive         │  │
└──────────────────┘  │
                      │
┌─────────────────────▼┐
│       Course         │
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
│ DeletedAt            │
└──────────────────────┘
         │
         │
┌────────▼───────────┐
│       Lesson       │
├────────────────────┤
│ Id (PK)            │──┐
│ CourseId (FK)      │◄─┘
│ Title              │
│ Description        │
│ YouTubeVideoId     │
│ Duration           │
│ OrderIndex         │
│ IsPremium          │
│ RewardPoints       │
│ ContentMarkdown    │
│ CreatedAt          │
│ UpdatedAt          │
│ IsDeleted          │
│ DeletedAt          │
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
│ VideoWatchTimeSeconds│      │ EnrolledAt           │
│ ProgressPercentage   │      │ LastAccessedAt       │
│ IsCompleted          │      │ ProgressPercentage   │
│ CompletedAt          │      │ IsCompleted          │
│ RewardPointsClaimed  │      │ CompletedAt          │
│ LastUpdatedAt        │      └──────────────────────┘
└──────────────────────┘

┌────────────────────────┐
│ UserTaskSubmission     │
├────────────────────────┤
│ Id (PK)                │
│ UserId (FK)            │
│ LearningTaskId (FK)    │
│ SubmissionData (JSONB) │
│ Status                 │
│ SubmittedAt            │
│ ReviewedAt             │
│ ReviewedByUserId (FK)  │
│ ReviewNotes            │
│ RewardPointsAwarded    │
└────────────────────────┘

┌────────────────────────────┐
│  RewardTransaction         │
├────────────────────────────┤
│ Id (PK)                    │
│ UserId (FK)                │
│ Amount                     │
│ TransactionType            │
│ RelatedTaskSubmissionId    │
│ RelatedDiscountRedemptionId│
│ Description                │
│ CreatedAt                  │
└────────────────────────────┘

┌──────────────────────┐
│   DiscountCode       │
├──────────────────────┤
│ Id (PK)              │
│ Code                 │
│ DiscountPercentage   │
│ PointCost            │
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

#### Indexes Strategy

```sql
-- User indexes
CREATE INDEX idx_user_email ON User(Email);
CREATE INDEX idx_user_username ON User(Username);
CREATE INDEX idx_user_role ON User(Role);
CREATE INDEX idx_user_subscription_tier ON User(SubscriptionTier);

-- Course indexes
CREATE INDEX idx_course_category_id ON Course(CategoryId);
CREATE INDEX idx_course_is_published ON Course(IsPublished);
CREATE INDEX idx_course_is_premium ON Course(IsPremium);
CREATE INDEX idx_course_difficulty ON Course(DifficultyLevel);

-- Lesson indexes
CREATE INDEX idx_lesson_course_id ON Lesson(CourseId);
CREATE INDEX idx_lesson_order_index ON Lesson(OrderIndex);

-- LearningTask indexes
CREATE INDEX idx_learning_task_lesson_id ON LearningTask(LessonId);
CREATE INDEX idx_learning_task_task_type ON LearningTask(TaskType);

-- JSONB GIN indexes for flexible querying
CREATE INDEX idx_learning_task_data_gin ON LearningTask USING gin(TaskData jsonb_path_ops);
CREATE INDEX idx_user_task_submission_data_gin ON UserTaskSubmission USING gin(SubmissionData jsonb_path_ops);

-- UserProgress indexes
CREATE INDEX idx_user_progress_user_id ON UserProgress(UserId);
CREATE INDEX idx_user_progress_lesson_id ON UserProgress(LessonId);
CREATE INDEX idx_user_progress_is_completed ON UserProgress(IsCompleted);
CREATE UNIQUE INDEX idx_user_progress_user_lesson ON UserProgress(UserId, LessonId);

-- UserTaskSubmission indexes
CREATE INDEX idx_user_task_submission_user_id ON UserTaskSubmission(UserId);
CREATE INDEX idx_user_task_submission_learning_task_id ON UserTaskSubmission(LearningTaskId);
CREATE INDEX idx_user_task_submission_status ON UserTaskSubmission(Status);
CREATE INDEX idx_user_task_submission_submitted_at ON UserTaskSubmission(SubmittedAt);

-- RewardTransaction indexes
CREATE INDEX idx_reward_transaction_user_id ON RewardTransaction(UserId);
CREATE INDEX idx_reward_transaction_created_at ON RewardTransaction(CreatedAt);
CREATE INDEX idx_reward_transaction_type ON RewardTransaction(TransactionType);
CREATE INDEX idx_reward_transaction_task_submission_id ON RewardTransaction(RelatedTaskSubmissionId);
CREATE INDEX idx_reward_transaction_discount_redemption_id ON RewardTransaction(RelatedDiscountRedemptionId);

-- Enrollment indexes
CREATE INDEX idx_user_course_enrollment_user_id ON UserCourseEnrollment(UserId);
CREATE INDEX idx_user_course_enrollment_course_id ON UserCourseEnrollment(CourseId);
CREATE UNIQUE INDEX idx_user_course_enrollment_user_course ON UserCourseEnrollment(UserId, CourseId);

-- Discount indexes
CREATE UNIQUE INDEX idx_discount_code_code ON DiscountCode(Code);
CREATE INDEX idx_discount_code_is_active ON DiscountCode(IsActive);
```

#### Enums

```csharp
public enum TaskType
{
    Quiz,
    ExternalLink,
    Screenshot,
    TextSubmission,
    WalletVerification
}

public enum SubscriptionTier
{
    Free,
    Monthly,
    Yearly
}

public enum UserRole
{
    Free,
    Premium,
    Admin,
    ContentCreator,
    Moderator
}

public enum SubmissionStatus
{
    Pending,
    Approved,
    Rejected
}

public enum TransactionType
{
    Earned,
    Redeemed,
    Bonus,
    Penalty,
    Expired
}

public enum DifficultyLevel
{
    Beginner,
    Intermediate,
    Advanced
}
```

---

## /speckit.checklist

### Implementation Checklist

#### Entity Creation
- [ ] Create all enum types
- [ ] Create User entity (extends IdentityUser)
- [ ] Create RefreshToken entity
- [ ] Create Category entity
- [ ] Create Course entity
- [ ] Create Lesson entity
- [ ] Create Task entity with JSONB
- [ ] Create UserProgress entity
- [ ] Create UserCourseEnrollment entity
- [ ] Create UserTaskSubmission entity with JSONB
- [ ] Create RewardTransaction entity
- [ ] Create DiscountCode entity
- [ ] Create UserDiscountRedemption entity

#### Navigation Properties
- [ ] Add navigation properties to all entities
- [ ] Configure bidirectional relationships
- [ ] Set up collection initializers

#### DbContext Configuration
- [ ] Create ApplicationDbContext
- [ ] Add all DbSets
- [ ] Configure entity relationships with Fluent API
- [ ] Set up cascade delete rules
- [ ] Configure indexes
- [ ] Set up value converters for enums
- [ ] Configure JSONB columns
- [ ] Set up timestamp defaults
- [ ] Add query filters for soft deletes

#### Migrations
- [ ] Install EF Core tools
- [ ] Create initial migration
- [ ] Review generated migration code
- [ ] Apply migration to local database
- [ ] Test database schema

#### Seed Data
- [ ] Create DbInitializer class
- [ ] Seed categories
- [ ] Seed admin user
- [ ] Seed sample courses (optional)
- [ ] Test seed data execution

#### Repository Interfaces
- [ ] Create IRepository<T> generic interface
- [ ] Create IUserRepository
- [ ] Create ICategoryRepository
- [ ] Create ICourseRepository
- [ ] Create ILessonRepository
- [ ] Create ITaskRepository
- [ ] Create IProgressRepository
- [ ] Create IRewardRepository
- [ ] Create IDiscountRepository

#### Validation
- [ ] Test all entity validations
- [ ] Test foreign key constraints
- [ ] Test unique constraints
- [ ] Test cascade delete behavior
- [ ] Test JSONB storage and retrieval
- [ ] Verify index creation
- [ ] Test timestamp automatic updates

#### Documentation
- [ ] Document ERD diagram
- [ ] Document all entities
- [ ] Document relationships
- [ ] Document indexes
- [ ] Create migration guide
- [ ] Document seed data

---

## /speckit.tasks

### Task Breakdown (Estimated 25-30 hours)

#### Task 1: Create Domain Enums (1 hour)
**Description:** Define all enum types needed for the domain
**Subtasks:**
1. Create Enums folder in Domain project
2. Define TaskType enum
3. Define SubscriptionTier enum
4. Define UserRole enum
5. Define SubmissionStatus enum
6. Define TransactionType enum
7. Define DifficultyLevel enum

#### Task 2: Create Core Entities (4 hours)
**Description:** Build User, Category, Course, Lesson entities
**Subtasks:**
1. Create Entities folder in Domain
2. Create User entity with all properties
3. Create RefreshToken entity
4. Create Category entity
5. Create Course entity with relationships
6. Create Lesson entity with relationships
7. Add data annotations
8. Add navigation properties

#### Task 3: Create Task Entity (2 hours)
**Description:** Build Task entity with JSONB support
**Subtasks:**
1. Create Task entity
2. Define TaskData as string (for JSONB)
3. Add TaskType enum property
4. Add validation attributes
5. Add navigation properties
6. Create TaskData DTOs for different task types

#### Task 4: Create Progress Entities (3 hours)
**Description:** Build progress tracking entities
**Subtasks:**
1. Create UserProgress entity
2. Create UserCourseEnrollment entity
3. Create UserTaskSubmission entity with JSONB
4. Add relationships to User, Course, Lesson, Task
5. Add validation attributes
6. Add indexes annotations

#### Task 5: Create Reward Entities (2 hours)
**Description:** Build reward and discount entities
**Subtasks:**
1. Create RewardTransaction entity
2. Create DiscountCode entity
3. Create UserDiscountRedemption entity
4. Add relationships
5. Add validation attributes
6. Add unique constraints

#### Task 6: Configure ApplicationDbContext (4 hours)
**Description:** Set up DbContext with Fluent API
**Subtasks:**
1. Create ApplicationDbContext class
2. Add all DbSets
3. Configure User entity with Fluent API
4. Configure Course entity and relationships
5. Configure Lesson entity and relationships
6. Configure Task entity with JSONB
7. Configure progress entities
8. Configure reward entities
9. Set up cascade delete rules
10. Add query filters for soft delete

#### Task 7: Configure Indexes (2 hours)
**Description:** Add performance indexes
**Subtasks:**
1. Add indexes to User entity
2. Add indexes to Course and Lesson entities
3. Add indexes to Task entity
4. Add indexes to UserProgress
5. Add indexes to UserTaskSubmissions
6. Add indexes to RewardTransactions
7. Add unique indexes where needed
8. Test index creation

#### Task 8: Create Migrations (2 hours)
**Description:** Generate and test EF Core migrations
**Subtasks:**
1. Install EF Core CLI tools
2. Generate initial migration
3. Review migration code
4. Test migration on local database
5. Verify schema correctness
6. Create rollback test
7. Document migration commands

#### Task 9: Create Seed Data (3 hours)
**Description:** Build database initialization with seed data
**Subtasks:**
1. Create DbInitializer class
2. Implement SeedCategoriesAsync method
3. Implement SeedAdminUserAsync method
4. Implement SeedDiscountCodesAsync method
5. Implement SeedSampleCoursesAsync (optional)
6. Call seed methods in Program.cs
7. Test seed execution

#### Task 10: Create Repository Interfaces (2 hours)
**Description:** Define repository contracts
**Subtasks:**
1. Create IRepository<T> generic interface
2. Define common CRUD methods
3. Create IUserRepository with specific methods
4. Create ICourseRepository with filtering methods
5. Create ILessonRepository
6. Create ITaskRepository
7. Create IProgressRepository
8. Create IRewardRepository
9. Add pagination support interfaces

#### Task 11: Testing & Validation (3 hours)
**Description:** Comprehensive testing of database schema
**Subtasks:**
1. Test entity creation
2. Test relationships and navigation
3. Test cascade deletes
4. Test unique constraints
5. Test JSONB storage/retrieval
6. Test indexes effectiveness
7. Test seed data
8. Performance test with sample data

#### Task 12: Documentation (2 hours)
**Description:** Document database design
**Subtasks:**
1. Create ERD diagram
2. Document entity descriptions
3. Document relationships
4. Document indexes rationale
5. Create migration guide
6. Document seed data
7. Create troubleshooting guide

---

## /speckit.implement

### Implementation Code

#### Enums

**File:** `WahadiniCryptoQuest.Domain/Enums/TaskType.cs`
```csharp
namespace WahadiniCryptoQuest.Domain.Enums;

public enum TaskType
{
    Quiz,
    ExternalLink,
    Screenshot,
    TextSubmission,
    WalletVerification
}
```

**File:** `WahadiniCryptoQuest.Domain/Enums/SubmissionStatus.cs`
```csharp
namespace WahadiniCryptoQuest.Domain.Enums;

public enum SubmissionStatus
{
    Pending,
    Approved,
    Rejected
}
```

**File:** `WahadiniCryptoQuest.Domain/Enums/TransactionType.cs`
```csharp
namespace WahadiniCryptoQuest.Domain.Enums;

public enum TransactionType
{
    Earned,
    Redeemed,
    Bonus,
    Penalty,
    Expired
}
```

**File:** `WahadiniCryptoQuest.Domain/Enums/DifficultyLevel.cs`
```csharp
namespace WahadiniCryptoQuest.Domain.Enums;

public enum DifficultyLevel
{
    Beginner,
    Intermediate,
    Advanced
}
```

#### Core Entities

**File:** `WahadiniCryptoQuest.Domain/Entities/Category.cs`
```csharp
using System.ComponentModel.DataAnnotations;

namespace WahadiniCryptoQuest.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? IconUrl { get; set; }
    
    public int DisplayOrder { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
```

**File:** `WahadiniCryptoQuest.Domain/Entities/Course.cs`
```csharp
using System.ComponentModel.DataAnnotations;
using WahadiniCryptoQuest.Domain.Enums;

namespace WahadiniCryptoQuest.Domain.Entities;

public class Course
{
    public Guid Id { get; set; }
    
    public Guid CategoryId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }
    
    public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Beginner;
    
    public int EstimatedDuration { get; set; } // in minutes
    
    public bool IsPremium { get; set; } = false;
    
    public int RewardPoints { get; set; } = 0;
    
    public bool IsPublished { get; set; } = false;
    
    public int ViewCount { get; set; } = 0;
    
    public Guid? CreatedByUserId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public virtual Category Category { get; set; } = null!;
    public virtual User? CreatedBy { get; set; }
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    public virtual ICollection<UserCourseEnrollment> Enrollments { get; set; } = new List<UserCourseEnrollment>();
}
```

**File:** `WahadiniCryptoQuest.Domain/Entities/Lesson.cs`
```csharp
using System.ComponentModel.DataAnnotations;

namespace WahadiniCryptoQuest.Domain.Entities;

public class Lesson
{
    public Guid Id { get; set; }
    
    public Guid CourseId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string YouTubeVideoId { get; set; } = string.Empty;
    
    public int Duration { get; set; } // in minutes
    
    public int OrderIndex { get; set; }
    
    public bool IsPremium { get; set; } = false;
    
    public int RewardPoints { get; set; } = 0;
    
    public string? ContentMarkdown { get; set; } // Unlimited text
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public virtual Course Course { get; set; } = null!;
    public virtual ICollection<LearningTask> Tasks { get; set; } = new List<LearningTask>();
    public virtual ICollection<UserProgress> UserProgress { get; set; } = new List<UserProgress>();
}
```

**File:** `WahadiniCryptoQuest.Domain/Entities/LearningTask.cs`
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WahadiniCryptoQuest.Domain.Enums;

namespace WahadiniCryptoQuest.Domain.Entities;

public class LearningTask
{
    public Guid Id { get; set; }
    
    public Guid LessonId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    public TaskType TaskType { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string TaskData { get; set; } = "{}"; // JSONB column
    
    public int RewardPoints { get; set; } = 0;
    
    public int? TimeLimit { get; set; } // in minutes, nullable
    
    public int OrderIndex { get; set; }
    
    public bool IsRequired { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Lesson Lesson { get; set; } = null!;
    public virtual ICollection<UserTaskSubmission> Submissions { get; set; } = new List<UserTaskSubmission>();
}
```

#### Progress Tracking Entities

**File:** `WahadiniCryptoQuest.Domain/Entities/UserProgress.cs`
```csharp
namespace WahadiniCryptoQuest.Domain.Entities;

public class UserProgress
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public Guid LessonId { get; set; }
    
    public int VideoWatchTimeSeconds { get; set; } = 0; // in seconds
    
    public decimal ProgressPercentage { get; set; } = 0;
    
    public bool IsCompleted { get; set; } = false;
    
    public DateTime? CompletedAt { get; set; }
    
    public bool RewardPointsClaimed { get; set; } = false;
    
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Lesson Lesson { get; set; } = null!;
}
```

**File:** `WahadiniCryptoQuest.Domain/Entities/UserCourseEnrollment.cs`
```csharp
namespace WahadiniCryptoQuest.Domain.Entities;

public class UserCourseEnrollment
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public Guid CourseId { get; set; }
    
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
    
    public decimal ProgressPercentage { get; set; } = 0;
    
    public bool IsCompleted { get; set; } = false;
    
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Course Course { get; set; } = null!;
}
```

**File:** `WahadiniCryptoQuest.Domain/Entities/UserTaskSubmission.cs`
```csharp
using System.ComponentModel.DataAnnotations.Schema;
using WahadiniCryptoQuest.Domain.Enums;

namespace WahadiniCryptoQuest.Domain.Entities;

public class UserTaskSubmission
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public Guid LearningTaskId { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string SubmissionData { get; set; } = "{}"; // JSONB column
    
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;
    
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ReviewedAt { get; set; }
    
    public Guid? ReviewedByUserId { get; set; }
    
    public string? ReviewNotes { get; set; }
    
    public int RewardPointsAwarded { get; set; } = 0;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual LearningTask LearningTask { get; set; } = null!;
    public virtual User? ReviewedBy { get; set; }
}
```

#### Reward System Entities

**File:** `WahadiniCryptoQuest.Domain/Entities/RewardTransaction.cs`
```csharp
using System.ComponentModel.DataAnnotations;
using WahadiniCryptoQuest.Domain.Enums;

namespace WahadiniCryptoQuest.Domain.Entities;

public class RewardTransaction
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public int Amount { get; set; } // Can be negative for redemptions
    
    public TransactionType TransactionType { get; set; }
    
    public Guid? RelatedTaskSubmissionId { get; set; }
    
    public Guid? RelatedDiscountRedemptionId { get; set; }
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual UserTaskSubmission? RelatedTaskSubmission { get; set; }
    public virtual UserDiscountRedemption? RelatedDiscountRedemption { get; set; }
}
```

**File:** `WahadiniCryptoQuest.Domain/Entities/DiscountCode.cs`
```csharp
using System.ComponentModel.DataAnnotations;

namespace WahadiniCryptoQuest.Domain.Entities;

public class DiscountCode
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;
    
    public int DiscountPercentage { get; set; }
    
    public int PointCost { get; set; }
    
    public int MaxRedemptions { get; set; } = 0; // 0 = unlimited
    
    public int CurrentRedemptions { get; set; } = 0;
    
    public DateTime? ExpiryDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<UserDiscountRedemption> Redemptions { get; set; } = new List<UserDiscountRedemption>();
}
```

**File:** `WahadiniCryptoQuest.Domain/Entities/UserDiscountRedemption.cs`
```csharp
namespace WahadiniCryptoQuest.Domain.Entities;

public class UserDiscountRedemption
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public Guid DiscountCodeId { get; set; }
    
    public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;
    
    public bool UsedInSubscription { get; set; } = false;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual DiscountCode DiscountCode { get; set; } = null!;
}
```

#### ApplicationDbContext with Fluent API

**File:** `WahadiniCryptoQuest.Infrastructure/Data/ApplicationDbContext.cs`
```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Domain.Entities;
using WahadiniCryptoQuest.Domain.Enums;

namespace WahadiniCryptoQuest.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    // DbSets
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<LearningTask> LearningTasks { get; set; }
    public DbSet<UserProgress> UserProgress { get; set; }
    public DbSet<UserCourseEnrollment> UserCourseEnrollments { get; set; }
    public DbSet<UserTaskSubmission> UserTaskSubmissions { get; set; }
    public DbSet<RewardTransaction> RewardTransactions { get; set; }
    public DbSet<DiscountCode> DiscountCodes { get; set; }
    public DbSet<UserDiscountRedemption> UserDiscountRedemptions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Role);
            entity.HasIndex(e => e.SubscriptionTier);
            
            // Query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
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
        
        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.DisplayOrder);
            entity.HasIndex(e => e.IsActive);
            
            // Query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        
        // Course configuration
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsPublished);
            entity.HasIndex(e => e.IsPremium);
            entity.HasIndex(e => e.DifficultyLevel);
            
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Courses)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.CreatedBy)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            // Enum stored as string
            entity.Property(e => e.DifficultyLevel)
                  .HasConversion<string>();
            
            // Query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        
        // Lesson configuration
        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasIndex(e => e.CourseId);
            entity.HasIndex(e => e.OrderIndex);
            
            entity.HasOne(e => e.Course)
                  .WithMany(c => c.Lessons)
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        
        // LearningTask configuration
        modelBuilder.Entity<LearningTask>(entity =>
        {
            entity.HasIndex(e => e.LessonId);
            entity.HasIndex(e => e.TaskType);
            
            entity.HasOne(e => e.Lesson)
                  .WithMany(l => l.Tasks)
                  .HasForeignKey(e => e.LessonId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Enum stored as string
            entity.Property(e => e.TaskType)
                  .HasConversion<string>();
        });
        
        // UserProgress configuration
        modelBuilder.Entity<UserProgress>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.LessonId);
            entity.HasIndex(e => e.IsCompleted);
            entity.HasIndex(e => new { e.UserId, e.LessonId }).IsUnique();
            
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Progress)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Lesson)
                  .WithMany(l => l.UserProgress)
                  .HasForeignKey(e => e.LessonId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        // UserCourseEnrollment configuration
        modelBuilder.Entity<UserCourseEnrollment>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CourseId);
            entity.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
            
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Enrollments)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Course)
                  .WithMany(c => c.Enrollments)
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        // UserTaskSubmission configuration
        modelBuilder.Entity<UserTaskSubmission>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.LearningTaskId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.SubmittedAt);
            
            entity.HasOne(e => e.User)
                  .WithMany(u => u.TaskSubmissions)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.LearningTask)
                  .WithMany(t => t.Submissions)
                  .HasForeignKey(e => e.LearningTaskId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ReviewedBy)
                  .WithMany()
                  .HasForeignKey(e => e.ReviewedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            // Enum stored as string
            entity.Property(e => e.Status)
                  .HasConversion<string>();
        });
        
        // RewardTransaction configuration
        modelBuilder.Entity<RewardTransaction>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.TransactionType);
            
            entity.HasOne(e => e.User)
                  .WithMany(u => u.RewardTransactions)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.RelatedTaskSubmission)
                  .WithMany()
                  .HasForeignKey(e => e.RelatedTaskSubmissionId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.RelatedDiscountRedemption)
                  .WithMany()
                  .HasForeignKey(e => e.RelatedDiscountRedemptionId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            // Enum stored as string
            entity.Property(e => e.TransactionType)
                  .HasConversion<string>();
        });
        
        // DiscountCode configuration
        modelBuilder.Entity<DiscountCode>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });
        
        // UserDiscountRedemption configuration
        modelBuilder.Entity<UserDiscountRedemption>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.DiscountCodeId);
            
            entity.HasOne(e => e.User)
                  .WithMany(u => u.DiscountRedemptions)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.DiscountCode)
                  .WithMany(d => d.Redemptions)
                  .HasForeignKey(e => e.DiscountCodeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

#### Seed Data

**File:** `WahadiniCryptoQuest.Infrastructure/Data/DbInitializer.cs`
```csharp
using Microsoft.AspNetCore.Identity;
using WahadiniCryptoQuest.Domain.Entities;

namespace WahadiniCryptoQuest.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<User> userManager)
    {
        // Seed Categories
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new() { Name = "Airdrops", Description = "Learn about crypto airdrops", DisplayOrder = 1, IconUrl = "/icons/airdrop.svg" },
                new() { Name = "GameFi", Description = "Gaming and blockchain", DisplayOrder = 2, IconUrl = "/icons/gamefi.svg" },
                new() { Name = "Task-to-Earn", Description = "Earn by completing tasks", DisplayOrder = 3, IconUrl = "/icons/task.svg" },
                new() { Name = "DeFi", Description = "Decentralized Finance basics", DisplayOrder = 4, IconUrl = "/icons/defi.svg" },
                new() { Name = "NFT Strategies", Description = "NFT investment strategies", DisplayOrder = 5, IconUrl = "/icons/nft.svg" }
            };
            
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }
        
        // Seed Admin User
        if (!await userManager.Users.AnyAsync(u => u.Email == "admin@WahadiniCryptoQuest.com"))
        {
            var adminUser = new User
            {
                Email = "admin@WahadiniCryptoQuest.com",
                Username = "admin",
                UserName = "admin@WahadiniCryptoQuest.com",
                EmailConfirmed = true,
                Role = "Admin",
                SubscriptionTier = "Yearly"
            };
            
            await userManager.CreateAsync(adminUser, "Admin@123");
        }
        
        // Seed Discount Codes
        if (!context.DiscountCodes.Any())
        {
            var discountCodes = new List<DiscountCode>
            {
                new() { Code = "SAVE10", DiscountPercentage = 10, PointCost = 500, MaxRedemptions = 0 },
                new() { Code = "SAVE20", DiscountPercentage = 20, PointCost = 1000, MaxRedemptions = 0 },
                new() { Code = "SAVE30", DiscountPercentage = 30, PointCost = 2000, MaxRedemptions = 0 }
            };
            
            context.DiscountCodes.AddRange(discountCodes);
            await context.SaveChangesAsync();
        }
    }
}
```

### Notes
- Always use migrations for schema changes
- Test JSONB columns thoroughly in PostgreSQL
- Ensure proper indexing before production
- Monitor query performance with sample data
- Keep migrations reversible
