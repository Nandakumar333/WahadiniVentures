using System;

namespace WahadiniCryptoQuest.Core.DTOs.Progress;

/// <summary>
/// Progress data transfer object for API responses
/// </summary>
public class ProgressDto
{
    public Guid LessonId { get; set; }
    public int LastWatchedPosition { get; set; }
    public decimal CompletionPercentage { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalWatchTime { get; set; }
}
