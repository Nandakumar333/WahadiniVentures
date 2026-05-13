using AutoMapper;
using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Subscription;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Queries.Subscription;

namespace WahadiniCryptoQuest.Service.Handlers.Subscription;

/// <summary>
/// Handler for getting user subscription status
/// </summary>
public class GetSubscriptionStatusHandler : IRequestHandler<GetSubscriptionStatusQuery, SubscriptionStatusDto?>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMapper _mapper;

    public GetSubscriptionStatusHandler(
        ISubscriptionRepository subscriptionRepository,
        IMapper mapper)
    {
        _subscriptionRepository = subscriptionRepository;
        _mapper = mapper;
    }

    public async Task<SubscriptionStatusDto?> Handle(GetSubscriptionStatusQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(request.UserId, cancellationToken);

        return subscription != null
            ? _mapper.Map<SubscriptionStatusDto>(subscription)
            : null;
    }
}
