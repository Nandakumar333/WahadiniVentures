using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.DAL.Configurations;

public class UserAchievementConfiguration : IEntityTypeConfiguration<UserAchievement>
{
    public void Configure(EntityTypeBuilder<UserAchievement> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AchievementId)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(x => x.User)
            .WithMany(u => u.Achievements)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ensure user can't have duplicate achievement records
        builder.HasIndex(x => new { x.UserId, x.AchievementId }).IsUnique();
    }
}
