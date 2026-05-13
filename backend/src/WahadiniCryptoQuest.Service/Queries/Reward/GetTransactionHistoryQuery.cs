using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Common;
using WahadiniCryptoQuest.Core.DTOs.Reward;

namespace WahadiniCryptoQuest.Service.Queries.Reward;

/// <summary>
/// Query to retrieve user's reward transaction history with cursor-based pagination
/// </summary>
public class GetTransactionHistoryQuery : IRequest<PaginatedResult<TransactionDto>>
{
    /// <summary>
    /// ID of the user to get transaction history for
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Number of items per page (default: 20, max: 100)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Base64-encoded cursor for pagination (contains timestamp + transaction ID)
    /// </summary>
    public string? Cursor { get; set; }

    /// <summary>
    /// Filter by transaction type (optional)
    /// </summary>
    public string? TransactionType { get; set; }

    public GetTransactionHistoryQuery(Guid userId)
    {
        UserId = userId;
    }
}
