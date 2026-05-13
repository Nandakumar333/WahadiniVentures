using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Admin;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Service.Queries.Admin;

/// <summary>
/// Query to retrieve paginated list of users with filters
/// T062: US3 - User Account Management
/// </summary>
public class GetUsersQuery : IRequest<PaginatedUsersDto>
{
    /// <summary>
    /// Search by email or name
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by role
    /// </summary>
    public UserRoleEnum? Role { get; set; }

    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Filter by banned status
    /// </summary>
    public bool? IsBanned { get; set; }

    /// <summary>
    /// Filter by email confirmed status
    /// </summary>
    public bool? EmailConfirmed { get; set; }

    /// <summary>
    /// Filter by subscription status
    /// </summary>
    public bool? HasActiveSubscription { get; set; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sort by field (Email, CreatedAt, LastLoginAt, Points)
    /// </summary>
    public string SortBy { get; set; } = "CreatedAt";

    /// <summary>
    /// Sort descending
    /// </summary>
    public bool SortDescending { get; set; } = true;
}
