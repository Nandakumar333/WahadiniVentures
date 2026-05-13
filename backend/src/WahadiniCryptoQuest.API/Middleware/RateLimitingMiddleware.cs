using System.Collections.Concurrent;
using System.Net;
using Microsoft.Extensions.Options;
using WahadiniCryptoQuest.API.Configuration;

namespace WahadiniCryptoQuest.API.Middleware;

/// <summary>
/// Rate limiting middleware to prevent API abuse and overload
/// Senior Architect Pattern: Token bucket algorithm with sliding window
/// Protects against DDoS and ensures fair resource allocation
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly PerformanceSettings _settings;
    
    // Token bucket per client IP
    private static readonly ConcurrentDictionary<string, TokenBucket> _clientBuckets = new();
    
    // Cleanup task to remove stale entries
    private static readonly Timer _cleanupTimer = new(CleanupStaleEntries, null, 
        TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        IOptions<PerformanceSettings> settings)
    {
        _next = next;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get client identifier (IP address + User-Agent for better distribution)
        var clientId = GetClientIdentifier(context);
        
        // Get or create token bucket for this client
        var bucket = _clientBuckets.GetOrAdd(clientId, _ => new TokenBucket(
            capacity: _settings.RateLimitPerMinute + _settings.RateLimitBurst,
            refillRate: _settings.RateLimitPerMinute / 60.0, // Per second
            _settings.RateLimitBurst));

        if (bucket.TryConsume())
        {
            // Request allowed - add rate limit headers
            context.Response.Headers["X-RateLimit-Limit"] = _settings.RateLimitPerMinute.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = ((int)bucket.CurrentTokens).ToString();
            context.Response.Headers["X-RateLimit-Reset"] = bucket.NextRefillTime.ToString("O");
            
            await _next(context);
        }
        else
        {
            // Rate limit exceeded
            _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientId);
            
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers["Retry-After"] = "60";
            context.Response.Headers["X-RateLimit-Limit"] = _settings.RateLimitPerMinute.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = "0";
            context.Response.Headers["X-RateLimit-Reset"] = bucket.NextRefillTime.ToString("O");
            
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                message = $"Too many requests. Maximum {_settings.RateLimitPerMinute} requests per minute allowed.",
                retryAfter = "60 seconds"
            });
        }
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Try to get real IP from X-Forwarded-For header (behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var ip = !string.IsNullOrEmpty(forwardedFor)
            ? forwardedFor.Split(',')[0].Trim()
            : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Include user agent for better distribution (optional)
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var userAgentHash = string.IsNullOrEmpty(userAgent) ? "" : $"_{userAgent.GetHashCode()}";
        
        return $"{ip}{userAgentHash}";
    }

    private static void CleanupStaleEntries(object? state)
    {
        var staleThreshold = DateTime.UtcNow.AddMinutes(-10);
        var staleKeys = _clientBuckets
            .Where(kvp => kvp.Value.LastAccessTime < staleThreshold)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in staleKeys)
        {
            _clientBuckets.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Token bucket implementation for rate limiting
    /// Allows bursts while maintaining average rate
    /// </summary>
    private class TokenBucket
    {
        private readonly double _capacity;
        private readonly double _refillRate;
        private readonly int _burst;
        private double _tokens;
        private DateTime _lastRefillTime;
        private readonly object _lock = new();

        public double CurrentTokens => _tokens;
        public DateTime LastAccessTime { get; private set; }
        public DateTime NextRefillTime => _lastRefillTime.AddSeconds(1 / _refillRate);

        public TokenBucket(double capacity, double refillRate, int burst)
        {
            _capacity = capacity;
            _refillRate = refillRate;
            _burst = burst;
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

/// <summary>
/// Extension method to add rate limiting middleware
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
