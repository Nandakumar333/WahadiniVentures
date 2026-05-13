namespace WahadiniCryptoQuest.Core.DTOs.Admin;

/// <summary>
/// DTO for discount code listing
/// T129: US5 - Reward System Management
/// </summary>
public class DiscountCodeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int DiscountPercentage { get; set; }
    public int RequiredPoints { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public string Status { get; set; } = string.Empty; // Active, Expired, FullyRedeemed
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating discount codes
/// T130: US5 - Reward System Management
/// </summary>
public class CreateDiscountCodeDto
{
    public string Code { get; set; } = string.Empty;
    public int DiscountPercentage { get; set; }
    public int RequiredPoints { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int UsageLimit { get; set; } = 0; // 0 = unlimited
}

/// <summary>
/// DTO for redemption log entries
/// T131: US5 - Reward System Management
/// </summary>
public class RedemptionLogDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime RedeemedAt { get; set; }
    public decimal DiscountAmount { get; set; }
}

/// <summary>
/// DTO for manual point adjustments
/// T132: US5 - Reward System Management
/// </summary>
public class AdjustPointsRequestDto
{
    public int AdjustmentAmount { get; set; } // Can be positive or negative
    public string Reason { get; set; } = string.Empty;
}
