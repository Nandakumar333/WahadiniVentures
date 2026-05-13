using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// LearningTask entity with flexible JSONB data for different task types
/// Renamed from Task to avoid ambiguity with System.Threading.Tasks.Task
/// </summary>
public class LearningTask : BaseEntity
{
    public Guid LessonId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public TaskType TaskType { get; set; }

    /// <summary>
    /// JSONB column for flexible task data structure
    /// Examples: Quiz questions, Screenshot requirements, Wallet verification data
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string TaskData { get; set; } = "{}";

    public int RewardPoints { get; set; } = 0;

    public int? TimeLimit { get; set; } // in minutes, nullable

    public int OrderIndex { get; set; }

    public bool IsRequired { get; set; } = true;

    // Navigation properties
    public virtual Lesson Lesson { get; set; } = null!;
    public virtual ICollection<UserTaskSubmission> Submissions { get; set; } = new List<UserTaskSubmission>();
}
