using FluentAssertions;
using WahadiniCryptoQuest.Core.Entities;
using Xunit;

namespace WahadiniCryptoQuest.Core.Tests.Entities;

public class EmailVerificationTokenTests
{
    [Fact]
    public void Create_ShouldGenerateUniqueTokenAutomatically()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var token1 = EmailVerificationToken.Create(userId);
        var token2 = EmailVerificationToken.Create(userId);

        // Assert
        token1.Should().NotBeNull();
        token2.Should().NotBeNull();
        token1.Token.Should().NotBeEmpty();
        token2.Token.Should().NotBeEmpty();
        token1.Token.Should().NotBe(token2.Token);
        token1.Id.Should().NotBe(token2.Id);
    }

    [Fact]
    public void Create_ShouldSetExpirationTo24HoursByDefault()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedExpiration = DateTime.UtcNow.AddHours(24);

        // Act
        var token = EmailVerificationToken.Create(userId);

        // Assert
        token.ExpiresAt.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1));
        token.UserId.Should().Be(userId);
        token.IsUsed.Should().BeFalse();
        token.UsedAt.Should().BeNull();
        token.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void IsValid_ForUnusedNonExpiredToken_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = EmailVerificationToken.Create(userId);

        // Act
        var isValid = token.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_ForAlreadyUsedToken_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = EmailVerificationToken.Create(userId);
        token.MarkAsUsed();

        // Act
        var isValid = token.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenExpirationIsInFuture_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = EmailVerificationToken.Create(userId);

        // Act
        var isExpired = token.IsExpired();

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void MarkAsUsed_ShouldSetIsUsedToTrueAndUpdateTimestamp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = EmailVerificationToken.Create(userId);
        token.IsUsed.Should().BeFalse();
        token.UsedAt.Should().BeNull();

        // Act
        token.MarkAsUsed();

        // Assert
        token.IsUsed.Should().BeTrue();
        token.UsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkAsUsed_WhenAlreadyUsed_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = EmailVerificationToken.Create(userId);
        token.MarkAsUsed();

        // Act & Assert
        var act = () => token.MarkAsUsed();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Token has already been used");
    }

    [Theory]
    [InlineData(1)] // 1 hour
    [InlineData(12)] // 12 hours  
    [InlineData(48)] // 48 hours
    public void Create_WithCustomExpiration_ShouldSetCorrectExpirationTime(int hours)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedExpiration = DateTime.UtcNow.AddHours(hours);

        // Act
        var token = EmailVerificationToken.Create(userId, hours);

        // Assert
        token.ExpiresAt.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Create_WithInvalidUserId_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidUserId = Guid.Empty;

        // Act & Assert
        var act = () => EmailVerificationToken.Create(invalidUserId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("User ID cannot be empty*");
    }

    [Fact]
    public void CreateWithToken_ShouldCreateTokenWithSpecificValue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var specificToken = "my-specific-token-value";

        // Act
        var token = EmailVerificationToken.CreateWithToken(userId, specificToken);

        // Assert
        token.Token.Should().Be(specificToken);
        token.UserId.Should().Be(userId);
        token.IsUsed.Should().BeFalse();
    }

    [Fact]
    public void MatchesToken_WithCorrectToken_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenValue = "test-token-123";
        var token = EmailVerificationToken.CreateWithToken(userId, tokenValue);

        // Act
        var matches = token.MatchesToken(tokenValue);

        // Assert
        matches.Should().BeTrue();
    }

    [Fact]
    public void MatchesToken_WithIncorrectToken_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenValue = "test-token-123";
        var wrongToken = "wrong-token";
        var token = EmailVerificationToken.CreateWithToken(userId, tokenValue);

        // Act
        var matches = token.MatchesToken(wrongToken);

        // Assert
        matches.Should().BeFalse();
    }

    [Fact]
    public void GetTimeUntilExpiration_ForValidToken_ShouldReturnPositiveTimeSpan()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = EmailVerificationToken.Create(userId, 24);

        // Act
        var timeRemaining = token.GetTimeUntilExpiration();

        // Assert
        timeRemaining.Should().BeGreaterThan(TimeSpan.Zero);
        timeRemaining.Should().BeLessThan(TimeSpan.FromHours(24));
    }
}
