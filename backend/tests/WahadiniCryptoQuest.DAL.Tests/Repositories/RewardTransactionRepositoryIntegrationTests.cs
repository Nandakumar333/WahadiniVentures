using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

public class RewardTransactionRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly RewardTransactionRepository _repository;
    private readonly UserRepository _userRepository;

    public RewardTransactionRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new RewardTransactionRepository(_context);
        _userRepository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetUserTransactionHistoryAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var user = await CreateUserAsync("user@example.com");

        for (int i = 0; i < 15; i++)
        {
            var transaction = RewardTransaction.Create(
                user.Id,
                10,
                TransactionType.LessonCompletion,
                $"Transaction {i}",
                10 * (i + 1));
            await _repository.AddAsync(transaction);
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserTransactionHistoryAsync(user.Id, 1, 10);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetTransactionsByDateRangeAsync_ShouldReturnTransactionsWithinRange()
    {
        // Arrange
        var user = await CreateUserAsync("user@example.com");
        var now = DateTime.UtcNow;

        var t1 = RewardTransaction.Create(user.Id, 10, TransactionType.LessonCompletion, "T1", 10);
        // Use reflection to set CreatedAt since it's protected/private set in BaseEntity usually
        SetCreatedAt(t1, now.AddDays(-5));

        var t2 = RewardTransaction.Create(user.Id, 10, TransactionType.LessonCompletion, "T2", 20);
        SetCreatedAt(t2, now.AddDays(-2));

        var t3 = RewardTransaction.Create(user.Id, 10, TransactionType.LessonCompletion, "T3", 30);
        SetCreatedAt(t3, now.AddDays(1));

        await _repository.AddAsync(t1);
        await _repository.AddAsync(t2);
        await _repository.AddAsync(t3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetTransactionsByDateRangeAsync(now.AddDays(-3), now);

        // Assert
        result.Should().HaveCount(1);
        result.First().Description.Should().Be("T2");
    }

    [Fact]
    public async Task GetByReferenceAsync_ShouldReturnCorrectTransaction()
    {
        // Arrange
        var user = await CreateUserAsync("user@example.com");
        var refId = Guid.NewGuid().ToString();
        var refType = "TaskSubmission";

        var transaction = RewardTransaction.Create(
            user.Id,
            100,
            TransactionType.TaskApproval,
            "Task Reward",
            100,
            refId,
            refType);

        await _repository.AddAsync(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByReferenceAsync(user.Id, refId, refType);

        // Assert
        result.Should().NotBeNull();
        result!.ReferenceId.Should().Be(refId);
        result.ReferenceType.Should().Be(refType);
    }

    private async Task<User> CreateUserAsync(string email)
    {
        var user = User.Create(email, "password", "First", "Last");
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    private void SetCreatedAt(RewardTransaction transaction, DateTime createdAt)
    {
        var prop = typeof(BaseEntity).GetProperty("CreatedAt");
        prop?.SetValue(transaction, createdAt);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
