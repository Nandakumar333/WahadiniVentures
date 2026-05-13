using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Commands.Subscription;

namespace WahadiniCryptoQuest.Service.Handlers.Subscription;

/// <summary>
/// Handler for reactivating cancelled subscriptions
/// </summary>
public class ReactivateSubscriptionHandler : IRequestHandler<ReactivateSubscriptionCommand, bool>
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionHistoryRepository _historyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReactivateSubscriptionHandler> _logger;

    public ReactivateSubscriptionHandler(
        IPaymentGateway paymentGateway,
        ISubscriptionRepository subscriptionRepository,
        ISubscriptionHistoryRepository historyRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReactivateSubscriptionHandler> logger)
    {
        _paymentGateway = paymentGateway;
        _subscriptionRepository = subscriptionRepository;
        _historyRepository = historyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ReactivateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // Get user's subscription
        var subscription = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(request.UserId, cancellationToken);
        if (subscription == null)
        {
            _logger.LogWarning("No active subscription found for user {UserId}", request.UserId);
            throw new InvalidOperationException("No active subscription found");
        }

        // Check if subscription is actually cancelled
        if (!subscription.IsCancelledAtPeriodEnd)
        {
            _logger.LogInformation("Subscription {SubscriptionId} is not cancelled", subscription.Id);
            return true;
        }

        // Reactivate in Stripe
        await _paymentGateway.ReactivateSubscriptionAsync(subscription.StripeSubscriptionId);

        // Update subscription record
        subscription.Reactivate();

        // Record reactivation in history
        var history = SubscriptionHistory.RecordReactivation(
            subscription.Id,
            subscription.Status,
            subscription.CurrentPeriodEnd,
            "User"
        );
        await _historyRepository.AddAsync(history);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reactivated subscription {SubscriptionId} for user {UserId}",
            subscription.Id, request.UserId);

        return true;
    }
}
