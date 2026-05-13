using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using WahadiniCryptoQuest.API.Middleware;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Middleware;

/// <summary>
/// Unit tests for JWT middleware functionality
/// Tests token validation, user context setting, and error handling
/// </summary>
public class JwtMiddlewareUnitTests
{
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly Mock<ILogger<JwtMiddleware>> _mockLogger;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly JwtMiddleware _middleware;
    private readonly DefaultHttpContext _httpContext;

    public JwtMiddlewareUnitTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockLogger = new Mock<ILogger<JwtMiddleware>>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _middleware = new JwtMiddleware(_mockNext.Object, _mockLogger.Object);
        _httpContext = new DefaultHttpContext();
    }

    [Fact]
    public async Task InvokeAsync_WithValidToken_SetsUserContext()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-jwt-token";
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Name, "testuser")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        
        _mockJwtTokenService
            .Setup(x => x.ValidateAccessTokenAsync(token))
            .ReturnsAsync(principal);

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockJwtTokenService.Object);

        // Assert
        _httpContext.User.Should().NotBeNull();
        _httpContext.User.Identity.Should().NotBeNull();
        _httpContext.User.Identity!.IsAuthenticated.Should().BeTrue();
        _httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(userId.ToString());
        _mockNext.Verify(x => x.Invoke(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidToken_DoesNotSetUserContext()
    {
        // Arrange
        var token = "invalid-jwt-token";
        _httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        
        _mockJwtTokenService
            .Setup(x => x.ValidateAccessTokenAsync(token))
            .ReturnsAsync((ClaimsPrincipal?)null);

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockJwtTokenService.Object);

        // Assert
        _httpContext.User.Identity?.IsAuthenticated.Should().BeFalse();
        _mockNext.Verify(x => x.Invoke(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithExpiredToken_DoesNotSetUserContext()
    {
        // Arrange
        var token = "expired-jwt-token";
        _httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        
        _mockJwtTokenService
            .Setup(x => x.ValidateAccessTokenAsync(token))
            .ThrowsAsync(new Exception("Token expired"));

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockJwtTokenService.Object);

        // Assert
        _httpContext.User.Identity?.IsAuthenticated.Should().BeFalse();
        _mockNext.Verify(x => x.Invoke(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithMissingToken_CallsNextMiddleware()
    {
        // Arrange
        // No Authorization header set
        
        // Act
        await _middleware.InvokeAsync(_httpContext, _mockJwtTokenService.Object);

        // Assert
        _httpContext.User.Identity?.IsAuthenticated.Should().BeFalse();
        _mockJwtTokenService.Verify(x => x.ValidateAccessTokenAsync(It.IsAny<string>()), Times.Never);
        _mockNext.Verify(x => x.Invoke(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithMalformedAuthorizationHeader_DoesNotProcessToken()
    {
        // Arrange
        _httpContext.Request.Headers["Authorization"] = "NotBearer token-value";
        
        // Act
        await _middleware.InvokeAsync(_httpContext, _mockJwtTokenService.Object);

        // Assert
        _httpContext.User.Identity?.IsAuthenticated.Should().BeFalse();
        _mockJwtTokenService.Verify(x => x.ValidateAccessTokenAsync(It.IsAny<string>()), Times.Never);
        _mockNext.Verify(x => x.Invoke(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyBearerToken_DoesNotProcessToken()
    {
        // Arrange
        _httpContext.Request.Headers["Authorization"] = "Bearer ";
        
        // Act
        await _middleware.InvokeAsync(_httpContext, _mockJwtTokenService.Object);

        // Assert
        _httpContext.User.Identity?.IsAuthenticated.Should().BeFalse();
        _mockJwtTokenService.Verify(x => x.ValidateAccessTokenAsync(It.IsAny<string>()), Times.Never);
        _mockNext.Verify(x => x.Invoke(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithAllowAnonymousEndpoint_SkipsTokenValidation()
    {
        // Arrange
        var token = "any-token";
        _httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        
        // Set up endpoint with AllowAnonymous metadata
        var endpoint = new Endpoint(
            requestDelegate: (context) => Task.CompletedTask,
            metadata: new EndpointMetadataCollection(new AllowAnonymousAttribute()),
            displayName: "TestEndpoint"
        );
        _httpContext.SetEndpoint(endpoint);

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockJwtTokenService.Object);

        // Assert
        _mockJwtTokenService.Verify(x => x.ValidateAccessTokenAsync(It.IsAny<string>()), Times.Never);
        _mockNext.Verify(x => x.Invoke(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithCaseInsensitiveBearerToken_ProcessesToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-jwt-token";
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _httpContext.Request.Headers["Authorization"] = $"bearer {token}"; // lowercase
        
        _mockJwtTokenService
            .Setup(x => x.ValidateAccessTokenAsync(token))
            .ReturnsAsync(principal);

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockJwtTokenService.Object);

        // Assert
        _httpContext.User.Identity!.IsAuthenticated.Should().BeTrue();
        _mockJwtTokenService.Verify(x => x.ValidateAccessTokenAsync(token), Times.Once);
        _mockNext.Verify(x => x.Invoke(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithWhitespaceInToken_TrimsToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-jwt-token";
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _httpContext.Request.Headers["Authorization"] = $"Bearer {token}   "; // trailing spaces
        
        _mockJwtTokenService
            .Setup(x => x.ValidateAccessTokenAsync(token))
            .ReturnsAsync(principal);

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockJwtTokenService.Object);

        // Assert
        _httpContext.User.Identity!.IsAuthenticated.Should().BeTrue();
        _mockJwtTokenService.Verify(x => x.ValidateAccessTokenAsync(token), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithTokenValidationException_LogsWarningAndContinues()
    {
        // Arrange
        var token = "problematic-token";
        _httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        
        _mockJwtTokenService
            .Setup(x => x.ValidateAccessTokenAsync(token))
            .ThrowsAsync(new Exception("Token validation failed"));

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockJwtTokenService.Object);

        // Assert
        _httpContext.User.Identity?.IsAuthenticated.Should().BeFalse();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error validating JWT token")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _mockNext.Verify(x => x.Invoke(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithMultipleClaimsInToken_ExtractsAllClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-jwt-token";
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim("Permission", "read:content"),
            new Claim("Permission", "write:content")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        
        _mockJwtTokenService
            .Setup(x => x.ValidateAccessTokenAsync(token))
            .ReturnsAsync(principal);

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockJwtTokenService.Object);

        // Assert
        _httpContext.User.Should().NotBeNull();
        _httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(userId.ToString());
        _httpContext.User.FindFirst(ClaimTypes.Email)?.Value.Should().Be("test@example.com");
        _httpContext.User.FindFirst(ClaimTypes.Name)?.Value.Should().Be("testuser");
        _httpContext.User.FindAll(ClaimTypes.Role).Should().HaveCount(2);
        _httpContext.User.FindAll("Permission").Should().HaveCount(2);
    }

    [Fact]
    public async Task InvokeAsync_LogsDebugMessage_WhenNoTokenProvided()
    {
        // Arrange
        // No Authorization header
        
        // Act
        await _middleware.InvokeAsync(_httpContext, _mockJwtTokenService.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No authorization token provided")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_LogsDebugMessage_OnSuccessfulValidation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-jwt-token";
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        
        _mockJwtTokenService
            .Setup(x => x.ValidateAccessTokenAsync(token))
            .ReturnsAsync(principal);

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockJwtTokenService.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("JWT token validated successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_LogsDebugMessage_OnInvalidToken()
    {
        // Arrange
        var token = "invalid-token";
        _httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        
        _mockJwtTokenService
            .Setup(x => x.ValidateAccessTokenAsync(token))
            .ReturnsAsync((ClaimsPrincipal?)null);

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockJwtTokenService.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid JWT token provided")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithNullEndpoint_ProcessesTokenNormally()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-jwt-token";
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        _httpContext.SetEndpoint(null); // Explicitly set null endpoint
        
        _mockJwtTokenService
            .Setup(x => x.ValidateAccessTokenAsync(token))
            .ReturnsAsync(principal);

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockJwtTokenService.Object);

        // Assert
        _httpContext.User.Identity!.IsAuthenticated.Should().BeTrue();
        _mockJwtTokenService.Verify(x => x.ValidateAccessTokenAsync(token), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithEndpointWithoutMetadata_ProcessesToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-jwt-token";
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        
        // Endpoint without any metadata
        var endpoint = new Endpoint(
            requestDelegate: (context) => Task.CompletedTask,
            metadata: new EndpointMetadataCollection(),
            displayName: "TestEndpoint"
        );
        _httpContext.SetEndpoint(endpoint);
        
        _mockJwtTokenService
            .Setup(x => x.ValidateAccessTokenAsync(token))
            .ReturnsAsync(principal);

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockJwtTokenService.Object);

        // Assert
        _httpContext.User.Identity!.IsAuthenticated.Should().BeTrue();
        _mockJwtTokenService.Verify(x => x.ValidateAccessTokenAsync(token), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithUnauthenticatedPrincipal_DoesNotSetUserContext()
    {
        // Arrange
        var token = "token-with-unauthenticated-identity";
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) };
        var identity = new ClaimsIdentity(claims); // No authentication type - IsAuthenticated will be false
        var principal = new ClaimsPrincipal(identity);

        _httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        
        _mockJwtTokenService
            .Setup(x => x.ValidateAccessTokenAsync(token))
            .ReturnsAsync(principal);

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockJwtTokenService.Object);

        // Assert
        // User context should not be set for unauthenticated principals
        _httpContext.User.Identity?.IsAuthenticated.Should().BeFalse();
        _mockNext.Verify(x => x.Invoke(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithPrincipalWithNullIdentity_DoesNotSetUserContext()
    {
        // Arrange
        var token = "token-with-null-identity";
        var principal = new ClaimsPrincipal(); // No identity

        _httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        
        _mockJwtTokenService
            .Setup(x => x.ValidateAccessTokenAsync(token))
            .ReturnsAsync(principal);

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockJwtTokenService.Object);

        // Assert
        _httpContext.User.Identity?.IsAuthenticated.Should().BeFalse();
        _mockNext.Verify(x => x.Invoke(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithTopLevelException_LogsWarningAndContinues()
    {
        // Arrange
        var token = "valid-token";
        _httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        
        _mockJwtTokenService
            .Setup(x => x.ValidateAccessTokenAsync(token))
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        await _middleware.InvokeAsync(_httpContext, _mockJwtTokenService.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error validating JWT token")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _mockNext.Verify(x => x.Invoke(_httpContext), Times.Once);
    }
}
