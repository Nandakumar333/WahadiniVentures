using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using WahadiniCryptoQuest.Core.DTOs.Common;
using WahadiniCryptoQuest.Core.DTOs.Reward;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Service.Commands.Rewards;
using WahadiniCryptoQuest.Service.Queries.Reward;
using WahadiniCryptoQuest.Service.Queries.Rewards;

namespace WahadiniCryptoQuest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RewardsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IReferralService _referralService;

    public RewardsController(IMediator mediator, IReferralService referralService)
    {
        _mediator = mediator;
        _referralService = referralService;
    }

    /// <summary>
    /// Awards points to a user (Admin only)
    /// </summary>
    [HttpPost("award")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AwardPoints([FromBody] AwardPointsRequest request)
    {
        var command = new AwardPointsCommand(
            request.UserId,
            request.Amount,
            TransactionType.AdminBonus,
            request.Description,
            null,
            null
        );

        var transactionId = await _mediator.Send(command);
        return Ok(new { TransactionId = transactionId });
    }

    /// <summary>
    /// Gets current user's reward point balance and statistics
    /// </summary>
    [HttpGet("balance")]
    [EnableRateLimiting("balance-history")]
    public async Task<ActionResult<BalanceDto>> GetBalance()
    {
        var userId = GetCurrentUserId();
        var query = new GetBalanceQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets current user's transaction history with cursor-based pagination
    /// </summary>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
    /// <param name="cursor">Cursor for next page (optional)</param>
    /// <param name="transactionType">Filter by transaction type (optional)</param>
    [HttpGet("transactions")]
    [EnableRateLimiting("balance-history")]
    public async Task<ActionResult<PaginatedResult<TransactionDto>>> GetTransactionHistory(
        [FromQuery] int pageSize = 20,
        [FromQuery] string? cursor = null,
        [FromQuery] string? transactionType = null)
    {
        var userId = GetCurrentUserId();
        var query = new GetTransactionHistoryQuery(userId)
        {
            PageSize = pageSize,
            Cursor = cursor,
            TransactionType = transactionType
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Processes user login to update daily streak and award bonus points
    /// </summary>
    [HttpPost("streak/process")]
    [EnableRateLimiting("rewards-general")]
    public async Task<ActionResult<StreakDto>> ProcessStreak()
    {
        var userId = GetCurrentUserId();
        var command = new ProcessStreakCommand(userId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Gets current user's daily streak information
    /// </summary>
    [HttpGet("streak")]
    [EnableRateLimiting("rewards-general")]
    public async Task<ActionResult<StreakDto>> GetStreak()
    {
        var userId = GetCurrentUserId();
        var query = new GetStreakQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets leaderboard for a specific period
    /// Implements T071: GET /api/v1/rewards/leaderboard
    /// </summary>
    /// <param name="period">Leaderboard period (Weekly, Monthly, AllTime) - default: Weekly</param>
    /// <param name="limit">Number of top users to return (default: 100, max: 500)</param>
    [HttpGet("leaderboard")]
    [EnableRateLimiting("leaderboard")]
    public async Task<ActionResult<IReadOnlyList<LeaderboardEntryDto>>> GetLeaderboard(
        [FromQuery] LeaderboardPeriod period = LeaderboardPeriod.Weekly,
        [FromQuery] int limit = 100)
    {
        // Validate limit
        if (limit < 1 || limit > 500)
        {
            return BadRequest(new { Error = "Limit must be between 1 and 500" });
        }

        var query = new GetLeaderboardQuery(period, limit);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets current user's rank for a specific period
    /// </summary>
    /// <param name="period">Leaderboard period (Weekly, Monthly, AllTime) - default: Weekly</param>
    [HttpGet("leaderboard/my-rank")]
    [EnableRateLimiting("leaderboard")]
    public async Task<ActionResult<UserRankDto>> GetMyRank(
        [FromQuery] LeaderboardPeriod period = LeaderboardPeriod.Weekly)
    {
        var userId = GetCurrentUserId();
        var query = new GetUserRankQuery(userId, period);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// T097: Gets all achievements with unlock status for current user
    /// </summary>
    [HttpGet("achievements")]
    [EnableRateLimiting("achievements")]
    public async Task<ActionResult<IEnumerable<AchievementDto>>> GetAchievements()
    {
        var userId = GetCurrentUserId();
        var query = new GetAchievementsQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// T098: Gets current user's referral information
    /// </summary>
    [HttpGet("referrals")]
    [EnableRateLimiting("rewards-general")]
    public async Task<ActionResult<ReferralCodeDto>> GetReferralInfo()
    {
        var userId = GetCurrentUserId();
        var result = await _referralService.GetReferralInfoAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// T099: Validates a referral code (public endpoint)
    /// </summary>
    [HttpGet("referrals/validate/{code}")]
    [AllowAnonymous]
    [EnableRateLimiting("rewards-general")]
    public async Task<ActionResult<object>> ValidateReferralCode(string code)
    {
        var (isValid, inviterUsername) = await _referralService.ValidateReferralCodeAsync(code);
        return Ok(new { isValid, inviterUsername });
    }

    /// <summary>
    /// T100: Gets current user's rank across all periods (alternative to my-rank endpoint)
    /// </summary>
    [HttpGet("rank")]
    [EnableRateLimiting("leaderboard")]
    public async Task<ActionResult<UserRankDto>> GetRank(
        [FromQuery] LeaderboardPeriod period = LeaderboardPeriod.Weekly)
    {
        var userId = GetCurrentUserId();
        var query = new GetUserRankQuery(userId, period);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Extracts current user ID from JWT claims
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return userId;
    }
}

public record AwardPointsRequest(Guid UserId, int Amount, string Description);
