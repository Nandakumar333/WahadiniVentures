using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for UserTaskSubmission entity with submission operations
/// </summary>
public interface IUserTaskSubmissionRepository : IRepository<UserTaskSubmission>
{
    Task<PagedResult<UserTaskSubmission>> GetByStatusAsync(SubmissionStatus status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<UserTaskSubmission>> GetPendingReviewQueueAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    
    // Added methods
    Task<IEnumerable<UserTaskSubmission>> GetUserSubmissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserTaskSubmission?> GetUserTaskSubmissionAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
}
