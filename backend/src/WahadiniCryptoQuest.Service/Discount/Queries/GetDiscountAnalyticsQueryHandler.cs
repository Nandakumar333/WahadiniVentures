using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Discount;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Discount.Queries;

/// <summary>
/// Handler for getting analytics of a specific discount code
/// </summary>
public class GetDiscountAnalyticsQueryHandler : IRequestHandler<GetDiscountAnalyticsQuery, DiscountAnalyticsDto>
{
    private readonly IDiscountService _discountService;
    private readonly ILogger<GetDiscountAnalyticsQueryHandler> _logger;

    public GetDiscountAnalyticsQueryHandler(
        IDiscountService discountService,
        ILogger<GetDiscountAnalyticsQueryHandler> logger)
    {
        _discountService = discountService;
        _logger = logger;
    }

    public async Task<DiscountAnalyticsDto> Handle(GetDiscountAnalyticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting analytics for discount code {DiscountCodeId}", request.DiscountCodeId);

        var analytics = await _discountService.GetDiscountAnalyticsAsync(request.DiscountCodeId, cancellationToken);

        _logger.LogInformation(
            "Retrieved analytics for discount {Code}: {Redemptions} redemptions, {Points} points burned",
            analytics.Code, analytics.TotalRedemptions, analytics.TotalPointsBurned);

        return analytics;
    }
}
