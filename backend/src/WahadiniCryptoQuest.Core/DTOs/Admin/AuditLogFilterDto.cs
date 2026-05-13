namespace WahadiniCryptoQuest.Core.DTOs.Admin;

/// <summary>
/// Filter criteria for querying audit logs with pagination.
/// All filters are optional and combine with AND logic.
/// </summary>
public class AuditLogFilterDto
{
    public Guid? AdminUserId { get; set; }
    public string? ActionType { get; set; }
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "Timestamp";
    public bool SortDescending { get; set; } = true;
}
