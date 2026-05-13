using FluentAssertions;
using Moq;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Services;
using Xunit;

namespace WahadiniCryptoQuest.Service.Tests.Services;

public class BulkActionTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserTaskSubmissionRepository> _submissionRepoMock;
    private readonly Mock<IRewardTransactionRepository> _rewardRepoMock;
    private readonly TaskSubmissionService _service;

    public BulkActionTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _submissionRepoMock = new Mock<IUserTaskSubmissionRepository>();
        _rewardRepoMock = new Mock<IRewardTransactionRepository>();
        var userRepoMock = new Mock<IUserRepository>();

        _unitOfWorkMock.Setup(x => x.TaskSubmissions).Returns(_submissionRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.RewardTransactions).Returns(_rewardRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.Users).Returns(userRepoMock.Object);

        // Setup default user mock
        userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => User.Create("test@example.com", "hash", "Test", "User"));

        _service = new TaskSubmissionService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task BulkReviewAsync_ShouldUpdateAllValidSubmissions()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var submissions = new List<UserTaskSubmission>
        {
            new UserTaskSubmission { Id = ids[0], Status = SubmissionStatus.Pending },
            new UserTaskSubmission { Id = ids[1], Status = SubmissionStatus.Pending }
        };

        // Mock finding submissions one by one or bulk if repository supports it
        // Assuming service calls GetByIdAsync in loop or FindAsync
        _submissionRepoMock.Setup(x => x.GetByIdAsync(ids[0])).ReturnsAsync(submissions[0]);
        _submissionRepoMock.Setup(x => x.GetByIdAsync(ids[1])).ReturnsAsync(submissions[1]);

        // Act
        var result = await _service.BulkReviewAsync(ids, reviewerId, true, "Bulk Approved", 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.SuccessCount.Should().Be(2);
        submissions.All(s => s.Status == SubmissionStatus.Approved).Should().BeTrue();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
