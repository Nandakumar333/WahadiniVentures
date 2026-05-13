using MediatR;

namespace WahadiniCryptoQuest.Service.Commands.Admin;

/// <summary>
/// Command to ban a user account
/// T065: US3 - User Account Management
/// </summary>
public class BanUserCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid AdminUserId { get; set; }
}
