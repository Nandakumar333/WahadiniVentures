using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Interfaces;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Commands.Subscription;

namespace WahadiniCryptoQuest.Service.Handlers.Subscription;

/// <summary>
/// Handler for creating Stripe billing portal sessions
/// </summary>
public class CreatePortalSessionHandler : IRequestHandler<CreatePortalSessionCommand, PortalSessionResponseDto>
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ILogger<CreatePortalSessionHandler> _logger;

    public CreatePortalSessionHandler(
        IPaymentGateway paymentGateway,
        ISubscriptionRepository subscriptionRepository,
        ILogger<CreatePortalSessionHandler> logger)
    {
        _paymentGateway = paymentGateway;
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
    }

    public async Task<PortalSessionResponseDto> Handle(CreatePortalSessionCommand request, CancellationToken cancellationToken)
    {
        // Get user's active subscription
        var subscription = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(request.UserId, cancellationToken);
        if (subscription == null)
        {
            _logger.LogWarning("No active subscription found for user {UserId}", request.UserId);
            throw new InvalidOperationException("No active subscription found");
        }

        if (string.IsNullOrEmpty(subscription.StripeCustomerId))
        {
            _logger.LogError("Subscription {SubscriptionId} has no Stripe customer ID", subscription.Id);
            throw new InvalidOperationException("Invalid subscription data");
        }

        // Create portal session via payment gateway
        var portalUrl = await _paymentGateway.CreatePortalSessionAsync(
            subscription.StripeCustomerId,
            request.ReturnUrl
        );

        _logger.LogInformation("Created billing portal session for user {UserId}", request.UserId);

        return new PortalSessionResponseDto
        {
            PortalUrl = portalUrl
        };
    }
}
