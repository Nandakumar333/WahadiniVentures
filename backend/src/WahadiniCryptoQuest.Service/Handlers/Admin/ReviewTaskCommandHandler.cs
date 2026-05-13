using MediatR;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Service.Commands.Admin;

namespace WahadiniCryptoQuest.Service.Handlers.Admin;

/// <summary>
/// Handler for reviewing task submissions
/// T047: US2 - Task Review Workflow
/// Awards points, sends notifications, logs audit entries
/// </summary>
public class ReviewTaskCommandHandler : IRequestHandler<ReviewTaskCommand, Unit>
{
    private readonly IUserTaskSubmissionRepository _taskSubmissionRepository;
    private readonly IRewardService _rewardService;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;

    public ReviewTaskCommandHandler(
        IUserTaskSubmissionRepository taskSubmissionRepository,
        IRewardService rewardService,
        INotificationService notificationService,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork)
    {
        _taskSubmissionRepository = taskSubmissionRepository;
        _rewardService = rewardService;
        _notificationService = notificationService;
        _auditLogService = auditLogService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ReviewTaskCommand request, CancellationToken cancellationToken)
    {
        // Get submission
        var submission = await _taskSubmissionRepository.GetByIdAsync(request.SubmissionId);
        if (submission == null)
        {
            throw new InvalidOperationException($"Task submission {request.SubmissionId} not found");
        }

        // Verify submission is pending
        if (submission.Status != SubmissionStatus.Pending)
        {
            throw new InvalidOperationException($"Task submission {request.SubmissionId} has already been reviewed");
        }

        // Update submission status using domain methods
        if (request.Status == SubmissionStatus.Approved)
        {
            submission.Approve(request.AdminUserId, submission.Task.RewardPoints, request.Feedback);

            // Award points using reward service
            await _rewardService.AwardPointsAsync(
                submission.UserId,
                submission.Task.RewardPoints,
                TransactionType.TaskApproval,
                $"Task completed: {submission.Task.Title}",
                submission.TaskId.ToString(),
                "LearningTask",
                request.AdminUserId,
                cancellationToken);
        }
        else if (request.Status == SubmissionStatus.Rejected)
        {
            submission.Reject(request.AdminUserId, request.Feedback ?? "No feedback provided");
        }

        // Send notification to user
        var notificationType = request.Status == SubmissionStatus.Approved
            ? NotificationType.TaskReviewApproved
            : NotificationType.TaskReviewRejected;

        var notificationMessage = request.Status == SubmissionStatus.Approved
            ? $"Your submission for '{submission.Task.Title}' has been approved! You earned {submission.Task.RewardPoints} points."
            : $"Your submission for '{submission.Task.Title}' was rejected. {request.Feedback}";

        await _notificationService.CreateInAppNotificationAsync(
            submission.UserId,
            notificationType,
            notificationMessage,
            cancellationToken);

        // Log audit entry
        var beforeState = System.Text.Json.JsonSerializer.Serialize(new { status = SubmissionStatus.Pending });
        var afterState = System.Text.Json.JsonSerializer.Serialize(new { status = request.Status, feedback = request.Feedback });

        await _auditLogService.LogActionAsync(
            request.AdminUserId,
            "TaskReview",
            "UserTaskSubmission",
            request.SubmissionId.ToString(),
            beforeState,
            afterState,
            string.Empty, // IP address - will be added from controller context in production
            cancellationToken);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
