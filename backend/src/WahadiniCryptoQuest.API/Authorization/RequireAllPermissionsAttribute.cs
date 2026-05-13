using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WahadiniCryptoQuest.API.Authorization;

/// <summary>
/// Custom authorization attribute that requires ALL specified permissions (AND logic)
/// Usage: [RequireAllPermissions("courses:read", "courses:publish")]
/// All permissions must be present for authorization to succeed
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireAllPermissionsAttribute : Attribute, IAuthorizationFilter
{
    /// <summary>
    /// Creates a permission requirement for ALL specified permissions (AND logic)
    /// </summary>
    /// <param name="permissions">All permissions required (AND logic - all must match)</param>
    public RequireAllPermissionsAttribute(params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
        {
            throw new ArgumentException("At least one permission must be specified", nameof(permissions));
        }

        if (permissions.Length == 1)
        {
            throw new ArgumentException(
                "RequireAllPermissions requires multiple permissions. Use RequirePermission for single permission checks.",
                nameof(permissions));
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
    }

    /// <summary>
    /// The permissions required by this attribute (all must be present)
    /// </summary>
    public string[] Permissions { get; }

    /// <summary>
    /// Authorization filter implementation - checks user has ALL permissions (AND logic)
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

        // Check if user has ALL of the required permissions (AND logic)
        var hasAllRequiredPermissions = Permissions
            .All(requiredPermission => userPermissions.Contains(requiredPermission));

        if (!hasAllRequiredPermissions)
        {
            // Find which permissions are missing
            var missingPermissions = Permissions
                .Where(p => !userPermissions.Contains(p))
                .ToArray();

            context.Result = new ObjectResult(new
            {
                error = "Insufficient permissions",
                message = $"You are missing required permission(s): {string.Join(", ", missingPermissions)}",
                requiredPermissions = Permissions,
                missingPermissions
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
