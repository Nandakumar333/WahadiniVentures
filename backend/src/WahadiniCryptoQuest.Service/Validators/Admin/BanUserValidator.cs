using FluentValidation;
using WahadiniCryptoQuest.Service.Commands.Admin;

namespace WahadiniCryptoQuest.Service.Validators.Admin;

/// <summary>
/// Validator for ban user command
/// T068: US3 - User Account Management
/// </summary>
public class BanUserValidator : AbstractValidator<BanUserCommand>
{
    public BanUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Ban reason is required")
            .MaximumLength(500).WithMessage("Ban reason cannot exceed 500 characters");

        RuleFor(x => x.AdminUserId)
            .NotEmpty().WithMessage("Admin user ID is required");
    }
}
