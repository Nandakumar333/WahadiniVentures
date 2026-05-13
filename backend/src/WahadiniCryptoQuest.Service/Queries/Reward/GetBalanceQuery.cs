using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Reward;

namespace WahadiniCryptoQuest.Service.Queries.Reward;

/// <summary>
/// Query to retrieve user's current reward point balance and statistics
/// </summary>
public class GetBalanceQuery : IRequest<BalanceDto>
{
    /// <summary>
    /// ID of the user to get balance for
    /// </summary>
    public Guid UserId { get; set; }

    public GetBalanceQuery(Guid userId)
    {
        UserId = userId;
    }
}
