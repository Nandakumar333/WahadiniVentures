using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Reward;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Handlers.Rewards;

/// <summary>
/// Handler for retrieving user achievements with progress calculation
/// T083: GetAchievementsQueryHandler with progress calculation
/// </summary>
public class GetAchievementsQueryHandler : IRequestHandler<Service.Queries.Rewards.GetAchievementsQuery, IEnumerable<AchievementDto>>
{
    private readonly IAchievementService _achievementService;

    public GetAchievementsQueryHandler(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    public async Task<IEnumerable<AchievementDto>> Handle(Service.Queries.Rewards.GetAchievementsQuery request, CancellationToken cancellationToken)
    {
        return await _achievementService.GetUserAchievementsAsync(request.UserId, cancellationToken);
    }
}
