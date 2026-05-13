using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

public class LearningTaskRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly LearningTaskRepository _repository;

    public LearningTaskRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new LearningTaskRepository(_context);
    }

    [Fact]
    public async Task GetByLessonIdOrderedAsync_ShouldReturnOrderedTasks()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var task1 = new LearningTask { Id = Guid.NewGuid(), LessonId = lessonId, OrderIndex = 2, Title = "Task 2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var task2 = new LearningTask { Id = Guid.NewGuid(), LessonId = lessonId, OrderIndex = 1, Title = "Task 1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

        _context.Set<LearningTask>().AddRange(task1, task2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByLessonIdOrderedAsync(lessonId);

        // Assert
        result.Should().HaveCount(2);
        result.First().Title.Should().Be("Task 1");
        result.Last().Title.Should().Be("Task 2");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
