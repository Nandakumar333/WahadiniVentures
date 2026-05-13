using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

public class UserStreakRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserStreakRepository _repository;
    private readonly UserRepository _userRepository;

    public UserStreakRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new UserStreakRepository(_context);
        _userRepository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnStreak()
    {
        // Arrange
        var user = await CreateUserAsync("user@example.com");
        var streak = UserStreak.Create(user.Id);
        await _repository.AddAsync(streak);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetActiveStreaksAsync_ShouldReturnOnlyActiveStreaks()
    {
        // Arrange
        var user1 = await CreateUserAsync("user1@example.com");
        var user2 = await CreateUserAsync("user2@example.com");

        var s1 = UserStreak.Create(user1.Id);
        s1.CurrentStreak = 5;

        var s2 = UserStreak.Create(user2.Id);
        s2.CurrentStreak = 0;

        await _repository.AddAsync(s1);
        await _repository.AddAsync(s2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveStreaksAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().UserId.Should().Be(user1.Id);
    }

    [Fact]
    public async Task GetTopLongestStreaksAsync_ShouldReturnTopStreaks()
    {
        // Arrange
        var user1 = await CreateUserAsync("user1@example.com");
        var user2 = await CreateUserAsync("user2@example.com");
        var user3 = await CreateUserAsync("user3@example.com");

        var s1 = UserStreak.Create(user1.Id);
        s1.LongestStreak = 10;
        s1.CurrentStreak = 5;

        var s2 = UserStreak.Create(user2.Id);
        s2.LongestStreak = 20;
        s2.CurrentStreak = 10;

        var s3 = UserStreak.Create(user3.Id);
        s3.LongestStreak = 5;
        s3.CurrentStreak = 2;

        await _repository.AddAsync(s1);
        await _repository.AddAsync(s2);
        await _repository.AddAsync(s3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetTopLongestStreaksAsync(2);

        // Assert
        result.Should().HaveCount(2);
        result.First().UserId.Should().Be(user2.Id); // 20
        result.Last().UserId.Should().Be(user1.Id); // 10
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
