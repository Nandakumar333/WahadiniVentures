using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Discount;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Discount.Queries;

/// <summary>
/// Handler for getting analytics summary across all discounts
/// </summary>
public class GetAnalyticsSummaryQueryHandler : IRequestHandler<GetAnalyticsSummaryQuery, AnalyticsSummaryDto>
{
    private readonly IDiscountService _discountService;
    private readonly ILogger<GetAnalyticsSummaryQueryHandler> _logger;

    public GetAnalyticsSummaryQueryHandler(
        IDiscountService discountService,
        ILogger<GetAnalyticsSummaryQueryHandler> logger)
    {
        _discountService = discountService;
        _logger = logger;
    }

    public async Task<AnalyticsSummaryDto> Handle(GetAnalyticsSummaryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting analytics summary for all discount codes");

        // Pass null to get summary for all discounts
        var summaryAnalytics = await _discountService.GetDiscountAnalyticsAsync(null, cancellationToken);

        // Transform single discount analytics to summary format
        var summary = new AnalyticsSummaryDto
        {
            TotalRedemptions = summaryAnalytics.TotalRedemptions,
            TotalPointsBurned = summaryAnalytics.TotalPointsBurned,
            UniqueRedeemingUsers = summaryAnalytics.UniqueUsers,
            EarliestRedemptionDate = summaryAnalytics.FirstRedemptionDate,
            LatestRedemptionDate = summaryAnalytics.LastRedemptionDate
        };

        _logger.LogInformation(
            "Retrieved analytics summary: {Redemptions} total redemptions, {Points} points burned",
            summary.TotalRedemptions, summary.TotalPointsBurned);

        return summary;
    }
}
