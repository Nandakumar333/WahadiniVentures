using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Role entity for Role-Based Access Control (RBAC)
/// Represents a role that can be assigned to users with associated permissions
/// </summary>
public class Role : BaseEntity
{
    private string _name = string.Empty;
    private string _description = string.Empty;

    /// <summary>
    /// Unique role name (e.g., "Free", "Premium", "Admin")
    /// </summary>
    public string Name 
    { 
        get => _name; 
        private set => _name = value ?? throw new ArgumentNullException(nameof(value)); 
    }

    /// <summary>
    /// Human-readable description of the role
    /// </summary>
    public string Description 
    { 
        get => _description; 
        private set => _description = value ?? string.Empty; 
    }

    /// <summary>
    /// Navigation property: Users who have this role
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    /// <summary>
    /// Navigation property: Permissions associated with this role
    /// </summary>
    public virtual ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    // Private constructor for Entity Framework
    private Role() { }

    /// <summary>
    /// Factory method to create a new Role with validation
    /// </summary>
    public static Role Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required", nameof(name));

        if (name.Length > 50)
            throw new ArgumentException("Role name cannot exceed 50 characters", nameof(name));

        return new Role
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Update role description
    /// </summary>
    public void UpdateDescription(string description)
    {
        Description = description?.Trim() ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Add permission to this role
    /// </summary>
    public void AddPermission(Permission permission)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));

        var existingPermission = RolePermissions.FirstOrDefault(rp => rp.PermissionId == permission.Id);
        if (existingPermission == null)
        {
            var rolePermission = RolePermission.Create(this, permission);
            RolePermissions.Add(rolePermission);
        }
    }

    /// <summary>
    /// Remove permission from this role
    /// </summary>
    public void RemovePermission(Guid permissionId)
    {
        var rolePermission = RolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (rolePermission != null)
        {
            rolePermission.Deactivate();
        }
    }
}
