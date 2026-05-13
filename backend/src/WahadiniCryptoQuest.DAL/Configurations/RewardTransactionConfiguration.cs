using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.DAL.Configurations;

public class RewardTransactionConfiguration : IEntityTypeConfiguration<RewardTransaction>
{
    public void Configure(EntityTypeBuilder<RewardTransaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ReferenceId)
            .HasMaxLength(100);

        builder.Property(x => x.ReferenceType)
            .HasMaxLength(50);

        builder.HasOne(x => x.User)
            .WithMany(u => u.RewardTransactions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for faster history queries
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.CreatedAt);

        // Deduplication index for idempotency (UserId + ReferenceId + ReferenceType must be unique)
        builder.HasIndex(x => new { x.UserId, x.ReferenceId, x.ReferenceType })
            .IsUnique()
            .HasFilter("\"ReferenceId\" IS NOT NULL AND \"ReferenceType\" IS NOT NULL");
    }
}
