using FluentAssertions;
using WahadiniCryptoQuest.API.Configuration;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Configuration;

public class JwtSettingsTests
{
    [Fact]
    public void IsValid_WithValidConfiguration_ReturnsTrue()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ThisIsAVerySecureSecretKeyWith32+Characters",
            Issuer = "WahadiniCryptoQuest",
            Audience = "WahadiniCryptoQuestUsers",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        // Act
        var result = settings.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_WithNullOrEmptySecretKey_ReturnsFalse(string? secretKey)
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = secretKey ?? string.Empty,
            Issuer = "WahadiniCryptoQuest",
            Audience = "WahadiniCryptoQuestUsers",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        // Act
        var result = settings.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithShortSecretKey_ReturnsFalse()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ShortKey", // Less than 32 characters
            Issuer = "WahadiniCryptoQuest",
            Audience = "WahadiniCryptoQuestUsers",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        // Act
        var result = settings.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_WithNullOrEmptyIssuer_ReturnsFalse(string? issuer)
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ThisIsAVerySecureSecretKeyWith32+Characters",
            Issuer = issuer ?? string.Empty,
            Audience = "WahadiniCryptoQuestUsers",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        // Act
        var result = settings.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_WithNullOrEmptyAudience_ReturnsFalse(string? audience)
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ThisIsAVerySecureSecretKeyWith32+Characters",
            Issuer = "WahadiniCryptoQuest",
            Audience = audience ?? string.Empty,
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        // Act
        var result = settings.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void IsValid_WithInvalidAccessTokenExpiration_ReturnsFalse(int expirationMinutes)
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ThisIsAVerySecureSecretKeyWith32+Characters",
            Issuer = "WahadiniCryptoQuest",
            Audience = "WahadiniCryptoQuestUsers",
            AccessTokenExpirationMinutes = expirationMinutes,
            RefreshTokenExpirationDays = 7
        };

        // Act
        var result = settings.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void IsValid_WithInvalidRefreshTokenExpiration_ReturnsFalse(int expirationDays)
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ThisIsAVerySecureSecretKeyWith32+Characters",
            Issuer = "WahadiniCryptoQuest",
            Audience = "WahadiniCryptoQuestUsers",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = expirationDays
        };

        // Act
        var result = settings.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetValidationErrors_WithValidConfiguration_ReturnsEmptyList()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ThisIsAVerySecureSecretKeyWith32+Characters",
            Issuer = "WahadiniCryptoQuest",
            Audience = "WahadiniCryptoQuestUsers",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        // Act
        var errors = settings.GetValidationErrors();

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void GetValidationErrors_WithNullSecretKey_ReturnsSecretKeyRequiredError()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = string.Empty,
            Issuer = "WahadiniCryptoQuest",
            Audience = "WahadiniCryptoQuestUsers",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        // Act
        var errors = settings.GetValidationErrors().ToList();

        // Assert
        errors.Should().Contain("JWT SecretKey is required");
    }

    [Fact]
    public void GetValidationErrors_WithShortSecretKey_ReturnsSecretKeyLengthError()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ShortKey",
            Issuer = "WahadiniCryptoQuest",
            Audience = "WahadiniCryptoQuestUsers",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        // Act
        var errors = settings.GetValidationErrors().ToList();

        // Assert
        errors.Should().Contain("JWT SecretKey must be at least 32 characters long");
    }

    [Fact]
    public void GetValidationErrors_WithNullIssuer_ReturnsIssuerRequiredError()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ThisIsAVerySecureSecretKeyWith32+Characters",
            Issuer = string.Empty,
            Audience = "WahadiniCryptoQuestUsers",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        // Act
        var errors = settings.GetValidationErrors().ToList();

        // Assert
        errors.Should().Contain("JWT Issuer is required");
    }

    [Fact]
    public void GetValidationErrors_WithNullAudience_ReturnsAudienceRequiredError()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ThisIsAVerySecureSecretKeyWith32+Characters",
            Issuer = "WahadiniCryptoQuest",
            Audience = string.Empty,
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        // Act
        var errors = settings.GetValidationErrors().ToList();

        // Assert
        errors.Should().Contain("JWT Audience is required");
    }

    [Fact]
    public void GetValidationErrors_WithInvalidAccessTokenExpiration_ReturnsExpirationError()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ThisIsAVerySecureSecretKeyWith32+Characters",
            Issuer = "WahadiniCryptoQuest",
            Audience = "WahadiniCryptoQuestUsers",
            AccessTokenExpirationMinutes = 0,
            RefreshTokenExpirationDays = 7
        };

        // Act
        var errors = settings.GetValidationErrors().ToList();

        // Assert
        errors.Should().Contain("JWT AccessTokenExpirationMinutes must be greater than 0");
    }

    [Fact]
    public void GetValidationErrors_WithInvalidRefreshTokenExpiration_ReturnsExpirationError()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ThisIsAVerySecureSecretKeyWith32+Characters",
            Issuer = "WahadiniCryptoQuest",
            Audience = "WahadiniCryptoQuestUsers",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 0
        };

        // Act
        var errors = settings.GetValidationErrors().ToList();

        // Assert
        errors.Should().Contain("JWT RefreshTokenExpirationDays must be greater than 0");
    }

    [Fact]
    public void GetValidationErrors_WithAllInvalidFields_ReturnsAllErrors()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = string.Empty,
            Issuer = string.Empty,
            Audience = string.Empty,
            AccessTokenExpirationMinutes = 0,
            RefreshTokenExpirationDays = 0
        };

        // Act
        var errors = settings.GetValidationErrors().ToList();

        // Assert
        errors.Should().HaveCount(5);
        errors.Should().Contain("JWT SecretKey is required");
        errors.Should().Contain("JWT Issuer is required");
        errors.Should().Contain("JWT Audience is required");
        errors.Should().Contain("JWT AccessTokenExpirationMinutes must be greater than 0");
        errors.Should().Contain("JWT RefreshTokenExpirationDays must be greater than 0");
    }

    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var settings = new JwtSettings();

        // Assert
        settings.SecretKey.Should().BeEmpty();
        settings.Issuer.Should().BeEmpty();
        settings.Audience.Should().BeEmpty();
        settings.AccessTokenExpirationMinutes.Should().Be(15);
        settings.RefreshTokenExpirationDays.Should().Be(7);
        settings.ValidateIssuer.Should().BeTrue();
        settings.ValidateAudience.Should().BeTrue();
        settings.ValidateLifetime.Should().BeTrue();
        settings.ValidateIssuerSigningKey.Should().BeTrue();
        settings.ClockSkewMinutes.Should().Be(0);
    }

    [Fact]
    public void SectionName_IsCorrectlyDefined()
    {
        // Assert
        JwtSettings.SectionName.Should().Be("JwtSettings");
    }

    [Fact]
    public void IsValid_WithMinimumValidSecretKeyLength_ReturnsTrue()
    {
        // Arrange - Exactly 32 characters
        var settings = new JwtSettings
        {
            SecretKey = "12345678901234567890123456789012",
            Issuer = "WahadiniCryptoQuest",
            Audience = "WahadiniCryptoQuestUsers",
            AccessTokenExpirationMinutes = 1,
            RefreshTokenExpirationDays = 1
        };

        // Act
        var result = settings.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithSecretKeyOneCharacterTooShort_ReturnsFalse()
    {
        // Arrange - 31 characters
        var settings = new JwtSettings
        {
            SecretKey = "1234567890123456789012345678901",
            Issuer = "WahadiniCryptoQuest",
            Audience = "WahadiniCryptoQuestUsers",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        // Act
        var result = settings.IsValid();

        // Assert
        result.Should().BeFalse();
    }
}
