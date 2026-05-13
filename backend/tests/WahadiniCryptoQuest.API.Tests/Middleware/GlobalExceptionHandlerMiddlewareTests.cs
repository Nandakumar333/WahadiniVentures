using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;
using WahadiniCryptoQuest.API.Middleware;
using WahadiniCryptoQuest.Core.Exceptions;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Middleware;

public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _mockLogger;
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    
    public GlobalExceptionHandlerMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        _mockNext = new Mock<RequestDelegate>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(env => env.EnvironmentName).Returns(Environments.Production);
    }
    
    private GlobalExceptionHandlerMiddleware CreateMiddleware()
    {
        return new GlobalExceptionHandlerMiddleware(_mockNext.Object, _mockLogger.Object);
    }
    
    private DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        // Setup service provider with mocked IWebHostEnvironment
        var services = new ServiceCollection();
        services.AddSingleton(_mockEnvironment.Object);
        context.RequestServices = services.BuildServiceProvider();
        
        return context;
    }
    
    private async Task<ErrorResponse> GetResponseFromContext(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }
    
    private class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
    }

    [Fact]
    public async Task InvokeAsync_WithNoException_CallsNextMiddleware()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task InvokeAsync_WithFluentValidationException_Returns400BadRequest()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Email", "Email is required"),
            new ValidationFailure("Password", "Password must be at least 8 characters")
        };
        var exception = new ValidationException(failures);
        
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var response = await GetResponseFromContext(context);
        response.Message.Should().Contain("Validation failed");
    }

    [Fact]
    public async Task InvokeAsync_WithEntityNotFoundException_Returns404NotFound()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        var exception = new EntityNotFoundException("User", "test-user-id");
        
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        var response = await GetResponseFromContext(context);
        response.Message.Should().Contain("User");
        response.Message.Should().Contain("test-user-id");
        response.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task InvokeAsync_WithUnauthorizedAccessException_Returns401Unauthorized()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        var exception = new UnauthorizedAccessException("Invalid credentials");
        
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        var response = await GetResponseFromContext(context);
        response.Message.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task InvokeAsync_WithDuplicateEntityException_Returns409Conflict()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        var exception = new DuplicateEntityException("User", "test@example.com");
        
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
        var response = await GetResponseFromContext(context);
        response.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task InvokeAsync_WithBusinessRuleValidationException_Returns400BadRequest()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        var exception = new BusinessRuleValidationException("Account is locked due to too many failed login attempts");
        
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var response = await GetResponseFromContext(context);
        response.Message.Should().Contain("Account is locked");
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidOperationException_Returns400BadRequest()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Email verification token has expired");
        
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var response = await GetResponseFromContext(context);
        response.Message.Should().Contain("Email verification token");
    }

    [Fact]
    public async Task InvokeAsync_WithGenericException_Returns500InternalServerError()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        var exception = new Exception("Database connection failed");
        
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        var response = await GetResponseFromContext(context);
        response.Message.Should().Contain("Database connection failed");
    }

    [Fact]
    public async Task InvokeAsync_LogsErrorForInternalServerErrors()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        var exception = new Exception("Test exception");
        
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Test exception")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_LogsWarningForClientErrors()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        var exception = new BusinessRuleValidationException("Client error");
        
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Client error")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_SetsCorrectContentType()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        var exception = new BusinessRuleValidationException("Test exception");
        
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task InvokeAsync_DoesNotExposeStackTraceInProduction()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        // Environment is already set to Production in constructor
        var exception = new Exception("Internal error with sensitive stack trace");
        
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await GetResponseFromContext(context);
        response.Details.Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_ExposesStackTraceInDevelopment()
    {
        // Arrange
        _mockEnvironment.Setup(env => env.EnvironmentName).Returns(Environments.Development);
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        var exception = new Exception("Test error");
        
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await GetResponseFromContext(context);
        response.Details.Should().NotBeNull();
    }

    [Fact]
    public async Task InvokeAsync_HandlesMultipleExceptionTypesWithCorrectStatusCodes()
    {
        // Arrange
        var testCases = new[]
        {
            (Exception: (Exception)new ValidationException("Validation error"), ExpectedStatus: HttpStatusCode.BadRequest),
            (Exception: (Exception)new EntityNotFoundException("User", "id"), ExpectedStatus: HttpStatusCode.NotFound),
            (Exception: (Exception)new UnauthorizedAccessException("Unauthorized"), ExpectedStatus: HttpStatusCode.Unauthorized),
            (Exception: (Exception)new DuplicateEntityException("User", "test@example.com"), ExpectedStatus: HttpStatusCode.Conflict),
            (Exception: (Exception)new BusinessRuleValidationException("Business rule violated"), ExpectedStatus: HttpStatusCode.BadRequest),
            (Exception: (Exception)new InvalidOperationException("Invalid operation"), ExpectedStatus: HttpStatusCode.BadRequest),
            (Exception: (Exception)new Exception("Generic error"), ExpectedStatus: HttpStatusCode.InternalServerError)
        };

        foreach (var testCase in testCases)
        {
            // Arrange
            var middleware = CreateMiddleware();
            var context = CreateHttpContext();
            _mockNext.Setup(next => next(It.IsAny<HttpContext>())).ThrowsAsync(testCase.Exception);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)testCase.ExpectedStatus, 
                $"Exception type {testCase.Exception.GetType().Name} should return {testCase.ExpectedStatus}");
        }
    }

    [Fact]
    public async Task InvokeAsync_LogsErrorWithAuthenticatedUser_WhenInternalServerError()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        
        // Set up authenticated user
        var identity = new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "test@example.com")
        }, "TestAuthType");
        context.User = new System.Security.Claims.ClaimsPrincipal(identity);
        
        var exception = new Exception("Critical database error");
        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Critical database error")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_LogsErrorWithAnonymousUser_WhenNotAuthenticated()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        // User is not set (anonymous)
        
        var exception = new Exception("Critical error for anonymous user");
        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Critical error for anonymous user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_LogsRequestDetailsForServerErrors()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        context.Request.Path = "/api/users/123";
        context.Request.Method = "DELETE";
        
        var exception = new Exception("Failed to delete user");
        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to delete user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_LogsRequestDetailsForClientErrors()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        context.Request.Path = "/api/auth/login";
        context.Request.Method = "POST";
        
        var exception = new BusinessRuleValidationException("Invalid email format");
        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid email format")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
