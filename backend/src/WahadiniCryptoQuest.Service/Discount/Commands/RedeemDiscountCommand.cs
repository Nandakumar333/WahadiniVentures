using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Discount;

namespace WahadiniCryptoQuest.Service.Discount.Commands;

/// <summary>
/// Command to redeem a discount code with points
/// </summary>
public class RedeemDiscountCommand : IRequest<RedemptionResponseDto>
{
    public Guid UserId { get; set; }
    public Guid DiscountCodeId { get; set; }

    public RedeemDiscountCommand(Guid userId, Guid discountCodeId)
    {
        UserId = userId;
        DiscountCodeId = discountCodeId;
    }
}
