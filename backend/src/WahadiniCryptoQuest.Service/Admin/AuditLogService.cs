using AutoMapper;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Admin;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Admin;

/// <summary>
/// Service implementation for audit logging of administrative actions.
/// Provides compliance tracking with before/after state capture in JSONB format.
/// T012: AuditLogService with repository pattern and JSONB serialization
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogEntryRepository _auditLogRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        IAuditLogEntryRepository auditLogRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<AuditLogService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task LogActionAsync(
        Guid adminUserId,
        string actionType,
        string resourceType,
        string resourceId,
        string? beforeValue,
        string? afterValue,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify admin user exists
            var adminUser = await _userRepository.GetByIdAsync(adminUserId);
            if (adminUser == null)
            {
                _logger.LogWarning("Attempted to log action for non-existent admin user {AdminUserId}", adminUserId);
                return;
            }

            // Create audit log entry using factory method
            var auditEntry = AuditLogEntry.Create(
                adminUserId: adminUserId,
                actionType: actionType,
                resourceType: resourceType,
                resourceId: resourceId,
                beforeValue: beforeValue,
                afterValue: afterValue,
                ipAddress: ipAddress);

            // Persist to database
            await _auditLogRepository.AddAsync(auditEntry);

            _logger.LogInformation(
                "Audit log created: Admin {AdminUserId} performed {ActionType} on {ResourceType}/{ResourceId}",
                adminUserId, actionType, resourceType, resourceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log audit action for admin {AdminUserId} on {ResourceType}/{ResourceId}",
                adminUserId, resourceType, resourceId);
            // Don't throw - audit logging failure should not block operations
        }
    }

    public async Task<PagedResultDto<AuditLogDto>> GetAuditLogsAsync(
        AuditLogFilterDto filters,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _auditLogRepository.GetFilteredAsync(filters, cancellationToken);

        // Map entities to DTOs with admin user details
        var dtos = new List<AuditLogDto>();
        foreach (var entry in items)
        {
            var adminUser = await _userRepository.GetByIdAsync(entry.AdminUserId);

            var dto = _mapper.Map<AuditLogDto>(entry);
            if (adminUser != null)
            {
                dto.AdminUserEmail = adminUser.Email;
                dto.AdminUserName = $"{adminUser.FirstName} {adminUser.LastName}";
            }

            dtos.Add(dto);
        }

        return new PagedResultDto<AuditLogDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };
    }

    public async Task<AuditLogDto?> GetAuditLogByIdAsync(
        Guid auditLogId,
        CancellationToken cancellationToken = default)
    {
        var entry = await _auditLogRepository.GetByIdAsync(auditLogId);
        if (entry == null)
            return null;

        var adminUser = await _userRepository.GetByIdAsync(entry.AdminUserId);

        var dto = _mapper.Map<AuditLogDto>(entry);
        if (adminUser != null)
        {
            dto.AdminUserEmail = adminUser.Email;
            dto.AdminUserName = $"{adminUser.FirstName} {adminUser.LastName}";
        }

        return dto;
    }

    public async Task<PagedResultDto<AuditLogDto>> GetResourceHistoryAsync(
        string resourceType,
        string resourceId,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _auditLogRepository.GetResourceHistoryAsync(
            resourceType, resourceId, pageNumber, pageSize, cancellationToken);

        // Map entities to DTOs with admin user details
        var dtos = new List<AuditLogDto>();
        foreach (var entry in items)
        {
            var adminUser = await _userRepository.GetByIdAsync(entry.AdminUserId);

            var dto = _mapper.Map<AuditLogDto>(entry);
            if (adminUser != null)
            {
                dto.AdminUserEmail = adminUser.Email;
                dto.AdminUserName = $"{adminUser.FirstName} {adminUser.LastName}";
            }

            dtos.Add(dto);
        }

        return new PagedResultDto<AuditLogDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
