using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Discount;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Discount.Commands;

/// <summary>
/// Handler for RedeemDiscountCommand using CQRS pattern with atomic transaction
/// </summary>
public class RedeemDiscountCommandHandler : IRequestHandler<RedeemDiscountCommand, RedemptionResponseDto>
{
    private readonly IDiscountService _discountService;
    private readonly ILogger<RedeemDiscountCommandHandler> _logger;

    public RedeemDiscountCommandHandler(
        IDiscountService discountService,
        ILogger<RedeemDiscountCommandHandler> logger)
    {
        _discountService = discountService;
        _logger = logger;
    }

    public async Task<RedemptionResponseDto> Handle(RedeemDiscountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing redemption command for User {UserId}, DiscountCode {DiscountCodeId}",
            request.UserId,
            request.DiscountCodeId);

        try
        {
            // Delegate to service layer which handles atomic transaction
            var result = await _discountService.RedeemDiscountAsync(
                request.UserId,
                request.DiscountCodeId,
                cancellationToken);

            _logger.LogInformation(
                "Successfully redeemed discount {Code} for user {UserId}",
                result.Code,
                request.UserId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to redeem discount {DiscountCodeId} for user {UserId}",
                request.DiscountCodeId,
                request.UserId);
            throw;
        }
    }
}
