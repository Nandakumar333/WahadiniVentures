using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using WahadiniCryptoQuest.API.Filters;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Filters;

/// <summary>
/// Tests for ValidateModelStateFilter
/// Coverage target: 100% line, 90%+ branch
/// </summary>
public class ValidateModelStateFilterTests
{
    private readonly ValidateModelStateFilter _filter;

    public ValidateModelStateFilterTests()
    {
        _filter = new ValidateModelStateFilter();
    }

    #region OnActionExecuting Tests

    [Fact]
    public void OnActionExecuting_WithValidModelState_ShouldNotSetResult()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        // ModelState is valid by default

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeNull();
    }

    [Fact]
    public void OnActionExecuting_WithInvalidModelState_ShouldSetBadRequestResult()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("TestField", "Test error message");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().NotBeNull();
        context.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void OnActionExecuting_WithInvalidModelState_ShouldReturn400StatusCode()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Email", "Email is required");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var result = context.Result as BadRequestObjectResult;
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
    }

    [Fact]
    public void OnActionExecuting_WithInvalidModelState_ShouldIncludeModelStateInResponse()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Email", "Email is required");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var result = context.Result as BadRequestObjectResult;
        result.Should().NotBeNull();
        // BadRequestObjectResult automatically wraps ModelStateDictionary in SerializableError
        result!.Value.Should().BeOfType<SerializableError>();

        var errors = result.Value as SerializableError;
        errors.Should().NotBeNull();
        errors!.Should().ContainKey("Email");
    }

    [Fact]
    public void OnActionExecuting_WithMultipleModelStateErrors_ShouldIncludeAll()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Email", "Email is required");
        context.ModelState.AddModelError("Password", "Password must be at least 8 characters");
        context.ModelState.AddModelError("FirstName", "First name is required");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var result = context.Result as BadRequestObjectResult;
        var errors = result!.Value as SerializableError;

        errors.Should().NotBeNull();
        errors!.Should().ContainKey("Email");
        errors.Should().ContainKey("Password");
        errors.Should().ContainKey("FirstName");
        errors?.Count.Should().Be(3);
    }

    [Fact]
    public void OnActionExecuting_WithMultipleErrorsOnSameField_ShouldIncludeAll()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Email", "Email is required");
        context.ModelState.AddModelError("Email", "Email format is invalid");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var result = context.Result as BadRequestObjectResult;
        var errors = result!.Value as SerializableError;

        errors.Should().NotBeNull();
        errors!.Should().ContainKey("Email");
        var emailErrors = errors?["Email"] as string[];
        emailErrors.Should().HaveCount(2);
    }

    [Fact]
    public void OnActionExecuting_CalledMultipleTimes_ShouldWorkConsistently()
    {
        // Arrange
        var context1 = CreateActionExecutingContext();
        context1.ModelState.AddModelError("Field1", "Error 1");

        var context2 = CreateActionExecutingContext();
        // Valid model state

        var context3 = CreateActionExecutingContext();
        context3.ModelState.AddModelError("Field3", "Error 3");

        // Act
        _filter.OnActionExecuting(context1);
        _filter.OnActionExecuting(context2);
        _filter.OnActionExecuting(context3);

        // Assert
        context1.Result.Should().BeOfType<BadRequestObjectResult>();
        context2.Result.Should().BeNull();
        context3.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void OnActionExecuting_WithNestedPropertyErrors_ShouldInclude()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("User.Email", "Email is invalid");
        context.ModelState.AddModelError("User.Address.City", "City is required");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var result = context.Result as BadRequestObjectResult;
        var errors = result!.Value as SerializableError;

        errors.Should().NotBeNull();
        errors!.Should().ContainKey("User.Email");
        errors.Should().ContainKey("User.Address.City");
    }

    [Fact]
    public void OnActionExecuting_WithEmptyErrorMessage_ShouldStillSetBadRequest()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Field", string.Empty);

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region OnActionExecuted Tests

    [Fact]
    public void OnActionExecuted_ShouldDoNothing()
    {
        // Arrange
        var context = CreateActionExecutedContext();

        // Act - Should not throw or modify context
        _filter.OnActionExecuted(context);

        // Assert - No exceptions thrown, context unchanged
        context.Should().NotBeNull();
    }

    [Fact]
    public void OnActionExecuted_WithValidResult_ShouldNotModifyResult()
    {
        // Arrange
        var context = CreateActionExecutedContext();
        var originalResult = new OkObjectResult(new { message = "Success" });
        context.Result = originalResult;

        // Act
        _filter.OnActionExecuted(context);

        // Assert
        context.Result.Should().BeSameAs(originalResult);
    }

    [Fact]
    public void OnActionExecuted_WithException_ShouldNotInterfere()
    {
        // Arrange
        var context = CreateActionExecutedContext();
        context.Exception = new Exception("Test exception");

        // Act - Should not throw
        _filter.OnActionExecuted(context);

        // Assert
        context.Exception.Should().NotBeNull();
    }

    [Fact]
    public void OnActionExecuted_CalledMultipleTimes_ShouldRemainInert()
    {
        // Arrange
        var context = CreateActionExecutedContext();

        // Act - Call multiple times
        _filter.OnActionExecuted(context);
        _filter.OnActionExecuted(context);
        _filter.OnActionExecuted(context);

        // Assert - No changes, no exceptions
        context.Should().NotBeNull();
    }

    #endregion

    #region Integration Scenario Tests

    [Fact]
    public void Filter_ExecutingThenExecuted_ShouldWorkCorrectly()
    {
        // Arrange
        var executingContext = CreateActionExecutingContext();
        executingContext.ModelState.AddModelError("Email", "Invalid email");

        // Act - Simulating filter pipeline
        _filter.OnActionExecuting(executingContext);

        // In real pipeline, action wouldn't execute if result is set
        // But we can still call OnActionExecuted to verify it doesn't interfere
        var executedContext = CreateActionExecutedContext();
        _filter.OnActionExecuted(executedContext);

        // Assert
        executingContext.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Filter_WithBothMethodsValid_ShouldNotInterfere()
    {
        // Arrange
        var executingContext = CreateActionExecutingContext();
        var executedContext = CreateActionExecutedContext();

        // Act
        _filter.OnActionExecuting(executingContext);
        _filter.OnActionExecuted(executedContext);

        // Assert
        executingContext.Result.Should().BeNull(); // Valid model state
        executedContext.Result.Should().BeNull(); // Not modified
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void OnActionExecuting_WithVeryLongErrorMessage_ShouldHandle()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        var longMessage = new string('A', 5000);
        context.ModelState.AddModelError("Field", longMessage);

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var result = context.Result as BadRequestObjectResult;
        result.Should().NotBeNull();
        var errors = result!.Value as SerializableError;
        errors.Should().NotBeNull();
        errors!.Should().ContainKey("Field");
        var fieldErrors = errors?["Field"] as string[];
        fieldErrors.Should().NotBeNull();
        fieldErrors![0].Should().Be(longMessage);
    }

    [Fact]
    public void OnActionExecuting_WithSpecialCharactersInFieldName_ShouldHandle()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("User[0].Email", "Invalid email");
        context.ModelState.AddModelError("Data['key']", "Invalid key");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var result = context.Result as BadRequestObjectResult;
        var errors = result!.Value as SerializableError;

        errors.Should().NotBeNull();
        errors!.Should().ContainKey("User[0].Email");
        errors.Should().ContainKey("Data['key']");
    }

    [Fact]
    public void OnActionExecuting_AfterClearingModelState_ShouldNotSetResult()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Email", "Error");
        context.ModelState.Clear(); // Clear all errors

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeNull(); // ModelState is now valid
    }

    #endregion

    #region Helper Methods

    private ActionExecutingContext CreateActionExecutingContext()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor()
        );

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: null!
        );
    }

    private ActionExecutedContext CreateActionExecutedContext()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor()
        );

        return new ActionExecutedContext(
            actionContext,
            new List<IFilterMetadata>(),
            controller: null!
        );
    }

    #endregion
}
