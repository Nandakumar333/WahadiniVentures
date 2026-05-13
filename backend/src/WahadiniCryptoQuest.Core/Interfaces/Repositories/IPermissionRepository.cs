using WahadiniCryptoQuest.Core.Entities;
using System.Threading.Tasks;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Permission entity
/// Provides data access methods for permission management
/// Part of Core layer - defines repository contracts
/// </summary>
public interface IPermissionRepository
{
    /// <summary>
    /// Gets a permission by its unique identifier
    /// </summary>
   Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a permission by its name
    /// </summary>
   Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions
    /// </summary>
   Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets permissions by resource
    /// </summary>
   Task<IEnumerable<Permission>> GetByResourceAsync(string resource, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets permissions by action
    /// </summary>
   Task<IEnumerable<Permission>> GetByActionAsync(string action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets permissions for a specific role
    /// </summary>
   Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new permission
    /// </summary>
   Task<Permission> CreateAsync(Permission permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing permission
    /// </summary>
   Task UpdateAsync(Permission permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a permission (soft delete)
    /// </summary>
   Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a permission exists by name
    /// </summary>
   Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
}
