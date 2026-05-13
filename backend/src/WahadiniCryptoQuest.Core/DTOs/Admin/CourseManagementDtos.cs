using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.DTOs.Admin;

/// <summary>
/// Admin course list item DTO
/// T094: US4 - Course Content Management
/// </summary>
public class CourseListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DifficultyLevel Difficulty { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalLessons { get; set; }
    public int EnrollmentCount { get; set; }
}

/// <summary>
/// Form DTO for creating/updating courses
/// T092: US4 - Course Content Management
/// </summary>
public class CourseFormDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public bool IsPremium { get; set; }
    public bool IsPublished { get; set; }
}

/// <summary>
/// Form DTO for creating/updating lessons
/// T093: US4 - Course Content Management
/// </summary>
public class LessonFormDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public int Duration { get; set; } // In minutes
    public int PointReward { get; set; }
    public int Order { get; set; }
}

/// <summary>
/// DTO for lesson reordering
/// </summary>
public class ReorderLessonsDto
{
    public List<Guid> LessonIds { get; set; } = new();
}
