using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Subscription?> GetActiveSubscriptionByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .Where(s => s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.PastDue)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(s => s.StripeSubscriptionId == stripeSubscriptionId && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Subscription?> GetByStripeCustomerIdAsync(string stripeCustomerId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(s => s.StripeCustomerId == stripeCustomerId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Subscription>> GetExpiredCancellationsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Subscriptions
            .Where(s => s.IsCancelledAtPeriodEnd && s.CurrentPeriodEnd < now && !s.IsDeleted)
            .Where(s => s.Status == SubscriptionStatus.Canceled)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Subscription>> GetExpiredGracePeriodSubscriptionsAsync(int gracePeriodDays = 3, CancellationToken cancellationToken = default)
    {
        var gracePeriodEnd = DateTime.UtcNow.AddDays(-gracePeriodDays);
        return await _context.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.PastDue && !s.IsDeleted)
            .Where(s => s.UpdatedAt < gracePeriodEnd) // UpdatedAt tracks when it went PastDue
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasSubscriptionHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .AnyAsync(s => s.UserId == userId, cancellationToken);
    }

    public async Task<Subscription?> GetWithHistoryAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(s => s.Id == subscriptionId && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
