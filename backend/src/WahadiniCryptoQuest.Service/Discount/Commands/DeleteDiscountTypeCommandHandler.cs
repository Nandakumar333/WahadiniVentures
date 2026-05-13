using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Discount.Commands;

/// <summary>
/// Handler for DeleteDiscountTypeCommand using CQRS pattern with soft delete
/// </summary>
public class DeleteDiscountTypeCommandHandler : IRequestHandler<DeleteDiscountTypeCommand, Unit>
{
    private readonly IDiscountService _discountService;
    private readonly ILogger<DeleteDiscountTypeCommandHandler> _logger;

    public DeleteDiscountTypeCommandHandler(
        IDiscountService discountService,
        ILogger<DeleteDiscountTypeCommandHandler> logger)
    {
        _discountService = discountService;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteDiscountTypeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing soft delete discount command for ID: {Id}",
            request.DiscountCodeId);

        try
        {
            await _discountService.DeleteDiscountCodeAsync(request.DiscountCodeId, cancellationToken);

            _logger.LogInformation(
                "Successfully soft deleted discount {Id}",
                request.DiscountCodeId);

            return Unit.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to soft delete discount {Id}",
                request.DiscountCodeId);
            throw;
        }
    }
}
