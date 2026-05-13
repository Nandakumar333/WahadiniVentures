namespace WahadiniCryptoQuest.Core.DTOs.Discount;

/// <summary>
/// Summary analytics across all discount codes
/// </summary>
public class AnalyticsSummaryDto
{
    public int TotalDiscountCodes { get; set; }
    public int ActiveDiscountCodes { get; set; }
    public int TotalRedemptions { get; set; }
    public int TotalPointsBurned { get; set; }
    public int UniqueRedeemingUsers { get; set; }
    public List<DiscountAnalyticsDto> TopPerformingDiscounts { get; set; } = new();
    public DateTime? EarliestRedemptionDate { get; set; }
    public DateTime? LatestRedemptionDate { get; set; }
}
