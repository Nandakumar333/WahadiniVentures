using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

/// <summary>
/// Integration tests for EmailVerificationTokenRepository
/// Tests token validation, retrieval, expiration, and cleanup operations
/// </summary>
public class EmailVerificationTokenRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly EmailVerificationTokenRepository _tokenRepository;
    private readonly User _testUser;

    public EmailVerificationTokenRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _tokenRepository = new EmailVerificationTokenRepository(_context);

        _testUser = User.Create("test@example.com", "hashedPassword", "John", "Doe");
        _context.Users.Add(_testUser);
        _context.SaveChanges();
    }

    [Fact]
    public async Task AddAsync_WithValidToken_ShouldPersistToDatabase()
    {
        // Arrange
        var token = EmailVerificationToken.Create(_testUser.Id, 24);

        // Act
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Assert
        var savedToken = await _context.EmailVerificationTokens.FindAsync(token.Id);
        savedToken.Should().NotBeNull();
        savedToken!.UserId.Should().Be(_testUser.Id);
        savedToken.IsUsed.Should().BeFalse();
    }

    [Fact]
    public async Task GetValidTokenAsync_WithValidToken_ShouldReturnToken()
    {
        // Arrange
        var token = EmailVerificationToken.Create(_testUser.Id, 24);
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetValidTokenAsync(token.Token);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be(token.Token);
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task GetValidTokenAsync_WithUsedToken_ShouldReturnNull()
    {
        // Arrange
        var token = EmailVerificationToken.Create(_testUser.Id, 24);
        token.MarkAsUsed();
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetValidTokenAsync(token.Token);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetValidTokenAsync_WithExpiredToken_ShouldReturnNull()
    {
        // Arrange
        var token = EmailVerificationToken.CreateExpiredToken(_testUser.Id, Guid.NewGuid().ToString(), 1);
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetValidTokenAsync(token.Token);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetValidTokenAsync_WithDeletedToken_ShouldReturnNull()
    {
        // Arrange
        var token = EmailVerificationToken.Create(_testUser.Id, 24);
        token.SoftDelete();
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetValidTokenAsync(token.Token);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_WithMultipleTokens_ShouldReturnAllNonDeleted()
    {
        // Arrange
        var token1 = EmailVerificationToken.Create(_testUser.Id, 24);
        await Task.Delay(10);
        var token2 = EmailVerificationToken.Create(_testUser.Id, 24);
        await Task.Delay(10);
        var token3 = EmailVerificationToken.Create(_testUser.Id, 24);
        token3.SoftDelete();

        await _tokenRepository.AddAsync(token1);
        await _tokenRepository.AddAsync(token2);
        await _tokenRepository.AddAsync(token3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetByUserIdAsync(_testUser.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(t => t.IsDeleted);
        result[0].CreatedAt.Should().BeAfter(result[1].CreatedAt); // Ordered descending
    }

    [Fact]
    public async Task GetLatestValidTokenForUserAsync_WithMultipleValidTokens_ShouldReturnMostRecent()
    {
        // Arrange
        var token1 = EmailVerificationToken.Create(_testUser.Id, 24);
        await Task.Delay(10);
        var token2 = EmailVerificationToken.Create(_testUser.Id, 24);
        await Task.Delay(10);
        var token3 = EmailVerificationToken.Create(_testUser.Id, 24);

        await _tokenRepository.AddAsync(token1);
        await _tokenRepository.AddAsync(token2);
        await _tokenRepository.AddAsync(token3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetLatestValidTokenForUserAsync(_testUser.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(token3.Id);
    }

    [Fact]
    public async Task GetLatestValidTokenForUserAsync_WithOnlyUsedTokens_ShouldReturnNull()
    {
        // Arrange
        var token1 = EmailVerificationToken.Create(_testUser.Id, 24);
        token1.MarkAsUsed();
        var token2 = EmailVerificationToken.Create(_testUser.Id, 24);
        token2.MarkAsUsed();

        await _tokenRepository.AddAsync(token1);
        await _tokenRepository.AddAsync(token2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetLatestValidTokenForUserAsync(_testUser.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task InvalidateAllUserTokensAsync_WithValidTokens_ShouldMarkAllAsUsed()
    {
        // Arrange
        var token1 = EmailVerificationToken.Create(_testUser.Id, 24);
        var token2 = EmailVerificationToken.Create(_testUser.Id, 24);
        var token3 = EmailVerificationToken.Create(_testUser.Id, 24);

        await _tokenRepository.AddAsync(token1);
        await _tokenRepository.AddAsync(token2);
        await _tokenRepository.AddAsync(token3);
        await _context.SaveChangesAsync();

        // Act
        await _tokenRepository.InvalidateAllUserTokensAsync(_testUser.Id);

        // Assert
        var tokens = await _context.EmailVerificationTokens
            .Where(t => t.UserId == _testUser.Id)
            .ToListAsync();
        
        tokens.Should().HaveCount(3);
        tokens.Should().OnlyContain(t => t.IsUsed);
    }

    [Fact]
    public async Task GetExpiredTokensAsync_WithExpiredTokens_ShouldReturnThem()
    {
        // Arrange
        var expiredToken1 = EmailVerificationToken.CreateExpiredToken(_testUser.Id, Guid.NewGuid().ToString(), 2);
        var expiredToken2 = EmailVerificationToken.CreateExpiredToken(_testUser.Id, Guid.NewGuid().ToString(), 1);
        var validToken = EmailVerificationToken.Create(_testUser.Id, 24);

        await _tokenRepository.AddAsync(expiredToken1);
        await _tokenRepository.AddAsync(expiredToken2);
        await _tokenRepository.AddAsync(validToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetExpiredTokensAsync(DateTime.UtcNow);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Id == expiredToken1.Id);
        result.Should().Contain(t => t.Id == expiredToken2.Id);
    }

    [Fact]
    public async Task DeleteExpiredTokensAsync_WithExpiredTokens_ShouldRemoveThemAndReturnCount()
    {
        // Arrange
        var expiredToken1 = EmailVerificationToken.CreateExpiredToken(_testUser.Id, Guid.NewGuid().ToString(), 2);
        var expiredToken2 = EmailVerificationToken.CreateExpiredToken(_testUser.Id, Guid.NewGuid().ToString(), 1);
        var validToken = EmailVerificationToken.Create(_testUser.Id, 24);

        await _tokenRepository.AddAsync(expiredToken1);
        await _tokenRepository.AddAsync(expiredToken2);
        await _tokenRepository.AddAsync(validToken);
        await _context.SaveChangesAsync();

        // Act
        var deletedCount = await _tokenRepository.DeleteExpiredTokensAsync(DateTime.UtcNow);

        // Assert
        deletedCount.Should().Be(2);
        var remainingTokens = await _context.EmailVerificationTokens.ToListAsync();
        remainingTokens.Should().HaveCount(1);
        remainingTokens[0].Id.Should().Be(validToken.Id);
    }

    [Fact]
    public async Task HasValidTokenAsync_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var token = EmailVerificationToken.Create(_testUser.Id, 24);
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.HasValidTokenAsync(_testUser.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasValidTokenAsync_WithOnlyUsedTokens_ShouldReturnFalse()
    {
        // Arrange
        var token = EmailVerificationToken.Create(_testUser.Id, 24);
        token.MarkAsUsed();
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.HasValidTokenAsync(_testUser.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnTokenWithUser()
    {
        // Arrange
        var token = EmailVerificationToken.Create(_testUser.Id, 24);
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetByIdAsync(token.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(token.Id);
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithDeletedToken_ShouldReturnNull()
    {
        // Arrange
        var token = EmailVerificationToken.Create(_testUser.Id, 24);
        token.SoftDelete();
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetByIdAsync(token.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldExcludeDeletedTokens()
    {
        // Arrange
        var token1 = EmailVerificationToken.Create(_testUser.Id, 24);
        var token2 = EmailVerificationToken.Create(_testUser.Id, 24);
        var deletedToken = EmailVerificationToken.Create(_testUser.Id, 24);
        deletedToken.SoftDelete();

        await _tokenRepository.AddAsync(token1);
        await _tokenRepository.AddAsync(token2);
        await _tokenRepository.AddAsync(deletedToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(t => t.IsDeleted);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

