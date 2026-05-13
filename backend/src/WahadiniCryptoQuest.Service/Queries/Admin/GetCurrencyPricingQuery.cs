using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Subscription;

namespace WahadiniCryptoQuest.Service.Queries.Admin;

/// <summary>
/// Query to get a single currency pricing by ID
/// Admin-only operation (US5)
/// </summary>
public class GetCurrencyPricingQuery : IRequest<CurrencyPricingDto?>
{
    public Guid Id { get; set; }
}
