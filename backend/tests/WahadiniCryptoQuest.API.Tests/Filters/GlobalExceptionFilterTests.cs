using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using WahadiniCryptoQuest.API.Filters;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Filters;

/// <summary>
/// Tests for GlobalExceptionFilter
/// Coverage target: 100% line, 90%+ branch
/// </summary>
public class GlobalExceptionFilterTests
{
    private readonly Mock<ILogger<GlobalExceptionFilter>> _mockLogger;
    private readonly GlobalExceptionFilter _filter;

    public GlobalExceptionFilterTests()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionFilter>>();
        _filter = new GlobalExceptionFilter(_mockLogger.Object);
    }

    #region OnException Tests

    [Fact]
    public void OnException_WithGenericException_ShouldReturn500StatusCode()
    {
        // Arrange
        var exception = new Exception("Test exception message");
        var context = CreateExceptionContext(exception, "/api/test");

        // Act
        _filter.OnException(context);

        // Assert
        var result = context.Result as ObjectResult;
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(500);
    }

    [Fact]
    public void OnException_ShouldSetExceptionHandledToTrue()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var context = CreateExceptionContext(exception, "/api/test");

        // Act
        _filter.OnException(context);

        // Assert
        context.ExceptionHandled.Should().BeTrue();
    }

    [Fact]
    public void OnException_ShouldLogErrorWithExceptionDetails()
    {
        // Arrange
        var exception = new Exception("Test exception message");
        var context = CreateExceptionContext(exception, "/api/test/path");

        // Act
        _filter.OnException(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unhandled exception occurred")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void OnException_ShouldIncludeErrorMessageInResponse()
    {
        // Arrange
        var exceptionMessage = "Specific error message";
        var exception = new Exception(exceptionMessage);
        var context = CreateExceptionContext(exception, "/api/test");

        // Act
        _filter.OnException(context);

        // Assert
        var result = context.Result as ObjectResult;
        result.Should().NotBeNull();
        
        var responseValue = result!.Value;
        responseValue.Should().NotBeNull();
        
        var errorProperty = responseValue!.GetType().GetProperty("error");
        errorProperty.Should().NotBeNull();
        errorProperty!.GetValue(responseValue).Should().Be("An unexpected error occurred");
        
        var messageProperty = responseValue.GetType().GetProperty("message");
        messageProperty.Should().NotBeNull();
        messageProperty!.GetValue(responseValue).Should().Be(exceptionMessage);
    }

    [Fact]
    public void OnException_ShouldIncludeRequestPathInResponse()
    {
        // Arrange
        var requestPath = "/api/users/123";
        var exception = new Exception("Test exception");
        var context = CreateExceptionContext(exception, requestPath);

        // Act
        _filter.OnException(context);

        // Assert
        var result = context.Result as ObjectResult;
        result.Should().NotBeNull();
        
        var responseValue = result!.Value;
        var pathProperty = responseValue!.GetType().GetProperty("path");
        pathProperty.Should().NotBeNull();
        pathProperty!.GetValue(responseValue).Should().Be(requestPath);
    }

    [Fact]
    public void OnException_WithDifferentExceptionTypes_ShouldHandleAll()
    {
        // Arrange & Act & Assert for ArgumentException
        var argException = new ArgumentException("Invalid argument");
        var context1 = CreateExceptionContext(argException, "/api/test1");
        _filter.OnException(context1);
        context1.ExceptionHandled.Should().BeTrue();
        (context1.Result as ObjectResult)?.StatusCode.Should().Be(500);

        // Act & Assert for InvalidOperationException
        var invalidOpException = new InvalidOperationException("Invalid operation");
        var context2 = CreateExceptionContext(invalidOpException, "/api/test2");
        _filter.OnException(context2);
        context2.ExceptionHandled.Should().BeTrue();
        (context2.Result as ObjectResult)?.StatusCode.Should().Be(500);

        // Act & Assert for NullReferenceException
        var nullRefException = new NullReferenceException("Null reference");
        var context3 = CreateExceptionContext(nullRefException, "/api/test3");
        _filter.OnException(context3);
        context3.ExceptionHandled.Should().BeTrue();
        (context3.Result as ObjectResult)?.StatusCode.Should().Be(500);
    }

    [Fact]
    public void OnException_WithEmptyExceptionMessage_ShouldHandleGracefully()
    {
        // Arrange
        var exception = new Exception(string.Empty);
        var context = CreateExceptionContext(exception, "/api/test");

        // Act
        _filter.OnException(context);

        // Assert
        var result = context.Result as ObjectResult;
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(500);
        context.ExceptionHandled.Should().BeTrue();
    }

    [Fact]
    public void OnException_WithNullExceptionMessage_ShouldHandleGracefully()
    {
        // Arrange
        var exception = new Exception(null);
        var context = CreateExceptionContext(exception, "/api/test");

        // Act
        _filter.OnException(context);

        // Assert
        var result = context.Result as ObjectResult;
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(500);
        context.ExceptionHandled.Should().BeTrue();
    }

    [Fact]
    public void OnException_WithLongExceptionMessage_ShouldIncludeFullMessage()
    {
        // Arrange
        var longMessage = new string('A', 1000) + " - Test exception with very long message";
        var exception = new Exception(longMessage);
        var context = CreateExceptionContext(exception, "/api/test");

        // Act
        _filter.OnException(context);

        // Assert
        var result = context.Result as ObjectResult;
        var responseValue = result!.Value;
        var messageProperty = responseValue!.GetType().GetProperty("message");
        messageProperty!.GetValue(responseValue).Should().Be(longMessage);
    }

    [Fact]
    public void OnException_WithNestedExceptions_ShouldLogOuterException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner exception");
        var outerException = new Exception("Outer exception", innerException);
        var context = CreateExceptionContext(outerException, "/api/test");

        // Act
        _filter.OnException(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unhandled exception occurred")),
                outerException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        
        var result = context.Result as ObjectResult;
        var responseValue = result!.Value;
        var messageProperty = responseValue!.GetType().GetProperty("message");
        messageProperty!.GetValue(responseValue).Should().Be("Outer exception");
    }

    [Fact]
    public void OnException_WithSpecialCharactersInPath_ShouldHandleCorrectly()
    {
        // Arrange
        // Note: PathString encodes special characters. Paths should not contain query strings (use QueryString for that)
        var specialPath = "/api/test-with-special-chars_123";
        var exception = new Exception("Test exception");
        var context = CreateExceptionContext(exception, specialPath);

        // Act
        _filter.OnException(context);

        // Assert
        var result = context.Result as ObjectResult;
        var responseValue = result!.Value;
        var pathProperty = responseValue!.GetType().GetProperty("path");
        pathProperty!.GetValue(responseValue).Should().Be(specialPath);
    }

    [Fact]
    public void OnException_MultipleCallsWithDifferentExceptions_ShouldHandleEach()
    {
        // Arrange
        var exception1 = new Exception("First exception");
        var exception2 = new Exception("Second exception");
        var context1 = CreateExceptionContext(exception1, "/api/test1");
        var context2 = CreateExceptionContext(exception2, "/api/test2");

        // Act
        _filter.OnException(context1);
        _filter.OnException(context2);

        // Assert
        context1.ExceptionHandled.Should().BeTrue();
        context2.ExceptionHandled.Should().BeTrue();
        
        var result1 = context1.Result as ObjectResult;
        var result2 = context2.Result as ObjectResult;
        
        result1!.StatusCode.Should().Be(500);
        result2!.StatusCode.Should().Be(500);
    }

    [Fact]
    public void OnException_ShouldCreateObjectResultWithCorrectStructure()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var context = CreateExceptionContext(exception, "/api/test");

        // Act
        _filter.OnException(context);

        // Assert
        var result = context.Result as ObjectResult;
        result.Should().NotBeNull();
        
        var responseValue = result!.Value;
        responseValue.Should().NotBeNull();
        
        // Verify all three properties exist
        var type = responseValue!.GetType();
        type.GetProperty("error").Should().NotBeNull();
        type.GetProperty("message").Should().NotBeNull();
        type.GetProperty("path").Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    private ExceptionContext CreateExceptionContext(Exception exception, string requestPath)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = requestPath;

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor()
        );

        return new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = exception
        };
    }

    #endregion
}
