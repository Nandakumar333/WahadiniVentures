using AutoMapper;
using WahadiniCryptoQuest.Core.DTOs.Admin;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Service.Mappings;

/// <summary>
/// AutoMapper profile for admin dashboard entities and DTOs.
/// Maps audit logs, notifications, and point adjustments.
/// T016: Admin dashboard mapping profile
/// </summary>
public class AdminMappingProfile : Profile
{
    public AdminMappingProfile()
    {
        // AuditLogEntry -> AuditLogDto
        CreateMap<AuditLogEntry, AuditLogDto>()
            .ForMember(dest => dest.AdminUserEmail, opt => opt.Ignore()) // Set manually in service
            .ForMember(dest => dest.AdminUserName, opt => opt.Ignore()); // Set manually in service

        // UserNotification -> UserNotificationDto
        CreateMap<UserNotification, UserNotificationDto>();

        // PointAdjustment -> PointAdjustmentDto (if needed later)
    }
}
