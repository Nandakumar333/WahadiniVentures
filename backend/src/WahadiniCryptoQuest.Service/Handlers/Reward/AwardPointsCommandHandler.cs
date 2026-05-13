using MediatR;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Service.Commands.Rewards;

namespace WahadiniCryptoQuest.Service.Handlers.Reward;

/// <summary>
/// Handler for awarding points to users with retry logic and notifications
/// T090: Notification queue integration for ≥100 point awards
/// </summary>
public class AwardPointsCommandHandler : IRequestHandler<AwardPointsCommand, Guid>
{
    private readonly IRewardService _rewardService;
    private readonly INotificationQueue _notificationQueue;
    private const int MaxRetries = 3;
    private const int LargeAwardThreshold = 100; // Points threshold for notification

    public AwardPointsCommandHandler(
        IRewardService rewardService,
        INotificationQueue notificationQueue)
    {
        _rewardService = rewardService;
        _notificationQueue = notificationQueue;
    }

    public async Task<Guid> Handle(AwardPointsCommand request, CancellationToken cancellationToken)
    {
        // Retry logic for optimistic concurrency conflicts
        int retryCount = 0;
        Guid transactionId;

        while (true)
        {
            try
            {
                transactionId = await _rewardService.AwardPointsAsync(
                    request.UserId,
                    request.Amount,
                    request.Type,
                    request.Description,
                    request.ReferenceId,
                    request.ReferenceType,
                    request.AdminUserId,
                    cancellationToken
                );
                break; // Success - exit retry loop
            }
            catch (DbUpdateConcurrencyException) when (retryCount < MaxRetries)
            {
                // Optimistic concurrency conflict on User.RowVersion - retry
                retryCount++;
                await Task.Delay(TimeSpan.FromMilliseconds(100 * retryCount), cancellationToken); // Exponential backoff
            }
        }

        // T090: Queue notification for significant point awards (≥100 points)
        if (request.Amount >= LargeAwardThreshold)
        {
            await _notificationQueue.QueueNotificationAsync(
                request.UserId,
                "Points Earned!",
                $"You've earned {request.Amount} points! {request.Description}",
                "Points",
                new Dictionary<string, string>
                {
                    { "Amount", request.Amount.ToString() },
                    { "Type", request.Type.ToString() },
                    { "TransactionId", transactionId.ToString() }
                });
        }

        return transactionId;
    }
}
