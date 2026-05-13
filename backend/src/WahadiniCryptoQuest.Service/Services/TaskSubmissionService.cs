using System.Text.Json;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.DTOs.Task;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;

namespace WahadiniCryptoQuest.Service.Services;

public class TaskSubmissionService : ITaskSubmissionService
{
    private readonly IUnitOfWork _unitOfWork;

    public TaskSubmissionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TaskSubmissionResponseDto>> SubmitTaskAsync(
        TaskSubmissionRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var task = await _unitOfWork.LearningTasks.GetByIdAsync(request.TaskId);
        if (task == null)
        {
            return Result<TaskSubmissionResponseDto>.Failure("Task not found", "TASK_NOT_FOUND");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return Result<TaskSubmissionResponseDto>.Failure("User not found", "USER_NOT_FOUND");
        }

        var existing = await _unitOfWork.TaskSubmissions.GetUserTaskSubmissionAsync(userId, request.TaskId, cancellationToken);
        if (existing != null)
        {
            // Allow resubmission only if rejected, prevent duplicate pending/approved submissions
            if (existing.Status == SubmissionStatus.Pending)
            {
                return Result<TaskSubmissionResponseDto>.Failure("You already have a pending submission for this task", "DUPLICATE_SUBMISSION");
            }
            if (existing.Status == SubmissionStatus.Approved)
            {
                return Result<TaskSubmissionResponseDto>.Failure("You have already completed this task", "DUPLICATE_SUBMISSION");
            }
            // If rejected, allow resubmission by soft deleting the old submission
            if (existing.Status == SubmissionStatus.Rejected)
            {
                existing.IsDeleted = true;
                existing.DeletedAt = DateTime.UtcNow;
            }
        }

        var submission = new UserTaskSubmission
        {
            UserId = userId,
            TaskId = request.TaskId,
            SubmissionData = request.SubmissionData,
            Status = SubmissionStatus.Pending,
            SubmittedAt = DateTime.UtcNow,
            FeedbackText = request.Notes
        };

        switch (task.TaskType)
        {
            case TaskType.Quiz:
                await HandleQuizSubmission(task, submission, request.SubmissionData);
                break;
            case TaskType.ExternalLink:
                if (string.IsNullOrWhiteSpace(request.SubmissionData))
                    return Result<TaskSubmissionResponseDto>.Failure("Link is required", "VALIDATION_ERROR");
                break;
            case TaskType.TextSubmission:
                if (string.IsNullOrWhiteSpace(request.SubmissionData))
                    return Result<TaskSubmissionResponseDto>.Failure("Text is required", "VALIDATION_ERROR");
                break;
            case TaskType.WalletVerification:
                if (string.IsNullOrWhiteSpace(request.SubmissionData))
                    return Result<TaskSubmissionResponseDto>.Failure("Wallet address is required", "VALIDATION_ERROR");
                break;
            case TaskType.Screenshot:
                if (string.IsNullOrWhiteSpace(request.SubmissionData))
                    return Result<TaskSubmissionResponseDto>.Failure("Screenshot is required", "VALIDATION_ERROR");

                // Validate file path/url structure if needed
                // The actual file upload happens in Controller which saves file and passes path in SubmissionData
                // Here we just verify the data exists
                break;
        }

        await _unitOfWork.TaskSubmissions.AddAsync(submission);

        // If auto-approved (Quiz), create transaction
        if (submission.Status == SubmissionStatus.Approved && submission.RewardPointsAwarded > 0)
        {
            var transaction = RewardTransaction.Create(
                userId,
                submission.RewardPointsAwarded,
                TransactionType.TaskApproval,
                $"Completed task: {task.Title}",
                user.CurrentPoints + submission.RewardPointsAwarded,
                submission.Id.ToString(),
                "TaskSubmission"
            );
            user.AwardPoints(submission.RewardPointsAwarded);
            await _unitOfWork.RewardTransactions.AddAsync(transaction);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TaskSubmissionResponseDto>.Success(new TaskSubmissionResponseDto
        {
            SubmissionId = submission.Id,
            Status = submission.Status,
            PointsAwarded = submission.RewardPointsAwarded,
            Message = submission.Status == SubmissionStatus.Approved ? "Approved" : "Submitted successfully"
        });
    }

    public async Task<Result<bool>> ReviewSubmissionAsync(
        Guid submissionId,
        Guid reviewerId,
        bool isApproved,
        string feedback,
        int pointsAwarded,
        byte[] rowVersion,
        CancellationToken cancellationToken = default)
    {
        var submission = await _unitOfWork.TaskSubmissions.GetByIdAsync(submissionId);
        if (submission == null)
        {
            return Result<bool>.Failure("Submission not found", "NOT_FOUND");
        }

        // Optimistic Locking Check
        if (!submission.Version.SequenceEqual(rowVersion))
        {
            return Result<bool>.Failure("Submission has been modified by another user", "CONCURRENCY_CONFLICT");
        }

        if (submission.Status != SubmissionStatus.Pending)
        {
            return Result<bool>.Failure("Submission is not pending", "INVALID_STATUS");
        }

        if (isApproved)
        {
            submission.Approve(reviewerId, pointsAwarded, feedback);

            if (pointsAwarded > 0)
            {
                var task = await _unitOfWork.LearningTasks.GetByIdAsync(submission.TaskId);
                var title = task?.Title ?? "Unknown Task";
                var user = await _unitOfWork.Users.GetByIdAsync(submission.UserId);
                if (user != null)
                {
                    user.AwardPoints(pointsAwarded);
                    var transaction = RewardTransaction.Create(
                        submission.UserId,
                        pointsAwarded,
                        TransactionType.TaskApproval,
                        $"Approved task: {title}",
                        user.CurrentPoints,
                        submission.Id.ToString(),
                        "TaskSubmission"
                    );
                    await _unitOfWork.RewardTransactions.AddAsync(transaction);
                }
            }
        }
        else
        {
            submission.Reject(reviewerId, feedback);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }

    public async Task<Result<IEnumerable<UserTaskSubmissionDto>>> GetMySubmissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var submissions = await _unitOfWork.TaskSubmissions.GetUserSubmissionsAsync(userId, cancellationToken);

        // Map to DTOs to avoid circular reference issues
        var submissionDtos = submissions.Select(s => new UserTaskSubmissionDto
        {
            Id = s.Id,
            UserId = s.UserId,
            TaskId = s.TaskId,
            SubmissionData = s.SubmissionData,
            Status = s.Status,
            FeedbackText = s.FeedbackText,
            SubmittedAt = s.SubmittedAt,
            RewardPointsAwarded = s.RewardPointsAwarded,
            Task = s.Task != null ? new LearningTaskDto
            {
                Id = s.Task.Id,
                LessonId = s.Task.LessonId,
                Title = s.Task.Title,
                Description = s.Task.Description,
                TaskType = s.Task.TaskType,
                TaskData = s.Task.TaskData,
                RewardPoints = s.Task.RewardPoints,
                TimeLimit = s.Task.TimeLimit,
                OrderIndex = s.Task.OrderIndex,
                IsRequired = s.Task.IsRequired
            } : null
        }).ToList();

        return Result<IEnumerable<UserTaskSubmissionDto>>.Success(submissionDtos);
    }

    public async Task<Result<BulkReviewResponseDto>> BulkReviewAsync(
        List<Guid> submissionIds,
        Guid reviewerId,
        bool isApproved,
        string feedback,
        int pointsAwarded,
        CancellationToken cancellationToken = default)
    {
        var response = new BulkReviewResponseDto();

        foreach (var id in submissionIds)
        {
            try
            {
                var submission = await _unitOfWork.TaskSubmissions.GetByIdAsync(id);
                if (submission == null || submission.Status != SubmissionStatus.Pending)
                {
                    response.FailureCount++;
                    response.Errors.Add($"Submission {id} not found or not pending");
                    continue;
                }

                if (isApproved)
                {
                    submission.Approve(reviewerId, pointsAwarded, feedback);
                    if (pointsAwarded > 0)
                    {
                        var user = await _unitOfWork.Users.GetByIdAsync(submission.UserId);
                        if (user != null)
                        {
                            user.AwardPoints(pointsAwarded);
                            var transaction = RewardTransaction.Create(
                                submission.UserId,
                                pointsAwarded,
                                TransactionType.TaskApproval,
                                "Bulk Approved",
                                user.CurrentPoints,
                                submission.Id.ToString(),
                                "TaskSubmission"
                            );
                            await _unitOfWork.RewardTransactions.AddAsync(transaction);
                        }
                    }
                }
                else
                {
                    submission.Reject(reviewerId, feedback);
                }

                response.SuccessCount++;
            }
            catch (Exception ex)
            {
                response.FailureCount++;
                response.Errors.Add($"Error processing {id}: {ex.Message}");
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<BulkReviewResponseDto>.Success(response);
    }

    public async Task<Result<TaskSubmissionStatusDto>> GetSubmissionStatusAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var submission = await _unitOfWork.TaskSubmissions.GetUserTaskSubmissionAsync(userId, taskId, cancellationToken);

        if (submission == null)
        {
            // User has never submitted this task
            return Result<TaskSubmissionStatusDto>.Success(new TaskSubmissionStatusDto
            {
                TaskId = taskId,
                UserId = userId,
                HasSubmitted = false
            });
        }

        // User has submitted - return full status
        var statusDto = new TaskSubmissionStatusDto
        {
            SubmissionId = submission.Id,
            TaskId = taskId,
            UserId = userId,
            Status = submission.Status,
            SubmittedAt = submission.SubmittedAt,
            ReviewedAt = submission.ReviewedAt,
            FeedbackText = submission.FeedbackText,
            RewardPointsAwarded = submission.RewardPointsAwarded,
            HasSubmitted = true
        };

        return Result<TaskSubmissionStatusDto>.Success(statusDto);
    }

    private async Task HandleQuizSubmission(LearningTask task, UserTaskSubmission submission, string submissionDataJson)
    {
        try
        {
            var taskData = JsonSerializer.Deserialize<QuizTaskDataDto>(task.TaskData);
            if (taskData == null || taskData.Questions.Count == 0) return;

            var submissionDto = JsonSerializer.Deserialize<QuizSubmissionDto>(submissionDataJson);
            if (submissionDto == null) return;

            int correctCount = 0;
            foreach (var (qIndex, optionIndex) in submissionDto.Answers)
            {
                if (qIndex >= 0 && qIndex < taskData.Questions.Count)
                {
                    if (taskData.Questions[qIndex].CorrectOption == optionIndex)
                    {
                        correctCount++;
                    }
                }
            }

            double score = (double)correctCount / taskData.Questions.Count * 100;
            bool passed = score >= taskData.PassingScore;

            if (passed)
            {
                submission.Approve(Guid.Empty, task.RewardPoints, $"Auto-graded: Passed with {score}%");
            }
            else
            {
                submission.Reject(Guid.Empty, $"Auto-graded: Failed with {score}% (Required: {taskData.PassingScore}%)");
            }
        }
        catch
        {
            // Ignore parsing errors
        }
    }
}