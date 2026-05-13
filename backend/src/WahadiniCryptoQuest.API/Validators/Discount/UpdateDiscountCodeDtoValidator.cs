using FluentValidation;
using WahadiniCryptoQuest.Core.DTOs.Discount;

namespace WahadiniCryptoQuest.API.Validators.Discount;

/// <summary>
/// Validator for UpdateDiscountCodeDto with business rule validations
/// </summary>
public class UpdateDiscountCodeDtoValidator : AbstractValidator<UpdateDiscountCodeDto>
{
    public UpdateDiscountCodeDtoValidator()
    {
        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(1, 100)
            .WithMessage("Discount percentage must be between 1 and 100")
            .When(x => x.DiscountPercentage.HasValue);

        RuleFor(x => x.RequiredPoints)
            .GreaterThan(0)
            .WithMessage("Required points must be greater than 0")
            .LessThanOrEqualTo(1000000)
            .WithMessage("Required points must not exceed 1,000,000")
            .When(x => x.RequiredPoints.HasValue);

        RuleFor(x => x.MaxRedemptions)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Max redemptions must be greater than or equal to 0 (0 means unlimited)")
            .When(x => x.MaxRedemptions.HasValue);

        RuleFor(x => x.ExpiryDate)
            .Must(date => !date.HasValue || date.Value > DateTime.UtcNow)
            .WithMessage("Expiry date must be in the future")
            .When(x => x.ExpiryDate.HasValue);

        // At least one field must be provided for update
        RuleFor(x => x)
            .Must(x => x.DiscountPercentage.HasValue ||
                      x.RequiredPoints.HasValue ||
                      x.MaxRedemptions.HasValue ||
                      x.ExpiryDate.HasValue)
            .WithMessage("At least one field must be provided for update");
    }
}
