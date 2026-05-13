using MediatR;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Commands.Admin;

namespace WahadiniCryptoQuest.Service.Handlers.Admin;

/// <summary>
/// Handler for deleting currency pricing configurations
/// Admin-only operation (US5)
/// </summary>
public class DeleteCurrencyPricingHandler : IRequestHandler<DeleteCurrencyPricingCommand, bool>
{
    private readonly ICurrencyPricingRepository _currencyPricingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCurrencyPricingHandler(
        ICurrencyPricingRepository currencyPricingRepository,
        IUnitOfWork unitOfWork)
    {
        _currencyPricingRepository = currencyPricingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteCurrencyPricingCommand request, CancellationToken cancellationToken)
    {
        var currencyPricing = await _currencyPricingRepository.GetByIdAsync(request.Id);
        if (currencyPricing == null)
        {
            throw new InvalidOperationException($"Currency pricing with ID {request.Id} not found");
        }

        // Soft delete - deactivate first, then mark as deleted
        if (currencyPricing.IsActive)
        {
            currencyPricing.Deactivate(Guid.Empty);
        }
        currencyPricing.SoftDelete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
