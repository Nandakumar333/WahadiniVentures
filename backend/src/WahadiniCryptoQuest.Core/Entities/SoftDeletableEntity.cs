using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Base entity for entities requiring soft delete capability
/// </summary>
public abstract class SoftDeletableEntity : BaseEntity
{
    /// <summary>
    /// Indicates whether the entity is active (false = soft deleted)
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Activates the entity (restores from soft delete)
    /// </summary>
    public void Activate() => IsActive = true;

    /// <summary>
    /// Deactivates the entity (soft delete)
    /// </summary>
    public void Deactivate() => IsActive = false;
}
