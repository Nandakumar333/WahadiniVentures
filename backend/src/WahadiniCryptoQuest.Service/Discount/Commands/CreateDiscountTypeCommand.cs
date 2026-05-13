using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Discount;

namespace WahadiniCryptoQuest.Service.Discount.Commands;

/// <summary>
/// Command to create a new discount type
/// </summary>
public class CreateDiscountTypeCommand : IRequest<AdminDiscountTypeDto>
{
    public string Code { get; set; } = string.Empty;
    public int DiscountPercentage { get; set; }
    public int RequiredPoints { get; set; }
    public int MaxRedemptions { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public CreateDiscountTypeCommand(string code, int discountPercentage, int requiredPoints, int maxRedemptions, DateTime? expiryDate)
    {
        Code = code;
        DiscountPercentage = discountPercentage;
        RequiredPoints = requiredPoints;
        MaxRedemptions = maxRedemptions;
        ExpiryDate = expiryDate;
    }

    public CreateDiscountTypeCommand() { }
}
