using System.ComponentModel.DataAnnotations;

namespace WahadiniCryptoQuest.Core.DTOs.Reward;

/// <summary>
/// Request to deduct points by admin with justification
/// </summary>
public class AdminDeductPointsRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Range(1, 100000, ErrorMessage = "Amount must be between 1 and 100,000")]
    public int Amount { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Justification must be at least 10 characters")]
    public string Justification { get; set; } = string.Empty;
}
