using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace WahadiniCryptoQuest.Service.Handlers.Authorization;

/// <summary>
/// Generic role-based authorization handler
/// Validates user has one of the required roles
/// </summary>
public class RoleHandler : AuthorizationHandler<IAuthorizationRequirement>
{
    private readonly ILogger<RoleHandler> _logger;

    public RoleHandler(ILogger<RoleHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        IAuthorizationRequirement requirement)
    {
        // This handler only processes RolesAuthorizationRequirement
        if (requirement is not Microsoft.AspNetCore.Authorization.Infrastructure.RolesAuthorizationRequirement rolesRequirement)
        {
            return Task.CompletedTask;
        }

        // Get user ID claim
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            _logger.LogWarning("Authorization failed: User ID claim not found");
            return Task.CompletedTask;
        }

        // Get user roles
        var userRoles = context.User.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value)
            .ToList();

        if (!userRoles.Any())
        {
            _logger.LogWarning("Authorization failed: No role claims found for user {UserId}", userIdClaim.Value);
            return Task.CompletedTask;
        }

        // Check if user has any of the required roles
        var hasRequiredRole = rolesRequirement.AllowedRoles
            .Any(allowedRole => userRoles.Contains(allowedRole, StringComparer.OrdinalIgnoreCase));

        if (hasRequiredRole)
        {
            _logger.LogInformation("Authorization succeeded: User {UserId} has one of required roles: [{RequiredRoles}]",
                userIdClaim.Value, string.Join(", ", rolesRequirement.AllowedRoles));
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Authorization failed: User {UserId} with roles [{UserRoles}] missing required roles: [{RequiredRoles}]",
                userIdClaim.Value, string.Join(", ", userRoles), string.Join(", ", rolesRequirement.AllowedRoles));
        }

        return Task.CompletedTask;
    }
}
