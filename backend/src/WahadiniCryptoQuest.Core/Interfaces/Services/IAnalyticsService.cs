using System.Threading.Tasks;
using WahadiniCryptoQuest.Core.DTOs.Admin;

namespace WahadiniCryptoQuest.Core.Interfaces.Services;

/// <summary>
/// Service for aggregating and analyzing platform-wide metrics
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Retrieves comprehensive dashboard statistics including user counts,
    /// subscription metrics, revenue data, and trend analysis
    /// </summary>
    /// <returns>Admin dashboard statistics DTO</returns>
    Task<AdminStatsDto> GetDashboardStatsAsync();
}
