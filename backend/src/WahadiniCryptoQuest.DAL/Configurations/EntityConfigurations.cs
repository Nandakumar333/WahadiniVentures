using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.DAL.Configurations;

/// <summary>
/// Entity Framework configuration for User entity
/// Defines schema, constraints, and relationships for the users table
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(320); // RFC 5321 max email length

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(WahadiniCryptoQuest.Core.Enums.UserRoleEnum.Free);

        builder.Property(u => u.EmailConfirmed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.FailedLoginAttempts)
            .HasDefaultValue(0);

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.CreatedBy)
            .HasMaxLength(100)
            .HasDefaultValue("System");

        builder.Property(u => u.UpdatedBy)
            .HasMaxLength(100)
            .HasDefaultValue("System");

        builder.Property(u => u.IsDeleted)
            .HasDefaultValue(false);

        // Reward system properties
        builder.Property(u => u.CurrentPoints)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.TotalPointsEarned)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.ReferralCode)
            .HasMaxLength(6);

        // Concurrency token for optimistic locking on point operations
        builder.Property(u => u.RowVersion)
            .IsRowVersion();

        // Indexes for performance
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("idx_users_email");

        builder.HasIndex(u => u.CreatedAt)
            .HasDatabaseName("idx_users_created_at");

        builder.HasIndex(u => u.Role)
            .HasDatabaseName("idx_users_role");

        builder.HasIndex(u => u.EmailConfirmed)
            .HasDatabaseName("idx_users_email_confirmed");

        builder.HasIndex(u => u.IsDeleted)
            .HasDatabaseName("idx_users_is_deleted");
    }
}

/// <summary>
/// Entity Framework configuration for EmailVerificationToken entity
/// </summary>
public class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.ToTable("email_verification_tokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(t => t.ExpiresAt)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(t => t.Token)
            .HasDatabaseName("idx_email_verification_tokens_token");

        builder.HasIndex(t => new { t.UserId, t.IsUsed })
            .HasDatabaseName("idx_email_verification_tokens_user_used");
    }
}

/// <summary>
/// Entity Framework configuration for Role entity
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(r => r.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(r => r.Name)
            .IsUnique()
            .HasDatabaseName("idx_roles_name");
    }
}

/// <summary>
/// Entity Framework configuration for Permission entity
/// </summary>
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Resource)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(p => p.Name)
            .IsUnique()
            .HasDatabaseName("idx_permissions_name");

        builder.HasIndex(p => new { p.Resource, p.Action })
            .HasDatabaseName("idx_permissions_resource_action");
    }
}

/// <summary>
/// Entity Framework configuration for RolePermission junction entity
/// </summary>
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");

        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.RoleId)
            .IsRequired();

        builder.Property(rp => rp.PermissionId)
            .IsRequired();

        builder.Property(rp => rp.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(rp => rp.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(rp => rp.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId })
            .IsUnique()
            .HasDatabaseName("idx_role_permissions_role_permission");

        builder.HasIndex(rp => rp.IsActive)
            .HasDatabaseName("idx_role_permissions_active");
    }
}

/// <summary>
/// Entity Framework configuration for UserRole junction entity
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");

        builder.HasKey(ur => ur.Id);

        builder.Property(ur => ur.UserId)
            .IsRequired();

        builder.Property(ur => ur.RoleId)
            .IsRequired();

        builder.Property(ur => ur.AssignedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(ur => ur.ExpiresAt)
            .IsRequired(false);

        builder.Property(ur => ur.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ur => ur.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(ur => ur.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
            .HasDatabaseName("idx_user_roles_user_role");

        builder.HasIndex(ur => ur.ExpiresAt)
            .HasDatabaseName("idx_user_roles_expires_at");

        builder.HasIndex(ur => ur.IsActive)
            .HasDatabaseName("idx_user_roles_active");
    }
}

/// <summary>
/// Entity Framework configuration for UserProgress entity
/// Configures video progress tracking with composite index and time-based partitioning support
/// </summary>
public class UserProgressConfiguration : IEntityTypeConfiguration<UserProgress>
{
    public void Configure(EntityTypeBuilder<UserProgress> builder)
    {
        builder.ToTable("user_progress");

        builder.HasKey(up => up.Id);

        builder.Property(up => up.UserId)
            .IsRequired();

        builder.Property(up => up.LessonId)
            .IsRequired();

        builder.Property(up => up.LastWatchedPosition)
            .IsRequired();

        builder.Property(up => up.CompletionPercentage)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(up => up.TotalWatchTime)
            .IsRequired();

        builder.Property(up => up.IsCompleted)
            .IsRequired();

        builder.Property(up => up.CompletedAt)
            .IsRequired(false);

        builder.Property(up => up.RewardPointsClaimed)
            .IsRequired();

        builder.Property(up => up.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(up => up.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(up => up.User)
            .WithMany(u => u.Progress)
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(up => up.Lesson)
            .WithMany(l => l.UserProgress)
            .HasForeignKey(up => up.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        // Composite index for efficient queries by user and lesson
        builder.HasIndex(up => new { up.UserId, up.LessonId })
            .IsUnique()
            .HasDatabaseName("idx_user_progress_user_lesson");

        // Index for time-based partitioning queries
        builder.HasIndex(up => up.UpdatedAt)
            .HasDatabaseName("idx_user_progress_updated_at");

        // Index for completion tracking
        builder.HasIndex(up => up.IsCompleted)
            .HasDatabaseName("idx_user_progress_completed");
    }
}

/// <summary>
/// Entity Framework configuration for LessonCompletion entity
/// Configures immutable audit records for lesson completion events
/// </summary>
public class LessonCompletionConfiguration : IEntityTypeConfiguration<LessonCompletion>
{
    public void Configure(EntityTypeBuilder<LessonCompletion> builder)
    {
        builder.ToTable("lesson_completions");

        builder.HasKey(lc => lc.Id);

        builder.Property(lc => lc.UserId)
            .IsRequired();

        builder.Property(lc => lc.LessonId)
            .IsRequired();

        builder.Property(lc => lc.CompletedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(lc => lc.PointsAwarded)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(lc => lc.CompletionPercentage)
            .IsRequired()
            .HasPrecision(5, 2)
            .HasDefaultValue(0);

        builder.Property(lc => lc.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(lc => lc.User)
            .WithMany()
            .HasForeignKey(lc => lc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(lc => lc.Lesson)
            .WithMany()
            .HasForeignKey(lc => lc.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        // Composite index to prevent duplicate completions
        builder.HasIndex(lc => new { lc.UserId, lc.LessonId })
            .IsUnique()
            .HasDatabaseName("idx_lesson_completions_user_lesson");

        // Index for time-based queries
        builder.HasIndex(lc => lc.CompletedAt)
            .HasDatabaseName("idx_lesson_completions_completed_at");
    }
}
