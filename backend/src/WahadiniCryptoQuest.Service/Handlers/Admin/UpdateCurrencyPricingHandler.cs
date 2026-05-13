using AutoMapper;
using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Subscription;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Commands.Admin;

namespace WahadiniCryptoQuest.Service.Handlers.Admin;

/// <summary>
/// Handler for updating currency pricing configurations
/// Admin-only operation (US5)
/// </summary>
public class UpdateCurrencyPricingHandler : IRequestHandler<UpdateCurrencyPricingCommand, CurrencyPricingDto>
{
    private readonly ICurrencyPricingRepository _currencyPricingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCurrencyPricingHandler(
        ICurrencyPricingRepository currencyPricingRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _currencyPricingRepository = currencyPricingRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CurrencyPricingDto> Handle(UpdateCurrencyPricingCommand request, CancellationToken cancellationToken)
    {
        // Get existing currency pricing
        var currencyPricing = await _currencyPricingRepository.GetByIdAsync(request.Id);
        if (currencyPricing == null)
        {
            throw new InvalidOperationException($"Currency pricing with ID {request.Id} not found");
        }

        // Note: Currency code cannot be changed after creation (entity doesn't support it)
        // Use StripePriceIdMonthly as the main price ID
        var stripePriceId = request.StripePriceIdMonthly ?? request.StripePriceIdYearly ?? "";
        if (string.IsNullOrEmpty(stripePriceId))
        {
            throw new ArgumentException("At least one Stripe Price ID must be provided");
        }

        // Update pricing using domain method
        currencyPricing.UpdatePricing(
            monthlyPrice: request.MonthlyPrice,
            yearlyPrice: request.YearlyPrice,
            stripePriceId: stripePriceId,
            adminUserId: Guid.Empty // TODO: Get from authenticated user context
        );

        // Handle active status change
        if (request.IsActive && !currencyPricing.IsActive)
        {
            currencyPricing.Activate(Guid.Empty);
        }
        else if (!request.IsActive && currencyPricing.IsActive)
        {
            currencyPricing.Deactivate(Guid.Empty);
        }

        // Repository Update method not needed - EF tracks changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CurrencyPricingDto>(currencyPricing);
    }
}
