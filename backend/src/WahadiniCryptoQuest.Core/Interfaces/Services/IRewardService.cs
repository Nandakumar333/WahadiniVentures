using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Interfaces.Services;

/// <summary>
/// Service for managing reward points and transactions
/// </summary>
public interface IRewardService
{
    /// <summary>
    /// Awards points to a user for a specific action
    /// </summary>
    /// <param name="userId">The user receiving points</param>
    /// <param name="amount">Amount of points to award</param>
    /// <param name="type">Type of transaction</param>
    /// <param name="description">Description of the reward</param>
    /// <param name="referenceId">Optional reference ID (e.g., LessonId)</param>
    /// <param name="referenceType">Optional reference type (e.g., "Lesson")</param>
    /// <param name="adminUserId">Optional admin user ID for manual adjustments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created transaction ID</returns>
    Task<Guid> AwardPointsAsync(
        Guid userId,
        int amount,
        TransactionType type,
        string description,
        string? referenceId = null,
        string? referenceType = null,
        Guid? adminUserId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deducts points from a user (e.g., for redemption or penalty)
    /// </summary>
    Task<Guid> DeductPointsAsync(
        Guid userId,
        int amount,
        TransactionType type,
        string description,
        string? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current point balance for a user
    /// </summary>
    Task<int> GetUserBalanceAsync(Guid userId, CancellationToken cancellationToken = default);
}
