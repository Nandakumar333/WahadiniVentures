using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Admin;

namespace WahadiniCryptoQuest.Service.Queries.Admin;

/// <summary>
/// Query to retrieve detailed user information
/// T063: US3 - User Account Management
/// </summary>
public class GetUserByIdQuery : IRequest<UserDetailDto?>
{
    public Guid UserId { get; set; }
}
