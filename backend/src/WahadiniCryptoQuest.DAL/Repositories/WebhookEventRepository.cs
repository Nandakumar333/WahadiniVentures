using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

public class WebhookEventRepository : Repository<WebhookEvent>, IWebhookEventRepository
{
    public WebhookEventRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<WebhookEvent?> GetByStripeEventIdAsync(string stripeEventId, CancellationToken cancellationToken = default)
    {
        return await _context.WebhookEvents
            .Where(we => we.StripeEventId == stripeEventId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsEventProcessedAsync(string stripeEventId, CancellationToken cancellationToken = default)
    {
        return await _context.WebhookEvents
            .Where(we => we.StripeEventId == stripeEventId)
            .AnyAsync(we => we.Status == WebhookProcessingStatus.Processed || we.Status == WebhookProcessingStatus.Duplicate, cancellationToken);
    }

    public async Task<List<WebhookEvent>> GetFailedEventsForRetryAsync(int maxRetries = 10, CancellationToken cancellationToken = default)
    {
        return await _context.WebhookEvents
            .Where(we => we.Status == WebhookProcessingStatus.Failed && we.RetryCount < maxRetries)
            .OrderBy(we => we.FailedAt)
            .Take(100) // Limit batch size
            .ToListAsync(cancellationToken);
    }

    public async Task<List<WebhookEvent>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await _context.WebhookEvents
            .Where(we => we.SubscriptionId == subscriptionId)
            .OrderByDescending(we => we.EventCreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<WebhookEvent>> GetRecentByEventTypeAsync(string eventType, int count = 100, CancellationToken cancellationToken = default)
    {
        return await _context.WebhookEvents
            .Where(we => we.EventType == eventType)
            .OrderByDescending(we => we.EventCreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<WebhookProcessingStatus, int>> GetProcessingStatsAsync(DateTime since, CancellationToken cancellationToken = default)
    {
        return await _context.WebhookEvents
            .Where(we => we.CreatedAt >= since)
            .GroupBy(we => we.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }
}
