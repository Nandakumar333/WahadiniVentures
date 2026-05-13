using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

/// <summary>
/// Integration tests for RefreshTokenRepository
/// Tests token creation, retrieval, revocation, expiration, and cleanup operations
/// </summary>
public class RefreshTokenRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly RefreshTokenRepository _tokenRepository;
    private readonly User _testUser;

    public RefreshTokenRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _tokenRepository = new RefreshTokenRepository(_context);

        _testUser = User.Create("test@example.com", "hashedPassword", "John", "Doe");
        _context.Users.Add(_testUser);
        _context.SaveChanges();
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidToken_ShouldPersistToDatabase()
    {
        // Arrange
        var token = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "TestDevice", "127.0.0.1");

        // Act
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Assert
        var savedToken = await _context.RefreshTokens.FindAsync(token.Id);
        savedToken.Should().NotBeNull();
        savedToken!.UserId.Should().Be(_testUser.Id);
        savedToken.IsUsed.Should().BeFalse();
        savedToken.IsRevoked.Should().BeFalse();
    }

    #endregion

    #region GetByTokenAsync Tests

    [Fact]
    public async Task GetByTokenAsync_WithValidToken_ShouldReturnToken()
    {
        // Arrange
        var token = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "TestDevice", "127.0.0.1");
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetByTokenAsync(token.Token);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be(token.Token);
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByTokenAsync_WithNullToken_ShouldReturnNull()
    {
        // Act
        var result = await _tokenRepository.GetByTokenAsync(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByTokenAsync_WithEmptyToken_ShouldReturnNull()
    {
        // Act
        var result = await _tokenRepository.GetByTokenAsync(string.Empty);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByTokenAsync_WithNonExistentToken_ShouldReturnNull()
    {
        // Act
        var result = await _tokenRepository.GetByTokenAsync("non-existent-token");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByTokenAsync_WithDeletedToken_ShouldReturnNull()
    {
        // Arrange
        var token = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "TestDevice", "127.0.0.1");
        token.SoftDelete();
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetByTokenAsync(token.Token);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetActiveTokensByUserIdAsync Tests

    [Fact]
    public async Task GetActiveTokensByUserIdAsync_WithActiveTokens_ShouldReturnThem()
    {
        // Arrange
        var token1 = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device1", "127.0.0.1");
        await Task.Delay(10);
        var token2 = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device2", "127.0.0.2");
        await Task.Delay(10);
        var token3 = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device3", "127.0.0.3");

        await _tokenRepository.AddAsync(token1);
        await _tokenRepository.AddAsync(token2);
        await _tokenRepository.AddAsync(token3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetActiveTokensByUserIdAsync(_testUser.Id);

        // Assert
        result.Should().HaveCount(3);
        var resultList = result.ToList();
        resultList[0].CreatedAt.Should().BeAfter(resultList[1].CreatedAt); // Ordered descending
    }

    [Fact]
    public async Task GetActiveTokensByUserIdAsync_ShouldExcludeUsedTokens()
    {
        // Arrange
        var activeToken = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device1", "127.0.0.1");
        var usedToken = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device2", "127.0.0.2");
        usedToken.MarkAsUsed();

        await _tokenRepository.AddAsync(activeToken);
        await _tokenRepository.AddAsync(usedToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetActiveTokensByUserIdAsync(_testUser.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(activeToken.Id);
    }

    [Fact]
    public async Task GetActiveTokensByUserIdAsync_ShouldExcludeRevokedTokens()
    {
        // Arrange
        var activeToken = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device1", "127.0.0.1");
        var revokedToken = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device2", "127.0.0.2");
        revokedToken.Revoke("System");

        await _tokenRepository.AddAsync(activeToken);
        await _tokenRepository.AddAsync(revokedToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetActiveTokensByUserIdAsync(_testUser.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(activeToken.Id);
    }

    [Fact]
    public async Task GetActiveTokensByUserIdAsync_ShouldExcludeExpiredTokens()
    {
        // Arrange
        var activeToken = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device1", "127.0.0.1");
        var expiredToken = CreateExpiredToken(_testUser.Id, DateTime.UtcNow.AddDays(-1), "Device2", "127.0.0.2");

        await _tokenRepository.AddAsync(activeToken);
        await _tokenRepository.AddAsync(expiredToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetActiveTokensByUserIdAsync(_testUser.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(activeToken.Id);
    }

    [Fact]
    public async Task GetActiveTokensByUserIdAsync_WithNoTokens_ShouldReturnEmpty()
    {
        // Act
        var result = await _tokenRepository.GetActiveTokensByUserIdAsync(_testUser.Id);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetAllTokensByUserIdAsync Tests

    [Fact]
    public async Task GetAllTokensByUserIdAsync_ShouldReturnAllNonDeletedTokens()
    {
        // Arrange
        var activeToken = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device1", "127.0.0.1");
        var usedToken = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device2", "127.0.0.2");
        usedToken.MarkAsUsed();
        var revokedToken = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device3", "127.0.0.3");
        revokedToken.Revoke("System");
        var deletedToken = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device4", "127.0.0.4");
        deletedToken.SoftDelete();

        await _tokenRepository.AddAsync(activeToken);
        await _tokenRepository.AddAsync(usedToken);
        await _tokenRepository.AddAsync(revokedToken);
        await _tokenRepository.AddAsync(deletedToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetAllTokensByUserIdAsync(_testUser.Id);

        // Assert
        result.Should().HaveCount(3); // Excludes deleted
        result.Should().NotContain(t => t.IsDeleted);
    }

    #endregion

    #region RevokeTokenAsync Tests

    [Fact]
    public async Task RevokeTokenAsync_WithValidToken_ShouldRevokeAndReturnTrue()
    {
        // Arrange
        var token = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "TestDevice", "127.0.0.1");
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.RevokeTokenAsync(token.Token, "TestUser");

        // Assert
        result.Should().BeTrue();
        var revokedToken = await _context.RefreshTokens.FindAsync(token.Id);
        revokedToken!.IsRevoked.Should().BeTrue();
        revokedToken.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeTokenAsync_WithNonExistentToken_ShouldReturnFalse()
    {
        // Act
        var result = await _tokenRepository.RevokeTokenAsync("non-existent-token");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeTokenAsync_WithNullToken_ShouldReturnFalse()
    {
        // Act
        var result = await _tokenRepository.RevokeTokenAsync(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeTokenAsync_WithEmptyToken_ShouldReturnFalse()
    {
        // Act
        var result = await _tokenRepository.RevokeTokenAsync(string.Empty);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region RevokeAllUserTokensAsync Tests

    [Fact]
    public async Task RevokeAllUserTokensAsync_WithActiveTokens_ShouldRevokeAllAndReturnCount()
    {
        // Arrange
        var token1 = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device1", "127.0.0.1");
        var token2 = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device2", "127.0.0.2");
        var token3 = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device3", "127.0.0.3");

        await _tokenRepository.AddAsync(token1);
        await _tokenRepository.AddAsync(token2);
        await _tokenRepository.AddAsync(token3);
        await _context.SaveChangesAsync();

        // Act
        var count = await _tokenRepository.RevokeAllUserTokensAsync(_testUser.Id, "Admin");

        // Assert
        count.Should().Be(3);
        var tokens = await _context.RefreshTokens.Where(t => t.UserId == _testUser.Id).ToListAsync();
        tokens.Should().OnlyContain(t => t.IsRevoked);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_ShouldOnlyRevokeActiveTokens()
    {
        // Arrange
        var activeToken = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device1", "127.0.0.1");
        var alreadyRevoked = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device2", "127.0.0.2");
        alreadyRevoked.Revoke("PreviousRevoke");
        var usedToken = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device3", "127.0.0.3");
        usedToken.MarkAsUsed();

        await _tokenRepository.AddAsync(activeToken);
        await _tokenRepository.AddAsync(alreadyRevoked);
        await _tokenRepository.AddAsync(usedToken);
        await _context.SaveChangesAsync();

        // Act
        var count = await _tokenRepository.RevokeAllUserTokensAsync(_testUser.Id);

        // Assert
        count.Should().Be(1); // Only the active token
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WithNoTokens_ShouldReturnZero()
    {
        // Act
        var count = await _tokenRepository.RevokeAllUserTokensAsync(_testUser.Id);

        // Assert
        count.Should().Be(0);
    }

    #endregion

    #region RemoveExpiredTokensAsync Tests

    [Fact]
    public async Task RemoveExpiredTokensAsync_WithExpiredTokens_ShouldSoftDeleteAndReturnCount()
    {
        // Arrange
        var expiredToken1 = CreateExpiredToken(_testUser.Id, DateTime.UtcNow.AddDays(-8), "Device1", "127.0.0.1");
        var expiredToken2 = CreateExpiredToken(_testUser.Id, DateTime.UtcNow.AddDays(-2), "Device2", "127.0.0.2");
        var validToken = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device3", "127.0.0.3");

        await _tokenRepository.AddAsync(expiredToken1);
        await _tokenRepository.AddAsync(expiredToken2);
        await _tokenRepository.AddAsync(validToken);
        await _context.SaveChangesAsync();

        // Act
        var count = await _tokenRepository.RemoveExpiredTokensAsync(DateTime.UtcNow);

        // Assert
        count.Should().Be(2);
        
        var allTokens = await _context.RefreshTokens.IgnoreQueryFilters().ToListAsync();
        var deletedTokens = allTokens.Where(t => t.IsDeleted).ToList();
        deletedTokens.Should().HaveCount(2);
        deletedTokens.Should().Contain(t => t.Id == expiredToken1.Id);
        deletedTokens.Should().Contain(t => t.Id == expiredToken2.Id);
    }

    [Fact]
    public async Task RemoveExpiredTokensAsync_WithNoneExpired_ShouldReturnZero()
    {
        // Arrange
        var token = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "TestDevice", "127.0.0.1");
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var count = await _tokenRepository.RemoveExpiredTokensAsync(DateTime.UtcNow);

        // Assert
        count.Should().Be(0);
        token.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveExpiredTokensAsync_ShouldOnlyDeleteTokensOlderThanCutoff()
    {
        // Arrange
        var veryOldToken = CreateExpiredToken(_testUser.Id, DateTime.UtcNow.AddDays(-30), "Device1", "127.0.0.1");
        var oldToken = CreateExpiredToken(_testUser.Id, DateTime.UtcNow.AddDays(-8), "Device2", "127.0.0.2");
        var recentExpired = CreateExpiredToken(_testUser.Id, DateTime.UtcNow.AddDays(-1), "Device3", "127.0.0.3");
        var validToken = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(1), "Device4", "127.0.0.4");

        await _tokenRepository.AddAsync(veryOldToken);
        await _tokenRepository.AddAsync(oldToken);
        await _tokenRepository.AddAsync(recentExpired);
        await _tokenRepository.AddAsync(validToken);
        await _context.SaveChangesAsync();

        // Act - Delete tokens older than 7 days
        var count = await _tokenRepository.RemoveExpiredTokensAsync(DateTime.UtcNow.AddDays(-7));

        // Assert
        count.Should().Be(2); // veryOldToken and oldToken
        
        // Query all tokens including deleted to verify soft delete behavior
        var allTokens = await _context.RefreshTokens.IgnoreQueryFilters().ToListAsync();
        allTokens.Should().HaveCount(4);
        
        var deletedTokens = allTokens.Where(t => t.IsDeleted).ToList();
        deletedTokens.Should().HaveCount(2);
        deletedTokens.Should().Contain(t => t.Id == veryOldToken.Id);
        deletedTokens.Should().Contain(t => t.Id == oldToken.Id);
        
        // Query non-deleted tokens (without IgnoreQueryFilters won't work since no global filter exists)
        var nonDeletedTokens = allTokens.Where(t => !t.IsDeleted).ToList();
        nonDeletedTokens.Should().HaveCount(2);
        nonDeletedTokens.Should().Contain(t => t.Id == recentExpired.Id);
        nonDeletedTokens.Should().Contain(t => t.Id == validToken.Id);
    }

    #endregion

    #region GetActiveTokenCountAsync Tests

    [Fact]
    public async Task GetActiveTokenCountAsync_WithActiveTokens_ShouldReturnCorrectCount()
    {
        // Arrange
        var token1 = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device1", "127.0.0.1");
        var token2 = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device2", "127.0.0.2");
        var token3 = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device3", "127.0.0.3");

        await _tokenRepository.AddAsync(token1);
        await _tokenRepository.AddAsync(token2);
        await _tokenRepository.AddAsync(token3);
        await _context.SaveChangesAsync();

        // Act
        var count = await _tokenRepository.GetActiveTokenCountAsync(_testUser.Id);

        // Assert
        count.Should().Be(3);
    }

    [Fact]
    public async Task GetActiveTokenCountAsync_ShouldExcludeInactiveTokens()
    {
        // Arrange
        var activeToken = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device1", "127.0.0.1");
        var usedToken = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device2", "127.0.0.2");
        usedToken.MarkAsUsed();
        var revokedToken = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "Device3", "127.0.0.3");
        revokedToken.Revoke("System");
        var expiredToken = CreateExpiredToken(_testUser.Id, DateTime.UtcNow.AddDays(-1), "Device4", "127.0.0.4");

        await _tokenRepository.AddAsync(activeToken);
        await _tokenRepository.AddAsync(usedToken);
        await _tokenRepository.AddAsync(revokedToken);
        await _tokenRepository.AddAsync(expiredToken);
        await _context.SaveChangesAsync();

        // Act
        var count = await _tokenRepository.GetActiveTokenCountAsync(_testUser.Id);

        // Assert
        count.Should().Be(1);
    }

    [Fact]
    public async Task GetActiveTokenCountAsync_WithNoTokens_ShouldReturnZero()
    {
        // Act
        var count = await _tokenRepository.GetActiveTokenCountAsync(_testUser.Id);

        // Assert
        count.Should().Be(0);
    }

    #endregion

    #region IsValidTokenAsync Tests

    [Fact]
    public async Task IsValidTokenAsync_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var token = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "TestDevice", "127.0.0.1");
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.IsValidTokenAsync(token.Token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsValidTokenAsync_WithUsedToken_ShouldReturnFalse()
    {
        // Arrange
        var token = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "TestDevice", "127.0.0.1");
        token.MarkAsUsed();
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.IsValidTokenAsync(token.Token);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsValidTokenAsync_WithRevokedToken_ShouldReturnFalse()
    {
        // Arrange
        var token = RefreshToken.Create(_testUser.Id, DateTime.UtcNow.AddDays(7), "TestDevice", "127.0.0.1");
        token.Revoke("System");
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.IsValidTokenAsync(token.Token);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsValidTokenAsync_WithExpiredToken_ShouldReturnFalse()
    {
        // Arrange
        var token = CreateExpiredToken(_testUser.Id, DateTime.UtcNow.AddDays(-1), "TestDevice", "127.0.0.1");
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.IsValidTokenAsync(token.Token);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsValidTokenAsync_WithNullToken_ShouldReturnFalse()
    {
        // Act
        var result = await _tokenRepository.IsValidTokenAsync(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsValidTokenAsync_WithEmptyToken_ShouldReturnFalse()
    {
        // Act
        var result = await _tokenRepository.IsValidTokenAsync(string.Empty);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsValidTokenAsync_WithNonExistentToken_ShouldReturnFalse()
    {
        // Act
        var result = await _tokenRepository.IsValidTokenAsync("non-existent-token");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    /// <summary>
    /// Helper method to create an expired token for testing
    /// Uses reflection to bypass validation in Create method
    /// </summary>
    private static RefreshToken CreateExpiredToken(Guid userId, DateTime expiresAt, string? deviceInfo = null, string? ipAddress = null)
    {
        var token = RefreshToken.Create(userId, DateTime.UtcNow.AddDays(1), deviceInfo, ipAddress);
        
        // Use reflection to set the ExpiresAt property to a past date
        var expiresAtProperty = typeof(RefreshToken).GetProperty(nameof(RefreshToken.ExpiresAt));
        expiresAtProperty!.SetValue(token, expiresAt);
        
        return token;
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

