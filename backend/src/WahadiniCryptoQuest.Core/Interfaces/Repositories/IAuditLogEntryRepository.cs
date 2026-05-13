using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.DTOs.Admin;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for audit log entry operations.
/// Provides data access for administrative action tracking and compliance.
/// </summary>
public interface IAuditLogEntryRepository : IRepository<AuditLogEntry>
{
    /// <summary>
    /// Retrieves audit logs with filtering and pagination.
    /// </summary>
    /// <param name="filters">Filter criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated audit log entries with admin user details</returns>
    Task<(List<AuditLogEntry> Items, int TotalCount)> GetFilteredAsync(
        AuditLogFilterDto filters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit history for a specific resource.
    /// </summary>
    /// <param name="resourceType">Type of resource</param>
    /// <param name="resourceId">ID of resource</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated audit logs for the resource</returns>
    Task<(List<AuditLogEntry> Items, int TotalCount)> GetResourceHistoryAsync(
        string resourceType,
        string resourceId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
