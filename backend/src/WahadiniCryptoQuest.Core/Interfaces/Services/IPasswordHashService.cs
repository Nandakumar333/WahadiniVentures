namespace WahadiniCryptoQuest.Core.Interfaces.Services;

/// <summary>
/// Interface for password hashing operations
/// Provides secure password hashing and verification services
/// </summary>
public interface IPasswordHashService
{
    /// <summary>
    /// Hashes a plain text password using a secure algorithm
    /// </summary>
    /// <param name="password">Plain text password to hash</param>
    /// <returns>Hashed password string</returns>
    Task<string> HashPasswordAsync(string password);

    /// <summary>
    /// Verifies a plain text password against a hashed password
    /// </summary>
    /// <param name="password">Plain text password to verify</param>
    /// <param name="hashedPassword">Hashed password to compare against</param>
    /// <returns>True if passwords match, false otherwise</returns>
    Task<bool> VerifyPasswordAsync(string password, string hashedPassword);

    /// <summary>
    /// Checks if a password needs to be rehashed (due to updated security parameters)
    /// </summary>
    /// <param name="hashedPassword">The hashed password to check</param>
    /// <returns>True if rehashing is needed, false otherwise</returns>
    bool NeedsRehashing(string hashedPassword);
}