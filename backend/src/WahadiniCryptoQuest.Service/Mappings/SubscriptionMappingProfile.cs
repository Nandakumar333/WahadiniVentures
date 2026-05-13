using AutoMapper;
using WahadiniCryptoQuest.Core.DTOs.Subscription;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Service.Mappings;

/// <summary>
/// AutoMapper profile for Subscription domain
/// Maps entities to DTOs for API responses
/// </summary>
public class SubscriptionMappingProfile : Profile
{
    public SubscriptionMappingProfile()
    {
        // Subscription entity to DTO
        CreateMap<Subscription, SubscriptionStatusDto>()
            .ForMember(dest => dest.HasPremiumAccess, opt => opt.MapFrom(src => src.HasPremiumAccess()))
            .ForMember(dest => dest.IsInGracePeriod, opt => opt.MapFrom(src => src.IsInGracePeriod()));

        // CurrencyPricing entity to DTO
        CreateMap<CurrencyPricing, CurrencyPricingDto>()
            .ForMember(dest => dest.FormattedMonthlyPrice, opt => opt.MapFrom(src => src.FormatPrice(src.MonthlyPrice)))
            .ForMember(dest => dest.FormattedYearlyPrice, opt => opt.MapFrom(src => src.FormatPrice(src.YearlyPrice)));

        // SubscriptionHistory entity to DTO
        CreateMap<SubscriptionHistory, SubscriptionHistoryDto>();
    }
}
