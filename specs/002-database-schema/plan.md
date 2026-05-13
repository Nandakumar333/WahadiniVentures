# Implementation Plan: Database Schema Design

**Feature**: 002-database-schema  
**Created**: November 14, 2025  
**Status**: In Planning  
**Specification**: [spec.md](spec.md)

---

## Technical Context

### Technology Stack
- **.NET 8.0** - Backend framework
- **Entity Framework Core 8.0** - ORM with code-first migrations
- **PostgreSQL 15+** - Database with JSONB support and time-based partitioning
- **ASP.NET Identity** - User authentication and authorization
- **Npgsql.EntityFrameworkCore.PostgreSQL** - PostgreSQL provider for EF Core
- **Microsoft.EntityFrameworkCore.Tools** - Migration CLI tools

### Architecture Patterns
- **Clean Architecture** - Domain/Application/Infrastructure/Presentation layers
- **Domain-Driven Design** - Rich domain models with business logic
- **Repository Pattern** - Data access abstraction
- **Unit of Work** - Transaction management
- **CQRS** - Command Query Responsibility Segregation (future)
- **Factory Pattern** - Entity creation with validation

### Key Design Decisions

#### 1. Entity Framework Core Code-First Approach
**Decision**: Use EF Core code-first migrations for all schema changes  
**Rationale**:
- Version control for database schema
- Automated migration generation
- Rollback support
- Cross-environment consistency
- CI/CD integration friendly

**Alternatives Considered**:
- Database-first: Rejected due to lack of version control and manual sync issues
- SQL scripts: Rejected due to lack of type safety and ORM integration

#### 2. Soft Delete Strategy
**Decision**: Implement soft delete for Users, Categories, Courses, and Lessons using IsDeleted flag  
**Rationale**:
- Data recovery capability
- Audit trail preservation
- GDPR compliance (right to be forgotten vs data retention)
- Historical reporting requirements

**Implementation**:
- Add IsDeleted (bool) and DeletedAt (DateTime?) to entities
- Configure global query filters in DbContext
- Provide explicit IncludeDeleted() extension for admin queries

#### 3. Enum Storage as Strings
**Decision**: Store all enums as strings in database using value converters  
**Rationale**:
- Database readability (SELECT queries show "Beginner" not 0)
- Migration safety (adding/reordering enum values doesn't break data)
- Cross-platform compatibility
- Easier debugging and troubleshooting

**Trade-offs**:
- Slightly larger storage (varchar vs int)
- Acceptable given modern storage costs and query performance

#### 4. JSONB for Flexible Data
**Decision**: Use PostgreSQL JSONB for TaskData and SubmissionData columns  
**Rationale**:
- Different task types require different schemas (Quiz vs Screenshot vs Wallet)
- Frequent schema evolution expected as task types expand
- PostgreSQL JSONB supports indexing and efficient queries
- Avoids EAV anti-pattern or excessive nullable columns

**Schema Patterns**:
```json
// Quiz TaskData
{
  "questions": [
    {
      "question": "What is blockchain?",
      "options": ["A", "B", "C", "D"],
      "correctAnswer": "A"
    }
  ]
}

// Screenshot TaskData
{
  "instructions": "Upload screenshot of wallet",
  "requiredElements": ["balance", "address"]
}

// Wallet TaskData
{
  "requiredNetwork": "Ethereum",
  "verificationMethod": "sign_message"
}
```

#### 5. Immutable Reward Ledger
**Decision**: RewardTransactions is append-only with no updates or deletes  
**Rationale**:
- Financial audit requirements
- Point balance always calculable from history
- Fraud detection and abuse prevention
- Regulatory compliance

**Implementation**:
- No Update/Delete methods in repository
- Calculate CurrentPoints as SUM of transactions
- Use database constraints to prevent modifications

#### 6. Cascade Delete Rules
**Decision**: Strategic cascade delete configuration based on entity relationships  
**Rationale**:
- Course → Lesson → Task: CASCADE (content hierarchy)
- Category → Course: RESTRICT (prevent accidental data loss)
- User → Progress/Submissions: CASCADE (user data removal)
- User (CreatedBy) → Course: SET NULL (preserve content, remove attribution)

#### 7. Index Strategy
**Decision**: Comprehensive indexing on foreign keys, frequently queried columns, and composite keys  
**Rationale**:
- Sub-second query performance targets (50ms-300ms based on complexity)
- Support for 10K+ users, 1K+ courses at launch
- Efficient JOIN operations
- Unique constraint enforcement

**Index Types**:
- Single column: Foreign keys, email, status fields
- Composite: (UserId, CourseId), (UserId, LessonId) for unique constraints
- Unique: Email, Username, DiscountCode.Code
- GIN: JSONB columns for JSON path queries

#### 8. Time-Based Partitioning (Future)
**Decision**: Document partition strategy but implement post-launch  
**Rationale**:
- UserProgress and RewardTransactions will grow to millions of records
- Monthly partitions based on CreatedAt/CompletedAt
- Implement when data volume justifies complexity (est. 6-12 months)
- PostgreSQL native partitioning simplifies implementation

---

## Constitution Check

### Learning-First ✅
- Schema tracks detailed learning progress (lesson completion, video watch time, progress percentages)
- Focus on educational outcomes, not just gamification metrics
- Progress tracking enables personalized learning paths (future enhancement)

### Security & Privacy ✅
- Password hashing via ASP.NET Identity
- Refresh tokens stored securely with expiration and revocation
- Soft delete supports GDPR right to be forgotten
- Audit fields (CreatedAt, UpdatedAt, ReviewedAt) enable security logging
- No sensitive data in plain text

### Scalability ✅
- Comprehensive indexing for sub-second queries
- Time-based partitioning strategy documented for future growth
- JSONB prevents schema bloat as features expand
- Efficient composite indexes for common query patterns

### Fair Economy ✅
- Immutable reward ledger prevents point manipulation
- Unique constraints prevent duplicate reward claims
- Discount code usage limits prevent abuse
- Transaction history enables fraud detection

### Quality Assurance ✅
- Foreign key constraints prevent orphaned records
- Unique constraints prevent data duplication
- Cascade rules maintain referential integrity
- Migration-based version control

### Accessibility ✅
- Schema supports future accessibility metadata (descriptions, alt text)
- Flexible JSONB allows adding accessibility data without migrations

### Business Ethics ✅
- Transparent pricing through DiscountCodes table
- Clear subscription tracking (tier, expiration)
- Point costs explicitly defined
- No hidden charges or dark patterns in data model

### Technical Excellence ✅
- Clean Architecture with clear domain models
- Repository pattern for data access abstraction
- SOLID principles in entity design
- Comprehensive migration strategy
- Proper constraint enforcement

**Gate Status**: ✅ **PASS** - All constitution principles satisfied

---

## Phase 0: Research & Design Decisions

All research completed and consolidated above in Technical Context section.

### Resolved Clarifications

1. **Soft Delete vs Hard Delete**: Soft delete for Users, Categories, Courses, Lessons; Hard delete acceptable for Progress and Submissions
2. **JSONB Handling**: Use EF Core column type jsonb, store as string in entity, deserialize as needed
3. **Indexing Strategy**: Index all FKs, frequently queried columns, composite unique constraints
4. **Time-Based Partitioning**: Document strategy, implement post-launch based on data volume
5. **Enum Storage**: Strings via value converters for readability and migration safety
6. **Cascade Delete Rules**: Cascade for child entities, Restrict for referenced entities, SetNull for attributions
7. **Text Field Lengths**: Email: 255, Username: 100, Title: 200, Description: 2000, ContentMarkdown: TEXT, URLs: 500

---

## Phase 1: Domain Layer Design

### Data Model

#### Base Entity Pattern
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
```

#### Core Entities

##### 1. User (extends IdentityUser<Guid>)
**Purpose**: Platform user with authentication and subscription data  
**Properties**:
- Inherited from IdentityUser: Id, Email, PasswordHash, EmailConfirmed, etc.
- Additional: Username (string, 100), Role (UserRole enum), SubscriptionTier (SubscriptionTier enum), SubscriptionExpiresAt (DateTime?), CurrentPoints (int), ProfileImageUrl (string?, 500), CreatedAt (DateTime), IsActive (bool)

**Navigation Properties**:
- RefreshTokens: ICollection<RefreshToken>
- CreatedCourses: ICollection<Course>
- Enrollments: ICollection<UserCourseEnrollment>
- Progress: ICollection<UserProgress>
- TaskSubmissions: ICollection<UserTaskSubmission>
- RewardTransactions: ICollection<RewardTransaction>
- DiscountRedemptions: ICollection<UserDiscountRedemption>

**Domain Methods**:
- EnrollInCourse(courseId): Creates enrollment record
- CalculatePointBalance(): SUM of RewardTransactions

##### 2. Category
**Purpose**: Course organization (Airdrops, GameFi, DeFi, NFT, Task-to-Earn)  
**Properties**:
- Id (Guid), Name (string, 100), Description (string, 500), IconUrl (string?, 500), DisplayOrder (int), IsActive (bool), CreatedAt (DateTime)

**Navigation Properties**:
- Courses: ICollection<Course>

**Domain Methods**:
- Activate(), Deactivate()

##### 3. Course
**Purpose**: Educational course with metadata and content  
**Properties**:
- Id (Guid), CategoryId (Guid FK), Title (string, 200), Description (string, 2000), ThumbnailUrl (string?, 500), DifficultyLevel (DifficultyLevel enum), EstimatedDurationHours (int), IsPublished (bool), IsPremium (bool), CategoryId (Guid FK), CreatedById (Guid? FK), ViewCount (int), CreatedAt (DateTime), UpdatedAt (DateTime), IsDeleted (bool), DeletedAt (DateTime?)

**Navigation Properties**:
- Category: Category
- CreatedBy: User?
- Lessons: ICollection<Lesson>
- Enrollments: ICollection<UserCourseEnrollment>

**Domain Methods**:
- Publish(), Unpublish()
- AddLesson(lesson): Adds lesson with auto OrderIndex
- CalculateCompletionPercentage(userId): Based on lesson completion

##### 4. Lesson
**Purpose**: Individual lesson with YouTube video  
**Properties**:
- Id (Guid), CourseId (Guid FK), Title (string, 200), Description (string, 2000), YouTubeVideoId (string, 50), VideoUrl (string, 500), DurationSeconds (int), OrderIndex (int), IsPreview (bool), IsPremium (bool), ContentMarkdown (string?), CreatedAt (DateTime), UpdatedAt (DateTime), IsDeleted (bool), DeletedAt (DateTime?)

**Navigation Properties**:
- Course: Course
- Tasks: ICollection<LearningTask>
- UserProgress: ICollection<UserProgress>

**Domain Methods**:
- AddTask(task): Adds task with auto OrderIndex

##### 5. LearningTask
**Purpose**: Interactive task for skill verification  
**Properties**:
- Id (Guid), LessonId (Guid FK), Title (string, 200), Description (string, 2000), TaskType (TaskType enum), TaskData (string, jsonb), PointsReward (int), TimeLimit (int?, nullable, in seconds), OrderIndex (int), IsRequired (bool), CreatedAt (DateTime)

**Navigation Properties**:
- Lesson: Lesson
- Submissions: ICollection<UserTaskSubmission>

**Domain Methods**:
- GetQuizData(): Deserialize TaskData as QuizTaskData
- GetScreenshotRequirements(): Deserialize TaskData as ScreenshotTaskData

**JSONB Schema Pattern**:
All TaskData and SubmissionData JSONB fields MUST include a version field for schema evolution:
```json
{
  "_version": 1,
  "data": {
    // Task-specific data here
  }
}
```

##### 6. UserProgress
**Purpose**: Lesson-level progress tracking  
**Properties**:
- Id (Guid), UserId (Guid FK), LessonId (Guid FK), CompletedAt (DateTime?), VideoWatchTimeSeconds (int, in seconds), ProgressPercentage (decimal), LastAccessedAt (DateTime), IsCompleted (bool), CreatedAt (DateTime), UpdatedAt (DateTime)

**Navigation Properties**:
- User: User
- Lesson: Lesson

**Unique Constraint**: (UserId, LessonId)

**Domain Methods**:
- MarkComplete(): Sets IsCompleted, CompletedAt
- UpdateProgress(watchTime, percentage): Updates tracking fields

##### 7. UserCourseEnrollment
**Purpose**: Course enrollment tracking  
**Properties**:
- Id (Guid), UserId (Guid FK), CourseId (Guid FK), EnrollmentDate (DateTime), CompletionDate (DateTime?), ProgressPercentage (decimal), LastAccessedAt (DateTime), IsCompleted (bool), CreatedAt (DateTime)

**Navigation Properties**:
- User: User
- Course: Course

**Unique Constraint**: (UserId, CourseId)

**Domain Methods**:
- UpdateProgress(): Recalculates based on lesson completion
- MarkComplete(): Sets IsCompleted, CompletionDate

##### 8. UserTaskSubmission
**Purpose**: Task submission with review status  
**Properties**:
- Id (Guid), UserId (Guid FK), LearningTaskId (Guid FK), SubmissionData (string, jsonb), SubmissionStatus (SubmissionStatus enum), SubmittedAt (DateTime), ReviewedAt (DateTime?), ReviewedById (Guid? FK), ReviewNotes (string?, 1000), CreatedAt (DateTime)

**Navigation Properties**:
- User: User
- LearningTask: LearningTask
- ReviewedBy: User?

**Domain Methods**:
- Approve(reviewerId, notes): Sets status, awards points
- Reject(reviewerId, notes): Sets status, records feedback
- GetQuizAnswers(): Deserialize SubmissionData

##### 9. RewardTransaction
**Purpose**: Immutable ledger of point transactions  
**Properties**:
- Id (Guid), UserId (Guid FK), TransactionType (TransactionType enum), Amount (int), Description (string, 500), RelatedTaskSubmissionId (Guid?), RelatedDiscountRedemptionId (Guid?), TransactionDate (DateTime)

**Navigation Properties**:
- User: User

**Domain Methods**:
- None (append-only, no modifications)

**Static Factory Methods**:
- CreateEarned(userId, amount, taskSubmissionId)
- CreateRedeemed(userId, amount, discountRedemptionId)
- CreateBonus(userId, amount, description)
- CreatePenalty(userId, amount, description)

##### 10. DiscountCode
**Purpose**: Point-redeemable discount codes  
**Properties**:
- Id (Guid), Code (string, 50), DiscountPercentage (int), PointCost (int), IsActive (bool), ExpiresAt (DateTime?), MaxUsageCount (int), CurrentUsageCount (int), CreatedAt (DateTime)

**Navigation Properties**:
- Redemptions: ICollection<UserDiscountRedemption>

**Unique Constraint**: Code

**Domain Methods**:
- CanBeRedeemed(): Checks active, expiration, usage limit
- IncrementUsage(): Increments CurrentUsageCount

##### 11. UserDiscountRedemption
**Purpose**: User redemption of discount code  
**Properties**:
- Id (Guid), UserId (Guid FK), DiscountCodeId (Guid FK), RedeemedAt (DateTime), PointsSpent (int), UsedForSubscription (bool)

**Navigation Properties**:
- User: User
- DiscountCode: DiscountCode

**Domain Methods**:
- MarkUsedForSubscription(): Sets flag

##### 12. RefreshToken
**Purpose**: JWT refresh token storage  
**Properties**:
- Id (Guid), UserId (Guid FK), Token (string, 500), ExpiresAt (DateTime), CreatedAt (DateTime), CreatedByIp (string, 50), RevokedAt (DateTime?), RevokedByIp (string?, 50), ReplacedByToken (string?, 500), ReasonRevoked (string?, 200), IsExpired (computed), IsRevoked (computed), IsActive (computed)

**Navigation Properties**:
- User: User

**Unique Constraint**: Token

**Domain Methods**:
- Revoke(ipAddress, reason, replacedByToken): Marks token as revoked
- IsValid(): Checks expiration and revocation status

#### Enums

```csharp
public enum TaskType
{
    Quiz,
    Screenshot,
    WalletVerification,
    TextSubmission,
    ExternalLink
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

public enum SubscriptionTier
{
    Free,
    Monthly,
    Yearly
}

public enum UserRole
{
    User,
    Premium,
    Admin,
    ContentCreator,
    Moderator
}
```

### API Contracts

#### Endpoints (Repository Interfaces Only - No REST APIs for Database Schema Feature)

Since this is a database schema feature, we define **Repository Interfaces** instead of REST API contracts:

##### IRepository<T> (Generic)
```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

##### ICategoryRepository
```csharp
public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
```

##### ICourseRepository
```csharp
public interface ICourseRepository : IRepository<Course>
{
    Task<IReadOnlyList<Course>> GetPublishedCoursesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Course>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<Course?> GetWithLessonsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Course>> SearchCoursesAsync(string searchTerm, DifficultyLevel? difficulty, bool? isPremium, CancellationToken cancellationToken = default);
}
```

##### ILessonRepository
```csharp
public interface ILessonRepository : IRepository<Lesson>
{
    Task<IReadOnlyList<Lesson>> GetByCourseAsync(Guid courseId, CancellationToken cancellationToken = default);
    Task<Lesson?> GetWithTasksAsync(Guid id, CancellationToken cancellationToken = default);
}
```

##### ILearningTaskRepository
```csharp
public interface ILearningTaskRepository : IRepository<LearningTask>
{
    Task<IReadOnlyList<LearningTask>> GetByLessonAsync(Guid lessonId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LearningTask>> GetByTypeAsync(TaskType taskType, CancellationToken cancellationToken = default);
}
```

##### IUserProgressRepository
```csharp
public interface IUserProgressRepository : IRepository<UserProgress>
{
    Task<UserProgress?> GetByUserAndLessonAsync(Guid userId, Guid lessonId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserProgress>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserProgress>> GetCompletedByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpsertProgressAsync(UserProgress progress, CancellationToken cancellationToken = default);
}
```

##### IUserCourseEnrollmentRepository
```csharp
public interface IUserCourseEnrollmentRepository : IRepository<UserCourseEnrollment>
{
    Task<UserCourseEnrollment?> GetByUserAndCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserCourseEnrollment>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsEnrolledAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default);
}
```

##### IUserTaskSubmissionRepository
```csharp
public interface IUserTaskSubmissionRepository : IRepository<UserTaskSubmission>
{
    Task<IReadOnlyList<UserTaskSubmission>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserTaskSubmission>> GetPendingSubmissionsAsync(CancellationToken cancellationToken = default);
    Task<UserTaskSubmission?> GetLatestSubmissionAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
}
```

##### IRewardTransactionRepository
```csharp
public interface IRewardTransactionRepository : IRepository<RewardTransaction>
{
    Task<IReadOnlyList<RewardTransaction>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> CalculateUserBalanceAsync(Guid userId, CancellationToken cancellationToken = default);
    // Note: No Update or Delete methods - append-only ledger
}
```

##### IDiscountCodeRepository
```csharp
public interface IDiscountCodeRepository : IRepository<DiscountCode>
{
    Task<DiscountCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DiscountCode>> GetActiveCodesAsync(CancellationToken cancellationToken = default);
}
```

##### IUserDiscountRedemptionRepository
```csharp
public interface IUserDiscountRedemptionRepository : IRepository<UserDiscountRedemption>
{
    Task<IReadOnlyList<UserDiscountRedemption>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasRedeemedCodeAsync(Guid userId, Guid discountCodeId, CancellationToken cancellationToken = default);
}
```

##### IRefreshTokenRepository
```csharp
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string token, string ipAddress, string reason, CancellationToken cancellationToken = default);
    Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}
```

##### IUnitOfWork
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
    IUserDiscountRedemptionRepository DiscountRedemptions { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

---

## Phase 2: Implementation Plan

### Milestone 1: Domain Layer Setup (Priority: P1, Est: 6 hours)

**Goal**: Establish domain entities with business logic

#### Task 1.1: Create Enum Definitions (1 hour)
**Deliverables**:
- `WahadiniCryptoQuest.Core/Enums/TaskType.cs`
- `WahadiniCryptoQuest.Core/Enums/SubmissionStatus.cs`
- `WahadiniCryptoQuest.Core/Enums/TransactionType.cs`
- `WahadiniCryptoQuest.Core/Enums/DifficultyLevel.cs`
- `WahadiniCryptoQuest.Core/Enums/SubscriptionTier.cs`
- `WahadiniCryptoQuest.Core/Enums/UserRole.cs`

**Success Criteria**:
- All 6 enums defined with correct values
- XML documentation comments for each enum and value
- No dependencies on infrastructure

#### Task 1.2: Create Base Entity (1 hour)
**Deliverables**:
- `WahadiniCryptoQuest.Core/Entities/BaseEntity.cs` (abstract class with Id, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)

**Success Criteria**:
- Base entity contains all common properties
- Protected setters for encapsulation
- Domain events support (optional)

#### Task 1.3: Create Core Entities (4 hours)
**Deliverables**:
- `WahadiniCryptoQuest.Core/Entities/User.cs` (extends IdentityUser<Guid>)
- `WahadiniCryptoQuest.Core/Entities/Category.cs`
- `WahadiniCryptoQuest.Core/Entities/Course.cs`
- `WahadiniCryptoQuest.Core/Entities/Lesson.cs`
- `WahadiniCryptoQuest.Core/Entities/LearningTask.cs`
- `WahadiniCryptoQuest.Core/Entities/UserProgress.cs`
- `WahadiniCryptoQuest.Core/Entities/UserCourseEnrollment.cs`
- `WahadiniCryptoQuest.Core/Entities/UserTaskSubmission.cs`
- `WahadiniCryptoQuest.Core/Entities/RewardTransaction.cs`
- `WahadiniCryptoQuest.Core/Entities/DiscountCode.cs`
- `WahadiniCryptoQuest.Core/Entities/UserDiscountRedemption.cs`
- `WahadiniCryptoQuest.Core/Entities/RefreshToken.cs`

**Success Criteria**:
- All entities have proper properties with correct types and max lengths
- Navigation properties defined for all relationships
- Domain methods implemented (factory methods, state transitions)
- Private setters for encapsulation
- No infrastructure dependencies

---

### Milestone 2: Infrastructure Layer - Data Access (Priority: P1, Est: 8 hours)

**Goal**: Configure EF Core DbContext with Fluent API and migrations

#### Task 2.1: Create ApplicationDbContext (3 hours)
**Deliverables**:
- `WahadiniCryptoQuest.DAL/Context/ApplicationDbContext.cs`
  - Inherits from IdentityDbContext<User, IdentityRole<Guid>, Guid>
  - All DbSets defined
  - OnModelCreating with Fluent API for all entities

**Success Criteria**:
- All relationships configured with correct cascade rules
- All indexes defined (single column, composite, unique)
- Enum value converters configured (string storage)
- JSONB column types configured
- Query filters for soft delete configured
- Timestamp defaults configured (CreatedAt = DateTime.UtcNow)

#### Task 2.2: Create Entity Configurations (3 hours)
**Deliverables**:
- `WahadiniCryptoQuest.DAL/Configurations/UserConfiguration.cs`
- `WahadiniCryptoQuest.DAL/Configurations/CategoryConfiguration.cs`
- `WahadiniCryptoQuest.DAL/Configurations/CourseConfiguration.cs`
- `WahadiniCryptoQuest.DAL/Configurations/LessonConfiguration.cs`
- `WahadiniCryptoQuest.DAL/Configurations/LearningTaskConfiguration.cs`
- `WahadiniCryptoQuest.DAL/Configurations/UserProgressConfiguration.cs`
- `WahadiniCryptoQuest.DAL/Configurations/UserCourseEnrollmentConfiguration.cs`
- `WahadiniCryptoQuest.DAL/Configurations/UserTaskSubmissionConfiguration.cs`
- `WahadiniCryptoQuest.DAL/Configurations/RewardTransactionConfiguration.cs`
- `WahadiniCryptoQuest.DAL/Configurations/DiscountCodeConfiguration.cs`
- `WahadiniCryptoQuest.DAL/Configurations/UserDiscountRedemptionConfiguration.cs`
- `WahadiniCryptoQuest.DAL/Configurations/RefreshTokenConfiguration.cs`

**Success Criteria**:
- Each entity configuration in separate file implementing IEntityTypeConfiguration<T>
- Applied via modelBuilder.ApplyConfiguration() in ApplicationDbContext
- Clean separation of concerns

#### Task 2.3: Create Initial Migration (2 hours)
**Deliverables**:
- EF Core migration files in `WahadiniCryptoQuest.DAL/Migrations/`
- Migration generated via: `dotnet ef migrations add InitialSchema`

**Success Criteria**:
- Migration creates all 12+ tables
- All foreign keys created correctly
- All indexes created correctly
- All constraints created correctly
- Migration can be applied to fresh database
- Migration can be rolled back cleanly

---

### Milestone 3: Repository Interfaces (Priority: P1, Est: 4 hours)

**Goal**: Define repository contracts in Core layer

#### Task 3.1: Create Generic Repository Interface (1 hour)
**Deliverables**:
- `WahadiniCryptoQuest.Core/Interfaces/Repositories/IRepository.cs`

**Success Criteria**:
- Generic CRUD operations defined
- All methods async
- CancellationToken support

#### Task 3.2: Create Specific Repository Interfaces (3 hours)
**Deliverables**:
- All 11 specific repository interfaces (listed in API Contracts section)
- `WahadiniCryptoQuest.Core/Interfaces/Repositories/IUnitOfWork.cs`

**Success Criteria**:
- Each repository interface extends IRepository<T>
- Specialized query methods defined for each repository
- IUnitOfWork aggregates all repositories
- Transaction support in IUnitOfWork

---

### Milestone 4: Seed Data (Priority: P2, Est: 3 hours)

**Goal**: Implement database initialization with seed data

#### Task 4.1: Create Seed Data Service (3 hours)
**Deliverables**:
- `WahadiniCryptoQuest.DAL/Seeders/DefaultDataSeeder.cs`
  - SeedCategoriesAsync(): 5 categories (Airdrops, GameFi, Task-to-Earn, DeFi, NFT Strategies)
  - SeedAdminUserAsync(): Admin user with secure password
  - SeedDiscountCodesAsync(): 3 codes (SAVE10, SAVE20, SAVE30)
- Program.cs integration to call seeder on startup

**Success Criteria**:
- All seed methods idempotent (check existence before insert)
- Seed data matches specification exactly
- Executed automatically on application startup
- No duplicate records created on re-run

---

### Milestone 5: Testing & Documentation (Priority: P2, Est: 4 hours)

**Goal**: Validate schema and document design

#### Task 5.1: Integration Testing (2 hours)
**Deliverables**:
- Test migration application
- Test entity creation for all types
- Test relationship navigation
- Test cascade deletes
- Test unique constraints
- Test JSONB storage/retrieval
- Test query filters for soft delete

**Success Criteria**:
- All tests pass
- No SQL errors
- Relationships load correctly
- Constraints enforce correctly

#### Task 5.2: Documentation (2 hours)
**Deliverables**:
- `specs/002-database-schema/DATABASE_SCHEMA_README.md` (ERD, entity descriptions, relationships, indexes, migration guide)
- Code XML comments on all entities and methods

**Success Criteria**:
- ERD diagram included
- All entities documented
- All relationships explained
- Index rationale provided
- Migration commands documented

---

## Phase 3: Future Enhancements (Post-Launch)

### Time-Based Partitioning
**When**: After 6-12 months or when UserProgress > 1M records
**Implementation**:
- Create partitioned tables for UserProgress and RewardTransactions
- Monthly partitions based on CreatedAt/CompletedAt
- Automated partition creation (3 months in advance)
- Partition pruning in queries
- Historical data archival strategy

### Advanced Indexing
**When**: Performance issues identified
**Options**:
- Partial indexes for frequently filtered subsets
- BRIN indexes for large sequential data
- Covering indexes for index-only scans
- Full-text search indexes for course/lesson search

### Materialized Views
**When**: Complex reporting queries become slow
**Options**:
- User leaderboard view
- Course statistics view
- Category analytics view
- Refresh strategy (manual, scheduled, trigger-based)

### Concurrency Control Strategy
**Point Balance Race Conditions**:
To prevent race conditions during concurrent point redemptions, implement the following strategy:

```csharp
// Use SERIALIZABLE transaction isolation for redemption operations
using var transaction = await _context.Database.BeginTransactionAsync(
    IsolationLevel.Serializable);
try
{
    // 1. Calculate current balance within transaction scope
    var currentBalance = await _context.RewardTransactions
        .Where(rt => rt.UserId == userId)
        .SumAsync(rt => rt.Amount);
    
    // 2. Validate sufficient balance
    if (currentBalance < pointCost)
    {
        throw new InsufficientPointsException();
    }
    
    // 3. Create redemption record
    var redemption = new UserDiscountRedemption { ... };
    await _context.UserDiscountRedemptions.AddAsync(redemption);
    
    // 4. Create reward transaction (negative amount)
    var transaction = new RewardTransaction
    {
        UserId = userId,
        Amount = -pointCost,
        TransactionType = TransactionType.Redeemed,
        RelatedDiscountRedemptionId = redemption.Id
    };
    await _context.RewardTransactions.AddAsync(transaction);
    
    // 5. Update discount code usage count
    discountCode.CurrentRedemptions++;
    
    // 6. Commit all changes atomically
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

**DeletedAt Use Case Documentation**:
The DeletedAt field serves multiple purposes:
- **Audit Trail**: Timestamp records when entity was soft-deleted for compliance and investigation
- **Restoration Window**: Enables "undo" functionality - admins can restore recently deleted items within configured timeframe (e.g., 30 days)
- **Data Retention**: Supports legal data retention requirements while marking data as inactive
- **Cascade Analysis**: Helps identify when related entities were orphaned for debugging cascade delete issues

---

## Risk Assessment & Mitigation

### Risk 1: Migration Failures in Production
**Likelihood**: Medium  
**Impact**: High  
**Mitigation**:
- Test migrations on production-like data volumes
- Create backup before migration
- Implement rollback procedures
- Use blue-green deployment for zero-downtime

### Risk 2: JSONB Schema Evolution
**Likelihood**: High  
**Impact**: Low  
**Mitigation**:
- Version JSONB schemas (`{"version": 1, "data": {...}}`)
- Implement schema migration utility
- Handle legacy schema formats gracefully
- Document schema changes

### Risk 3: Index Bloat
**Likelihood**: Medium  
**Impact**: Medium  
**Mitigation**:
- Monitor index usage with pg_stat_user_indexes
- Remove unused indexes
- Regular REINDEX on fragmented indexes
- Balance index count with write performance

### Risk 4: Soft Delete Query Filter Bypass
**Likelihood**: Low  
**Impact**: Medium  
**Mitigation**:
- Test query filters thoroughly
- Provide explicit IgnoreQueryFilters() for admin queries
- Code review for query filter bypasses
- Integration tests for soft delete behavior

---

## Dependencies

### External
- PostgreSQL 15+ (JSONB, native partitioning)
- Npgsql.EntityFrameworkCore.PostgreSQL 8.0+
- Microsoft.EntityFrameworkCore.Tools (CLI)
- Microsoft.AspNetCore.Identity.EntityFrameworkCore

### Internal
- None (foundational feature)

---

## Deployment Checklist

### Development Environment
- [ ] PostgreSQL 15+ installed locally
- [ ] EF Core CLI tools installed globally
- [ ] Connection string configured in appsettings.Development.json
- [ ] Initial migration created
- [ ] Migration applied to local database
- [ ] Seed data executed and verified

### Staging Environment
- [ ] PostgreSQL database provisioned
- [ ] Connection string configured
- [ ] Migrations applied
- [ ] Seed data executed
- [ ] Indexes verified
- [ ] Performance testing completed

### Production Environment
- [ ] Database backup created
- [ ] Maintenance window scheduled
- [ ] Migration script reviewed
- [ ] Rollback plan documented
- [ ] Migrations applied
- [ ] Seed data executed
- [ ] Smoke tests passed
- [ ] Monitoring configured

---

## Success Metrics

### Performance
- [ ] User login query < 50ms (target from spec.md §SC-003)
- [ ] Course search query < 200ms (target from spec.md §SC-004)
- [ ] User progress query < 150ms (target from spec.md §SC-005)
- [ ] Task submission query < 300ms (target from spec.md §SC-006)

### Functional
- [ ] All migrations execute successfully < 30 seconds (target from spec.md §SC-001)
- [ ] All foreign keys enforce referential integrity (target from spec.md §SC-002)
- [ ] Seed data creates exactly 5 categories, 1 admin, 3 codes (target from spec.md §SC-008)
- [ ] JSONB columns store/retrieve complex JSON without corruption (target from spec.md §SC-009)
- [ ] Soft delete excludes deleted records from queries (target from spec.md §SC-010)
- [ ] Cascade deletes remove associated records (target from spec.md §SC-011)

### Quality
- [ ] Zero schema-related bugs in first month
- [ ] 100% migration success rate across environments
- [ ] All indexes show > 95% hit ratio (pg_stat_user_indexes)

---

## Timeline Summary

| Milestone | Duration | Priority | Dependencies |
|-----------|----------|----------|--------------|
| 1. Domain Layer | 6 hours | P1 | None |
| 2. Infrastructure Layer | 8 hours | P1 | Milestone 1 |
| 3. Repository Interfaces | 4 hours | P1 | Milestone 1 |
| 4. Seed Data | 3 hours | P2 | Milestone 2 |
| 5. Testing & Documentation | 4 hours | P2 | Milestones 1-4 |
| **Total** | **25 hours** | | |

**Recommended Sprint**: 1 week (5 working days) with buffer for code review and testing

---

## Next Steps

1. **Immediate**: Review and approve this implementation plan
2. **Phase 0**: All research already completed ✅
3. **Phase 1**: Begin Milestone 1 (Domain Layer Setup)
4. **Code Review**: After each milestone completion
5. **Testing**: Comprehensive integration testing after Milestone 2
6. **Documentation**: Update as implementation progresses
7. **Deployment**: Follow deployment checklist for each environment

---

**Plan Status**: ✅ **READY FOR IMPLEMENTATION**  
**Approved By**: Pending  
**Start Date**: TBD  
**Target Completion**: TBD (Est. 25 hours over 1 week)
