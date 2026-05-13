using MediatR;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Service.Commands.Admin;

/// <summary>
/// Command to update a user's role
/// T064: US3 - User Account Management
/// </summary>
public class UpdateUserRoleCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public UserRoleEnum NewRole { get; set; }
    public string? Reason { get; set; }
    public Guid AdminUserId { get; set; }
}
