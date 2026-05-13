using AutoMapper;
using WahadiniCryptoQuest.Core.DTOs.Discount;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Service.Mappings;

/// <summary>
/// AutoMapper profile for discount-related mappings
/// </summary>
public class DiscountMappingProfile : Profile
{
    public DiscountMappingProfile()
    {
        // DiscountCode -> DiscountTypeDto (user view)
        CreateMap<DiscountCode, DiscountTypeDto>()
            .ForMember(dest => dest.CanAfford, opt => opt.Ignore()) // Calculated in handler
            .ForMember(dest => dest.CanRedeem, opt => opt.Ignore()); // Calculated in handler

        // DiscountCode -> AdminDiscountTypeDto (admin view)
        CreateMap<DiscountCode, AdminDiscountTypeDto>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

        // CreateDiscountCodeDto -> DiscountCode
        CreateMap<CreateDiscountCodeDto, DiscountCode>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentRedemptions, opt => opt.MapFrom(_ => 0))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Redemptions, opt => opt.Ignore());

        // UpdateDiscountCodeDto -> DiscountCode (partial update)
        CreateMap<UpdateDiscountCodeDto, DiscountCode>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // UserDiscountRedemption -> UserRedemptionDto
        CreateMap<UserDiscountRedemption, UserRedemptionDto>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.DiscountCode.Code))
            .ForMember(dest => dest.DiscountPercentage, opt => opt.MapFrom(src => src.DiscountCode.DiscountPercentage))
            .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.DiscountCode.ExpiryDate));
    }
}
