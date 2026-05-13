using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.DAL.Configurations;

/// <summary>
/// EF Core configuration for SubscriptionHistory entity
/// Implements immutable audit trail for subscription lifecycle events
/// </summary>
public class SubscriptionHistoryConfiguration : IEntityTypeConfiguration<SubscriptionHistory>
{
    public void Configure(EntityTypeBuilder<SubscriptionHistory> builder)
    {
        // Table Name
        builder.ToTable("SubscriptionHistories");

        // Primary Key
        builder.HasKey(sh => sh.Id);

        // Properties
        builder.Property(sh => sh.SubscriptionId)
            .IsRequired();

        builder.Property(sh => sh.ChangeType)
            .IsRequired()
            .HasMaxLength(50); // Created, Activated, Renewed, Canceled, Expired, PastDue, Upgraded, Downgraded

        builder.Property(sh => sh.PreviousTier)
            .IsRequired(false);

        builder.Property(sh => sh.NewTier)
            .IsRequired(false);

        builder.Property(sh => sh.PreviousStatus)
            .IsRequired(false);

        builder.Property(sh => sh.NewStatus)
            .IsRequired(false);

        builder.Property(sh => sh.PreviousPeriodEnd)
            .IsRequired(false);

        builder.Property(sh => sh.NewPeriodEnd)
            .IsRequired(false);

        builder.Property(sh => sh.Notes)
            .IsRequired(false)
            .HasMaxLength(1000);

        builder.Property(sh => sh.TriggeredBy)
            .IsRequired(false)
            .HasMaxLength(450); // UserId, System, Stripe Webhook

        builder.Property(sh => sh.WebhookEventId)
            .IsRequired(false)
            .HasMaxLength(255);

        // Audit Fields (inherited from BaseEntity)
        builder.Property(sh => sh.CreatedAt)
            .IsRequired();

        builder.Property(sh => sh.UpdatedAt)
            .IsRequired();

        builder.Property(sh => sh.CreatedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(sh => sh.UpdatedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(sh => sh.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes for analytics and audit queries
        builder.HasIndex(sh => sh.SubscriptionId)
            .HasDatabaseName("IX_SubscriptionHistories_SubscriptionId");

        builder.HasIndex(sh => sh.ChangeType)
            .HasDatabaseName("IX_SubscriptionHistories_ChangeType");

        builder.HasIndex(sh => new { sh.SubscriptionId, sh.CreatedAt })
            .HasDatabaseName("IX_SubscriptionHistories_SubscriptionId_CreatedAt");

        builder.HasIndex(sh => sh.WebhookEventId)
            .HasDatabaseName("IX_SubscriptionHistories_WebhookEventId")
            .HasFilter("[WebhookEventId] IS NOT NULL"); // Partial index

        builder.HasIndex(sh => sh.TriggeredBy)
            .HasDatabaseName("IX_SubscriptionHistories_TriggeredBy")
            .HasFilter("[TriggeredBy] IS NOT NULL"); // Partial index

        // Foreign Key Relationship
        builder.HasOne(sh => sh.Subscription)
            .WithMany()
            .HasForeignKey(sh => sh.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict); // Never delete history
    }
}
