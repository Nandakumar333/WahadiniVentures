using Microsoft.AspNetCore.Authorization;

namespace WahadiniCryptoQuest.Core.Authorization;

/// <summary>
/// Custom authorization policy names
/// </summary>
public static class PolicyNames
{
    public const string RequireAdminRole = "RequireAdminRole";
    public const string RequirePremiumSubscription = "RequirePremiumSubscription";
    public const string RequireEmailConfirmed = "RequireEmailConfirmed";
}

/// <summary>
/// Requirement for email confirmation
/// </summary>
public class EmailConfirmedRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Requirement for premium subscription
/// </summary>
public class PremiumSubscriptionRequirement : IAuthorizationRequirement
{
}
