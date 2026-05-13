using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Reward;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Queries.Reward;

/// <summary>
/// Handler for GetUserRankQuery
/// Real-time calculation without caching
/// </summary>
public class GetUserRankQueryHandler : IRequestHandler<GetUserRankQuery, UserRankDto>
{
    private readonly ILeaderboardService _leaderboardService;

    public GetUserRankQueryHandler(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    public async Task<UserRankDto> Handle(
        GetUserRankQuery request,
        CancellationToken cancellationToken)
    {
        return await _leaderboardService.GetUserRankAsync(
            request.UserId,
            request.Period,
            cancellationToken);
    }
}
