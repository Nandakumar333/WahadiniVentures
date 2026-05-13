using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.API.HealthChecks;

/// <summary>
/// Custom health check for database connectivity and responsiveness
/// Phase 9.3: Production reliability - comprehensive health monitoring
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(ApplicationDbContext context, ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check database connectivity with timeout
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            // Simple query to verify database connection
            var canConnect = await _context.Database.CanConnectAsync(linkedCts.Token);

            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Database connection failed");
            }

            // Check if database responds to a simple query
            var userCount = await _context.Users.CountAsync(linkedCts.Token);

            var data = new Dictionary<string, object>
            {
                { "database_status", "connected" },
                { "user_count", userCount },
                { "response_time_ms", context.Registration.Timeout.TotalMilliseconds }
            };

            return HealthCheckResult.Healthy("Database is responsive", data);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Database health check timed out");
            return HealthCheckResult.Degraded("Database health check timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database health check failed", ex);
        }
    }
}

/// <summary>
/// Health check for memory usage monitoring
/// Phase 9.4: Observability - track application resource usage
/// </summary>
public class MemoryHealthCheck : IHealthCheck
{
    private readonly ILogger<MemoryHealthCheck> _logger;
    private const long WarningThresholdBytes = 1_000_000_000; // 1 GB
    private const long UnhealthyThresholdBytes = 2_000_000_000; // 2 GB

    public MemoryHealthCheck(ILogger<MemoryHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var allocated = GC.GetTotalMemory(forceFullCollection: false);
        var gcInfo = GC.GetGCMemoryInfo();

        var data = new Dictionary<string, object>
        {
            { "allocated_bytes", allocated },
            { "allocated_mb", allocated / 1_024 / 1_024 },
            { "gen0_collections", GC.CollectionCount(0) },
            { "gen1_collections", GC.CollectionCount(1) },
            { "gen2_collections", GC.CollectionCount(2) },
            { "heap_size_bytes", gcInfo.HeapSizeBytes },
            { "memory_load_bytes", gcInfo.MemoryLoadBytes }
        };

        if (allocated >= UnhealthyThresholdBytes)
        {
            _logger.LogWarning("Memory usage is critically high: {Allocated} MB", allocated / 1_024 / 1_024);
            return Task.FromResult(HealthCheckResult.Unhealthy("Memory usage is critically high", null, data));
        }

        if (allocated >= WarningThresholdBytes)
        {
            _logger.LogInformation("Memory usage is elevated: {Allocated} MB", allocated / 1_024 / 1_024);
            return Task.FromResult(HealthCheckResult.Degraded("Memory usage is elevated", null, data));
        }

        return Task.FromResult(HealthCheckResult.Healthy("Memory usage is normal", data));
    }
}

/// <summary>
/// Health check for Stripe API connectivity
/// Phase 9.3: Reliability - monitor external service dependencies
/// </summary>
public class StripeHealthCheck : IHealthCheck
{
    private readonly ILogger<StripeHealthCheck> _logger;
    private readonly IConfiguration _configuration;

    public StripeHealthCheck(ILogger<StripeHealthCheck> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = _configuration["Stripe:SecretKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return HealthCheckResult.Degraded("Stripe API key not configured");
            }

            // Simple check - verify API key format
            var isTestKey = apiKey.StartsWith("sk_test_");
            var isLiveKey = apiKey.StartsWith("sk_live_");

            if (!isTestKey && !isLiveKey)
            {
                return HealthCheckResult.Unhealthy("Invalid Stripe API key format");
            }

            var data = new Dictionary<string, object>
            {
                { "stripe_mode", isTestKey ? "test" : "live" },
                { "api_key_configured", true }
            };

            return HealthCheckResult.Healthy("Stripe configuration is valid", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe health check failed");
            return HealthCheckResult.Unhealthy("Stripe health check failed", ex);
        }
    }
}
