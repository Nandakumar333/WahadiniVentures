using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.DAL.Context;

/// <summary>
/// Application database context using custom domain entities
/// Follows Clean Architecture patterns with proper entity configuration
/// Note: We use custom User/Role entities instead of ASP.NET Identity for full control
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Domain User entities
    public DbSet<User> Users { get; set; }

    // Auth-related DbSets
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    // RBAC DbSets
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Core.Entities.UserRole> UserRoles { get; set; }

    // Course & Learning DbSets (007-database-schema)
    public DbSet<Category> Categories { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Core.Entities.LearningTask> Tasks { get; set; }
    public DbSet<UserProgress> UserProgress { get; set; }
    public DbSet<LessonCompletion> LessonCompletions { get; set; }
    public DbSet<UserCourseEnrollment> UserCourseEnrollments { get; set; }
    public DbSet<UserTaskSubmission> UserTaskSubmissions { get; set; }
    public DbSet<RewardTransaction> RewardTransactions { get; set; }
    public DbSet<DiscountCode> DiscountCodes { get; set; }
    public DbSet<UserDiscountRedemption> UserDiscountRedemptions { get; set; }

    // Reward System DbSets (006-reward-system)
    public DbSet<UserStreak> UserStreaks { get; set; }
    public DbSet<UserAchievement> UserAchievements { get; set; }
    public DbSet<ReferralAttribution> ReferralAttributions { get; set; }

    // Stripe Subscription DbSets (008-stripe-subscription)
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<CurrencyPricing> CurrencyPricings { get; set; }
    public DbSet<WebhookEvent> WebhookEvents { get; set; }
    public DbSet<SubscriptionHistory> SubscriptionHistories { get; set; }

    // Admin Dashboard DbSets (009-admin-dashboard)
    public DbSet<AuditLogEntry> AuditLogEntries { get; set; }
    public DbSet<UserNotification> UserNotifications { get; set; }
    public DbSet<PointAdjustment> PointAdjustments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all configurations from assembly (including domain User and RBAC entities)
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure custom entities (not using IEntityTypeConfiguration pattern)
        ConfigureUser(builder);
        ConfigureRefreshToken(builder);
        ConfigureEmailVerificationToken(builder);
        ConfigurePasswordResetToken(builder);

        // Subscription entities
        builder.Entity<CurrencyPricing>().ToTable("currency_pricings");
        builder.Entity<Subscription>().ToTable("subscriptions");
        builder.Entity<SubscriptionHistory>().ToTable("subscription_histories");
        builder.Entity<WebhookEvent>().ToTable("webhook_events");

        // Configure 007-database-schema entities
        ConfigureCourseEntities(builder);
        ConfigureProgressEntities(builder);
        ConfigureRewardEntities(builder);
    }

    private static void ConfigureUser(ModelBuilder builder)
    {
        builder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Email)
                .HasMaxLength(320)
                .IsRequired();

            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(Core.Enums.UserRoleEnum.Free);

            entity.Property(e => e.EmailConfirmed)
                .HasDefaultValue(false);

            entity.Property(e => e.FailedLoginAttempts)
                .HasDefaultValue(0);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Indexes for performance
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Role);
            entity.HasIndex(e => e.EmailConfirmed);

            // T112: Composite index for leaderboard queries (total points DESC, ID)
            entity.HasIndex(e => new { e.TotalPointsEarned, e.Id })
                .HasDatabaseName("IX_Users_Leaderboard")
                .IsDescending(true, false); // TotalPointsEarned DESC, Id ASC

            // Soft delete query filter
            entity.HasQueryFilter(u => !u.IsDeleted);
        });
    }

    private static void ConfigureRefreshToken(ModelBuilder builder)
    {
        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.ExpiresAt)
                .IsRequired();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiresAt);
        });
    }

    private static void ConfigureEmailVerificationToken(ModelBuilder builder)
    {
        builder.Entity<EmailVerificationToken>(entity =>
        {
            entity.ToTable("email_verification_tokens");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.ExpiresAt)
                .IsRequired();

            entity.Property(e => e.IsUsed)
                .HasDefaultValue(false);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => e.IsUsed);
            entity.HasIndex(e => new { e.UserId, e.IsUsed, e.ExpiresAt });
        });
    }

    private static void ConfigurePasswordResetToken(ModelBuilder builder)
    {
        builder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("password_reset_tokens");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.ExpiresAt)
                .IsRequired();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiresAt);
        });
    }

    private static void ConfigureCourseEntities(ModelBuilder builder)
    {
        // Category configuration
        builder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasIndex(e => e.DisplayOrder);
            entity.HasIndex(e => e.IsActive);

            // Soft delete query filter
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Course configuration
        builder.Entity<Course>(entity =>
        {
            entity.ToTable("courses");
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsPublished);
            entity.HasIndex(e => e.IsPremium);
            entity.HasIndex(e => e.DifficultyLevel);
            // Composite index for multi-column queries (T170 - Performance optimization)
            entity.HasIndex(e => new { e.IsPublished, e.CategoryId, e.DifficultyLevel, e.IsPremium });

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Courses)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany(u => u.CreatedCourses)
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(e => e.DifficultyLevel)
                .HasConversion<string>();

            // Soft delete query filter (Course inherits from SoftDeletableEntity)
            entity.HasQueryFilter(e => e.IsActive);
        });

        // Lesson configuration
        builder.Entity<Lesson>(entity =>
        {
            entity.ToTable("lessons");
            entity.HasIndex(e => e.CourseId);
            entity.HasIndex(e => e.OrderIndex);

            entity.HasOne(e => e.Course)
                .WithMany(c => c.Lessons)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Map PascalCase properties to snake_case columns for PostgreSQL
            entity.Property(e => e.VideoDuration).HasColumnName("video_duration");

            // Soft delete query filter (Lesson inherits from SoftDeletableEntity)
            entity.HasQueryFilter(e => e.IsActive);
        });

        // LearningTask configuration (renamed from Task to avoid ambiguity)
        builder.Entity<Core.Entities.LearningTask>(entity =>
        {
            entity.ToTable("learning_tasks");
            entity.HasIndex(e => e.LessonId);
            entity.HasIndex(e => e.TaskType);

            entity.HasOne(e => e.Lesson)
                .WithMany(l => l.Tasks)
                .HasForeignKey(e => e.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.TaskType)
                .HasConversion<string>();

            entity.Property(e => e.TaskData)
                .HasColumnType("jsonb");
        });
    }

    private static void ConfigureProgressEntities(ModelBuilder builder)
    {
        // UserProgress configuration
        builder.Entity<UserProgress>(entity =>
        {
            entity.ToTable("user_progress");
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
        builder.Entity<UserCourseEnrollment>(entity =>
        {
            entity.ToTable("user_course_enrollments");
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
        builder.Entity<UserTaskSubmission>(entity =>
        {
            entity.ToTable("user_task_submissions");
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.TaskId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.SubmittedAt);

            entity.HasOne(e => e.User)
                .WithMany(u => u.TaskSubmissions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Task)
                .WithMany(t => t.Submissions)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ReviewedBy)
                .WithMany()
                .HasForeignKey(e => e.ReviewedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(e => e.Status)
                .HasConversion<string>();

            entity.Property(e => e.SubmissionData)
                .HasColumnType("jsonb");
        });
    }

    private static void ConfigureRewardEntities(ModelBuilder builder)
    {
        // RewardTransaction configuration
        builder.Entity<RewardTransaction>(entity =>
        {
            entity.ToTable("reward_transactions");
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.TransactionType);

            entity.HasOne(e => e.User)
                .WithMany(u => u.RewardTransactions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.TransactionType)
                .HasConversion<string>();
        });

        // DiscountCode configuration
        builder.Entity<DiscountCode>(entity =>
        {
            entity.ToTable("discount_codes");
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });

        // UserDiscountRedemption configuration
        builder.Entity<UserDiscountRedemption>(entity =>
        {
            entity.ToTable("user_discount_redemptions");
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