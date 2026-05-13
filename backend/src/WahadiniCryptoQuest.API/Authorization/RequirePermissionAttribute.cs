using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WahadiniCryptoQuest.API.Authorization;

/// <summary>
/// Custom authorization attribute that requires specific permissions
/// Usage: [RequirePermission("courses:manage")]
/// Supports multiple permissions with OR logic (any permission matches)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    /// <summary>
    /// Creates a permission requirement for the specified permission(s)
    /// </summary>
    /// <param name="permissions">One or more permissions required (OR logic - any matches)</param>
    public RequirePermissionAttribute(params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
        {
            throw new ArgumentException("At least one permission must be specified", nameof(permissions));
        }

        // Validate permission format: resource:action
        foreach (var permission in permissions)
        {
            if (string.IsNullOrWhiteSpace(permission) || !permission.Contains(':'))
            {
                throw new ArgumentException(
                    $"Permission '{permission}' must follow 'resource:action' format (e.g., 'courses:view')",
                    nameof(permissions));
            }
        }

        // Store permissions for later use
        Permissions = permissions;
        Policy = "RequirePermission"; // For backwards compatibility with tests
    }

    /// <summary>
    /// The permissions required by this attribute
    /// </summary>
    public string[] Permissions { get; }

    /// <summary>
    /// Policy name (for backwards compatibility)
    /// </summary>
    public string Policy { get; }

    /// <summary>
    /// Authorization filter implementation - checks user permissions
    /// </summary>
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Check if user is authenticated
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Authentication required",
                message = "You must be logged in to access this resource"
            });
            return;
        }

        // Get all permission claims from the user's JWT token
        var userPermissions = context.HttpContext.User.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Check if user has ANY of the required permissions (OR logic)
        var hasRequiredPermission = Permissions
            .Any(requiredPermission => userPermissions.Contains(requiredPermission));

        if (!hasRequiredPermission)
        {
            context.Result = new ObjectResult(new
            {
                error = "Insufficient permissions",
                message = $"You do not have the required permission(s): {string.Join(", ", Permissions)}",
                requiredPermissions = Permissions
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
