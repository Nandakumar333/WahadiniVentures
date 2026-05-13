using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Reward;

namespace WahadiniCryptoQuest.Service.Queries.Rewards;

/// <summary>
/// T082: Query to get all achievements with user's unlock status and progress
/// </summary>
public record GetAchievementsQuery(Guid UserId) : IRequest<IEnumerable<AchievementDto>>;
