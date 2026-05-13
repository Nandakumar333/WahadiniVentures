using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Discount;

namespace WahadiniCryptoQuest.Service.Discount.Queries;

/// <summary>
/// Query to get analytics summary across all discount codes
/// </summary>
public class GetAnalyticsSummaryQuery : IRequest<AnalyticsSummaryDto>
{
}
