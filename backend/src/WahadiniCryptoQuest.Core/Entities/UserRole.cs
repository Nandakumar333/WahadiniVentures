using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Junction entity for many-to-many relationship between Users and Roles
/// Represents which roles are assigned to which users with expiration support
/// </summary>
public class UserRole : BaseEntity
{
    /// <summary>
    /// Foreign key to User
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Foreign key to Role
    /// </summary>
    public Guid RoleId { get; private set; }

    /// <summary>
    /// When this role was assigned to the user
    /// </summary>
    public DateTime AssignedAt { get; private set; }

    /// <summary>
    /// When this role assignment expires (null for permanent)
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }

    /// <summary>
    /// Whether this user-role assignment is active
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Navigation property to User
    /// </summary>
    public virtual User User { get; private set; } = null!;

    /// <summary>
    /// Navigation property to Role
    /// </summary>
    public virtual Role Role { get; private set; } = null!;

    // Private constructor for Entity Framework
    private UserRole() { }

    /// <summary>
    /// Factory method to create a new UserRole assignment
    /// </summary>
    public static UserRole Create(User user, Role role, DateTime? expiresAt = null)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
            throw new ArgumentException("Expiration date must be in the future", nameof(expiresAt));

        return new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RoleId = role.Id,
            User = user,
            Role = role,
            AssignedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Check if this role assignment is expired
    /// </summary>
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;
    }

    /// <summary>
    /// Expire this role assignment immediately
    /// </summary>
    public void Expire()
    {
        ExpiresAt = DateTime.UtcNow;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivate this role assignment
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activate this role assignment (if not expired)
    /// </summary>
    public void Activate()
    {
        if (IsExpired())
            throw new InvalidOperationException("Cannot activate an expired role assignment");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Extend the expiration date
    /// </summary>
    public void ExtendExpiration(DateTime newExpiresAt)
    {
        if (newExpiresAt <= DateTime.UtcNow)
            throw new ArgumentException("New expiration date must be in the future", nameof(newExpiresAt));

        ExpiresAt = newExpiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Make this role assignment permanent (no expiration)
    /// </summary>
    public void MakePermanent()
    {
        ExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
