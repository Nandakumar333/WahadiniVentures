using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

/// <summary>
/// Integration tests for PasswordResetTokenRepository
/// Tests token creation, retrieval, validation, invalidation, and cleanup operations
/// </summary>
public class PasswordResetTokenRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordResetTokenRepository _tokenRepository;
    private readonly User _testUser;

    public PasswordResetTokenRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _tokenRepository = new PasswordResetTokenRepository(_context);

        _testUser = User.Create("test@example.com", "hashedPassword", "John", "Doe");
        _context.Users.Add(_testUser);
        _context.SaveChanges();
    }

    #region AddAsync and GetByHashedTokenAsync Tests

    [Fact]
    public async Task AddAsync_WithValidToken_ShouldPersistToDatabase()
    {
        // Arrange
        var token = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1), "Browser", "127.0.0.1");
        var hashedToken = token.HashedToken;

        // Act
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Assert
        var savedToken = await _context.PasswordResetTokens.FindAsync(token.Id);
        savedToken.Should().NotBeNull();
        savedToken!.UserId.Should().Be(_testUser.Id);
        savedToken.HashedToken.Should().Be(hashedToken);
        savedToken.IsUsed.Should().BeFalse();
    }

    [Fact]
    public async Task GetByHashedTokenAsync_WithValidHashedToken_ShouldReturnToken()
    {
        // Arrange
        var token = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetByHashedTokenAsync(token.HashedToken);

        // Assert
        result.Should().NotBeNull();
        result!.HashedToken.Should().Be(token.HashedToken);
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByHashedTokenAsync_WithNonExistentToken_ShouldReturnNull()
    {
        // Act
        var result = await _tokenRepository.GetByHashedTokenAsync("non-existent-hash");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetValidTokenAsync Tests

    [Fact]
    public async Task GetValidTokenAsync_WithValidRawToken_ShouldReturnToken()
    {
        // Arrange
        var token = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        var rawToken = token.Token; // Capture before persisting
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetValidTokenAsync(rawToken);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(token.Id);
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task GetValidTokenAsync_WithUsedToken_ShouldReturnNull()
    {
        // Arrange
        var token = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        var rawToken = token.Token;
        token.MarkAsUsed();
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetValidTokenAsync(rawToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetValidTokenAsync_WithExpiredToken_ShouldReturnNull()
    {
        // Arrange
        var token = CreateExpiredToken(_testUser.Id, DateTime.UtcNow.AddHours(-1));
        var rawToken = token.Token;
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetValidTokenAsync(rawToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetValidTokenAsync_WithNullToken_ShouldReturnNull()
    {
        // Act
        var result = await _tokenRepository.GetValidTokenAsync(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetValidTokenAsync_WithEmptyToken_ShouldReturnNull()
    {
        // Act
        var result = await _tokenRepository.GetValidTokenAsync(string.Empty);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetValidTokenAsync_WithDeletedToken_ShouldReturnNull()
    {
        // Arrange
        var token = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        var rawToken = token.Token;
        token.SoftDelete();
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetValidTokenAsync(rawToken);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetActiveTokensByUserIdAsync Tests

    [Fact]
    public async Task GetActiveTokensByUserIdAsync_WithActiveTokens_ShouldReturnThem()
    {
        // Arrange
        var token1 = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        await Task.Delay(10);
        var token2 = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        await Task.Delay(10);
        var token3 = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));

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
        var activeToken = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        var usedToken = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
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
    public async Task GetActiveTokensByUserIdAsync_ShouldExcludeExpiredTokens()
    {
        // Arrange
        var activeToken = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        var expiredToken = CreateExpiredToken(_testUser.Id, DateTime.UtcNow.AddHours(-1));

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
    public async Task GetActiveTokensByUserIdAsync_ShouldExcludeDeletedTokens()
    {
        // Arrange
        var activeToken = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        var deletedToken = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        deletedToken.SoftDelete();

        await _tokenRepository.AddAsync(activeToken);
        await _tokenRepository.AddAsync(deletedToken);
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
        var activeToken = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        var usedToken = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        usedToken.MarkAsUsed();
        var expiredToken = CreateExpiredToken(_testUser.Id, DateTime.UtcNow.AddHours(-1));
        var deletedToken = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        deletedToken.SoftDelete();

        await _tokenRepository.AddAsync(activeToken);
        await _tokenRepository.AddAsync(usedToken);
        await _tokenRepository.AddAsync(expiredToken);
        await _tokenRepository.AddAsync(deletedToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetAllTokensByUserIdAsync(_testUser.Id);

        // Assert
        result.Should().HaveCount(3); // Excludes deleted
        result.Should().NotContain(t => t.IsDeleted);
    }

    #endregion

    #region MarkTokenAsUsedAsync Tests

    [Fact]
    public async Task MarkTokenAsUsedAsync_WithValidToken_ShouldMarkAsUsedAndReturnTrue()
    {
        // Arrange
        var token = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.MarkTokenAsUsedAsync(token.Id, "TestUser");

        // Assert
        result.Should().BeTrue();
        var updatedToken = await _context.PasswordResetTokens.FindAsync(token.Id);
        updatedToken!.IsUsed.Should().BeTrue();
        updatedToken.UsedAt.Should().NotBeNull();
        updatedToken.UpdatedBy.Should().Be("TestUser");
    }

    [Fact]
    public async Task MarkTokenAsUsedAsync_WithNonExistentToken_ShouldReturnFalse()
    {
        // Act
        var result = await _tokenRepository.MarkTokenAsUsedAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task MarkTokenAsUsedAsync_WithDeletedToken_ShouldReturnFalse()
    {
        // Arrange
        var token = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        token.SoftDelete();
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.MarkTokenAsUsedAsync(token.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region InvalidateAllUserTokensAsync Tests

    [Fact]
    public async Task InvalidateAllUserTokensAsync_WithActiveTokens_ShouldInvalidateAllAndReturnCount()
    {
        // Arrange
        var token1 = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        var token2 = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        var token3 = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));

        await _tokenRepository.AddAsync(token1);
        await _tokenRepository.AddAsync(token2);
        await _tokenRepository.AddAsync(token3);
        await _context.SaveChangesAsync();

        // Act
        var count = await _tokenRepository.InvalidateAllUserTokensAsync(_testUser.Id, "Admin");

        // Assert
        count.Should().Be(3);
        var tokens = await _context.PasswordResetTokens.Where(t => t.UserId == _testUser.Id).ToListAsync();
        tokens.Should().OnlyContain(t => t.IsUsed);
        tokens.Should().OnlyContain(t => t.UpdatedBy == "Admin");
    }

    [Fact]
    public async Task InvalidateAllUserTokensAsync_ShouldOnlyInvalidateActiveTokens()
    {
        // Arrange
        var activeToken = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        var alreadyUsed = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        alreadyUsed.MarkAsUsed("PreviousUse");
        var expiredToken = CreateExpiredToken(_testUser.Id, DateTime.UtcNow.AddHours(-1));

        await _tokenRepository.AddAsync(activeToken);
        await _tokenRepository.AddAsync(alreadyUsed);
        await _tokenRepository.AddAsync(expiredToken);
        await _context.SaveChangesAsync();

        // Act
        var count = await _tokenRepository.InvalidateAllUserTokensAsync(_testUser.Id);

        // Assert
        count.Should().Be(1); // Only the active token
    }

    [Fact]
    public async Task InvalidateAllUserTokensAsync_WithNoTokens_ShouldReturnZero()
    {
        // Act
        var count = await _tokenRepository.InvalidateAllUserTokensAsync(_testUser.Id);

        // Assert
        count.Should().Be(0);
    }

    #endregion

    #region RemoveExpiredTokensAsync Tests

    [Fact]
    public async Task RemoveExpiredTokensAsync_WithExpiredTokens_ShouldRemoveAndReturnCount()
    {
        // Arrange
        var expiredToken1 = CreateExpiredToken(_testUser.Id, DateTime.UtcNow.AddDays(-8));
        var expiredToken2 = CreateExpiredToken(_testUser.Id, DateTime.UtcNow.AddDays(-2));
        var validToken = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));

        await _tokenRepository.AddAsync(expiredToken1);
        await _tokenRepository.AddAsync(expiredToken2);
        await _tokenRepository.AddAsync(validToken);
        await _context.SaveChangesAsync();

        // Act
        var count = await _tokenRepository.RemoveExpiredTokensAsync(DateTime.UtcNow);

        // Assert
        count.Should().Be(2);
        
        var allTokens = await _context.PasswordResetTokens.IgnoreQueryFilters().ToListAsync();
        allTokens.Should().HaveCount(1); // Only validToken remains (hard delete)
        allTokens.First().Id.Should().Be(validToken.Id);
    }

    [Fact]
    public async Task RemoveExpiredTokensAsync_WithNoneExpired_ShouldReturnZero()
    {
        // Arrange
        var token = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        await _tokenRepository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var count = await _tokenRepository.RemoveExpiredTokensAsync(DateTime.UtcNow);

        // Assert
        count.Should().Be(0);
        var allTokens = await _context.PasswordResetTokens.ToListAsync();
        allTokens.Should().HaveCount(1);
    }

    [Fact]
    public async Task RemoveExpiredTokensAsync_ShouldOnlyRemoveTokensOlderThanCutoff()
    {
        // Arrange
        var veryOldToken = CreateExpiredToken(_testUser.Id, DateTime.UtcNow.AddDays(-30));
        var oldToken = CreateExpiredToken(_testUser.Id, DateTime.UtcNow.AddDays(-8));
        var recentExpired = CreateExpiredToken(_testUser.Id, DateTime.UtcNow.AddHours(-2));
        var validToken = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));

        await _tokenRepository.AddAsync(veryOldToken);
        await _tokenRepository.AddAsync(oldToken);
        await _tokenRepository.AddAsync(recentExpired);
        await _tokenRepository.AddAsync(validToken);
        await _context.SaveChangesAsync();

        // Act - Remove tokens older than 7 days
        var count = await _tokenRepository.RemoveExpiredTokensAsync(DateTime.UtcNow.AddDays(-7));

        // Assert
        count.Should().Be(2); // veryOldToken and oldToken
        
        var remainingTokens = await _context.PasswordResetTokens.ToListAsync();
        remainingTokens.Should().HaveCount(2);
        remainingTokens.Should().Contain(t => t.Id == recentExpired.Id);
        remainingTokens.Should().Contain(t => t.Id == validToken.Id);
    }

    [Fact]
    public async Task RemoveExpiredTokensAsync_ShouldAlsoRemoveDeletedTokens()
    {
        // Arrange
        var deletedToken = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        deletedToken.SoftDelete();
        var validToken = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));

        await _tokenRepository.AddAsync(deletedToken);
        await _tokenRepository.AddAsync(validToken);
        await _context.SaveChangesAsync();

        // Act
        var count = await _tokenRepository.RemoveExpiredTokensAsync(DateTime.UtcNow.AddDays(-1));

        // Assert
        count.Should().Be(1); // Deleted token removed
        
        var remainingTokens = await _context.PasswordResetTokens.IgnoreQueryFilters().ToListAsync();
        remainingTokens.Should().HaveCount(1);
        remainingTokens.First().Id.Should().Be(validToken.Id);
    }

    #endregion

    #region GetActiveTokenCountAsync Tests

    [Fact]
    public async Task GetActiveTokenCountAsync_WithActiveTokens_ShouldReturnCorrectCount()
    {
        // Arrange
        var token1 = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        var token2 = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        var token3 = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));

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
        var activeToken = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        var usedToken = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        usedToken.MarkAsUsed();
        var expiredToken = CreateExpiredToken(_testUser.Id, DateTime.UtcNow.AddHours(-1));
        var deletedToken = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        deletedToken.SoftDelete();

        await _tokenRepository.AddAsync(activeToken);
        await _tokenRepository.AddAsync(usedToken);
        await _tokenRepository.AddAsync(expiredToken);
        await _tokenRepository.AddAsync(deletedToken);
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

    #region GetLatestTokenByUserIdAsync Tests

    [Fact]
    public async Task GetLatestTokenByUserIdAsync_WithMultipleTokens_ShouldReturnMostRecent()
    {
        // Arrange
        var token1 = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        await Task.Delay(10);
        var token2 = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        await Task.Delay(10);
        var token3 = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));

        await _tokenRepository.AddAsync(token1);
        await _tokenRepository.AddAsync(token2);
        await _tokenRepository.AddAsync(token3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetLatestTokenByUserIdAsync(_testUser.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(token3.Id); // Most recent
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task GetLatestTokenByUserIdAsync_ShouldExcludeDeletedTokens()
    {
        // Arrange
        var oldToken = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        await Task.Delay(10);
        var newestToken = PasswordResetToken.Create(_testUser.Id, DateTime.UtcNow.AddHours(1));
        newestToken.SoftDelete();

        await _tokenRepository.AddAsync(oldToken);
        await _tokenRepository.AddAsync(newestToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetLatestTokenByUserIdAsync(_testUser.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(oldToken.Id); // Deleted token excluded
    }

    [Fact]
    public async Task GetLatestTokenByUserIdAsync_WithNoTokens_ShouldReturnNull()
    {
        // Act
        var result = await _tokenRepository.GetLatestTokenByUserIdAsync(_testUser.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    /// <summary>
    /// Helper method to create an expired token for testing
    /// Uses reflection to bypass validation in Create method
    /// </summary>
    private static PasswordResetToken CreateExpiredToken(Guid userId, DateTime expiresAt)
    {
        var token = PasswordResetToken.Create(userId, DateTime.UtcNow.AddHours(1));
        
        // Use reflection to set the ExpiresAt property to a past date
        var expiresAtProperty = typeof(PasswordResetToken).GetProperty(nameof(PasswordResetToken.ExpiresAt));
        expiresAtProperty!.SetValue(token, expiresAt);
        
        return token;
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

