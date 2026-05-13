using FluentAssertions;
using Moq;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.DTOs.Task;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Services;
using Xunit;

namespace WahadiniCryptoQuest.Service.Tests.Services;

public class ResubmissionTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserTaskSubmissionRepository> _submissionRepoMock;
    private readonly Mock<ILearningTaskRepository> _taskRepoMock;
    private readonly TaskSubmissionService _service;

    public ResubmissionTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _submissionRepoMock = new Mock<IUserTaskSubmissionRepository>();
        _taskRepoMock = new Mock<ILearningTaskRepository>();
        var userRepoMock = new Mock<IUserRepository>();

        _unitOfWorkMock.Setup(x => x.TaskSubmissions).Returns(_submissionRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.LearningTasks).Returns(_taskRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.Users).Returns(userRepoMock.Object);

        // Setup default user mock
        userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => User.Create("test@example.com", "hash", "Test", "User"));

        _service = new TaskSubmissionService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task SubmitTask_PendingSubmissionExists_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var request = new TaskSubmissionRequest { TaskId = taskId, TaskType = TaskType.TextSubmission, SubmissionData = "test" };

        _taskRepoMock.Setup(x => x.GetByIdAsync(taskId)).ReturnsAsync(new LearningTask { Id = taskId, TaskType = TaskType.TextSubmission });

        _submissionRepoMock.Setup(x => x.GetUserTaskSubmissionAsync(userId, taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserTaskSubmission { Status = SubmissionStatus.Pending });

        // Act
        var result = await _service.SubmitTaskAsync(request, userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("DUPLICATE_SUBMISSION");
    }

    [Fact]
    public async Task SubmitTask_ApprovedSubmissionExists_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var request = new TaskSubmissionRequest { TaskId = taskId, TaskType = TaskType.TextSubmission, SubmissionData = "test" };

        _taskRepoMock.Setup(x => x.GetByIdAsync(taskId)).ReturnsAsync(new LearningTask { Id = taskId, TaskType = TaskType.TextSubmission });

        _submissionRepoMock.Setup(x => x.GetUserTaskSubmissionAsync(userId, taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserTaskSubmission { Status = SubmissionStatus.Approved });

        // Act
        var result = await _service.SubmitTaskAsync(request, userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("DUPLICATE_SUBMISSION");
    }

    [Fact]
    public async Task SubmitTask_RejectedSubmissionExists_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var request = new TaskSubmissionRequest { TaskId = taskId, TaskType = TaskType.TextSubmission, SubmissionData = "test" };

        _taskRepoMock.Setup(x => x.GetByIdAsync(taskId)).ReturnsAsync(new LearningTask { Id = taskId, TaskType = TaskType.TextSubmission });

        _submissionRepoMock.Setup(x => x.GetUserTaskSubmissionAsync(userId, taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserTaskSubmission { Status = SubmissionStatus.Rejected });

        _unitOfWorkMock.Setup(x => x.TaskSubmissions.AddAsync(It.IsAny<UserTaskSubmission>()));

        // Act
        var result = await _service.SubmitTaskAsync(request, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
