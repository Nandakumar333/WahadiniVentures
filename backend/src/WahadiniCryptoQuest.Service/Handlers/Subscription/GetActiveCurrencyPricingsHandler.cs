using AutoMapper;
using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Subscription;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Queries.Subscription;

namespace WahadiniCryptoQuest.Service.Handlers.Subscription;

/// <summary>
/// Handler for getting all active currency pricings
/// </summary>
public class GetActiveCurrencyPricingsHandler : IRequestHandler<GetActiveCurrencyPricingsQuery, List<CurrencyPricingDto>>
{
    private readonly ICurrencyPricingRepository _currencyPricingRepository;
    private readonly IMapper _mapper;

    public GetActiveCurrencyPricingsHandler(
        ICurrencyPricingRepository currencyPricingRepository,
        IMapper mapper)
    {
        _currencyPricingRepository = currencyPricingRepository;
        _mapper = mapper;
    }

    public async Task<List<CurrencyPricingDto>> Handle(GetActiveCurrencyPricingsQuery request, CancellationToken cancellationToken)
    {
        var currencies = await _currencyPricingRepository.GetActiveCurrenciesAsync(cancellationToken);
        return _mapper.Map<List<CurrencyPricingDto>>(currencies);
    }
}
