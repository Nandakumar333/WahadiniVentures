using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Discount;

namespace WahadiniCryptoQuest.Service.Discount.Queries;

/// <summary>
/// Query to get paginated redemption history for a user
/// </summary>
public class GetMyRedemptionsQuery : IRequest<PaginatedRedemptionsDto>
{
    public Guid UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public GetMyRedemptionsQuery(Guid userId, int pageNumber = 1, int pageSize = 10)
    {
        UserId = userId;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
