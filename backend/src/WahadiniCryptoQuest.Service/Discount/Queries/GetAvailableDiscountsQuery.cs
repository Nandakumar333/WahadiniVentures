using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Discount;

namespace WahadiniCryptoQuest.Service.Discount.Queries;

/// <summary>
/// Query to get available discounts for a specific user
/// </summary>
public class GetAvailableDiscountsQuery : IRequest<List<DiscountTypeDto>>
{
    public Guid UserId { get; set; }

    public GetAvailableDiscountsQuery(Guid userId)
    {
        UserId = userId;
    }
}
