using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for user notification operations.
/// Provides data access for in-app notification management.
/// </summary>
public interface IUserNotificationRepository : IRepository<UserNotification>
{
    /// <summary>
    /// Retrieves unread notifications for a user with pagination.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated unread notifications ordered by CreatedAt descending</returns>
    Task<(List<UserNotification> Items, int TotalCount)> GetUnreadAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all notifications for a user with optional read status filter.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="isRead">Optional read status filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated notifications ordered by CreatedAt descending</returns>
    Task<(List<UserNotification> Items, int TotalCount)> GetByUserIdAsync(
        Guid userId,
        bool? isRead,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of unread notifications for a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of unread notifications</returns>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks all unread notifications as read for a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of notifications marked as read</returns>
    Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}
