namespace WahadiniCryptoQuest.Core.DTOs.Admin;

/// <summary>
/// DTO representing an audit log entry for admin dashboard.
/// Contains full before/after JSON values for compliance tracking.
/// </summary>
public class AuditLogDto
{
    public Guid Id { get; set; }
    public Guid AdminUserId { get; set; }
    public string AdminUserEmail { get; set; } = string.Empty;
    public string AdminUserName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string? BeforeValue { get; set; }
    public string? AfterValue { get; set; }
    public string IPAddress { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
