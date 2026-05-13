namespace WahadiniCryptoQuest.Core.Common;

/// <summary>
/// Application-wide constants for WahadiniCryptoQuest platform
/// </summary>
public static class ApplicationConstants
{
    /// <summary>
    /// Security and authentication constants
    /// </summary>
    public static class Security
    {
        /// <summary>
        /// Maximum failed login attempts before account lockout
        /// </summary>
        public const int MaxFailedLoginAttempts = 5;
        
        /// <summary>
        /// Account lockout duration in minutes after max failed attempts
        /// </summary>
        public const int LockoutDurationMinutes = 30;
        
        /// <summary>
        /// Password reset token size in bytes (256 bits)
        /// </summary>
        public const int PasswordResetTokenSizeBytes = 32;
        
        /// <summary>
        /// Email verification token size in bytes (256 bits)
        /// </summary>
        public const int EmailVerificationTokenSizeBytes = 32;
    }
    
    /// <summary>
    /// Token expiration constants
    /// </summary>
    public static class TokenExpiration
    {
        /// <summary>
        /// Access token expiration in minutes (1 hour)
        /// </summary>
        public const int AccessTokenMinutes = 60;
        
        /// <summary>
        /// Refresh token expiration in days
        /// </summary>
        public const int RefreshTokenDays = 30;
        
        /// <summary>
        /// Password reset token expiration in hours
        /// </summary>
        public const int PasswordResetTokenHours = 24;
        
        /// <summary>
        /// Email verification token expiration in days
        /// </summary>
        public const int EmailVerificationTokenDays = 7;
    }
    
    /// <summary>
    /// Database connection pool constants
    /// </summary>
    public static class DatabasePool
    {
        /// <summary>
        /// Connection lifetime in seconds (5 minutes)
        /// </summary>
        public const int ConnectionLifetimeSeconds = 300;
        
        /// <summary>
        /// Connection idle lifetime in seconds (1 minute)
        /// </summary>
        public const int ConnectionIdleLifetimeSeconds = 60;
        
        /// <summary>
        /// Command timeout in seconds
        /// </summary>
        public const int CommandTimeoutSeconds = 30;
        
        /// <summary>
        /// TCP keep-alive interval in seconds
        /// </summary>
        public const int TcpKeepAliveIntervalSeconds = 10;
        
        /// <summary>
        /// TCP keep-alive time in seconds
        /// </summary>
        public const int TcpKeepAliveTimeSeconds = 30;
    }
    
    /// <summary>
    /// Caching constants
    /// </summary>
    public static class Cache
    {
        /// <summary>
        /// Authorization cache duration in minutes
        /// </summary>
        public const int AuthorizationCacheMinutes = 5;
        
        /// <summary>
        /// General cache duration in minutes
        /// </summary>
        public const int DefaultCacheMinutes = 5;
    }
    
    /// <summary>
    /// Rate limiting constants
    /// </summary>
    public static class RateLimiting
    {
        /// <summary>
        /// Rate limit window duration in minutes
        /// </summary>
        public const int WindowMinutes = 5;
        
        /// <summary>
        /// Retry-After header value in seconds when rate limited
        /// </summary>
        public const int RetryAfterSeconds = 60;
        
        /// <summary>
        /// Stale entry cleanup threshold in minutes
        /// </summary>
        public const int StaleThresholdMinutes = 10;
    }
    
    /// <summary>
    /// Logging constants
    /// </summary>
    public static class Logging
    {
        /// <summary>
        /// Number of log files to retain
        /// </summary>
        public const int RetainedFileCountLimit = 30;
        
        /// <summary>
        /// Log file size limit in bytes (10 MB)
        /// </summary>
        public const long FileSizeLimitBytes = 10L * 1024 * 1024;
    }
    
    /// <summary>
    /// HSTS (HTTP Strict Transport Security) constants
    /// </summary>
    public static class Hsts
    {
        /// <summary>
        /// HSTS max-age in days (1 year)
        /// </summary>
        public const int MaxAgeDays = 365;
    }
    
    /// <summary>
    /// Validation constants
    /// </summary>
    public static class Validation
    {
        /// <summary>
        /// Maximum email length
        /// </summary>
        public const int MaxEmailLength = 256;
        
        /// <summary>
        /// Maximum password length
        /// </summary>
        public const int MaxPasswordLength = 100;
        
        /// <summary>
        /// Minimum password length
        /// </summary>
        public const int MinPasswordLength = 8;
        
        /// <summary>
        /// Maximum first name length
        /// </summary>
        public const int MaxFirstNameLength = 50;
        
        /// <summary>
        /// Maximum last name length
        /// </summary>
        public const int MaxLastNameLength = 50;
    }
    
    /// <summary>
    /// Retry policy constants
    /// </summary>
    public static class RetryPolicy
    {
        /// <summary>
        /// Maximum number of retry attempts
        /// </summary>
        public const int MaxRetryAttempts = 3;
        
        /// <summary>
        /// Maximum retry delay in seconds
        /// </summary>
        public const int MaxRetryDelaySeconds = 5;
        
        /// <summary>
        /// Initial retry delay in milliseconds
        /// </summary>
        public const int InitialRetryDelayMilliseconds = 100;
    }
}
