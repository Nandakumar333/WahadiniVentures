using Microsoft.AspNetCore.Identity;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// ApplicationUser entity for ASP.NET Identity integration
/// Extends IdentityUser to integrate with our custom User domain entity
/// Acts as a bridge between Identity framework and our rich domain model
/// </summary>
public class ApplicationUser : IdentityUser<Guid>, IAuditableEntity
{
    public ApplicationUser()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedBy = "System";
        UpdatedBy = "System";
        IsDeleted = false;
        Role = UserRoleEnum.Free; // Default to free tier
        SecurityStamp = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Reference to our rich domain User entity
    /// </summary>
    public Guid? DomainUserId { get; set; }
    
    /// <summary>
    /// Navigation property to our domain User entity
    /// </summary>
    public virtual User? DomainUser { get; set; }

    // Extended user properties (synced with domain User)
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRoleEnum Role { get; set; }
    
    // Audit properties
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Creates ApplicationUser linked to domain User entity
    /// </summary>
    /// <param name="domainUser">Domain User entity</param>
    /// <returns>ApplicationUser for Identity</returns>
    public static ApplicationUser CreateFromDomainUser(User domainUser)
    {
        if (domainUser == null)
            throw new ArgumentNullException(nameof(domainUser));

        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            DomainUserId = domainUser.Id,
            UserName = domainUser.Email,
            Email = domainUser.Email,
            NormalizedUserName = domainUser.Email.ToUpperInvariant(),
            NormalizedEmail = domainUser.Email.ToUpperInvariant(),
            FirstName = domainUser.FirstName,
            LastName = domainUser.LastName,
            Role = domainUser.Role,
            EmailConfirmed = domainUser.EmailConfirmed,
            LockoutEnabled = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            DomainUser = domainUser,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        };
    }

    /// <summary>
    /// Syncs ApplicationUser state with domain User entity
    /// Call this whenever the domain User changes
    /// </summary>
    /// <param name="domainUser">Updated domain User entity</param>
    public void SyncWithDomainUser(User domainUser)
    {
        if (domainUser == null)
            throw new ArgumentNullException(nameof(domainUser));

        if (DomainUserId.HasValue && domainUser.Id != DomainUserId.Value)
            throw new InvalidOperationException("Cannot sync with different user entity");

        DomainUserId = domainUser.Id;
        UserName = domainUser.Email;
        Email = domainUser.Email;
        NormalizedUserName = domainUser.Email.ToUpperInvariant();
        NormalizedEmail = domainUser.Email.ToUpperInvariant();
        FirstName = domainUser.FirstName;
        LastName = domainUser.LastName;
        Role = domainUser.Role;
        EmailConfirmed = domainUser.EmailConfirmed;
        
        if (domainUser.IsLockedOut())
        {
            LockoutEnd = DateTimeOffset.MaxValue; // Permanent lockout
        }
        else
        {
            LockoutEnd = null;
        }

        UpdateAuditFields();
    }

    /// <summary>
    /// Marks the user as deleted (soft delete)
    /// </summary>
    public virtual void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the audit fields when user is modified
    /// </summary>
    /// <param name="updatedBy">Who is updating the user</param>
    public virtual void UpdateAuditFields(string updatedBy = "System")
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Updates the user's role with audit tracking
    /// </summary>
    /// <param name="newRole">New role to assign</param>
    /// <param name="updatedBy">Who is making the change</param>
    public virtual void UpdateRole(UserRoleEnum newRole, string updatedBy = "System")
    {
        Role = newRole;
        UpdateAuditFields(updatedBy);
    }
}