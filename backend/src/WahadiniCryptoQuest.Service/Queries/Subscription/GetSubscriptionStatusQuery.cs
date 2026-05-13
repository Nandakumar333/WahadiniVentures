using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Subscription;

namespace WahadiniCryptoQuest.Service.Queries.Subscription;

/// <summary>
/// Query to get user's subscription status
/// </summary>
public record GetSubscriptionStatusQuery : IRequest<SubscriptionStatusDto?>
{
    public Guid UserId { get; init; }
}
