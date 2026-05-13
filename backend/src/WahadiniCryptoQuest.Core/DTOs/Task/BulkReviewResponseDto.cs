namespace WahadiniCryptoQuest.Core.DTOs.Task;

public class BulkReviewResponseDto
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
