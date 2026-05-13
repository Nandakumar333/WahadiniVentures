using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WahadiniCryptoQuest.Core.DTOs.Progress;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Services;
using Xunit;

namespace WahadiniCryptoQuest.Service.Tests.Services;

public class ProgressServiceTests
{
    private readonly Mock<IUserProgressRepository> _userProgressRepositoryMock;
    private readonly Mock<ILessonRepository> _lessonRepositoryMock;
    private readonly Mock<ILessonCompletionRepository> _lessonCompletionRepositoryMock;
    private readonly Mock<IUserCourseEnrollmentRepository> _enrollmentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<ProgressService>> _loggerMock;
    private readonly ProgressService _progressService;

    public ProgressServiceTests()
    {
        _userProgressRepositoryMock = new Mock<IUserProgressRepository>();
        _lessonRepositoryMock = new Mock<ILessonRepository>();
        _lessonCompletionRepositoryMock = new Mock<ILessonCompletionRepository>();
        _enrollmentRepositoryMock = new Mock<IUserCourseEnrollmentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ProgressService>>();

        _progressService = new ProgressService(
            _userProgressRepositoryMock.Object,
            _lessonRepositoryMock.Object,
            _lessonCompletionRepositoryMock.Object,
            _enrollmentRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object
        );
    }

    #region GetProgressAsync Tests

    [Fact]
    public async Task GetProgressAsync_ReturnsNull_WhenNoProgressExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();

        _userProgressRepositoryMock
            .Setup(x => x.GetByUserAndLessonAsync(userId, lessonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProgress?)null);

        // Act
        var result = await _progressService.GetProgressAsync(userId, lessonId);

        // Assert
        result.Should().BeNull();
        _userProgressRepositoryMock.Verify(
            x => x.GetByUserAndLessonAsync(userId, lessonId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetProgressAsync_ReturnsProgressDto_WhenProgressExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var progress = UserProgress.Create(userId, lessonId);

        _userProgressRepositoryMock
            .Setup(x => x.GetByUserAndLessonAsync(userId, lessonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);

        // Act
        var result = await _progressService.GetProgressAsync(userId, lessonId);

        // Assert
        result.Should().NotBeNull();
        result!.LessonId.Should().Be(lessonId);
        result.LastWatchedPosition.Should().Be(0);
        result.TotalWatchTime.Should().Be(0);
    }

    #endregion

    #region UpdateProgressAsync Tests

    [Fact]
    public async Task UpdateProgressAsync_CreatesNewProgress_WhenNoneExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var lesson = CreateTestLesson(lessonId, 600);
        var dto = new UpdateProgressDto { WatchPosition = 60 };

        _lessonRepositoryMock
            .Setup(x => x.GetByIdAsync(lessonId))
            .ReturnsAsync(lesson);

        _userProgressRepositoryMock
            .Setup(x => x.GetByUserAndLessonAsync(userId, lessonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProgress?)null);

        _userProgressRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<UserProgress>()))
            .ReturnsAsync((UserProgress p) => p);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _progressService.UpdateProgressAsync(userId, lessonId, dto);

        // Assert
        result.Should().NotBeNull();
        result.IsNewlyCompleted.Should().BeFalse();
        _userProgressRepositoryMock.Verify(x => x.AddAsync(It.IsAny<UserProgress>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProgressAsync_UpdatesExistingProgress_WithHigherPosition()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var lesson = CreateTestLesson(lessonId, 600);
        var existingProgress = UserProgress.Create(userId, lessonId);
        existingProgress.UpdatePosition(100, 600); // Already at position 100
        var dto = new UpdateProgressDto { WatchPosition = 200 }; // Moving forward to 200

        _lessonRepositoryMock
            .Setup(x => x.GetByIdAsync(lessonId))
            .ReturnsAsync(lesson);

        _userProgressRepositoryMock
            .Setup(x => x.GetByUserAndLessonAsync(userId, lessonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProgress);

        _userProgressRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<UserProgress>()))
            .ReturnsAsync((UserProgress p) => p);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _progressService.UpdateProgressAsync(userId, lessonId, dto);

        // Assert
        result.Should().NotBeNull();
        existingProgress.LastWatchedPosition.Should().Be(200);
        _userProgressRepositoryMock.Verify(x => x.UpdateAsync(existingProgress), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProgressAsync_DoesNotUpdate_WhenWatchPositionIsLower()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var lesson = CreateTestLesson(lessonId, 600);
        var existingProgress = UserProgress.Create(userId, lessonId);
        existingProgress.UpdatePosition(200, 600); // Already at position 200
        var dto = new UpdateProgressDto { WatchPosition = 100 }; // Trying to go back to 100

        _lessonRepositoryMock
            .Setup(x => x.GetByIdAsync(lessonId))
            .ReturnsAsync(lesson);

        _userProgressRepositoryMock
            .Setup(x => x.GetByUserAndLessonAsync(userId, lessonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProgress);

        _userProgressRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<UserProgress>()))
            .ReturnsAsync((UserProgress p) => p);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _progressService.UpdateProgressAsync(userId, lessonId, dto);

        // Assert
        existingProgress.LastWatchedPosition.Should().Be(200); // Should remain unchanged
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProgressAsync_IncrementsTotalWatchTime_By10Seconds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var lesson = CreateTestLesson(lessonId, 600);
        var existingProgress = UserProgress.Create(userId, lessonId);
        var initialWatchTime = existingProgress.TotalWatchTime;
        var dto = new UpdateProgressDto { WatchPosition = 50 };

        _lessonRepositoryMock
            .Setup(x => x.GetByIdAsync(lessonId))
            .ReturnsAsync(lesson);

        _userProgressRepositoryMock
            .Setup(x => x.GetByUserAndLessonAsync(userId, lessonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProgress);

        _userProgressRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<UserProgress>()))
            .ReturnsAsync((UserProgress p) => p);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _progressService.UpdateProgressAsync(userId, lessonId, dto);

        // Assert
        existingProgress.TotalWatchTime.Should().Be(initialWatchTime + 5);
    }

    [Fact]
    public async Task UpdateProgressAsync_ThrowsInvalidOperationException_WhenLessonNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var dto = new UpdateProgressDto { WatchPosition = 60 };

        _lessonRepositoryMock
            .Setup(x => x.GetByIdAsync(lessonId))
            .ReturnsAsync((Core.Entities.Lesson?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _progressService.UpdateProgressAsync(userId, lessonId, dto));
    }

    [Fact]
    public async Task UpdateProgressAsync_ClampsPosition_WhenExceedsVideoDuration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var lesson = CreateTestLesson(lessonId, 600); // 600 seconds duration
        var existingProgress = UserProgress.Create(userId, lessonId);
        var dto = new UpdateProgressDto { WatchPosition = 700 }; // Exceeds duration (should clamp to 600)

        _lessonRepositoryMock
            .Setup(x => x.GetByIdAsync(lessonId))
            .ReturnsAsync(lesson);

        _userProgressRepositoryMock
            .Setup(x => x.GetByUserAndLessonAsync(userId, lessonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProgress);

        _userProgressRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<UserProgress>()))
            .ReturnsAsync((UserProgress p) => p);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _progressService.UpdateProgressAsync(userId, lessonId, dto);

        // Assert
        existingProgress.LastWatchedPosition.Should().Be(600); // Clamped to max duration
        existingProgress.CompletionPercentage.Should().Be(100m);
    }

    [Fact]
    public async Task UpdateProgressAsync_MarksAsComplete_When80PercentReached()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var lesson = CreateTestLesson(lessonId, 500);
        var existingProgress = UserProgress.Create(userId, lessonId);
        var dto = new UpdateProgressDto { WatchPosition = 480 }; // 96% completion (above 80%)

        _lessonRepositoryMock
            .Setup(x => x.GetByIdAsync(lessonId))
            .ReturnsAsync(lesson);

        _userProgressRepositoryMock
            .Setup(x => x.GetByUserAndLessonAsync(userId, lessonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProgress);

        _userProgressRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<UserProgress>()))
            .ReturnsAsync((UserProgress p) => p);

        _lessonCompletionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<LessonCompletion>()))
            .ReturnsAsync((LessonCompletion lc) => lc);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _progressService.UpdateProgressAsync(userId, lessonId, dto);

        // Assert
        result.IsNewlyCompleted.Should().BeTrue();
        result.PointsAwarded.Should().Be(lesson.RewardPoints);
        existingProgress.IsCompleted.Should().BeTrue();
        existingProgress.CompletedAt.Should().NotBeNull();
        existingProgress.RewardPointsClaimed.Should().BeTrue();

        _lessonCompletionRepositoryMock.Verify(
            x => x.AddAsync(It.Is<LessonCompletion>(lc =>
                lc.UserId == userId &&
                lc.LessonId == lessonId &&
                lc.PointsAwarded == lesson.RewardPoints)),
            Times.Once);
    }

    [Fact]
    public async Task UpdateProgressAsync_DoesNotAwardPointsTwice_WhenAlreadyClaimed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var lesson = CreateTestLesson(lessonId, 500);
        var existingProgress = UserProgress.Create(userId, lessonId);
        existingProgress.UpdatePosition(400, 500); // 80% complete
        existingProgress.MarkComplete(DateTime.UtcNow);
        existingProgress.ClaimRewardPoints(); // Already claimed

        var dto = new UpdateProgressDto { WatchPosition = 450 }; // Further progress

        _lessonRepositoryMock
            .Setup(x => x.GetByIdAsync(lessonId))
            .ReturnsAsync(lesson);

        _userProgressRepositoryMock
            .Setup(x => x.GetByUserAndLessonAsync(userId, lessonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProgress);

        _userProgressRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<UserProgress>()))
            .ReturnsAsync((UserProgress p) => p);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _progressService.UpdateProgressAsync(userId, lessonId, dto);

        // Assert
        result.IsNewlyCompleted.Should().BeFalse();
        result.PointsAwarded.Should().Be(0); // No points awarded second time

        _lessonCompletionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<LessonCompletion>()),
            Times.Never); // Should not create duplicate completion record
    }

    #endregion

    #region Helper Methods

    private static Core.Entities.Lesson CreateTestLesson(Guid lessonId, int durationSeconds)
    {
        return new Core.Entities.Lesson
        {
            Id = lessonId,
            CourseId = Guid.NewGuid(),
            Title = "Test Lesson",
            Description = "Test Description",
            YouTubeVideoId = "testVideoId",
            Duration = durationSeconds / 60,
            VideoDuration = durationSeconds,
            RewardPoints = 50,
            OrderIndex = 1,
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
