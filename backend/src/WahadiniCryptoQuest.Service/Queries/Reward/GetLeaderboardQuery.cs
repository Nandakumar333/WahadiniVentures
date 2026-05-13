using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Reward;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Queries.Reward;

/// <summary>
/// Query to get leaderboard for a specific period
/// Implements T068
/// </summary>
public record GetLeaderboardQuery(
    LeaderboardPeriod Period,
    int Limit = 100
) : IRequest<IReadOnlyList<LeaderboardEntryDto>>;
