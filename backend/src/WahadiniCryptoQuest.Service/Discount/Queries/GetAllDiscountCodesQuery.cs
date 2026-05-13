using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Discount;

namespace WahadiniCryptoQuest.Service.Discount.Queries;

/// <summary>
/// Query to get all discount codes for admin management
/// </summary>
public class GetAllDiscountCodesQuery : IRequest<List<AdminDiscountTypeDto>>
{
}
