using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.DAL.Configurations;

public class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ToTable("audit_log_entries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ActionType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ResourceType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ResourceId)
            .IsRequired()
            .HasMaxLength(100); // String for flexibility (GUIDs, integers, custom IDs)

        builder.Property(x => x.BeforeValue)
            .HasColumnType("jsonb"); // PostgreSQL JSONB for efficient querying

        builder.Property(x => x.AfterValue)
            .HasColumnType("jsonb"); // PostgreSQL JSONB for efficient querying

        builder.Property(x => x.IPAddress)
            .IsRequired()
            .HasMaxLength(45); // IPv6 max length

        builder.Property(x => x.Timestamp)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Navigation property
        builder.HasOne(x => x.AdminUser)
            .WithMany()
            .HasForeignKey(x => x.AdminUserId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

        // Composite index for common filter queries (admin audit queries)
        builder.HasIndex(x => new { x.AdminUserId, x.ActionType, x.Timestamp })
            .HasDatabaseName("IX_AuditLog_AdminAction");

        // Index for resource lookup
        builder.HasIndex(x => new { x.ResourceType, x.ResourceId })
            .HasDatabaseName("IX_AuditLog_Resource");

        // Index for timestamp-based queries (date range filtering)
        builder.HasIndex(x => x.Timestamp)
            .HasDatabaseName("IX_AuditLog_Timestamp");
    }
}
