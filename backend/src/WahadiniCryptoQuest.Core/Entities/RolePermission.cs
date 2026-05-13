using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Junction entity for many-to-many relationship between Roles and Permissions
/// Represents which permissions are assigned to which roles
/// </summary>
public class RolePermission : BaseEntity
{
    /// <summary>
    /// Foreign key to Role
    /// </summary>
    public Guid RoleId { get; private set; }

    /// <summary>
    /// Foreign key to Permission
    /// </summary>
    public Guid PermissionId { get; private set; }

    /// <summary>
    /// Whether this role-permission assignment is active
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Navigation property to Role
    /// </summary>
    public virtual Role Role { get; private set; } = null!;

    /// <summary>
    /// Navigation property to Permission
    /// </summary>
    public virtual Permission Permission { get; private set; } = null!;

    // Private constructor for Entity Framework
    private RolePermission() { }

    /// <summary>
    /// Factory method to create a new RolePermission assignment
    /// </summary>
    public static RolePermission Create(Role role, Permission permission)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (permission == null)
            throw new ArgumentNullException(nameof(permission));

        return new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = role.Id,
            PermissionId = permission.Id,
            Role = role,
            Permission = permission,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method to create a new RolePermission assignment using IDs only
    /// </summary>
    public static RolePermission CreateFromIds(Guid roleId, Guid permissionId)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("Role ID cannot be empty", nameof(roleId));

        if (permissionId == Guid.Empty)
            throw new ArgumentException("Permission ID cannot be empty", nameof(permissionId));

        return new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionId = permissionId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Deactivate this role-permission assignment
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivate this role-permission assignment
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
