using System.Diagnostics;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace WahadiniCryptoQuest.Performance.Tests;

public class RepositoryPerformanceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly RewardTransactionRepository _repository;
    private readonly ITestOutputHelper _output;

    public RepositoryPerformanceTests(ITestOutputHelper output)
    {
        _output = output;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new RewardTransactionRepository(_context);
    }

    [Fact]
    public async Task Insert_BulkTransactions_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var transactionCount = 10000;
        var transactions = new List<RewardTransaction>();

        for (int i = 0; i < transactionCount; i++)
        {
            transactions.Add(RewardTransaction.Create(
                userId,
                10,
                TransactionType.LessonCompletion,
                $"Transaction {i}",
                10 * (i + 1)));
        }

        // Act
        var stopwatch = Stopwatch.StartNew();

        // In EF Core, AddRange is much faster than Add in a loop
        await _context.RewardTransactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();

        stopwatch.Stop();

        // Assert
        _output.WriteLine($"Inserted {transactionCount} transactions in {stopwatch.ElapsedMilliseconds}ms");

        // Expecting < 2 seconds for 10k records in-memory
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000);

        var count = await _context.RewardTransactions.CountAsync();
        count.Should().Be(transactionCount);
    }

    [Fact]
    public async Task Query_UserHistory_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var transactionCount = 5000;
        var transactions = new List<RewardTransaction>();

        for (int i = 0; i < transactionCount; i++)
        {
            transactions.Add(RewardTransaction.Create(
                userId,
                10,
                TransactionType.LessonCompletion,
                $"Transaction {i}",
                10 * (i + 1)));
        }

        await _context.RewardTransactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();

        // Act
        var stopwatch = Stopwatch.StartNew();

        var result = await _repository.GetUserTransactionHistoryAsync(userId, 1, 50);

        stopwatch.Stop();

        // Assert
        _output.WriteLine($"Queried user history from {transactionCount} records in {stopwatch.ElapsedMilliseconds}ms");

        // Expecting < 100ms for query
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
        result.Items.Should().HaveCount(50);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
