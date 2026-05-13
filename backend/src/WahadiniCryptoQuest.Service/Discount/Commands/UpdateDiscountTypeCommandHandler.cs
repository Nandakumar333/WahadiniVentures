using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Discount;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Discount.Commands;

/// <summary>
/// Handler for UpdateDiscountTypeCommand using CQRS pattern
/// </summary>
public class UpdateDiscountTypeCommandHandler : IRequestHandler<UpdateDiscountTypeCommand, AdminDiscountTypeDto>
{
    private readonly IDiscountService _discountService;
    private readonly ILogger<UpdateDiscountTypeCommandHandler> _logger;

    public UpdateDiscountTypeCommandHandler(
        IDiscountService discountService,
        ILogger<UpdateDiscountTypeCommandHandler> logger)
    {
        _discountService = discountService;
        _logger = logger;
    }

    public async Task<AdminDiscountTypeDto> Handle(UpdateDiscountTypeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing update discount type command for ID: {Id}",
            request.Id);

        try
        {
            var dto = new UpdateDiscountCodeDto
            {
                DiscountPercentage = request.DiscountPercentage,
                RequiredPoints = request.RequiredPoints,
                MaxRedemptions = request.MaxRedemptions,
                ExpiryDate = request.ExpiryDate
            };

            var result = await _discountService.UpdateDiscountCodeAsync(request.Id, dto, cancellationToken);

            _logger.LogInformation(
                "Successfully updated discount type {Id}",
                result.Id);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to update discount type {Id}",
                request.Id);
            throw;
        }
    }
}
