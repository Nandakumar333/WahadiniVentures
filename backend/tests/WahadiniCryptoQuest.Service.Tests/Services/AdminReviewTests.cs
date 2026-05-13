using FluentAssertions;
using Moq;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Services;
using Xunit;

namespace WahadiniCryptoQuest.Service.Tests.Services;

public class AdminReviewTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserTaskSubmissionRepository> _submissionRepoMock;
    private readonly Mock<IRewardTransactionRepository> _rewardRepoMock;
    private readonly TaskSubmissionService _service;

    public AdminReviewTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _submissionRepoMock = new Mock<IUserTaskSubmissionRepository>();
        _rewardRepoMock = new Mock<IRewardTransactionRepository>();
        var userRepoMock = new Mock<IUserRepository>();
        var taskRepoMock = new Mock<ILearningTaskRepository>();

        _unitOfWorkMock.Setup(x => x.TaskSubmissions).Returns(_submissionRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.RewardTransactions).Returns(_rewardRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.Users).Returns(userRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.LearningTasks).Returns(taskRepoMock.Object);

        // Setup default user mock
        userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => User.Create("test@example.com", "hash", "Test", "User"));

        // Setup default task mock
        taskRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => new LearningTask { Id = id, Title = "Test Task" });

        _service = new TaskSubmissionService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ReviewSubmissionAsync_Approve_ShouldUpdateStatusAndAwardPoints()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var version = new byte[] { 1, 2, 3 };

        var submission = new UserTaskSubmission
        {
            Id = submissionId,
            UserId = userId,
            Status = SubmissionStatus.Pending,
            Version = version
        };

        _submissionRepoMock.Setup(x => x.GetByIdAsync(submissionId)).ReturnsAsync(submission);

        // Act
        var result = await _service.ReviewSubmissionAsync(submissionId, reviewerId, true, "Good job", 50, version);

        // Assert
        result.IsSuccess.Should().BeTrue();
        submission.Status.Should().Be(SubmissionStatus.Approved);
        submission.RewardPointsAwarded.Should().Be(50);
        submission.FeedbackText.Should().Be("Good job");
        submission.ReviewedByUserId.Should().Be(reviewerId);

        // Verify points transaction added
        _rewardRepoMock.Verify(x => x.AddAsync(It.Is<RewardTransaction>(t =>
            t.UserId == userId &&
            t.Amount == 50 &&
            t.TransactionType == TransactionType.TaskApproval)), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReviewSubmissionAsync_Reject_ShouldUpdateStatusOnly()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var version = new byte[] { 1, 2, 3 };

        var submission = new UserTaskSubmission
        {
            Id = submissionId,
            Status = SubmissionStatus.Pending,
            Version = version
        };

        _submissionRepoMock.Setup(x => x.GetByIdAsync(submissionId)).ReturnsAsync(submission);

        // Act
        var result = await _service.ReviewSubmissionAsync(submissionId, reviewerId, false, "Missing info", 0, version);

        // Assert
        result.IsSuccess.Should().BeTrue();
        submission.Status.Should().Be(SubmissionStatus.Rejected);
        submission.RewardPointsAwarded.Should().Be(0);

        // Verify NO points transaction
        _rewardRepoMock.Verify(x => x.AddAsync(It.IsAny<RewardTransaction>()), Times.Never);
    }

    [Fact]
    public async Task ReviewSubmissionAsync_ConcurrencyConflict_ShouldFail()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var version = new byte[] { 1, 2, 3 };
        var differentVersion = new byte[] { 4, 5, 6 };

        var submission = new UserTaskSubmission
        {
            Id = submissionId,
            Status = SubmissionStatus.Pending,
            Version = differentVersion // DB has newer version
        };

        _submissionRepoMock.Setup(x => x.GetByIdAsync(submissionId)).ReturnsAsync(submission);

        // Act
        var result = await _service.ReviewSubmissionAsync(submissionId, reviewerId, true, "Ok", 10, version);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("CONCURRENCY_CONFLICT");
    }

    [Fact]
    public async Task GetSubmissionStatusAsync_WithExistingSubmission_ReturnsStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        var submittedAt = DateTime.UtcNow.AddHours(-2);
        var reviewedAt = DateTime.UtcNow.AddHours(-1);

        var submission = new UserTaskSubmission
        {
            Id = submissionId,
            UserId = userId,
            TaskId = taskId,
            Status = SubmissionStatus.Approved,
            SubmittedAt = submittedAt,
            ReviewedAt = reviewedAt,
            FeedbackText = "Great work!",
            RewardPointsAwarded = 50
        };

        _submissionRepoMock
            .Setup(x => x.GetUserTaskSubmissionAsync(userId, taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);

        // Act
        var result = await _service.GetSubmissionStatusAsync(taskId, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.HasSubmitted.Should().BeTrue();
        result.Data.SubmissionId.Should().Be(submissionId);
        result.Data.Status.Should().Be(SubmissionStatus.Approved);
        result.Data.SubmittedAt.Should().Be(submittedAt);
        result.Data.ReviewedAt.Should().Be(reviewedAt);
        result.Data.FeedbackText.Should().Be("Great work!");
        result.Data.RewardPointsAwarded.Should().Be(50);
    }

    [Fact]
    public async Task GetSubmissionStatusAsync_WithNoSubmission_ReturnsNotSubmittedStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        _submissionRepoMock
            .Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync((UserTaskSubmission?)null);

        // Act
        var result = await _service.GetSubmissionStatusAsync(taskId, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.HasSubmitted.Should().BeFalse();
        result.Data.SubmissionId.Should().BeNull();
        result.Data.Status.Should().BeNull();
        result.Data.SubmittedAt.Should().BeNull();
        result.Data.ReviewedAt.Should().BeNull();
        result.Data.FeedbackText.Should().BeNull();
        result.Data.RewardPointsAwarded.Should().Be(0);
    }

    [Fact]
    public async Task GetSubmissionStatusAsync_WithPendingSubmission_ReturnsCorrectStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        var submittedAt = DateTime.UtcNow.AddMinutes(-30);

        var submission = new UserTaskSubmission
        {
            Id = submissionId,
            UserId = userId,
            TaskId = taskId,
            Status = SubmissionStatus.Pending,
            SubmittedAt = submittedAt,
            ReviewedAt = null, // Not reviewed yet
            FeedbackText = null
        };

        _submissionRepoMock
            .Setup(x => x.GetUserTaskSubmissionAsync(userId, taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);

        // Act
        var result = await _service.GetSubmissionStatusAsync(taskId, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.HasSubmitted.Should().BeTrue();
        result.Data.SubmissionId.Should().Be(submissionId);
        result.Data.Status.Should().Be(SubmissionStatus.Pending);
        result.Data.SubmittedAt.Should().Be(submittedAt);
        result.Data.ReviewedAt.Should().BeNull();
        result.Data.FeedbackText.Should().BeNull();
    }
}
