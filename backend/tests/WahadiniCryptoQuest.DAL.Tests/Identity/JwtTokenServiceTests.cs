using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WahadiniCryptoQuest.Core.Settings;
using WahadiniCryptoQuest.DAL.Identity;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Identity;

/// <summary>
/// Unit tests for JWT token service
/// Tests JWT token generation and validation logic in isolation
/// </summary>
public class JwtTokenServiceTests
{
    private readonly JwtTokenService _jwtTokenService;
    private readonly JwtSettings _jwtSettings;

    public JwtTokenServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = "ThisIsATestSecretKeyThatIsLongEnoughForHMACSHA256Signing",
            Issuer = "WahadiniCryptoQuest",
            Audience = "WahadiniCryptoQuestUsers",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };
        
        var options = Options.Create(_jwtSettings);
        _jwtTokenService = new JwtTokenService(options);
    }

    [Fact]
    public async Task GenerateAccessTokenAsync_WithValidUserData_CreatesValidJWTWithCorrectClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var username = "testuser";
        var roles = new[] { "Free" };

        // Act
        var token = await _jwtTokenService.GenerateAccessTokenAsync(userId, email, username, roles);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        
        // Verify required claims using the actual claim names from JWT token
        jsonToken.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == userId.ToString());
        jsonToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == email);
        jsonToken.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == username);
        jsonToken.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Free");
    }

    [Fact]
    public async Task GenerateAccessTokenAsync_CreatesTokenWithCorrectExpiration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var username = "testuser";
        var roles = new[] { "Free" };

        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = await _jwtTokenService.GenerateAccessTokenAsync(userId, email, username, roles);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        
        // Should expire in 15 minutes (+/- 1 minute tolerance)
        var expectedExpiry = beforeGeneration.AddMinutes(15);
        jsonToken.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_CreatesUniqueSecureToken()
    {
        // Act
        var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
        refreshToken.Length.Should().BeGreaterOrEqualTo(32); // Should be reasonably long
        
        // Generate another token to ensure uniqueness
        var secondToken = await _jwtTokenService.GenerateRefreshTokenAsync();
        refreshToken.Should().NotBe(secondToken);
    }

    [Fact]
    public async Task ValidateAccessToken_WithValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var username = "testuser";
        var roles = new[] { "Free" };

        var token = await _jwtTokenService.GenerateAccessTokenAsync(userId, email, username, roles);

        // Act
        var principal = await _jwtTokenService.ValidateAccessTokenAsync(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(userId.ToString());
        principal.FindFirst(ClaimTypes.Email)?.Value.Should().Be(email);
    }

    [Fact]
    public async Task ValidateAccessToken_WithInvalidSignature_ReturnsNull()
    {
        // Arrange
        var invalidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.invalid_signature";

        // Act
        var principal = await _jwtTokenService.ValidateAccessTokenAsync(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAccessToken_WithMalformedToken_ReturnsNull()
    {
        // Arrange
        var malformedToken = "this-is-not-a-jwt-token";

        // Act
        var principal = await _jwtTokenService.ValidateAccessTokenAsync(malformedToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public async Task GetPrincipalFromToken_WithValidToken_ExtractsCorrectClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var username = "testuser";
        var roles = new[] { "Premium" };

        var token = await _jwtTokenService.GenerateAccessTokenAsync(userId, email, username, roles);

        // Act
        var principal = await _jwtTokenService.GetPrincipalFromTokenAsync(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(userId.ToString());
        principal.FindFirst(ClaimTypes.Email)?.Value.Should().Be(email);
        principal.FindFirst(ClaimTypes.Role)?.Value.Should().Be("Premium");
    }

    [Fact]
    public async Task GetPrincipalFromToken_WithMalformedToken_ReturnsNull()
    {
        // Arrange
        var malformedToken = "invalid-token";

        // Act
        var principal = await _jwtTokenService.GetPrincipalFromTokenAsync(malformedToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public async Task GetTokenRemainingLifetime_WithValidToken_ReturnsCorrectTimespan()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = await _jwtTokenService.GenerateAccessTokenAsync(userId, "test@example.com", "testuser", new[] { "Free" });

        // Act
        var remainingTime = _jwtTokenService.GetTokenRemainingLifetime(token);

        // Assert
        remainingTime.Should().NotBeNull();
        remainingTime!.Value.Should().BeGreaterThan(TimeSpan.FromMinutes(14)); // Should be close to 15 minutes
        remainingTime.Value.Should().BeLessThanOrEqualTo(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void GetTokenRemainingLifetime_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid-token";

        // Act
        var remainingTime = _jwtTokenService.GetTokenRemainingLifetime(invalidToken);

        // Assert
        remainingTime.Should().BeNull();
    }
}
