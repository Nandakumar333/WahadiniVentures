using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Rich domain model for EmailVerificationToken entity
/// Manages email verification tokens with proper encapsulation and business logic
/// </summary>
public class EmailVerificationToken : BaseEntity
{
    // Private fields for encapsulation
    private string _token = string.Empty;

    // Public properties with private setters for encapsulation
    public Guid UserId { get; private set; }
    
    public string Token 
    { 
        get => _token; 
        private set => _token = value ?? throw new ArgumentNullException(nameof(value)); 
    }
    
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }

    // Navigation property
    public virtual User? User { get; private set; }

    // Private constructor for Entity Framework
    private EmailVerificationToken() { }

    /// <summary>
    /// Factory method for creating a new EmailVerificationToken with auto-generated token
    /// Follows Domain-Driven Design principles
    /// </summary>
    /// <param name="userId">ID of the user this token belongs to</param>
    /// <param name="expirationHours">Hours until token expires (default 24 hours)</param>
    /// <returns>New EmailVerificationToken entity</returns>
    public static EmailVerificationToken Create(Guid userId, int expirationHours = 24)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (expirationHours <= 0)
            throw new ArgumentException("Expiration hours must be positive", nameof(expirationHours));

        var token = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            _token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddHours(expirationHours),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        };

        return token;
    }

    /// <summary>
    /// Factory method for creating a token with a specific token value (for testing)
    /// </summary>
    /// <param name="userId">ID of the user this token belongs to</param>
    /// <param name="tokenValue">Specific token value</param>
    /// <param name="expirationHours">Hours until token expires</param>
    /// <returns>New EmailVerificationToken entity</returns>
    public static EmailVerificationToken CreateWithToken(Guid userId, string tokenValue, int expirationHours = 24)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (string.IsNullOrWhiteSpace(tokenValue))
            throw new ArgumentException("Token value cannot be null or empty", nameof(tokenValue));

        if (expirationHours <= 0)
            throw new ArgumentException("Expiration hours must be positive", nameof(expirationHours));

        var token = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            _token = tokenValue,
            ExpiresAt = DateTime.UtcNow.AddHours(expirationHours),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        };

        return token;
    }

    /// <summary>
    /// Factory method for creating an expired token (for testing purposes)
    /// </summary>
    /// <param name="userId">ID of the user this token belongs to</param>
    /// <param name="tokenValue">Specific token value</param>
    /// <param name="hoursExpiredAgo">How many hours ago the token expired</param>
    /// <returns>New expired EmailVerificationToken entity</returns>
    public static EmailVerificationToken CreateExpiredToken(Guid userId, string tokenValue, int hoursExpiredAgo = 1)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (string.IsNullOrWhiteSpace(tokenValue))
            throw new ArgumentException("Token value cannot be null or empty", nameof(tokenValue));

        if (hoursExpiredAgo <= 0)
            throw new ArgumentException("Hours expired ago must be positive", nameof(hoursExpiredAgo));

        var token = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            _token = tokenValue,
            ExpiresAt = DateTime.UtcNow.AddHours(-hoursExpiredAgo), // Set to past date
            IsUsed = false,
            CreatedAt = DateTime.UtcNow.AddHours(-hoursExpiredAgo - 1), // Created before expiration
            UpdatedAt = DateTime.UtcNow.AddHours(-hoursExpiredAgo - 1),
            CreatedBy = "System",
            UpdatedBy = "System"
        };

        return token;
    }

    /// <summary>
    /// Domain method to mark the token as used
    /// </summary>
    public void MarkAsUsed()
    {
        if (IsUsed)
            throw new InvalidOperationException("Token has already been used");

        if (IsExpired())
            throw new InvalidOperationException("Cannot use expired token");

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Domain method to check if token has expired
    /// </summary>
    /// <returns>True if token is expired, false otherwise</returns>
    public bool IsExpired()
    {
        return DateTime.UtcNow > ExpiresAt;
    }

    /// <summary>
    /// Domain method to check if token is valid (not used and not expired)
    /// </summary>
    /// <returns>True if token is valid, false otherwise</returns>
    public bool IsValid()
    {
        return !IsUsed && !IsExpired();
    }

    /// <summary>
    /// Domain method to check if token matches the provided value
    /// </summary>
    /// <param name="tokenValue">Token value to compare</param>
    /// <returns>True if tokens match, false otherwise</returns>
    public bool MatchesToken(string tokenValue)
    {
        if (string.IsNullOrWhiteSpace(tokenValue))
            return false;

        return string.Equals(_token, tokenValue, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the remaining time until token expires
    /// </summary>
    /// <returns>TimeSpan until expiration, or TimeSpan.Zero if already expired</returns>
    public TimeSpan GetTimeUntilExpiration()
    {
        var remaining = ExpiresAt - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Generates a cryptographically secure random token
    /// </summary>
    /// <returns>Base64-encoded random token</returns>
    private static string GenerateSecureToken()
    {
        const int tokenLength = 32; // 32 bytes = 256 bits
        var randomBytes = new byte[tokenLength];
        
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", ""); // URL-safe Base64
    }
}