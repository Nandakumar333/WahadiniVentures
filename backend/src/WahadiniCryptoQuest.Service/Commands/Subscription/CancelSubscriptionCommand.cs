using MediatR;

namespace WahadiniCryptoQuest.Service.Commands.Subscription;

/// <summary>
/// Command to cancel a subscription at period end
/// </summary>
public class CancelSubscriptionCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
