using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Discount;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Discount.Commands;

/// <summary>
/// Handler for CreateDiscountTypeCommand using CQRS pattern
/// </summary>
public class CreateDiscountTypeCommandHandler : IRequestHandler<CreateDiscountTypeCommand, AdminDiscountTypeDto>
{
    private readonly IDiscountService _discountService;
    private readonly ILogger<CreateDiscountTypeCommandHandler> _logger;

    public CreateDiscountTypeCommandHandler(
        IDiscountService discountService,
        ILogger<CreateDiscountTypeCommandHandler> logger)
    {
        _discountService = discountService;
        _logger = logger;
    }

    public async Task<AdminDiscountTypeDto> Handle(CreateDiscountTypeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing create discount type command for code: {Code}",
            request.Code);

        try
        {
            var dto = new CreateDiscountCodeDto
            {
                Code = request.Code,
                DiscountPercentage = request.DiscountPercentage,
                RequiredPoints = request.RequiredPoints,
                MaxRedemptions = request.MaxRedemptions,
                ExpiryDate = request.ExpiryDate
            };

            var result = await _discountService.CreateDiscountCodeAsync(dto, cancellationToken);

            _logger.LogInformation(
                "Successfully created discount type {Code} with ID {Id}",
                result.Code,
                result.Id);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create discount type with code {Code}",
                request.Code);
            throw;
        }
    }
}
