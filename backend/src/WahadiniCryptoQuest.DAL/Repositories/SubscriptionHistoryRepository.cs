using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

public class SubscriptionHistoryRepository : Repository<SubscriptionHistory>, ISubscriptionHistoryRepository
{
    public SubscriptionHistoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<SubscriptionHistory>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await _context.SubscriptionHistories
            .Where(sh => sh.SubscriptionId == subscriptionId && !sh.IsDeleted)
            .OrderByDescending(sh => sh.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SubscriptionHistory>> GetByChangeTypeAsync(string changeType, DateTime since, CancellationToken cancellationToken = default)
    {
        return await _context.SubscriptionHistories
            .Where(sh => sh.ChangeType == changeType && sh.CreatedAt >= since && !sh.IsDeleted)
            .OrderByDescending(sh => sh.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SubscriptionHistory>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.SubscriptionHistories
            .Include(sh => sh.Subscription)
            .Where(sh => sh.Subscription.UserId == userId && !sh.IsDeleted)
            .OrderByDescending(sh => sh.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SubscriptionHistory>> GetByWebhookEventIdAsync(string webhookEventId, CancellationToken cancellationToken = default)
    {
        return await _context.SubscriptionHistories
            .Where(sh => sh.WebhookEventId == webhookEventId && !sh.IsDeleted)
            .OrderByDescending(sh => sh.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SubscriptionHistory>> GetCancellationsAsync(DateTime since, CancellationToken cancellationToken = default)
    {
        return await _context.SubscriptionHistories
            .Where(sh => sh.ChangeType == "Canceled" && sh.CreatedAt >= since && !sh.IsDeleted)
            .OrderByDescending(sh => sh.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SubscriptionHistory>> GetTierChangesAsync(DateTime since, CancellationToken cancellationToken = default)
    {
        return await _context.SubscriptionHistories
            .Where(sh => (sh.ChangeType == "Upgraded" || sh.ChangeType == "Downgraded") && sh.CreatedAt >= since && !sh.IsDeleted)
            .OrderByDescending(sh => sh.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
