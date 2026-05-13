using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for UserRole entity
/// Provides data access methods for user-role assignments
/// Part of Core layer - defines repository contracts
/// </summary>
public interface IUserRoleRepository
{
    /// <summary>
    /// Gets a user role assignment by its unique identifier
    /// </summary>
    Task<UserRole?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles assigned to a user
    /// </summary>
    Task<IEnumerable<UserRole>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active role for a user (non-expired)
    /// </summary>
    Task<UserRole?> GetActiveUserRoleAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users with a specific role
    /// </summary>
    Task<IEnumerable<UserRole>> GetUsersWithRoleAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a role to a user
    /// </summary>
    Task<UserRole> AssignRoleAsync(UserRole userRole, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user role assignment (e.g., extend expiration)
    /// </summary>
   Task UpdateAsync(UserRole userRole, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a role from a user (soft delete)
    /// </summary>
   Task RemoveRoleAsync(Guid userRoleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has a specific role
    /// </summary>
    Task<bool> UserHasRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets expired role assignments that need cleanup
    /// </summary>
    Task<IEnumerable<UserRole>> GetExpiredRolesAsync(CancellationToken cancellationToken = default);
}
