using FluentValidation;
using WahadiniCryptoQuest.Service.Commands.Rewards;

namespace WahadiniCryptoQuest.Service.Validators.Reward;

public class AwardPointsValidator : AbstractValidator<AwardPointsCommand>
{
    public AwardPointsValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Points amount must be greater than 0")
            .LessThanOrEqualTo(100000).WithMessage("Points amount exceeds maximum limit");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.ReferenceId)
            .MaximumLength(100).WithMessage("Reference ID cannot exceed 100 characters");

        RuleFor(x => x.ReferenceType)
            .MaximumLength(50).WithMessage("Reference Type cannot exceed 50 characters");
    }
}
