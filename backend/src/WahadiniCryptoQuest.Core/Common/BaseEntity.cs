namespace WahadiniCryptoQuest.Core.Common;

/// <summary>
/// Base class for all domain entities following Clean Architecture
/// Implements common entity patterns with audit support
/// </summary>
public abstract class BaseEntity : IAuditableEntity
{
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedBy = "System";
        UpdatedBy = "System";
        IsDeleted = false;
    }

    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; init; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Marks the entity as deleted (soft delete)
    /// </summary>
    public virtual void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the audit fields when entity is modified
    /// </summary>
    /// <param name="updatedBy">Who is updating the entity</param>
    public virtual void UpdateAuditFields(string updatedBy = "System")
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}