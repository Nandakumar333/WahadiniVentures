using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace WahadiniCryptoQuest.API.Extensions;

/// <summary>
/// Extension methods for configuring rate limiting policies
/// Implements T043-T044: Rate limiting with endpoint-specific policies
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Configures rate limiting policies for different endpoint categories
    /// </summary>
    public static IServiceCollection AddRateLimitingPolicies(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Progress update rate limit (existing - 5 requests per 5 seconds)
            // Relaxed from 1 to 5 to handle multiple updates during playback/seeking
            options.AddPolicy("progress-update", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetClientIdentifier(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromSeconds(5),
                        SegmentsPerWindow = 1,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // Balance and transaction history endpoints (100 req/min)
            options.AddPolicy("balance-history", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetClientIdentifier(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 2,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // Leaderboard endpoints (60 req/min - backend has 15-minute cache, so safe to allow more requests)
            options.AddPolicy("leaderboard", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetClientIdentifier(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 2,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // Achievement endpoints (50 req/min)
            options.AddPolicy("achievements", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetClientIdentifier(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 50,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 2,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // General reward endpoints (100 req/min)
            options.AddPolicy("rewards-general", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetClientIdentifier(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 2,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // Global rejection response
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                var retryAfterSeconds = 60;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    retryAfterSeconds = (int)retryAfter.TotalSeconds;
                    context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString();
                }

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Rate limit exceeded",
                    message = "Too many requests. Please try again later.",
                    retryAfter = retryAfterSeconds
                }, cancellationToken);
            };
        });

        return services;
    }

    /// <summary>
    /// Gets a unique identifier for the client making the request
    /// Uses authenticated user name if available, otherwise IP address
    /// </summary>
    private static string GetClientIdentifier(HttpContext context)
    {
        return context.User.Identity?.Name
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "anonymous";
    }
}
