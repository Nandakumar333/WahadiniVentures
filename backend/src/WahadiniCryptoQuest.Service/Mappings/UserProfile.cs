using AutoMapper;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.Core.DTOs;

namespace WahadiniCryptoQuest.Service.Mappings;

/// <summary>
/// AutoMapper profile for User entity mappings
/// </summary>
public class UserProfile : Profile
{
    public UserProfile()
    {
        // ApplicationUser mappings
        CreateMap<ApplicationUser, UserDto>();

        CreateMap<RegisterDto, ApplicationUser>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
            .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore())
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => Core.Enums.UserRoleEnum.Free));

        // TODO: Add mapping for UpdateUserDto when implementing user profile update feature
    }
}