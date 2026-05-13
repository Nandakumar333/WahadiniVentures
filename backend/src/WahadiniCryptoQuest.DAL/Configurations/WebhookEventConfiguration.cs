using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.DAL.Configurations;

/// <summary>
/// EF Core configuration for WebhookEvent entity
/// Implements idempotency and audit trail for Stripe webhook processing
/// </summary>
public class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
{
    public void Configure(EntityTypeBuilder<WebhookEvent> builder)
    {
        // Table Name
        builder.ToTable("WebhookEvents");

        // Primary Key
        builder.HasKey(we => we.Id);

        // Properties
        builder.Property(we => we.StripeEventId)
            .IsRequired()
            .HasMaxLength(255); // evt_*

        builder.Property(we => we.EventType)
            .IsRequired()
            .HasMaxLength(100); // checkout.session.completed, customer.subscription.updated, etc.

        builder.Property(we => we.EventCreatedAt)
            .IsRequired();

        builder.Property(we => we.Status)
            .IsRequired();

        builder.Property(we => we.ProcessedAt)
            .IsRequired(false);

        builder.Property(we => we.FailedAt)
            .IsRequired(false);

        builder.Property(we => we.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(we => we.ErrorMessage)
            .IsRequired(false)
            .HasMaxLength(2000); // Truncated error messages

        builder.Property(we => we.PayloadJson)
            .IsRequired()
            .HasColumnType("text"); // Full JSON payload for audit

        builder.Property(we => we.SubscriptionId)
            .IsRequired(false);

        // Audit Fields (inherited from BaseEntity)
        builder.Property(we => we.CreatedAt)
            .IsRequired();

        builder.Property(we => we.UpdatedAt)
            .IsRequired();

        builder.Property(we => we.CreatedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(we => we.UpdatedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(we => we.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes for idempotency and monitoring
        builder.HasIndex(we => we.StripeEventId)
            .IsUnique()
            .HasDatabaseName("IX_WebhookEvents_StripeEventId"); // Critical for idempotency

        builder.HasIndex(we => we.Status)
            .HasDatabaseName("IX_WebhookEvents_Status");

        builder.HasIndex(we => new { we.EventType, we.EventCreatedAt })
            .HasDatabaseName("IX_WebhookEvents_EventType_CreatedAt");

        builder.HasIndex(we => we.SubscriptionId)
            .HasDatabaseName("IX_WebhookEvents_SubscriptionId")
            .HasFilter("[SubscriptionId] IS NOT NULL"); // Partial index

        builder.HasIndex(we => new { we.Status, we.RetryCount })
            .HasDatabaseName("IX_WebhookEvents_Status_RetryCount")
            .HasFilter("[Status] = 3"); // Failed events only (for retry monitoring)

        // Foreign Key Relationship (optional)
        builder.HasOne(we => we.Subscription)
            .WithMany()
            .HasForeignKey(we => we.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict) // Prevent cascading deletes
            .IsRequired(false);
    }
}
