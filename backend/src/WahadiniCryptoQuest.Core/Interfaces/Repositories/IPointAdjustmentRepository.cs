using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for point adjustment operations.
/// Provides data access for manual point balance adjustments by admins.
/// </summary>
public interface IPointAdjustmentRepository : IRepository<PointAdjustment>
{
    /// <summary>
    /// Retrieves point adjustment history for a user with pagination.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated point adjustments ordered by Timestamp descending</returns>
    Task<(List<PointAdjustment> Items, int TotalCount)> GetByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves point adjustments made by a specific admin with pagination.
    /// </summary>
    /// <param name="adminUserId">Admin user ID</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated point adjustments ordered by Timestamp descending</returns>
    Task<(List<PointAdjustment> Items, int TotalCount)> GetByAdminUserIdAsync(
        Guid adminUserId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
