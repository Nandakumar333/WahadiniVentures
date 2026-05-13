using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing UserDiscountRedemption entities
/// </summary>
public interface IUserDiscountRedemptionRepository : IRepository<UserDiscountRedemption>
{
    /// <summary>
    /// Checks if a user has already redeemed a specific discount code
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="discountCodeId">The discount code's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the user has already redeemed this code</returns>
    Task<bool> HasUserRedeemedDiscountAsync(
        Guid userId,
        Guid discountCodeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all redemptions for a specific user with pagination
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of redemptions and total count</returns>
    Task<(IEnumerable<UserDiscountRedemption> Items, int TotalCount)> GetUserRedemptionsAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all redemptions for analytics purposes
    /// </summary>
    /// <param name="discountCodeId">Optional filter by discount code ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all redemptions</returns>
    Task<IEnumerable<UserDiscountRedemption>> GetRedemptionsForAnalyticsAsync(
        Guid? discountCodeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all redemptions for a specific discount code
    /// </summary>
    /// <param name="discountCodeId">Discount code ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of redemptions for the discount code</returns>
    Task<IEnumerable<UserDiscountRedemption>> GetByDiscountCodeIdAsync(
        Guid discountCodeId,
        CancellationToken cancellationToken = default);
}
