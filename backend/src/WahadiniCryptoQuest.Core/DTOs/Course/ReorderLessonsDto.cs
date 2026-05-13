using System.ComponentModel.DataAnnotations;

namespace WahadiniCryptoQuest.Core.DTOs.Course;

/// <summary>
/// Data transfer object for reordering lessons
/// </summary>
public record ReorderLessonsDto
{
    /// <summary>
    /// Course ID containing the lessons
    /// </summary>
    [Required(ErrorMessage = "Course ID is required")]
    public Guid CourseId { get; init; }

    /// <summary>
    /// Array of lesson IDs with their new order indices
    /// </summary>
    [Required(ErrorMessage = "Lesson orders are required")]
    public LessonOrderDto[] LessonOrders { get; init; } = Array.Empty<LessonOrderDto>();
}

/// <summary>
/// Lesson order specification
/// </summary>
public record LessonOrderDto
{
    /// <summary>
    /// Lesson ID
    /// </summary>
    [Required(ErrorMessage = "Lesson ID is required")]
    public Guid LessonId { get; init; }

    /// <summary>
    /// New order index (1-based)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Order index must be greater than 0")]
    public int OrderIndex { get; init; }
}
