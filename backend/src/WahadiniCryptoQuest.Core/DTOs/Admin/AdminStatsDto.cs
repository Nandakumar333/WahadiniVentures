using System.Collections.Generic;

namespace WahadiniCryptoQuest.Core.DTOs.Admin;

/// <summary>
/// Admin dashboard statistics DTO containing platform health metrics
/// </summary>
public class AdminStatsDto
{
    /// <summary>
    /// Total number of registered users
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Number of users with active Premium subscriptions
    /// </summary>
    public int ActiveSubscribers { get; set; }

    /// <summary>
    /// Monthly Recurring Revenue from active subscriptions
    /// </summary>
    public decimal MonthlyRecurringRevenue { get; set; }

    /// <summary>
    /// Number of task submissions awaiting review
    /// </summary>
    public int PendingTasks { get; set; }

    /// <summary>
    /// 30-day revenue trend data points
    /// </summary>
    public List<ChartPointDto> RevenueTrend { get; set; } = new();

    /// <summary>
    /// 30-day user growth trend data points
    /// </summary>
    public List<ChartPointDto> UserGrowthTrend { get; set; } = new();
}
