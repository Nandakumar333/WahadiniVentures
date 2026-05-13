using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using WahadiniCryptoQuest.Core.Authorization;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Service.Handlers.Authorization;

/// <summary>
/// Authorization handler for subscription-based access control
/// Validates user has required subscription tier and email confirmation
/// </summary>
public class SubscriptionRequirementHandler : AuthorizationHandler<SubscriptionRequirement>
{
    private readonly ILogger<SubscriptionRequirementHandler> _logger;

    public SubscriptionRequirementHandler(ILogger<SubscriptionRequirementHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SubscriptionRequirement requirement)
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

        // Get user role
        var roleClaim = context.User.FindFirst(ClaimTypes.Role) 
                       ?? context.User.FindFirst("role");
        
        if (roleClaim == null)
        {
            _logger.LogWarning("Authorization failed: Role claim not found for user {UserId}", userIdClaim.Value);
            return Task.CompletedTask;
        }

        // Parse and validate role
        if (!Enum.TryParse<UserRoleEnum>(roleClaim.Value, out var userRole))
        {
            _logger.LogWarning("Authorization failed: Invalid role value '{Role}' for user {UserId}", 
                roleClaim.Value, userIdClaim.Value);
            return Task.CompletedTask;
        }

        // Check if user role meets or exceeds requirement
        // Admin (2) > Premium (1) > Free (0)
        if ((int)userRole >= (int)requirement.RequiredRole)
        {
            _logger.LogInformation("Authorization succeeded: User {UserId} has role {UserRole} (required: {RequiredRole})",
                userIdClaim.Value, userRole, requirement.RequiredRole);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Authorization failed: User {UserId} has role {UserRole} (required: {RequiredRole})",
                userIdClaim.Value, userRole, requirement.RequiredRole);
        }

        return Task.CompletedTask;
    }
}
