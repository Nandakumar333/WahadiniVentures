namespace WahadiniCryptoQuest.Core.Common;

/// <summary>
/// Base interface for all domain entities
/// Provides common properties for entity identification and tracking
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    Guid Id { get; }
}

/// <summary>
/// Interface for entities that support audit tracking
/// Provides creation and modification timestamps with user tracking
/// </summary>
public interface IAuditableEntity : IEntity
{
    /// <summary>
    /// When the entity was created
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// When the entity was last updated
    /// </summary>
    DateTime UpdatedAt { get; }

    /// <summary>
    /// Who created the entity
    /// </summary>
    string CreatedBy { get; }

    /// <summary>
    /// Who last updated the entity
    /// </summary>
    string UpdatedBy { get; }

    /// <summary>
    /// Whether the entity is soft deleted
    /// </summary>
    bool IsDeleted { get; }
}