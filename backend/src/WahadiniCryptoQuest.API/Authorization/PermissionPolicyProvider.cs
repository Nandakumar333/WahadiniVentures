using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace WahadiniCryptoQuest.API.Authorization;

/// <summary>
/// Custom authorization policy provider that creates dynamic permission-based policies
/// This allows RequirePermission and RequireAllPermissions attributes to work with specific permissions
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // If this is a standard policy, use the fallback provider
        if (!policyName.StartsWith("RequirePermission", StringComparison.OrdinalIgnoreCase) &&
            !policyName.StartsWith("RequireAllPermissions", StringComparison.OrdinalIgnoreCase))
        {
            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }

        // For permission-based policies, the actual enforcement happens through
        // the IAuthorizationFilter implementation in the attributes
        // Just return a policy that requires authentication
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}
