using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Discount.Commands;

/// <summary>
/// Handler for DeactivateDiscountCommand using CQRS pattern
/// </summary>
public class DeactivateDiscountCommandHandler : IRequestHandler<DeactivateDiscountCommand, Unit>
{
    private readonly IDiscountService _discountService;
    private readonly ILogger<DeactivateDiscountCommandHandler> _logger;

    public DeactivateDiscountCommandHandler(
        IDiscountService discountService,
        ILogger<DeactivateDiscountCommandHandler> logger)
    {
        _discountService = discountService;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeactivateDiscountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing deactivate discount command for ID: {Id}",
            request.DiscountCodeId);

        try
        {
            await _discountService.DeactivateDiscountCodeAsync(request.DiscountCodeId, cancellationToken);

            _logger.LogInformation(
                "Successfully deactivated discount {Id}",
                request.DiscountCodeId);

            return Unit.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to deactivate discount {Id}",
                request.DiscountCodeId);
            throw;
        }
    }
}
