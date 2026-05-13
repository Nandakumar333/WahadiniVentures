using MediatR;

namespace WahadiniCryptoQuest.Service.Discount.Commands;

/// <summary>
/// Command to soft delete a discount type
/// </summary>
public class DeleteDiscountTypeCommand : IRequest<Unit>
{
    public Guid DiscountCodeId { get; set; }

    public DeleteDiscountTypeCommand(Guid discountCodeId)
    {
        DiscountCodeId = discountCodeId;
    }

    public DeleteDiscountTypeCommand() { }
}
