using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace WahadiniCryptoQuest.API.Policies;

/// <summary>
/// Polly resilience policies for production reliability
/// Phase 9.3: Retry logic, circuit breaker, timeout policies
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    /// Retry policy for transient failures with exponential backoff
    /// </summary>
    public static AsyncRetryPolicy GetRetryPolicy(int retryCount = 3)
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retry, context) =>
                {
                    var logger = context.GetLogger();
                    logger?.LogWarning(
                        exception,
                        "Retry {Retry} after {Delay}s due to {ExceptionType}: {Message}",
                        retry,
                        timeSpan.TotalSeconds,
                        exception.GetType().Name,
                        exception.Message
                    );
                }
            );
    }

    /// <summary>
    /// Circuit breaker policy to prevent cascading failures
    /// Opens circuit after 5 consecutive failures, stays open for 30 seconds
    /// </summary>
    public static AsyncCircuitBreakerPolicy GetCircuitBreakerPolicy()
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration, context) =>
                {
                    var logger = context.GetLogger();
                    logger?.LogError(
                        exception,
                        "Circuit breaker opened for {Duration}s due to {ExceptionType}: {Message}",
                        duration.TotalSeconds,
                        exception.GetType().Name,
                        exception.Message
                    );
                },
                onReset: (context) =>
                {
                    var logger = context.GetLogger();
                    logger?.LogInformation("Circuit breaker reset");
                },
                onHalfOpen: () =>
                {
                    // Log when circuit is testing if it should close
                }
            );
    }

    /// <summary>
    /// Timeout policy to prevent hanging requests
    /// </summary>
    public static AsyncTimeoutPolicy GetTimeoutPolicy(int timeoutSeconds = 10)
    {
        return Policy
            .TimeoutAsync(
                TimeSpan.FromSeconds(timeoutSeconds),
                TimeoutStrategy.Pessimistic,
                onTimeoutAsync: (context, timeSpan, task) =>
                {
                    var logger = context.GetLogger();
                    logger?.LogWarning(
                        "Request timed out after {Timeout}s",
                        timeSpan.TotalSeconds
                    );
                    return Task.CompletedTask;
                }
            );
    }

    /// <summary>
    /// Combined policy: Timeout → Retry → Circuit Breaker
    /// Use this for external service calls (e.g., Stripe API)
    /// </summary>
    public static IAsyncPolicy GetCombinedPolicy(int retryCount = 3, int timeoutSeconds = 10)
    {
        return Policy.WrapAsync(
            GetCircuitBreakerPolicy(),
            GetRetryPolicy(retryCount),
            GetTimeoutPolicy(timeoutSeconds)
        );
    }

    /// <summary>
    /// Database-specific retry policy for transient database errors
    /// </summary>
    public static AsyncRetryPolicy GetDatabaseRetryPolicy()
    {
        return Policy
            .Handle<Microsoft.EntityFrameworkCore.DbUpdateException>()
            .Or<System.Data.Common.DbException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(100 * retryAttempt),
                onRetry: (exception, timeSpan, retry, context) =>
                {
                    var logger = context.GetLogger();
                    logger?.LogWarning(
                        exception,
                        "Database retry {Retry} after {Delay}ms due to transient error",
                        retry,
                        timeSpan.TotalMilliseconds
                    );
                }
            );
    }

    /// <summary>
    /// Extension method to get logger from Polly context
    /// </summary>
    private static ILogger? GetLogger(this Context context)
    {
        if (context.TryGetValue("logger", out var loggerObj) && loggerObj is ILogger logger)
        {
            return logger;
        }
        return null;
    }
}
