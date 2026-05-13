using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.DAL.Services;

/// <summary>
/// Service for password hashing and verification using BCrypt
/// </summary>
public class PasswordHashingService : IPasswordHashingService
{
    private readonly int _workFactor;

    public PasswordHashingService()
    {
        // Use different work factors for development vs production
        _workFactor = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ? 4 : 12;
    }

    /// <summary>
    /// Hashes a password using BCrypt
    /// </summary>
    /// <param name="password">Plain text password to hash</param>
    /// <returns>Hashed password</returns>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, _workFactor);
    }

    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    /// <param name="password">Plain text password to verify</param>
    /// <param name="hash">Stored password hash</param>
    /// <returns>True if password matches hash, false otherwise</returns>
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            // Return false if verification fails for any reason
            return false;
        }
    }

    /// <summary>
    /// Checks if a hash needs to be rehashed (e.g., due to security updates)
    /// </summary>
    /// <param name="hash">Password hash to check</param>
    /// <returns>True if hash needs to be updated, false otherwise</returns>
    public bool NeedsRehash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return true;

        try
        {
            // Check if the hash was created with a lower work factor
            return BCrypt.Net.BCrypt.PasswordNeedsRehash(hash, _workFactor);
        }
        catch
        {
            // If we can't determine, assume it needs rehashing
            return true;
        }
    }
}