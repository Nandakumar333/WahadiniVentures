using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Admin;

namespace WahadiniCryptoQuest.Service.Queries.Admin;

/// <summary>
/// Query to get discount codes with status filter
/// T133: US5 - Reward System Management
/// </summary>
public class GetDiscountCodesQuery : IRequest<List<DiscountCodeDto>>
{
    public string? StatusFilter { get; set; } // Active, Expired, FullyRedeemed
}

/// <summary>
/// Query to get redemption history for a discount code
/// T137: US5 - Reward System Management
/// </summary>
public class GetRedemptionsQuery : IRequest<List<RedemptionLogDto>>
{
    public string Code { get; set; } = string.Empty;
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
