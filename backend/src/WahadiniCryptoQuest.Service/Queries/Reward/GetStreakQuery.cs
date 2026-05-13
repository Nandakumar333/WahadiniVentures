using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Reward;

namespace WahadiniCryptoQuest.Service.Queries.Reward;

/// <summary>
/// Query to retrieve user's current daily login streak information
/// </summary>
public class GetStreakQuery : IRequest<StreakDto>
{
    /// <summary>
    /// ID of the user to get streak information for
    /// </summary>
    public Guid UserId { get; set; }

    public GetStreakQuery(Guid userId)
    {
        UserId = userId;
    }
}
