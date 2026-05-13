using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Discount;

namespace WahadiniCryptoQuest.Service.Discount.Queries;

/// <summary>
/// Query to get analytics for a specific discount code
/// </summary>
public class GetDiscountAnalyticsQuery : IRequest<DiscountAnalyticsDto>
{
    public Guid DiscountCodeId { get; set; }
}
