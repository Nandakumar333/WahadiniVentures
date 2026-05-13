using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Discount.Commands;

/// <summary>
/// Handler for ActivateDiscountCommand using CQRS pattern
/// </summary>
public class ActivateDiscountCommandHandler : IRequestHandler<ActivateDiscountCommand, Unit>
{
    private readonly IDiscountService _discountService;
    private readonly ILogger<ActivateDiscountCommandHandler> _logger;

    public ActivateDiscountCommandHandler(
        IDiscountService discountService,
        ILogger<ActivateDiscountCommandHandler> logger)
    {
        _discountService = discountService;
        _logger = logger;
    }

    public async Task<Unit> Handle(ActivateDiscountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing activate discount command for ID: {Id}",
            request.DiscountCodeId);

        try
        {
            await _discountService.ActivateDiscountCodeAsync(request.DiscountCodeId, cancellationToken);

            _logger.LogInformation(
                "Successfully activated discount {Id}",
                request.DiscountCodeId);

            return Unit.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to activate discount {Id}",
                request.DiscountCodeId);
            throw;
        }
    }
}
