using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.API.Controllers;

public class ReviewSubmissionRequest
{
    public bool IsApproved { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public int PointsAwarded { get; set; }
    public byte[] Version { get; set; } = Array.Empty<byte>();
}
