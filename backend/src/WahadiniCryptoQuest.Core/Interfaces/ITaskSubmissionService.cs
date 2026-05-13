using WahadiniCryptoQuest.Core.DTOs.Task;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces;

public interface ITaskSubmissionService
{
    Task<Result<TaskSubmissionResponseDto>> SubmitTaskAsync(
        TaskSubmissionRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> ReviewSubmissionAsync(
        Guid submissionId,
        Guid reviewerId,
        bool isApproved,
        string feedback,
        int pointsAwarded,
        byte[] rowVersion,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<UserTaskSubmissionDto>>> GetMySubmissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<BulkReviewResponseDto>> BulkReviewAsync(
        List<Guid> submissionIds,
        Guid reviewerId,
        bool isApproved,
        string feedback,
        int pointsAwarded,
        CancellationToken cancellationToken = default);

    Task<Result<TaskSubmissionStatusDto>> GetSubmissionStatusAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken = default);
}