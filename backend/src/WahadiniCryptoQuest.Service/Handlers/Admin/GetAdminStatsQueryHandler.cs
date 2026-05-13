using MediatR;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.DTOs.Admin;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;

namespace WahadiniCryptoQuest.Service.Handlers.Admin;

/// <summary>
/// Handler for retrieving admin dashboard statistics
/// Aggregates platform-wide metrics: users, subscriptions, revenue, tasks (US1)
/// </summary>
public class GetAdminStatsQueryHandler : IRequestHandler<Queries.Admin.GetAdminStatsQuery, AdminStatsDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserTaskSubmissionRepository _taskSubmissionRepository;

    public GetAdminStatsQueryHandler(
        IUserRepository userRepository,
        ISubscriptionRepository subscriptionRepository,
        IUserTaskSubmissionRepository taskSubmissionRepository)
    {
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
        _taskSubmissionRepository = taskSubmissionRepository;
    }

    public async Task<AdminStatsDto> Handle(Queries.Admin.GetAdminStatsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var thirtyDaysAgo = now.AddDays(-30);

        // Get total user count
        var totalUsers = await _userRepository.CountAsync(u => true);

        // Get active subscribers count (Active status)
        var activeSubscribers = await _subscriptionRepository.CountAsync(
            s => s.Status == SubscriptionStatus.Active);

        // Calculate Monthly Recurring Revenue
        // Note: This is a simplified calculation. In production, you would:
        // 1. Look up actual prices from CurrencyPricing based on StripePriceId
        // 2. Convert all currencies to a base currency (e.g., USD)
        // 3. Handle different billing intervals (monthly vs yearly normalized)
        // For MVP, we'll return 0 and implement proper calculation in future iteration
        decimal monthlyRecurringRevenue = 0; // TODO: Implement price lookup from CurrencyPricing

        // Get pending tasks count (Pending status)
        var pendingTasks = await _taskSubmissionRepository.CountAsync(
            t => t.Status == SubmissionStatus.Pending);

        // Generate 30-day user growth trend (daily new user signups)
        var recentUsers = await _userRepository.FindAsync(u => u.CreatedAt >= thirtyDaysAgo);
        var userGrowthTrend = recentUsers
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new ChartPointDto
            {
                Date = g.Key,
                Value = g.Count()
            })
            .OrderBy(c => c.Date)
            .ToList();

        // Fill missing dates with zero counts
        userGrowthTrend = FillMissingDates(userGrowthTrend, thirtyDaysAgo, now);

        // Generate 30-day revenue trend
        // For MVP, return zero values - implement actual revenue tracking in future iteration
        var revenueTrend = FillMissingDates(new List<ChartPointDto>(), thirtyDaysAgo, now);

        return new AdminStatsDto
        {
            TotalUsers = totalUsers,
            ActiveSubscribers = activeSubscribers,
            MonthlyRecurringRevenue = monthlyRecurringRevenue,
            PendingTasks = pendingTasks,
            RevenueTrend = revenueTrend,
            UserGrowthTrend = userGrowthTrend
        };
    }

    /// <summary>
    /// Fills missing dates in trend data with zero values for continuous chart display
    /// </summary>
    private List<ChartPointDto> FillMissingDates(List<ChartPointDto> data, DateTime startDate, DateTime endDate)
    {
        var result = new List<ChartPointDto>();
        var currentDate = startDate.Date;
        var endDateOnly = endDate.Date;

        while (currentDate <= endDateOnly)
        {
            var existingPoint = data.FirstOrDefault(d => d.Date.Date == currentDate);
            result.Add(existingPoint ?? new ChartPointDto
            {
                Date = currentDate,
                Value = 0
            });

            currentDate = currentDate.AddDays(1);
        }

        return result;
    }
}
