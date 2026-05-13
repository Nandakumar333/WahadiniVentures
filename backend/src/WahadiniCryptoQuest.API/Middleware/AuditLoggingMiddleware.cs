using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.API.Middleware;

/// <summary>
/// Middleware to audit administrative actions by capturing HTTP requests to admin endpoints.
/// Logs POST/PUT/DELETE operations with before/after state for compliance and security.
/// T009: Audit logging for /api/admin/* endpoints
/// </summary>
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    // HTTP methods to audit
    private static readonly string[] AuditableMethods = { "POST", "PUT", "DELETE" };

    // Admin endpoint prefix
    private const string AdminPathPrefix = "/api/admin";

    public AuditLoggingMiddleware(
        RequestDelegate next,
        ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
    {
        // Only audit admin endpoints with state-changing methods
        if (!ShouldAudit(context))
        {
            await _next(context);
            return;
        }

        // Extract admin user ID from JWT claims
        var adminUserId = GetAdminUserId(context);
        if (!adminUserId.HasValue)
        {
            _logger.LogWarning("Admin endpoint accessed without valid admin user ID: {Path}", context.Request.Path);
            await _next(context);
            return;
        }

        // Capture request body for "before" state
        var requestBody = await ReadRequestBodyAsync(context.Request);

        // Capture response body for "after" state
        var originalResponseBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            // Execute the next middleware
            await _next(context);

            // Only log successful operations (2xx status codes)
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                var responseBody = await ReadResponseBodyAsync(responseBodyStream);

                // Extract resource type and ID from path (e.g., /api/admin/users/{id})
                var (resourceType, resourceId) = ExtractResourceInfo(context.Request.Path);

                // Get client IP address
                var ipAddress = GetClientIpAddress(context);

                // Log the audit entry asynchronously
                await auditLogService.LogActionAsync(
                    adminUserId: adminUserId.Value,
                    actionType: context.Request.Method,
                    resourceType: resourceType,
                    resourceId: resourceId,
                    beforeValue: requestBody,
                    afterValue: responseBody,
                    ipAddress: ipAddress
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in audit logging middleware for {Path}", context.Request.Path);
            throw; // Re-throw to be handled by global exception handler
        }
        finally
        {
            // Copy response body back to original stream
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalResponseBodyStream);
            context.Response.Body = originalResponseBodyStream;
        }
    }

    /// <summary>
    /// Determines if the request should be audited based on path and method.
    /// </summary>
    private static bool ShouldAudit(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments(AdminPathPrefix, StringComparison.OrdinalIgnoreCase)
               && AuditableMethods.Contains(context.Request.Method, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Extracts the admin user ID from JWT claims.
    /// </summary>
    private static Guid? GetAdminUserId(HttpContext context)
    {
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return null;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Reads the request body as string for audit logging.
    /// </summary>
    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();

        using var reader = new StreamReader(
            request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0; // Reset for next middleware

        return body;
    }

    /// <summary>
    /// Reads the response body as string for audit logging.
    /// </summary>
    private static async Task<string> ReadResponseBodyAsync(Stream responseBodyStream)
    {
        responseBodyStream.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(responseBodyStream, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        responseBodyStream.Seek(0, SeekOrigin.Begin); // Reset for copying

        return body;
    }

    /// <summary>
    /// Extracts resource type and ID from admin API path.
    /// Example: /api/admin/users/123 -> ("users", "123")
    /// </summary>
    private static (string ResourceType, string ResourceId) ExtractResourceInfo(PathString path)
    {
        var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

        // Expected format: ["api", "admin", "resourceType", "resourceId"]
        if (segments.Length >= 4)
        {
            var resourceType = segments[2]; // e.g., "users"
            var resourceId = segments[3];   // e.g., "123"
            return (resourceType, resourceId);
        }
        else if (segments.Length == 3)
        {
            // Collection endpoint (e.g., POST /api/admin/users)
            var resourceType = segments[2];
            return (resourceType, "collection");
        }

        return ("unknown", "unknown");
    }

    /// <summary>
    /// Gets the client IP address from the request, considering proxy headers.
    /// </summary>
    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded IP (when behind reverse proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For can contain multiple IPs, take the first (original client)
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            return ips[0].Trim();
        }

        // Fallback to remote IP address
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
