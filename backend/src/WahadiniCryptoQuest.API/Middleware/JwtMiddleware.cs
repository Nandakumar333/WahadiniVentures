using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.API.Middleware;

/// <summary>
/// JWT middleware for validating and extracting user information from JWT tokens
/// Handles token validation and sets user context for authorized requests
/// </summary>
public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtMiddleware> _logger;

    public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IJwtTokenService jwtTokenService)
    {
        try
        {
            // Skip JWT processing for endpoints that explicitly allow anonymous access
            var endpoint = context.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null;
            
            if (!allowAnonymous)
            {
                var token = ExtractTokenFromRequest(context.Request);
                
                if (!string.IsNullOrEmpty(token))
                {
                    await ValidateTokenAndSetUserContext(context, jwtTokenService, token);
                }
                else
                {
                    // No token provided for a protected endpoint
                    _logger.LogDebug("No authorization token provided for protected endpoint: {Path}", 
                        context.Request.Path);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "JWT middleware encountered an error processing token");
            // Don't block the request - let the authorization attribute handle it
        }

        await _next(context);
    }

    /// <summary>
    /// Extracts JWT token from Authorization header
    /// Supports Bearer token format: "Bearer {token}"
    /// </summary>
    /// <param name="request">HTTP request containing authorization header</param>
    /// <returns>JWT token string or null if not found</returns>
    private static string? ExtractTokenFromRequest(HttpRequest request)
    {
        if (request.Headers.TryGetValue("Authorization", out StringValues authHeader))
        {
            var authHeaderValue = authHeader.FirstOrDefault();
            
            if (!string.IsNullOrEmpty(authHeaderValue) && 
                authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeaderValue.Substring("Bearer ".Length).Trim();
            }
        }

        return null;
    }

    /// <summary>
    /// Validates JWT token and sets user context in HttpContext
    /// Extracts user claims and makes them available for authorization
    /// </summary>
    /// <param name="context">HTTP context to update with user information</param>
    /// <param name="jwtTokenService">JWT service for token validation</param>
    /// <param name="token">JWT token to validate</param>
    private async Task ValidateTokenAndSetUserContext(
        HttpContext context, 
        IJwtTokenService jwtTokenService, 
        string token)
    {
        try
        {
            // Validate the token and get claims principal
            var principal = await jwtTokenService.ValidateAccessTokenAsync(token);
            
            if (principal != null && principal.Identity?.IsAuthenticated == true)
            {
                // Set the user context
                context.User = principal;
                
                _logger.LogDebug("JWT token validated successfully for user: {UserId}", 
                    principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            }
            else
            {
                _logger.LogDebug("Invalid JWT token provided");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error validating JWT token");
            // Don't set user context for invalid tokens
        }
    }
}

/// <summary>
/// Extension methods for registering JWT middleware
/// </summary>
public static class JwtMiddlewareExtensions
{
    /// <summary>
    /// Registers JWT middleware in the application pipeline
    /// Should be called after authentication middleware
    /// </summary>
    /// <param name="builder">Application builder</param>
    /// <returns>Application builder for method chaining</returns>
    public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtMiddleware>();
    }
}