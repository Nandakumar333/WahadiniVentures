namespace WahadiniCryptoQuest.API.Configuration;

/// <summary>
/// Rate limit policy configuration for specific endpoint categories
/// Implements T044: Configure rate limit policies for different endpoint types
/// </summary>
public class RateLimitPolicy
{
    /// <summary>
    /// Maximum number of requests allowed within the window
    /// </summary>
    public int PermitLimit { get; set; }

    /// <summary>
    /// Time window in seconds
    /// </summary>
    public int Window { get; set; }

    /// <summary>
    /// Maximum number of queued requests (0 = no queueing)
    /// </summary>
    public int QueueLimit { get; set; }
}

/// <summary>
/// Rate limit policies configuration wrapper
/// Maps to "RateLimitPolicies" section in appsettings.json
/// </summary>
public class RateLimitPolicies
{
    public const string SectionName = "RateLimitPolicies";

    /// <summary>
    /// General API endpoints (default)
    /// Default: 100 requests/minute
    /// </summary>
    public RateLimitPolicy General { get; set; } = new()
    {
        PermitLimit = 100,
        Window = 60,
        QueueLimit = 0
    };

    /// <summary>
    /// Balance and transaction history endpoints
    /// Default: 100 requests/minute
    /// </summary>
    public RateLimitPolicy BalanceAndHistory { get; set; } = new()
    {
        PermitLimit = 100,
        Window = 60,
        QueueLimit = 0
    };

    /// <summary>
    /// Leaderboard endpoints (more restrictive due to computational cost)
    /// Default: 10 requests/minute
    /// </summary>
    public RateLimitPolicy Leaderboard { get; set; } = new()
    {
        PermitLimit = 10,
        Window = 60,
        QueueLimit = 0
    };

    /// <summary>
    /// Achievement endpoints
    /// Default: 50 requests/minute
    /// </summary>
    public RateLimitPolicy Achievements { get; set; } = new()
    {
        PermitLimit = 50,
        Window = 60,
        QueueLimit = 0
    };

    /// <summary>
    /// Admin operations (higher limit for administrative tasks)
    /// Default: 200 requests/minute
    /// </summary>
    public RateLimitPolicy AdminOperations { get; set; } = new()
    {
        PermitLimit = 200,
        Window = 60,
        QueueLimit = 0
    };
}
