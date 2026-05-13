using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Permission entity for fine-grained access control
/// Represents a specific permission that can be granted to roles
/// Format: "resource:action" (e.g., "courses:create", "tasks:review")
/// </summary>
public class Permission : BaseEntity
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private string _resource = string.Empty;
    private string _action = string.Empty;

    /// <summary>
    /// Unique permission name (e.g., "courses:create", "tasks:review")
    /// </summary>
    public string Name 
    { 
        get => _name; 
        private set => _name = value ?? throw new ArgumentNullException(nameof(value)); 
    }

    /// <summary>
    /// Human-readable description of the permission
    /// </summary>
    public string Description 
    { 
        get => _description; 
        private set => _description = value ?? string.Empty; 
    }

    /// <summary>
    /// Resource this permission applies to (e.g., "courses", "tasks", "users")
    /// </summary>
    public string Resource 
    { 
        get => _resource; 
        private set => _resource = value ?? throw new ArgumentNullException(nameof(value)); 
    }

    /// <summary>
    /// Action allowed on the resource (e.g., "create", "read", "update", "delete", "review")
    /// </summary>
    public string Action 
    { 
        get => _action; 
        private set => _action = value ?? throw new ArgumentNullException(nameof(value)); 
    }

    /// <summary>
    /// Navigation property: Roles that have this permission
    /// </summary>
    public virtual ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    // Private constructor for Entity Framework
    private Permission() { }

    /// <summary>
    /// Factory method to create a new Permission with validation
    /// </summary>
    public static Permission Create(string resource, string action, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(resource))
            throw new ArgumentException("Resource is required", nameof(resource));

        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action is required", nameof(action));

        if (resource.Length > 50)
            throw new ArgumentException("Resource cannot exceed 50 characters", nameof(resource));

        if (action.Length > 50)
            throw new ArgumentException("Action cannot exceed 50 characters", nameof(action));

        var name = $"{resource.Trim().ToLowerInvariant()}:{action.Trim().ToLowerInvariant()}";

        return new Permission
        {
            Id = Guid.NewGuid(),
            Name = name,
            Resource = resource.Trim().ToLowerInvariant(),
            Action = action.Trim().ToLowerInvariant(),
            Description = description?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Update permission description
    /// </summary>
    public void UpdateDescription(string description)
    {
        Description = description?.Trim() ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }
}
