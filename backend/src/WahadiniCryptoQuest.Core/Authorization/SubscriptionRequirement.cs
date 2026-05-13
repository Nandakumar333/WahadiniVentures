using Microsoft.AspNetCore.Authorization;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Authorization;

/// <summary>
/// Authorization requirement for subscription-based access control
/// Requires user to have an active subscription at the specified tier or higher
/// </summary>
public class SubscriptionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Minimum required subscription tier
    /// </summary>
    public UserRoleEnum RequiredRole { get; }

    /// <summary>
    /// Whether email confirmation is required
    /// </summary>
    public bool RequireEmailConfirmation { get; }

    public SubscriptionRequirement(UserRoleEnum requiredRole, bool requireEmailConfirmation = true)
    {
        RequiredRole = requiredRole;
        RequireEmailConfirmation = requireEmailConfirmation;
    }
}
