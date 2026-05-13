using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Authorization;

namespace WahadiniCryptoQuest.Service.Handlers.Authorization;

/// <summary>
/// Handler for email confirmation requirement
/// </summary>
public class EmailConfirmedHandler : AuthorizationHandler<EmailConfirmedRequirement>
{
    private readonly ILogger<EmailConfirmedHandler> _logger;

    public EmailConfirmedHandler(ILogger<EmailConfirmedHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EmailConfirmedRequirement requirement)
    {
        var emailConfirmedClaim = context.User.FindFirst("email_verified");
        
        if (emailConfirmedClaim != null && 
            bool.TryParse(emailConfirmedClaim.Value, out var isConfirmed) && 
            isConfirmed)
        {
            _logger.LogInformation("Email confirmed authorization succeeded");
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Email confirmed authorization failed: Email not verified");
        }

        return Task.CompletedTask;
    }
}
