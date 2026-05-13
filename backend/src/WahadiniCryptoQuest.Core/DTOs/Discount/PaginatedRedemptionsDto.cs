using WahadiniCryptoQuest.Core.DTOs.Common;

namespace WahadiniCryptoQuest.Core.DTOs.Discount;

/// <summary>
/// Paginated response for user redemption history
/// </summary>
public class PaginatedRedemptionsDto
{
    public List<UserRedemptionDto> Items { get; set; } = new();
    public PaginationMetadata Pagination { get; set; } = null!;
}
