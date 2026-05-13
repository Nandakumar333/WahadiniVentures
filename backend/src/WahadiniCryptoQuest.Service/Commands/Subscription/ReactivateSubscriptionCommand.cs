using MediatR;

namespace WahadiniCryptoQuest.Service.Commands.Subscription;

/// <summary>
/// Command to reactivate a cancelled subscription
/// </summary>
public class ReactivateSubscriptionCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
}
