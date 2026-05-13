using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Core.Settings;

namespace WahadiniCryptoQuest.DAL.Identity;

/// <summary>
/// Service for JWT token generation, validation, and management
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public JwtTokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
        _tokenHandler = new JwtSecurityTokenHandler();
        
        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    /// <summary>
    /// Generates a new access token for the authenticated user with roles and permissions
    /// </summary>
    public async Task<string> GenerateAccessTokenAsync(Guid userId, string email, string username, IEnumerable<string> roles, bool emailConfirmed = false, IEnumerable<string>? permissions = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, username),
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("email_verified", emailConfirmed.ToString().ToLower())
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("role", role)); // Add 'role' claim for policy matching
        }

        // Add permission claims if provided
        if (permissions != null)
        {
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            SigningCredentials = creds,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return await Task.FromResult(_tokenHandler.WriteToken(token));
    }

    /// <summary>
    /// Generates a new refresh token for session management
    /// </summary>
    public async Task<string> GenerateRefreshTokenAsync()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return await Task.FromResult(Convert.ToBase64String(randomBytes));
    }

    /// <summary>
    /// Validates an access token and returns the claims principal
    /// </summary>
    public Task<ClaimsPrincipal?> ValidateAccessTokenAsync(string token)
    {
        try
        {
            var principal = _tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);
            
            // Ensure the token is a JWT and uses the correct algorithm
            if (validatedToken is JwtSecurityToken jwtToken &&
                jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return Task.FromResult<ClaimsPrincipal?>(principal);
            }

            return Task.FromResult<ClaimsPrincipal?>(null);
        }
        catch
        {
            return Task.FromResult<ClaimsPrincipal?>(null);
        }
    }

    /// <summary>
    /// Extracts claims from a token without validating expiration
    /// </summary>
    public Task<ClaimsPrincipal?> GetPrincipalFromTokenAsync(string token)
    {
        try
        {
            var tokenValidationParametersForExpired = _tokenValidationParameters.Clone();
            tokenValidationParametersForExpired.ValidateLifetime = false;

            var principal = _tokenHandler.ValidateToken(token, tokenValidationParametersForExpired, out var validatedToken);
            
            if (validatedToken is JwtSecurityToken jwtToken &&
                jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return Task.FromResult<ClaimsPrincipal?>(principal);
            }

            return Task.FromResult<ClaimsPrincipal?>(null);
        }
        catch
        {
            return Task.FromResult<ClaimsPrincipal?>(null);
        }
    }

    /// <summary>
    /// Gets the remaining time until token expiration
    /// </summary>
    public TimeSpan? GetTokenRemainingLifetime(string token)
    {
        try
        {
            var jwtToken = _tokenHandler.ReadJwtToken(token);
            var expirationTime = jwtToken.ValidTo;
            
            if (expirationTime <= DateTime.UtcNow)
            {
                return TimeSpan.Zero;
            }

            return expirationTime.Subtract(DateTime.UtcNow);
        }
        catch
        {
            return null;
        }
    }
}