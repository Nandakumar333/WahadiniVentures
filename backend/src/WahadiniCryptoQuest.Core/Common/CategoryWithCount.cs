namespace WahadiniCryptoQuest.Core.Common;

/// <summary>
/// Data transfer object for Category with course count
/// </summary>
public class CategoryWithCount
{
    /// <summary>
    /// Category ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Category name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Icon URL for the category
    /// </summary>
    public string IconUrl { get; set; } = string.Empty;

    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Indicates if the category is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Number of courses in this category
    /// </summary>
    public int CourseCount { get; set; }
}
