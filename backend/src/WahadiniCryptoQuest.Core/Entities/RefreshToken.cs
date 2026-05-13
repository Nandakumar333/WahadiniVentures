using System.Security.Cryptography;
using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Represents a refresh token for maintaining user sessions
/// Implements domain logic for token lifecycle management and security
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? DeviceInfo { get; private set; }
    public string? IpAddress { get; private set; }

    // Navigation property
    public virtual User User { get; private set; } = null!;

    // Private constructor for EF Core
    private RefreshToken() { }

    /// <summary>
    /// Factory method to create a new refresh token
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="expiresAt">Token expiration time</param>
    /// <param name="deviceInfo">Optional device information</param>
    /// <param name="ipAddress">Optional IP address</param>
    /// <returns>New RefreshToken instance</returns>
    public static RefreshToken Create(
        Guid userId,
        DateTime expiresAt,
        string? deviceInfo = null,
        string? ipAddress = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiration date must be in the future", nameof(expiresAt));

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = GenerateSecureToken(),
            ExpiresAt = expiresAt,
            IsUsed = false,
            IsRevoked = false,
            DeviceInfo = deviceInfo?.Trim(),
            IpAddress = ipAddress?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        };
    }

    /// <summary>
    /// Checks if the refresh token is valid for use
    /// </summary>
    public bool IsValid => !IsUsed && !IsRevoked && !IsExpired && !IsDeleted;

    /// <summary>
    /// Checks if the refresh token has expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Marks the refresh token as used (one-time use pattern)
    /// </summary>
    /// <param name="usedBy">Who used the token</param>
    public void MarkAsUsed(string usedBy = "System")
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = usedBy;
    }

    /// <summary>
    /// Revokes the refresh token (for logout or security purposes)
    /// </summary>
    /// <param name="revokedBy">Who revoked the token</param>
    public void Revoke(string revokedBy = "System")
    {
        if (IsRevoked)
            return; // Already revoked

        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = revokedBy;
    }

    /// <summary>
    /// Updates device information for the token
    /// </summary>
    /// <param name="deviceInfo">New device information</param>
    /// <param name="ipAddress">New IP address</param>
    public void UpdateDeviceInfo(string? deviceInfo, string? ipAddress)
    {
        DeviceInfo = deviceInfo?.Trim();
        IpAddress = ipAddress?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the token matches the provided token string
    /// </summary>
    /// <param name="tokenToMatch">Token to compare</param>
    /// <returns>True if tokens match</returns>
    public bool MatchesToken(string tokenToMatch)
    {
        if (string.IsNullOrEmpty(tokenToMatch))
            return false;

        return Token.Equals(tokenToMatch, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the remaining time until expiration
    /// </summary>
    /// <returns>TimeSpan until expiration, or zero if expired</returns>
    public TimeSpan GetRemainingTime()
    {
        var remaining = ExpiresAt - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Generates a cryptographically secure random token
    /// </summary>
    /// <returns>Base64 encoded secure token</returns>
    private static string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var tokenBytes = new byte[64]; // 512 bits
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes);
    }
}