using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Admin;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Service.Queries.Admin;

namespace WahadiniCryptoQuest.Service.Services;

/// <summary>
/// Service implementation for analytics and platform metrics aggregation
/// Orchestrates MediatR queries for dashboard statistics (US1)
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly IMediator _mediator;

    public AnalyticsService(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves comprehensive dashboard statistics
    /// </summary>
    public async Task<AdminStatsDto> GetDashboardStatsAsync()
    {
        return await _mediator.Send(new GetAdminStatsQuery());
    }
}
