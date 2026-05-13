using MediatR;

namespace WahadiniCryptoQuest.Service.Discount.Commands;

/// <summary>
/// Command to deactivate a discount type
/// </summary>
public class DeactivateDiscountCommand : IRequest<Unit>
{
    public Guid DiscountCodeId { get; set; }

    public DeactivateDiscountCommand(Guid discountCodeId)
    {
        DiscountCodeId = discountCodeId;
    }

    public DeactivateDiscountCommand() { }
}
