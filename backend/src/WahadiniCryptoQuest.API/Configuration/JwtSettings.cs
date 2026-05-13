namespace WahadiniCryptoQuest.API.Configuration;

/// <summary>
/// JWT configuration settings for the WahadiniCryptoQuest platform
/// Contains all JWT-related configuration parameters for authentication
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// JWT secret key for signing tokens
    /// Must be at least 256 bits (32 characters) for HS256 algorithm
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// JWT token issuer (iss claim)
    /// Identifies the principal that issued the JWT
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// JWT token audience (aud claim)
    /// Identifies the recipients that the JWT is intended for
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// JWT access token expiration time in minutes
    /// Default: 15 minutes for security best practices
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Refresh token expiration time in days
    /// Default: 7 days for balance between security and user experience
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// Whether to validate the issuer claim
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Whether to validate the audience claim
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Whether to validate the token lifetime
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// Whether to validate the issuer signing key
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;

    /// <summary>
    /// Clock skew tolerance for token validation (in minutes)
    /// Default: 0 for strict validation
    /// </summary>
    public int ClockSkewMinutes { get; set; } = 0;

    /// <summary>
    /// Validates that all required settings are configured
    /// </summary>
    /// <returns>True if configuration is valid, false otherwise</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(SecretKey) &&
               SecretKey.Length >= 32 &&
               !string.IsNullOrWhiteSpace(Issuer) &&
               !string.IsNullOrWhiteSpace(Audience) &&
               AccessTokenExpirationMinutes > 0 &&
               RefreshTokenExpirationDays > 0;
    }

    /// <summary>
    /// Gets validation error messages for invalid configuration
    /// </summary>
    /// <returns>List of validation error messages</returns>
    public IEnumerable<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(SecretKey))
            errors.Add("JWT SecretKey is required");
        else if (SecretKey.Length < 32)
            errors.Add("JWT SecretKey must be at least 32 characters long");

        if (string.IsNullOrWhiteSpace(Issuer))
            errors.Add("JWT Issuer is required");

        if (string.IsNullOrWhiteSpace(Audience))
            errors.Add("JWT Audience is required");

        if (AccessTokenExpirationMinutes <= 0)
            errors.Add("JWT AccessTokenExpirationMinutes must be greater than 0");

        if (RefreshTokenExpirationDays <= 0)
            errors.Add("JWT RefreshTokenExpirationDays must be greater than 0");

        return errors;
    }
}