namespace WahadiniCryptoQuest.Core.DTOs.Progress;

/// <summary>
/// Result DTO for progress update operations
/// </summary>
public class UpdateProgressResultDto
{
    public bool Success { get; set; }
    public decimal CompletionPercentage { get; set; }
    public int PointsAwarded { get; set; }
    public bool IsNewlyCompleted { get; set; }
}
