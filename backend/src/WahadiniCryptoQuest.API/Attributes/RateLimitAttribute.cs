using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Concurrent;
using System.Net;

namespace WahadiniCryptoQuest.API.Attributes;

/// <summary>
/// T182: Rate limiting attribute for specific endpoints
/// Provides finer-grained rate control beyond global middleware
/// Usage: [RateLimit(RequestsPerMinute = 10)] on admin endpoints
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RateLimitAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Maximum requests per minute for this endpoint (default: 10 for admin endpoints)
    /// </summary>
    public int RequestsPerMinute { get; set; } = 10;

    /// <summary>
    /// Burst capacity - allows brief spikes above rate limit (default: 2)
    /// </summary>
    public int BurstCapacity { get; set; } = 2;

    // Static storage for rate limit buckets per endpoint+user
    private static readonly ConcurrentDictionary<string, EndpointBucket> _buckets = new();

    // Cleanup timer to remove stale entries
    private static readonly Timer _cleanupTimer = new(CleanupStaleEntries, null,
        TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Get endpoint identifier (controller + action + user)
        var endpoint = GetEndpointIdentifier(context);

        // Get or create rate limit bucket for this endpoint+user combination
        var bucket = _buckets.GetOrAdd(endpoint, _ => new EndpointBucket(
            capacity: RequestsPerMinute + BurstCapacity,
            refillRate: RequestsPerMinute / 60.0)); // Per second

        if (bucket.TryConsume())
        {
            // Request allowed - add custom rate limit headers
            context.HttpContext.Response.Headers["X-Endpoint-RateLimit-Limit"] = RequestsPerMinute.ToString();
            context.HttpContext.Response.Headers["X-Endpoint-RateLimit-Remaining"] = ((int)bucket.CurrentTokens).ToString();

            await next();
        }
        else
        {
            // Rate limit exceeded for this specific endpoint
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<RateLimitAttribute>>();
            logger.LogWarning("Endpoint rate limit exceeded: {Endpoint}", endpoint);

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.HttpContext.Response.Headers["Retry-After"] = "60";
            context.HttpContext.Response.Headers["X-Endpoint-RateLimit-Limit"] = RequestsPerMinute.ToString();
            context.HttpContext.Response.Headers["X-Endpoint-RateLimit-Remaining"] = "0";

            context.Result = new JsonResult(new
            {
                error = "Endpoint rate limit exceeded",
                message = $"Too many requests to this endpoint. Maximum {RequestsPerMinute} requests per minute allowed.",
                retryAfter = "60 seconds",
                endpoint = context.ActionDescriptor.DisplayName
            })
            {
                StatusCode = (int)HttpStatusCode.TooManyRequests
            };
        }
    }

    private static string GetEndpointIdentifier(ActionExecutingContext context)
    {
        // Combine controller, action, and user ID for unique rate limit key
        var controller = context.Controller.GetType().Name;
        var action = context.ActionDescriptor.DisplayName ?? "unknown";
        var userId = context.HttpContext.User?.Identity?.Name ?? "anonymous";

        return $"{controller}_{action}_{userId}";
    }

    private static void CleanupStaleEntries(object? state)
    {
        var staleThreshold = DateTime.UtcNow.AddMinutes(-5);
        var staleKeys = _buckets
            .Where(kvp => kvp.Value.LastAccessTime < staleThreshold)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in staleKeys)
        {
            _buckets.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Token bucket for endpoint-specific rate limiting
    /// </summary>
    private class EndpointBucket
    {
        private readonly double _capacity;
        private readonly double _refillRate;
        private double _tokens;
        private DateTime _lastRefillTime;
        private readonly object _lock = new();

        public double CurrentTokens => _tokens;
        public DateTime LastAccessTime { get; private set; }

        public EndpointBucket(double capacity, double refillRate)
        {
            _capacity = capacity;
            _refillRate = refillRate;
            _tokens = capacity;
            _lastRefillTime = DateTime.UtcNow;
            LastAccessTime = DateTime.UtcNow;
        }

        public bool TryConsume(double tokens = 1)
        {
            lock (_lock)
            {
                Refill();
                LastAccessTime = DateTime.UtcNow;

                if (_tokens >= tokens)
                {
                    _tokens -= tokens;
                    return true;
                }

                return false;
            }
        }

        private void Refill()
        {
            var now = DateTime.UtcNow;
            var timeSinceLastRefill = (now - _lastRefillTime).TotalSeconds;
            var tokensToAdd = timeSinceLastRefill * _refillRate;

            _tokens = Math.Min(_capacity, _tokens + tokensToAdd);
            _lastRefillTime = now;
        }
    }
}
