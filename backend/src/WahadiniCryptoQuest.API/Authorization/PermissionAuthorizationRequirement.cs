using Microsoft.AspNetCore.Authorization;

namespace WahadiniCryptoQuest.API.Authorization;

/// <summary>
/// Authorization requirement for permission-based access control
/// Used with RequirePermissionAttribute to enforce fine-grained permissions
/// </summary>
public class PermissionAuthorizationRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Creates a permission requirement
    /// </summary>
    /// <param name="permissions">One or more permissions required (OR logic)</param>
    public PermissionAuthorizationRequirement(params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
        {
            throw new ArgumentException("At least one permission must be specified", nameof(permissions));
        }

        Permissions = permissions;
    }

    /// <summary>
    /// The permissions required by this requirement
    /// If multiple permissions are specified, ANY match will satisfy the requirement (OR logic)
    /// </summary>
    public string[] Permissions { get; }
}

/// <summary>
/// Authorization handler for permission-based access control
/// Validates that the user has at least one of the required permissions in their JWT claims
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionAuthorizationRequirement>
{
    /// <summary>
    /// Validates that the user has at least one of the required permissions
    /// </summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionAuthorizationRequirement requirement)
    {
        // Get all permission claims from the user's JWT token
        var userPermissions = context.User.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Check if user has ANY of the required permissions (OR logic)
        var hasRequiredPermission = requirement.Permissions
            .Any(requiredPermission => userPermissions.Contains(requiredPermission));

        if (hasRequiredPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
