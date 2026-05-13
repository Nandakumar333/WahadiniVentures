using WahadiniCryptoQuest.Core.DTOs.Admin;

namespace WahadiniCryptoQuest.Core.Interfaces.Services;

/// <summary>
/// Service interface for audit logging of administrative actions.
/// Provides compliance tracking with before/after state capture in JSONB format.
/// T010: IAuditLogService interface for admin action tracking
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Logs an administrative action with before/after state for audit trail.
    /// </summary>
    /// <param name="adminUserId">ID of the admin user performing the action</param>
    /// <param name="actionType">Type of action (POST, PUT, DELETE, or custom action name)</param>
    /// <param name="resourceType">Type of resource being modified (users, courses, submissions, etc.)</param>
    /// <param name="resourceId">ID of the specific resource instance</param>
    /// <param name="beforeValue">JSON representation of resource state before modification (nullable for CREATE)</param>
    /// <param name="afterValue">JSON representation of resource state after modification (nullable for DELETE)</param>
    /// <param name="ipAddress">IP address of the admin user making the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completing when audit log is persisted</returns>
    Task LogActionAsync(
        Guid adminUserId,
        string actionType,
        string resourceType,
        string resourceId,
        string? beforeValue,
        string? afterValue,
        string ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit logs with filtering, sorting, and pagination.
    /// </summary>
    /// <param name="filters">Filter criteria including admin user, action type, resource type, date range</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of audit log entries with admin user details</returns>
    Task<PagedResultDto<AuditLogDto>> GetAuditLogsAsync(
        AuditLogFilterDto filters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific audit log entry by ID.
    /// </summary>
    /// <param name="auditLogId">ID of the audit log entry</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Audit log entry with full before/after JSON values, or null if not found</returns>
    Task<AuditLogDto?> GetAuditLogByIdAsync(
        Guid auditLogId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit logs for a specific resource to show modification history.
    /// </summary>
    /// <param name="resourceType">Type of resource (users, courses, etc.)</param>
    /// <param name="resourceId">ID of the resource</param>
    /// <param name="pageNumber">Page number for pagination (1-based)</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated audit history for the resource, ordered by timestamp descending</returns>
    Task<PagedResultDto<AuditLogDto>> GetResourceHistoryAsync(
        string resourceType,
        string resourceId,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}
