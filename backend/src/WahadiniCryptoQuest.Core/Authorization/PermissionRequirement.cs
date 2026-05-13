using Microsoft.AspNetCore.Authorization;

namespace WahadiniCryptoQuest.Core.Authorization;

/// <summary>
/// Authorization requirement for permission-based access control
/// Requires user to have a specific permission
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Required permission name
    /// </summary>
    public string Permission { get; }

    /// <summary>
    /// Whether email confirmation is required
    /// </summary>
    public bool RequireEmailConfirmation { get; }

    public PermissionRequirement(string permission, bool requireEmailConfirmation = true)
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw new ArgumentException("Permission cannot be null or empty", nameof(permission));

        Permission = permission;
        RequireEmailConfirmation = requireEmailConfirmation;
    }
}
