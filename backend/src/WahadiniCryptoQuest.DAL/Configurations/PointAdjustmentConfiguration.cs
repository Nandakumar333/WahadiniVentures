using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.DAL.Configurations;

public class PointAdjustmentConfiguration : IEntityTypeConfiguration<PointAdjustment>
{
    public void Configure(EntityTypeBuilder<PointAdjustment> builder)
    {
        builder.ToTable("point_adjustments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PreviousBalance)
            .IsRequired();

        builder.Property(x => x.AdjustmentAmount)
            .IsRequired();

        builder.Property(x => x.NewBalance)
            .IsRequired();

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Timestamp)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Navigation properties
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Preserve audit trail

        builder.HasOne(x => x.AdminUser)
            .WithMany()
            .HasForeignKey(x => x.AdminUserId)
            .OnDelete(DeleteBehavior.Restrict); // Preserve audit trail

        // Index for user's point adjustment history
        builder.HasIndex(x => new { x.UserId, x.Timestamp })
            .HasDatabaseName("IX_PointAdjustments_UserHistory")
            .IsDescending(false, true); // Timestamp DESC for recent first

        // Index for admin's adjustments (for admin audit)
        builder.HasIndex(x => new { x.AdminUserId, x.Timestamp })
            .HasDatabaseName("IX_PointAdjustments_AdminHistory");
    }
}
