using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using WahadiniCryptoQuest.Core.DTOs.Discount;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Services;

/// <summary>
/// Service implementation for discount code management and redemption
/// Handles business logic for point-based discount system with atomic transactions
/// Includes retry logic for transient database failures using Polly
/// </summary>
public class DiscountService : IDiscountService
{
    private readonly IDiscountCodeRepository _discountRepository;
    private readonly IUserDiscountRedemptionRepository _redemptionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRewardService _rewardService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DiscountService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public DiscountService(
        IDiscountCodeRepository discountRepository,
        IUserDiscountRedemptionRepository redemptionRepository,
        IUserRepository userRepository,
        IRewardService rewardService,
        IUnitOfWork unitOfWork,
        ILogger<DiscountService> logger)
    {
        _discountRepository = discountRepository;
        _redemptionRepository = redemptionRepository;
        _userRepository = userRepository;
        _rewardService = rewardService;
        _unitOfWork = unitOfWork;
        _logger = logger;

        // Configure Polly retry policy for transient database failures
        // Retries 3 times with exponential backoff: 1s, 2s, 4s
        _retryPolicy = Policy
            .Handle<DbUpdateException>(ex => IsTransientError(ex))
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Database operation failed with transient error. Retry attempt {RetryCount} after {Delay}s delay. Operation: {Operation}",
                        retryCount,
                        timeSpan.TotalSeconds,
                        context["Operation"]);
                });
    }

    /// <summary>
    /// Determines if a database exception is transient (temporary) and safe to retry
    /// </summary>
    private static bool IsTransientError(DbUpdateException ex)
    {
        // Check for common transient SQL errors (deadlocks, timeouts, connection issues)
        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("deadlock", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("network", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Retrieves all available discount codes for a user with their affordability status
    /// Includes retry logic for transient database failures
    /// </summary>
    /// <param name="userId">The ID of the user requesting discounts</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of available discounts with user-specific affordability information</returns>
    public async Task<IEnumerable<DiscountTypeDto>> GetAvailableDiscountsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation(
            "GetAvailableDiscountsAsync started for user {UserId} at {StartTime}",
            userId, startTime);

        try
        {
            return await _retryPolicy.ExecuteAsync(async (context) =>
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found while fetching discounts", userId);
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }

                var activeDiscounts = await _discountRepository.GetActiveCodesAsync(cancellationToken);

                var discountDtos = activeDiscounts.Select(d => new DiscountTypeDto
                {
                    Id = d.Id,
                    Code = d.Code,
                    DiscountPercentage = d.DiscountPercentage,
                    RequiredPoints = d.RequiredPoints,
                    MaxRedemptions = d.MaxRedemptions,
                    CurrentRedemptions = d.CurrentRedemptions,
                    ExpiryDate = d.ExpiryDate,
                    IsActive = d.IsActive,
                    CanAfford = user.CurrentPoints >= d.RequiredPoints,
                    CanRedeem = d.CanRedeem(user.CurrentPoints)
                }).ToList();

                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogInformation(
                    "GetAvailableDiscountsAsync completed for user {UserId}. Found {DiscountCount} discounts, User balance: {UserBalance} points. Duration: {Duration}ms",
                    userId, discountDtos.Count, user.CurrentPoints, duration);

                return discountDtos;
            }, new Context("GetAvailableDiscounts"));
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(
                ex,
                "GetAvailableDiscountsAsync failed for user {UserId} after {Duration}ms. Error: {ErrorMessage}",
                userId, duration, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Processes a discount redemption with atomic transaction and point deduction
    /// Includes retry logic for transient failures and comprehensive logging
    /// </summary>
    /// <param name="userId">The ID of the user redeeming the discount</param>
    /// <param name="discountCodeId">The ID of the discount code to redeem</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Redemption response with generated code and updated balances</returns>
    /// <exception cref="KeyNotFoundException">User or discount code not found</exception>
    /// <exception cref="InvalidOperationException">Redemption validation failed</exception>
    public async Task<RedemptionResponseDto> RedeemDiscountAsync(
        Guid userId,
        Guid discountCodeId,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation(
            "RedeemDiscountAsync started. UserId: {UserId}, DiscountCodeId: {DiscountCodeId}, StartTime: {StartTime}",
            userId, discountCodeId, startTime);

        try
        {
            return await _retryPolicy.ExecuteAsync(async (context) =>
            {
                // Start atomic transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                try
                {
                    // 1. Get user with concurrency token
                    var user = await _userRepository.GetByIdAsync(userId);
                    if (user == null)
                    {
                        _logger.LogWarning("RedeemDiscount failed: User {UserId} not found", userId);
                        throw new KeyNotFoundException($"User with ID {userId} not found");
                    }

                    // 2. Get discount code
                    var discount = await _discountRepository.GetByIdAsync(discountCodeId);
                    if (discount == null)
                    {
                        _logger.LogWarning(
                            "RedeemDiscount failed: Discount {DiscountCodeId} not found for user {UserId}",
                            discountCodeId, userId);
                        throw new KeyNotFoundException($"Discount code with ID {discountCodeId} not found");
                    }

                    _logger.LogDebug(
                        "Redemption validation: UserId={UserId}, DiscountCode={Code}, UserPoints={UserPoints}, RequiredPoints={RequiredPoints}, IsActive={IsActive}, MaxRedemptions={MaxRedemptions}, CurrentRedemptions={CurrentRedemptions}",
                        userId, discount.Code, user.CurrentPoints, discount.RequiredPoints, discount.IsActive, discount.MaxRedemptions, discount.CurrentRedemptions);

                    // 3. Validate discount availability
                    if (!discount.CanRedeem(user.CurrentPoints))
                    {
                        var reason = !discount.IsActive ? "inactive" :
                                     discount.ExpiryDate.HasValue && discount.ExpiryDate < DateTime.UtcNow ? "expired" :
                                     user.CurrentPoints < discount.RequiredPoints ? "insufficient points" :
                                     "maximum redemptions reached";

                        _logger.LogWarning(
                            "RedeemDiscount validation failed for user {UserId}, discount {DiscountCode}. Reason: {Reason}",
                            userId, discount.Code, reason);

                        throw new InvalidOperationException($"Cannot redeem discount: {reason}");
                    }

                    // 4. Check if user already redeemed this code (one per user rule)
                    var hasRedeemed = await _redemptionRepository.HasUserRedeemedDiscountAsync(
                        userId, discountCodeId, cancellationToken);

                    if (hasRedeemed)
                    {
                        _logger.LogWarning(
                            "RedeemDiscount failed: User {UserId} already redeemed discount {DiscountCode}",
                            userId, discount.Code);
                        throw new InvalidOperationException("You have already redeemed this discount code");
                    }

                    var pointsToDeduct = discount.RequiredPoints;
                    var previousBalance = user.CurrentPoints;

                    // 5. Deduct points from user (using domain method with validation)
                    user.DeductPoints(pointsToDeduct);

                    _logger.LogDebug(
                        "Points deducted: UserId={UserId}, PreviousBalance={PreviousBalance}, PointsDeducted={PointsDeducted}, NewBalance={NewBalance}",
                        userId, previousBalance, pointsToDeduct, user.CurrentPoints);

                    // 6. Create reward transaction for audit trail
                    await _rewardService.AwardPointsAsync(
                        userId,
                        -pointsToDeduct, // Negative for deduction
                        TransactionType.Redemption,
                        $"Redeemed discount code: {discount.Code}",
                        discountCodeId.ToString(),
                        "DiscountRedemption",
                        cancellationToken: cancellationToken);

                    // 7. Increment redemption counter
                    discount.IncrementRedemptions();

                    _logger.LogDebug(
                        "Redemption count incremented: DiscountCode={Code}, NewCount={NewCount}/{MaxCount}",
                        discount.Code, discount.CurrentRedemptions, discount.MaxRedemptions);

                    // 8. Create redemption record
                    var redemption = new UserDiscountRedemption
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        DiscountCodeId = discountCodeId,
                        RedeemedAt = DateTime.UtcNow,
                        UsedInSubscription = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _redemptionRepository.AddAsync(redemption);

                    // 9. Save all changes in atomic transaction
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    _logger.LogInformation(
                        "RedeemDiscountAsync succeeded. UserId={UserId}, DiscountCode={Code}, DiscountPercentage={Percentage}%, " +
                        "PointsDeducted={PointsDeducted}, PreviousBalance={PreviousBalance}, NewBalance={NewBalance}, " +
                        "RedemptionId={RedemptionId}, Duration={Duration}ms",
                        userId, discount.Code, discount.DiscountPercentage, pointsToDeduct, previousBalance,
                        user.CurrentPoints, redemption.Id, duration);

                    return new RedemptionResponseDto
                    {
                        Id = redemption.Id,
                        Code = discount.Code,
                        DiscountPercentage = discount.DiscountPercentage,
                        PointsDeducted = pointsToDeduct,
                        RemainingPoints = user.CurrentPoints,
                        RedeemedAt = redemption.RedeemedAt,
                        ExpiryDate = discount.ExpiryDate,
                        Message = $"Success! Use code {discount.Code} for {discount.DiscountPercentage}% off your subscription."
                    };
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    _logger.LogWarning(
                        ex,
                        "RedeemDiscount concurrency conflict: UserId={UserId}, DiscountCodeId={DiscountCodeId}. Another transaction in progress.",
                        userId, discountCodeId);
                    throw new InvalidOperationException(
                        "Another transaction is in progress. Please try again.", ex);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    _logger.LogError(
                        ex,
                        "RedeemDiscount transaction failed: UserId={UserId}, DiscountCodeId={DiscountCodeId}, Error={ErrorMessage}",
                        userId, discountCodeId, ex.Message);
                    throw;
                }
            }, new Context("RedeemDiscount"));
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(
                ex,
                "RedeemDiscountAsync failed after {Duration}ms. UserId={UserId}, DiscountCodeId={DiscountCodeId}, Error={ErrorMessage}",
                duration, userId, discountCodeId, ex.Message);
            throw;
        }
    }

    public async Task<(IEnumerable<UserRedemptionDto> Items, int TotalCount)> GetMyRedemptionsAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting redemptions for user {UserId}, page {PageNumber}, size {PageSize}",
            userId, pageNumber, pageSize);

        var (redemptions, totalCount) = await _redemptionRepository.GetUserRedemptionsAsync(
            userId, pageNumber, pageSize, cancellationToken);

        var redemptionDtos = redemptions.Select(r => new UserRedemptionDto
        {
            Id = r.Id,
            Code = r.DiscountCode.Code,
            DiscountPercentage = r.DiscountCode.DiscountPercentage,
            RedeemedAt = r.RedeemedAt,
            ExpiryDate = r.DiscountCode.ExpiryDate,
            UsedInSubscription = r.UsedInSubscription
        }).ToList();

        return (redemptionDtos, totalCount);
    }

    public async Task<AdminDiscountTypeDto> CreateDiscountCodeAsync(
        CreateDiscountCodeDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new discount code: {Code}", dto.Code);

        // Check if code already exists
        var existing = await _discountRepository.GetByCodeAsync(dto.Code, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException($"Discount code '{dto.Code}' already exists");
        }

        var discountCode = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = dto.Code.ToUpperInvariant(),
            DiscountPercentage = dto.DiscountPercentage,
            RequiredPoints = dto.RequiredPoints,
            MaxRedemptions = dto.MaxRedemptions,
            ExpiryDate = dto.ExpiryDate,
            IsActive = true,
            CurrentRedemptions = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _discountRepository.AddAsync(discountCode);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created discount code {Code} with ID {Id}", dto.Code, discountCode.Id);

        return MapToAdminDto(discountCode);
    }

    public async Task<AdminDiscountTypeDto> UpdateDiscountCodeAsync(
        Guid discountCodeId,
        UpdateDiscountCodeDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating discount code {DiscountCodeId}", discountCodeId);

        var discount = await _discountRepository.GetByIdAsync(discountCodeId);
        if (discount == null)
        {
            throw new KeyNotFoundException($"Discount code with ID {discountCodeId} not found");
        }

        if (dto.DiscountPercentage.HasValue)
            discount.DiscountPercentage = dto.DiscountPercentage.Value;

        if (dto.RequiredPoints.HasValue)
            discount.RequiredPoints = dto.RequiredPoints.Value;

        if (dto.MaxRedemptions.HasValue)
            discount.MaxRedemptions = dto.MaxRedemptions.Value;

        if (dto.ExpiryDate.HasValue)
            discount.ExpiryDate = dto.ExpiryDate.Value;

        discount.UpdatedAt = DateTime.UtcNow;

        await _discountRepository.UpdateAsync(discount);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated discount code {DiscountCodeId}", discountCodeId);

        return MapToAdminDto(discount);
    }

    public async Task DeactivateDiscountCodeAsync(
        Guid discountCodeId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deactivating discount code {DiscountCodeId}", discountCodeId);

        var discount = await _discountRepository.GetByIdAsync(discountCodeId);
        if (discount == null)
        {
            throw new KeyNotFoundException($"Discount code with ID {discountCodeId} not found");
        }

        discount.IsActive = false;
        discount.UpdatedAt = DateTime.UtcNow;

        await _discountRepository.UpdateAsync(discount);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deactivated discount code {DiscountCodeId}", discountCodeId);
    }

    public async Task ActivateDiscountCodeAsync(
        Guid discountCodeId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Activating discount code {DiscountCodeId}", discountCodeId);

        var discount = await _discountRepository.GetByIdAsync(discountCodeId);
        if (discount == null)
        {
            throw new KeyNotFoundException($"Discount code with ID {discountCodeId} not found");
        }

        discount.IsActive = true;
        discount.UpdatedAt = DateTime.UtcNow;

        await _discountRepository.UpdateAsync(discount);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Activated discount code {DiscountCodeId}", discountCodeId);
    }

    public async Task DeleteDiscountCodeAsync(
        Guid discountCodeId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Soft deleting discount code {DiscountCodeId}", discountCodeId);

        var discount = await _discountRepository.GetByIdAsync(discountCodeId);
        if (discount == null)
        {
            throw new KeyNotFoundException($"Discount code with ID {discountCodeId} not found");
        }

        discount.SoftDelete();

        await _discountRepository.UpdateAsync(discount);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Soft deleted discount code {DiscountCodeId}", discountCodeId);
    }

    public async Task<DiscountAnalyticsDto> GetDiscountAnalyticsAsync(
        Guid? discountCodeId = null,
        CancellationToken cancellationToken = default)
    {
        if (discountCodeId.HasValue)
        {
            // Get analytics for specific discount
            _logger.LogInformation("Getting analytics for discount code {DiscountCodeId}", discountCodeId.Value);

            var discount = await _discountRepository.GetByIdAsync(discountCodeId.Value);
            if (discount == null)
            {
                throw new KeyNotFoundException($"Discount code with ID {discountCodeId} not found");
            }

            var redemptions = await _redemptionRepository.GetByDiscountCodeIdAsync(discountCodeId.Value, cancellationToken);
            var redemptionList = redemptions.ToList();

            var uniqueUsers = redemptionList.Select(r => r.UserId).Distinct().Count();
            var firstRedemption = redemptionList.MinBy(r => r.RedeemedAt)?.RedeemedAt;
            var lastRedemption = redemptionList.MaxBy(r => r.RedeemedAt)?.RedeemedAt;

            decimal avgPerDay = 0;
            if (firstRedemption.HasValue && lastRedemption.HasValue)
            {
                var daySpan = (lastRedemption.Value - firstRedemption.Value).TotalDays;
                if (daySpan > 0)
                {
                    avgPerDay = (decimal)redemptionList.Count / (decimal)daySpan;
                }
                else
                {
                    avgPerDay = redemptionList.Count; // All on same day
                }
            }

            return new DiscountAnalyticsDto
            {
                DiscountCodeId = discount.Id,
                Code = discount.Code,
                DiscountPercentage = discount.DiscountPercentage,
                RequiredPoints = discount.RequiredPoints,
                IsActive = discount.IsActive,
                ExpiryDate = discount.ExpiryDate,
                TotalRedemptions = redemptionList.Count,
                TotalPointsBurned = redemptionList.Count * discount.RequiredPoints,
                UniqueUsers = uniqueUsers,
                FirstRedemptionDate = firstRedemption,
                LastRedemptionDate = lastRedemption,
                AverageRedemptionsPerDay = avgPerDay
            };
        }
        else
        {
            // Get summary analytics for all discounts
            _logger.LogInformation("Getting summary analytics for all discount codes");

            var allDiscounts = await _discountRepository.GetAllAsync();
            var allRedemptions = await _redemptionRepository.GetAllAsync();

            var discountList = allDiscounts.ToList();
            var redemptionList = allRedemptions.ToList();

            var topPerforming = discountList
                .Select(d => new
                {
                    Discount = d,
                    Redemptions = redemptionList.Where(r => r.DiscountCodeId == d.Id).ToList()
                })
                .OrderByDescending(x => x.Redemptions.Count)
                .Take(5)
                .Select(x => new DiscountPerformanceDto
                {
                    Id = x.Discount.Id,
                    Code = x.Discount.Code,
                    RedemptionCount = x.Redemptions.Count,
                    PointsBurned = x.Redemptions.Count * x.Discount.RequiredPoints
                })
                .ToList();

            return new DiscountAnalyticsDto
            {
                TotalDiscountCodes = discountList.Count,
                ActiveDiscountCodes = discountList.Count(d => d.IsActive && (!d.ExpiryDate.HasValue || d.ExpiryDate >= DateTime.UtcNow)),
                TotalRedemptions = redemptionList.Count,
                TotalPointsBurned = redemptionList.Sum(r =>
                    discountList.FirstOrDefault(d => d.Id == r.DiscountCodeId)?.RequiredPoints ?? 0),
                UniqueUsers = redemptionList.Select(r => r.UserId).Distinct().Count(),
                FirstRedemptionDate = redemptionList.MinBy(r => r.RedeemedAt)?.RedeemedAt,
                LastRedemptionDate = redemptionList.MaxBy(r => r.RedeemedAt)?.RedeemedAt,
                TopPerformingDiscounts = topPerforming
            };
        }
    }

    private static AdminDiscountTypeDto MapToAdminDto(DiscountCode discount)
    {
        return new AdminDiscountTypeDto
        {
            Id = discount.Id,
            Code = discount.Code,
            DiscountPercentage = discount.DiscountPercentage,
            RequiredPoints = discount.RequiredPoints,
            MaxRedemptions = discount.MaxRedemptions,
            CurrentRedemptions = discount.CurrentRedemptions,
            ExpiryDate = discount.ExpiryDate,
            IsActive = discount.IsActive,
            CreatedAt = discount.CreatedAt,
            UpdatedAt = discount.UpdatedAt
        };
    }
}
