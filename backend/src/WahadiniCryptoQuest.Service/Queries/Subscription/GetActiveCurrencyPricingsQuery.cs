using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Subscription;

namespace WahadiniCryptoQuest.Service.Queries.Subscription;

/// <summary>
/// Query to get all active currency pricings for display
/// </summary>
public record GetActiveCurrencyPricingsQuery : IRequest<List<CurrencyPricingDto>>
{
}
