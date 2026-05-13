using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

public class UserAchievementRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserAchievementRepository _repository;
    private readonly UserRepository _userRepository;

    public UserAchievementRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new UserAchievementRepository(_context);
        _userRepository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetUserAchievementsAsync_ShouldReturnAllUserAchievements()
    {
        // Arrange
        var user = await CreateUserAsync("user@example.com");
        
        var a1 = UserAchievement.Create(user.Id, "ACH_1");
        var a2 = UserAchievement.Create(user.Id, "ACH_2");
        
        await _repository.AddAsync(a1);
        await _repository.AddAsync(a2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserAchievementsAsync(user.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(a => a.AchievementId == "ACH_1");
        result.Should().Contain(a => a.AchievementId == "ACH_2");
    }

    [Fact]
    public async Task HasAchievementAsync_ShouldReturnTrueIfUnlocked()
    {
        // Arrange
        var user = await CreateUserAsync("user@example.com");
        var achievementId = "ACH_TEST";
        
        var achievement = UserAchievement.Create(user.Id, achievementId);
        await _repository.AddAsync(achievement);
        await _context.SaveChangesAsync();

        // Act
        var hasAchievement = await _repository.HasAchievementAsync(user.Id, achievementId);

        // Assert
        hasAchievement.Should().BeTrue();
    }

    [Fact]
    public async Task HasAchievementAsync_ShouldReturnFalseIfNotUnlocked()
    {
        // Arrange
        var user = await CreateUserAsync("user@example.com");
        
        // Act
        var hasAchievement = await _repository.HasAchievementAsync(user.Id, "NON_EXISTENT");

        // Assert
        hasAchievement.Should().BeFalse();
    }

    [Fact]
    public async Task GetUnnotifiedAchievementsAsync_ShouldReturnOnlyUnnotified()
    {
        // Arrange
        var user = await CreateUserAsync("user@example.com");
        
        var a1 = UserAchievement.Create(user.Id, "ACH_1"); // Notified = false by default
        
        var a2 = UserAchievement.Create(user.Id, "ACH_2");
        a2.MarkAsNotified();
        
        await _repository.AddAsync(a1);
        await _repository.AddAsync(a2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUnnotifiedAchievementsAsync(user.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().AchievementId.Should().Be("ACH_1");
    }

    [Fact]
    public async Task MarkAsNotifiedAsync_ShouldUpdateNotifiedStatus()
    {
        // Arrange
        var user = await CreateUserAsync("user@example.com");
        var achievementId = "ACH_TEST";
        
        var achievement = UserAchievement.Create(user.Id, achievementId);
        await _repository.AddAsync(achievement);
        await _context.SaveChangesAsync();

        // Act
        await _repository.MarkAsNotifiedAsync(user.Id, new[] { achievementId });
        await _context.SaveChangesAsync();

        // Assert
        var updated = await _repository.GetByIdAsync(achievement.Id);
        updated.Should().NotBeNull();
        updated!.Notified.Should().BeTrue();
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
