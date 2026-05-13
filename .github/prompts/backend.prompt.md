# WahadiniCryptoQuest Platform - Backend Development Prompt

## Context
You are an expert .NET Core developer working on the WahadiniCryptoQuest Platform backend. This is a comprehensive crypto learning platform with gamified task-to-earn features, video-based education, and reward systems. The application follows clean architecture principles, implements comprehensive authentication and authorization, and provides a robust RESTful API for the crypto education platform.

## Backend Architecture Overview

### Technology Stack
- **.NET 8.0** Web API
- **Entity Framework Core 8.0** for data access
- **PostgreSQL** as primary database with time-based partitioning for user activity data
- **AutoMapper** for object mapping
- **FluentValidation** for input validation
- **MediatR** for CQRS and messaging
- **JWT Bearer Tokens** for authentication
- **ASP.NET Identity** for user management
- **Stripe SDK** for payment processing
- **Swagger/OpenAPI** for API documentation
- **Docker** for containerization

### Project Structure
```
backend/
└── src/
    ├── WahadiniCryptoQuest.API/                            # Presentation Layer
    │   ├── Controllers/
    │   │   ├── AuthController.cs
    │   │   ├── UserController.cs
    │   │   ├── CourseController.cs
    │   │   ├── LessonController.cs
    │   │   ├── TaskController.cs
    │   │   ├── RewardController.cs
    │   │   ├── SubscriptionController.cs
    │   │   └── AdminController.cs
    │   │
    │   ├── Middleware/
    │   │   ├── ErrorHandlingMiddleware.cs
    │   │   ├── RequestLoggingMiddleware.cs
    │   │   └── JwtMiddleware.cs
    │   │
    │   ├── Filters/
    │   │   ├── ExceptionFilter.cs
    │   │   ├── ValidationFilter.cs
    │   │   └── AuthorizationFilter.cs
    │   │
    │   ├── Validators/
    │   │   ├── Auth/
    │   │   │   ├── LoginRequestValidator.cs
    │   │   │   └── RegisterRequestValidator.cs
    │   │   ├── Course/
    │   │   │   ├── CreateCourseValidator.cs
    │   │   │   └── UpdateCourseValidator.cs
    │   │   └── Task/
    │   │       ├── SubmitTaskValidator.cs
    │   │       └── TaskUpdateValidator.cs
    │   │
    │   ├── Extensions/
    │   │   ├── DependencyInjection/
    │   │   │   ├── CoreExtensions.cs
    │   │   │   ├── ServiceExtensions.cs
    │   │   │   ├── InfrastructureExtensions.cs
    │   │   │   ├── AuthExtensions.cs
    │   │   │   ├── SwaggerExtensions.cs
    │   │   │   ├── CorsExtensions.cs
    │   │   │   └── MapperExtensions.cs
    │   │   ├── ApplicationBuilderExtensions/
    │   │   │   ├── MiddlewareExtensions.cs
    │   │   │   ├── EndpointExtensions.cs
    │   │   │   └── DatabaseMigrationExtensions.cs
    │   │   └── StartupExtensions.cs
    │   │
    │   └── Program.cs
    │
    ├── WahadiniCryptoQuest.Core/                           # Domain Layer
    │   ├── Entities/
    │   │   ├── User.cs
    │   │   ├── Course.cs
    │   │   ├── Lesson.cs
    │   │   ├── Task.cs
    │   │   ├── RewardTransaction.cs
    │   │   ├── Subscription.cs
    │   │   └── Category.cs
    │   │
    │   ├── DTOs/
    │   │   ├── Auth/
    │   │   │   ├── LoginRequestDto.cs
    │   │   │   ├── RegisterRequestDto.cs
    │   │   │   └── AuthResponseDto.cs
    │   │   ├── User/
    │   │   │   ├── UserDto.cs
    │   │   │   └── UserProfileDto.cs
    │   │   ├── Course/
    │   │   │   ├── CourseDto.cs
    │   │   │   └── CourseDetailDto.cs
    │   │   ├── Task/
    │   │   │   ├── TaskDto.cs
    │   │   │   └── TaskSubmissionDto.cs
    │   │   ├── Reward/
    │   │   │   └── RewardDto.cs
    │   │   └── Subscription/
    │   │       └── SubscriptionDto.cs
    │   │
    │   ├── Interfaces/
    │   │   ├── Repositories/
    │   │   │   ├── IRepository.cs
    │   │   │   ├── IUserRepository.cs
    │   │   │   ├── ICourseRepository.cs
    │   │   │   └── IUnitOfWork.cs
    │   │   ├── Services/
    │   │   │   ├── IAuthService.cs
    │   │   │   ├── IUserService.cs
    │   │   │   ├── ICourseService.cs
    │   │   │   ├── ILessonService.cs
    │   │   │   ├── ITaskService.cs
    │   │   │   ├── IRewardService.cs
    │   │   │   └── ISubscriptionService.cs
    │   │   └── External/
    │   │       ├── IEmailService.cs
    │   │       ├── IPaymentGateway.cs
    │   │       └── IVideoService.cs
    │   │
    │   ├── Enums/
    │   │   ├── TaskType.cs
    │   │   ├── TaskStatus.cs
    │   │   ├── UserRole.cs
    │   │   └── SubscriptionTier.cs
    │   │
    │   ├── Specifications/
    │   │   ├── CourseSpecification.cs
    │   │   ├── UserSpecification.cs
    │   │   └── RewardSpecification.cs
    │   │
    │   └── ValueObjects/
    │       ├── Email.cs
    │       ├── Money.cs
    │       └── YouTubeUrl.cs
    │
    ├── WahadiniCryptoQuest.Service/                       # Application Layer (Use Cases)
    │   ├── Auth/
    │   │   ├── AuthService.cs
    │   │   ├── AuthCommandHandler.cs
    │   │   └── AuthQueryHandler.cs
    │   ├── Course/
    │   │   ├── CourseService.cs
    │   │   ├── CreateCourseCommand.cs
    │   │   └── GetCoursesQuery.cs
    │   ├── Task/
    │   │   ├── TaskService.cs
    │   │   ├── SubmitTaskCommand.cs
    │   │   └── GetTasksQuery.cs
    │   ├── Reward/
    │   │   ├── RewardService.cs
    │   │   └── RedeemRewardCommand.cs
    │   ├── Subscription/
    │   │   ├── SubscriptionService.cs
    │   │   └── GetSubscriptionPlansQuery.cs
    │   ├── User/
    │   │   ├── UserService.cs
    │   │   ├── GetUserProfileQuery.cs
    │   │   └── UpdateUserProfileCommand.cs
    │   │
    │   ├── Mappings/
    │   │   ├── AuthMappingProfile.cs
    │   │   ├── CourseMappingProfile.cs
    │   │   ├── UserMappingProfile.cs
    │   │   └── GlobalMappingProfile.cs
    │   │
    │   └── Behaviors/
    │       ├── ValidationBehavior.cs
    │       └── LoggingBehavior.cs
    │
    ├── WahadiniCryptoQuest.DAL/                          # Infrastructure Layer
    │   ├── Context/
    │   │   ├── ApplicationDbContext.cs
    │   │   └── DbInitializer.cs
    │   │
    │   ├── Repositories/
    │   │   ├── Repository.cs
    │   │   ├── UserRepository.cs
    │   │   ├── CourseRepository.cs
    │   │   ├── LessonRepository.cs
    │   │   ├── RewardRepository.cs
    │   │   └── UnitOfWork.cs
    │   │
    │   ├── Configurations/
    │   │   ├── UserConfiguration.cs
    │   │   ├── CourseConfiguration.cs
    │   │   ├── LessonConfiguration.cs
    │   │   └── SubscriptionConfiguration.cs
    │   │
    │   ├── Identity/
    │   │   ├── JwtTokenService.cs
    │   │   └── IdentityConfiguration.cs
    │   │
    │   ├── Services/
    │   │   ├── StripePaymentService.cs
    │   │   ├── EmailService.cs
    │   │   └── YouTubeService.cs
    │   │
    │   ├── Seeders/
    │   │   └── DefaultDataSeeder.cs
    │   │
    │   └── Migrations/
    │       ├── 2025xxxx_InitialCreate.cs
    │       └── ApplicationDbContextModelSnapshot.cs
    │
    └── tests/                                             # Automated Testing Layer
        ├── WahadiniCryptoQuest.API.Tests/                 # Controller + Endpoint + Middleware tests
        │   ├── Controllers/
        │   │   ├── AuthControllerTests.cs
        │   │   ├── CourseControllerTests.cs
        │   │   ├── TaskControllerTests.cs
        │   │   └── RewardControllerTests.cs
        │   │
        │   ├── Middleware/
        │   │   ├── ErrorHandlingMiddlewareTests.cs
        │   │   └── JwtMiddlewareTests.cs
        │   │
        │   ├── Filters/
        │   │   └── ValidationFilterTests.cs
        │   │
        │   ├── Integration/
        │   │   ├── AuthEndpointsIntegrationTests.cs
        │   │   ├── CourseEndpointsIntegrationTests.cs
        │   │   └── TaskEndpointsIntegrationTests.cs
        │   │
        │   └── Helpers/
        │       ├── CustomWebApplicationFactory.cs         # For in-memory WebApp testing
        │       └── HttpClientExtensions.cs
        │
        ├── WahadiniCryptoQuest.Service.Tests/             # Application Layer Unit Tests
        │   ├── Auth/
        │   │   ├── AuthServiceTests.cs
        │   │   ├── LoginCommandHandlerTests.cs
        │   │   └── RegisterCommandHandlerTests.cs
        │   ├── Course/
        │   │   ├── CourseServiceTests.cs
        │   │   └── CourseMappingTests.cs
        │   ├── Task/
        │   │   ├── TaskServiceTests.cs
        │   │   └── SubmitTaskCommandTests.cs
        │   ├── Reward/
        │   │   ├── RewardServiceTests.cs
        │   │   └── RedeemRewardCommandTests.cs
        │   ├── Subscription/
        │   │   ├── SubscriptionServiceTests.cs
        │   │   └── SubscriptionQueryHandlerTests.cs
        │   └── Shared/
        │       ├── ValidationBehaviorTests.cs
        │       └── LoggingBehaviorTests.cs
        │
        ├── WahadiniCryptoQuest.DAL.Tests/                 # Repository + EF Integration tests
        │   ├── Repositories/
        │   │   ├── UserRepositoryTests.cs
        │   │   ├── CourseRepositoryTests.cs
        │   │   ├── RewardRepositoryTests.cs
        │   │   └── UnitOfWorkTests.cs
        │   ├── Context/
        │   │   ├── ApplicationDbContextTests.cs
        │   │   └── DbInitializerTests.cs
        │   ├── Migrations/
        │   │   └── MigrationSnapshotTests.cs
        │   └── Helpers/
        │       ├── InMemoryDbContextFactory.cs
        │       └── TestDatabaseSeeder.cs
        │
        └── WahadiniCryptoQuest.Core.Tests/                # Domain Layer (Pure Logic) tests
            ├── Entities/
            │   ├── UserEntityTests.cs
            │   ├── CourseEntityTests.cs
            │   ├── TaskEntityTests.cs
            │   └── RewardTransactionTests.cs
            │
            ├── ValueObjects/
            │   ├── EmailTests.cs
            │   ├── MoneyTests.cs
            │   └── YouTubeUrlTests.cs
            │
            ├── Specifications/
            │   ├── CourseSpecificationTests.cs
            │   └── UserSpecificationTests.cs
            │
            ├── Exceptions/
            │   └── DomainExceptionTests.cs
            │
            └── DTOs/
                ├── AuthDtoValidationTests.cs
                └── CourseDtoMappingTests.cs

```

## Core Patterns and Guidelines

### 1. Clean Architecture Implementation

#### Domain Layer (Core)
```csharp
// Domain Entity with Business Logic
public class Course : BaseEntity
{
    public Guid CategoryId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string ThumbnailUrl { get; private set; }
    public DifficultyLevel Difficulty { get; private set; }
    public bool IsPremium { get; private set; }
    public decimal Price { get; private set; }
    public int EstimatedDuration { get; private set; } // in minutes
    public bool IsPublished { get; private set; }
    public int ViewCount { get; private set; }
    public int RewardPoints { get; private set; }
    
    // Navigation properties
    public virtual Category Category { get; private set; }
    public virtual ICollection<Lesson> Lessons { get; private set; } = new List<Lesson>();
    public virtual ICollection<UserCourseEnrollment> Enrollments { get; private set; } = new List<UserCourseEnrollment>();

    // Private constructor for EF
    private Course() { }

    // Factory method for creation
    public static Course Create(
        Guid categoryId,
        string title,
        string description,
        string thumbnailUrl,
        DifficultyLevel difficulty,
        bool isPremium,
        decimal price = 0,
        int estimatedDuration = 0,
        int rewardPoints = 0)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Course title is required");
        
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Course description is required");

        if (isPremium && price <= 0)
            throw new DomainException("Premium courses must have a price greater than 0");

        if (rewardPoints < 0)
            throw new DomainException("Reward points cannot be negative");

        return new Course
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            Title = title.Trim(),
            Description = description.Trim(),
            ThumbnailUrl = thumbnailUrl,
            Difficulty = difficulty,
            IsPremium = isPremium,
            Price = price,
            EstimatedDuration = estimatedDuration,
            RewardPoints = rewardPoints,
            IsPublished = false,
            ViewCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    // Business methods
    public void UpdateDetails(string title, string description, string thumbnailUrl)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Course title is required");
        
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Course description is required");
        
        Title = title.Trim();
        Description = description.Trim();
        ThumbnailUrl = thumbnailUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        if (IsPublished)
            throw new DomainException("Course is already published");
        
        if (!Lessons.Any())
            throw new DomainException("Cannot publish course without lessons");
        
        IsPublished = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unpublish()
    {
        if (!IsPublished)
            throw new DomainException("Course is not published");
        
        IsPublished = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;
    }
}

// Lesson Entity
public class Lesson : BaseEntity
{
    public Guid CourseId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string VideoUrl { get; private set; }
    public int Duration { get; private set; } // in seconds
    public int OrderIndex { get; private set; }
    public string? ContentMarkdown { get; private set; }
    public int RewardPoints { get; private set; }
    public bool IsPreview { get; private set; }
    
    // Navigation properties
    public virtual Course Course { get; private set; }
    public virtual ICollection<Task> Tasks { get; private set; } = new List<Task>();
    public virtual ICollection<UserProgress> UserProgresses { get; private set; } = new List<UserProgress>();

    private Lesson() { }

    public static Lesson Create(
        Guid courseId,
        string title,
        string description,
        string videoUrl,
        int duration,
        int orderIndex,
        int rewardPoints = 0,
        string? contentMarkdown = null,
        bool isPreview = false)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Lesson title is required");
        
        if (string.IsNullOrWhiteSpace(videoUrl))
            throw new DomainException("Lesson video URL is required");

        if (duration <= 0)
            throw new DomainException("Lesson duration must be greater than 0");

        return new Lesson
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = title.Trim(),
            Description = description.Trim(),
            VideoUrl = videoUrl,
            Duration = duration,
            OrderIndex = orderIndex,
            ContentMarkdown = contentMarkdown,
            RewardPoints = rewardPoints,
            IsPreview = isPreview,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateContent(string title, string description, string? contentMarkdown)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Lesson title is required");
        
        Title = title.Trim();
        Description = description.Trim();
        ContentMarkdown = contentMarkdown;
        UpdatedAt = DateTime.UtcNow;
    }
}

// Base Entity
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }
    public string CreatedBy { get; protected set; } = "System";
    public string UpdatedBy { get; protected set; } = "System";
    public bool IsDeleted { get; protected set; }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

#### Repository Interfaces
```csharp
// Generic repository interface
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}

// Course repository interface
public interface ICourseRepository : IRepository<Course>
{
    Task<IEnumerable<Course>> GetByCategoryIdAsync(
        Guid categoryId, 
        int page = 1, 
        int pageSize = 50,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Course>> GetPublishedCoursesAsync(
        bool isPremium = false,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Course>> GetUserEnrolledCoursesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    
    Task<Course?> GetCourseWithLessonsAsync(
        Guid courseId,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Course>> SearchCoursesAsync(
        string searchTerm,
        Guid? categoryId = null,
        DifficultyLevel? difficulty = null,
        bool? isPremium = null,
        CancellationToken cancellationToken = default);
    
    Task<decimal> GetCourseCompletionRateAsync(
        Guid courseId,
        CancellationToken cancellationToken = default);
}

// Lesson repository interface
public interface ILessonRepository : IRepository<Lesson>
{
    Task<IEnumerable<Lesson>> GetByCourseIdAsync(
        Guid courseId,
        CancellationToken cancellationToken = default);
    
    Task<Lesson?> GetLessonWithTasksAsync(
        Guid lessonId,
        CancellationToken cancellationToken = default);
    
    Task<Lesson?> GetNextLessonAsync(
        Guid currentLessonId,
        CancellationToken cancellationToken = default);
    
    Task<Lesson?> GetPreviousLessonAsync(
        Guid currentLessonId,
        CancellationToken cancellationToken = default);
    
    Task<bool> IsLessonAccessibleToUserAsync(
        Guid lessonId,
        Guid userId,
        CancellationToken cancellationToken = default);
}

// Task repository interface
public interface ITaskRepository : IRepository<Task>
{
    Task<IEnumerable<Task>> GetByLessonIdAsync(
        Guid lessonId,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Task>> GetPendingTaskSubmissionsAsync(
        TaskType? taskType = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Task>> GetUserCompletedTasksAsync(
        Guid userId,
        Guid? lessonId = null,
        CancellationToken cancellationToken = default);
    
    Task<UserTaskSubmission?> GetUserTaskSubmissionAsync(
        Guid userId,
        Guid taskId,
        CancellationToken cancellationToken = default);
}

// User Progress repository interface
public interface IUserProgressRepository : IRepository<UserProgress>
{
    Task<UserProgress?> GetUserLessonProgressAsync(
        Guid userId,
        Guid lessonId,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<UserProgress>> GetUserCourseProgressAsync(
        Guid userId,
        Guid courseId,
        CancellationToken cancellationToken = default);
    
    Task<decimal> GetCourseCompletionPercentageAsync(
        Guid userId,
        Guid courseId,
        CancellationToken cancellationToken = default);
    
    Task<int> GetUserTotalRewardPointsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
```

### 2. Application Layer (Service)

#### Service Implementation
```csharp
public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authService;
    private readonly ILogger<CourseService> _logger;

    public CourseService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAuthorizationService authService,
        ILogger<CourseService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _authService = authService;
        _logger = logger;
    }

    public async Task<Result<CourseResponseDto>> CreateAsync(
        CreateCourseRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Authorization check
            if (!await _authService.HasPermissionAsync(currentUserId, Permission.CreateCourses))
            {
                return Result<CourseResponseDto>.Failure("Insufficient permissions", "PERMISSION_DENIED");
            }

            // Validate category exists
            var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                return Result<CourseResponseDto>.Failure("Category not found", "CATEGORY_NOT_FOUND");
            }

            // Create domain entity
            var course = Course.Create(
                request.CategoryId,
                request.Title,
                request.Description,
                request.ThumbnailUrl,
                request.Difficulty,
                request.IsPremium,
                request.Price,
                request.EstimatedDuration,
                request.RewardPoints);

            // Save to repository
            var createdCourse = await _unitOfWork.Courses.CreateAsync(course, cancellationToken);
            
            // Commit changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to response DTO
            var responseDto = _mapper.Map<CourseResponseDto>(createdCourse);
            
            _logger.LogInformation("Course created successfully by user {UserId}", currentUserId);
            return Result<CourseResponseDto>.Success(responseDto);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Domain validation failed: {Message}", ex.Message);
            return Result<CourseResponseDto>.Failure(ex.Message, "DOMAIN_VALIDATION_ERROR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course for user {UserId}", currentUserId);
            return Result<CourseResponseDto>.Failure("An error occurred while creating the course", "INTERNAL_ERROR");
        }
    }

    public async Task<Result<IEnumerable<CourseResponseDto>>> GetCoursesAsync(
        CourseQueryRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get user roles to determine access level
            var isAdmin = await _authService.HasRoleAsync(currentUserId, UserRole.Admin);
            var isPremium = await _authService.HasRoleAsync(currentUserId, UserRole.Premium);

            // Get courses with filtering
            var courses = await _unitOfWork.Courses.SearchCoursesAsync(
                request.SearchTerm,
                request.CategoryId,
                request.Difficulty,
                request.IsPremium,
                cancellationToken);

            // Filter based on user access level
            if (!isAdmin && !isPremium)
            {
                courses = courses.Where(c => !c.IsPremium || c.IsPreview);
            }

            // Apply pagination
            courses = courses
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);

            // Map to response DTOs
            var responseDtos = _mapper.Map<IEnumerable<CourseResponseDto>>(courses);
            
            return Result<IEnumerable<CourseResponseDto>>.Success(responseDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving courses for user {UserId}", currentUserId);
            return Result<IEnumerable<CourseResponseDto>>.Failure("An error occurred while retrieving courses", "INTERNAL_ERROR");
        }
    }

    public async Task<Result<CourseDetailResponseDto>> GetCourseDetailAsync(
        Guid courseId,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get course with lessons
            var course = await _unitOfWork.Courses.GetCourseWithLessonsAsync(courseId, cancellationToken);
            if (course == null)
            {
                return Result<CourseDetailResponseDto>.Failure("Course not found", "COURSE_NOT_FOUND");
            }

            // Check access permissions
            var hasAccess = await ValidateUserCourseAccessAsync(currentUserId, course, cancellationToken);
            if (!hasAccess)
            {
                return Result<CourseDetailResponseDto>.Failure("Access denied to premium content", "ACCESS_DENIED");
            }

            // Get user's progress for this course
            var userProgress = await _unitOfWork.UserProgress.GetUserCourseProgressAsync(
                currentUserId, courseId, cancellationToken);

            // Calculate completion percentage
            var completionPercentage = await _unitOfWork.UserProgress.GetCourseCompletionPercentageAsync(
                currentUserId, courseId, cancellationToken);

            // Increment view count
            course.IncrementViewCount();
            await _unitOfWork.Courses.UpdateAsync(course, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to detailed response DTO
            var responseDto = _mapper.Map<CourseDetailResponseDto>(course);
            responseDto.CompletionPercentage = completionPercentage;
            responseDto.UserProgress = _mapper.Map<IEnumerable<UserProgressDto>>(userProgress);
            
            return Result<CourseDetailResponseDto>.Success(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving course detail {CourseId} for user {UserId}", courseId, currentUserId);
            return Result<CourseDetailResponseDto>.Failure("An error occurred while retrieving course details", "INTERNAL_ERROR");
        }
    }

    public async Task<Result<bool>> EnrollUserAsync(
        Guid courseId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if course exists
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId, cancellationToken);
            if (course == null)
            {
                return Result<bool>.Failure("Course not found", "COURSE_NOT_FOUND");
            }

            // Check if user is already enrolled
            var existingEnrollment = await _unitOfWork.UserCourseEnrollments
                .GetUserEnrollmentAsync(userId, courseId, cancellationToken);
            
            if (existingEnrollment != null)
            {
                return Result<bool>.Failure("User already enrolled in this course", "ALREADY_ENROLLED");
            }

            // Validate access (premium course requires premium subscription)
            if (course.IsPremium)
            {
                var isPremium = await _authService.HasRoleAsync(userId, UserRole.Premium);
                if (!isPremium)
                {
                    return Result<bool>.Failure("Premium subscription required", "PREMIUM_REQUIRED");
                }
            }

            // Create enrollment
            var enrollment = UserCourseEnrollment.Create(userId, courseId);
            await _unitOfWork.UserCourseEnrollments.CreateAsync(enrollment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} enrolled in course {CourseId}", userId, courseId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enrolling user {UserId} in course {CourseId}", userId, courseId);
            return Result<bool>.Failure("An error occurred while enrolling in the course", "INTERNAL_ERROR");
        }
    }

    private async Task<bool> ValidateUserCourseAccessAsync(
        Guid userId,
        Course course,
        CancellationToken cancellationToken)
    {
        // Admin always has access
        if (await _authService.HasRoleAsync(userId, UserRole.Admin))
            return true;

        // Free courses are accessible to everyone
        if (!course.IsPremium)
            return true;

        // Premium courses require premium subscription
        return await _authService.HasRoleAsync(userId, UserRole.Premium);
    }
}
```

#### CQRS Implementation
```csharp
// Command for creating course
public class CreateCourseCommand : IRequest<Result<CourseResponseDto>>
{
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public DifficultyLevel Difficulty { get; set; }
    public bool IsPremium { get; set; }
    public decimal Price { get; set; }
    public int EstimatedDuration { get; set; }
    public int RewardPoints { get; set; }
}

// Command handler
public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Result<CourseResponseDto>>
{
    private readonly ICourseService _courseService;
    private readonly ICurrentUserService _currentUserService;

    public CreateCourseCommandHandler(
        ICourseService courseService,
        ICurrentUserService currentUserService)
    {
        _courseService = courseService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CourseResponseDto>> Handle(
        CreateCourseCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var createRequest = new CreateCourseRequest
        {
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            Difficulty = request.Difficulty,
            IsPremium = request.IsPremium,
            Price = request.Price,
            EstimatedDuration = request.EstimatedDuration,
            RewardPoints = request.RewardPoints
        };

        return await _courseService.CreateAsync(createRequest, currentUserId, cancellationToken);
    }
}

// Query for getting courses
public class GetCoursesQuery : IRequest<Result<IEnumerable<CourseResponseDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? CategoryId { get; set; }
    public DifficultyLevel? Difficulty { get; set; }
    public bool? IsPremium { get; set; }
    public string? SearchTerm { get; set; }
}

// Query handler
public class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, Result<IEnumerable<CourseResponseDto>>>
{
    private readonly ICourseService _courseService;
    private readonly ICurrentUserService _currentUserService;

    public GetCoursesQueryHandler(
        ICourseService courseService,
        ICurrentUserService currentUserService)
    {
        _courseService = courseService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IEnumerable<CourseResponseDto>>> Handle(
        GetCoursesQuery request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var queryRequest = new CourseQueryRequest
        {
            Page = request.Page,
            PageSize = request.PageSize,
            CategoryId = request.CategoryId,
            Difficulty = request.Difficulty,
            IsPremium = request.IsPremium,
            SearchTerm = request.SearchTerm
        };

        return await _courseService.GetCoursesAsync(queryRequest, currentUserId, cancellationToken);
    }
}

// Command for enrolling in course
public class EnrollInCourseCommand : IRequest<Result<bool>>
{
    public Guid CourseId { get; set; }
}

// Command handler for enrollment
public class EnrollInCourseCommandHandler : IRequestHandler<EnrollInCourseCommand, Result<bool>>
{
    private readonly ICourseService _courseService;
    private readonly ICurrentUserService _currentUserService;

    public EnrollInCourseCommandHandler(
        ICourseService courseService,
        ICurrentUserService currentUserService)
    {
        _courseService = courseService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(
        EnrollInCourseCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        return await _courseService.EnrollUserAsync(request.CourseId, currentUserId, cancellationToken);
    }
}

// Query for getting lesson details
public class GetLessonDetailQuery : IRequest<Result<LessonDetailResponseDto>>
{
    public Guid LessonId { get; set; }
}

// Query handler for lesson details
public class GetLessonDetailQueryHandler : IRequestHandler<GetLessonDetailQuery, Result<LessonDetailResponseDto>>
{
    private readonly ILessonService _lessonService;
    private readonly ICurrentUserService _currentUserService;

    public GetLessonDetailQueryHandler(
        ILessonService lessonService,
        ICurrentUserService currentUserService)
    {
        _lessonService = lessonService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<LessonDetailResponseDto>> Handle(
        GetLessonDetailQuery request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        return await _lessonService.GetLessonDetailAsync(request.LessonId, currentUserId, cancellationToken);
    }
}

// Command for submitting task
public class SubmitTaskCommand : IRequest<Result<TaskSubmissionResponseDto>>
{
    public Guid TaskId { get; set; }
    public string SubmissionData { get; set; } = string.Empty;
    public TaskType TaskType { get; set; }
}

// Command handler for task submission
public class SubmitTaskCommandHandler : IRequestHandler<SubmitTaskCommand, Result<TaskSubmissionResponseDto>>
{
    private readonly ITaskService _taskService;
    private readonly ICurrentUserService _currentUserService;

    public SubmitTaskCommandHandler(
        ITaskService taskService,
        ICurrentUserService currentUserService)
    {
        _taskService = taskService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TaskSubmissionResponseDto>> Handle(
        SubmitTaskCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var submissionRequest = new TaskSubmissionRequest
        {
            TaskId = request.TaskId,
            SubmissionData = request.SubmissionData,
            TaskType = request.TaskType
        };

        return await _taskService.SubmitTaskAsync(submissionRequest, currentUserId, cancellationToken);
    }
}
```

### 3. Presentation Layer (API)

#### Controller Implementation
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(IMediator mediator, ILogger<CoursesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get courses with pagination and filtering
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CourseResponseDto>>), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CourseResponseDto>>>> GetCourses(
        [FromQuery] GetCoursesQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _mediator.Send(query, cancellationToken);
            return HandleApiResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Action}", nameof(GetCourses));
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "An unexpected error occurred",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Create a new course (Admin only)
    /// </summary>
    [HttpPost]
    [RequirePermission(Permission.CreateCourses)]
    [ProducesResponseType(typeof(ApiResponse<CourseResponseDto>), 201)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 401)]
    [ProducesResponseType(typeof(ErrorResponseDto), 403)]
    public async Task<ActionResult<ApiResponse<CourseResponseDto>>> CreateCourse(
        CreateCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new CreateCourseCommand
            {
                CategoryId = request.CategoryId,
                Title = request.Title,
                Description = request.Description,
                ThumbnailUrl = request.ThumbnailUrl,
                Difficulty = request.Difficulty,
                IsPremium = request.IsPremium,
                Price = request.Price,
                EstimatedDuration = request.EstimatedDuration,
                RewardPoints = request.RewardPoints
            };

            var result = await _mediator.Send(command, cancellationToken);
            
            if (result.IsSuccess)
            {
                return CreatedAtAction(
                    nameof(GetCourse),
                    new { id = result.Data!.Id },
                    ApiResponse<CourseResponseDto>.SuccessResult(result.Data, "Course created successfully"));
            }

            return HandleApiResponse(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ErrorResponseDto
            {
                Message = ex.Message,
                ErrorCode = "VALIDATION_ERROR",
                Details = ex.Errors?.ToDictionary(e => e.PropertyName, e => e.ErrorMessage)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Action}", nameof(CreateCourse));
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "An unexpected error occurred",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Get course by ID with details
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CourseDetailResponseDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<ActionResult<ApiResponse<CourseDetailResponseDto>>> GetCourse(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetCourseDetailQuery { CourseId = id };
            var result = await _mediator.Send(query, cancellationToken);
            return HandleApiResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Action} for ID {Id}", nameof(GetCourse), id);
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "An unexpected error occurred",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Enroll in a course
    /// </summary>
    [HttpPost("{id:guid}/enroll")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<ActionResult<ApiResponse<object>>> EnrollInCourse(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new EnrollInCourseCommand { CourseId = id };
            var result = await _mediator.Send(command, cancellationToken);
            
            if (result.IsSuccess)
            {
                return Ok(ApiResponse<object>.SuccessResult(null, "Successfully enrolled in course"));
            }

            return HandleApiResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Action} for course ID {Id}", nameof(EnrollInCourse), id);
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "An unexpected error occurred",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Update course (Admin only)
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permission.UpdateCourses)]
    [ProducesResponseType(typeof(ApiResponse<CourseResponseDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<ActionResult<ApiResponse<CourseResponseDto>>> UpdateCourse(
        Guid id,
        UpdateCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new UpdateCourseCommand
            {
                Id = id,
                Title = request.Title,
                Description = request.Description,
                ThumbnailUrl = request.ThumbnailUrl,
                Difficulty = request.Difficulty,
                IsPremium = request.IsPremium,
                Price = request.Price,
                EstimatedDuration = request.EstimatedDuration,
                RewardPoints = request.RewardPoints
            };

            var result = await _mediator.Send(command, cancellationToken);
            return HandleApiResponse(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ErrorResponseDto
            {
                Message = ex.Message,
                ErrorCode = "VALIDATION_ERROR",
                Details = ex.Errors?.ToDictionary(e => e.PropertyName, e => e.ErrorMessage)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Action} for ID {Id}", nameof(UpdateCourse), id);
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "An unexpected error occurred",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Publish/Unpublish course (Admin only)
    /// </summary>
    [HttpPatch("{id:guid}/publish")]
    [RequirePermission(Permission.PublishCourses)]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<ActionResult<ApiResponse<object>>> TogglePublishCourse(
        Guid id,
        [FromBody] TogglePublishRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new TogglePublishCourseCommand 
            { 
                CourseId = id, 
                IsPublished = request.IsPublished 
            };
            
            var result = await _mediator.Send(command, cancellationToken);
            
            if (result.IsSuccess)
            {
                var message = request.IsPublished ? "Course published successfully" : "Course unpublished successfully";
                return Ok(ApiResponse<object>.SuccessResult(null, message));
            }

            return HandleApiResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Action} for ID {Id}", nameof(TogglePublishCourse), id);
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "An unexpected error occurred",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    // Helper method for consistent API responses
    private ActionResult<ApiResponse<T>> HandleApiResponse<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(ApiResponse<T>.SuccessResult(result.Data, result.Message));
        }

        return result.ErrorCode switch
        {
            "NOT_FOUND" or "COURSE_NOT_FOUND" => NotFound(new ErrorResponseDto { Message = result.ErrorMessage, ErrorCode = result.ErrorCode }),
            "PERMISSION_DENIED" or "ACCESS_DENIED" => Forbid(),
            "VALIDATION_ERROR" => BadRequest(new ErrorResponseDto { Message = result.ErrorMessage, ErrorCode = result.ErrorCode }),
            "PREMIUM_REQUIRED" => StatusCode(402, new ErrorResponseDto { Message = result.ErrorMessage, ErrorCode = result.ErrorCode }),
            "ALREADY_ENROLLED" => Conflict(new ErrorResponseDto { Message = result.ErrorMessage, ErrorCode = result.ErrorCode }),
            _ => StatusCode(500, new ErrorResponseDto { Message = result.ErrorMessage, ErrorCode = result.ErrorCode })
        };
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LessonsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LessonsController> _logger;

    public LessonsController(IMediator mediator, ILogger<LessonsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get lesson details with video and tasks
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<LessonDetailResponseDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    [ProducesResponseType(typeof(ErrorResponseDto), 403)]
    public async Task<ActionResult<ApiResponse<LessonDetailResponseDto>>> GetLesson(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetLessonDetailQuery { LessonId = id };
            var result = await _mediator.Send(query, cancellationToken);
            return HandleApiResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Action} for ID {Id}", nameof(GetLesson), id);
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "An unexpected error occurred",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Update lesson progress
    /// </summary>
    [HttpPost("{id:guid}/progress")]
    [ProducesResponseType(typeof(ApiResponse<UserProgressResponseDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<ActionResult<ApiResponse<UserProgressResponseDto>>> UpdateProgress(
        Guid id,
        UpdateProgressRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new UpdateLessonProgressCommand
            {
                LessonId = id,
                WatchedDuration = request.WatchedDuration,
                TotalDuration = request.TotalDuration,
                IsCompleted = request.IsCompleted
            };

            var result = await _mediator.Send(command, cancellationToken);
            return HandleApiResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Action} for lesson ID {Id}", nameof(UpdateProgress), id);
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "An unexpected error occurred",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    private ActionResult<ApiResponse<T>> HandleApiResponse<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(ApiResponse<T>.SuccessResult(result.Data, result.Message));
        }

        return result.ErrorCode switch
        {
            "NOT_FOUND" or "LESSON_NOT_FOUND" => NotFound(new ErrorResponseDto { Message = result.ErrorMessage, ErrorCode = result.ErrorCode }),
            "ACCESS_DENIED" => Forbid(),
            "VALIDATION_ERROR" => BadRequest(new ErrorResponseDto { Message = result.ErrorMessage, ErrorCode = result.ErrorCode }),
            _ => StatusCode(500, new ErrorResponseDto { Message = result.ErrorMessage, ErrorCode = result.ErrorCode })
        };
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IMediator mediator, ILogger<TasksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Submit a task
    /// </summary>
    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(ApiResponse<TaskSubmissionResponseDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<ActionResult<ApiResponse<TaskSubmissionResponseDto>>> SubmitTask(
        Guid id,
        TaskSubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new SubmitTaskCommand
            {
                TaskId = id,
                SubmissionData = request.SubmissionData,
                TaskType = request.TaskType
            };

            var result = await _mediator.Send(command, cancellationToken);
            return HandleApiResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Action} for task ID {Id}", nameof(SubmitTask), id);
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "An unexpected error occurred",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Get pending task submissions for admin review
    /// </summary>
    [HttpGet("submissions/pending")]
    [RequirePermission(Permission.ReviewTasks)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TaskSubmissionReviewDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskSubmissionReviewDto>>>> GetPendingSubmissions(
        [FromQuery] GetPendingSubmissionsQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _mediator.Send(query, cancellationToken);
            return HandleApiResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Action}", nameof(GetPendingSubmissions));
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "An unexpected error occurred",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Review task submission (Admin only)
    /// </summary>
    [HttpPost("submissions/{submissionId:guid}/review")]
    [RequirePermission(Permission.ReviewTasks)]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<ActionResult<ApiResponse<object>>> ReviewTaskSubmission(
        Guid submissionId,
        ReviewTaskSubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new ReviewTaskSubmissionCommand
            {
                SubmissionId = submissionId,
                IsApproved = request.IsApproved,
                FeedbackText = request.FeedbackText,
                RewardPointsAwarded = request.RewardPointsAwarded
            };

            var result = await _mediator.Send(command, cancellationToken);
            
            if (result.IsSuccess)
            {
                var message = request.IsApproved ? "Task submission approved" : "Task submission rejected";
                return Ok(ApiResponse<object>.SuccessResult(null, message));
            }

            return HandleApiResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Action} for submission ID {Id}", nameof(ReviewTaskSubmission), submissionId);
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "An unexpected error occurred",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    private ActionResult<ApiResponse<T>> HandleApiResponse<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(ApiResponse<T>.SuccessResult(result.Data, result.Message));
        }

        return result.ErrorCode switch
        {
            "NOT_FOUND" or "TASK_NOT_FOUND" => NotFound(new ErrorResponseDto { Message = result.ErrorMessage, ErrorCode = result.ErrorCode }),
            "PERMISSION_DENIED" => Forbid(),
            "VALIDATION_ERROR" => BadRequest(new ErrorResponseDto { Message = result.ErrorMessage, ErrorCode = result.ErrorCode }),
            "ALREADY_SUBMITTED" => Conflict(new ErrorResponseDto { Message = result.ErrorMessage, ErrorCode = result.ErrorCode }),
            _ => StatusCode(500, new ErrorResponseDto { Message = result.ErrorMessage, ErrorCode = result.ErrorCode })
        };
    }
}
```

### 4. Data Access Layer (Infrastructure)

#### Repository Implementation
```csharp
public class CourseRepository : Repository<Course>, ICourseRepository
{
    public CourseRepository(WahadiniCryptoQuestDbContext context) : base(context) { }

    public async Task<IEnumerable<Course>> GetByCategoryIdAsync(
        Guid categoryId,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        return await Context.Courses
            .Where(c => c.CategoryId == categoryId && !c.IsDeleted && c.IsPublished)
            .Include(c => c.Category)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Course>> GetPublishedCoursesAsync(
        bool isPremium = false,
        CancellationToken cancellationToken = default)
    {
        return await Context.Courses
            .Where(c => c.IsPublished && !c.IsDeleted && c.IsPremium == isPremium)
            .Include(c => c.Category)
            .OrderByDescending(c => c.ViewCount)
            .ThenByDescending(c => c.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Course>> GetUserEnrolledCoursesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await Context.UserCourseEnrollments
            .Where(e => e.UserId == userId && !e.IsDeleted)
            .Select(e => e.Course)
            .Include(c => c.Category)
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Course?> GetCourseWithLessonsAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Courses
            .Where(c => c.Id == courseId && !c.IsDeleted)
            .Include(c => c.Category)
            .Include(c => c.Lessons.Where(l => !l.IsDeleted).OrderBy(l => l.OrderIndex))
                .ThenInclude(l => l.Tasks.Where(t => !t.IsDeleted).OrderBy(t => t.OrderIndex))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Course>> SearchCoursesAsync(
        string searchTerm,
        Guid? categoryId = null,
        DifficultyLevel? difficulty = null,
        bool? isPremium = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Courses
            .Where(c => c.IsPublished && !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => 
                c.Title.Contains(searchTerm) || 
                c.Description.Contains(searchTerm) ||
                c.Category.Name.Contains(searchTerm));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(c => c.CategoryId == categoryId.Value);
        }

        if (difficulty.HasValue)
        {
            query = query.Where(c => c.Difficulty == difficulty.Value);
        }

        if (isPremium.HasValue)
        {
            query = query.Where(c => c.IsPremium == isPremium.Value);
        }

        return await query
            .Include(c => c.Category)
            .OrderByDescending(c => c.ViewCount)
            .ThenByDescending(c => c.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetCourseCompletionRateAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var totalEnrollments = await Context.UserCourseEnrollments
            .CountAsync(e => e.CourseId == courseId && !e.IsDeleted, cancellationToken);

        if (totalEnrollments == 0)
            return 0;

        var completedEnrollments = await Context.UserCourseEnrollments
            .CountAsync(e => e.CourseId == courseId && !e.IsDeleted && e.IsCompleted, cancellationToken);

        return Math.Round((decimal)completedEnrollments / totalEnrollments * 100, 2);
    }
}

public class LessonRepository : Repository<Lesson>, ILessonRepository
{
    public LessonRepository(WahadiniCryptoQuestDbContext context) : base(context) { }

    public async Task<IEnumerable<Lesson>> GetByCourseIdAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Lessons
            .Where(l => l.CourseId == courseId && !l.IsDeleted)
            .Include(l => l.Course)
            .OrderBy(l => l.OrderIndex)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Lesson?> GetLessonWithTasksAsync(
        Guid lessonId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Lessons
            .Where(l => l.Id == lessonId && !l.IsDeleted)
            .Include(l => l.Course)
                .ThenInclude(c => c.Category)
            .Include(l => l.Tasks.Where(t => !t.IsDeleted).OrderBy(t => t.OrderIndex))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Lesson?> GetNextLessonAsync(
        Guid currentLessonId,
        CancellationToken cancellationToken = default)
    {
        var currentLesson = await Context.Lessons
            .Where(l => l.Id == currentLessonId && !l.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentLesson == null)
            return null;

        return await Context.Lessons
            .Where(l => l.CourseId == currentLesson.CourseId && 
                       !l.IsDeleted && 
                       l.OrderIndex > currentLesson.OrderIndex)
            .OrderBy(l => l.OrderIndex)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Lesson?> GetPreviousLessonAsync(
        Guid currentLessonId,
        CancellationToken cancellationToken = default)
    {
        var currentLesson = await Context.Lessons
            .Where(l => l.Id == currentLessonId && !l.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentLesson == null)
            return null;

        return await Context.Lessons
            .Where(l => l.CourseId == currentLesson.CourseId && 
                       !l.IsDeleted && 
                       l.OrderIndex < currentLesson.OrderIndex)
            .OrderByDescending(l => l.OrderIndex)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsLessonAccessibleToUserAsync(
        Guid lessonId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var lesson = await Context.Lessons
            .Where(l => l.Id == lessonId && !l.IsDeleted)
            .Include(l => l.Course)
            .FirstOrDefaultAsync(cancellationToken);

        if (lesson == null)
            return false;

        // Check if lesson is a preview lesson
        if (lesson.IsPreview)
            return true;

        // Check if user is enrolled in the course
        var isEnrolled = await Context.UserCourseEnrollments
            .AnyAsync(e => e.UserId == userId && 
                          e.CourseId == lesson.CourseId && 
                          !e.IsDeleted, cancellationToken);

        if (!isEnrolled)
            return false;

        // Check if course is premium and user has premium access
        if (lesson.Course.IsPremium)
        {
            var userRoles = await Context.UserRoleAssignments
                .Where(ura => ura.UserId == userId && ura.IsActive && !ura.IsDeleted)
                .Include(ura => ura.Role)
                .Select(ura => ura.Role.RoleType)
                .ToListAsync(cancellationToken);

            return userRoles.Contains(UserRole.Premium) || userRoles.Contains(UserRole.Admin);
        }

        return true;
    }
}

public class TaskRepository : Repository<Task>, ITaskRepository
{
    public TaskRepository(WahadiniCryptoQuestDbContext context) : base(context) { }

    public async Task<IEnumerable<Task>> GetByLessonIdAsync(
        Guid lessonId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Tasks
            .Where(t => t.LessonId == lessonId && !t.IsDeleted)
            .Include(t => t.Lesson)
            .OrderBy(t => t.OrderIndex)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Task>> GetPendingTaskSubmissionsAsync(
        TaskType? taskType = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = Context.UserTaskSubmissions
            .Where(uts => uts.Status == TaskSubmissionStatus.Pending && !uts.IsDeleted)
            .Select(uts => uts.Task);

        if (taskType.HasValue)
        {
            query = query.Where(t => t.TaskType == taskType.Value);
        }

        return await query
            .Include(t => t.Lesson)
                .ThenInclude(l => l.Course)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Task>> GetUserCompletedTasksAsync(
        Guid userId,
        Guid? lessonId = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.UserTaskSubmissions
            .Where(uts => uts.UserId == userId && 
                         uts.Status == TaskSubmissionStatus.Approved && 
                         !uts.IsDeleted)
            .Select(uts => uts.Task);

        if (lessonId.HasValue)
        {
            query = query.Where(t => t.LessonId == lessonId.Value);
        }

        return await query
            .Include(t => t.Lesson)
            .OrderBy(t => t.Lesson.OrderIndex)
            .ThenBy(t => t.OrderIndex)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<UserTaskSubmission?> GetUserTaskSubmissionAsync(
        Guid userId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        return await Context.UserTaskSubmissions
            .Where(uts => uts.UserId == userId && 
                         uts.TaskId == taskId && 
                         !uts.IsDeleted)
            .Include(uts => uts.Task)
            .OrderByDescending(uts => uts.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

// Generic Repository Base Class
public abstract class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly WahadiniCryptoQuestDbContext Context;
    protected readonly DbSet<T> DbSet;

    protected Repository(WahadiniCryptoQuestDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(e => !e.IsDeleted).ToListAsync(cancellationToken);
    }

    public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var entry = await DbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        Context.Entry(entity).State = EntityState.Modified;
        return entity;
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            entity.SoftDelete();
        }
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
    }
}
```

#### Entity Configuration
```csharp
public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("courses");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(c => c.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(c => c.ThumbnailUrl)
            .HasColumnName("thumbnail_url")
            .HasMaxLength(500);

        builder.Property(c => c.Difficulty)
            .HasColumnName("difficulty")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.IsPremium)
            .HasColumnName("is_premium")
            .HasDefaultValue(false);

        builder.Property(c => c.Price)
            .HasColumnName("price")
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0);

        builder.Property(c => c.EstimatedDuration)
            .HasColumnName("estimated_duration")
            .HasDefaultValue(0);

        builder.Property(c => c.IsPublished)
            .HasColumnName("is_published")
            .HasDefaultValue(false);

        builder.Property(c => c.ViewCount)
            .HasColumnName("view_count")
            .HasDefaultValue(0);

        builder.Property(c => c.RewardPoints)
            .HasColumnName("reward_points")
            .HasDefaultValue(0);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(c => c.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(c => c.Category)
            .WithMany()
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Lessons)
            .WithOne(l => l.Course)
            .HasForeignKey(l => l.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Enrollments)
            .WithOne(e => e.Course)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => c.CategoryId);
        builder.HasIndex(c => c.IsPublished);
        builder.HasIndex(c => c.IsPremium);
        builder.HasIndex(c => new { c.CategoryId, c.IsPublished });
        builder.HasIndex(c => new { c.IsPublished, c.IsPremium });

        // Query filters
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("lessons");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(l => l.CourseId)
            .HasColumnName("course_id")
            .IsRequired();

        builder.Property(l => l.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(l => l.Description)
            .HasColumnName("description")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(l => l.VideoUrl)
            .HasColumnName("video_url")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(l => l.Duration)
            .HasColumnName("duration")
            .IsRequired();

        builder.Property(l => l.OrderIndex)
            .HasColumnName("order_index")
            .IsRequired();

        builder.Property(l => l.ContentMarkdown)
            .HasColumnName("content_markdown")
            .HasColumnType("text");

        builder.Property(l => l.RewardPoints)
            .HasColumnName("reward_points")
            .HasDefaultValue(0);

        builder.Property(l => l.IsPreview)
            .HasColumnName("is_preview")
            .HasDefaultValue(false);

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(l => l.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(l => l.Course)
            .WithMany(c => c.Lessons)
            .HasForeignKey(l => l.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Tasks)
            .WithOne(t => t.Lesson)
            .HasForeignKey(t => t.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.UserProgresses)
            .WithOne(up => up.Lesson)
            .HasForeignKey(up => up.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(l => l.CourseId);
        builder.HasIndex(l => new { l.CourseId, l.OrderIndex });
        builder.HasIndex(l => l.IsPreview);

        // Query filters
        builder.HasQueryFilter(l => !l.IsDeleted);
    }
}

public class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    public void Configure(EntityTypeBuilder<Task> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(t => t.LessonId)
            .HasColumnName("lesson_id")
            .IsRequired();

        builder.Property(t => t.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(t => t.TaskType)
            .HasColumnName("task_type")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.TaskData)
            .HasColumnName("task_data")
            .HasColumnType("jsonb");

        builder.Property(t => t.RewardPoints)
            .HasColumnName("reward_points")
            .HasDefaultValue(0);

        builder.Property(t => t.OrderIndex)
            .HasColumnName("order_index")
            .IsRequired();

        builder.Property(t => t.IsRequired)
            .HasColumnName("is_required")
            .HasDefaultValue(true);

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(t => t.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(t => t.Lesson)
            .WithMany(l => l.Tasks)
            .HasForeignKey(t => t.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Submissions)
            .WithOne(uts => uts.Task)
            .HasForeignKey(uts => uts.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(t => t.LessonId);
        builder.HasIndex(t => new { t.LessonId, t.OrderIndex });
        builder.HasIndex(t => t.TaskType);

        // Query filters
        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}

public class UserProgressConfiguration : IEntityTypeConfiguration<UserProgress>
{
    public void Configure(EntityTypeBuilder<UserProgress> builder)
    {
        builder.ToTable("user_progress");

        builder.HasKey(up => up.Id);

        builder.Property(up => up.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(up => up.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(up => up.LessonId)
            .HasColumnName("lesson_id")
            .IsRequired();

        builder.Property(up => up.WatchedDuration)
            .HasColumnName("watched_duration")
            .HasDefaultValue(0);

        builder.Property(up => up.IsCompleted)
            .HasColumnName("is_completed")
            .HasDefaultValue(false);

        builder.Property(up => up.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(up => up.RewardPointsClaimed)
            .HasColumnName("reward_points_claimed")
            .HasDefaultValue(false);

        builder.Property(up => up.LastWatchedPosition)
            .HasColumnName("last_watched_position")
            .HasDefaultValue(0);

        builder.Property(up => up.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(up => up.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(up => up.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(up => up.Lesson)
            .WithMany(l => l.UserProgresses)
            .HasForeignKey(up => up.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(up => up.UserId);
        builder.HasIndex(up => up.LessonId);
        builder.HasIndex(up => new { up.UserId, up.LessonId }).IsUnique();
        builder.HasIndex(up => new { up.UserId, up.IsCompleted });

        // Query filters
        builder.HasQueryFilter(up => !up.IsDeleted);
    }
}
```

### 5. Authentication and Authorization

#### JWT Service Implementation
```csharp
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IOptions<JwtSettings> jwtSettings, ILogger<JwtTokenService> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public string GenerateAccessToken(User user, IEnumerable<Role> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new("security_stamp", user.SecurityStamp)
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
            
            // Add permission claims
            foreach (var permission in role.Permissions)
            {
                claims.Add(new Claim("permission", permission.Permission.ToString()));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(Guid userId)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow
        };
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // Don't validate lifetime for expired tokens
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.FromMinutes(_jwtSettings.ClockSkewMinutes)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
            
            if (validatedToken is not JwtSecurityToken jwtToken || 
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
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

#### Authorization Service
```csharp
public class AuthorizationService : IAuthorizationService
{
    private readonly IUserRoleAssignmentRepository _userRoleRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuthorizationService> _logger;

    public AuthorizationService(
        IUserRoleAssignmentRepository userRoleRepository,
        IRolePermissionRepository rolePermissionRepository,
        IMemoryCache cache,
        ILogger<AuthorizationService> logger)
    {
        _userRoleRepository = userRoleRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, Permission permission)
    {
        var cacheKey = $"user_permissions_{userId}";
        
        if (!_cache.TryGetValue(cacheKey, out HashSet<Permission> userPermissions))
        {
            userPermissions = await GetUserPermissionsAsync(userId);
            _cache.Set(cacheKey, userPermissions, TimeSpan.FromMinutes(10));
        }

        return userPermissions.Contains(permission);
    }

    public async Task<bool> HasRoleAsync(Guid userId, UserRole role)
    {
        var userRoles = await _userRoleRepository.GetActiveRolesByUserIdAsync(userId);
        return userRoles.Any(r => r.Role.RoleType == role);
    }

    public async Task<bool> HasAnyRoleAsync(Guid userId, params UserRole[] roles)
    {
        var userRoles = await _userRoleRepository.GetActiveRolesByUserIdAsync(userId);
        var userRoleTypes = userRoles.Select(r => r.Role.RoleType).ToHashSet();
        return roles.Any(role => userRoleTypes.Contains(role));
    }

    public async Task<bool> IsResourceOwnerAsync(Guid userId, string resourceType, Guid resourceId)
    {
        return resourceType.ToLower() switch
        {
            "transaction" => await IsTransactionOwnerAsync(userId, resourceId),
            "account" => await IsAccountOwnerAsync(userId, resourceId),
            "budget" => await IsBudgetOwnerAsync(userId, resourceId),
            _ => false
        };
    }

    private async Task<HashSet<Permission>> GetUserPermissionsAsync(Guid userId)
    {
        var userRoles = await _userRoleRepository.GetActiveRolesByUserIdAsync(userId);
        var permissions = new HashSet<Permission>();

        foreach (var userRole in userRoles)
        {
            var rolePermissions = await _rolePermissionRepository.GetPermissionsByRoleIdAsync(userRole.RoleId);
            foreach (var rolePermission in rolePermissions.Where(rp => rp.IsActive))
            {
                permissions.Add(rolePermission.Permission);
            }
        }

        return permissions;
    }

    private async Task<bool> IsTransactionOwnerAsync(Guid userId, Guid transactionId)
    {
        // Implementation would check if the transaction belongs to the user
        // This is a simplified version
        return true; // Implement actual logic
    }

    private async Task<bool> IsAccountOwnerAsync(Guid userId, Guid accountId)
    {
        // Implementation would check if the account belongs to the user
        return true; // Implement actual logic
    }

    private async Task<bool> IsBudgetOwnerAsync(Guid userId, Guid budgetId)
    {
        // Implementation would check if the budget belongs to the user
        return true; // Implement actual logic
    }
}
```

### 6. Validation

#### FluentValidation Implementation
```csharp
public class CreateCourseRequestValidator : AbstractValidator<CreateCourseRequest>
{
    public CreateCourseRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Category ID is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Course title is required")
            .MaximumLength(200)
            .WithMessage("Course title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Course description is required")
            .MaximumLength(2000)
            .WithMessage("Course description cannot exceed 2000 characters");

        RuleFor(x => x.ThumbnailUrl)
            .MaximumLength(500)
            .WithMessage("Thumbnail URL cannot exceed 500 characters")
            .Must(BeAValidUrl)
            .When(x => !string.IsNullOrEmpty(x.ThumbnailUrl))
            .WithMessage("Thumbnail URL must be a valid URL");

        RuleFor(x => x.Difficulty)
            .IsInEnum()
            .WithMessage("Invalid difficulty level");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price cannot be negative")
            .LessThanOrEqualTo(10000)
            .WithMessage("Price cannot exceed $10,000");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .When(x => x.IsPremium)
            .WithMessage("Premium courses must have a price greater than 0");

        RuleFor(x => x.EstimatedDuration)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Estimated duration cannot be negative")
            .LessThanOrEqualTo(1440) // 24 hours in minutes
            .WithMessage("Estimated duration cannot exceed 24 hours");

        RuleFor(x => x.RewardPoints)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Reward points cannot be negative")
            .LessThanOrEqualTo(10000)
            .WithMessage("Reward points cannot exceed 10,000");
    }

    private static bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

public class CreateLessonRequestValidator : AbstractValidator<CreateLessonRequest>
{
    public CreateLessonRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty()
            .WithMessage("Course ID is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Lesson title is required")
            .MaximumLength(200)
            .WithMessage("Lesson title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Lesson description is required")
            .MaximumLength(1000)
            .WithMessage("Lesson description cannot exceed 1000 characters");

        RuleFor(x => x.VideoUrl)
            .NotEmpty()
            .WithMessage("Video URL is required")
            .MaximumLength(500)
            .WithMessage("Video URL cannot exceed 500 characters")
            .Must(BeAValidYouTubeUrl)
            .WithMessage("Video URL must be a valid YouTube URL");

        RuleFor(x => x.Duration)
            .GreaterThan(0)
            .WithMessage("Lesson duration must be greater than 0 seconds")
            .LessThanOrEqualTo(86400) // 24 hours in seconds
            .WithMessage("Lesson duration cannot exceed 24 hours");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Order index cannot be negative");

        RuleFor(x => x.ContentMarkdown)
            .MaximumLength(10000)
            .WithMessage("Content markdown cannot exceed 10,000 characters");

        RuleFor(x => x.RewardPoints)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Reward points cannot be negative")
            .LessThanOrEqualTo(1000)
            .WithMessage("Reward points cannot exceed 1,000 per lesson");
    }

    private static bool BeAValidYouTubeUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        // Accept various YouTube URL formats
        var youtubePatterns = new[]
        {
            @"^https?://(?:www\.)?youtube\.com/watch\?v=[\w-]+",
            @"^https?://(?:www\.)?youtube\.com/embed/[\w-]+",
            @"^https?://youtu\.be/[\w-]+",
            @"^https?://(?:www\.)?youtube\.com/v/[\w-]+"
        };

        return youtubePatterns.Any(pattern => System.Text.RegularExpressions.Regex.IsMatch(url, pattern));
    }
}

public class TaskSubmissionRequestValidator : AbstractValidator<TaskSubmissionRequest>
{
    public TaskSubmissionRequestValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("Task ID is required");

        RuleFor(x => x.TaskType)
            .IsInEnum()
            .WithMessage("Invalid task type");

        RuleFor(x => x.SubmissionData)
            .NotEmpty()
            .WithMessage("Submission data is required")
            .MaximumLength(10000)
            .WithMessage("Submission data cannot exceed 10,000 characters");

        // Validate based on task type
        When(x => x.TaskType == TaskType.Quiz, () =>
        {
            RuleFor(x => x.SubmissionData)
                .Must(BeValidJsonQuizAnswers)
                .WithMessage("Quiz submission must contain valid JSON with answers");
        });

        When(x => x.TaskType == TaskType.Screenshot, () =>
        {
            RuleFor(x => x.SubmissionData)
                .Must(BeValidBase64Image)
                .WithMessage("Screenshot submission must contain valid base64 image data");
        });

        When(x => x.TaskType == TaskType.TextSubmission, () =>
        {
            RuleFor(x => x.SubmissionData)
                .MinimumLength(10)
                .WithMessage("Text submission must be at least 10 characters long")
                .Must(ContainMeaningfulContent)
                .WithMessage("Text submission must contain meaningful content");
        });

        When(x => x.TaskType == TaskType.WalletVerification, () =>
        {
            RuleFor(x => x.SubmissionData)
                .Must(BeValidWalletAddress)
                .WithMessage("Wallet verification must contain a valid wallet address");
        });
    }

    private static bool BeValidJsonQuizAnswers(string submissionData)
    {
        try
        {
            var answers = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(submissionData);
            return answers != null && answers.Any();
        }
        catch
        {
            return false;
        }
    }

    private static bool BeValidBase64Image(string submissionData)
    {
        try
        {
            // Check if it's a valid base64 string and has image data URL format
            if (submissionData.StartsWith("data:image/"))
            {
                var base64Data = submissionData.Substring(submissionData.IndexOf(',') + 1);
                var imageBytes = Convert.FromBase64String(base64Data);
                return imageBytes.Length > 0 && imageBytes.Length <= 5 * 1024 * 1024; // Max 5MB
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private static bool ContainMeaningfulContent(string text)
    {
        // Basic check for meaningful content (not just spaces, repeated characters, etc.)
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length >= 3 && words.Distinct().Count() >= 2;
    }

    private static bool BeValidWalletAddress(string address)
    {
        // Basic validation for common crypto wallet address formats
        if (string.IsNullOrEmpty(address))
            return false;

        // Ethereum address
        if (address.StartsWith("0x") && address.Length == 42)
            return System.Text.RegularExpressions.Regex.IsMatch(address, @"^0x[a-fA-F0-9]{40}$");

        // Bitcoin address (simplified validation)
        if (address.Length >= 26 && address.Length <= 35)
            return System.Text.RegularExpressions.Regex.IsMatch(address, @"^[13][a-km-zA-HJ-NP-Z1-9]{25,34}$") ||
                   System.Text.RegularExpressions.Regex.IsMatch(address, @"^bc1[a-z0-9]{39,59}$");

        return false;
    }
}

public class UpdateProgressRequestValidator : AbstractValidator<UpdateProgressRequest>
{
    public UpdateProgressRequestValidator()
    {
        RuleFor(x => x.WatchedDuration)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Watched duration cannot be negative");

        RuleFor(x => x.TotalDuration)
            .GreaterThan(0)
            .WithMessage("Total duration must be greater than 0");

        RuleFor(x => x.WatchedDuration)
            .LessThanOrEqualTo(x => x.TotalDuration)
            .WithMessage("Watched duration cannot exceed total duration");

        // Consider lesson complete if watched at least 80%
        RuleFor(x => x.IsCompleted)
            .Equal(true)
            .When(x => x.WatchedDuration >= (x.TotalDuration * 0.8))
            .WithMessage("Lesson should be marked complete when 80% or more is watched");
    }
}

// Custom validation behavior for MediatR
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Any())
            {
                throw new ValidationException(failures);
            }
        }

        return await next();
    }
}
```

## Best Practices

### 1. Clean Architecture
- **Dependency Inversion**: Always depend on abstractions, not concretions
- **Separation of Concerns**: Each layer has distinct responsibilities
- **Domain-Driven Design**: Rich domain models with business logic
- **SOLID Principles**: Follow Single Responsibility, Open/Closed, etc.

### 2. Error Handling
- **Result Pattern**: Use Result<T> for operation outcomes
- **Custom Exceptions**: Define specific exception types for different scenarios
- **Global Exception Handling**: Implement middleware for consistent error responses
- **Logging**: Comprehensive logging for debugging and monitoring

### 3. Security
- **Authentication**: JWT-based authentication with refresh tokens
- **Authorization**: Permission-based authorization with role hierarchy
- **Input Validation**: Validate all inputs at multiple layers
- **SQL Injection Protection**: Use Entity Framework parameterized queries

### 4. Performance
- **Async/Await**: Use async operations for all I/O operations
- **Caching**: Implement caching for frequently accessed data
- **Pagination**: Always paginate large data sets
- **Query Optimization**: Use AsNoTracking for read-only queries

### 5. Testing
- **Unit Testing**: Test business logic in isolation
- **Integration Testing**: Test API endpoints and database operations
- **Mocking**: Use mock objects for external dependencies
- **Test Coverage**: Maintain high test coverage (>80%)

## Instructions

When working on the WahadiniCryptoQuest Platform backend:

1. **Follow Clean Architecture**: Maintain clear separation between layers
2. **Domain-First Design**: Start with domain entities and business rules for crypto education
3. **Async Operations**: Use async/await for all I/O operations
4. **Error Handling**: Implement proper error handling and logging
5. **Security First**: Always validate authorization and authentication, especially for premium content
6. **Performance Conscious**: Consider performance implications of database queries, especially for video progress tracking
7. **Test Coverage**: Write comprehensive unit and integration tests
8. **Documentation**: Document API endpoints with OpenAPI/Swagger
9. **Validation**: Implement validation at both API and domain levels, especially for task submissions
10. **Monitoring**: Add logging and metrics for operational visibility
11. **Gamification**: Ensure reward points are accurately calculated and awarded
12. **Content Access Control**: Implement proper premium content access controls
13. **Task Verification**: Implement robust task verification systems for different task types
14. **Video Progress**: Accurately track and persist video watching progress
15. **Subscription Management**: Handle premium subscriptions and payment processing securely

### Crypto Learning Platform Specific Considerations:

- **Content Access**: Implement proper access control for premium courses and lessons
- **Progress Tracking**: Accurately track user progress through courses and lessons
- **Task Verification**: Implement different validation logic for various task types (Quiz, Screenshot, Wallet verification, etc.)
- **Reward System**: Ensure reward points are calculated correctly and prevent gaming the system
- **Video Integration**: Handle YouTube video embedding and progress tracking efficiently
- **Subscription Tiers**: Properly manage free vs premium user access levels
- **Admin Features**: Provide comprehensive admin tools for content management and user oversight
- **Gamification**: Implement engaging gamification features like leaderboards, achievements, and streaks
- **Mobile Responsiveness**: Ensure API design supports mobile learning experiences
- **Offline Capability**: Consider implementing offline learning capabilities for downloaded content

Always consider scalability, maintainability, security, and user experience when implementing new features or making changes to existing functionality. The platform should provide an engaging, secure, and educational experience for users learning about cryptocurrency and blockchain technology.
