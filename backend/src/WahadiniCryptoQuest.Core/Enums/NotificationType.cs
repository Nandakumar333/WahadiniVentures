namespace WahadiniCryptoQuest.Core.Enums;

/// <summary>
/// Types of user notifications for in-app notification system.
/// Used to categorize and filter notifications by purpose.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Task submission was approved by admin/instructor
    /// </summary>
    TaskReviewApproved = 1,

    /// <summary>
    /// Task submission was rejected by admin/instructor with feedback
    /// </summary>
    TaskReviewRejected = 2,

    /// <summary>
    /// Administrative action affecting user account (ban, role change, etc.)
    /// </summary>
    AdminAction = 3,

    /// <summary>
    /// Points were manually adjusted by admin
    /// </summary>
    PointAdjustment = 4,

    /// <summary>
    /// System notification for general announcements
    /// </summary>
    SystemAnnouncement = 5,

    /// <summary>
    /// Achievement unlocked or reward earned
    /// </summary>
    Achievement = 6,

    /// <summary>
    /// Course or lesson-related update
    /// </summary>
    CourseUpdate = 7
}
