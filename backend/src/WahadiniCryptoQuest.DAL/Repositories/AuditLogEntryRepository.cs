using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.DTOs.Admin;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Repository implementation for audit log entry operations.
/// </summary>
public class AuditLogEntryRepository : Repository<AuditLogEntry>, IAuditLogEntryRepository
{
    private new readonly ApplicationDbContext _context;

    public AuditLogEntryRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<(List<AuditLogEntry> Items, int TotalCount)> GetFilteredAsync(
        AuditLogFilterDto filters,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // Apply filters
        if (filters.AdminUserId.HasValue)
            query = query.Where(a => a.AdminUserId == filters.AdminUserId.Value);

        if (!string.IsNullOrWhiteSpace(filters.ActionType))
            query = query.Where(a => a.ActionType == filters.ActionType);

        if (!string.IsNullOrWhiteSpace(filters.ResourceType))
            query = query.Where(a => a.ResourceType == filters.ResourceType);

        if (!string.IsNullOrWhiteSpace(filters.ResourceId))
            query = query.Where(a => a.ResourceId == filters.ResourceId);

        if (filters.StartDate.HasValue)
            query = query.Where(a => a.Timestamp >= filters.StartDate.Value);

        if (filters.EndDate.HasValue)
            query = query.Where(a => a.Timestamp <= filters.EndDate.Value);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = filters.SortDescending
            ? query.OrderByDescending(a => EF.Property<object>(a, filters.SortBy ?? "Timestamp"))
            : query.OrderBy(a => EF.Property<object>(a, filters.SortBy ?? "Timestamp"));

        // Apply pagination
        var items = await query
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<AuditLogEntry> Items, int TotalCount)> GetResourceHistoryAsync(
        string resourceType,
        string resourceId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(a => a.ResourceType == resourceType && a.ResourceId == resourceId)
            .OrderByDescending(a => a.Timestamp);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
