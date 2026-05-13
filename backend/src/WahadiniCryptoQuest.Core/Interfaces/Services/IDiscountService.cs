using WahadiniCryptoQuest.Core.DTOs.Discount;

namespace WahadiniCryptoQuest.Core.Interfaces.Services;

/// <summary>
/// Service interface for discount code management and redemption
/// Handles business logic for point-based discount system
/// </summary>
public interface IDiscountService
{
    /// <summary>
    /// Gets all available discount codes for a user based on their point balance
    /// </summary>
    /// <param name="userId">User ID to check point balance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available discount codes</returns>
    Task<IEnumerable<DiscountTypeDto>> GetAvailableDiscountsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Redeems a discount code for a user
    /// Deducts points and creates redemption record in atomic transaction
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="discountCodeId">Discount code ID to redeem</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Redemption details with issued code</returns>
    Task<RedemptionResponseDto> RedeemDiscountAsync(Guid userId, Guid discountCodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user's redemption history with pagination
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of user redemptions</returns>
    Task<(IEnumerable<UserRedemptionDto> Items, int TotalCount)> GetMyRedemptionsAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets discount analytics (admin only)
    /// </summary>
    /// <param name="discountCodeId">Optional discount code ID for specific analytics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Discount analytics data</returns>
    Task<DiscountAnalyticsDto> GetDiscountAnalyticsAsync(Guid? discountCodeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new discount code (admin only)
    /// </summary>
    /// <param name="dto">Discount code creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created discount code</returns>
    Task<AdminDiscountTypeDto> CreateDiscountCodeAsync(CreateDiscountCodeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing discount code (admin only)
    /// </summary>
    /// <param name="discountCodeId">Discount code ID</param>
    /// <param name="dto">Update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated discount code</returns>
    Task<AdminDiscountTypeDto> UpdateDiscountCodeAsync(Guid discountCodeId, UpdateDiscountCodeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a discount code (admin only)
    /// </summary>
    /// <param name="discountCodeId">Discount code ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeactivateDiscountCodeAsync(Guid discountCodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a discount code (admin only)
    /// </summary>
    /// <param name="discountCodeId">Discount code ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ActivateDiscountCodeAsync(Guid discountCodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a discount code (admin only)
    /// Preserves historical redemptions
    /// </summary>
    /// <param name="discountCodeId">Discount code ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteDiscountCodeAsync(Guid discountCodeId, CancellationToken cancellationToken = default);
}
