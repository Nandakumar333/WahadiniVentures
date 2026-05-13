using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Admin;

namespace WahadiniCryptoQuest.Service.Queries.Admin;

/// <summary>
/// Query to retrieve comprehensive admin dashboard statistics
/// Admin/SuperAdmin only operation (US1 - Platform Health Overview)
/// </summary>
public class GetAdminStatsQuery : IRequest<AdminStatsDto>
{
    // No parameters needed - retrieves current platform-wide statistics
}
