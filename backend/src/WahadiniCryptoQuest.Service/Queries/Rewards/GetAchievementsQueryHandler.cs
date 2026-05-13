using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Reward;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Queries.Rewards;

/// <summary>
/// T083: Handler for GetAchievementsQuery with progress calculation
/// </summary>
public class GetAchievementsQueryHandler : IRequestHandler<GetAchievementsQuery, IEnumerable<AchievementDto>>
{
    private readonly IAchievementService _achievementService;

    public GetAchievementsQueryHandler(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    public async Task<IEnumerable<AchievementDto>> Handle(GetAchievementsQuery request, CancellationToken cancellationToken)
    {
        return await _achievementService.GetUserAchievementsAsync(request.UserId, cancellationToken);
    }
}
