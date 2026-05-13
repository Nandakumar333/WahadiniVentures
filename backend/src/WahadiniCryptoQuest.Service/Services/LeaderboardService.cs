using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Reward;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Services;

/// <summary>
/// Leaderboard service with 15-minute caching
/// Implements T064-T067: Cached leaderboards with tie-breaking logic
/// </summary>
public class LeaderboardService : ILeaderboardService
{
    private readonly IUserRepository _userRepository;
    private readonly IRewardTransactionRepository _transactionRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LeaderboardService> _logger;
    private const string CACHE_KEY_PREFIX = "leaderboard_";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(15);

    public LeaderboardService(
        IUserRepository userRepository,
        IRewardTransactionRepository transactionRepository,
        IMemoryCache cache,
        ILogger<LeaderboardService> logger)
    {
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Gets leaderboard with 15-minute cache TTL (T065)
    /// </summary>
    public async Task<IReadOnlyList<LeaderboardEntryDto>> GetLeaderboardAsync(
        LeaderboardPeriod period,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CACHE_KEY_PREFIX}{period}_{limit}";

        // Try to get from cache first
        if (_cache.TryGetValue<IReadOnlyList<LeaderboardEntryDto>>(cacheKey, out var cachedLeaderboard))
        {
            _logger.LogDebug("Leaderboard cache hit for period: {Period}, limit: {Limit}", period, limit);
            return cachedLeaderboard!;
        }

        _logger.LogDebug("Leaderboard cache miss for period: {Period}, limit: {Limit}", period, limit);

        // Calculate date range based on period
        var (startDate, endDate) = GetPeriodRange(period);

        // Get leaderboard data with tie-breaking (T067)
        var leaderboardEntries = await CalculateLeaderboardAsync(startDate, endDate, limit, cancellationToken);

        // Cache the result for 15 minutes
        _cache.Set(cacheKey, leaderboardEntries, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CACHE_DURATION,
            Priority = CacheItemPriority.Normal
        });

        return leaderboardEntries;
    }

    /// <summary>
    /// Gets user rank with real-time calculation (T066)
    /// No caching for accurate rank display
    /// </summary>
    public async Task<UserRankDto> GetUserRankAsync(
        Guid userId,
        LeaderboardPeriod period,
        CancellationToken cancellationToken = default)
    {
        var (startDate, endDate) = GetPeriodRange(period);

        // Get user's points for the period
        var userPoints = await CalculateUserPointsAsync(userId, startDate, endDate, cancellationToken);

        // Get total number of users with points in this period
        var totalUsers = await GetTotalUsersWithPointsAsync(startDate, endDate, cancellationToken);

        // Calculate rank by counting users with higher points
        // Tie-breaking: same points = same rank, use CreatedAt for ordering (T067)
        var rank = await CalculateUserRankAsync(userId, userPoints, startDate, endDate, cancellationToken);

        return new UserRankDto
        {
            Rank = rank,
            Points = userPoints,
            TotalUsers = totalUsers,
            Period = period
        };
    }

    /// <summary>
    /// Clears leaderboard cache
    /// </summary>
    public void ClearCache(LeaderboardPeriod? period = null)
    {
        if (period.HasValue)
        {
            // Clear specific period cache
            for (int limit = 10; limit <= 500; limit += 10)
            {
                var cacheKey = $"{CACHE_KEY_PREFIX}{period}_{limit}";
                _cache.Remove(cacheKey);
            }
            _logger.LogInformation("Cleared leaderboard cache for period: {Period}", period);
        }
        else
        {
            // Clear all period caches
            foreach (LeaderboardPeriod p in Enum.GetValues(typeof(LeaderboardPeriod)))
            {
                ClearCache(p);
            }
            _logger.LogInformation("Cleared all leaderboard caches");
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Calculates leaderboard with tie-breaking logic (T067)
    /// Tie-breaking: User.CreatedAt ASC (earlier registration wins)
    /// </summary>
    private async Task<IReadOnlyList<LeaderboardEntryDto>> CalculateLeaderboardAsync(
        DateTime? startDate,
        DateTime? endDate,
        int limit,
        CancellationToken cancellationToken)
    {
        var usersQuery = _userRepository.GetQueryable().Where(u => u.CurrentPoints > 0);

        // Calculate points for period
        IQueryable<dynamic> leaderboardQuery;

        if (startDate.HasValue && endDate.HasValue)
        {
            // Weekly or Monthly: Calculate from transactions
            leaderboardQuery = from user in usersQuery
                               let periodPoints = _transactionRepository.GetQueryable()
                                   .Where(t => t.UserId == user.Id &&
                                               t.CreatedAt >= startDate.Value &&
                                               t.CreatedAt <= endDate.Value)
                                   .Sum(t => (int?)t.Amount) ?? 0
                               where periodPoints > 0
                               orderby periodPoints descending, user.CreatedAt ascending
                               select new
                               {
                                   UserId = user.Id,
                                   Name = user.Email,
                                   Points = periodPoints,
                                   AvatarUrl = (string?)null,
                                   CreatedAt = user.CreatedAt
                               };
        }
        else
        {
            // All-Time: Use CurrentPoints
            leaderboardQuery = from user in usersQuery
                               orderby user.CurrentPoints descending, user.CreatedAt ascending
                               select new
                               {
                                   UserId = user.Id,
                                   Name = user.Email,
                                   Points = user.CurrentPoints,
                                   AvatarUrl = (string?)null,
                                   CreatedAt = user.CreatedAt
                               };
        }

        var results = await leaderboardQuery
            .Take(limit)
            .ToListAsync(cancellationToken);

        // Assign ranks with tie handling
        var entries = new List<LeaderboardEntryDto>();
        int currentRank = 1;
        int? previousPoints = null;
        int usersAtSameRank = 0;

        foreach (var result in results)
        {
            if (previousPoints.HasValue && result.Points < previousPoints.Value)
            {
                currentRank += usersAtSameRank;
                usersAtSameRank = 1;
            }
            else if (previousPoints.HasValue && result.Points == previousPoints.Value)
            {
                usersAtSameRank++;
            }
            else
            {
                usersAtSameRank = 1;
            }

            entries.Add(new LeaderboardEntryDto
            {
                UserId = result.UserId,
                Name = result.Name,
                Points = result.Points,
                Rank = currentRank,
                AvatarUrl = result.AvatarUrl
            });

            previousPoints = result.Points;
        }

        return entries;
    }

    /// <summary>
    /// Calculates user's points for a specific period
    /// </summary>
    private async Task<int> CalculateUserPointsAsync(
        Guid userId,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken)
    {
        if (startDate.HasValue && endDate.HasValue)
        {
            // Period-specific calculation
            var points = await _transactionRepository.GetQueryable()
                .Where(t => t.UserId == userId &&
                            t.CreatedAt >= startDate.Value &&
                            t.CreatedAt <= endDate.Value)
                .SumAsync(t => (int?)t.Amount, cancellationToken) ?? 0;
            return points;
        }
        else
        {
            // All-time: Use CurrentPoints
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.CurrentPoints ?? 0;
        }
    }

    /// <summary>
    /// Calculates user's rank with tie-breaking
    /// </summary>
    private async Task<int> CalculateUserRankAsync(
        Guid userId,
        int userPoints,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return 0;

        int rank;

        if (startDate.HasValue && endDate.HasValue)
        {
            // Period-specific ranking
            rank = await _userRepository.GetQueryable()
                .Select(u => new
                {
                    UserId = u.Id,
                    Points = _transactionRepository.GetQueryable()
                        .Where(t => t.UserId == u.Id &&
                                    t.CreatedAt >= startDate.Value &&
                                    t.CreatedAt <= endDate.Value)
                        .Sum(t => (int?)t.Amount) ?? 0,
                    CreatedAt = u.CreatedAt
                })
                .CountAsync(u => u.Points > userPoints ||
                            (u.Points == userPoints && u.CreatedAt < user.CreatedAt),
                    cancellationToken);
        }
        else
        {
            // All-time ranking
            rank = await _userRepository.GetQueryable()
                .CountAsync(u => u.CurrentPoints > userPoints ||
                            (u.CurrentPoints == userPoints && u.CreatedAt < user.CreatedAt),
                    cancellationToken);
        }

        return rank + 1; // Rank is 1-indexed
    }

    /// <summary>
    /// Gets total number of users with points in the period
    /// </summary>
    private async Task<int> GetTotalUsersWithPointsAsync(
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken)
    {
        if (startDate.HasValue && endDate.HasValue)
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                var count = await _transactionRepository.GetQueryable()
                    .Where(t => t.CreatedAt >= startDate.Value && t.CreatedAt <= endDate.Value)
                    .Select(t => t.UserId)
                    .Distinct()
                    .CountAsync(cancellationToken);
                return count;
            }
            else
            {
                var count = await _userRepository.GetQueryable()
                    .CountAsync(u => u.CurrentPoints > 0, cancellationToken);
                return count;
            }
        }
        else
        {
            var count = await _userRepository.GetQueryable()
                .CountAsync(u => u.CurrentPoints > 0, cancellationToken);
            return count;
        }
    }


    /// <summary>
    /// Gets date range for leaderboard period
    /// </summary>
    private (DateTime? StartDate, DateTime? EndDate) GetPeriodRange(LeaderboardPeriod period)
    {
        var now = DateTime.UtcNow;

        return period switch
        {
            LeaderboardPeriod.Weekly => (now.AddDays(-7), now),
            LeaderboardPeriod.Monthly => (now.AddDays(-30), now),
            LeaderboardPeriod.AllTime => (null, null),
            _ => throw new ArgumentException($"Invalid leaderboard period: {period}")
        };
    }

    #endregion
}
