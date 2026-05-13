using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.DAL.Configurations;

/// <summary>
/// EF Core configuration for CurrencyPricing entity
/// Implements multi-currency pricing with admin control
/// </summary>
public class CurrencyPricingConfiguration : IEntityTypeConfiguration<CurrencyPricing>
{
    public void Configure(EntityTypeBuilder<CurrencyPricing> builder)
    {
        // Table Name
        builder.ToTable("CurrencyPricings");

        // Primary Key
        builder.HasKey(cp => cp.Id);

        // Properties
        builder.Property(cp => cp.CurrencyCode)
            .IsRequired()
            .HasMaxLength(3); // ISO 4217

        builder.Property(cp => cp.StripePriceId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(cp => cp.MonthlyPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(cp => cp.YearlyPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(cp => cp.YearlySavingsPercent)
            .IsRequired()
            .HasColumnType("decimal(5,2)"); // Percentage (e.g., 16.67%)

        builder.Property(cp => cp.CurrencySymbol)
            .IsRequired()
            .HasMaxLength(10); // $, ₹, €, ¥, £

        builder.Property(cp => cp.ShowDecimal)
            .IsRequired();

        builder.Property(cp => cp.DecimalPlaces)
            .IsRequired();

        builder.Property(cp => cp.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit Fields (inherited from BaseEntity)
        builder.Property(cp => cp.CreatedAt)
            .IsRequired();

        builder.Property(cp => cp.UpdatedAt)
            .IsRequired();

        builder.Property(cp => cp.CreatedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(cp => cp.UpdatedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(cp => cp.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(cp => cp.CurrencyCode)
            .IsUnique()
            .HasDatabaseName("IX_CurrencyPricings_CurrencyCode");

        builder.HasIndex(cp => cp.IsActive)
            .HasDatabaseName("IX_CurrencyPricings_IsActive");

        builder.HasIndex(cp => cp.StripePriceId)
            .HasDatabaseName("IX_CurrencyPricings_StripePriceId");
    }
}
