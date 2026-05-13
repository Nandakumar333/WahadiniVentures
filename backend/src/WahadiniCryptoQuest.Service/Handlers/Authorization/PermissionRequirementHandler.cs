using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using WahadiniCryptoQuest.Core.Authorization;

namespace WahadiniCryptoQuest.Service.Handlers.Authorization;

/// <summary>
/// Authorization handler for permission-based access control
/// Validates user has required permission through claims
/// </summary>
public class PermissionRequirementHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionRequirementHandler> _logger;

    public PermissionRequirementHandler(ILogger<PermissionRequirementHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Get user ID claim
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            _logger.LogWarning("Authorization failed: User ID claim not found");
            return Task.CompletedTask;
        }

        // Check email confirmation if required
        if (requirement.RequireEmailConfirmation)
        {
            var emailVerifiedClaim = context.User.FindFirst("email_verified");
            if (emailVerifiedClaim?.Value != "true")
            {
                _logger.LogWarning("Authorization failed: Email not confirmed for user {UserId}", userIdClaim.Value);
                return Task.CompletedTask;
            }
        }

        // Check if user has the required permission claim
        var hasPermission = context.User.Claims.Any(c => 
            c.Type == "permission" && c.Value == requirement.Permission);

        if (hasPermission)
        {
            _logger.LogInformation("Authorization succeeded: User {UserId} has permission '{Permission}'",
                userIdClaim.Value, requirement.Permission);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Authorization failed: User {UserId} missing permission '{Permission}'",
                userIdClaim.Value, requirement.Permission);
        }

        return Task.CompletedTask;
    }
}
