using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Reflection;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.API.Configuration;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.Service.Mappings;
using WahadiniCryptoQuest.Core.Interfaces;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.DAL.Identity;
using WahadiniCryptoQuest.DAL.Services;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Repositories;
using MediatR;
using FluentValidation;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.DTOs.Progress;
using WahadiniCryptoQuest.Service.Validators.Auth;
using WahadiniCryptoQuest.API.Validators.Course;
using WahadiniCryptoQuest.API.Validators.Progress;
using WahadiniCryptoQuest.Service.Commands.Auth;
using WahadiniCryptoQuest.Service.Commands.Rewards;
using WahadiniCryptoQuest.Service.Validators.Reward;
using WahadiniCryptoQuest.Service.Services;
using WahadiniCryptoQuest.Service.Handlers.Authorization;
using WahadiniCryptoQuest.API.Authorization;
using WahadiniCryptoQuest.API.Filters;
using WahadiniCryptoQuest.Service.Course;
using WahadiniCryptoQuest.Service.Lesson;

namespace WahadiniCryptoQuest.API.Extensions;

/// <summary>
/// Extension methods for configuring services in the DI container
/// Follows Clean Architecture and separation of concerns
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures Entity Framework with PostgreSQL with performance optimizations
    /// Senior Architect Pattern: Connection pooling, command timeout, retry logic
    /// </summary>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var performanceSettings = configuration.GetSection(PerformanceSettings.SectionName)
            .Get<PerformanceSettings>() ?? new PerformanceSettings();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Build connection string with performance optimizations
            var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString)
            {
                // Connection Pooling: Reuse database connections
                Pooling = true,
                MinPoolSize = performanceSettings.MinDatabaseConnections,
                MaxPoolSize = performanceSettings.MaxDatabaseConnections,

                // Connection Lifetime: Recycle connections periodically
                ConnectionLifetime = 300, // 5 minutes
                ConnectionIdleLifetime = 60, // 1 minute

                // Timeout Settings: Prevent hanging connections
                Timeout = 30,
                CommandTimeout = performanceSettings.CommandTimeoutSeconds,

                // Performance: Use prepared statements
                MaxAutoPrepare = 20,
                AutoPrepareMinUsages = 2,

                // Reliability: Enable keep-alive
                KeepAlive = 30,
                TcpKeepAlive = true,
                TcpKeepAliveInterval = 10,
                TcpKeepAliveTime = 30,

                // Load Balancing: Enable for read replicas
                LoadBalanceHosts = true,

                // Application Identification
                ApplicationName = "WahadiniCryptoQuest"
            };

            options.UseNpgsql(builder.ConnectionString, npgsqlOptions =>
            {
                // Enable retry on failure (transient fault handling)
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: performanceSettings.MaxRetryAttempts,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);

                // Command timeout
                npgsqlOptions.CommandTimeout(performanceSettings.CommandTimeoutSeconds);

                // Batch size for bulk operations
                npgsqlOptions.MaxBatchSize(1000);
            });

            // Development: Enable sensitive data logging
            if (configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"))
            {
                options.EnableSensitiveDataLogging();
            }

            // Development: Enable detailed errors
            if (configuration.GetValue<bool>("Logging:EnableDetailedErrors"))
            {
                options.EnableDetailedErrors();
            }

            // Performance: Use compiled queries when possible
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        // Configure Performance Settings
        services.Configure<PerformanceSettings>(
            configuration.GetSection(PerformanceSettings.SectionName));

        services.Configure<RateLimitPolicies>(
            configuration.GetSection(RateLimitPolicies.SectionName));

        return services;
    }

    /// <summary>
    /// Configures ASP.NET Identity with security requirements
    /// </summary>
    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            // Password requirements
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequiredUniqueChars = 1;

            // User requirements
            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

            // Email confirmation required
            options.SignIn.RequireConfirmedEmail = true;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    /// <summary>
    /// Configures JWT authentication
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(Core.Settings.JwtSettings.SectionName).Get<Core.Settings.JwtSettings>();
        if (jwtSettings == null || !jwtSettings.IsValid())
        {
            throw new InvalidOperationException("JWT settings are not properly configured");
        }

        services.Configure<Core.Settings.JwtSettings>(configuration.GetSection(Core.Settings.JwtSettings.SectionName));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = jwtSettings.ValidateIssuer,
                ValidateAudience = jwtSettings.ValidateAudience,
                ValidateLifetime = jwtSettings.ValidateLifetime,
                ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = jwtSettings.ClockSkew,
                // Include role claims from JWT
                RoleClaimType = "role",
                NameClaimType = "sub"
            };
        });

        // Configure Authorization Policies (US5 - RBAC)
        services.AddAuthorizationPolicies();

        return services;
    }

    /// <summary>
    /// Configures CORS for frontend communication
    /// T184: CORS Configuration - Reads allowed origins from appsettings for security
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Read allowed origins from configuration for flexibility across environments
        var allowedOrigins = configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:5173", "https://localhost:5173" }; // Fallback to default Vite dev server

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials() // Required for JWT cookies
                      .SetIsOriginAllowedToAllowWildcardSubdomains(); // Allow subdomains if wildcard used
            });
        });

        return services;
    }

    /// <summary>
    /// Configures role-based authorization policies
    /// Following Clean Architecture and principle of least privilege
    /// </summary>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        // Register authorization handlers from Service/Handlers/Authorization
        services.AddScoped<IAuthorizationHandler, SubscriptionRequirementHandler>();
        services.AddScoped<IAuthorizationHandler, PermissionRequirementHandler>();
        services.AddScoped<IAuthorizationHandler, RoleHandler>();
        services.AddScoped<IAuthorizationHandler, EmailConfirmedHandler>();
        services.AddScoped<IAuthorizationHandler, PremiumSubscriptionHandler>();

        // Register custom permission authorization handlers
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, AllPermissionsAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            // Role-based policies
            // Admin-only policy - requires Admin role
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin"));

            // Premium user policy - requires Premium or Admin role
            options.AddPolicy("PremiumUser", policy =>
                policy.RequireRole("Premium", "Admin"));

            // Free user policy - requires any authenticated user (Free, Premium, or Admin)
            options.AddPolicy("FreeUser", policy =>
                policy.RequireAuthenticatedUser());

            // Email verification policies
            // Require email confirmed for all authenticated users
            options.AddPolicy("EmailConfirmed", policy =>
                policy.RequireClaim("email_verified", "true"));

            // Combine Premium access with email confirmation
            options.AddPolicy("PremiumAccess", policy =>
                policy.RequireRole("Premium", "Admin")
                      .RequireClaim("email_verified", "true"));

            // Combine Admin access with email confirmation
            options.AddPolicy("AdminAccess", policy =>
                policy.RequireRole("Admin")
                      .RequireClaim("email_verified", "true"));

            // T008: SuperAdmin-only policy for elevated admin operations
            // Used for user role changes, admin account banning, system configuration
            options.AddPolicy("SuperAdmin", policy =>
                policy.RequireRole("SuperAdmin"));

            // Permission-based policies (US5 - Fine-grained RBAC)
            // User management permissions
            options.AddPolicy("CanViewUsers", policy =>
                policy.RequireClaim("permission", "users:view"));

            options.AddPolicy("CanEditUsers", policy =>
                policy.RequireClaim("permission", "users:edit"));

            options.AddPolicy("CanDeleteUsers", policy =>
                policy.RequireClaim("permission", "users:delete"));

            // Course management permissions
            options.AddPolicy("CanViewCourses", policy =>
                policy.RequireClaim("permission", "courses:view"));

            options.AddPolicy("CanManageCourses", policy =>
                policy.RequireClaim("permission", "courses:manage"));

            options.AddPolicy("CanCompleteCourses", policy =>
                policy.RequireClaim("permission", "courses:complete"));

            // Lesson management permissions
            options.AddPolicy("CanViewLessons", policy =>
                policy.RequireClaim("permission", "lessons:view"));

            options.AddPolicy("CanManageLessons", policy =>
                policy.RequireClaim("permission", "lessons:manage"));

            options.AddPolicy("CanCompleteLessons", policy =>
                policy.RequireClaim("permission", "lessons:complete"));

            // Task management permissions
            options.AddPolicy("CanViewTasks", policy =>
                policy.RequireClaim("permission", "tasks:view"));

            options.AddPolicy("CanSubmitTasks", policy =>
                policy.RequireClaim("permission", "tasks:submit"));

            options.AddPolicy("CanReviewTasks", policy =>
                policy.RequireClaim("permission", "tasks:review"));

            // Reward management permissions
            options.AddPolicy("CanViewRewards", policy =>
                policy.RequireClaim("permission", "rewards:view"));

            options.AddPolicy("CanClaimRewards", policy =>
                policy.RequireClaim("permission", "rewards:claim"));

            options.AddPolicy("CanManageRewards", policy =>
                policy.RequireClaim("permission", "rewards:manage"));

            // Subscription management permissions
            options.AddPolicy("CanViewSubscriptions", policy =>
                policy.RequireClaim("permission", "subscriptions:view"));

            options.AddPolicy("CanManageSubscriptions", policy =>
                policy.RequireClaim("permission", "subscriptions:manage"));

            // Profile management permissions
            options.AddPolicy("CanReadProfile", policy =>
                policy.RequireClaim("permission", "profile:read"));

            options.AddPolicy("CanUpdateProfile", policy =>
                policy.RequireClaim("permission", "profile:update"));

            // Analytics permissions
            options.AddPolicy("CanViewAnalytics", policy =>
                policy.RequireClaim("permission", "analytics:view"));

            // Settings management permissions
            options.AddPolicy("CanManageSettings", policy =>
                policy.RequireClaim("permission", "settings:manage"));

            // Generic permission policies for custom attributes
            // Actual permission checking is done by authorization handlers
            options.AddPolicy("RequirePermission", policy =>
                policy.RequireAuthenticatedUser());

            options.AddPolicy("RequireAllPermissions", policy =>
                policy.RequireAuthenticatedUser());
        });

        return services;
    }

    /// <summary>
    /// Configures Swagger/OpenAPI documentation with JWT authentication support
    /// </summary>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            // API Information
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "WahadiniCryptoQuest API",
                Description = "A comprehensive cryptocurrency education platform API with gamification features, user authentication, and subscription management",
                Contact = new OpenApiContact
                {
                    Name = "WahadiniCryptoQuest Team",
                    Email = "support@wahadinicryptoquest.com"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // JWT Authentication Configuration
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
                              "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                              "Example: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });

            // XML Comments for documentation
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Enable annotations for more detailed documentation
            options.EnableAnnotations();

            // Add example schemas for request/response documentation
            options.SchemaFilter<ExampleSchemaFilter>();

            // Custom schema IDs to avoid conflicts
            options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

            // Order actions by controller and HTTP method
            options.OrderActionsBy((apiDesc) =>
                $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
        });

        return services;
    }

    /// <summary>
    /// Configures MediatR for CQRS pattern
    /// </summary>
    public static IServiceCollection AddMediatRConfiguration(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(UserProfile).Assembly);
        });

        // Register pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Service.Behaviors.ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Service.Behaviors.LoggingBehavior<,>));

        return services;
    }

    /// <summary>
    /// Configures AutoMapper with all profiles
    /// </summary>
    public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(UserProfile));
        return services;
    }

    /// <summary>
    /// Registers application services (to be extended with MediatR, AutoMapper, etc.)
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register core services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHashingService, PasswordHashingService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<Core.Interfaces.Services.IAuthorizationService, AuthorizationService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ILessonService, LessonService>();
        services.AddScoped<IProgressService, ProgressService>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUserProgressRepository, UserProgressRepository>();
        services.AddScoped<IUserCourseEnrollmentRepository, UserCourseEnrollmentRepository>();
        services.AddScoped<ILessonCompletionRepository, LessonCompletionRepository>();
        services.AddScoped(typeof(Core.Common.IRepository<>), typeof(Repository<>));

        // Reward System Repositories
        services.AddScoped<IRewardTransactionRepository, RewardTransactionRepository>();
        services.AddScoped<IUserStreakRepository, UserStreakRepository>();
        services.AddScoped<IUserAchievementRepository, UserAchievementRepository>();
        services.AddScoped<IReferralAttributionRepository, ReferralAttributionRepository>();
        services.AddScoped<IDiscountCodeRepository, DiscountCodeRepository>();
        services.AddScoped<IUserDiscountRedemptionRepository, UserDiscountRedemptionRepository>();

        // Subscription System Repositories (008-stripe-subscription)
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ICurrencyPricingRepository, CurrencyPricingRepository>();
        services.AddScoped<IWebhookEventRepository, WebhookEventRepository>();
        services.AddScoped<ISubscriptionHistoryRepository, SubscriptionHistoryRepository>();

        // Reward System Services
        services.AddScoped<IRewardService, RewardService>();
        services.AddScoped<IStreakService, StreakService>();
        services.AddScoped<ILeaderboardService, LeaderboardService>();
        services.AddScoped<IAchievementService, AchievementService>();
        services.AddScoped<IReferralService, ReferralService>();
        services.AddScoped<IDiscountService, DiscountService>();
        services.AddSingleton<INotificationQueue, NotificationQueueService>(); // Singleton for in-memory queue

        // Register FluentValidation validators (T183 - Input Validation Audit)
        // Auth validators
        services.AddScoped<IValidator<RegisterDto>, RegisterDtoValidator>();
        services.AddScoped<IValidator<EmailConfirmationDto>, EmailConfirmationDtoValidator>();
        services.AddScoped<IValidator<ResendEmailConfirmationDto>, ResendEmailConfirmationDtoValidator>();
        services.AddScoped<IValidator<LoginUserCommand>, LoginUserValidator>();

        // Course validators
        services.AddScoped<IValidator<CreateCourseDto>, CreateCourseValidator>();
        services.AddScoped<IValidator<UpdateCourseDto>, UpdateCourseValidator>();
        services.AddScoped<IValidator<CreateLessonDto>, CreateLessonValidator>();
        services.AddScoped<IValidator<UpdateLessonDto>, UpdateLessonValidator>();

        // Progress validators (T316)
        services.AddScoped<IValidator<UpdateProgressDto>, UpdateProgressValidator>();

        // Reward validators
        services.AddScoped<IValidator<AwardPointsCommand>, AwardPointsValidator>();

        // Stripe Payment Gateway (008-stripe-subscription)
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();

        // T016: Admin Dashboard Services (009-admin-dashboard)
        services.AddScoped<IAuditLogService, WahadiniCryptoQuest.Service.Admin.AuditLogService>();
        services.AddScoped<INotificationService, WahadiniCryptoQuest.Service.Notifications.InAppNotificationService>();
        services.AddScoped<WahadiniCryptoQuest.Service.Notifications.EmailNotificationService>();

        // T030: Analytics Service (009-admin-dashboard US1)
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        // T016: Admin Dashboard Repositories (009-admin-dashboard)
        services.AddScoped<IAuditLogEntryRepository, WahadiniCryptoQuest.DAL.Repositories.AuditLogEntryRepository>();
        services.AddScoped<IUserNotificationRepository, WahadiniCryptoQuest.DAL.Repositories.UserNotificationRepository>();
        services.AddScoped<IPointAdjustmentRepository, WahadiniCryptoQuest.DAL.Repositories.PointAdjustmentRepository>();

        return services;
    }
}