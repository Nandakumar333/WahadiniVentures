using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Admin;

namespace WahadiniCryptoQuest.Service.Queries.Admin;

/// <summary>
/// Query to retrieve pending task submissions for admin review
/// T044: US2 - Task Review Workflow
/// </summary>
public class GetPendingTasksQuery : IRequest<List<PendingTaskDto>>
{
    /// <summary>
    /// Filter by submissions from this date
    /// </summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>
    /// Filter by submissions until this date
    /// </summary>
    public DateTime? DateTo { get; set; }

    /// <summary>
    /// Filter by specific course
    /// </summary>
    public Guid? CourseId { get; set; }

    /// <summary>
    /// Page number for pagination (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size for pagination
    /// </summary>
    public int PageSize { get; set; } = 20;
}
