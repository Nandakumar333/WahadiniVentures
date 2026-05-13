using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

public class DiscountCodeRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly DiscountCodeRepository _repository;

    public DiscountCodeRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new DiscountCodeRepository(_context);
    }

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnDiscountCode()
    {
        // Arrange
        var code = new DiscountCode
        {
            Code = "TEST10",
            DiscountPercentage = 10,
            RequiredPoints = 100,
            IsActive = true
        };
        await _repository.AddAsync(code);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCodeAsync("TEST10");

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be("TEST10");
        result.DiscountPercentage.Should().Be(10);
    }

    [Fact]
    public async Task GetActiveCodesAsync_ShouldReturnOnlyActiveAndUnexpiredCodes()
    {
        // Arrange
        var activeCode = new DiscountCode
        {
            Code = "ACTIVE",
            DiscountPercentage = 10,
            RequiredPoints = 100,
            IsActive = true,
            ExpiryDate = DateTime.UtcNow.AddDays(1)
        };

        var inactiveCode = new DiscountCode
        {
            Code = "INACTIVE",
            DiscountPercentage = 10,
            RequiredPoints = 100,
            IsActive = false
        };

        var expiredCode = new DiscountCode
        {
            Code = "EXPIRED",
            DiscountPercentage = 10,
            RequiredPoints = 100,
            IsActive = true,
            ExpiryDate = DateTime.UtcNow.AddDays(-1)
        };

        await _repository.AddAsync(activeCode);
        await _repository.AddAsync(inactiveCode);
        await _repository.AddAsync(expiredCode);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveCodesAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Code.Should().Be("ACTIVE");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
