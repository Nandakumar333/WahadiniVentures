using FluentValidation;
using WahadiniCryptoQuest.Service.Commands.Admin;

namespace WahadiniCryptoQuest.Service.Validators.Admin;

/// <summary>
/// Validator for creating discount codes
/// T141: US5 - Reward System Management
/// </summary>
public class CreateDiscountCodeValidator : AbstractValidator<CreateDiscountCodeCommand>
{
    public CreateDiscountCodeValidator()
    {
        RuleFor(x => x.Data.Code)
            .NotEmpty().WithMessage("Code is required")
            .Length(6, 20).WithMessage("Code must be between 6 and 20 characters")
            .Matches("^[A-Za-z0-9]+$").WithMessage("Code must be alphanumeric");

        RuleFor(x => x.Data.DiscountPercentage)
            .InclusiveBetween(0, 100).WithMessage("Discount percentage must be between 0 and 100");

        RuleFor(x => x.Data.RequiredPoints)
            .GreaterThanOrEqualTo(0).WithMessage("Required points must be non-negative");

        RuleFor(x => x.Data.ExpirationDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future")
            .When(x => x.Data.ExpirationDate.HasValue);

        RuleFor(x => x.Data.UsageLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Usage limit must be non-negative");

        RuleFor(x => x.AdminUserId)
            .NotEmpty().WithMessage("Admin user ID is required");
    }
}

/// <summary>
/// Validator for point adjustments
/// T142: US5 - Reward System Management
/// </summary>
public class AdjustPointsValidator : AbstractValidator<AdjustPointsCommand>
{
    public AdjustPointsValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.AdjustmentAmount)
            .NotEqual(0).WithMessage("Adjustment amount cannot be zero");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");

        RuleFor(x => x.AdminUserId)
            .NotEmpty().WithMessage("Admin user ID is required");
    }
}
