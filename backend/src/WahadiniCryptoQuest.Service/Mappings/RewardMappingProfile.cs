using AutoMapper;
using WahadiniCryptoQuest.Core.DTOs.Reward;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Service.Mappings;

/// <summary>
/// AutoMapper profile for Reward system entity mappings
/// </summary>
public class RewardMappingProfile : Profile
{
    public RewardMappingProfile()
    {
        // RewardTransaction to TransactionDto
        CreateMap<RewardTransaction, TransactionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TransactionType.ToString()))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.ReferenceId, opt => opt.MapFrom(src => src.ReferenceId))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

        // RewardTransaction to AdminTransactionHistoryDto
        CreateMap<RewardTransaction, AdminTransactionHistoryDto>()
            .IncludeBase<RewardTransaction, TransactionDto>()
            .ForMember(dest => dest.AdminUserId, opt => opt.MapFrom(src => src.AdminUserId))
            .ForMember(dest => dest.AdminName, opt => opt.MapFrom(src => src.AdminUser != null ? src.AdminUser.FullName : null));

        // User to BalanceDto (simplified - without Rank for now)
        CreateMap<User, BalanceDto>()
            .ForMember(dest => dest.CurrentPoints, opt => opt.MapFrom(src => src.CurrentPoints))
            .ForMember(dest => dest.TotalEarned, opt => opt.MapFrom(src => src.TotalPointsEarned))
            .ForMember(dest => dest.Rank, opt => opt.MapFrom(src => 0)); // Rank requires cross-user query

        // UserAchievement to AchievementDto
        CreateMap<UserAchievement, AchievementDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AchievementId))
            .ForMember(dest => dest.Name, opt => opt.Ignore()) // Populated from AchievementCatalog
            .ForMember(dest => dest.Description, opt => opt.Ignore()) // Populated from AchievementCatalog
            .ForMember(dest => dest.Category, opt => opt.Ignore()) // Populated from AchievementCatalog
            .ForMember(dest => dest.IconUrl, opt => opt.Ignore()) // Populated from AchievementCatalog
            .ForMember(dest => dest.PointBonus, opt => opt.Ignore()) // Populated from AchievementCatalog
            .ForMember(dest => dest.IsUnlocked, opt => opt.MapFrom(src => true)) // All UserAchievements are unlocked
            .ForMember(dest => dest.UnlockedAt, opt => opt.MapFrom(src => src.UnlockedAt))
            .ForMember(dest => dest.Progress, opt => opt.MapFrom(src => 100)) // Unlocked = 100%
            .ForMember(dest => dest.IsNotified, opt => opt.MapFrom(src => src.Notified));
    }
}
