using AutoMapper;
using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Subscription;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Queries.Admin;

namespace WahadiniCryptoQuest.Service.Handlers.Admin;

/// <summary>
/// Handler for getting a single currency pricing by ID
/// Admin-only operation (US5)
/// </summary>
public class GetCurrencyPricingHandler : IRequestHandler<GetCurrencyPricingQuery, CurrencyPricingDto?>
{
    private readonly ICurrencyPricingRepository _currencyPricingRepository;
    private readonly IMapper _mapper;

    public GetCurrencyPricingHandler(
        ICurrencyPricingRepository currencyPricingRepository,
        IMapper mapper)
    {
        _currencyPricingRepository = currencyPricingRepository;
        _mapper = mapper;
    }

    public async Task<CurrencyPricingDto?> Handle(GetCurrencyPricingQuery request, CancellationToken cancellationToken)
    {
        var currencyPricing = await _currencyPricingRepository.GetByIdAsync(request.Id);
        return currencyPricing == null ? null : _mapper.Map<CurrencyPricingDto>(currencyPricing);
    }
}
