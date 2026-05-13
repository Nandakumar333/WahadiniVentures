using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Role entity
/// Provides data access methods for role management
/// Part of Core layer - defines repository contracts
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Gets a role by its unique identifier
    /// </summary>
   Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by its name
    /// </summary>
   Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles
    /// </summary>
   Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active roles
    /// </summary>
   Task<IEnumerable<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets roles with their permissions
    /// </summary>
   Task<Role?> GetRoleWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new role
    /// </summary>
   Task<Role> CreateAsync(Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing role
    /// </summary>
   Task UpdateAsync(Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role (soft delete)
    /// </summary>
   Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a role exists by name
    /// </summary>
   Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
}
