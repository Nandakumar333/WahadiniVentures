using WahadiniCryptoQuest.Core.DTOs.Admin;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Interfaces.Services;

/// <summary>
/// Service interface for managing user notifications (in-app and email).
/// Handles task review notifications, admin action alerts, and email templates.
/// T011: INotificationService interface for notification management
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Creates an in-app notification for a user.
    /// </summary>
    /// <param name="userId">ID of the user to notify</param>
    /// <param name="type">Type of notification (TaskReviewApproved, TaskReviewRejected, AdminAction, etc.)</param>
    /// <param name="message">Notification message content (max 500 characters)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created notification DTO with ID</returns>
    Task<UserNotificationDto> CreateInAppNotificationAsync(
        Guid userId,
        NotificationType type,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email notification to a user using a template.
    /// </summary>
    /// <param name="userId">ID of the user to email</param>
    /// <param name="subject">Email subject line</param>
    /// <param name="templateName">Name of email template (TaskApproved, TaskRejected, AccountBanned, etc.)</param>
    /// <param name="templateData">Dictionary of template variable replacements</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if email sent successfully, false otherwise</returns>
    Task<bool> SendEmailAsync(
        Guid userId,
        string subject,
        string templateName,
        Dictionary<string, string> templateData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an in-app notification as read.
    /// </summary>
    /// <param name="notificationId">ID of the notification</param>
    /// <param name="userId">ID of the user (for authorization check)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if marked successfully, false if notification not found or unauthorized</returns>
    Task<bool> MarkAsReadAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves unread notifications for a user.
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="pageNumber">Page number for pagination (1-based)</param>
    /// <param name="pageSize">Number of notifications per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of unread notifications ordered by creation date descending</returns>
    Task<PagedResultDto<UserNotificationDto>> GetUnreadNotificationsAsync(
        Guid userId,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all notifications for a user with filtering.
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="isRead">Filter by read status (null = all, true = read only, false = unread only)</param>
    /// <param name="pageNumber">Page number for pagination (1-based)</param>
    /// <param name="pageSize">Number of notifications per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of notifications ordered by creation date descending</returns>
    Task<PagedResultDto<UserNotificationDto>> GetNotificationsAsync(
        Guid userId,
        bool? isRead = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of unread notifications for a user.
    /// Used for badge display in UI.
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of unread notifications</returns>
    Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks all notifications as read for a user.
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of notifications marked as read</returns>
    Task<int> MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
