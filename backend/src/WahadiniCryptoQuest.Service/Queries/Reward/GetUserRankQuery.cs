using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Reward;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Queries.Reward;

/// <summary>
/// Query to get user's rank for a specific period
/// </summary>
public record GetUserRankQuery(
    Guid UserId,
    LeaderboardPeriod Period
) : IRequest<UserRankDto>;
