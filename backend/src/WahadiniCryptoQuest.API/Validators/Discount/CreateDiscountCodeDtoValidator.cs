using FluentValidation;
using WahadiniCryptoQuest.Core.DTOs.Discount;

namespace WahadiniCryptoQuest.API.Validators.Discount;

/// <summary>
/// Validator for CreateDiscountCodeDto with business rule validations
/// </summary>
public class CreateDiscountCodeDtoValidator : AbstractValidator<CreateDiscountCodeDto>
{
    public CreateDiscountCodeDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Discount code is required")
            .MinimumLength(3)
            .WithMessage("Discount code must be at least 3 characters")
            .MaximumLength(50)
            .WithMessage("Discount code must not exceed 50 characters")
            .Matches("^[A-Z0-9_-]+$")
            .WithMessage("Discount code must only contain uppercase letters, numbers, hyphens, and underscores");

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(1, 100)
            .WithMessage("Discount percentage must be between 1 and 100");

        RuleFor(x => x.RequiredPoints)
            .GreaterThan(0)
            .WithMessage("Required points must be greater than 0")
            .LessThanOrEqualTo(1000000)
            .WithMessage("Required points must not exceed 1,000,000");

        RuleFor(x => x.MaxRedemptions)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Max redemptions must be greater than or equal to 0 (0 means unlimited)");

        RuleFor(x => x.ExpiryDate)
            .Must(date => !date.HasValue || date.Value > DateTime.UtcNow)
            .WithMessage("Expiry date must be in the future")
            .When(x => x.ExpiryDate.HasValue);
    }
}
