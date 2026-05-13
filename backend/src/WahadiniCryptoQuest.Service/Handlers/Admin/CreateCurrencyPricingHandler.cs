using AutoMapper;
using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Subscription;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Commands.Admin;

namespace WahadiniCryptoQuest.Service.Handlers.Admin;

/// <summary>
/// Handler for creating currency pricing configurations
/// Admin-only operation (US5)
/// </summary>
public class CreateCurrencyPricingHandler : IRequestHandler<CreateCurrencyPricingCommand, CurrencyPricingDto>
{
    private readonly ICurrencyPricingRepository _currencyPricingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCurrencyPricingHandler(
        ICurrencyPricingRepository currencyPricingRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _currencyPricingRepository = currencyPricingRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CurrencyPricingDto> Handle(CreateCurrencyPricingCommand request, CancellationToken cancellationToken)
    {
        // Check if currency code already exists
        var existing = await _currencyPricingRepository.GetByCurrencyCodeAsync(request.CurrencyCode);
        if (existing != null)
        {
            throw new InvalidOperationException($"Currency pricing for {request.CurrencyCode} already exists");
        }

        // Use StripePriceIdMonthly as the main price ID (entity expects single ID)
        var stripePriceId = request.StripePriceIdMonthly ?? request.StripePriceIdYearly ?? "";
        if (string.IsNullOrEmpty(stripePriceId))
        {
            throw new ArgumentException("At least one Stripe Price ID must be provided");
        }

        // Create new currency pricing entity using factory method
        var currencyPricing = CurrencyPricing.Create(
            currencyCode: request.CurrencyCode,
            stripePriceId: stripePriceId,
            monthlyPrice: request.MonthlyPrice,
            yearlyPrice: request.YearlyPrice,
            currencySymbol: GetCurrencySymbol(request.CurrencyCode),
            showDecimal: ShouldShowDecimal(request.CurrencyCode),
            decimalPlaces: GetDecimalPlaces(request.CurrencyCode),
            adminUserId: Guid.Empty // TODO: Get from authenticated user context
        );

        // Set active status if different from default
        if (!request.IsActive)
        {
            currencyPricing.Deactivate(Guid.Empty);
        }

        await _currencyPricingRepository.AddAsync(currencyPricing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CurrencyPricingDto>(currencyPricing);
    }

    private static string GetCurrencySymbol(string currencyCode) => currencyCode.ToUpper() switch
    {
        "USD" => "$",
        "EUR" => "€",
        "GBP" => "£",
        "JPY" => "¥",
        "INR" => "₹",
        _ => currencyCode
    };

    private static bool ShouldShowDecimal(string currencyCode) => currencyCode.ToUpper() != "JPY";

    private static int GetDecimalPlaces(string currencyCode) => currencyCode.ToUpper() == "JPY" ? 0 : 2;
}
