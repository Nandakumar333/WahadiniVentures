using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Services;

/// <summary>
/// Service for authorization and permission checks with caching support
/// Implements business logic for role and permission validation
/// Part of Service/Application layer
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuthorizationService> _logger;

    // Cache configuration
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private const string UserRoleCacheKeyPrefix = "user_role_";
    private const string UserPermissionsCacheKeyPrefix = "user_permissions_";

    public AuthorizationService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IUserRoleRepository userRoleRepository,
        IMemoryCache cache,
        ILogger<AuthorizationService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _userRoleRepository = userRoleRepository;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Checks if a user has a specific permission
    /// Uses role-based permissions (Admin has all, Premium has premium features, Free has basic)
    /// </summary>
    public async Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default)
    {
        var userRole = await GetUserRoleAsync(userId, cancellationToken);
        if (!userRole.HasValue)
        {
            _logger.LogWarning("Permission check failed: User {UserId} not found", userId);
            return false;
        }

        var permissions = await GetUserPermissionsAsync(userId, cancellationToken);
        var hasPermission = permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);

        _logger.LogDebug("Permission check for user {UserId}: {Permission} = {HasPermission}", 
            userId, permission, hasPermission);

        return hasPermission;
    }

    /// <summary>
    /// Checks if a user has a specific role
    /// </summary>
    public async Task<bool> HasRoleAsync(Guid userId, UserRoleEnum role, CancellationToken cancellationToken = default)
    {
        var userRole = await GetUserRoleAsync(userId, cancellationToken);
        if (!userRole.HasValue)
        {
            _logger.LogWarning("Role check failed: User {UserId} not found", userId);
            return false;
        }

        var hasRole = userRole.Value == role;
        _logger.LogDebug("Role check for user {UserId}: {Role} = {HasRole}", userId, role, hasRole);

        return hasRole;
    }

    /// <summary>
    /// Checks if a user has any of the specified roles
    /// </summary>
    public async Task<bool> HasAnyRoleAsync(Guid userId, IEnumerable<UserRoleEnum> roles, CancellationToken cancellationToken = default)
    {
        var userRole = await GetUserRoleAsync(userId, cancellationToken);
        if (!userRole.HasValue)
        {
            _logger.LogWarning("Role check failed: User {UserId} not found", userId);
            return false;
        }

        var hasAnyRole = roles.Contains(userRole.Value);
        _logger.LogDebug("Any role check for user {UserId}: Has any of [{Roles}] = {HasAnyRole}", 
            userId, string.Join(", ", roles), hasAnyRole);

        return hasAnyRole;
    }

    /// <summary>
    /// Checks if a user's subscription is active and meets the required tier
    /// Admin (2) > Premium (1) > Free (0)
    /// </summary>
    public async Task<bool> IsSubscriptionActiveAsync(Guid userId, UserRoleEnum requiredRole, CancellationToken cancellationToken = default)
    {
        var userRole = await GetUserRoleAsync(userId, cancellationToken);
        if (!userRole.HasValue)
        {
            _logger.LogWarning("Subscription check failed: User {UserId} not found", userId);
            return false;
        }

        // Check if user's role meets or exceeds required role
        var meetsRequirement = (int)userRole.Value >= (int)requiredRole;

        _logger.LogDebug("Subscription check for user {UserId}: Role {UserRole} (required: {RequiredRole}) = {MeetsRequirement}",
            userId, userRole.Value, requiredRole, meetsRequirement);

        return meetsRequirement;
    }

    /// <summary>
    /// Gets the user's current role with caching
    /// Cache duration: 5 minutes
    /// </summary>
    public async Task<UserRoleEnum?> GetUserRoleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{UserRoleCacheKeyPrefix}{userId}";

        // Try to get from cache
        if (_cache.TryGetValue(cacheKey, out UserRoleEnum cachedRole))
        {
            _logger.LogDebug("User role retrieved from cache for user {UserId}", userId);
            return cachedRole;
        }

        // Get from database
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return null;
        }

        // Cache the role
        _cache.Set(cacheKey, user.Role, CacheDuration);
        _logger.LogDebug("User role cached for user {UserId}: {Role}", userId, user.Role);

        return user.Role;
    }

    /// <summary>
    /// Gets all permissions for a user based on their role with caching
    /// Cache duration: 5 minutes
    /// Loads permissions from database via Role-Permission assignments
    /// </summary>
    public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{UserPermissionsCacheKeyPrefix}{userId}";

        // Try to get from cache
        if (_cache.TryGetValue(cacheKey, out IEnumerable<string>? cachedPermissions) && cachedPermissions != null)
        {
            _logger.LogDebug("User permissions retrieved from cache for user {UserId}", userId);
            return cachedPermissions;
        }

        // Get active user role assignment
        var userRole = await _userRoleRepository.GetActiveUserRoleAsync(userId, cancellationToken);
        if (userRole == null || userRole.Role == null)
        {
            _logger.LogWarning("Cannot get permissions: No active role for user {UserId}", userId);
            return Enumerable.Empty<string>();
        }

        // Get permissions from database for the role
        var permissions = await _permissionRepository.GetPermissionsByRoleIdAsync(userRole.RoleId, cancellationToken);
        var permissionNames = permissions.Select(p => p.Name).ToList();

        // Cache the permissions
        _cache.Set(cacheKey, permissionNames, CacheDuration);
        _logger.LogDebug("User permissions cached for user {UserId}: {PermissionCount} permissions", 
            userId, permissionNames.Count);

        return permissionNames;
    }

    /// <summary>
    /// Gets all role names assigned to a user from UserRole table
    /// </summary>
    public async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all active user role assignments
            var userRoles = await _userRoleRepository.GetUserRolesAsync(userId, cancellationToken);
            if (userRoles == null || !userRoles.Any())
            {
                // Fallback to user's base role if no UserRole assignments exist
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    return new[] { user.Role.ToString() };
                }
                
                _logger.LogWarning("No roles found for user {UserId}", userId);
                return Enumerable.Empty<string>();
            }

            var roleNames = userRoles
                .Where(ur => ur.Role != null)
                .Select(ur => ur.Role!.Name)
                .ToList();

            _logger.LogDebug("Retrieved {RoleCount} roles for user {UserId}", roleNames.Count, userId);
            return roleNames;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for user {UserId}", userId);
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// Invalidates cached permissions for a user
    /// Should be called when user's role or permissions change
    /// </summary>
    public void InvalidateUserPermissionsCache(Guid userId)
    {
        var cacheKey = $"{UserPermissionsCacheKeyPrefix}{userId}";
        _cache.Remove(cacheKey);
        _logger.LogDebug("Invalidated permission cache for user {UserId}", userId);
    }
}
