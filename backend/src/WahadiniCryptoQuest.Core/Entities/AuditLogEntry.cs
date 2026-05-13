using System;
using WahadiniCryptoQuest.Core.Common;
namespace WahadiniCryptoQuest.Core.Entities
{
    /// <summary>
    /// Represents an immutable audit log entry for administrative actions
    /// Tracks who did what, when, and captures before/after state for compliance
    /// </summary>
    public class AuditLogEntry : BaseEntity
    {
        // Private parameterless constructor for EF Core
        private AuditLogEntry() { }

        /// <summary>
        /// Factory method to create a new audit log entry
        /// </summary>
        public static AuditLogEntry Create(
            Guid adminUserId,
            string actionType,
            string resourceType,
            string resourceId,
            string? beforeValue,
            string? afterValue,
            string ipAddress)
        {
            if (adminUserId == Guid.Empty)
                throw new ArgumentException("AdminUserId cannot be empty", nameof(adminUserId));

            if (string.IsNullOrWhiteSpace(actionType))
                throw new ArgumentException("ActionType is required", nameof(actionType));

            if (string.IsNullOrWhiteSpace(resourceType))
                throw new ArgumentException("ResourceType is required", nameof(resourceType));

            if (string.IsNullOrWhiteSpace(resourceId))
                throw new ArgumentException("ResourceId is required", nameof(resourceId));

            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new ArgumentException("IP Address is required", nameof(ipAddress));

            return new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                AdminUserId = adminUserId,
                ActionType = actionType,
                ResourceType = resourceType,
                ResourceId = resourceId,
                BeforeValue = beforeValue,
                AfterValue = afterValue,
                IPAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// ID of the admin user who performed the action
        /// </summary>
        public Guid AdminUserId { get; private set; }

        /// <summary>
        /// Type of action performed (e.g., "UPDATE_USER_ROLE", "BAN_USER", "APPROVE_TASK")
        /// </summary>
        public string ActionType { get; private set; } = string.Empty;

        /// <summary>
        /// Type of resource affected (e.g., "User", "Course", "TaskSubmission")
        /// </summary>
        public string ResourceType { get; private set; } = string.Empty;

        /// <summary>
        /// ID of the affected resource (stored as string for flexibility with different ID types)
        /// </summary>
        public string ResourceId { get; private set; } = string.Empty;

        /// <summary>
        /// JSONB serialized state before the action (nullable for CREATE operations)
        /// </summary>
        public string? BeforeValue { get; private set; }

        /// <summary>
        /// JSONB serialized state after the action (nullable for DELETE operations)
        /// </summary>
        public string? AfterValue { get; private set; }

        /// <summary>
        /// IP address from which the action was performed
        /// </summary>
        public string IPAddress { get; private set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the action occurred
        /// </summary>
        public DateTime Timestamp { get; private set; }

        // Navigation property
        public virtual User? AdminUser { get; protected set; }
    }
}
