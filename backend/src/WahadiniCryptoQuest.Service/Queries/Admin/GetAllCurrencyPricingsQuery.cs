using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Subscription;

namespace WahadiniCryptoQuest.Service.Queries.Admin;

/// <summary>
/// Query to get all currency pricing configurations (including inactive)
/// Admin-only operation (US5)
/// </summary>
public class GetAllCurrencyPricingsQuery : IRequest<List<CurrencyPricingDto>>
{
    // No parameters - returns all currency pricings for admin management
}
