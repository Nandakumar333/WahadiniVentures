using System.ComponentModel.DataAnnotations;

namespace WahadiniCryptoQuest.Core.Entities
{

    /// <summary>
    /// Lesson entity with YouTube video integration
    /// </summary>
    public class Lesson : SoftDeletableEntity
    {
        public Guid CourseId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string YouTubeVideoId { get; set; } = string.Empty;

        public int Duration { get; set; } // in minutes (legacy field, use VideoDuration for seconds)

        public int? VideoDuration { get; set; } // in seconds (for video progress tracking)

        public int OrderIndex { get; set; }

        public bool IsPremium { get; set; } = false;

        public int RewardPoints { get; set; } = 0;

        public string? ContentMarkdown { get; set; } // Unlimited text

        // Navigation properties
        public virtual Course Course { get; set; } = null!;
        public virtual ICollection<LearningTask> Tasks { get; set; } = new List<LearningTask>();
        public virtual ICollection<UserProgress> UserProgress { get; set; } = new List<UserProgress>();
    }

}