using System;
using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Core.Entities
{
    /// <summary>
    /// Represents an in-app notification for users
    /// Used for task review notifications and other admin actions affecting users
    /// </summary>
    public class UserNotification : BaseEntity
    {
        // Private parameterless constructor for EF Core
        private UserNotification() { }

        /// <summary>
        /// Factory method to create a new user notification
        /// </summary>
        public static UserNotification Create(
            Guid userId,
            string type,
            string message)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Notification type is required", nameof(type));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message is required", nameof(message));

            if (message.Length > 500)
                throw new ArgumentException("Message cannot exceed 500 characters", nameof(message));

            return new UserNotification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Message = message,
                IsRead = false
                // CreatedAt is automatically set by BaseEntity constructor
            };
        }

        /// <summary>
        /// ID of the user who will receive this notification
        /// </summary>
        public Guid UserId { get; private set; }

        /// <summary>
        /// Type of notification (e.g., "TASK_APPROVED", "TASK_REJECTED", "ACCOUNT_BANNED")
        /// </summary>
        public string Type { get; private set; } = string.Empty;

        /// <summary>
        /// Notification message content
        /// </summary>
        public string Message { get; private set; } = string.Empty;

        /// <summary>
        /// Indicates whether the user has read this notification
        /// </summary>
        public bool IsRead { get; private set; }

        // Inherits CreatedAt from BaseEntity

        // Navigation property
        public virtual User? User { get; private set; }

        /// <summary>
        /// Marks the notification as read
        /// </summary>
        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}
