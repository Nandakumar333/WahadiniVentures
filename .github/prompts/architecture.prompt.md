# WahadiniCryptoQuest - Architecture & Clean Code Standards Prompt

## Context
You are an expert software architect and clean code advocate working on WahadiniCryptoQuest, a comprehensive crypto learning platform with gamified task-to-earn features. This prompt defines the architectural principles, clean code standards, and implementation guidelines that must be followed when implementing features.

## System Overview

### Application Type
**Crypto Learning Platform** with:
- Video-based crypto education (YouTube embedded)
- Real-time task verification system
- Reward points system with redemption
- Premium subscription with discounts
- Gamification elements and achievements
- Multi-category learning paths (Airdrops, GameFi, Task-to-Earn, DeFi, NFT Strategies)
- Comprehensive admin dashboard
- Real-time progress tracking and analytics

### Technology Stack

#### Backend
- **.NET 8 C#** - Primary backend language  
- **ASP.NET Core Web API** - RESTful API framework
- **Entity Framework Core 8.0** - ORM for data access
- **PostgreSQL 15+** - Primary database with JSONB support for task data and time-based partitioning
- **ASP.NET Identity** - User authentication and management
- **JWT Bearer Tokens** - Stateless authentication with refresh tokens
- **AutoMapper** - Object-to-object mapping
- **FluentValidation** - Input validation
- **MediatR** - CQRS pattern implementation for commands and queries
- **Serilog** - Structured logging
- **Stripe SDK** - Payment processing for premium subscriptions

#### Frontend
- **React 18** with **TypeScript 4.9+** - UI framework with strong typing
- **Vite** - Modern build tool and dev server for fast development
- **TailwindCSS 3.4** - Utility-first CSS framework for responsive design
- **react-player** - YouTube video integration and playback control
- **React Router 7** - Client-side routing for SPA navigation
- **React Query 5** - Server state management, caching, and synchronization
- **Zustand** - Lightweight global state management
- **React Hook Form 7** with **Zod** - Form handling and schema validation
- **Axios** - HTTP client with request/response interceptors

#### Infrastructure & Services
- **Docker** - Containerization for development and deployment
- **PostgreSQL** - Database server with time-based partitioning for user activity data
- **YouTube API** - Video content integration (embedded player)
- **Stripe** - Payment processing for subscription management
- **MailKit** - Email service for notifications and user communication

## Architectural Principles

### 1. Clean Architecture Layers

The application MUST follow Clean Architecture with strict separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                   Presentation Layer                         │
│  (API Controllers, DTOs, Request/Response Models)            │
├─────────────────────────────────────────────────────────────┤
│                   Application Layer                          │
│  (Use Cases, Services, Commands, Queries, Business Logic)   │
├─────────────────────────────────────────────────────────────┤
│                   Domain Layer                               │
│  (Entities, Value Objects, Domain Events, Interfaces)       │
├─────────────────────────────────────────────────────────────┤
│                   Infrastructure Layer                       │
│  (Repositories, External APIs, Database Context, Payments)  │
└─────────────────────────────────────────────────────────────┘
```

#### Layer Responsibilities

**Domain Layer (Core)**
- Contains business entities and rules
- No dependencies on other layers
- Pure business logic only
- Domain events and specifications
- Business validations

**Application Layer (Services)**
- Implements use cases and business workflows
- Orchestrates domain logic
- Command and Query handlers (CQRS)
- Application-specific validations
- DTOs and mapping logic

**Infrastructure Layer**
- External service integrations
- Data persistence (repositories)
- Database context and migrations
- File storage, email services
- Third-party API clients (Stripe, YouTube)

**Presentation Layer (API)**
- API controllers and endpoints
- Request/response models
- Authentication/authorization filters
- API versioning
- Swagger/OpenAPI documentation

### 2. Backend Project Structure
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

### 3. Frontend Project Structure

```
frontend/
├── src/
│   ├── components/                                 # Feature-based components
│   │   ├── auth/                                   # Authentication components
│   │   │   ├── LoginForm/
│   │   │   │   ├── LoginForm.tsx
│   │   │   │   ├── LoginForm.module.css
│   │   │   │   └── index.ts
│   │   │   ├── RegisterForm/
│   │   │   │   ├── RegisterForm.tsx
│   │   │   │   ├── RegisterForm.module.css
│   │   │   │   └── index.ts
│   │   │   ├── ProtectedRoute/
│   │   │   │   ├── ProtectedRoute.tsx
│   │   │   │   └── index.ts
│   │   │   └── index.ts                            # Export all auth components
│   │   │
│   │   ├── course/                                 # Course-related components
│   │   │   ├── CourseCard/
│   │   │   │   ├── CourseCard.tsx
│   │   │   │   ├── CourseCard.module.css
│   │   │   │   └── index.ts
│   │   │   ├── CourseList/
│   │   │   │   ├── CourseList.tsx
│   │   │   │   └── index.ts
│   │   │   ├── CourseDetails/
│   │   │   │   ├── CourseDetails.tsx
│   │   │   │   └── index.ts
│   │   │   └── index.ts
│   │   │
│   │   ├── lesson/                                 # Lesson-related components
│   │   │   ├── VideoPlayer/
│   │   │   │   ├── VideoPlayer.tsx
│   │   │   │   ├── VideoPlayer.module.css
│   │   │   │   ├── useVideoTracking.ts             # Component-specific hook
│   │   │   │   └── index.ts
│   │   │   ├── LessonContent/
│   │   │   │   ├── LessonContent.tsx
│   │   │   │   └── index.ts
│   │   │   ├── ProgressBar/
│   │   │   │   ├── ProgressBar.tsx
│   │   │   │   ├── ProgressBar.module.css
│   │   │   │   └── index.ts
│   │   │   └── index.ts
│   │   │
│   │   ├── task/                                   # Task-related components
│   │   │   ├── QuizTask/
│   │   │   │   ├── QuizTask.tsx
│   │   │   │   └── index.ts
│   │   │   ├── ScreenshotTask/
│   │   │   │   ├── ScreenshotTask.tsx
│   │   │   │   └── index.ts
│   │   │   ├── WalletTask/
│   │   │   │   ├── WalletTask.tsx
│   │   │   │   └── index.ts
│   │   │   ├── TaskSubmissionForm/
│   │   │   │   ├── TaskSubmissionForm.tsx
│   │   │   │   ├── useTaskSubmission.ts            # Component-specific hook
│   │   │   │   └── index.ts
│   │   │   └── index.ts
│   │   │
│   │   ├── reward/                                 # Reward-related components
│   │   │   ├── PointsDisplay/
│   │   │   │   ├── PointsDisplay.tsx
│   │   │   │   └── index.ts
│   │   │   ├── Leaderboard/
│   │   │   │   ├── Leaderboard.tsx
│   │   │   │   └── index.ts
│   │   │   ├── RewardHistory/
│   │   │   │   ├── RewardHistory.tsx
│   │   │   │   └── index.ts
│   │   │   └── index.ts
│   │   │
│   │   ├── admin/                                  # Admin components
│   │   │   ├── Dashboard/
│   │   │   │   ├── Dashboard.tsx
│   │   │   │   └── index.ts
│   │   │   ├── CourseManager/
│   │   │   │   ├── CourseManager.tsx
│   │   │   │   └── index.ts
│   │   │   ├── TaskReview/
│   │   │   │   ├── TaskReview.tsx
│   │   │   │   └── index.ts
│   │   │   └── index.ts
│   │   │
│   │   └── ui/                                     # Reusable UI components
│   │       ├── Button/
│   │       │   ├── Button.tsx
│   │       │   ├── Button.module.css
│   │       │   └── index.ts
│   │       ├── Input/
│   │       │   ├── Input.tsx
│   │       │   ├── Input.module.css
│   │       │   └── index.ts
│   │       ├── Card/
│   │       │   ├── Card.tsx
│   │       │   ├── Card.module.css
│   │       │   └── index.ts
│   │       ├── Modal/
│   │       │   ├── Modal.tsx
│   │       │   ├── Modal.module.css
│   │       │   └── index.ts
│   │       ├── Layout/
│   │       │   ├── Header/
│   │       │   │   ├── Header.tsx
│   │       │   │   └── index.ts
│   │       │   ├── Footer/
│   │       │   │   ├── Footer.tsx
│   │       │   │   └── index.ts
│   │       │   ├── Sidebar/
│   │       │   │   ├── Sidebar.tsx
│   │       │   │   └── index.ts
│   │       │   └── index.ts
│   │       └── index.ts                            # Export all UI components
│   │
│   ├── hooks/                                      # Custom hooks (business logic)
│   │   ├── auth/
│   │   │   ├── useAuth.ts
│   │   │   ├── useLogin.ts
│   │   │   ├── useRegister.ts
│   │   │   └── index.ts
│   │   ├── course/
│   │   │   ├── useCourses.ts
│   │   │   ├── useCourse.ts
│   │   │   ├── useCourseEnrollment.ts
│   │   │   └── index.ts
│   │   ├── lesson/
│   │   │   ├── useLessons.ts
│   │   │   ├── useLesson.ts
│   │   │   ├── useLessonProgress.ts
│   │   │   └── index.ts
│   │   ├── task/
│   │   │   ├── useTasks.ts
│   │   │   ├── useTask.ts
│   │   │   ├── useTaskSubmissions.ts
│   │   │   └── index.ts
│   │   ├── reward/
│   │   │   ├── useRewards.ts
│   │   │   ├── useLeaderboard.ts
│   │   │   ├── usePoints.ts
│   │   │   └── index.ts
│   │   ├── shared/                                 # Utility hooks
│   │   │   ├── useApi.ts
│   │   │   ├── useDebounce.ts
│   │   │   ├── useLocalStorage.ts
│   │   │   └── index.ts
│   │   └── index.ts
│   │
│   ├── services/                                   # API and business services
│   │   ├── api/
│   │   │   ├── client.ts                           # Axios configuration
│   │   │   ├── auth.service.ts
│   │   │   ├── course.service.ts
│   │   │   ├── lesson.service.ts
│   │   │   ├── task.service.ts
│   │   │   ├── reward.service.ts
│   │   │   ├── subscription.service.ts
│   │   │   └── index.ts
│   │   ├── storage/
│   │   │   ├── localStorage.service.ts
│   │   │   └── index.ts
│   │   ├── validation/
│   │   │   ├── auth.validation.ts
│   │   │   ├── course.validation.ts
│   │   │   ├── task.validation.ts
│   │   │   └── index.ts
│   │   └── index.ts
│   │
│   ├── types/                                      # TypeScript type definitions
│   │   ├── auth.types.ts
│   │   ├── course.types.ts
│   │   ├── lesson.types.ts
│   │   ├── task.types.ts
│   │   ├── reward.types.ts
│   │   ├── subscription.types.ts
│   │   ├── api.types.ts
│   │   ├── common.types.ts
│   │   └── index.ts
│   │
│   ├── utils/                                      # Utility functions
│   │   ├── formatters/
│   │   │   ├── date.formatter.ts
│   │   │   ├── currency.formatter.ts
│   │   │   ├── text.formatter.ts
│   │   │   └── index.ts
│   │   ├── validators/
│   │   │   ├── email.validator.ts
│   │   │   ├── password.validator.ts
│   │   │   ├── url.validator.ts
│   │   │   └── index.ts
│   │   ├── helpers/
│   │   │   ├── array.helpers.ts
│   │   │   ├── object.helpers.ts
│   │   │   ├── string.helpers.ts
│   │   │   └── index.ts
│   │   └── index.ts
│   │
│   ├── store/                                      # Global state management (Zustand)
│   │   ├── auth.store.ts
│   │   ├── user.store.ts
│   │   ├── course.store.ts
│   │   ├── reward.store.ts
│   │   ├── theme.store.ts
│   │   └── index.ts
│   │
│   ├── constants/                                  # Application constants
│   │   ├── routes.ts
│   │   ├── api.constants.ts
│   │   ├── app.constants.ts
│   │   └── index.ts
│   │
│   ├── pages/                                      # Page-level components (route handlers)
│   │   ├── HomePage.tsx
│   │   ├── auth/
│   │   │   ├── LoginPage.tsx
│   │   │   ├── RegisterPage.tsx
│   │   │   └── index.ts
│   │   ├── course/
│   │   │   ├── CoursesPage.tsx
│   │   │   ├── CourseDetailPage.tsx
│   │   │   └── index.ts
│   │   ├── lesson/
│   │   │   ├── LessonPage.tsx
│   │   │   └── index.ts
│   │   ├── dashboard/
│   │   │   ├── DashboardPage.tsx
│   │   │   ├── ProfilePage.tsx
│   │   │   └── index.ts
│   │   ├── admin/
│   │   │   ├── AdminDashboardPage.tsx
│   │   │   ├── AdminCoursesPage.tsx
│   │   │   ├── AdminUsersPage.tsx
│   │   │   └── index.ts
│   │   └── index.ts
│   │
│   ├── styles/                                     # Global styles and themes
│   │   ├── globals.css
│   │   ├── tailwind.css
│   │   ├── variables.css
│   │   └── themes/
│   │       ├── light.theme.css
│   │       ├── dark.theme.css
│   │       └── index.ts
│   │
│   ├── config/                                     # Configuration files
│   │   ├── env.config.ts
│   │   ├── api.config.ts
│   │   ├── router.config.ts
│   │   └── index.ts
│   │
│   ├── App.tsx                                     # Root component
│   ├── main.tsx                                    # Entry point
│   └── vite-env.d.ts                               # Vite types
│
├── public/                                         # Static assets
│   ├── favicon.ico
│   ├── logo.svg
│   └── manifest.json
│
├── package.json
├── vite.config.ts
├── tailwind.config.js
├── tsconfig.json
├── tsconfig.app.json
├── tsconfig.node.json
├── postcss.config.js
├── eslint.config.js
└── README.md
```

## Clean Code Standards

### 1. Naming Conventions

#### C# Backend

**Classes and Interfaces**
```csharp
// PascalCase for classes, interfaces (prefix with I), enums
public class UserService { }
public interface IUserRepository { }
public enum TaskType { Quiz, Screenshot, WalletVerification }

// Suffix conventions
public class UserDto { }                    // Data Transfer Objects
public class CreateUserCommand { }          // Commands
public class GetUsersQuery { }              // Queries
public class UserNotFoundException { }      // Exceptions
```

**Methods and Properties**
```csharp
// PascalCase for public methods and properties
public async Task<User> GetUserByIdAsync(Guid userId) { }
public string FullName { get; set; }

// camelCase for private fields with underscore prefix
private readonly IUserRepository _userRepository;
private string _internalValue;

// camelCase for parameters and local variables
public void ProcessUser(User user, int retryCount) 
{
    var processedData = TransformData(user);
}
```

**Constants and Configuration**
```csharp
// PascalCase for constants
public const int MaxRetryAttempts = 3;
public const string DefaultCurrency = "USD";
```

#### TypeScript Frontend

**Files and Folders**
```typescript
// PascalCase for component files
LoginForm.tsx
CourseCard.tsx
UserProfile.tsx

// camelCase for non-component files
authService.ts
useAuth.ts
formatters.ts

// kebab-case for CSS files
button-styles.css
```

**Components and Types**
```typescript
// PascalCase for components, interfaces, types, enums
export const LoginForm: React.FC<LoginFormProps> = () => { }
export interface User { }
export type CourseStatus = 'active' | 'completed' | 'archived'
export enum TaskType { Quiz, Screenshot, Wallet }
```

**Functions and Variables**
```typescript
// camelCase for functions, variables, parameters
const getUserById = async (userId: string) => { }
const [isLoading, setIsLoading] = useState(false)
const totalPoints = calculatePoints(submissions)
```

**Constants**
```typescript
// SCREAMING_SNAKE_CASE for true constants
export const API_BASE_URL = 'https://api.example.com'
export const MAX_FILE_SIZE = 5 * 1024 * 1024

// PascalCase for configuration objects
export const ApiConfig = {
  baseUrl: 'https://api.example.com',
  timeout: 30000
}
```

### 2. Code Organization Principles

#### Single Responsibility Principle (SRP)
```csharp
// ❌ BAD - Multiple responsibilities
public class UserService
{
    public User CreateUser() { }
    public void SendEmail() { }
    public void LogActivity() { }
    public void ProcessPayment() { }
}

// ✅ GOOD - Single responsibility
public class UserService
{
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;
    private readonly IPaymentService _paymentService;
    
    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        // Only handles user creation logic
        var user = new User(...);
        await _emailService.SendWelcomeEmailAsync(user.Email);
        _logger.LogInformation("User created: {UserId}", user.Id);
        return user;
    }
}
```

#### Dependency Inversion Principle (DIP)
```csharp
// ✅ Depend on abstractions, not concretions
public class CourseService
{
    private readonly ICourseRepository _repository;  // Interface
    private readonly IEmailService _emailService;    // Interface
    
    public CourseService(
        ICourseRepository repository,
        IEmailService emailService)
    {
        _repository = repository;
        _emailService = emailService;
    }
}
```

#### Open/Closed Principle (OCP)
```csharp
// ✅ Open for extension, closed for modification
public abstract class TaskValidator
{
    public abstract Task<bool> ValidateAsync(TaskSubmission submission);
}

public class QuizTaskValidator : TaskValidator
{
    public override async Task<bool> ValidateAsync(TaskSubmission submission)
    {
        // Quiz-specific validation
        return true;
    }
}

public class ScreenshotTaskValidator : TaskValidator
{
    public override async Task<bool> ValidateAsync(TaskSubmission submission)
    {
        // Screenshot-specific validation
        return true;
    }
}
```

### 3. Domain Entity Design

```csharp
// Rich domain model with business logic
public class Course
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public Guid CategoryId { get; private set; }
    public bool IsPremium { get; private set; }
    public CourseStatus Status { get; private set; }
    public int EnrollmentCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    
    // Navigation properties
    public virtual Category Category { get; set; }
    public virtual ICollection<Lesson> Lessons { get; set; }
    public virtual ICollection<Enrollment> Enrollments { get; set; }
    
    // Private constructor for EF Core
    private Course() 
    {
        Lessons = new List<Lesson>();
        Enrollments = new List<Enrollment>();
    }
    
    // Factory method for creation
    public static Course Create(
        string title,
        string description,
        Guid categoryId,
        bool isPremium)
    {
        // Business rule validations
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Course title cannot be empty");
            
        if (title.Length > 200)
            throw new DomainException("Course title cannot exceed 200 characters");
        
        return new Course
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description?.Trim() ?? string.Empty,
            CategoryId = categoryId,
            IsPremium = isPremium,
            Status = CourseStatus.Draft,
            EnrollmentCount = 0,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    // Domain methods with business logic
    public void Publish()
    {
        if (Status == CourseStatus.Published)
            throw new DomainException("Course is already published");
            
        if (!Lessons.Any())
            throw new DomainException("Cannot publish course without lessons");
        
        Status = CourseStatus.Published;
        PublishedAt = DateTime.UtcNow;
    }
    
    public void Archive()
    {
        if (Status == CourseStatus.Archived)
            throw new DomainException("Course is already archived");
        
        Status = CourseStatus.Archived;
    }
    
    public void IncrementEnrollment()
    {
        EnrollmentCount++;
    }
    
    public void UpdateDetails(string title, string description)
    {
        if (Status == CourseStatus.Published)
            throw new DomainException("Cannot modify published course directly");
        
        Title = title?.Trim() ?? throw new ArgumentNullException(nameof(title));
        Description = description?.Trim() ?? string.Empty;
    }
}
```

### 4. Service Layer Pattern

```csharp
// Service interface
public interface ICourseService
{
    Task<Result<CourseDto>> CreateCourseAsync(CreateCourseRequest request, Guid userId);
    Task<Result<CourseDto>> GetCourseByIdAsync(Guid courseId, Guid userId);
    Task<Result<IEnumerable<CourseDto>>> GetCoursesAsync(CourseFilter filter, Guid userId);
    Task<Result<CourseDto>> PublishCourseAsync(Guid courseId, Guid userId);
    Task<Result<bool>> DeleteCourseAsync(Guid courseId, Guid userId);
}

// Service implementation
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
    
    public async Task<Result<CourseDto>> CreateCourseAsync(
        CreateCourseRequest request,
        Guid userId)
    {
        try
        {
            // Authorization check
            if (!await _authService.CanCreateCourseAsync(userId))
            {
                return Result<CourseDto>.Failure(
                    "Insufficient permissions to create course",
                    "PERMISSION_DENIED");
            }
            
            // Business logic
            var course = Course.Create(
                request.Title,
                request.Description,
                request.CategoryId,
                request.IsPremium);
            
            // Persist
            var createdCourse = await _unitOfWork.Courses.CreateAsync(course);
            await _unitOfWork.CommitAsync();
            
            // Map and return
            var courseDto = _mapper.Map<CourseDto>(createdCourse);
            
            _logger.LogInformation(
                "Course created successfully. Id: {CourseId}, User: {UserId}",
                createdCourse.Id,
                userId);
            
            return Result<CourseDto>.Success(courseDto);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Domain validation failed: {Message}", ex.Message);
            return Result<CourseDto>.Failure(ex.Message, "DOMAIN_ERROR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course for user {UserId}", userId);
            return Result<CourseDto>.Failure(
                "An error occurred while creating the course",
                "INTERNAL_ERROR");
        }
    }
}
```

### 5. API Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly ILogger<CoursesController> _logger;
    
    public CoursesController(
        ICourseService courseService,
        ILogger<CoursesController> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get all courses with filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CourseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PagedResult<CourseDto>>>> GetCourses(
        [FromQuery] GetCoursesRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var filter = new CourseFilter
            {
                CategoryId = request.CategoryId,
                IsPremium = request.IsPremium,
                Page = request.Page,
                PageSize = request.PageSize
            };
            
            var result = await _courseService.GetCoursesAsync(filter, userId);
            
            if (result.IsSuccess)
            {
                return Ok(ApiResponse<PagedResult<CourseDto>>.Success(result.Data));
            }
            
            return HandleFailureResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving courses");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse("An error occurred while retrieving courses"));
        }
    }
    
    /// <summary>
    /// Create a new course
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CourseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<CourseDto>>> CreateCourse(
        CreateCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _courseService.CreateCourseAsync(request, userId);
            
            if (result.IsSuccess)
            {
                return CreatedAtAction(
                    nameof(GetCourseById),
                    new { id = result.Data.Id },
                    ApiResponse<CourseDto>.Success(result.Data));
            }
            
            return HandleFailureResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse("An error occurred while creating the course"));
        }
    }
    
    // Helper methods
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
    
    private ActionResult HandleFailureResult<T>(Result<T> result)
    {
        return result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(new ErrorResponse(result.ErrorMessage)),
            "PERMISSION_DENIED" => Forbid(),
            "VALIDATION_ERROR" => BadRequest(new ErrorResponse(result.ErrorMessage)),
            "DOMAIN_ERROR" => BadRequest(new ErrorResponse(result.ErrorMessage)),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse("An unexpected error occurred"))
        };
    }
}
```

### 6. Frontend Component Pattern

```typescript
// Component with TypeScript and proper structure
interface CourseCardProps {
  course: Course
  onEnroll?: (courseId: string) => void
  onViewDetails?: (courseId: string) => void
  className?: string
}

export const CourseCard: React.FC<CourseCardProps> = ({
  course,
  onEnroll,
  onViewDetails,
  className
}) => {
  // State
  const [isEnrolling, setIsEnrolling] = useState(false)
  
  // Hooks
  const { user } = useAuth()
  const { mutate: enrollCourse } = useEnrollCourse()
  
  // Computed values
  const isEnrolled = useMemo(
    () => course.enrollments?.some(e => e.userId === user?.id),
    [course.enrollments, user?.id]
  )
  
  const canEnroll = useMemo(
    () => !isEnrolled && (!course.isPremium || user?.subscription === 'premium'),
    [isEnrolled, course.isPremium, user?.subscription]
  )
  
  // Event handlers
  const handleEnroll = async () => {
    if (!canEnroll || !course.id) return
    
    setIsEnrolling(true)
    try {
      await enrollCourse(course.id)
      onEnroll?.(course.id)
    } catch (error) {
      console.error('Enrollment failed:', error)
    } finally {
      setIsEnrolling(false)
    }
  }
  
  const handleViewDetails = () => {
    if (course.id) {
      onViewDetails?.(course.id)
    }
  }
  
  // Render
  return (
    <div className={cn('bg-white rounded-lg shadow-md p-6', className)}>
      {/* Premium badge */}
      {course.isPremium && (
        <div className="inline-flex items-center px-3 py-1 rounded-full bg-yellow-100 text-yellow-800 text-sm font-medium mb-3">
          <Crown className="w-4 h-4 mr-1" />
          Premium
        </div>
      )}
      
      {/* Course title and description */}
      <h3 className="text-xl font-bold text-gray-900 mb-2">
        {course.title}
      </h3>
      <p className="text-gray-600 mb-4 line-clamp-3">
        {course.description}
      </p>
      
      {/* Course meta */}
      <div className="flex items-center justify-between text-sm text-gray-500 mb-4">
        <div className="flex items-center">
          <BookOpen className="w-4 h-4 mr-1" />
          <span>{course.lessonCount} lessons</span>
        </div>
        <div className="flex items-center">
          <Users className="w-4 h-4 mr-1" />
          <span>{course.enrollmentCount} enrolled</span>
        </div>
      </div>
      
      {/* Action buttons */}
      <div className="flex gap-2">
        <Button
          variant="primary"
          onClick={handleEnroll}
          disabled={!canEnroll || isEnrolling}
          className="flex-1"
        >
          {isEnrolling ? (
            <>
              <Loader2 className="w-4 h-4 mr-2 animate-spin" />
              Enrolling...
            </>
          ) : isEnrolled ? (
            'Enrolled'
          ) : (
            'Enroll Now'
          )}
        </Button>
        <Button
          variant="outline"
          onClick={handleViewDetails}
          className="flex-1"
        >
          View Details
        </Button>
      </div>
    </div>
  )
}
```

### 7. React Hook Pattern

```typescript
// Custom hook with proper typing and error handling
interface UseCoursesOptions {
  categoryId?: string
  isPremium?: boolean
  page?: number
  pageSize?: number
}

interface UseCoursesReturn {
  courses: Course[]
  isLoading: boolean
  isError: boolean
  error: Error | null
  totalPages: number
  currentPage: number
  refetch: () => void
}

export const useCourses = (options: UseCoursesOptions = {}): UseCoursesReturn => {
  const {
    categoryId,
    isPremium,
    page = 1,
    pageSize = 12
  } = options
  
  // Build query key
  const queryKey = ['courses', { categoryId, isPremium, page, pageSize }]
  
  // Fetch data with React Query
  const {
    data,
    isLoading,
    isError,
    error,
    refetch
  } = useQuery({
    queryKey,
    queryFn: async () => {
      const response = await courseService.getCourses({
        categoryId,
        isPremium,
        page,
        pageSize
      })
      return response.data
    },
    staleTime: 5 * 60 * 1000, // 5 minutes
    cacheTime: 10 * 60 * 1000, // 10 minutes
    retry: 2,
    onError: (err) => {
      console.error('Failed to fetch courses:', err)
    }
  })
  
  return {
    courses: data?.items ?? [],
    isLoading,
    isError,
    error: error as Error | null,
    totalPages: data?.totalPages ?? 0,
    currentPage: data?.currentPage ?? 1,
    refetch
  }
}
```

## Implementation Guidelines for Feature Development

When implementing features for WahadiniCryptoQuest, follow these guidelines to ensure consistency with the established architecture:

### 1. Start with Core Features First
- Begin with user authentication and basic course structure
- Implement video player integration with YouTube
- Add task system with basic types (Quiz, Screenshot, Text submission)
- Build reward points system
- Add subscription/payment integration

### 2. Backend Development Approach
- Define domain entities with proper relationships
- Create repository interfaces and implementations
- Build service layer with business logic
- Implement API controllers with proper validation
- Add authentication and authorization middleware

### 3. Frontend Development Approach  
- Start with basic components and layout
- Implement authentication flow
- Build course and lesson viewing components
- Add task submission functionality
- Create user dashboard and progress tracking

### 4. Database Design
- Use Entity Framework Core code-first approach
- Implement proper foreign key relationships
- Add indexes for performance on frequently queried fields
- Use JSONB columns in PostgreSQL for flexible task data storage
- Implement time-based partitioning for user activity data

### 5. API Design
- Follow RESTful principles
- Use consistent response formats
- Implement proper error handling and status codes
- Add comprehensive input validation
- Document APIs with OpenAPI/Swagger

### 6. Security Implementation
- Use JWT tokens for authentication
- Implement role-based authorization
- Validate all inputs on both client and server
- Use HTTPS in production
- Implement rate limiting to prevent abuse

### 7. State Management
- Use Zustand for global state (auth, user data)
- Use React Query for server state and caching
- Keep component state local when possible
- Implement optimistic updates for better UX

### 8. Testing Strategy
- Unit tests for business logic and utilities
- Integration tests for API endpoints
- Component tests for critical UI elements
- E2E tests for main user flows

## Code Quality Checklist

### Backend Code Review
- [ ] Follows Clean Architecture layers
- [ ] Uses dependency injection
- [ ] Implements proper error handling
- [ ] Includes logging
- [ ] Has unit tests
- [ ] Uses async/await correctly
- [ ] Validates inputs
- [ ] Handles concurrency
- [ ] Documents public APIs
- [ ] Follows naming conventions

### Frontend Code Review
- [ ] Components are properly typed
- [ ] Follows feature-based structure
- [ ] Uses hooks correctly
- [ ] Implements error boundaries
- [ ] Handles loading states
- [ ] Validates user inputs
- [ ] Accessible UI (a11y)
- [ ] Responsive design
- [ ] Optimized performance
- [ ] Follows naming conventions

## Error Handling Standards

### Backend Error Handling
```csharp
// Custom exceptions
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string entity, object id)
        : base($"{entity} with id {id} not found") { }
}

// Result pattern
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Data { get; }
    public string ErrorMessage { get; }
    public string ErrorCode { get; }
    
    public static Result<T> Success(T data) =>
        new Result<T> { IsSuccess = true, Data = data };
        
    public static Result<T> Failure(string message, string code) =>
        new Result<T> { IsSuccess = false, ErrorMessage = message, ErrorCode = code };
}

// Global exception handler
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var response = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "NOT_FOUND"),
            DomainException => (StatusCodes.Status400BadRequest, "DOMAIN_ERROR"),
            ValidationException => (StatusCodes.Status400BadRequest, "VALIDATION_ERROR"),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "FORBIDDEN"),
            _ => (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR")
        };
        
        httpContext.Response.StatusCode = response.Item1;
        await httpContext.Response.WriteAsJsonAsync(
            new ErrorResponse(exception.Message, response.Item2),
            cancellationToken);
        
        return true;
    }
}
```

### Frontend Error Handling
```typescript
// Error boundary component
export class ErrorBoundary extends React.Component<
  { children: ReactNode },
  { hasError: boolean; error: Error | null }
> {
  constructor(props: { children: ReactNode }) {
    super(props)
    this.state = { hasError: false, error: null }
  }
  
  static getDerivedStateFromError(error: Error) {
    return { hasError: true, error }
  }
  
  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('Error boundary caught:', error, errorInfo)
  }
  
  render() {
    if (this.state.hasError) {
      return (
        <div className="min-h-screen flex items-center justify-center">
          <div className="text-center">
            <h1 className="text-2xl font-bold text-red-600 mb-4">
              Something went wrong
            </h1>
            <p className="text-gray-600 mb-4">
              {this.state.error?.message ?? 'Unknown error'}
            </p>
            <Button onClick={() => this.setState({ hasError: false, error: null })}>
              Try Again
            </Button>
          </div>
        </div>
      )
    }
    
    return this.props.children
  }
}

// API error handling
export const handleApiError = (error: unknown): string => {
  if (axios.isAxiosError(error)) {
    if (error.response) {
      return error.response.data?.message ?? 'Server error occurred'
    } else if (error.request) {
      return 'No response from server'
    }
  }
  return 'An unexpected error occurred'
}
```

## Security Best Practices

### Authentication & Authorization
```csharp
// JWT configuration
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"])),
            ClockSkew = TimeSpan.Zero
        };
    });

// Role-based authorization
services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("PremiumUser", policy => 
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "subscription" && c.Value == "premium")));
});
```

### Input Validation
```csharp
// FluentValidation
public class CreateCourseValidator : AbstractValidator<CreateCourseRequest>
{
    public CreateCourseValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");
            
        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");
            
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");
    }
}
```

### SQL Injection Prevention
```csharp
// Always use parameterized queries with EF Core
public async Task<IEnumerable<Course>> SearchCoursesAsync(string searchTerm)
{
    // ✅ Safe - parameterized
    return await _context.Courses
        .Where(c => EF.Functions.ILike(c.Title, $"%{searchTerm}%"))
        .ToListAsync();
        
    // ❌ Never do this - SQL injection vulnerability
    // return await _context.Courses
    //     .FromSqlRaw($"SELECT * FROM Courses WHERE Title LIKE '%{searchTerm}%'")
    //     .ToListAsync();
}
```

## Performance Optimization

### Backend Optimization
```csharp
// Use AsNoTracking for read-only queries
public async Task<IEnumerable<Course>> GetCoursesAsync()
{
    return await _context.Courses
        .Include(c => c.Category)
        .AsNoTracking()
        .ToListAsync();
}

// Implement caching
public class CourseService
{
    private readonly IMemoryCache _cache;
    
    public async Task<Course> GetCourseByIdAsync(Guid id)
    {
        var cacheKey = $"course_{id}";
        
        if (_cache.TryGetValue(cacheKey, out Course cachedCourse))
        {
            return cachedCourse;
        }
        
        var course = await _repository.GetByIdAsync(id);
        
        _cache.Set(cacheKey, course, TimeSpan.FromMinutes(10));
        
        return course;
    }
}

// Use pagination
public async Task<PagedResult<Course>> GetPagedCoursesAsync(int page, int pageSize)
{
    var query = _context.Courses.AsQueryable();
    
    var totalCount = await query.CountAsync();
    
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return new PagedResult<Course>
    {
        Items = items,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };
}
```

### Frontend Optimization
```typescript
// Lazy loading components
const AdminDashboard = lazy(() => import('./features/admin/components/Dashboard'))

// Memoization
const ExpensiveComponent: React.FC<Props> = ({ data }) => {
  const processedData = useMemo(() => {
    return expensiveCalculation(data)
  }, [data])
  
  return <div>{processedData}</div>
}

// Debouncing
const SearchInput: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState('')
  const debouncedSearch = useDebounce(searchTerm, 500)
  
  useEffect(() => {
    if (debouncedSearch) {
      performSearch(debouncedSearch)
    }
  }, [debouncedSearch])
  
  return <input onChange={(e) => setSearchTerm(e.target.value)} />
}
```

## Testing Standards

### Backend Unit Tests
```csharp
public class CourseServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CourseService>> _loggerMock;
    private readonly CourseService _sut;
    
    public CourseServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CourseService>>();
        _sut = new CourseService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateCourseAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var request = new CreateCourseRequest
        {
            Title = "Test Course",
            Description = "Test Description",
            CategoryId = Guid.NewGuid()
        };
        
        var course = Course.Create(
            request.Title,
            request.Description,
            request.CategoryId,
            false);
        
        _unitOfWorkMock
            .Setup(x => x.Courses.CreateAsync(It.IsAny<Course>()))
            .ReturnsAsync(course);
        
        // Act
        var result = await _sut.CreateCourseAsync(request, Guid.NewGuid());
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }
}
```

### Frontend Component Tests
```typescript
describe('CourseCard', () => {
  const mockCourse: Course = {
    id: '123',
    title: 'Test Course',
    description: 'Test Description',
    isPremium: false,
    lessonCount: 10,
    enrollmentCount: 100
  }
  
  it('renders course information correctly', () => {
    render(<CourseCard course={mockCourse} />)
    
    expect(screen.getByText('Test Course')).toBeInTheDocument()
    expect(screen.getByText('Test Description')).toBeInTheDocument()
    expect(screen.getByText('10 lessons')).toBeInTheDocument()
  })
  
  it('calls onEnroll when enroll button is clicked', async () => {
    const onEnroll = jest.fn()
    render(<CourseCard course={mockCourse} onEnroll={onEnroll} />)
    
    const enrollButton = screen.getByText('Enroll Now')
    fireEvent.click(enrollButton)
    
    await waitFor(() => {
      expect(onEnroll).toHaveBeenCalledWith('123')
    })
  })
})
```

## Documentation Standards

### Code Documentation
```csharp
/// <summary>
/// Service for managing courses in the WahadiniCryptoQuest platform.
/// Handles course creation, updates, publishing, and enrollment.
/// </summary>
/// <remarks>
/// This service implements business logic for course management including:
/// - Course lifecycle management (draft, published, archived)
/// - Enrollment validation and processing
/// - Premium course access control
/// </remarks>
public class CourseService : ICourseService
{
    /// <summary>
    /// Creates a new course with the specified details.
    /// </summary>
    /// <param name="request">The course creation request containing title, description, etc.</param>
    /// <param name="userId">The ID of the user creating the course (must have Admin role)</param>
    /// <returns>A Result containing the created CourseDto if successful, or error information if failed</returns>
    /// <exception cref="DomainException">Thrown when business rules are violated</exception>
    public async Task<Result<CourseDto>> CreateCourseAsync(
        CreateCourseRequest request,
        Guid userId)
    {
        // Implementation
    }
}
```

### API Documentation
```csharp
/// <summary>
/// Retrieves a paginated list of courses with optional filtering
/// </summary>
/// <param name="request">Filter and pagination parameters</param>
/// <param name="cancellationToken">Cancellation token for async operation</param>
/// <returns>Paginated list of courses matching the filter criteria</returns>
/// <response code="200">Returns the list of courses</response>
/// <response code="400">If the request parameters are invalid</response>
/// <response code="401">If the user is not authenticated</response>
[HttpGet]
[ProducesResponseType(typeof(ApiResponse<PagedResult<CourseDto>>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<ActionResult<ApiResponse<PagedResult<CourseDto>>>> GetCourses(
    [FromQuery] GetCoursesRequest request,
    CancellationToken cancellationToken = default)
{
    // Implementation
}
```

## Conclusion

This architecture and clean code standards document serves as the foundation for implementing WahadiniCryptoQuest features. The key principles to follow are:

1. **Clean Architecture** - Maintain separation between domain, application, and infrastructure layers
2. **SOLID Principles** - Write maintainable and extensible code
3. **Consistent Standards** - Follow established naming conventions and project structure  
4. **Security First** - Implement proper authentication, authorization, and input validation
5. **Performance Optimization** - Use caching, pagination, and efficient database queries
6. **User Experience** - Provide loading states, error handling, and responsive design

### Quick Start for New Features

1. **Define the domain entities** and their relationships
2. **Create repository interfaces** and implementations
3. **Build service layer** with business logic
4. **Implement API endpoints** with proper validation
5. **Create frontend components** with TypeScript typing
6. **Add authentication/authorization** as needed
7. **Include tests** for critical functionality
8. **Update documentation** as features are added

### Technology Stack Summary

- **Backend**: .NET 8, EF Core, PostgreSQL, JWT, Stripe
- **Frontend**: React 18, TypeScript, Vite, TailwindCSS, React Query
- **Infrastructure**: Docker, YouTube API, Email services

This provides a solid foundation for building a scalable, maintainable crypto education platform with gamified learning experiences.
