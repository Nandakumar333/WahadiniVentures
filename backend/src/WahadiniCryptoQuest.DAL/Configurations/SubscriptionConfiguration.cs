using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.DAL.Configurations;

/// <summary>
/// EF Core configuration for Subscription entity
/// Implements referential integrity and proper indexing for Stripe integration
/// </summary>
public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        // Table Name
        builder.ToTable("Subscriptions");

        // Primary Key
        builder.HasKey(s => s.Id);

        // Properties
        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.StripeCustomerId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.StripeSubscriptionId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.StripePriceId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.Tier)
            .IsRequired();

        builder.Property(s => s.Status)
            .IsRequired();

        builder.Property(s => s.CurrencyCode)
            .IsRequired()
            .HasMaxLength(3); // ISO 4217 (USD, INR, EUR, JPY, GBP)

        builder.Property(s => s.CurrentPeriodStart)
            .IsRequired();

        builder.Property(s => s.CurrentPeriodEnd)
            .IsRequired();

        builder.Property(s => s.IsCancelledAtPeriodEnd)
            .IsRequired();

        builder.Property(s => s.CancelledAt)
            .IsRequired(false);

        // Audit Fields (inherited from BaseEntity)
        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(s => s.UpdatedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(s => s.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes for performance
        builder.HasIndex(s => s.UserId)
            .HasDatabaseName("IX_Subscriptions_UserId");

        builder.HasIndex(s => s.StripeSubscriptionId)
            .IsUnique()
            .HasDatabaseName("IX_Subscriptions_StripeSubscriptionId");

        builder.HasIndex(s => s.StripeCustomerId)
            .HasDatabaseName("IX_Subscriptions_StripeCustomerId");

        builder.HasIndex(s => s.Status)
            .HasDatabaseName("IX_Subscriptions_Status");

        builder.HasIndex(s => new { s.UserId, s.Status })
            .HasDatabaseName("IX_Subscriptions_UserId_Status");

        // Index for checking cancellations
        builder.HasIndex(s => new { s.IsCancelledAtPeriodEnd, s.CurrentPeriodEnd })
            .HasDatabaseName("IX_Subscriptions_CancelAtPeriodEnd_PeriodEnd")
            .HasFilter("[IsCancelledAtPeriodEnd] = 1"); // Partial index for scheduled cancellations

        // Foreign Key Relationship
        builder.HasOne(s => s.User)
            .WithMany() // User doesn't need collection navigation
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes - subscriptions should be archived
    }
}
