using System.ComponentModel.DataAnnotations;

namespace WahadiniCryptoQuest.Core.DTOs.Reward;

/// <summary>
/// Request to award points by admin
/// </summary>
public class AdminAwardPointsRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Range(1, 100000, ErrorMessage = "Amount must be between 1 and 100,000")]
    public int Amount { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;
}
