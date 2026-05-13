using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.DAL.Configurations;

public class ReferralAttributionConfiguration : IEntityTypeConfiguration<ReferralAttribution>
{
    public void Configure(EntityTypeBuilder<ReferralAttribution> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Referrer)
            .WithMany(u => u.ReferralsMade)
            .HasForeignKey(x => x.ReferrerId)
            .OnDelete(DeleteBehavior.Restrict); // Don't delete referral record if referrer is deleted

        builder.HasOne(x => x.ReferredUser)
            .WithOne(u => u.ReferredBy)
            .HasForeignKey<ReferralAttribution>(x => x.ReferredUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // A user can only be referred once
        builder.HasIndex(x => x.ReferredUserId).IsUnique();
    }
}
