using System.Security.Cryptography;
using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Password reset token entity for secure password reset functionality
/// Contains token data and validation logic for password reset operations
/// </summary>
public class PasswordResetToken : BaseEntity
{
    // Private fields for encapsulation
    private string _token = string.Empty;
    private string _hashedToken = string.Empty;
    private DateTime _expiresAt;
    private bool _isUsed;
    private string? _usedAt;
    private string? _clientInfo;
    private string? _ipAddress;

    /// <summary>
    /// User ID who requested the password reset
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Raw token value (only set during creation, not persisted)
    /// </summary>
    public string Token 
    { 
        get => _token; 
        private set => _token = value ?? string.Empty; 
    }

    /// <summary>
    /// Hashed token value for secure storage
    /// </summary>
    public string HashedToken 
    { 
        get => _hashedToken; 
        private set => _hashedToken = value ?? throw new ArgumentNullException(nameof(value)); 
    }

    /// <summary>
    /// When the token expires (typically 1 hour from creation)
    /// </summary>
    public DateTime ExpiresAt 
    { 
        get => _expiresAt; 
        private set => _expiresAt = value; 
    }

    /// <summary>
    /// Whether the token has been used for password reset
    /// </summary>
    public bool IsUsed 
    { 
        get => _isUsed; 
        private set => _isUsed = value; 
    }

    /// <summary>
    /// When the token was used (if applicable)
    /// </summary>
    public string? UsedAt 
    { 
        get => _usedAt; 
        private set => _usedAt = value; 
    }

    /// <summary>
    /// Client information when the token was created
    /// </summary>
    public string? ClientInfo 
    { 
        get => _clientInfo; 
        private set => _clientInfo = value; 
    }

    /// <summary>
    /// IP address when the token was created
    /// </summary>
    public string? IpAddress 
    { 
        get => _ipAddress; 
        private set => _ipAddress = value; 
    }

    /// <summary>
    /// Navigation property to User
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Computed property to check if token is expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Computed property to check if token is valid (not expired and not used)
    /// </summary>
    public bool IsValid => !IsExpired && !IsUsed;

    /// <summary>
    /// Gets remaining time until token expiration
    /// </summary>
    public TimeSpan GetRemainingTime()
    {
        var remaining = ExpiresAt - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Private parameterless constructor for EF Core
    /// </summary>
    private PasswordResetToken() : base() { }

    /// <summary>
    /// Private constructor for creating password reset tokens
    /// </summary>
    private PasswordResetToken(Guid userId, string hashedToken, DateTime expiresAt, string? clientInfo, string? ipAddress) : base()
    {
        UserId = userId;
        _hashedToken = hashedToken;
        _expiresAt = expiresAt;
        _clientInfo = clientInfo;
        _ipAddress = ipAddress;
        _isUsed = false;
    }

    /// <summary>
    /// Factory method to create a new password reset token
    /// Generates a secure random token and hashes it for storage
    /// </summary>
    /// <param name="userId">User ID requesting password reset</param>
    /// <param name="expiresAt">Token expiration time</param>
    /// <param name="clientInfo">Client information</param>
    /// <param name="ipAddress">Client IP address</param>
    /// <returns>New password reset token with raw token value</returns>
    public static PasswordResetToken Create(Guid userId, DateTime expiresAt, string? clientInfo = null, string? ipAddress = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiration time must be in the future", nameof(expiresAt));

        // Generate secure random token (URL-safe base64)
        var tokenBytes = new byte[32]; // 256 bits
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }
        var rawToken = Convert.ToBase64String(tokenBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        // Hash the token for secure storage
        var hashedToken = HashToken(rawToken);

        var token = new PasswordResetToken(userId, hashedToken, expiresAt, clientInfo, ipAddress);
        
        // Set the raw token (will not be persisted)
        token.Token = rawToken;
        
        return token;
    }

    /// <summary>
    /// Factory method to create a password reset token with default 1-hour expiration
    /// </summary>
    /// <param name="userId">User ID requesting password reset</param>
    /// <param name="clientInfo">Client information</param>
    /// <param name="ipAddress">Client IP address</param>
    /// <returns>New password reset token</returns>
    public static PasswordResetToken Create(Guid userId, string? clientInfo = null, string? ipAddress = null)
    {
        return Create(userId, DateTime.UtcNow.AddHours(1), clientInfo, ipAddress);
    }

    /// <summary>
    /// Validates if a raw token matches this password reset token
    /// </summary>
    /// <param name="rawToken">Raw token to validate</param>
    /// <returns>True if token matches and is valid</returns>
    public bool MatchesToken(string rawToken)
    {
        if (string.IsNullOrEmpty(rawToken) || !IsValid)
            return false;

        var hashedProvidedToken = HashToken(rawToken);
        return HashedToken.Equals(hashedProvidedToken, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Marks the token as used for password reset
    /// </summary>
    /// <param name="usedBy">Information about who used the token</param>
    public void MarkAsUsed(string usedBy = "System")
    {
        if (IsUsed)
            throw new InvalidOperationException("Token has already been used");

        if (IsExpired)
            throw new InvalidOperationException("Cannot use expired token");

        _isUsed = true;
        _usedAt = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC by {usedBy}";
        UpdateAuditFields(usedBy);
    }

    /// <summary>
    /// Hashes a token using SHA-256 for secure storage
    /// </summary>
    /// <param name="token">Raw token to hash</param>
    /// <returns>Hashed token</returns>
    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashedBytes);
    }
}