using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Discount;

namespace WahadiniCryptoQuest.Service.Discount.Commands;

/// <summary>
/// Command to update an existing discount type
/// </summary>
public class UpdateDiscountTypeCommand : IRequest<AdminDiscountTypeDto>
{
    public Guid Id { get; set; }
    public int? DiscountPercentage { get; set; }
    public int? RequiredPoints { get; set; }
    public int? MaxRedemptions { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public UpdateDiscountTypeCommand(Guid id)
    {
        Id = id;
    }

    public UpdateDiscountTypeCommand() { }
}
