using FluentAssertions;
using WahadiniCryptoQuest.Core.Entities;
using Xunit;

namespace WahadiniCryptoQuest.Core.Tests.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void Create_WithValidData_GeneratesUniqueSecureToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var token1 = RefreshToken.Create(userId, expiresAt);
        var token2 = RefreshToken.Create(userId, expiresAt);

        // Assert
        token1.Token.Should().NotBeNullOrEmpty();
        token2.Token.Should().NotBeNullOrEmpty();
        token1.Token.Should().NotBe(token2.Token); // Tokens should be unique
        token1.Token.Length.Should().BeGreaterThan(80); // Base64 of 64 bytes is ~88 chars
    }

    [Fact]
    public void Create_WithValidData_SetsExpirationTo7Days()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var token = RefreshToken.Create(userId, expiresAt);

        // Assert
        token.ExpiresAt.Should().BeCloseTo(expiresAt, TimeSpan.FromSeconds(1));
        token.GetRemainingTime().Should().BeCloseTo(TimeSpan.FromDays(7), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void IsValid_ForNonRevokedNonExpiredToken_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var token = RefreshToken.Create(userId, expiresAt);

        // Act & Assert
        token.IsValid.Should().BeTrue();
        token.IsUsed.Should().BeFalse();
        token.IsRevoked.Should().BeFalse();
        token.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ForRevokedToken_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var token = RefreshToken.Create(userId, expiresAt);

        // Act
        token.Revoke();

        // Assert
        token.IsValid.Should().BeFalse();
        token.IsRevoked.Should().BeTrue();
        token.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public void IsValid_ForExpiredToken_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMilliseconds(10);
        var token = RefreshToken.Create(userId, expiresAt);

        // Act - Wait for expiration
        System.Threading.Thread.Sleep(15);

        // Assert
        token.IsValid.Should().BeFalse();
        token.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_CorrectlyChecksExpirationTime()
    {
        // Arrange - Not expired
        var userId = Guid.NewGuid();
        var futureExpiry = DateTime.UtcNow.AddDays(7);
        var validToken = RefreshToken.Create(userId, futureExpiry);

        // Arrange - Create token that will expire soon
        var shortExpiry = DateTime.UtcNow.AddMilliseconds(10);
        var expiringToken = RefreshToken.Create(userId, shortExpiry);

        // Act - Wait for expiration
        System.Threading.Thread.Sleep(15);

        // Assert
        validToken.IsExpired.Should().BeFalse();
        expiringToken.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void Revoke_SetsIsRevokedTrueAndRevokedAtTimestamp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var token = RefreshToken.Create(userId, expiresAt);
        var beforeRevoke = DateTime.UtcNow;

        // Act
        token.Revoke("TestUser");

        // Assert
        token.IsRevoked.Should().BeTrue();
        token.RevokedAt.Should().NotBeNull();
        token.RevokedAt.Should().BeOnOrAfter(beforeRevoke);
        token.UpdatedBy.Should().Be("TestUser");
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_DoesNotThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var token = RefreshToken.Create(userId, expiresAt);
        token.Revoke();
        var firstRevokedAt = token.RevokedAt;

        // Act
        var action = () => token.Revoke();

        // Assert
        action.Should().NotThrow();
        token.RevokedAt.Should().Be(firstRevokedAt); // Should not change
    }

    [Fact]
    public void MarkAsUsed_SetsIsUsedTrueAndUsedAtTimestamp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var token = RefreshToken.Create(userId, expiresAt);
        var beforeUse = DateTime.UtcNow;

        // Act
        token.MarkAsUsed("TestUser");

        // Assert
        token.IsUsed.Should().BeTrue();
        token.UsedAt.Should().NotBeNull();
        token.UsedAt.Should().BeOnOrAfter(beforeUse);
        token.UpdatedBy.Should().Be("TestUser");
        token.IsValid.Should().BeFalse(); // Used tokens are invalid
    }

    [Fact]
    public void MatchesToken_WithMatchingToken_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var token = RefreshToken.Create(userId, expiresAt);
        var tokenString = token.Token;

        // Act
        var matches = token.MatchesToken(tokenString);

        // Assert
        matches.Should().BeTrue();
    }

    [Fact]
    public void MatchesToken_WithNonMatchingToken_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var token = RefreshToken.Create(userId, expiresAt);

        // Act
        var matches = token.MatchesToken("different-token-string");

        // Assert
        matches.Should().BeFalse();
    }

    [Fact]
    public void MatchesToken_WithNullOrEmptyToken_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var token = RefreshToken.Create(userId, expiresAt);

        // Act & Assert
        token.MatchesToken(null!).Should().BeFalse();
        token.MatchesToken(string.Empty).Should().BeFalse();
    }

    [Fact]
    public void GetRemainingTime_ForValidToken_ReturnsCorrectTimeSpan()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var token = RefreshToken.Create(userId, expiresAt);

        // Act
        var remaining = token.GetRemainingTime();

        // Assert
        remaining.Should().BeCloseTo(TimeSpan.FromDays(7), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GetRemainingTime_ForExpiredToken_ReturnsZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMilliseconds(10);
        var token = RefreshToken.Create(userId, expiresAt);

        // Act - Wait for expiration
        System.Threading.Thread.Sleep(15);
        var remaining = token.GetRemainingTime();

        // Assert
        remaining.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void UpdateDeviceInfo_UpdatesDeviceInfoAndIpAddress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var token = RefreshToken.Create(userId, expiresAt, "OldDevice", "192.168.1.1");

        // Act
        token.UpdateDeviceInfo("NewDevice", "192.168.1.2");

        // Assert
        token.DeviceInfo.Should().Be("NewDevice");
        token.IpAddress.Should().Be("192.168.1.2");
    }

    [Fact]
    public void Create_WithEmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var emptyUserId = Guid.Empty;
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var action = () => RefreshToken.Create(emptyUserId, expiresAt);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*User ID cannot be empty*");
    }

    [Fact]
    public void Create_WithPastExpirationDate_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var action = () => RefreshToken.Create(userId, pastDate);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Expiration date must be in the future*");
    }

    [Fact]
    public void Create_WithDeviceInfoAndIpAddress_StoresCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var deviceInfo = "iPhone 15 Pro";
        var ipAddress = "192.168.1.100";

        // Act
        var token = RefreshToken.Create(userId, expiresAt, deviceInfo, ipAddress);

        // Assert
        token.UserId.Should().Be(userId);
        token.DeviceInfo.Should().Be(deviceInfo);
        token.IpAddress.Should().Be(ipAddress);
    }
}
