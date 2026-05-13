using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.DTOs.Admin;

/// <summary>
/// DTO for paginated user list with summary information
/// T058: US3 - User Account Management
/// </summary>
public class PaginatedUsersDto
{
    public List<UserSummaryDto> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}

/// <summary>
/// Summary DTO for user in list view
/// </summary>
public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRoleEnum Role { get; set; }
    public int CurrentPoints { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool IsActive { get; set; }
    public bool IsBanned { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// Detailed DTO for individual user view
/// T059: US3 - User Account Management
/// </summary>
public class UserDetailDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRoleEnum Role { get; set; }
    public int CurrentPoints { get; set; }
    public int TotalPointsEarned { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool EmailVerified { get; set; }
    public bool IsActive { get; set; }
    public bool IsBanned { get; set; }
    public string? BanReason { get; set; }
    public DateTime? BannedAt { get; set; }
    public Guid? BannedBy { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Subscription info
    public bool HasActiveSubscription { get; set; }
    public string? SubscriptionStatus { get; set; }
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }

    // Activity stats
    public int EnrolledCoursesCount { get; set; }
    public int CompletedTasksCount { get; set; }
    public int PendingTasksCount { get; set; }
}

/// <summary>
/// DTO for updating user role
/// T060: US3 - User Account Management
/// </summary>
public class UpdateUserRoleDto
{
    public UserRoleEnum Role { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for banning/unbanning user
/// T061: US3 - User Account Management
/// </summary>
public class BanUserDto
{
    public string Reason { get; set; } = string.Empty;
}
