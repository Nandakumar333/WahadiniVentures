using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Interfaces.Services;

/// <summary>
/// Service for authorization and permission checks
/// Provides methods to validate user permissions, roles, and subscription status
/// Part of Core layer - defines authorization contracts
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Checks if a user has a specific permission
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="permission">The permission to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user has the permission, false otherwise</returns>
    Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has a specific role
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="role">The role to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user has the role, false otherwise</returns>
    Task<bool> HasRoleAsync(Guid userId, UserRoleEnum role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has any of the specified roles
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="roles">The roles to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user has any of the roles, false otherwise</returns>
    Task<bool> HasAnyRoleAsync(Guid userId, IEnumerable<UserRoleEnum> roles, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user's subscription is active and meets the required tier
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="requiredRole">The minimum required subscription role</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user's subscription is active and meets requirement, false otherwise</returns>
    Task<bool> IsSubscriptionActiveAsync(Guid userId, UserRoleEnum requiredRole, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the user's current role
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user's role, or null if user not found</returns>
    Task<UserRoleEnum?> GetUserRoleAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions for a user based on their role
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of permission names</returns>
    Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all role names assigned to a user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of role names</returns>
    Task<IEnumerable<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates cached permissions for a user
    /// Should be called when user's role or permissions change
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    void InvalidateUserPermissionsCache(Guid userId);
}
