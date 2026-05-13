namespace WahadiniCryptoQuest.Core.Interfaces.Services;

/// <summary>
/// Service interface for queuing notifications with retry capability
/// T088: Notification queue with exponential backoff retry
/// </summary>
public interface INotificationQueue
{
    /// <summary>
    /// Queues a notification for delivery with automatic retry on failure
    /// </summary>
    /// <param name="userId">Target user ID</param>
    /// <param name="title">Notification title</param>
    /// <param name="message">Notification message</param>
    /// <param name="notificationType">Type of notification (Achievement, Points, Streak, etc.)</param>
    /// <param name="metadata">Optional metadata (e.g., achievement ID, points amount)</param>
    Task QueueNotificationAsync(
        Guid userId,
        string title,
        string message,
        string notificationType,
        Dictionary<string, string>? metadata = null);

    /// <summary>
    /// Gets pending notification count for a user
    /// </summary>
    Task<int> GetPendingCountAsync(Guid userId);
}
