using System.ComponentModel.DataAnnotations;
using WahadiniCryptoQuest.Core.Common;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Course category entity (Airdrops, GameFi, DeFi, NFT, Task-to-Earn)
/// </summary>
public class Category : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? IconUrl { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
