namespace WahadiniCryptoQuest.API.Configuration;

/// <summary>
/// Performance and scalability configuration settings
/// Senior Architect Pattern: Centralized performance tuning parameters
/// </summary>
public class PerformanceSettings
{
    public const string SectionName = "Performance";

    /// <summary>
    /// Maximum concurrent database operations (Connection Pool Size)
    /// Default: 100 connections - Prevents database overload
    /// </summary>
    public int MaxDatabaseConnections { get; set; } = 100;

    /// <summary>
    /// Minimum database connection pool size for quick response
    /// Default: 10 - Keeps warm connections ready
    /// </summary>
    public int MinDatabaseConnections { get; set; } = 10;

    /// <summary>
    /// Command timeout in seconds for database operations
    /// Default: 30 seconds - Prevents long-running queries
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum degree of parallelism for batch operations
    /// Default: 4 - Balances throughput and resource usage
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = 4;

    /// <summary>
    /// API rate limit: Max requests per minute per IP
    /// Default: 100 - Prevents API abuse
    /// </summary>
    public int RateLimitPerMinute { get; set; } = 100;

    /// <summary>
    /// Burst allowance: Extra requests allowed in short burst
    /// Default: 20 - Handles legitimate traffic spikes
    /// </summary>
    public int RateLimitBurst { get; set; } = 20;

    /// <summary>
    /// Bulk operation batch size
    /// Default: 500 - Optimal for most operations
    /// </summary>
    public int BatchSize { get; set; } = 500;

    /// <summary>
    /// Enable response caching
    /// Default: true - Improves performance for repeated requests
    /// </summary>
    public bool EnableResponseCaching { get; set; } = true;

    /// <summary>
    /// Cache duration in minutes
    /// Default: 5 minutes - Balance between freshness and performance
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 5;

    /// <summary>
    /// Enable distributed caching (Redis)
    /// Default: false - Enable in production with Redis
    /// </summary>
    public bool EnableDistributedCache { get; set; } = false;

    /// <summary>
    /// Circuit breaker failure threshold before opening circuit
    /// Default: 5 - Fails fast to prevent cascading failures
    /// </summary>
    public int CircuitBreakerThreshold { get; set; } = 5;

    /// <summary>
    /// Circuit breaker duration in seconds before attempting retry
    /// Default: 30 seconds
    /// </summary>
    public int CircuitBreakerDurationSeconds { get; set; } = 30;

    /// <summary>
    /// Enable async processing for non-critical operations
    /// Default: true - Offloads work to background
    /// </summary>
    public bool EnableAsyncProcessing { get; set; } = true;

    /// <summary>
    /// Email sending parallelism
    /// Default: 3 - Prevents email server overload
    /// </summary>
    public int EmailParallelism { get; set; } = 3;

    /// <summary>
    /// Query result page size for pagination
    /// Default: 50 - Optimal balance
    /// </summary>
    public int DefaultPageSize { get; set; } = 50;

    /// <summary>
    /// Maximum page size to prevent excessive data transfer
    /// Default: 100
    /// </summary>
    public int MaxPageSize { get; set; } = 100;

    /// <summary>
    /// Enable query result streaming for large datasets
    /// Default: true
    /// </summary>
    public bool EnableStreaming { get; set; } = true;

    /// <summary>
    /// HTTP client timeout in seconds
    /// Default: 30 seconds
    /// </summary>
    public int HttpClientTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Retry policy: Max retry attempts for transient failures
    /// Default: 3 attempts
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Retry policy: Initial delay in milliseconds (exponential backoff)
    /// Default: 100ms
    /// </summary>
    public int RetryDelayMilliseconds { get; set; } = 100;

    /// <summary>
    /// Enable compression for API responses
    /// Default: true - Reduces bandwidth
    /// </summary>
    public bool EnableCompression { get; set; } = true;

    /// <summary>
    /// Minimum response size in bytes to compress
    /// Default: 1024 bytes (1KB)
    /// </summary>
    public int CompressionMinimumBytes { get; set; } = 1024;
}
