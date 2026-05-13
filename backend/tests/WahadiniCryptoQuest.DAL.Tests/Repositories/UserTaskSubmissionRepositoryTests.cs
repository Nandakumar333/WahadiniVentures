using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

public class UserTaskSubmissionRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserTaskSubmissionRepository _repository;

    public UserTaskSubmissionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new UserTaskSubmissionRepository(_context);
    }

    [Fact]
    public async Task GetUserTaskSubmissionAsync_ShouldReturnCorrectSubmission()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        // Add required LearningTask first
        var task = new LearningTask
        {
            Id = taskId,
            LessonId = Guid.NewGuid(),
            Title = "Test Task",
            TaskType = TaskType.TextSubmission
        };
        _context.Set<LearningTask>().Add(task);

        var submission = new UserTaskSubmission
        {
            UserId = userId,
            TaskId = taskId,
            Status = SubmissionStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        _context.Set<UserTaskSubmission>().Add(submission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserTaskSubmissionAsync(userId, taskId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(submission.Id);
    }
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
