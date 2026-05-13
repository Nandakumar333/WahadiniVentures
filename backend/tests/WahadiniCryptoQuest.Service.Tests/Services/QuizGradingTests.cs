using FluentAssertions;
using Moq;
using System.Text.Json;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.DTOs.Task;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Services;
using Xunit;

namespace WahadiniCryptoQuest.Service.Tests.Services;

public class QuizGradingTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserTaskSubmissionRepository> _submissionRepositoryMock;
    private readonly Mock<ILearningTaskRepository> _taskRepositoryMock;
    private readonly Mock<IRewardTransactionRepository> _rewardRepoMock;
    private readonly TaskSubmissionService _service;

    public QuizGradingTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _submissionRepositoryMock = new Mock<IUserTaskSubmissionRepository>();
        _taskRepositoryMock = new Mock<ILearningTaskRepository>();
        _rewardRepoMock = new Mock<IRewardTransactionRepository>();
        var userRepoMock = new Mock<IUserRepository>();

        _unitOfWorkMock.Setup(x => x.TaskSubmissions).Returns(_submissionRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.LearningTasks).Returns(_taskRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Users).Returns(userRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.RewardTransactions).Returns(_rewardRepoMock.Object);

        // Setup default user mock
        userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => User.Create("test@example.com", "hash", "Test", "User"));

        _service = new TaskSubmissionService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task SubmitQuiz_WithPassingScore_ShouldApproveAndAwardPoints()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var quizData = new QuizTaskDataDto
        {
            PassingScore = 50,
            Questions = new List<QuizQuestionDto>
            {
                new QuizQuestionDto { Question = "Q1", Options = new List<string>{"A", "B"}, CorrectOption = 0 }, // A
                new QuizQuestionDto { Question = "Q2", Options = new List<string>{"A", "B"}, CorrectOption = 1 }  // B
            }
        };

        var task = new LearningTask
        {
            Id = taskId,
            TaskType = TaskType.Quiz,
            RewardPoints = 100,
            TaskData = JsonSerializer.Serialize(quizData)
        };

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        _submissionRepositoryMock.Setup(x => x.GetUserTaskSubmissionAsync(userId, taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserTaskSubmission?)null);

        // User answers both correctly (100% score)
        var submissionDto = new QuizSubmissionDto
        {
            Answers = new Dictionary<int, int> { { 0, 0 }, { 1, 1 } }
        };
        var request = new TaskSubmissionRequest
        {
            TaskId = taskId,
            TaskType = TaskType.Quiz,
            SubmissionData = JsonSerializer.Serialize(submissionDto)
        };

        _submissionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<UserTaskSubmission>()))
            .ReturnsAsync((UserTaskSubmission s) => s);

        // Act
        var result = await _service.SubmitTaskAsync(request, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Status.Should().Be(SubmissionStatus.Approved);
        result.Data.PointsAwarded.Should().Be(100);

        _submissionRepositoryMock.Verify(x => x.AddAsync(It.Is<UserTaskSubmission>(s =>
            s.Status == SubmissionStatus.Approved &&
            s.RewardPointsAwarded == 100)), Times.Once);
    }

    [Fact]
    public async Task SubmitQuiz_WithFailingScore_ShouldReject()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var quizData = new QuizTaskDataDto
        {
            PassingScore = 100, // Requires 100%
            Questions = new List<QuizQuestionDto>
            {
                new QuizQuestionDto { Question = "Q1", Options = new List<string>{"A", "B"}, CorrectOption = 0 }
            }
        };

        var task = new LearningTask
        {
            Id = taskId,
            TaskType = TaskType.Quiz,
            RewardPoints = 100,
            TaskData = JsonSerializer.Serialize(quizData)
        };

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        // User answers incorrectly
        var submissionDto = new QuizSubmissionDto
        {
            Answers = new Dictionary<int, int> { { 0, 1 } } // Wrong answer
        };
        var request = new TaskSubmissionRequest
        {
            TaskId = taskId,
            TaskType = TaskType.Quiz,
            SubmissionData = JsonSerializer.Serialize(submissionDto)
        };

        _submissionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<UserTaskSubmission>()))
            .ReturnsAsync((UserTaskSubmission s) => s);

        // Act
        var result = await _service.SubmitTaskAsync(request, userId);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Request succeeded
        result.Data.Status.Should().Be(SubmissionStatus.Rejected); // But submission rejected
        result.Data.PointsAwarded.Should().Be(0);
    }
}
