using FluentAssertions;
using WahadiniCryptoQuest.Core.Entities;
using Xunit;

namespace WahadiniCryptoQuest.Core.Tests.Entities;

public class PasswordResetTokenTests
{
    [Fact]
    public void Create_GeneratesSecureToken()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var token = PasswordResetToken.Create(userId);

        // Assert
        token.Token.Should().NotBeNullOrEmpty();
        token.Token.Length.Should().BeGreaterThan(30); // URL-safe base64 of 32 bytes
        token.HashedToken.Should().NotBeNullOrEmpty();
        token.HashedToken.Should().NotBe(token.Token); // Hashed token should differ from raw token
    }

    [Fact]
    public void Create_WithDefaultExpiration_SetsExpirationTo1Hour()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var beforeCreation = DateTime.UtcNow;

        // Act
        var token = PasswordResetToken.Create(userId);

        // Assert
        var expectedExpiry = beforeCreation.AddHours(1);
        token.ExpiresAt.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(2));
        token.GetRemainingTime().Should().BeCloseTo(TimeSpan.FromHours(1), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void IsValid_ForUnusedNonExpiredToken_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Act
        var token = PasswordResetToken.Create(userId, expiresAt);

        // Assert
        token.IsValid.Should().BeTrue();
        token.IsUsed.Should().BeFalse();
        token.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ForUsedToken_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var token = PasswordResetToken.Create(userId, expiresAt);

        // Act
        token.MarkAsUsed("TestUser");

        // Assert
        token.IsValid.Should().BeFalse();
        token.IsUsed.Should().BeTrue();
        token.UsedAt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void IsValid_ForExpiredToken_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMilliseconds(10);
        var token = PasswordResetToken.Create(userId, expiresAt);

        // Act - Wait for expiration
        System.Threading.Thread.Sleep(15);

        // Assert
        token.IsValid.Should().BeFalse();
        token.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void MarkAsUsed_SetsIsUsedTrueAndUsedAtTimestamp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var token = PasswordResetToken.Create(userId, expiresAt);

        // Act
        token.MarkAsUsed("TestUser");

        // Assert
        token.IsUsed.Should().BeTrue();
        token.UsedAt.Should().NotBeNullOrEmpty();
        token.UsedAt.Should().Contain("UTC by TestUser");
        token.IsValid.Should().BeFalse();
    }

    [Fact]
    public void MarkAsUsed_WhenAlreadyUsed_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var token = PasswordResetToken.Create(userId, expiresAt);
        token.MarkAsUsed();

        // Act
        var action = () => token.MarkAsUsed();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*already been used*");
    }

    [Fact]
    public void MarkAsUsed_WhenExpired_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMilliseconds(10);
        var token = PasswordResetToken.Create(userId, expiresAt);

        // Wait for expiration
        System.Threading.Thread.Sleep(15);

        // Act
        var action = () => token.MarkAsUsed();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot use expired token*");
    }

    [Fact]
    public void IsExpired_CorrectlyValidatesExpiration()
    {
        // Arrange - Future expiry
        var userId = Guid.NewGuid();
        var futureExpiry = DateTime.UtcNow.AddHours(1);
        var validToken = PasswordResetToken.Create(userId, futureExpiry);

        // Arrange - Create token that will expire soon
        var shortExpiry = DateTime.UtcNow.AddMilliseconds(10);
        var expiringToken = PasswordResetToken.Create(userId, shortExpiry);

        // Act - Wait for expiration
        System.Threading.Thread.Sleep(15);

        // Assert
        validToken.IsExpired.Should().BeFalse();
        expiringToken.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void GetRemainingTime_ForValidToken_ReturnsCorrectTimeSpan()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var token = PasswordResetToken.Create(userId, expiresAt);

        // Act
        var remaining = token.GetRemainingTime();

        // Assert
        remaining.Should().BeCloseTo(TimeSpan.FromHours(1), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void GetRemainingTime_ForExpiredToken_ReturnsZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMilliseconds(10);
        var token = PasswordResetToken.Create(userId, expiresAt);

        // Act - Wait for expiration
        System.Threading.Thread.Sleep(15);
        var remaining = token.GetRemainingTime();

        // Assert
        remaining.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void MatchesToken_WithCorrectRawToken_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var token = PasswordResetToken.Create(userId, expiresAt);
        var rawToken = token.Token; // Get the raw token before it's cleared

        // Act
        var matches = token.MatchesToken(rawToken);

        // Assert
        matches.Should().BeTrue();
    }

    [Fact]
    public void MatchesToken_WithIncorrectRawToken_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var token = PasswordResetToken.Create(userId, expiresAt);

        // Act
        var matches = token.MatchesToken("incorrect-token-string");

        // Assert
        matches.Should().BeFalse();
    }

    [Fact]
    public void MatchesToken_ForUsedToken_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var token = PasswordResetToken.Create(userId, expiresAt);
        var rawToken = token.Token;
        token.MarkAsUsed();

        // Act
        var matches = token.MatchesToken(rawToken);

        // Assert
        matches.Should().BeFalse(); // Used tokens should not match
    }

    [Fact]
    public void MatchesToken_ForExpiredToken_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMilliseconds(10);
        var token = PasswordResetToken.Create(userId, expiresAt);
        var rawToken = token.Token;

        // Wait for expiration
        System.Threading.Thread.Sleep(15);

        // Act
        var matches = token.MatchesToken(rawToken);

        // Assert
        matches.Should().BeFalse(); // Expired tokens should not match
    }

    [Fact]
    public void MatchesToken_WithNullOrEmptyToken_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var token = PasswordResetToken.Create(userId, expiresAt);

        // Act & Assert
        token.MatchesToken(null!).Should().BeFalse();
        token.MatchesToken(string.Empty).Should().BeFalse();
        token.MatchesToken("   ").Should().BeFalse();
    }

    [Fact]
    public void Create_WithEmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var emptyUserId = Guid.Empty;
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Act
        var action = () => PasswordResetToken.Create(emptyUserId, expiresAt);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*User ID cannot be empty*");
    }

    [Fact]
    public void Create_WithPastExpirationTime_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pastExpiry = DateTime.UtcNow.AddHours(-1);

        // Act
        var action = () => PasswordResetToken.Create(userId, pastExpiry);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Expiration time must be in the future*");
    }

    [Fact]
    public void Create_WithClientInfoAndIpAddress_StoresCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var clientInfo = "Chrome/120.0 Windows 10";
        var ipAddress = "192.168.1.100";

        // Act
        var token = PasswordResetToken.Create(userId, expiresAt, clientInfo, ipAddress);

        // Assert
        token.UserId.Should().Be(userId);
        token.ClientInfo.Should().Be(clientInfo);
        token.IpAddress.Should().Be(ipAddress);
    }

    [Fact]
    public void Create_GeneratesUniqueTokensForSameUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Act
        var token1 = PasswordResetToken.Create(userId, expiresAt);
        var token2 = PasswordResetToken.Create(userId, expiresAt);

        // Assert
        token1.Token.Should().NotBe(token2.Token);
        token1.HashedToken.Should().NotBe(token2.HashedToken);
    }

    [Fact]
    public void Token_IsUrlSafe()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Act
        var token = PasswordResetToken.Create(userId, expiresAt);

        // Assert
        token.Token.Should().NotContain("+");
        token.Token.Should().NotContain("/");
        token.Token.Should().NotContain("=");
        // URL-safe base64 uses - and _ instead
    }
}
