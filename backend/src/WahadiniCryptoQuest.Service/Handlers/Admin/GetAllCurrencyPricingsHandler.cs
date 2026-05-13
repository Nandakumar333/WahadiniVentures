using AutoMapper;
using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Subscription;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Queries.Admin;

namespace WahadiniCryptoQuest.Service.Handlers.Admin;

/// <summary>
/// Handler for getting all currency pricing configurations
/// Includes inactive pricings for admin management (US5)
/// </summary>
public class GetAllCurrencyPricingsHandler : IRequestHandler<GetAllCurrencyPricingsQuery, List<CurrencyPricingDto>>
{
    private readonly ICurrencyPricingRepository _currencyPricingRepository;
    private readonly IMapper _mapper;

    public GetAllCurrencyPricingsHandler(
        ICurrencyPricingRepository currencyPricingRepository,
        IMapper mapper)
    {
        _currencyPricingRepository = currencyPricingRepository;
        _mapper = mapper;
    }

    public async Task<List<CurrencyPricingDto>> Handle(GetAllCurrencyPricingsQuery request, CancellationToken cancellationToken)
    {
        var currencyPricings = await _currencyPricingRepository.GetAllIncludingInactiveAsync();
        return _mapper.Map<List<CurrencyPricingDto>>(currencyPricings);
    }
}
