using MediatR;

namespace WahadiniCryptoQuest.Service.Commands.Admin;

/// <summary>
/// Command to unban a user account
/// T066: US3 - User Account Management
/// </summary>
public class UnbanUserCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public Guid AdminUserId { get; set; }
}
