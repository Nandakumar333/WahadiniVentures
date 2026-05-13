using MediatR;

namespace WahadiniCryptoQuest.Service.Discount.Commands;

/// <summary>
/// Command to activate a discount type
/// </summary>
public class ActivateDiscountCommand : IRequest<Unit>
{
    public Guid DiscountCodeId { get; set; }

    public ActivateDiscountCommand(Guid discountCodeId)
    {
        DiscountCodeId = discountCodeId;
    }

    public ActivateDiscountCommand() { }
}
