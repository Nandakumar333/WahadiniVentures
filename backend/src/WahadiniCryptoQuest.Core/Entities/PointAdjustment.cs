using System;
using WahadiniCryptoQuest.Core.Common;
namespace WahadiniCryptoQuest.Core.Entities
{
    /// <summary>
    /// Represents a manual adjustment to a user's point balance by an admin
    /// Tracks reason and admin who made the adjustment for audit purposes
    /// </summary>
    public class PointAdjustment : BaseEntity
    {
        // Private parameterless constructor for EF Core
        private PointAdjustment() { }

        /// <summary>
        /// Factory method to create a new point adjustment
        /// </summary>
        public static PointAdjustment Create(
            Guid userId,
            int previousBalance,
            int adjustmentAmount,
            string reason,
            Guid adminUserId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty", nameof(userId));

            if (previousBalance < 0)
                throw new ArgumentException("Previous balance cannot be negative", nameof(previousBalance));

            if (adjustmentAmount == 0)
                throw new ArgumentException("Adjustment amount cannot be zero", nameof(adjustmentAmount));

            var newBalance = previousBalance + adjustmentAmount;
            if (newBalance < 0)
                throw new ArgumentException("Adjustment would result in negative balance", nameof(adjustmentAmount));

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Reason is required for point adjustments", nameof(reason));

            if (reason.Length > 500)
                throw new ArgumentException("Reason cannot exceed 500 characters", nameof(reason));

            if (adminUserId == Guid.Empty)
                throw new ArgumentException("AdminUserId cannot be empty", nameof(adminUserId));

            return new PointAdjustment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PreviousBalance = previousBalance,
                AdjustmentAmount = adjustmentAmount,
                NewBalance = newBalance,
                Reason = reason,
                AdminUserId = adminUserId,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// ID of the user whose points were adjusted
        /// </summary>
        public Guid UserId { get; private set; }

        /// <summary>
        /// Point balance before the adjustment
        /// </summary>
        public int PreviousBalance { get; private set; }

        /// <summary>
        /// Amount added or subtracted (can be positive or negative)
        /// </summary>
        public int AdjustmentAmount { get; private set; }

        /// <summary>
        /// Point balance after the adjustment
        /// </summary>
        public int NewBalance { get; private set; }

        /// <summary>
        /// Admin's explanation for the adjustment
        /// </summary>
        public string Reason { get; private set; } = string.Empty;

        /// <summary>
        /// ID of the admin who made the adjustment
        /// </summary>
        public Guid AdminUserId { get; private set; }

        /// <summary>
        /// UTC timestamp when the adjustment was made
        /// </summary>
        public DateTime Timestamp { get; private set; }

        // Navigation properties
        public virtual User? User { get; private set; }
        public virtual User? AdminUser { get; private set; }
    }
}
