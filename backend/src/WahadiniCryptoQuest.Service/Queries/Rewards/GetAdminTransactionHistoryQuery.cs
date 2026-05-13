using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Common;
using WahadiniCryptoQuest.Core.DTOs.Reward;

namespace WahadiniCryptoQuest.Service.Queries.Rewards;

/// <summary>
/// Query to get admin-enriched transaction history for a specific user
/// </summary>
public class GetAdminTransactionHistoryQuery : IRequest<PaginatedResult<AdminTransactionHistoryDto>>
{
    public Guid UserId { get; set; }
    public int PageSize { get; set; } = 20;
    public string? Cursor { get; set; }

    public GetAdminTransactionHistoryQuery(Guid userId)
    {
        UserId = userId;
    }
}
