using Microsoft.AspNetCore.Authorization;

namespace WahadiniCryptoQuest.API.Authorization;

/// <summary>
/// Authorization requirement for permission-based access control with AND logic
/// Used with RequireAllPermissionsAttribute to enforce compound permissions
/// </summary>
public class AllPermissionsAuthorizationRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Creates an authorization requirement for ALL permissions (AND logic)
    /// </summary>
    /// <param name="permissions">All permissions required (AND logic)</param>
    public AllPermissionsAuthorizationRequirement(params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
        {
            throw new ArgumentException("At least one permission must be specified", nameof(permissions));
        }

        Permissions = permissions;
    }

    /// <summary>
    /// The permissions required by this requirement
    /// ALL permissions must be present for authorization to succeed (AND logic)
    /// </summary>
    public string[] Permissions { get; }
}

/// <summary>
/// Authorization handler for compound permission-based access control with AND logic
/// Validates that the user has ALL of the required permissions in their JWT claims
/// </summary>
public class AllPermissionsAuthorizationHandler : AuthorizationHandler<AllPermissionsAuthorizationRequirement>
{
    /// <summary>
    /// Validates that the user has ALL of the required permissions (AND logic)
    /// </summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AllPermissionsAuthorizationRequirement requirement)
    {
        // Get all permission claims from the user's JWT token
        var userPermissions = context.User.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Check if user has ALL of the required permissions (AND logic)
        var hasAllRequiredPermissions = requirement.Permissions
            .All(requiredPermission => userPermissions.Contains(requiredPermission));

        if (hasAllRequiredPermissions)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
