using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WahadiniCryptoQuest.Core.Exceptions;

namespace WahadiniCryptoQuest.API.Filters;

/// <summary>
/// Global exception filter for handling unhandled exceptions in API controllers
/// Converts exceptions to appropriate HTTP responses
/// </summary>
public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception,
            "Unhandled exception occurred. Path: {Path}",
            context.HttpContext.Request.Path);

        // Handle specific domain exceptions
        if (context.Exception is PremiumAccessDeniedException premiumEx)
        {
            var premiumResponse = new
            {
                error = premiumEx.Message,
                requiresUpgrade = true,
                upgradeUrl = "/upgrade" // Frontend route for upgrade page
            };

            context.Result = new ObjectResult(premiumResponse)
            {
                StatusCode = 403 // Forbidden
            };

            context.ExceptionHandled = true;
            return;
        }

        if (context.Exception is EntityNotFoundException notFoundEx)
        {
            context.Result = new NotFoundObjectResult(new
            {
                error = notFoundEx.Message
            });

            context.ExceptionHandled = true;
            return;
        }

        if (context.Exception is BusinessRuleValidationException validationEx)
        {
            context.Result = new BadRequestObjectResult(new
            {
                error = validationEx.Message
            });

            context.ExceptionHandled = true;
            return;
        }

        if (context.Exception is DuplicateEntityException duplicateEx)
        {
            context.Result = new ConflictObjectResult(new
            {
                error = duplicateEx.Message
            });

            context.ExceptionHandled = true;
            return;
        }

        // Default error response for unhandled exceptions
        var response = new
        {
            error = "An unexpected error occurred",
            message = context.Exception.Message,
            path = context.HttpContext.Request.Path.ToString()
        };

        context.Result = new ObjectResult(response)
        {
            StatusCode = 500
        };

        context.ExceptionHandled = true;
    }
}

/// <summary>
/// Validation filter for model state validation
/// Returns 400 Bad Request with validation errors if model state is invalid
/// </summary>
public class ValidateModelStateFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution
    }
}
