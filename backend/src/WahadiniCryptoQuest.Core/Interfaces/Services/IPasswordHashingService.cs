namespace WahadiniCryptoQuest.Core.Interfaces.Services;

/// <summary>
/// Interface for password hashing and verification operations
/// </summary>
public interface IPasswordHashingService
{
    /// <summary>
    /// Hashes a password using BCrypt
    /// </summary>
    /// <param name="password">Plain text password to hash</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    /// <param name="password">Plain text password to verify</param>
    /// <param name="hash">Stored password hash</param>
    /// <returns>True if password matches hash, false otherwise</returns>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Checks if a hash needs to be rehashed (e.g., due to security updates)
    /// </summary>
    /// <param name="hash">Password hash to check</param>
    /// <returns>True if hash needs to be updated, false otherwise</returns>
    bool NeedsRehash(string hash);
}