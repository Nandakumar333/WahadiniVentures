using FluentValidation;
using WahadiniCryptoQuest.Service.Discount.Commands;

namespace WahadiniCryptoQuest.API.Validators.Discount;

/// <summary>
/// Validator for RedeemDiscountCommand
/// </summary>
public class RedeemDiscountRequestValidator : AbstractValidator<RedeemDiscountCommand>
{
    public RedeemDiscountRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.DiscountCodeId)
            .NotEmpty()
            .WithMessage("Discount code ID is required");
    }
}
