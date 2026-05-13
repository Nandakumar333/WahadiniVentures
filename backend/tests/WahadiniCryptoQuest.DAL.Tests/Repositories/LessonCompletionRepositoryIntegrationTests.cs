using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

public class LessonCompletionRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly LessonCompletionRepository _repository;
    private readonly UserRepository _userRepository;

    public LessonCompletionRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new LessonCompletionRepository(_context);
        _userRepository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetByUserAndLessonAsync_ShouldReturnCompletion()
    {
        // Arrange
        var user = await CreateUserAsync("user@example.com");
        var lessonId = Guid.NewGuid();
        
        var completion = LessonCompletion.Create(user.Id, lessonId, 100, 100);
        await _repository.AddAsync(completion);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserAndLessonAsync(user.Id, lessonId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(user.Id);
        result.LessonId.Should().Be(lessonId);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrueIfCompletionExists()
    {
        // Arrange
        var user = await CreateUserAsync("user@example.com");
        var lessonId = Guid.NewGuid();
        
        var completion = LessonCompletion.Create(user.Id, lessonId, 100, 100);
        await _repository.AddAsync(completion);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync(user.Id, lessonId);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalseIfCompletionDoesNotExist()
    {
        // Arrange
        var user = await CreateUserAsync("user@example.com");
        var lessonId = Guid.NewGuid();

        // Act
        var exists = await _repository.ExistsAsync(user.Id, lessonId);

        // Assert
        exists.Should().BeFalse();
    }

    private async Task<User> CreateUserAsync(string email)
    {
        var user = User.Create(email, "password", "First", "Last");
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
