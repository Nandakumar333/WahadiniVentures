namespace WahadiniCryptoQuest.Core.DTOs.Discount;

/// <summary>
/// DTO for discount analytics - can represent single discount or summary
/// </summary>
public class DiscountAnalyticsDto
{
    // Single discount analytics (when DiscountCodeId is provided)
    public Guid? DiscountCodeId { get; set; }
    public string? Code { get; set; }
    public int? DiscountPercentage { get; set; }
    public int? RequiredPoints { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? ExpiryDate { get; set; }

    // Common analytics
    public int TotalRedemptions { get; set; }
    public int TotalPointsBurned { get; set; }
    public int UniqueUsers { get; set; }
    public DateTime? FirstRedemptionDate { get; set; }
    public DateTime? LastRedemptionDate { get; set; }
    public decimal AverageRedemptionsPerDay { get; set; }

    // Summary analytics (when DiscountCodeId is null)
    public int ActiveDiscountCodes { get; set; }
    public int TotalDiscountCodes { get; set; }
    public List<DiscountPerformanceDto> TopPerformingDiscounts { get; set; } = new();
}

public class DiscountPerformanceDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int RedemptionCount { get; set; }
    public int PointsBurned { get; set; }
}
