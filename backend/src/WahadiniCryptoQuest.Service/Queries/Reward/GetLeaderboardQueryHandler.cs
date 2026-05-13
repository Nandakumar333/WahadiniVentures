using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Reward;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Queries.Reward;

/// <summary>
/// Handler for GetLeaderboardQuery
/// Implements T069: Delegates to ILeaderboardService
/// </summary>
public class GetLeaderboardQueryHandler : IRequestHandler<GetLeaderboardQuery, IReadOnlyList<LeaderboardEntryDto>>
{
    private readonly ILeaderboardService _leaderboardService;

    public GetLeaderboardQueryHandler(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    public async Task<IReadOnlyList<LeaderboardEntryDto>> Handle(
        GetLeaderboardQuery request,
        CancellationToken cancellationToken)
    {
        return await _leaderboardService.GetLeaderboardAsync(
            request.Period,
            request.Limit,
            cancellationToken);
    }
}
