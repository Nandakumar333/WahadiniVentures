using System.Net;
using System.Text.Json;
using Serilog;
using WahadiniCryptoQuest.Core.Exceptions;

namespace WahadiniCryptoQuest.API.Middleware;

/// <summary>
/// Global exception handling middleware with structured logging
/// Catches all unhandled exceptions and returns standardized error responses
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            // Reward system exceptions
            InsufficientPointsException => HttpStatusCode.BadRequest,
            TransactionConcurrencyException => HttpStatusCode.Conflict,
            DuplicateTransactionException => HttpStatusCode.Conflict,
            RewardException => HttpStatusCode.BadRequest,
            // Existing exceptions
            PremiumAccessDeniedException => HttpStatusCode.Forbidden,
            EntityNotFoundException => HttpStatusCode.NotFound,
            BusinessRuleValidationException => HttpStatusCode.BadRequest,
            DuplicateEntityException => HttpStatusCode.Conflict,
            FluentValidation.ValidationException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            InvalidOperationException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        // Special handling for reward system transaction failures (log as errors for monitoring)
        if (exception is TransactionConcurrencyException tce)
        {
            _logger.LogError(exception,
                "Transaction concurrency error for user {UserId} after {RetryCount} retries. Path: {Path}, Method: {Method}",
                tce.UserId,
                tce.RetryCount,
                context.Request.Path,
                context.Request.Method);
        }
        // Log with appropriate level based on status code
        else if ((int)statusCode >= 500)
        {
            _logger.LogError(exception,
                "Internal Server Error: {Message}. Path: {Path}, Method: {Method}, User: {User}",
                exception.Message,
                context.Request.Path,
                context.Request.Method,
                context.User?.Identity?.Name ?? "Anonymous");
        }
        else if ((int)statusCode >= 400)
        {
            _logger.LogWarning(exception,
                "Client Error: {StatusCode} - {Message}. Path: {Path}, Method: {Method}",
                (int)statusCode,
                exception.Message,
                context.Request.Path,
                context.Request.Method);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        // Special handling for PremiumAccessDeniedException
        object response;
        if (exception is PremiumAccessDeniedException)
        {
            response = new
            {
                error = exception.Message,
                requiresUpgrade = true,
                upgradeUrl = "/upgrade"
            };
        }
        else
        {
            response = new
            {
                statusCode = (int)statusCode,
                message = exception.Message,
                error = exception.Message, // Include 'error' field for compatibility
                details = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment()
                    ? exception.StackTrace
                    : null
            };
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Extension method for adding the global exception handler middleware to the pipeline
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
