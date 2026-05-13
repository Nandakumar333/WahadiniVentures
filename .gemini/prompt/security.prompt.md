# WahadiniCryptoQuest - Security Development Prompt

## Context
You are an expert security engineer working on the WahadiniCryptoQuest. The application handles sensitive user data and learning progress and requires comprehensive security measures including authentication, authorization, data protection, and security monitoring across both frontend and backend systems.

## Security Architecture Overview

### Security Principles
- **Defense in Depth**: Multiple layers of security controls
- **Principle of Least Privilege**: Users get minimum required access
- **Zero Trust**: Never trust, always verify
- **Security by Design**: Security considerations built into every component
- **Data Privacy**: Protect sensitive financial information
- **Compliance**: Meet user data and learning progress protection standards

### Technology Stack

#### Authentication & Authorization
- **JWT Bearer Tokens** - Stateless authentication
- **BCrypt** - Password hashing with salt
- **ASP.NET Core Identity** - User management
- **Policy-Based Authorization** - Fine-grained access control
- **Role-Based Access Control (RBAC)** - Hierarchical permissions

#### Security Infrastructure
- **HTTPS/TLS 1.3** - Transport layer security
- **Content Security Policy (CSP)** - XSS protection
- **CORS** - Cross-origin resource sharing control
- **Rate Limiting** - DDoS and brute force protection
- **Input Validation** - SQL injection and XSS prevention

## Authentication System

### 1. JWT Token Management
```csharp
// JWT Service Implementation
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<JwtTokenService> _logger;

    public string GenerateAccessToken(User user, IEnumerable<Role> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new("security_stamp", user.SecurityStamp), // For token invalidation
            new("jti", Guid.NewGuid().ToString()), // JWT ID for tracking
            new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add role and permission claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
            
            foreach (var permission in role.Permissions.Where(p => p.IsActive))
            {
                claims.Add(new Claim("permission", permission.Permission.ToString()));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(Guid userId)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        return refreshToken;
    }

    public async Task<bool> ValidateRefreshTokenAsync(string token)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);
        
        return refreshToken != null &&
               refreshToken.IsActive &&
               refreshToken.ExpiryDate > DateTime.UtcNow &&
               !refreshToken.IsRevoked;
    }

    public async Task RevokeRefreshTokenAsync(string token, string reason = "Manual revocation")
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);
        if (refreshToken != null)
        {
            refreshToken.Revoke(reason);
            await _refreshTokenRepository.UpdateAsync(refreshToken);
        }
    }
}
```

### 2. Password Security
```csharp
// Password hashing service
public class PasswordHashService : IPasswordHashService
{
    private const int WorkFactor = 12; // BCrypt work factor

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        // Generate salt and hash password
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    public bool IsPasswordComplex(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }

    public string GenerateSecurePassword(int length = 16)
    {
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        
        var allChars = uppercase + lowercase + digits + special;
        var random = new Random();
        
        var password = new StringBuilder();
        
        // Ensure at least one character from each category
        password.Append(uppercase[random.Next(uppercase.Length)]);
        password.Append(lowercase[random.Next(lowercase.Length)]);
        password.Append(digits[random.Next(digits.Length)]);
        password.Append(special[random.Next(special.Length)]);
        
        // Fill the rest with random characters
        for (int i = 4; i < length; i++)
        {
            password.Append(allChars[random.Next(allChars.Length)]);
        }
        
        // Shuffle the password
        return new string(password.ToString().OrderBy(c => random.Next()).ToArray());
    }
}
```

### 3. Account Security
```csharp
// Account lockout service
public class AccountSecurityService : IAccountSecurityService
{
    private readonly IUserRepository _userRepository;
    private readonly SecuritySettings _securitySettings;
    private readonly ILogger<AccountSecurityService> _logger;

    public async Task<bool> IsAccountLockedAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        return user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow;
    }

    public async Task RecordFailedLoginAsync(Guid userId, string ipAddress)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return;

        user.FailedLoginAttempts++;
        user.LastFailedLoginAt = DateTime.UtcNow;
        user.LastFailedLoginIp = ipAddress;

        if (user.FailedLoginAttempts >= _securitySettings.MaxFailedLoginAttempts)
        {
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(_securitySettings.LockoutDurationMinutes);
            
            _logger.LogWarning("Account {UserId} locked due to {Attempts} failed login attempts from IP {IpAddress}",
                userId, user.FailedLoginAttempts, ipAddress);
                
            // Send security alert email
            await _emailService.SendSecurityAlertAsync(user.Email, "Account Locked", 
                $"Your account has been locked due to multiple failed login attempts from IP {ipAddress}");
        }

        await _userRepository.UpdateAsync(user);
    }

    public async Task ResetFailedLoginAttemptsAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return;

        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastSuccessfulLoginAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }

    public async Task InvalidateAllUserTokensAsync(Guid userId, string reason)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return;

        // Update security stamp to invalidate all existing tokens
        user.UpdateSecurityStamp();
        
        // Revoke all refresh tokens
        await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, reason);
        
        await _userRepository.UpdateAsync(user);
        
        _logger.LogInformation("All tokens invalidated for user {UserId}. Reason: {Reason}",
            userId, reason);
    }
}
```

## Authorization System

### 1. Role-Based Access Control
```csharp
// Hierarchical role system
public enum UserRole
{
    User = 1,          // Basic user access (23 permissions)
    Employee = 2,      // User + support permissions (27 permissions)
    Manager = 3,       // Employee + management permissions (34 permissions)
    Admin = 4,         // Manager + admin permissions (46 permissions)
    SuperAdmin = 5     // All permissions (47 permissions)
}

// Permission categories
public enum Permission
{
    // User Management (1-6)
    ViewUsers = 1, CreateUsers = 2, UpdateUsers = 3, DeleteUsers = 4,
    ManageUsers = 5, ManageUserRoles = 6,

    // Account Management (10-14)
    ViewAccounts = 10, CreateAccounts = 11, UpdateAccounts = 12,
    DeleteAccounts = 13, ViewAllAccounts = 14,

    // Transaction Management (20-24)
    ViewTransactions = 20, CreateTransactions = 21, UpdateTransactions = 22,
    DeleteTransactions = 23, ViewAllTransactions = 24,

    // Budget Management (30-34)
    ViewBudgets = 30, CreateBudgets = 31, UpdateBudgets = 32,
    DeleteBudgets = 33, ViewAllBudgets = 34,

    // System Administration (70-77)
    ViewSystemLogs = 70, ManageSystemSettings = 71, ManageSystem = 75,
    ViewFinancialData = 76, ManageFinancialData = 77
}

// Authorization service implementation
public class AuthorizationService : IAuthorizationService
{
    private readonly IUserRoleAssignmentRepository _userRoleRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuthorizationService> _logger;

    public async Task<bool> HasPermissionAsync(Guid userId, Permission permission)
    {
        var cacheKey = $"user_permissions_{userId}";
        
        if (!_cache.TryGetValue(cacheKey, out HashSet<Permission> userPermissions))
        {
            userPermissions = await GetUserPermissionsAsync(userId);
            _cache.Set(cacheKey, userPermissions, TimeSpan.FromMinutes(10));
        }

        var hasPermission = userPermissions.Contains(permission);
        
        if (!hasPermission)
        {
            _logger.LogWarning("Permission denied for user {UserId} accessing {Permission}",
                userId, permission);
        }

        return hasPermission;
    }

    public async Task<bool> HasAnyPermissionAsync(Guid userId, params Permission[] permissions)
    {
        var userPermissions = await GetUserPermissionsAsync(userId);
        return permissions.Any(p => userPermissions.Contains(p));
    }

    public async Task<bool> HasAllPermissionsAsync(Guid userId, params Permission[] permissions)
    {
        var userPermissions = await GetUserPermissionsAsync(userId);
        return permissions.All(p => userPermissions.Contains(p));
    }

    public async Task<bool> IsResourceOwnerAsync(Guid userId, string resourceType, Guid resourceId)
    {
        return resourceType.ToLower() switch
        {
            "transaction" => await IsTransactionOwnerAsync(userId, resourceId),
            "account" => await IsAccountOwnerAsync(userId, resourceId),
            "budget" => await IsBudgetOwnerAsync(userId, resourceId),
            _ => false
        };
    }

    private async Task<HashSet<Permission>> GetUserPermissionsAsync(Guid userId)
    {
        var userRoles = await _userRoleRepository.GetActiveRolesByUserIdAsync(userId);
        var permissions = new HashSet<Permission>();

        foreach (var userRole in userRoles)
        {
            var rolePermissions = await _rolePermissionRepository.GetPermissionsByRoleIdAsync(userRole.RoleId);
            foreach (var rolePermission in rolePermissions.Where(rp => rp.IsActive))
            {
                permissions.Add(rolePermission.Permission);
            }
        }

        return permissions;
    }
}
```

### 2. Custom Authorization Attributes
```csharp
// Permission-based authorization attribute
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(Permission permission)
    {
        Policy = $"Permission.{permission}";
    }

    public RequirePermissionAttribute(params Permission[] permissions)
    {
        Policy = $"Permissions.{string.Join(",", permissions)}";
    }
}

// Role-based authorization attributes
public class AdminOnlyAttribute : AuthorizeAttribute
{
    public AdminOnlyAttribute() => Policy = "AdminOnly";
}

public class ManagerAndAboveAttribute : AuthorizeAttribute
{
    public ManagerAndAboveAttribute() => Policy = "ManagerAndAbove";
}

// Resource ownership attribute
public class RequireResourceOwnershipAttribute : AuthorizeAttribute
{
    public RequireResourceOwnershipAttribute(string resourceType, string action)
    {
        Policy = $"ResourceOwnership.{resourceType}.{action}";
    }
}

// Usage examples
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all actions
public class TransactionsController : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permission.ViewTransactions)]
    public async Task<IActionResult> GetTransactions() { }

    [HttpPost]
    [RequirePermission(Permission.CreateTransactions)]
    public async Task<IActionResult> CreateTransaction() { }

    [HttpDelete("{id}")]
    [AdminOnly] // Only admins can delete transactions
    public async Task<IActionResult> DeleteTransaction(Guid id) { }
}
```

## Input Validation and Sanitization

### 1. Request Validation
```csharp
// Input validation with FluentValidation
public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be positive")
            .LessThanOrEqualTo(1000000).WithMessage("Amount too large")
            .Must(BeValidCurrency).WithMessage("Invalid currency format");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description too long")
            .Must(BeSafeContent).WithMessage("Description contains invalid characters");

        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Account is required")
            .Must(BeValidGuid).WithMessage("Invalid account ID format");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Date cannot be in future")
            .GreaterThan(DateTime.UtcNow.AddYears(-10)).WithMessage("Date too far in past");
    }

    private bool BeValidCurrency(decimal amount)
    {
        return Math.Round(amount, 2) == amount; // Ensure 2 decimal places max
    }

    private bool BeSafeContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return false;

        // Check for potential XSS patterns
        var dangerousPatterns = new[]
        {
            "<script", "</script>", "javascript:", "data:", "vbscript:",
            "onload=", "onerror=", "onclick=", "onmouseover="
        };

        return !dangerousPatterns.Any(pattern => 
            content.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private bool BeValidGuid(Guid guid)
    {
        return guid != Guid.Empty;
    }
}

// Global model validation filter
public class ModelValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            context.Result = new BadRequestObjectResult(new
            {
                Message = "Validation failed",
                Errors = errors,
                ErrorCode = "VALIDATION_ERROR"
            });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
```

### 2. HTML Sanitization
```typescript
// Frontend input sanitization
import DOMPurify from 'dompurify'

export class InputSanitizer {
  // Sanitize HTML content
  static sanitizeHtml(input: string): string {
    return DOMPurify.sanitize(input, {
      ALLOWED_TAGS: ['b', 'i', 'em', 'strong', 'u'],
      ALLOWED_ATTR: [],
      KEEP_CONTENT: true,
      ALLOW_DATA_ATTR: false
    })
  }

  // Remove potential XSS vectors
  static sanitizeText(input: string): string {
    if (!input) return ''
    
    return input
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#x27;')
      .replace(/\//g, '&#x2F;')
      .trim()
  }

  // Validate and sanitize financial amounts
  static sanitizeAmount(input: string): number | null {
    // Remove all non-numeric characters except decimal point
    const cleaned = input.replace(/[^\d.-]/g, '')
    const amount = parseFloat(cleaned)
    
    // Validate amount
    if (isNaN(amount) || amount < 0 || amount > 1000000) {
      return null
    }
    
    // Round to 2 decimal places
    return Math.round(amount * 100) / 100
  }

  // Validate email format
  static isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    return emailRegex.test(email)
  }

  // Check for SQL injection patterns
  static hasSqlInjectionPattern(input: string): boolean {
    const sqlPatterns = [
      /('|(\\')|(;|\\;))/i,
      /((\%27)|(\'))/i,
      /(\%27\%4F\%52)|(\'\s*(or|OR)\s*)/i,
      /((select|SELECT|union|UNION|insert|INSERT|update|UPDATE|delete|DELETE|drop|DROP|create|CREATE|alter|ALTER)\s)/i
    ]
    
    return sqlPatterns.some(pattern => pattern.test(input))
  }
}
```

## HTTPS and Transport Security

### 1. HTTPS Configuration
```csharp
// HTTPS enforcement middleware
public class HttpsRedirectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpsRedirectionMiddleware> _logger;

    public HttpsRedirectionMiddleware(RequestDelegate next, ILogger<HttpsRedirectionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.IsHttps && !IsLocalDevelopment(context))
        {
            var httpsUrl = $"https://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            
            _logger.LogWarning("HTTP request redirected to HTTPS: {OriginalUrl} -> {HttpsUrl}",
                context.Request.GetDisplayUrl(), httpsUrl);
                
            context.Response.Redirect(httpsUrl, permanent: true);
            return;
        }

        // Add security headers
        AddSecurityHeaders(context);
        
        await _next(context);
    }

    private void AddSecurityHeaders(HttpContext context)
    {
        var headers = context.Response.Headers;
        
        // HSTS - HTTP Strict Transport Security
        headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
        
        // Prevent MIME type sniffing
        headers.Add("X-Content-Type-Options", "nosniff");
        
        // XSS Protection
        headers.Add("X-XSS-Protection", "1; mode=block");
        
        // Frame options
        headers.Add("X-Frame-Options", "DENY");
        
        // Referrer policy
        headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        
        // Remove server header
        headers.Remove("Server");
    }

    private bool IsLocalDevelopment(HttpContext context)
    {
        return context.Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
               context.Request.Host.Host.Equals("127.0.0.1");
    }
}
```

### 2. Content Security Policy
```typescript
// CSP configuration
export const contentSecurityPolicy = {
  'default-src': ["'self'"],
  'script-src': [
    "'self'",
    "'unsafe-inline'", // Only for development
    'https://cdnjs.cloudflare.com',
    "'nonce-{nonce}'" // Use nonces in production
  ],
  'style-src': [
    "'self'",
    "'unsafe-inline'", // Required for CSS-in-JS
    'https://fonts.googleapis.com'
  ],
  'font-src': [
    "'self'",
    'https://fonts.gstatic.com'
  ],
  'img-src': [
    "'self'",
    'data:',
    'https:'
  ],
  'connect-src': [
    "'self'",
    'https://api.WahadiniCryptoQuest.com',
    'wss://api.WahadiniCryptoQuest.com'
  ],
  'media-src': ["'none'"],
  'object-src': ["'none'"],
  'base-uri': ["'self'"],
  'form-action': ["'self'"],
  'frame-ancestors': ["'none'"],
  'upgrade-insecure-requests': []
}

// CSP header generation
export const generateCSPHeader = (nonce?: string): string => {
  const policy = Object.entries(contentSecurityPolicy)
    .map(([directive, sources]) => {
      const sourceList = sources
        .map(source => source.replace('{nonce}', nonce || ''))
        .join(' ')
      return `${directive} ${sourceList}`
    })
    .join('; ')
  
  return policy
}
```

## Rate Limiting and DDoS Protection

### 1. Rate Limiting Middleware
```csharp
// Rate limiting service
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly RateLimitSettings _settings;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var endpoint = GetEndpointIdentifier(context);
        var key = $"rate_limit_{clientId}_{endpoint}";

        var rateLimitInfo = _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return new RateLimitInfo
            {
                RequestCount = 0,
                WindowStart = DateTime.UtcNow
            };
        });

        // Reset window if expired
        if (DateTime.UtcNow - rateLimitInfo.WindowStart > TimeSpan.FromMinutes(1))
        {
            rateLimitInfo.RequestCount = 0;
            rateLimitInfo.WindowStart = DateTime.UtcNow;
        }

        rateLimitInfo.RequestCount++;

        var limit = GetRateLimitForEndpoint(endpoint);
        
        if (rateLimitInfo.RequestCount > limit)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}. Count: {Count}",
                clientId, endpoint, rateLimitInfo.RequestCount);

            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.Headers.Add("Retry-After", "60");
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "Rate limit exceeded",
                message = "Too many requests. Please try again later.",
                retryAfter = 60
            }));
            
            return;
        }

        // Add rate limit headers
        context.Response.Headers.Add("X-RateLimit-Limit", limit.ToString());
        context.Response.Headers.Add("X-RateLimit-Remaining", (limit - rateLimitInfo.RequestCount).ToString());
        context.Response.Headers.Add("X-RateLimit-Reset", DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds().ToString());

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Use user ID if authenticated, otherwise IP address
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user_{userId}";
        }

        return $"ip_{context.Connection.RemoteIpAddress}";
    }

    private int GetRateLimitForEndpoint(string endpoint)
    {
        return endpoint switch
        {
            "/api/auth/login" => 5,      // 5 login attempts per minute
            "/api/auth/register" => 3,   // 3 registrations per minute
            "/api/transactions" => 100,  // 100 transaction requests per minute
            _ => 60                      // Default 60 requests per minute
        };
    }
}

public class RateLimitInfo
{
    public int RequestCount { get; set; }
    public DateTime WindowStart { get; set; }
}
```

### 2. Brute Force Protection
```csharp
// Brute force protection service
public class BruteForceProtectionService : IBruteForceProtectionService
{
    private readonly IMemoryCache _cache;
    private readonly SecuritySettings _settings;
    private readonly ILogger<BruteForceProtectionService> _logger;

    public async Task<bool> IsIpAddressBlockedAsync(string ipAddress)
    {
        var key = $"blocked_ip_{ipAddress}";
        return _cache.TryGetValue(key, out _);
    }

    public async Task RecordFailedAttemptAsync(string ipAddress, string endpoint)
    {
        var key = $"failed_attempts_{ipAddress}_{endpoint}";
        
        var attempts = _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            return new FailedAttemptInfo
            {
                Count = 0,
                FirstAttempt = DateTime.UtcNow,
                LastAttempt = DateTime.UtcNow
            };
        });

        attempts.Count++;
        attempts.LastAttempt = DateTime.UtcNow;

        // Block IP if too many failed attempts
        if (attempts.Count >= _settings.MaxFailedAttemptsPerIp)
        {
            var blockKey = $"blocked_ip_{ipAddress}";
            _cache.Set(blockKey, true, TimeSpan.FromHours(1)); // Block for 1 hour

            _logger.LogWarning("IP address {IpAddress} blocked due to {Count} failed attempts on {Endpoint}",
                ipAddress, attempts.Count, endpoint);

            // Optional: Send alert to security team
            await _alertService.SendSecurityAlertAsync(
                "Brute Force Attack Detected",
                $"IP {ipAddress} has been blocked due to {attempts.Count} failed login attempts");
        }
    }

    public async Task ClearFailedAttemptsAsync(string ipAddress, string endpoint)
    {
        var key = $"failed_attempts_{ipAddress}_{endpoint}";
        _cache.Remove(key);
    }
}
```

## Frontend Security

### 1. Secure Token Storage
```typescript
// Secure token storage service
export class SecureTokenStorage {
  private static readonly ACCESS_TOKEN_KEY = 'pf_access_token'
  private static readonly REFRESH_TOKEN_KEY = 'pf_refresh_token'
  private static readonly TOKEN_EXPIRY_KEY = 'pf_token_expiry'

  static setTokens(accessToken: string, refreshToken: string, expiresIn: number): void {
    // Store in httpOnly cookie if possible, otherwise localStorage
    if (this.supportsHttpOnlyCookies()) {
      this.setSecureCookie(this.ACCESS_TOKEN_KEY, accessToken, expiresIn)
      this.setSecureCookie(this.REFRESH_TOKEN_KEY, refreshToken, 30 * 24 * 60 * 60) // 30 days
    } else {
      // Fallback to localStorage with encryption
      const encryptedAccessToken = this.encryptToken(accessToken)
      const encryptedRefreshToken = this.encryptToken(refreshToken)
      
      localStorage.setItem(this.ACCESS_TOKEN_KEY, encryptedAccessToken)
      localStorage.setItem(this.REFRESH_TOKEN_KEY, encryptedRefreshToken)
    }
    
    localStorage.setItem(this.TOKEN_EXPIRY_KEY, (Date.now() + expiresIn * 1000).toString())
  }

  static getAccessToken(): string | null {
    if (this.supportsHttpOnlyCookies()) {
      return this.getCookieValue(this.ACCESS_TOKEN_KEY)
    } else {
      const encryptedToken = localStorage.getItem(this.ACCESS_TOKEN_KEY)
      return encryptedToken ? this.decryptToken(encryptedToken) : null
    }
  }

  static getRefreshToken(): string | null {
    if (this.supportsHttpOnlyCookies()) {
      return this.getCookieValue(this.REFRESH_TOKEN_KEY)
    } else {
      const encryptedToken = localStorage.getItem(this.REFRESH_TOKEN_KEY)
      return encryptedToken ? this.decryptToken(encryptedToken) : null
    }
  }

  static isTokenExpired(): boolean {
    const expiryTime = localStorage.getItem(this.TOKEN_EXPIRY_KEY)
    if (!expiryTime) return true
    
    return Date.now() >= parseInt(expiryTime)
  }

  static clearTokens(): void {
    if (this.supportsHttpOnlyCookies()) {
      this.deleteCookie(this.ACCESS_TOKEN_KEY)
      this.deleteCookie(this.REFRESH_TOKEN_KEY)
    } else {
      localStorage.removeItem(this.ACCESS_TOKEN_KEY)
      localStorage.removeItem(this.REFRESH_TOKEN_KEY)
    }
    
    localStorage.removeItem(this.TOKEN_EXPIRY_KEY)
  }

  private static encryptToken(token: string): string {
    // Simple XOR encryption with device fingerprint
    const key = this.getDeviceFingerprint()
    return btoa(String.fromCharCode(...token.split('').map((char, i) => 
      char.charCodeAt(0) ^ key.charCodeAt(i % key.length))))
  }

  private static decryptToken(encryptedToken: string): string {
    try {
      const key = this.getDeviceFingerprint()
      const encoded = atob(encryptedToken)
      return String.fromCharCode(...encoded.split('').map((char, i) => 
        char.charCodeAt(0) ^ key.charCodeAt(i % key.length)))
    } catch {
      return ''
    }
  }

  private static getDeviceFingerprint(): string {
    // Create a simple device fingerprint
    const canvas = document.createElement('canvas')
    const ctx = canvas.getContext('2d')
    ctx?.fillText('fingerprint', 10, 10)
    
    const fingerprint = [
      navigator.userAgent,
      navigator.language,
      screen.width + 'x' + screen.height,
      new Date().getTimezoneOffset(),
      canvas.toDataURL()
    ].join('|')
    
    return btoa(fingerprint).substring(0, 32)
  }

  private static setSecureCookie(name: string, value: string, maxAge: number): void {
    document.cookie = `${name}=${value}; Max-Age=${maxAge}; Secure; HttpOnly; SameSite=Strict; Path=/`
  }

  private static supportsHttpOnlyCookies(): boolean {
    // Check if we can set httpOnly cookies (server-side only)
    return false // Client-side can't set httpOnly cookies
  }
}
```

### 2. XSS Protection
```typescript
// XSS protection utilities
export class XSSProtection {
  // Escape HTML entities
  static escapeHtml(text: string): string {
    const div = document.createElement('div')
    div.textContent = text
    return div.innerHTML
  }

  // Validate and sanitize URLs
  static sanitizeUrl(url: string): string {
    const allowedProtocols = ['http:', 'https:', 'mailto:', 'tel:']
    
    try {
      const parsedUrl = new URL(url)
      if (!allowedProtocols.includes(parsedUrl.protocol)) {
        return '#'
      }
      return url
    } catch {
      return '#'
    }
  }

  // Sanitize user-generated content
  static sanitizeUserContent(content: string): string {
    return DOMPurify.sanitize(content, {
      ALLOWED_TAGS: ['p', 'br', 'strong', 'em', 'u', 'ul', 'ol', 'li'],
      ALLOWED_ATTR: [],
      FORBID_TAGS: ['script', 'object', 'embed', 'iframe', 'form'],
      FORBID_ATTR: ['onerror', 'onload', 'onclick', 'onmouseover']
    })
  }

  // Create safe innerHTML replacement
  static setSafeInnerHTML(element: HTMLElement, html: string): void {
    const sanitized = this.sanitizeUserContent(html)
    element.innerHTML = sanitized
  }

  // Safe event handler creation
  static createSafeEventHandler(handler: (event: Event) => void) {
    return (event: Event) => {
      // Prevent default and stop propagation for safety
      event.preventDefault()
      event.stopPropagation()
      
      try {
        handler(event)
      } catch (error) {
        console.error('Event handler error:', error)
        // Log to security monitoring
        SecurityMonitor.logSecurityEvent('event_handler_error', { error: error.message })
      }
    }
  }
}
```

## Security Monitoring and Logging

### 1. Security Event Logging
```csharp
// Security event logging service
public class SecurityAuditService : ISecurityAuditService
{
    private readonly ILogger<SecurityAuditService> _logger;
    private readonly IAuditRepository _auditRepository;
    private readonly IEmailService _emailService;

    public async Task LogSecurityEventAsync(SecurityEvent securityEvent)
    {
        // Log to application logs
        _logger.LogWarning("Security Event: {EventType} - {Message} - User: {UserId} - IP: {IpAddress}",
            securityEvent.EventType,
            securityEvent.Message,
            securityEvent.UserId,
            securityEvent.IpAddress);

        // Store in audit table
        var auditLog = new SecurityAuditLog
        {
            Id = Guid.NewGuid(),
            EventType = securityEvent.EventType,
            Message = securityEvent.Message,
            UserId = securityEvent.UserId,
            IpAddress = securityEvent.IpAddress,
            UserAgent = securityEvent.UserAgent,
            RequestPath = securityEvent.RequestPath,
            AdditionalData = JsonSerializer.Serialize(securityEvent.AdditionalData),
            Timestamp = DateTime.UtcNow
        };

        await _auditRepository.CreateAsync(auditLog);

        // Send alerts for critical events
        if (securityEvent.Severity == SecurityEventSeverity.Critical)
        {
            await _emailService.SendSecurityAlertAsync(
                "security-team@WahadiniCryptoQuest.com",
                $"Critical Security Event: {securityEvent.EventType}",
                securityEvent.Message);
        }
    }

    public async Task LogLoginAttemptAsync(string email, string ipAddress, bool successful, string failureReason = null)
    {
        var eventType = successful ? "login_success" : "login_failure";
        var message = successful 
            ? $"Successful login for {email}"
            : $"Failed login attempt for {email}. Reason: {failureReason}";

        var securityEvent = new SecurityEvent
        {
            EventType = eventType,
            Message = message,
            IpAddress = ipAddress,
            Severity = successful ? SecurityEventSeverity.Info : SecurityEventSeverity.Warning,
            AdditionalData = new Dictionary<string, object>
            {
                ["email"] = email,
                ["success"] = successful,
                ["failure_reason"] = failureReason
            }
        };

        await LogSecurityEventAsync(securityEvent);
    }

    public async Task LogPermissionDeniedAsync(Guid userId, string permission, string resource, string ipAddress)
    {
        var securityEvent = new SecurityEvent
        {
            EventType = "permission_denied",
            Message = $"Permission denied for user {userId} attempting to access {resource} with permission {permission}",
            UserId = userId,
            IpAddress = ipAddress,
            Severity = SecurityEventSeverity.Warning,
            AdditionalData = new Dictionary<string, object>
            {
                ["permission"] = permission,
                ["resource"] = resource
            }
        };

        await LogSecurityEventAsync(securityEvent);
    }
}

public class SecurityEvent
{
    public string EventType { get; set; }
    public string Message { get; set; }
    public Guid? UserId { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public string RequestPath { get; set; }
    public SecurityEventSeverity Severity { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

public enum SecurityEventSeverity
{
    Info,
    Warning,
    High,
    Critical
}
```

### 2. Frontend Security Monitoring
```typescript
// Frontend security monitoring
export class SecurityMonitor {
  private static readonly SECURITY_ENDPOINT = '/api/security/events'

  static async logSecurityEvent(eventType: string, data: Record<string, any>): Promise<void> {
    try {
      const securityEvent = {
        eventType,
        timestamp: new Date().toISOString(),
        userAgent: navigator.userAgent,
        url: window.location.href,
        data
      }

      // Send to backend
      await fetch(this.SECURITY_ENDPOINT, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${SecureTokenStorage.getAccessToken()}`
        },
        body: JSON.stringify(securityEvent)
      })
    } catch (error) {
      console.error('Failed to log security event:', error)
    }
  }

  // Monitor for suspicious activity
  static startSecurityMonitoring(): void {
    // Monitor for console access (potential XSS)
    this.monitorConsoleAccess()
    
    // Monitor for suspicious DOM modifications
    this.monitorDOMModifications()
    
    // Monitor for unexpected navigation
    this.monitorNavigation()
  }

  private static monitorConsoleAccess(): void {
    const originalLog = console.log
    console.log = (...args) => {
      this.logSecurityEvent('console_access', {
        message: 'Console access detected',
        args: args.map(arg => typeof arg === 'string' ? arg : '[object]')
      })
      originalLog.apply(console, args)
    }
  }

  private static monitorDOMModifications(): void {
    const observer = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        if (mutation.type === 'childList') {
          mutation.addedNodes.forEach((node) => {
            if (node.nodeType === Node.ELEMENT_NODE) {
              const element = node as Element
              if (element.tagName === 'SCRIPT' && !element.hasAttribute('data-approved')) {
                this.logSecurityEvent('suspicious_script_injection', {
                  tag: element.tagName,
                  src: element.getAttribute('src'),
                  innerHTML: element.innerHTML
                })
              }
            }
          })
        }
      })
    })

    observer.observe(document.body, {
      childList: true,
      subtree: true
    })
  }

  private static monitorNavigation(): void {
    window.addEventListener('beforeunload', (event) => {
      // Check if navigation is expected
      if (!this.isExpectedNavigation()) {
        this.logSecurityEvent('unexpected_navigation', {
          from: window.location.href,
          userInitiated: event.isTrusted
        })
      }
    })
  }

  private static isExpectedNavigation(): boolean {
    // Implement logic to determine if navigation is expected
    // This could check for user interactions, form submissions, etc.
    return true
  }
}
```

## Best Practices

### 1. Security Development Lifecycle
- **Threat Modeling**: Identify potential security threats for each feature
- **Security Code Review**: Review all code changes for security implications
- **Penetration Testing**: Regular security testing by external experts
- **Vulnerability Scanning**: Automated scanning for known vulnerabilities

### 2. Data Protection
- **Encryption at Rest**: Encrypt sensitive data in database
- **Encryption in Transit**: Use TLS 1.3 for all communications
- **Data Minimization**: Only collect and store necessary data
- **Data Retention**: Implement data retention and deletion policies

### 3. Incident Response
- **Security Monitoring**: Real-time monitoring for security events
- **Incident Response Plan**: Documented procedures for security incidents
- **Forensic Logging**: Comprehensive logging for incident investigation
- **Recovery Procedures**: Plans for system recovery after incidents

### 4. Compliance
- **PCI DSS**: If processing payments, ensure PCI compliance
- **GDPR**: Ensure compliance with data protection regulations
- **SOX**: If publicly traded, ensure Sarbanes-Oxley compliance
- **Regular Audits**: Schedule regular security audits

## Instructions

When implementing security for the WahadiniCryptoQuest:

1. **Always Validate Input**: Validate and sanitize all inputs on both client and server
2. **Use HTTPS Everywhere**: Ensure all communications use TLS encryption
3. **Implement RBAC**: Use role-based access control for all sensitive operations
4. **Log Security Events**: Log all authentication, authorization, and security events
5. **Protect user data and learning progress**: Apply extra security measures for financial information
6. **Monitor Continuously**: Implement real-time security monitoring
7. **Test Regularly**: Conduct regular security testing and penetration testing
8. **Update Dependencies**: Keep all dependencies updated with security patches
9. **Follow OWASP**: Implement OWASP Top 10 security controls
10. **Plan for Incidents**: Have incident response procedures ready

Always consider the financial nature of the application and implement security measures appropriate for handling sensitive user data and learning progress and personally identifiable information.
