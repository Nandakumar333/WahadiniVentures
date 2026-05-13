namespace WahadiniCryptoQuest.API.Controllers;

public class BulkReviewRequest
{
    public List<Guid> SubmissionIds { get; set; } = new();
    public bool IsApproved { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public int PointsAwarded { get; set; }
}
