using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Commands.Subscription;

namespace WahadiniCryptoQuest.Service.Handlers.Subscription;

/// <summary>
/// Handler for canceling subscriptions at period end
/// </summary>
public class CancelSubscriptionHandler : IRequestHandler<CancelSubscriptionCommand, bool>
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionHistoryRepository _historyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelSubscriptionHandler> _logger;

    public CancelSubscriptionHandler(
        IPaymentGateway paymentGateway,
        ISubscriptionRepository subscriptionRepository,
        ISubscriptionHistoryRepository historyRepository,
        IUnitOfWork unitOfWork,
        ILogger<CancelSubscriptionHandler> logger)
    {
        _paymentGateway = paymentGateway;
        _subscriptionRepository = subscriptionRepository;
        _historyRepository = historyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // Get user's active subscription
        var subscription = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(request.UserId, cancellationToken);
        if (subscription == null)
        {
            _logger.LogWarning("No active subscription found for user {UserId}", request.UserId);
            throw new InvalidOperationException("No active subscription found");
        }

        // Check if already cancelled
        if (subscription.IsCancelledAtPeriodEnd)
        {
            _logger.LogInformation("Subscription {SubscriptionId} is already cancelled", subscription.Id);
            return true;
        }

        // Cancel in Stripe
        await _paymentGateway.CancelSubscriptionAsync(subscription.StripeSubscriptionId, cancelAtPeriodEnd: true);

        // Update subscription record
        subscription.CancelAtPeriodEnd();

        // Record cancellation in history
        var history = SubscriptionHistory.RecordCancellation(
            subscription.Id,
            subscription.Status,
            subscription.CurrentPeriodEnd,
            "User",
            request.Reason
        );
        await _historyRepository.AddAsync(history);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cancelled subscription {SubscriptionId} for user {UserId} at period end",
            subscription.Id, request.UserId);

        return true;
    }
}
