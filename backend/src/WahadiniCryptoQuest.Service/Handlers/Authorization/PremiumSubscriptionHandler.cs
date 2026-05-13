using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Authorization;

namespace WahadiniCryptoQuest.Service.Handlers.Authorization;

/// <summary>
/// Handler for premium subscription requirement
/// </summary>
public class PremiumSubscriptionHandler : AuthorizationHandler<PremiumSubscriptionRequirement>
{
    private readonly ILogger<PremiumSubscriptionHandler> _logger;

    public PremiumSubscriptionHandler(ILogger<PremiumSubscriptionHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PremiumSubscriptionRequirement requirement)
    {
        var subscriptionClaim = context.User.FindFirst("subscription_tier");
        
        if (subscriptionClaim != null && 
            (subscriptionClaim.Value == "Premium" || subscriptionClaim.Value == "Elite"))
        {
            _logger.LogInformation("Premium subscription authorization succeeded");
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Premium subscription authorization failed: Insufficient subscription tier");
        }

        return Task.CompletedTask;
    }
}
